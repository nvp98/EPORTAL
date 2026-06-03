using System;
using System.Configuration;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;

namespace EPORTAL.Common
{
    /// <summary>
    /// Shared SQL connection setup cho cac Store dung raw ADO.NET (bypass EDMX):
    ///   - SceneCalibrationStore, ChatbotContentStore, KuulaSceneStore, View360AccessTracker
    /// EDMX khong auto-include tables V360_*/View360_* moi nen phai dung raw SQL.
    /// Connection string lay tu 'EPORTALEntities' (extract ProviderConnectionString neu la
    /// EntityClient).
    /// </summary>
    public static class SqlConnectionHelper
    {
        /// <summary>Tra ve raw ADO.NET connection string (loai bo metadata= cua EF).</summary>
        public static string GetConnectionString()
        {
            var entry = ConfigurationManager.ConnectionStrings["EPORTALEntities"];
            if (entry == null)
                throw new InvalidOperationException("Connection string 'EPORTALEntities' not found in Web.config");
            var raw = entry.ConnectionString;
            if (raw.IndexOf("metadata=", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                try { return new EntityConnectionStringBuilder(raw).ProviderConnectionString; }
                catch { /* fallthrough: try raw */ }
            }
            return raw;
        }

        /// <summary>Open new SqlConnection. Caller chiu trach nhiem dispose.</summary>
        public static SqlConnection Open()
        {
            var conn = new SqlConnection(GetConnectionString());
            conn.Open();
            return conn;
        }
    }
}
