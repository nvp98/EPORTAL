using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Runtime.Caching;
using System.Text;

namespace EPORTAL.Common
{
    /// <summary>Ket qua tra ve khi hit knowledge cache.</summary>
    public class KnowledgeAnswer
    {
        public long     Id           { get; set; }
        public string   AnswerText   { get; set; }
        public string   ActionType   { get; set; }
        public string   ActionTarget { get; set; }
        public string[] Suggestions  { get; set; }
    }

    /// <summary>
    /// Kho Q&A tu hoc (V360_ChatbotKnowledge). Lookup theo embedding (semantic) + QuestionNorm (exact fallback).
    /// Upsert do LLM Curator goi sau moi luot MISS. (Phase 1-3 - knowledge-cache plan)
    /// Moi loi deu nuot (try/catch) -> KHONG bao gio lam vo luong chat.
    /// </summary>
    public static class KnowledgeStore
    {
        private static SqlConnection OpenConnection() => SqlConnectionHelper.Open();

        // ===== Chuan hoa cau hoi: bo dau tieng Viet + lowercase + gom khoang trang =====
        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var norm = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark) sb.Append(ch);
            var r = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Replace('đ', 'd');
            r = System.Text.RegularExpressions.Regex.Replace(r, @"\s+", " ").Trim();
            if (r.Length > 500) r = r.Substring(0, 500);
            return r;
        }

        // ===== Embedding pack/unpack + cosine =====
        public static byte[] PackEmbedding(float[] v)
        {
            if (v == null || v.Length == 0) return null;
            var b = new byte[v.Length * 4];
            Buffer.BlockCopy(v, 0, b, 0, b.Length);
            return b;
        }
        private static float[] UnpackEmbedding(byte[] b)
        {
            if (b == null || b.Length < 4) return null;
            var v = new float[b.Length / 4];
            Buffer.BlockCopy(b, 0, v, 0, v.Length * 4);
            return v;
        }
        private static double Cosine(float[] a, float[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return -1;
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++) { dot += a[i] * b[i]; na += a[i] * a[i]; nb += b[i] * b[i]; }
            if (na <= 0 || nb <= 0) return -1;
            return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
        }

        // ===== Candidate index cache (per collection, 60s) =====
        private sealed class Cand
        {
            public long Id; public float[] Emb; public string Norm; public string Scene; public byte Score;
        }
        private static string CandKey(string collectionId) => "kb_cand_" + collectionId;

        private static List<Cand> GetCandidates(string collectionId)
        {
            var key = CandKey(collectionId);
            if (MemoryCache.Default.Get(key) is List<Cand> cached) return cached;
            var list = new List<Cand>();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT Id, Embedding, QuestionNorm, SceneScope, QualityScore
                      FROM dbo.V360_ChatbotKnowledge
                      WHERE CollectionId = @c AND Status = 'active'", conn))
                {
                    cmd.Parameters.AddWithValue("@c", collectionId);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            list.Add(new Cand {
                                Id = rd.GetInt64(0),
                                Emb = rd.IsDBNull(1) ? null : UnpackEmbedding((byte[])rd[1]),
                                Norm = rd.IsDBNull(2) ? "" : rd.GetString(2),
                                Scene = rd.IsDBNull(3) ? null : rd.GetString(3),
                                Score = rd.IsDBNull(4) ? (byte)0 : (byte)rd.GetByte(4)
                            });
                }
                MemoryCache.Default.Set(key, list, DateTimeOffset.UtcNow.AddSeconds(60));
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.GetCandidates] " + ex.Message); }
            return list;
        }
        private static void InvalidateCandidates(string collectionId)
        {
            if (!string.IsNullOrEmpty(collectionId)) MemoryCache.Default.Remove(CandKey(collectionId));
        }

        /// <summary>
        /// Tim cau tra loi cache cho cau hoi. queryEmbedding co the null (chi dung exact-norm).
        /// Tra null neu khong co hit du tot.
        /// </summary>
        public static KnowledgeAnswer Lookup(string collectionId, string sceneUuid,
            float[] queryEmbedding, string message, double simThreshold, int minScore)
        {
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(message)) return null;
            try
            {
                var norm = Normalize(message);
                var cands = GetCandidates(collectionId);
                if (cands.Count == 0) return null;

                Cand best = null; double bestScore = -1;
                foreach (var c in cands)
                {
                    if (c.Score < minScore) continue;
                    if (c.Scene != null && c.Scene != sceneUuid) continue;   // scene-specific cua scene khac -> bo

                    double sim;
                    if (c.Norm == norm) sim = 1.0;                            // exact normalized -> coi nhu 1.0
                    else if (queryEmbedding != null && c.Emb != null)
                    {
                        sim = Cosine(queryEmbedding, c.Emb);
                        if (sim < simThreshold) continue;
                    }
                    else continue;

                    // Uu tien: similarity cao hon; cung muc thi scene-specific > tour-level
                    var rank = sim + (c.Scene != null ? 0.0001 : 0);
                    if (rank > bestScore) { bestScore = rank; best = c; }
                }
                if (best == null) return null;

                return FetchAndHit(best.Id);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[KnowledgeStore.Lookup] " + ex.Message);
                return null;
            }
        }

        /// <summary>Lay AnswerText theo Id (KHONG tang hit) - dung cho admin (nghe/xoa audio).</summary>
        public static string GetAnswerById(long id)
        {
            if (id <= 0) return null;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand("SELECT AnswerText FROM dbo.V360_ChatbotKnowledge WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var v = cmd.ExecuteScalar();
                    return (v == null || v == DBNull.Value) ? null : (string)v;
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.GetAnswerById] " + ex.Message); }
            return null;
        }

        private static KnowledgeAnswer FetchAndHit(long id)
        {
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.V360_ChatbotKnowledge
                         SET HitCount = HitCount + 1, LastUsedAt = GETDATE()
                       OUTPUT inserted.AnswerText, inserted.ActionType, inserted.ActionTarget, inserted.Suggestions
                       WHERE Id = @id AND Status = 'active'", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        var sugg = rd.IsDBNull(3) ? null : rd.GetString(3);
                        return new KnowledgeAnswer {
                            Id = id,
                            AnswerText = rd.IsDBNull(0) ? "" : rd.GetString(0),
                            ActionType = rd.IsDBNull(1) ? null : rd.GetString(1),
                            ActionTarget = rd.IsDBNull(2) ? null : rd.GetString(2),
                            Suggestions = string.IsNullOrEmpty(sugg) ? null
                                : sugg.Split('|').Select(x => x.Trim()).Where(x => x.Length > 0).Take(3).ToArray()
                        };
                    }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.FetchAndHit] " + ex.Message); }
            return null;
        }

        /// <summary>Tim entry GAN TRUNG (cosine >= mergeThreshold) cung scope -> de gop thay vi tao moi.</summary>
        private static long? FindSimilarId(string collectionId, string sceneScope, float[] embedding, double mergeThreshold)
        {
            if (embedding == null) return null;
            try
            {
                var cands = GetCandidates(collectionId);
                long? best = null; double bestSim = mergeThreshold;
                foreach (var c in cands)
                {
                    if (!((c.Scene == null && sceneScope == null) || c.Scene == sceneScope)) continue;  // cung scope
                    if (c.Emb == null) continue;
                    var sim = Cosine(embedding, c.Emb);
                    if (sim > bestSim) { bestSim = sim; best = c.Id; }
                }
                return best;
            }
            catch { return null; }
        }

        /// <summary>Them/cap nhat 1 entry (do Curator quyet dinh). Gop neu trung norm HOAC gan trung embedding (cung scope).</summary>
        public static void Upsert(string collectionId, string sceneScope, string intent,
            string canonicalQuestion, string answerText, string actionType, string actionTarget,
            string suggestionsPipe, string category, string tags, int qualityScore, double confidence,
            string modelUsed, float[] embedding, string source = "curator")
        {
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(canonicalQuestion) || string.IsNullOrEmpty(answerText)) return;
            try
            {
                var norm = Normalize(canonicalQuestion);
                var emb = PackEmbedding(embedding);
                // Gop gan trung qua embedding (vd canonicalQuestion khac chu nhung cung y) -> tranh tao dong trung.
                var forceId = FindSimilarId(collectionId, sceneScope, embedding, 0.95);
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"DECLARE @existId BIGINT = @forceId, @existScore TINYINT = 0;
                      IF @existId IS NULL
                        SELECT TOP 1 @existId = Id, @existScore = QualityScore
                        FROM dbo.V360_ChatbotKnowledge
                        WHERE CollectionId=@c AND QuestionNorm=@norm
                          AND ((SceneScope IS NULL AND @scene IS NULL) OR SceneScope=@scene);
                      ELSE
                        SELECT @existScore = QualityScore FROM dbo.V360_ChatbotKnowledge WHERE Id=@existId;
                      IF @existId IS NULL
                      BEGIN
                        INSERT INTO dbo.V360_ChatbotKnowledge
                          (CollectionId, SceneScope, Intent, CanonicalQuestion, QuestionNorm, Embedding,
                           AnswerText, ActionType, ActionTarget, Suggestions, Category, Tags,
                           QualityScore, Confidence, Source, Status, ModelUsed, CreatedAt, UpdatedAt)
                        VALUES
                          (@c, @scene, @intent, @cq, @norm, @emb,
                           @ans, @at, @atg, @sugg, @cat, @tags,
                           @score, @conf, @src, 'active', @model, GETDATE(), GETDATE());
                      END
                      ELSE
                      BEGIN
                        UPDATE dbo.V360_ChatbotKnowledge
                           SET Confirmations = Confirmations + 1,
                               UpdatedAt = GETDATE(),
                               Embedding = COALESCE(@emb, Embedding),
                               -- chi ghi de noi dung khi ban moi diem cao hon hoac bang
                               AnswerText   = CASE WHEN @score >= @existScore THEN @ans ELSE AnswerText END,
                               ActionType   = CASE WHEN @score >= @existScore THEN @at  ELSE ActionType END,
                               ActionTarget = CASE WHEN @score >= @existScore THEN @atg ELSE ActionTarget END,
                               Suggestions  = CASE WHEN @score >= @existScore THEN @sugg ELSE Suggestions END,
                               QualityScore = CASE WHEN @score >= @existScore THEN @score ELSE QualityScore END,
                               Confidence   = CASE WHEN @score >= @existScore THEN @conf ELSE Confidence END,
                               Category     = CASE WHEN @score >= @existScore THEN @cat ELSE Category END,
                               Tags         = CASE WHEN @score >= @existScore THEN @tags ELSE Tags END,
                               Status       = 'active'
                         WHERE Id = @existId;
                      END", conn))
                {
                    cmd.Parameters.AddWithValue("@forceId", forceId.HasValue ? (object)forceId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@c", collectionId);
                    cmd.Parameters.AddWithValue("@scene", (object)sceneScope ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@intent", (object)intent ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cq", canonicalQuestion.Length > 500 ? canonicalQuestion.Substring(0, 500) : canonicalQuestion);
                    cmd.Parameters.AddWithValue("@norm", norm);
                    cmd.Parameters.AddWithValue("@emb", (object)emb ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ans", answerText);
                    cmd.Parameters.AddWithValue("@at", (object)actionType ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@atg", (object)actionTarget ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@sugg", (object)suggestionsPipe ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@cat", (object)category ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tags", (object)tags ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@score", (byte)Math.Max(0, Math.Min(100, qualityScore)));
                    cmd.Parameters.AddWithValue("@conf", confidence);
                    cmd.Parameters.AddWithValue("@src", string.IsNullOrEmpty(source) ? "curator" : source);
                    cmd.Parameters.AddWithValue("@model", (object)modelUsed ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                InvalidateCandidates(collectionId);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.Upsert] " + ex.Message); }
        }

        // ============================================================
        //   ADMIN (trang quan ly KB)
        // ============================================================
        public class KnowledgeRow
        {
            public long     Id { get; set; }
            public string   CollectionId { get; set; }
            public string   SceneScope { get; set; }
            public string   Intent { get; set; }
            public string   CanonicalQuestion { get; set; }
            public string   AnswerText { get; set; }
            public string   Suggestions { get; set; }
            public string   ActionType { get; set; }
            public string   ActionTarget { get; set; }
            public string   Category { get; set; }
            public string   Tags { get; set; }
            public int      QualityScore { get; set; }
            public string   Source { get; set; }
            public string   Status { get; set; }
            public int      HitCount { get; set; }
            public int      Confirmations { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public DateTime? LastUsedAt { get; set; }
        }

        public static List<KnowledgeRow> ListForAdmin(string collectionId, string status, string search, int top = 500)
        {
            var list = new List<KnowledgeRow>();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT TOP (" + Math.Max(1, Math.Min(2000, top)) + @") Id, CollectionId, SceneScope, Intent,
                             CanonicalQuestion, AnswerText, ActionType, ActionTarget, Category, Tags,
                             QualityScore, Source, Status, HitCount, Confirmations, CreatedAt, UpdatedAt, LastUsedAt, Suggestions
                      FROM dbo.V360_ChatbotKnowledge
                      WHERE (@c IS NULL OR @c='' OR CollectionId=@c)
                        AND (@st IS NULL OR @st='' OR Status=@st)
                        AND (@q IS NULL OR @q='' OR CanonicalQuestion LIKE '%'+@q+'%' OR AnswerText LIKE '%'+@q+'%')
                      ORDER BY UpdatedAt DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@c", (object)collectionId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@st", (object)status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@q", (object)search ?? DBNull.Value);
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            list.Add(new KnowledgeRow {
                                Id = rd.GetInt64(0),
                                CollectionId = rd.IsDBNull(1) ? "" : rd.GetString(1),
                                SceneScope = rd.IsDBNull(2) ? null : rd.GetString(2),
                                Intent = rd.IsDBNull(3) ? null : rd.GetString(3),
                                CanonicalQuestion = rd.IsDBNull(4) ? "" : rd.GetString(4),
                                AnswerText = rd.IsDBNull(5) ? "" : rd.GetString(5),
                                ActionType = rd.IsDBNull(6) ? null : rd.GetString(6),
                                ActionTarget = rd.IsDBNull(7) ? null : rd.GetString(7),
                                Category = rd.IsDBNull(8) ? null : rd.GetString(8),
                                Tags = rd.IsDBNull(9) ? null : rd.GetString(9),
                                QualityScore = rd.IsDBNull(10) ? 0 : rd.GetByte(10),
                                Source = rd.IsDBNull(11) ? "" : rd.GetString(11),
                                Status = rd.IsDBNull(12) ? "" : rd.GetString(12),
                                HitCount = rd.IsDBNull(13) ? 0 : rd.GetInt32(13),
                                Confirmations = rd.IsDBNull(14) ? 0 : rd.GetInt32(14),
                                CreatedAt = rd.GetDateTime(15),
                                UpdatedAt = rd.GetDateTime(16),
                                LastUsedAt = rd.IsDBNull(17) ? (DateTime?)null : rd.GetDateTime(17),
                                Suggestions = rd.IsDBNull(18) ? null : rd.GetString(18)
                            });
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.ListForAdmin] " + ex.Message); }
            return list;
        }

        public static void SetStatus(long id, string collectionId, string status)
        {
            if (id <= 0 || string.IsNullOrEmpty(status)) return;
            ExecById("UPDATE dbo.V360_ChatbotKnowledge SET Status=@v, UpdatedAt=GETDATE() WHERE Id=@id", id, "@v", status, collectionId);
        }

        public static void Delete(long id, string collectionId)
        {
            if (id <= 0) return;
            ExecById("DELETE FROM dbo.V360_ChatbotKnowledge WHERE Id=@id", id, null, null, collectionId);
        }

        public static void UpdateAnswer(long id, string collectionId, string canonicalQuestion, string answerText, int qualityScore, string suggestionsPipe)
        {
            if (id <= 0) return;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.V360_ChatbotKnowledge
                         SET CanonicalQuestion = @cq, QuestionNorm = @norm, AnswerText = @ans,
                             QualityScore = @score, Suggestions = @sugg, Source = 'admin', Status='active', UpdatedAt = GETDATE()
                       WHERE Id = @id", conn))
                {
                    var cq = canonicalQuestion ?? "";
                    if (cq.Length > 500) cq = cq.Substring(0, 500);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@cq", cq);
                    cmd.Parameters.AddWithValue("@norm", Normalize(cq));
                    cmd.Parameters.AddWithValue("@ans", (object)answerText ?? "");
                    cmd.Parameters.AddWithValue("@score", (byte)Math.Max(0, Math.Min(100, qualityScore)));
                    cmd.Parameters.AddWithValue("@sugg", (object)suggestionsPipe ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                InvalidateCandidates(collectionId);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.UpdateAnswer] " + ex.Message); }
        }

        private static void ExecById(string sql, long id, string extraParam, object extraVal, string collectionId)
        {
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    if (extraParam != null) cmd.Parameters.AddWithValue(extraParam, extraVal ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                InvalidateCandidates(collectionId);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.ExecById] " + ex.Message); }
        }

        /// <summary>Khi admin sua noi dung tour/scene -> danh dau stale (sceneScope null = ca tour-level + moi scene).</summary>
        public static void MarkStale(string collectionId, string sceneScope = null)
        {
            if (string.IsNullOrEmpty(collectionId)) return;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.V360_ChatbotKnowledge
                         SET Status='stale', UpdatedAt=GETDATE()
                       WHERE CollectionId=@c AND Status='active'
                         AND (@scene IS NULL OR SceneScope=@scene OR SceneScope IS NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@c", collectionId);
                    cmd.Parameters.AddWithValue("@scene", (object)sceneScope ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                InvalidateCandidates(collectionId);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KnowledgeStore.MarkStale] " + ex.Message); }
        }
    }
}
