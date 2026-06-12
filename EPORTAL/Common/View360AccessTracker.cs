using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace EPORTAL.Common
{
    /// <summary>
    /// Ghi log truy cap View360 content (Project / Virtual). Fire-and-forget,
    /// khong block request. Dedupe 30 phut/session de tranh dem F5.
    /// Bang: View360_AccessLog (migration: App_Data/migrations/v360-all.sql SECTION 7).
    ///
    /// VI SAO RAW ADO.NET: bang V360_* moi, chua map vao EDMX -> khong query qua EF duoc.
    /// Pattern store chuan cua View360
    /// </summary>
    public static class View360AccessTracker
    {
        public enum ContentType : byte
        {
            Project = 1,
            Virtual = 2
        }

        private const int DedupeMinutes = 30;

        public static void Log(int nhanVienId, ContentType type, int contentId, string sessionId)
        {
            if (nhanVienId <= 0 || contentId <= 0) return;

            // Snapshot conn string trong thread chinh, tranh case ConfigurationManager
            // bi disposed khi Task chay sau.
            string connStr;
            try { connStr = SqlConnectionHelper.GetConnectionString(); }
            catch { return; }

            // Synchronous - INSERT ngay trong request thread. Block ~5-30ms nhung dam bao
            // mark seen complete TRUOC khi Razor render Details. Tradeoff acceptable cho UX
            // (user thay het "Chua xem" badge sau khi click back).
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Dedupe: neu da co row trong DedupeMinutes phut gan day cho cung
                    // (NhanVien, Type, Content, Session) thi bo qua.
                    // CONTRACT: AccessAt column default la GETDATE() (local time) trong
                    // migration v360-all.sql. DateTime.Now phai match. Neu doi default sang
                    // GETUTCDATE -> doi @since sang DateTime.UtcNow tai day.
                    if (!string.IsNullOrEmpty(sessionId))
                    {
                        using (var chk = new SqlCommand(
                            @"SELECT TOP 1 1 FROM dbo.View360_AccessLog
                              WHERE NhanVienID = @nv AND ContentType = @ct
                                AND ContentID = @cid AND SessionID = @sid
                                AND AccessAt >= @since", conn))
                        {
                            chk.Parameters.AddWithValue("@nv", nhanVienId);
                            chk.Parameters.AddWithValue("@ct", (byte)type);
                            chk.Parameters.AddWithValue("@cid", contentId);
                            chk.Parameters.AddWithValue("@sid", sessionId);
                            chk.Parameters.AddWithValue("@since", DateTime.Now.AddMinutes(-DedupeMinutes));
                            var hit = chk.ExecuteScalar();
                            if (hit != null) return;
                        }
                    }

                    using (var ins = new SqlCommand(
                        @"INSERT INTO dbo.View360_AccessLog
                          (NhanVienID, ContentType, ContentID, SessionID)
                          VALUES (@nv, @ct, @cid, @sid)", conn))
                    {
                        ins.Parameters.AddWithValue("@nv", nhanVienId);
                        ins.Parameters.AddWithValue("@ct", (byte)type);
                        ins.Parameters.AddWithValue("@cid", contentId);
                        ins.Parameters.AddWithValue("@sid",
                            (object)sessionId ?? DBNull.Value);
                        ins.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Surface ra Debug Output (Visual Studio Output window) de dev biet
                // INSERT that bai - vd. bang chua ton tai sau khi switch DB.
                System.Diagnostics.Debug.WriteLine("[View360AccessTracker.Log] " + ex.Message);
            }
        }

        /// <summary>
        /// Lay tat ca ContentID ma user da view (theo ContentType). Dung cho UI
        /// "da xem / chua xem" indicator tren card. Returns empty set neu bang chua ton tai
        /// (migration chua chay) hoac loi - de UI fallback graceful.
        /// </summary>
        public static HashSet<int> GetSeenIds(int nhanVienId, ContentType type)
        {
            var result = new HashSet<int>();
            if (nhanVienId <= 0) return result;
            string connStr;
            try { connStr = SqlConnectionHelper.GetConnectionString(); }
            catch { return result; }

            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    // Check table ton tai (migration co the chua chay)
                    using (var chk = new SqlCommand(
                        "SELECT 1 FROM sys.tables WHERE name='View360_AccessLog' AND schema_id=SCHEMA_ID('dbo')", conn))
                    {
                        if (chk.ExecuteScalar() == null) return result;
                    }
                    using (var cmd = new SqlCommand(
                        @"SELECT DISTINCT ContentID FROM dbo.View360_AccessLog
                          WHERE NhanVienID = @nv AND ContentType = @ct", conn))
                    {
                        cmd.Parameters.AddWithValue("@nv", nhanVienId);
                        cmd.Parameters.AddWithValue("@ct", (byte)type);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read()) result.Add(rd.GetInt32(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[View360AccessTracker.GetSeenIds] " + ex.Message);
            }
            return result;
        }

    }
}
