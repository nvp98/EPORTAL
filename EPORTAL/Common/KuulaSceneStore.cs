using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json;

namespace EPORTAL.Common
{
    /// <summary>
    /// Persistent storage cho Kuula scenes - phuc vu:
    ///   1) Fallback: khi KuulaCollectionFetcher fetch HTML fail (Kuula down/schema change),
    ///      tra ve last-known data tu DB de minimap van hoat dong (degraded but functional).
    ///   2) GPS drift detection: khi Kuula doi uuid cua scene cu (vi du re-create collection),
    ///      detect bang GPS proximity va migrate calibration data tu uuid cu sang uuid moi.
    ///
    /// Bang: dbo.View360_KuulaSceneCache + dbo.View360_KuulaUuidDrift (audit log).
    /// Migration: App_Data/migrations/v360-all.sql SECTION 8
    ///
    /// Khong dung EDMX (auto-generated khong co cac bang nay). Dung raw ADO.NET.
    /// </summary>
    public static class KuulaSceneStore
    {
        /// <summary>~11m at lat 15°. Same physical drone scene usually < 1m drift.</summary>
        private const double GpsMatchToleranceDeg = 0.0001;


        private static bool TableExists(SqlConnection conn, string tableName, SqlTransaction tx = null)
        {
            using (var cmd = new SqlCommand(
                "SELECT 1 FROM sys.tables WHERE name=@n AND schema_id=SCHEMA_ID('dbo')", conn, tx))
            {
                cmd.Parameters.AddWithValue("@n", tableName);
                return cmd.ExecuteScalar() != null;
            }
        }

