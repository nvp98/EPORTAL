using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPORTAL.Common
{
    public class SceneCalibration
    {
        public double? Offset      { get; set; }
        public bool    HidePin     { get; set; }
        public string  CustomTitle { get; set; }

        [JsonIgnore]
        public bool IsEmpty
        {
            get { return !Offset.HasValue && !HidePin && string.IsNullOrEmpty(CustomTitle); }
        }
    }

    /// <summary>
    /// Featured scene: 1 entry tren list "Khu vuc tham quan chinh" o Intro page.
    /// Admin pick scene, upload anh card, custom title (override Kuula's). DisplayOrder
    /// quyet dinh thu tu hien thi.
    /// </summary>
    public class FeaturedScene
    {
        public string SceneUuid    { get; set; }
        public int    DisplayOrder { get; set; }
        public string CustomTitle  { get; set; }
        public string ImagePath    { get; set; } // legacy fallback under ~/Content/view360-featured/
        public bool   HasImage     { get; set; }
        public string ImageUrl     { get; set; }
        public long   ImageVersion { get; set; }
    }

    public class FeaturedSceneImage
    {
        public string   FileName        { get; set; }
        public string   LegacyImagePath { get; set; }   // duong dan file tren server (~/Content/view360-featured/...)
        public DateTime UpdatedAt       { get; set; }
    }

    public class TourConfig
    {
        public int?    PinSize       { get; set; }
        public string  PinColor      { get; set; }
        public string  SelectedColor { get; set; }
        public string  ConeColor     { get; set; }
        public double? ConeFanDeg    { get; set; }
        public int?    ConeRadius    { get; set; }
        public bool?   ShowMinimap   { get; set; }   // null/true = hien (default); false = an minimap cho nguoi xem

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                // ShowMinimap=false la cau hinh CO Y NGHIA -> KHONG coi la empty (phai luu, khong xoa row).
                return !PinSize.HasValue && string.IsNullOrEmpty(PinColor)
                       && string.IsNullOrEmpty(SelectedColor) && string.IsNullOrEmpty(ConeColor)
                       && !ConeFanDeg.HasValue && !ConeRadius.HasValue
                       && (!ShowMinimap.HasValue || ShowMinimap.Value);
            }
        }
    }

    /// <summary>
    /// Persist trong cung DB voi EPORTALEntities (V360_SceneCalibration + V360_FeaturedScene + V360_TourConfig).
    /// Schema duoc tao boi migration v360-all.sql SECTION 1 (deploy-time).
    /// One-time import file JSON cu (App_Data/v360-calibration.json) chay lazy lan dau khi co data.
    ///
    /// VI SAO RAW ADO.NET: cac bang V360_* la bang MOI, chua map vao EDMX -> khong query qua EF duoc.
    /// Pattern store chuan cua View360 (static class + ADO.NET tham so hoa + using)
    /// </summary>
    public static class SceneCalibrationStore
    {
        private static readonly object _initLock = new object();
        private static bool _legacyImported = false;

        private static SqlConnection OpenConnection() => SqlConnectionHelper.Open();

        // Schema duoc tao boi migration v360-all.sql SECTION 1 (deploy-time).
        // Tai day chi xu ly one-time import tu file JSON cu (~/App_Data/v360-calibration.json).
        private static void EnsureLegacyImported()
        {
            if (_legacyImported) return;
            lock (_initLock)
            {
                if (_legacyImported) return;
                try { TryImportLegacyJson(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore] LegacyImport err: " + ex.Message);
                }
                _legacyImported = true;
            }
        }

        /// <summary>Import file JSON cu (neu co) vao DB lan dau. File se duoc rename sau khi xong.</summary>
        private static void TryImportLegacyJson()
        {
            try
            {
                string path = null;
                var ctx = HttpContext.Current;
                if (ctx != null) path = ctx.Server.MapPath("~/App_Data/v360-calibration.json");
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

                var json = File.ReadAllText(path);
                var root = JObject.Parse(json);
                int imported = 0;
                foreach (var colProp in root.Properties())
                {
                    var cid = colProp.Name;
                    var col = colProp.Value as JObject;
                    if (col == null) continue;
                    foreach (var p in col.Properties())
                    {
                        if (p.Name == "__config__")
                        {
                            var cfg = p.Value.ToObject<TourConfig>();
                            if (cfg != null && !cfg.IsEmpty) { SaveTourConfigInternal(cid, cfg); imported++; }
                        }
                        else
                        {
                            var sc = p.Value.ToObject<SceneCalibration>();
                            if (sc != null && !sc.IsEmpty) { SaveSceneInternal(cid, p.Name, sc); imported++; }
                        }
                    }
                }
                if (imported > 0)
                {
                    var bak = path + ".imported-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
                    File.Move(path, bak);
                    System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore] Imported " + imported + " entries from legacy JSON -> renamed to " + Path.GetFileName(bak));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore] TryImportLegacyJson err: " + ex.Message);
            }
        }

        // ===== Per-scene =====
        public static Dictionary<string, SceneCalibration> Get(string collectionId)
        {
            var result = new Dictionary<string, SceneCalibration>();
            if (string.IsNullOrEmpty(collectionId)) return result;
            EnsureLegacyImported();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    "SELECT SceneUuid, [Offset], HidePin, CustomTitle FROM dbo.V360_SceneCalibration WHERE CollectionId=@cid",
                    conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            result[rd.GetString(0)] = new SceneCalibration
                            {
                                Offset      = rd.IsDBNull(1) ? (double?)null : rd.GetDouble(1),
                                HidePin     = !rd.IsDBNull(2) && rd.GetBoolean(2),
                                CustomTitle = rd.IsDBNull(3) ? null : rd.GetString(3)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.Get] " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Dem so scene da calibrate cho NHIEU collection trong 1 query (GROUP BY).
        /// Tranh N+1: truoc day trang admin goi Get(cid).Count moi tour -> moi tour 1 connection.
        /// Tra dict collectionId -> count (chi cac collection co du lieu).
        /// </summary>
        public static Dictionary<string, int> GetCalibratedCounts(IEnumerable<string> collectionIds)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            return CountByCollection("dbo.V360_SceneCalibration", collectionIds, result);
        }

        /// <summary>Dem so featured scene cho NHIEU collection trong 1 query (GROUP BY).</summary>
        public static Dictionary<string, int> GetFeaturedCounts(IEnumerable<string> collectionIds)
        {
            var result = new Dictionary<string, int>(StringComparer.Ordinal);
            return CountByCollection("dbo.V360_FeaturedScene", collectionIds, result);
        }

        private static Dictionary<string, int> CountByCollection(string table, IEnumerable<string> collectionIds, Dictionary<string, int> result)
        {
            var ids = new List<string>();
            if (collectionIds != null)
                foreach (var c in collectionIds)
                    if (!string.IsNullOrEmpty(c) && !result.ContainsKey(c)) { result[c] = 0; ids.Add(c); }
            if (ids.Count == 0) return result;
            EnsureLegacyImported();
            try
            {
                var sb = new System.Text.StringBuilder();
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    for (int i = 0; i < ids.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("@c").Append(i);
                        cmd.Parameters.AddWithValue("@c" + i, ids[i]);
                    }
                    // table la literal noi bo (khong phai input user) -> an toan ghep chuoi
                    cmd.CommandText = "SELECT CollectionId, COUNT(*) FROM " + table +
                                      " WHERE CollectionId IN (" + sb + ") GROUP BY CollectionId";
                    using (var rd = cmd.ExecuteReader())
                        while (rd.Read())
                            if (!rd.IsDBNull(0)) result[rd.GetString(0)] = rd.GetInt32(1);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.CountByCollection] " + ex.Message);
            }
            return result;
        }

        public static bool Save(string collectionId, string uuid, SceneCalibration calib)
        {
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(uuid)) return false;
            EnsureLegacyImported();
            return SaveSceneInternal(collectionId, uuid, calib);
        }

        private static bool SaveSceneInternal(string collectionId, string uuid, SceneCalibration calib)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    if (calib == null || calib.IsEmpty)
                    {
                        using (var cmd = new SqlCommand(
                            "DELETE FROM dbo.V360_SceneCalibration WHERE CollectionId=@cid AND SceneUuid=@u", conn))
                        {
                            cmd.Parameters.AddWithValue("@cid", collectionId);
                            cmd.Parameters.AddWithValue("@u", uuid);
                            cmd.ExecuteNonQuery();
                        }
                        return true;
                    }
                    using (var cmd = new SqlCommand(@"
MERGE dbo.V360_SceneCalibration AS T
USING (SELECT @cid AS CollectionId, @u AS SceneUuid) AS S
  ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN
    UPDATE SET [Offset]=@o, HidePin=@hp, CustomTitle=@ct, UpdatedAt=GETDATE()
WHEN NOT MATCHED THEN
    INSERT (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt)
    VALUES (@cid, @u, @o, @hp, @ct, GETDATE());", conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", collectionId);
                        cmd.Parameters.AddWithValue("@u", uuid);
                        cmd.Parameters.AddWithValue("@o", calib.Offset.HasValue ? (object)calib.Offset.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@hp", calib.HidePin);
                        cmd.Parameters.AddWithValue("@ct",
                            string.IsNullOrEmpty(calib.CustomTitle) ? (object)DBNull.Value : calib.CustomTitle);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.Save] " + ex.Message);
                return false;
            }
        }

        // ===== Tour config =====
        public static TourConfig GetTourConfig(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId)) return new TourConfig();
            EnsureLegacyImported();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT PinSize, PinColor, SelectedColor, ConeColor, ConeFanDeg, ConeRadius, ShowMinimap
                      FROM dbo.V360_TourConfig WHERE CollectionId=@cid", conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            return new TourConfig
                            {
                                PinSize       = rd.IsDBNull(0) ? (int?)null : rd.GetInt32(0),
                                PinColor      = rd.IsDBNull(1) ? null : rd.GetString(1),
                                SelectedColor = rd.IsDBNull(2) ? null : rd.GetString(2),
                                ConeColor     = rd.IsDBNull(3) ? null : rd.GetString(3),
                                ConeFanDeg    = rd.IsDBNull(4) ? (double?)null : rd.GetDouble(4),
                                ConeRadius    = rd.IsDBNull(5) ? (int?)null : rd.GetInt32(5),
                                ShowMinimap   = rd.IsDBNull(6) ? (bool?)null : rd.GetBoolean(6)
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.GetTourConfig] " + ex.Message);
            }
            return new TourConfig();
        }

        public static bool SaveTourConfig(string collectionId, TourConfig cfg)
        {
            if (string.IsNullOrEmpty(collectionId)) return false;
            EnsureLegacyImported();
            return SaveTourConfigInternal(collectionId, cfg);
        }

        private static bool SaveTourConfigInternal(string collectionId, TourConfig cfg)
        {
            try
            {
                using (var conn = OpenConnection())
                {
                    if (cfg == null || cfg.IsEmpty)
                    {
                        using (var cmd = new SqlCommand(
                            "DELETE FROM dbo.V360_TourConfig WHERE CollectionId=@cid", conn))
                        {
                            cmd.Parameters.AddWithValue("@cid", collectionId);
                            cmd.ExecuteNonQuery();
                        }
                        return true;
                    }
                    using (var cmd = new SqlCommand(@"
MERGE dbo.V360_TourConfig AS T
USING (SELECT @cid AS CollectionId) AS S ON T.CollectionId = S.CollectionId
WHEN MATCHED THEN UPDATE SET
    PinSize=@ps, PinColor=@pc, SelectedColor=@sc, ConeColor=@cc,
    ConeFanDeg=@cf, ConeRadius=@cr, ShowMinimap=@mm, UpdatedAt=GETDATE()
WHEN NOT MATCHED THEN
    INSERT (CollectionId, PinSize, PinColor, SelectedColor, ConeColor, ConeFanDeg, ConeRadius, ShowMinimap, UpdatedAt)
    VALUES (@cid, @ps, @pc, @sc, @cc, @cf, @cr, @mm, GETDATE());", conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", collectionId);
                        cmd.Parameters.AddWithValue("@ps", cfg.PinSize.HasValue ? (object)cfg.PinSize.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@pc", string.IsNullOrEmpty(cfg.PinColor) ? (object)DBNull.Value : cfg.PinColor);
                        cmd.Parameters.AddWithValue("@sc", string.IsNullOrEmpty(cfg.SelectedColor) ? (object)DBNull.Value : cfg.SelectedColor);
                        cmd.Parameters.AddWithValue("@cc", string.IsNullOrEmpty(cfg.ConeColor) ? (object)DBNull.Value : cfg.ConeColor);
                        cmd.Parameters.AddWithValue("@cf", cfg.ConeFanDeg.HasValue ? (object)cfg.ConeFanDeg.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@cr", cfg.ConeRadius.HasValue ? (object)cfg.ConeRadius.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@mm", cfg.ShowMinimap.HasValue ? (object)cfg.ShowMinimap.Value : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.SaveTourConfig] " + ex.Message);
                return false;
            }
        }

        // ===== Featured scenes (Khu vuc tham quan chinh) =====
        public static List<FeaturedScene> GetFeatured(string collectionId)
        {
            var result = new List<FeaturedScene>();
            if (string.IsNullOrEmpty(collectionId)) return result;
            EnsureLegacyImported();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(
                    @"SELECT SceneUuid, DisplayOrder, CustomTitle, ImagePath,
                             CASE WHEN NULLIF(ImagePath, N'') IS NOT NULL
                                  THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS HasImage,
                             UpdatedAt
                      FROM dbo.V360_FeaturedScene
                      WHERE CollectionId=@cid
                      ORDER BY DisplayOrder ASC, Id ASC", conn))
                {
                    cmd.Parameters.AddWithValue("@cid", collectionId);
                    using (var rd = cmd.ExecuteReader())
                    {
                        while (rd.Read())
                        {
                            result.Add(new FeaturedScene
                            {
                                SceneUuid    = rd.GetString(0),
                                DisplayOrder = rd.GetInt32(1),
                                CustomTitle  = rd.IsDBNull(2) ? null : rd.GetString(2),
                                ImagePath    = rd.IsDBNull(3) ? null : rd.GetString(3),
                                HasImage     = !rd.IsDBNull(4) && rd.GetBoolean(4),
                                ImageVersion = rd.IsDBNull(5) ? 0L : rd.GetDateTime(5).Ticks
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.GetFeatured] " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Upsert featured list cho tour, giu nguyen image bytes khi admin reorder/rename,
        /// va xoa cac row khong con nam trong danh sach.
        /// </summary>
        public static bool SaveFeatured(string collectionId, List<FeaturedScene> items)
        {
            if (string.IsNullOrEmpty(collectionId) || collectionId.Length > 50) return false;
            EnsureLegacyImported();
            try
            {
                var normalized = new List<FeaturedScene>();
                var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item == null || string.IsNullOrWhiteSpace(item.SceneUuid)) continue;
                        var uuid = item.SceneUuid.Trim();
                        if (uuid.Length > 100 || !seen.Add(uuid)) continue;
                        if (!string.IsNullOrEmpty(item.CustomTitle) && item.CustomTitle.Length > 500) return false;
                        normalized.Add(new FeaturedScene
                        {
                            SceneUuid = uuid,
                            CustomTitle = string.IsNullOrWhiteSpace(item.CustomTitle) ? null : item.CustomTitle.Trim()
                        });
                    }
                }

                using (var conn = OpenConnection())
                using (var tx = conn.BeginTransaction())
                {
                    using (var createKeep = new SqlCommand(
                        "CREATE TABLE #FeaturedKeep (SceneUuid NVARCHAR(100) NOT NULL PRIMARY KEY);", conn, tx))
                    {
                        createKeep.ExecuteNonQuery();
                    }

                    int ord = 0;
                    foreach (var f in normalized)
                    {
                        using (var upsert = new SqlCommand(@"
INSERT INTO #FeaturedKeep (SceneUuid) VALUES (@u);

MERGE dbo.V360_FeaturedScene AS T
USING (SELECT @cid AS CollectionId, @u AS SceneUuid) AS S
  ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN
    UPDATE SET DisplayOrder=@ord, CustomTitle=@ct, UpdatedAt=GETDATE()
WHEN NOT MATCHED THEN
    INSERT (CollectionId, SceneUuid, DisplayOrder, CustomTitle, UpdatedAt)
    VALUES (@cid, @u, @ord, @ct, GETDATE());", conn, tx))
                        {
                            upsert.Parameters.Add("@cid", SqlDbType.NVarChar, 50).Value = collectionId;
                            upsert.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = f.SceneUuid;
                            upsert.Parameters.Add("@ord", SqlDbType.Int).Value = ord++;
                            upsert.Parameters.Add("@ct", SqlDbType.NVarChar, 500).Value =
                                string.IsNullOrEmpty(f.CustomTitle) ? (object)DBNull.Value : f.CustomTitle;
                            upsert.ExecuteNonQuery();
                        }
                    }

                    using (var deleteRemoved = new SqlCommand(@"
DELETE F
FROM dbo.V360_FeaturedScene AS F
WHERE F.CollectionId=@cid
  AND NOT EXISTS (SELECT 1 FROM #FeaturedKeep AS K WHERE K.SceneUuid=F.SceneUuid);", conn, tx))
                    {
                        deleteRemoved.Parameters.Add("@cid", SqlDbType.NVarChar, 50).Value = collectionId;
                        deleteRemoved.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.SaveFeatured] " + ex.Message);
                return false;
            }
        }

        public static FeaturedSceneImage GetFeaturedImage(string collectionId, string sceneUuid)
        {
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(sceneUuid)) return null;
            EnsureLegacyImported();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(@"
SELECT ImageFileName, ImagePath, UpdatedAt
FROM dbo.V360_FeaturedScene
WHERE CollectionId=@cid AND SceneUuid=@u;", conn))
                {
                    cmd.Parameters.Add("@cid", SqlDbType.NVarChar, 50).Value = collectionId;
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = sceneUuid;
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read()) return null;
                        return new FeaturedSceneImage
                        {
                            FileName        = rd.IsDBNull(0) ? null : rd.GetString(0),
                            LegacyImagePath = rd.IsDBNull(1) ? null : rd.GetString(1),
                            UpdatedAt       = rd.GetDateTime(2)
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.GetFeaturedImage] " + ex.Message);
                return null;
            }
        }

        // RULE: anh featured luu thanh FILE tren server, DB chi giu DUONG DAN (ImagePath).
        public static bool SaveFeaturedImagePath(string collectionId, string sceneUuid, string imagePath, string fileName)
        {
            if (string.IsNullOrEmpty(collectionId) || collectionId.Length > 50
                || string.IsNullOrEmpty(sceneUuid) || sceneUuid.Length > 100
                || string.IsNullOrEmpty(imagePath) || imagePath.Length > 500)
                return false;

            EnsureLegacyImported();
            try
            {
                using (var conn = OpenConnection())
                using (var cmd = new SqlCommand(@"
DECLARE @ord INT =
    ISNULL((SELECT MAX(DisplayOrder) + 1 FROM dbo.V360_FeaturedScene WHERE CollectionId=@cid), 0);

MERGE dbo.V360_FeaturedScene AS T
USING (SELECT @cid AS CollectionId, @u AS SceneUuid) AS S
  ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
WHEN MATCHED THEN
    UPDATE SET ImagePath=@path, ImageFileName=@fileName, UpdatedAt=GETDATE()
WHEN NOT MATCHED THEN
    INSERT (CollectionId, SceneUuid, DisplayOrder, ImagePath, ImageFileName, UpdatedAt)
    VALUES (@cid, @u, @ord, @path, @fileName, GETDATE());", conn))
                {
                    cmd.Parameters.Add("@cid", SqlDbType.NVarChar, 50).Value = collectionId;
                    cmd.Parameters.Add("@u", SqlDbType.NVarChar, 100).Value = sceneUuid;
                    cmd.Parameters.Add("@path", SqlDbType.NVarChar, 500).Value = imagePath;
                    cmd.Parameters.Add("@fileName", SqlDbType.NVarChar, 255).Value =
                        string.IsNullOrEmpty(fileName) ? (object)DBNull.Value : fileName;
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SceneCalibrationStore.SaveFeaturedImagePath] " + ex.Message);
                return false;
            }
        }
    }
}
