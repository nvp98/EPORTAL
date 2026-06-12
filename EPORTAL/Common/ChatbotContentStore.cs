using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EPORTAL.Common
{
    public class ChatbotTourInfo
    {
        public string Overview     { get; set; }
        public string SystemPrompt { get; set; }
        public bool   IsEnabled    { get; set; }
    }

    public class ChatbotSceneInfo
    {
        public string ShortIntro    { get; set; }
        public string DetailContent { get; set; }
    }

    public class ChatbotMessageLog
    {
        public Guid    SessionGuid  { get; set; }
        public int?    NhanVienID   { get; set; }
        public string  CollectionId { get; set; }
        public string  SceneUuid    { get; set; }
        public byte    Role         { get; set; } // 0=user, 1=assistant, 2=system
        public string  Content      { get; set; }
        public string  Action       { get; set; }
        public int?    TokensIn     { get; set; }
        public int?    TokensOut    { get; set; }
        public int?    LatencyMs    { get; set; }
    }

    /// <summary>
    /// Persist tour-level + scene-level content cho V360 Chatbot AI.
    /// Schema duoc tao boi migration v360-all.sql SECTION 1 (deploy-time).
    ///
    /// VI SAO RAW ADO.NET: bang V360_* moi, chua map vao EDMX -> khong query qua EF duoc.
    /// Pattern store chuan cua View360
    /// </summary>
    public static class ChatbotContentStore
    {
        private static SqlConnection OpenConnection() => SqlConnectionHelper.Open();

        // ===== Tour info =====
        public static ChatbotTourInfo GetTourInfo(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId)) return null;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    "SELECT Overview, SystemPrompt, IsEnabled FROM dbo.V360_ChatbotTourInfo WHERE CollectionId=@cid",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return new ChatbotTourInfo
                            {
                                Overview     = rd.IsDBNull(0) ? null : rd.GetString(0),
                                SystemPrompt = rd.IsDBNull(1) ? null : rd.GetString(1),
                                IsEnabled    = !rd.IsDBNull(2) && rd.GetBoolean(2)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.GetTourInfo] " + ex.Message);
            }
            return null;
        }

        public static bool SaveTourInfo(string collectionId, ChatbotTourInfo info, int? userId = null)
        {
            if (string.IsNullOrEmpty(collectionId) || info == null) return false;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(@"
MERGE dbo.V360_ChatbotTourInfo AS T
USING (SELECT @cid AS CollectionId) AS S ON T.CollectionId = S.CollectionId
WHEN MATCHED THEN UPDATE SET
    Overview=@ov, SystemPrompt=@sp, IsEnabled=@en, UpdatedAt=GETDATE(), UpdatedBy=@uid
WHEN NOT MATCHED THEN
    INSERT (CollectionId, Overview, SystemPrompt, IsEnabled, UpdatedAt, UpdatedBy)
    VALUES (@cid, @ov, @sp, @en, GETDATE(), @uid);", conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    cmd.Parameters.AddWithValue("@ov", string.IsNullOrEmpty(info.Overview)     ? (object)DBNull.Value : info.Overview);
                    cmd.Parameters.AddWithValue("@sp", string.IsNullOrEmpty(info.SystemPrompt) ? (object)DBNull.Value : info.SystemPrompt);
                    cmd.Parameters.AddWithValue("@en", info.IsEnabled);
                    cmd.Parameters.AddWithValue("@uid", userId.HasValue ? (object)userId.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.SaveTourInfo] " + ex.Message);
                return false;
            }
        }

        // ===== Scene info =====
        public static Dictionary<string, ChatbotSceneInfo> GetSceneInfo(string collectionId)
        {
            var result = new Dictionary<string, ChatbotSceneInfo>();
            if (string.IsNullOrEmpty(collectionId)) return result;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT SceneUuid, ShortIntro, DetailContent
                      FROM dbo.V360_ChatbotSceneInfo WHERE CollectionId=@cid", conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            result[rd.GetString(0)] = new ChatbotSceneInfo
                            {
                                ShortIntro    = rd.IsDBNull(1) ? null : rd.GetString(1),
                                DetailContent = rd.IsDBNull(2) ? null : rd.GetString(2)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.GetSceneInfo] " + ex.Message);
            }
            return result;
        }

        public static int GetConfiguredSceneCount(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId)) return 0;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT COUNT(*) FROM dbo.V360_ChatbotSceneInfo
                      WHERE CollectionId=@cid AND (ShortIntro IS NOT NULL OR DetailContent IS NOT NULL)", conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.GetConfiguredSceneCount] " + ex.Message);
            }
            return 0;
        }

        public static bool SaveSceneInfo(string collectionId, string uuid, ChatbotSceneInfo info, int? userId = null)
        {
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(uuid)) return false;
            try
            {
                bool isEmpty = info == null
                    || (string.IsNullOrEmpty(info.ShortIntro)
                        && string.IsNullOrEmpty(info.DetailContent));

                using (var conn = OpenConnection())
                {
                    if (isEmpty)
                    {
                        using (var cmd = new SqlCommand(
                            "DELETE FROM dbo.V360_ChatbotSceneInfo WHERE CollectionId=@cid AND SceneUuid=@u", conn))
                        {
                            cmd.Parameters.AddWithValue("@cid", collectionId);
                            cmd.Parameters.AddWithValue("@u", uuid);
                            cmd.ExecuteNonQuery();
                        }
                        return true;
                    }
                    using (var cmd = new SqlCommand(@"
MERGE dbo.V360_ChatbotSceneInfo AS T
USING (SELECT @cid AS CollectionId, @u AS SceneUuid) AS S
  ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN UPDATE SET
    ShortIntro=@si, DetailContent=@dc, UpdatedAt=GETDATE(), UpdatedBy=@uid
WHEN NOT MATCHED THEN
    INSERT (CollectionId, SceneUuid, ShortIntro, DetailContent, UpdatedAt, UpdatedBy)
    VALUES (@cid, @u, @si, @dc, GETDATE(), @uid);", conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", collectionId);
                        cmd.Parameters.AddWithValue("@u", uuid);
                        cmd.Parameters.AddWithValue("@si", string.IsNullOrEmpty(info.ShortIntro)    ? (object)DBNull.Value : info.ShortIntro);
                        cmd.Parameters.AddWithValue("@dc", string.IsNullOrEmpty(info.DetailContent) ? (object)DBNull.Value : info.DetailContent);
                        cmd.Parameters.AddWithValue("@uid", userId.HasValue ? (object)userId.Value : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.SaveSceneInfo] " + ex.Message);
                return false;
            }
        }

        // ===== Message log (fire-and-forget OK) =====
        public static void LogMessage(ChatbotMessageLog m)
        {
            if (m == null || string.IsNullOrEmpty(m.CollectionId) || string.IsNullOrEmpty(m.Content)) return;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.V360_ChatbotMessage
    (SessionGuid, NhanVienID, CollectionId, SceneUuid, Role, Content, [Action], TokensIn, TokensOut, LatencyMs, CreatedAt)
VALUES
    (@sg, @nv, @cid, @su, @ro, @ct, @ac, @ti, @to, @la, GETDATE());", conn))
                {
                    cmd.Parameters.AddWithValue("@sg", m.SessionGuid);
                    cmd.Parameters.AddWithValue("@nv", m.NhanVienID.HasValue ? (object)m.NhanVienID.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@cid", m.CollectionId);
                    cmd.Parameters.AddWithValue("@su", string.IsNullOrEmpty(m.SceneUuid) ? (object)DBNull.Value : m.SceneUuid);
                    cmd.Parameters.AddWithValue("@ro", m.Role);
                    cmd.Parameters.AddWithValue("@ct", m.Content);
                    cmd.Parameters.AddWithValue("@ac", string.IsNullOrEmpty(m.Action) ? (object)DBNull.Value : m.Action);
                    cmd.Parameters.AddWithValue("@ti", m.TokensIn.HasValue  ? (object)m.TokensIn.Value  : DBNull.Value);
                    cmd.Parameters.AddWithValue("@to", m.TokensOut.HasValue ? (object)m.TokensOut.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@la", m.LatencyMs.HasValue ? (object)m.LatencyMs.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.LogMessage] " + ex.Message);
            }
        }

        // ===== Rate limit helper =====
        /// <summary>Count messages user gui trong N phut gan day (chi role=user).</summary>
        public static int CountRecentUserMessages(int nhanVienId, int minutes)
        {
            if (nhanVienId <= 0 || minutes <= 0) return 0;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT COUNT(*) FROM dbo.V360_ChatbotMessage
                      WHERE NhanVienID=@nv AND Role=0
                        AND CreatedAt >= DATEADD(MINUTE, -@m, GETDATE())", conn))
                {
                    cmd.Parameters.AddWithValue("@nv", nhanVienId);
                    cmd.Parameters.AddWithValue("@m", minutes);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.CountRecentUserMessages] " + ex.Message);
            }
            return 0;
        }

        /// <summary>
        /// Tong token (TokensIn + TokensOut) da dung trong NGAY hom nay (theo gio server, reset luc 0h).
        /// nhanVienId = null -> tong TOAN HE THONG. Dung cho tran token/ngay (chong dot quota).
        /// Lay tu bang log nen song sot qua IIS recycle (khac MemoryCache).
        /// </summary>
        public static long SumTokensToday(int? nhanVienId)
        {
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT ISNULL(SUM(ISNULL(TokensIn,0) + ISNULL(TokensOut,0)), 0)
                      FROM dbo.V360_ChatbotMessage
                      WHERE CreatedAt >= CAST(GETDATE() AS DATE)
                        AND (@nv IS NULL OR NhanVienID = @nv)", conn))
                {
                    cmd.Parameters.AddWithValue("@nv", nhanVienId.HasValue ? (object)nhanVienId.Value : DBNull.Value);
                    var v = cmd.ExecuteScalar();
                    return (v == null || v == DBNull.Value) ? 0L : Convert.ToInt64(v);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.SumTokensToday] " + ex.Message);
            }
            return 0L;
        }

        // ===== VBee usage (TTS/STT) - tran NGAY (V360_ChatbotUsageDaily) =====
        /// <summary>So lan goi VBee (Kind='tts'|'stt') cua user trong NGAY hom nay (gio server).</summary>
        public static int GetDailyUsageCalls(int nhanVienId, string kind)
        {
            if (nhanVienId <= 0 || string.IsNullOrEmpty(kind)) return 0;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT ISNULL(Calls,0) FROM dbo.V360_ChatbotUsageDaily
                      WHERE NhanVienID=@nv AND UsageDate=CAST(GETDATE() AS DATE) AND Kind=@k", conn))
                {
                    cmd.Parameters.AddWithValue("@nv", nhanVienId);
                    cmd.Parameters.AddWithValue("@k", kind);
                    var v = cmd.ExecuteScalar();
                    return (v == null || v == DBNull.Value) ? 0 : Convert.ToInt32(v);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.GetDailyUsageCalls] " + ex.Message);
            }
            return 0;
        }

        /// <summary>Tang counter usage VBee cho user/ngay/kind (+1 call, +units chars-hoac-giay). Fire-and-forget.</summary>
        public static void IncrementDailyUsage(int nhanVienId, string kind, long units)
        {
            if (nhanVienId <= 0 || string.IsNullOrEmpty(kind)) return;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"MERGE dbo.V360_ChatbotUsageDaily AS t
                      USING (SELECT @nv AS NhanVienID, CAST(GETDATE() AS DATE) AS UsageDate, @k AS Kind) AS s
                      ON (t.NhanVienID = s.NhanVienID AND t.UsageDate = s.UsageDate AND t.Kind = s.Kind)
                      WHEN MATCHED THEN UPDATE SET Calls = t.Calls + 1, Units = t.Units + @u
                      WHEN NOT MATCHED THEN INSERT (NhanVienID, UsageDate, Kind, Calls, Units)
                           VALUES (s.NhanVienID, s.UsageDate, s.Kind, 1, @u);", conn))
                {
                    cmd.Parameters.AddWithValue("@nv", nhanVienId);
                    cmd.Parameters.AddWithValue("@k", kind);
                    cmd.Parameters.AddWithValue("@u", units < 0 ? 0L : units);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatbotContentStore.IncrementDailyUsage] " + ex.Message);
            }
        }
    }
}