        /// <summary>
        /// Upsert tat ca scenes vua fetch tu Kuula. Cap nhat LastSeenAt, FirstSeenAt giu nguyen
        /// (de track scenes ton tai bao lau). Tra ve so rows da affect.
        /// </summary>
        public static int SaveScenes(string collectionId, List<KuulaCollectionFetcher.SceneInfo> scenes)
        {
            if (string.IsNullOrEmpty(collectionId) || scenes == null || scenes.Count == 0) return 0;
            try
            {
                using (var conn = SqlConnectionHelper.Open())
                {
                    if (!TableExists(conn, "View360_KuulaSceneCache")) return 0;

                    int affected = 0;
                    foreach (var s in scenes)
                    {
                        if (string.IsNullOrEmpty(s.Uuid)) continue; // can stable uuid de upsert
                        var spotsJson = (s.Spots != null && s.Spots.Count > 0)
                            ? JsonConvert.SerializeObject(s.Spots) : null;

                        // MERGE upsert
                        using (var cmd = new SqlCommand(
                            @"MERGE dbo.View360_KuulaSceneCache AS T
                              USING (SELECT @cid AS CollectionId, @uuid AS SceneUuid) AS S
                                 ON T.CollectionId = S.CollectionId AND T.SceneUuid = S.SceneUuid
                              WHEN MATCHED THEN UPDATE SET
                                  T.SceneRuntimeId = @rid,
                                  T.Title = @title,
                                  T.Description = @desc,
                                  T.Lat = @lat,
                                  T.Lng = @lng,
                                  T.CameraHeading = @hd,
                                  T.SpotsJson = @spots,
                                  T.LastSeenAt = GETDATE(),
                                  T.UpdatedAt = GETDATE()
                              WHEN NOT MATCHED THEN INSERT
                                  (CollectionId, SceneUuid, SceneRuntimeId, Title, Description,
                                   Lat, Lng, CameraHeading, SpotsJson)
                                  VALUES (@cid, @uuid, @rid, @title, @desc, @lat, @lng, @hd, @spots);", conn))
                        {
                            cmd.Parameters.AddWithValue("@cid", collectionId);
                            cmd.Parameters.AddWithValue("@uuid", s.Uuid);
                            cmd.Parameters.AddWithValue("@rid", (object)s.Id ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@title", (object)s.Title ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@desc", (object)s.Description ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@lat", (object)s.Lat ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@lng", (object)s.Lng ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@hd", (object)s.CameraHeading ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@spots", (object)spotsJson ?? DBNull.Value);
                            affected += cmd.ExecuteNonQuery();
                        }
                    }
                    return affected;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[KuulaSceneStore.SaveScenes] " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Load scenes tu DB (last-known data). Dung khi fetch tu Kuula fail.
        /// </summary>
        public static List<KuulaCollectionFetcher.SceneInfo> LoadScenes(string collectionId)
        {
            var result = new List<KuulaCollectionFetcher.SceneInfo>();
            if (string.IsNullOrEmpty(collectionId)) return result;
            try
            {
                using (var conn = SqlConnectionHelper.Open())
                {
                    if (!TableExists(conn, "View360_KuulaSceneCache")) return result;

                    using (var cmd = new SqlCommand(
                        @"SELECT SceneUuid, SceneRuntimeId, Title, Description,
                                 Lat, Lng, CameraHeading, SpotsJson
                          FROM dbo.View360_KuulaSceneCache
                          WHERE CollectionId = @cid
                          ORDER BY Id", conn))
                    {
                        cmd.Parameters.AddWithValue("@cid", collectionId);
                        using (var rd = cmd.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                var s = new KuulaCollectionFetcher.SceneInfo
                                {
                                    Uuid = rd.GetString(0),
                                    Id = rd.IsDBNull(1) ? null : rd.GetString(1),
                                    Title = rd.IsDBNull(2) ? null : rd.GetString(2),
                                    Description = rd.IsDBNull(3) ? null : rd.GetString(3),
                                    Lat = rd.IsDBNull(4) ? (double?)null : rd.GetDouble(4),
                                    Lng = rd.IsDBNull(5) ? (double?)null : rd.GetDouble(5),
                                    CameraHeading = rd.IsDBNull(6) ? (double?)null : rd.GetDouble(6),
                                    Spots = new List<KuulaCollectionFetcher.SceneSpot>()
                                };
                                if (!rd.IsDBNull(7))
                                {
                                    try
                                    {
                                        s.Spots = JsonConvert.DeserializeObject<List<KuulaCollectionFetcher.SceneSpot>>(rd.GetString(7))
                                                  ?? new List<KuulaCollectionFetcher.SceneSpot>();
                                    }
                                    catch { /* swallow malformed json */ }
                                }
                                result.Add(s);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[KuulaSceneStore.LoadScenes] " + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Detect uuid drift: so sanh scenes vua fetch (`fresh`) voi DB cu (`old`).
        /// Cho moi fresh scene voi uuid CHUA TON TAI trong old, neu GPS match voi 1 old uuid
        /// (now stale - khong xuat hien trong fresh), -> log drift + migrate calibration.
        /// Returns: number of drifts detected.
        /// </summary>
        public static int DetectAndMigrateDrift(
            string collectionId,
            List<KuulaCollectionFetcher.SceneInfo> freshScenes)
        {
            if (string.IsNullOrEmpty(collectionId) || freshScenes == null) return 0;
            try
            {
                var oldScenes = LoadScenes(collectionId);
                if (oldScenes.Count == 0) return 0; // first fetch, no comparison possible

                var freshUuids = new HashSet<string>(freshScenes.Where(s => !string.IsNullOrEmpty(s.Uuid)).Select(s => s.Uuid));
                var oldUuids = new HashSet<string>(oldScenes.Where(s => !string.IsNullOrEmpty(s.Uuid)).Select(s => s.Uuid));

                // Stale: trong DB nhung KHONG con trong fresh (Kuula da xoa hoac doi uuid)
                var stale = oldScenes.Where(s => !freshUuids.Contains(s.Uuid)
                                              && s.Lat.HasValue && s.Lng.HasValue).ToList();
                // Newcomers: trong fresh nhung KHONG co trong DB (moi them hoac uuid moi)
                var newcomers = freshScenes.Where(s => !oldUuids.Contains(s.Uuid)
                                                    && s.Lat.HasValue && s.Lng.HasValue).ToList();

                if (stale.Count == 0 || newcomers.Count == 0) return 0;

                // Build tat ca candidate pairs (newer, stale, dist) trong tolerance,
                // sort tang dan theo dist roi assign greedy -> globally-optimal nearest match.
                // Cu (greedy iterate-newcomers) bi order-dependent: newer B process truoc co the
                // chiem stale X gan A hon, lam A orphan.
                var candidates = new List<Tuple<KuulaCollectionFetcher.SceneInfo, KuulaCollectionFetcher.SceneInfo, double>>();
                foreach (var newer in newcomers)
                {
                    foreach (var oldOne in stale)
                    {
                        var dLat = oldOne.Lat.Value - newer.Lat.Value;
                        var dLng = oldOne.Lng.Value - newer.Lng.Value;
                        if (Math.Abs(dLat) > GpsMatchToleranceDeg || Math.Abs(dLng) > GpsMatchToleranceDeg)
                            continue;
                        var distDeg = Math.Sqrt(dLat * dLat + dLng * dLng);
                        candidates.Add(Tuple.Create(newer, oldOne, distDeg));
                    }
                }
                candidates.Sort((a, b) => a.Item3.CompareTo(b.Item3));

                var usedNewer = new HashSet<string>();
                var usedStale = new HashSet<string>();

                int driftCount = 0;
                using (var conn = SqlConnectionHelper.Open())
                {
                    if (!TableExists(conn, "View360_KuulaUuidDrift")) return 0;

                    foreach (var cand in candidates)
                    {
                        var newer = cand.Item1;
                        var oldOne = cand.Item2;
                        if (usedNewer.Contains(newer.Uuid) || usedStale.Contains(oldOne.Uuid))
                            continue;

                        var distMeters = cand.Item3 * 111000.0; // rough deg-to-meter at low lat

                        // Drift: wrap audit-insert + calib-migrate trong SqlTransaction de tranh
                        // partial state (migrated nhung khong log, hoac log nhung khong migrated).
                        bool migrated = false;
                        using (var tx = conn.BeginTransaction())
                        {
                            try
                            {
                                migrated = MigrateCalibration(conn, tx, collectionId, oldOne.Uuid, newer.Uuid);
                                using (var ins = new SqlCommand(
                                    @"INSERT INTO dbo.View360_KuulaUuidDrift
                                      (CollectionId, OldUuid, NewUuid, Lat, Lng, DistanceMeters, CalibMigrated)
                                      VALUES (@cid, @oldU, @newU, @lat, @lng, @dist, @mig)", conn, tx))
                                {
                                    ins.Parameters.AddWithValue("@cid", collectionId);
                                    ins.Parameters.AddWithValue("@oldU", oldOne.Uuid);
                                    ins.Parameters.AddWithValue("@newU", newer.Uuid);
                                    ins.Parameters.AddWithValue("@lat", newer.Lat.Value);
                                    ins.Parameters.AddWithValue("@lng", newer.Lng.Value);
                                    ins.Parameters.AddWithValue("@dist", distMeters);
                                    ins.Parameters.AddWithValue("@mig", migrated);
                                    ins.ExecuteNonQuery();
                                }
                                tx.Commit();
                            }
                            catch
                            {
                                try { tx.Rollback(); } catch { }
                                throw;
                            }
                        }

                        usedNewer.Add(newer.Uuid);
                        usedStale.Add(oldOne.Uuid);
                        driftCount++;
                        System.Diagnostics.Debug.WriteLine(string.Format(
                            "[KuulaSceneStore] Drift {0}: old={1} -> new={2} ({3:F1}m apart, calib_migrated={4})",
                            collectionId, oldOne.Uuid, newer.Uuid, distMeters, migrated));
                    }
                }
                return driftCount;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[KuulaSceneStore.DetectAndMigrateDrift] " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Migrate SceneCalibration rows tu oldUuid sang newUuid (cung CollectionId).
        /// Tranh duplicate: chi insert neu newUuid CHUA co row.
        /// Tra ve true neu da migrate row.
        /// </summary>
        private static bool MigrateCalibration(SqlConnection conn, SqlTransaction tx, string collectionId, string oldUuid, string newUuid)
        {
            if (!TableExists(conn, "V360_SceneCalibration", tx)) return false;

            using (var cmd = new SqlCommand(
                @"INSERT INTO dbo.V360_SceneCalibration
                    (CollectionId, SceneUuid, [Offset], HidePin, CustomTitle, UpdatedAt)
                  SELECT @cid, @newU, [Offset], HidePin, CustomTitle, GETDATE()
                  FROM dbo.V360_SceneCalibration
                  WHERE CollectionId = @cid AND SceneUuid = @oldU
                    AND NOT EXISTS (
                        SELECT 1 FROM dbo.V360_SceneCalibration
                        WHERE CollectionId = @cid AND SceneUuid = @newU
                    );", conn, tx))
            {
                cmd.Parameters.AddWithValue("@cid", collectionId);
                cmd.Parameters.AddWithValue("@oldU", oldUuid);
                cmd.Parameters.AddWithValue("@newU", newUuid);
                return cmd.ExecuteNonQuery() > 0;
            }
            // KHONG catch: exception se propagate ra ngoai, transaction ngoai goi rollback.
        }
    }
}
