using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace EPORTAL.Common
{
    /// <summary>
    /// Cache audio TTS (VBee) theo hash(voiceCode|speed|text) -> luu DB (V360_ChatbotAudioCache).
    /// Cung text + giong + toc do => cung audio, nen tai dung duoc giua cac user/luot -> BO QUA goi VBee.
    /// (Phase 0 cua plan docs/v360-chatbot-knowledge-cache-plan.md)
    /// </summary>
    public static class AudioCacheStore
    {
        private static SqlConnection OpenConnection() => SqlConnectionHelper.Open();

        /// <summary>Key cache = SHA-256(voiceCode|speed|text) (hex 64 ky tu).</summary>
        public static string HashFor(string voiceCode, double speed, string text)
        {
            var raw = (voiceCode ?? "") + "|"
                    + speed.ToString("0.0###", CultureInfo.InvariantCulture) + "|"
                    + (text ?? "");
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var sb = new StringBuilder(64);
                foreach (var b in bytes) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        /// <summary>Tra audio bytes neu da co trong cache (dong thoi HitCount++ / LastUsedAt). null neu chua co.</summary>
        public static byte[] Get(string textHash)
        {
            if (string.IsNullOrEmpty(textHash)) return null;
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"UPDATE dbo.V360_ChatbotAudioCache
                         SET HitCount = HitCount + 1, LastUsedAt = GETDATE()
                       OUTPUT inserted.AudioData
                       WHERE TextHash = @h", conn))
                {
                    cmd.Parameters.AddWithValue("@h", textHash);
                    var v = cmd.ExecuteScalar();
                    return (v == null || v == DBNull.Value) ? null : (byte[])v;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AudioCacheStore.Get] " + ex.Message);
            }
            return null;
        }

        /// <summary>Lay nhieu audio theo danh sach hash (KHONG tang HitCount - dung cho admin preview).
        /// Tra dict hash -> bytes (chi cac hash co trong cache).</summary>
        public static System.Collections.Generic.Dictionary<string, byte[]> GetMany(System.Collections.Generic.IList<string> hashes)
        {
            var map = new System.Collections.Generic.Dictionary<string, byte[]>(StringComparer.Ordinal);
            if (hashes == null || hashes.Count == 0) return map;
            try
            {
                var sb = new StringBuilder();
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    for (int i = 0; i < hashes.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("@h").Append(i);
                        cmd.Parameters.AddWithValue("@h" + i, hashes[i] ?? "");
                    }
                    cmd.CommandText = "SELECT TextHash, AudioData FROM dbo.V360_ChatbotAudioCache WHERE TextHash IN (" + sb + ")";
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            if (!rd.IsDBNull(1)) map[rd.GetString(0)] = (byte[])rd[1];
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[AudioCacheStore.GetMany] " + ex.Message); }
            return map;
        }

        /// <summary>Xoa nhieu audio theo hash (admin "xoa audio" de cache lai). Tra so dong da xoa.</summary>
        public static int DeleteMany(System.Collections.Generic.IList<string> hashes)
        {
            if (hashes == null || hashes.Count == 0) return 0;
            try
            {
                var sb = new StringBuilder();
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    for (int i = 0; i < hashes.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("@h").Append(i);
                        cmd.Parameters.AddWithValue("@h" + i, hashes[i] ?? "");
                    }
                    cmd.CommandText = "DELETE FROM dbo.V360_ChatbotAudioCache WHERE TextHash IN (" + sb + ")";
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[AudioCacheStore.DeleteMany] " + ex.Message); return 0; }
        }

        /// <summary>Luu audio vao cache (idempotent theo TextHash). Fire-and-forget; loi khong lam vo TTS.</summary>
        public static void Save(string textHash, string voiceCode, double speed, string text, byte[] audio)
        {
            if (string.IsNullOrEmpty(textHash) || audio == null || audio.Length == 0) return;
            try
            {
                var sample = text ?? "";
                if (sample.Length > 400) sample = sample.Substring(0, 400);
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"IF NOT EXISTS (SELECT 1 FROM dbo.V360_ChatbotAudioCache WHERE TextHash = @h)
                      INSERT INTO dbo.V360_ChatbotAudioCache
                          (TextHash, VoiceCode, Speed, SampleText, AudioData, Bytes, HitCount, CreatedAt, LastUsedAt)
                      VALUES (@h, @v, @s, @t, @data, @bytes, 0, GETDATE(), GETDATE());", conn))
                {
                    cmd.Parameters.AddWithValue("@h", textHash);
                    cmd.Parameters.AddWithValue("@v", (object)(voiceCode ?? "") );
                    cmd.Parameters.AddWithValue("@s", speed);
                    cmd.Parameters.AddWithValue("@t", sample);
                    cmd.Parameters.AddWithValue("@data", audio);
                    cmd.Parameters.AddWithValue("@bytes", audio.Length);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AudioCacheStore.Save] " + ex.Message);
            }
        }
    }
}
