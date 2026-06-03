using System;
using System.Collections.Generic;
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
        public string ImagePath    { get; set; } // relative path under ~/Content/view360-featured/
    }

    public class TourConfig
    {
        public int?    PinSize       { get; set; }
        public string  PinColor      { get; set; }
        public string  SelectedColor { get; set; }
        public string  ConeColor     { get; set; }
        public double? ConeFanDeg    { get; set; }
        public int?    ConeRadius    { get; set; }

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                return !PinSize.HasValue && string.IsNullOrEmpty(PinColor)
                       && string.IsNullOrEmpty(SelectedColor) && string.IsNullOrEmpty(ConeColor)
                       && !ConeFanDeg.HasValue && !ConeRadius.HasValue;
            }
        }
    }

    /// <summary>
    /// Persist trong cung DB voi EPORTALEntities (V360_SceneCalibration + V360_FeaturedScene + V360_TourConfig).
    /// Schema duoc tao boi migration v360-all.sql SECTION 1 (deploy-time).
    /// One-time import file JSON cu (App_Data/v360-calibration.json) chay lazy lan dau khi co data.
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
                    @"SELECT PinSize, PinColor, SelectedColor, ConeColor, ConeFanDeg, ConeRadius
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
                                ConeRadius    = rd.IsDBNull(5) ? (int?)null : rd.GetInt32(5)
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
    ConeFanDeg=@cf, ConeRadius=@cr, UpdatedAt=GETDATE()
WHEN NOT MATCHED THEN
    INSERT (CollectionId, PinSize, PinColor, SelectedColor, ConeColor, ConeFanDeg, ConeRadius, UpdatedAt)
    VALUES (@cid, @ps, @pc, @sc, @cc, @cf, @cr, GETDATE());", conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", collectionId);
                        cmd.Parameters.AddWithValue("@ps", cfg.PinSize.HasValue ? (object)cfg.PinSize.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@pc", string.IsNullOrEmpty(cfg.PinColor) ? (object)DBNull.Value : cfg.PinColor);
                        cmd.Parameters.AddWithValue("@sc", string.IsNullOrEmpty(cfg.SelectedColor) ? (object)DBNull.Value : cfg.SelectedColor);
                        cmd.Parameters.AddWithValue("@cc", string.IsNullOrEmpty(cfg.ConeColor) ? (object)DBNull.Value : cfg.ConeColor);
                        cmd.Parameters.AddWithValue("@cf", cfg.ConeFanDeg.HasValue ? (object)cfg.ConeFanDeg.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@cr", cfg.ConeRadius.HasValue ? (object)cfg.ConeRadius.Value : DBNull.Value);
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
                    @"SELECT SceneUuid, DisplayOrder, CustomTitle, ImagePath
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
                                ImagePath    = rd.IsDBNull(3) ? null : rd.GetString(3)
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
        /// Replace toan bo featured list cho tour - delete het row cu roi insert moi.
        /// Don gian hon MERGE va dam bao DisplayOrder dung khi admin reorder/remove.
        /// </summary>
        public static bool SaveFeatured(string collectionId, List<FeaturedScene> items)
        {
            if (string.IsNullOrEmpty(collectionId)) return false;
            EnsureLegacyImported();
            try
            {
                using (var conn = OpenConnection())
                using (var tx = conn.BeginTransaction())
                {
                    using (var del = new SqlCommand(
                        "DELETE FROM dbo.V360_FeaturedScene WHERE CollectionId=@cid", conn, tx))
                    {
                        del.Parameters.AddWithValue("@cid", collectionId);
                        del.ExecuteNonQuery();
                    }
                    if (items != null && items.Count > 0)
                    {
                        int ord = 0;
                        foreach (var f in items)
                        {
                            if (string.IsNullOrEmpty(f.SceneUuid)) continue;
                            using (var ins = new SqlCommand(@"
INSERT INTO dbo.V360_FeaturedScene
    (CollectionId, SceneUuid, DisplayOrder, CustomTitle, ImagePath, UpdatedAt)
VALUES (@cid, @u, @ord, @ct, @ip, GETDATE());", conn, tx))
                            {
                                ins.Parameters.AddWithValue("@cid", collectionId);
                                ins.Parameters.AddWithValue("@u", f.SceneUuid);
                                ins.Parameters.AddWithValue("@ord", ord++);
                                ins.Parameters.AddWithValue("@ct",
                                    string.IsNullOrEmpty(f.CustomTitle) ? (object)DBNull.Value : f.CustomTitle);
                                ins.Parameters.AddWithValue("@ip",
                                    string.IsNullOrEmpty(f.ImagePath) ? (object)DBNull.Value : f.ImagePath);
                                ins.ExecuteNonQuery();
                            }
                        }
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
    }
}
