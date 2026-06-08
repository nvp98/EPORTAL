using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace EPORTAL.Common
{
    /// <summary>
    /// Fetch Kuula collection share page, extract embedded base64 JSON metadata,
    /// parse out per-post GPS (lat/lng) and titles. No API token required - data is
    /// embedded server-side in window.KUULA_COLLECTION.data on the share page.
    ///
    /// Result cached 24h in MemoryCache (keyed by collectionId). Outbound HTTP only
    /// fires on cache miss / expiry; safe to call from per-request controller action.
    /// </summary>
    public static class KuulaCollectionFetcher
    {
        private const string KuulaShareUrlFmt = "https://kuula.co/share/collection/{0}";
        private const int CacheMinutes = 1440;   // 24h - GPS rarely changes
        private const int TimeoutSec   = 10;
        private static readonly Regex DataExtractRx = new Regex(
            @"window\.KUULA_COLLECTION\s*=\s*\{[^}]*?data:\s*""([^""]+)""",
            RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex CollectionIdRx = new Regex(
            @"collection/([A-Za-z0-9]+)", RegexOptions.Compiled);

        public class SceneInfo
        {
            public string Id          { get; set; }
            // Kuula dung 'id' khac giua share page va iframe postMessage. 'uuid' la dinh danh on
            // dinh xuyen suot (cung xuat hien trong cover URL nen client lookup duoc).
            public string Uuid        { get; set; }
            public string Title       { get; set; }
            public string Description { get; set; }
            public double? Lat        { get; set; }
            public double? Lng        { get; set; }
            public double? CameraHeading { get; set; }
            public List<SceneSpot> Spots { get; set; } = new List<SceneSpot>();
        }

        public class SceneSpot
        {
            public string TargetId { get; set; }
            public double Yaw      { get; set; }
        }

        /// <summary>Pull collection ID from a Kuula share/embed URL.</summary>
        public static string ExtractCollectionId(string kuulaUrl)
        {
            if (string.IsNullOrEmpty(kuulaUrl)) return null;
            var m = CollectionIdRx.Match(kuulaUrl);
            return m.Success ? m.Groups[1].Value : null;
        }

        /// <summary>
        /// Get all scenes (with GPS where set) for a collection. 3-layer fault tolerance:
        ///   L1: MemoryCache 24h (fast)
        ///   L2: Live fetch tu Kuula HTML + persist to DB (refresh + drift detection)
        ///   L3: DB last-known data (fallback khi Kuula down / fetch fail)
        /// Returns empty list ONLY khi cac 3 layers deu fail (rare).
        /// </summary>
        public static List<SceneInfo> GetScenes(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId)) return new List<SceneInfo>();

            // L1: MemoryCache
            var cacheKey = "kuula_scenes_" + collectionId;
            var cached = MemoryCache.Default.Get(cacheKey) as List<SceneInfo>;
            if (cached != null) return cached;

            // L2: Live fetch
            List<SceneInfo> scenes = null;
            string fetchError = null;
            try
            {
                scenes = FetchAndParse(collectionId);
            }
            catch (Exception ex)
            {
                fetchError = ex.GetType().Name + ": " + ex.Message;
                System.Diagnostics.Debug.WriteLine("[KuulaFetcher] " + collectionId
                    + " EXCEPTION: " + fetchError);
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine("[KuulaFetcher]   inner: " + ex.InnerException.Message);
                }
            }

            if (scenes != null && scenes.Count > 0)
            {
                // Detect uuid drift TRUOC khi save (so sanh fresh vs old DB)
                // -> auto-migrate SceneCalibration neu Kuula doi uuid cua scene cu.
                try { KuulaSceneStore.DetectAndMigrateDrift(collectionId, scenes); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KuulaFetcher] drift err: " + ex.Message); }

                // Persist snapshot moi -> serve fallback lan sau khi Kuula fail
                try { KuulaSceneStore.SaveScenes(collectionId, scenes); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[KuulaFetcher] save err: " + ex.Message); }

                MemoryCache.Default.Set(cacheKey, scenes,
                    DateTimeOffset.UtcNow.AddMinutes(CacheMinutes));
                return scenes;
            }

            // L3: Fallback to DB last-known
            try
            {
                var dbScenes = KuulaSceneStore.LoadScenes(collectionId);
                if (dbScenes.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("[KuulaFetcher] " + collectionId
                        + " FALLBACK to DB (" + dbScenes.Count + " scenes) - fetch_err=" + (fetchError ?? "empty result"));
                    // Cache fallback 5min (ngan hon 24h - cho phep retry live som hon)
                    MemoryCache.Default.Set(cacheKey, dbScenes,
                        DateTimeOffset.UtcNow.AddMinutes(5));
                    return dbScenes;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[KuulaFetcher] DB fallback err: " + ex.Message);
            }

            // All layers failed - return empty (don't cache)
            System.Diagnostics.Debug.WriteLine("[KuulaFetcher] " + collectionId
                + " ALL LAYERS FAILED (fetch + DB empty)");
            return new List<SceneInfo>();
        }

        /// <summary>Clear cached scenes for one collection (call sau khi Kuula regenerate IDs).</summary>
        public static void InvalidateCache(string collectionId)
        {
            if (string.IsNullOrEmpty(collectionId)) return;
            MemoryCache.Default.Remove("kuula_scenes_" + collectionId);
        }

        private const int MaxResponseBytes = 5 * 1024 * 1024;  // 5MB - share page chi ~200KB-1MB
        private const int MaxRedirects = 3;
        private const string KuulaHost = "kuula.co";

        private static List<SceneInfo> FetchAndParse(string collectionId)
        {
            var url = string.Format(KuulaShareUrlFmt, collectionId);
            string html;

            // TLS 1.2 for .NET 4.7.2 default; Kuula refuses older.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Timeout = TimeoutSec * 1000;
            req.ReadWriteTimeout = TimeoutSec * 1000;
            req.UserAgent = "Mozilla/5.0 EPORTAL-View360-Fetcher";
            req.Accept = "text/html";
            // SSRF guard: chi follow redirect ngan, va manually verify Location stay tren kuula.co.
            req.AllowAutoRedirect = false;
            req.MaximumAutomaticRedirections = MaxRedirects;

            HttpWebResponse resp;
            int redirectCount = 0;
            while (true)
            {
                resp = (HttpWebResponse)req.GetResponse();
                var status = (int)resp.StatusCode;
                if (status < 300 || status >= 400) break;
                if (++redirectCount > MaxRedirects) { resp.Close(); throw new Exception("Too many redirects"); }
                var location = resp.Headers["Location"];
                resp.Close();
                if (string.IsNullOrEmpty(location)) throw new Exception("Redirect without Location");
                var nextUri = new Uri(new Uri(url), location);
                // SSRF: only allow same host (kuula.co), not internal IPs
                if (!nextUri.Host.EndsWith(KuulaHost, StringComparison.OrdinalIgnoreCase))
                    throw new Exception("Refused cross-host redirect to " + nextUri.Host);
                req = (HttpWebRequest)WebRequest.Create(nextUri);
                req.Method = "GET";
                req.Timeout = TimeoutSec * 1000;
                req.UserAgent = "Mozilla/5.0 EPORTAL-View360-Fetcher";
                req.Accept = "text/html";
                req.AllowAutoRedirect = false;
            }

            using (resp)
            using (var stream = resp.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                // OOM guard: cap response size. Doc theo chunk de stop som neu vuot.
                var buf = new char[8192];
                var sb = new StringBuilder();
                int totalChars = 0;
                int maxChars = MaxResponseBytes; // approx (UTF-8 worst case 1 char = 4 bytes; conservative)
                int n;
                while ((n = reader.Read(buf, 0, buf.Length)) > 0)
                {
                    totalChars += n;
                    if (totalChars > maxChars) throw new Exception("Response > " + (MaxResponseBytes / 1024 / 1024) + "MB - refused");
                    sb.Append(buf, 0, n);
                }
                html = sb.ToString();
            }

            var m = DataExtractRx.Match(html);
            if (!m.Success)
            {
                System.Diagnostics.Debug.WriteLine("[KuulaFetcher] window.KUULA_COLLECTION.data not found in HTML");
                return new List<SceneInfo>();
            }

            var base64 = m.Groups[1].Value;
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
            var root = JObject.Parse(json);

            // NOTE: settings.excludes la danh sach "hide from menu" cua Kuula author,
            // KHONG phai "hide from map". Kuula's built-in map van show diem do.
            // Truoc day fetcher filter qua aggressive loai mat ~70% scene.

            var posts = root["posts"] as JArray;
            if (posts == null)
            {
                System.Diagnostics.Debug.WriteLine("[KuulaFetcher] root.posts missing or null - Kuula JSON schema changed?");
                return new List<SceneInfo>();
            }

            var result = new List<SceneInfo>(posts.Count);
            int failed = 0;
            string firstError = null;
            foreach (var p in posts)
            {
                try
                {
                var id = p["id"]?.ToString();
                if (string.IsNullOrEmpty(id)) continue;

                var scene = new SceneInfo
                {
                    Id = id,
                    Uuid = p["uuid"]?.ToString(),
                    Title = p["title"]?.ToString() ?? p["description"]?.ToString(),
                    Description = p["description"]?.ToString()
                };

                // location.latlng = { lat, lng }. Kuula's "hidden" flag che location info panel,
                // KHONG nen che marker tren map - lay GPS bat ke hidden de match Kuula's map behavior.
                var loc = p["location"] as JObject;
                if (loc != null)
                {
                    var latlng = loc["latlng"] as JObject;
                    if (latlng != null)
                    {
                        // Dung Value<double?>() (khong phai <double>) - mot so post co latlng:{lat:null}
                        // se throw InvalidCastException neu cast null sang non-nullable double.
                        scene.Lat = latlng["lat"]?.Value<double?>();
                        scene.Lng = latlng["lng"]?.Value<double?>();
                    }
                }

                // CRITICAL: photos[0].options.heading (radians) la "Heading X" trong Kuula edit panel.
                // Day la per-scene initial-view direction nguoi capture set trong Kuula.
                // Tu observation, mai scene cua tour HPDQ co value rieng (0°-345°), correlate voi
                // huong cone bi lech tren minimap. Convert sang degrees, dung lam default offset.
                var photos = p["photos"] as JArray;
                if (photos != null && photos.Count > 0)
                {
                    var opts = photos[0]["options"] as JObject;
                    if (opts != null)
                    {
                        var hRad = opts["heading"]?.Value<double?>();
                        if (hRad.HasValue)
                        {
                            scene.CameraHeading = hRad.Value * 180.0 / Math.PI;
                        }
                    }
                }

                // Hotspots: tour HPDQ co addons embedded trong photos[0].addons (JSON STRING).
                // Parse 2-lop: outer JArray cho post, inner string -> JArray cho addons.
                // Most posts in this tour khong co in-collection nav addons, fetcher van work cho
                // cac tour khac voi structure khac.
                var spotsArr = (p["spots"] as JArray)
                            ?? (p["markers"] as JArray)
                            ?? (p["hotspots"] as JArray);
                if (spotsArr != null)
                {
                    foreach (var s in spotsArr)
                    {
                        // Chi nhan navigation spots ('point' type) - bo info/video/audio/etc.
                        var spotType = s["type"]?.ToString();
                        if (!string.IsNullOrEmpty(spotType) &&
                            !string.Equals(spotType, "point", StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(spotType, "nav", StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Target id: try nhieu field name
                        var target = s["target"]?.ToString()
                                  ?? s["targetUid"]?.ToString()
                                  ?? s["targetId"]?.ToString();
                        if (string.IsNullOrEmpty(target)) continue;

                        // Yaw: 'pan' la field chinh cua Kuula, fallback yaw/heading
                        var yaw = s["pan"]?.Value<double?>()
                               ?? s["yaw"]?.Value<double?>()
                               ?? s["heading"]?.Value<double?>();
                        if (!yaw.HasValue) continue;

                        scene.Spots.Add(new SceneSpot { TargetId = target, Yaw = yaw.Value });
                    }
                }

                result.Add(scene);
                }
                catch (Exception exPost)
                {
                    failed++;
                    if (firstError == null) firstError = exPost.GetType().Name + ": " + exPost.Message;
                    // KHONG abort cac post con lai chi vi 1 post bi loi schema
                }
            }

            System.Diagnostics.Debug.WriteLine(string.Format(
                "[KuulaFetcher] {0}: parsed {1}/{2} posts ({3} failed){4}",
                collectionId, result.Count, posts.Count, failed,
                failed > 0 ? " - first err: " + firstError : ""));

            // Diagnostic: dump 1 post raw structure
            if (posts.Count > 0 && (result.Count == 0 || failed > posts.Count / 4))
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[KuulaFetcher] Sample post keys: "
                        + string.Join(",", ((JObject)posts[0]).Properties().Select(pr => pr.Name)));
                } catch { }
            }

            return result;
        }
    }
}
