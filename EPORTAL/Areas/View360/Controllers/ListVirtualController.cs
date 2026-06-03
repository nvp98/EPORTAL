using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    public class ListVirtualController : Controller
    {
        // GET: View360/ListVirtual
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = MyAuthentication.IDQuyenHT;
        const string AdminPermKey = "Projects";  // re-use admin permission key

        // Gate admin actions (Calibrate/Featured/SaveCalibration/...) - mat dam bao
        // chi user co quyen "Projects" moi mutate config cua tour.
        private bool HasAdminPerm(string action)
        {
            try { return dbP.A_CheckQuyen(IDQuyenHT, AdminPermKey, action).First() != 0; }
            catch { return false; }
        }

        // ===== Tunable constants =====
        private const int PAGE_SIZE_MIN = 6;
        private const int PAGE_SIZE_MAX = 60;
        private const int PAGE_SIZE_DEFAULT = 10;
        private const int FEATURED_UPLOAD_MAX_BYTES = 8 * 1024 * 1024;  // 8MB

        // KHONG cache HTML output (phan quyen phuc tap, risk bleed neu VaryByCustom soti).
        // Speedup chu yeu dua vao Session cache SP `Virtual_select_USER` (per-user inherent).
        public ActionResult Index(int? page, string search, int? id, int? ps)
        {
            if (search == null) search = "";
            ViewBag.search = search;
            int pageSize = (ps.HasValue && ps.Value >= PAGE_SIZE_MIN && ps.Value <= PAGE_SIZE_MAX) ? ps.Value : PAGE_SIZE_DEFAULT;
            ViewBag.PageSize = pageSize;

            // Cache tabs + virtual list per user trong Session 120s -> tranh re-execute SP
            // khi user navigate giua cac Index/{groupId} khac nhau.
            var sessKey = "v360_virt_idx_" + MyAuthentication.ID + "_" + (search ?? "");
            var sess = System.Web.HttpContext.Current?.Session;
            var cached = sess != null ? sess[sessKey] as Tuple<DateTime, List<TabGroupViewModel>, List<VirtualValidation>> : null;
            List<TabGroupViewModel> tabGroups;
            List<VirtualValidation> all;
            if (cached != null && (DateTime.UtcNow - cached.Item1).TotalSeconds < 120)
            {
                tabGroups = cached.Item2;
                all = cached.Item3;
            }
            else
            {
                var userGroupIds = db.Get_IDGroupVirtual(MyAuthentication.ID)
                    .Select(g => g.IDGroup).Where(gid => gid.HasValue).Select(gid => gid.Value).Distinct().ToList();
                tabGroups = db.VirtualGroups
                    .Where(g => userGroupIds.Contains(g.IDGroup))
                    .OrderBy(g => g.GroupName)
                    .Select(g => new { g.IDGroup, g.GroupName }).ToList()
                    .Select(g => new TabGroupViewModel { IDGroup = g.IDGroup, GroupName = g.GroupName })
                    .ToList();
                all = db.Virtual_select_USER(search, MyAuthentication.ID)
                    .OrderByDescending(a => a.Date)
                    .Select(a => new VirtualValidation
                    {
                        ID = a.ID, Title = a.Title, Images = a.Images, URL = a.URL,
                        Date = (DateTime)a.Date, Note = a.Note, FilePDF = a.FilePDF,
                        IDPhongBan = (int)a.IDPhongBan, IDGroup = a.IDGroup ?? 0
                    })
                    .ToList();
                if (sess != null) sess[sessKey] = Tuple.Create(DateTime.UtcNow, tabGroups, all);
            }
            // Set Active cho tab theo id hien tai (xu ly ngoai cache de moi id reflect dung)
            foreach (var t in tabGroups) t.Active = (id ?? 0) == t.IDGroup;
            ViewBag.TabGroups = tabGroups;

            var res = id.HasValue ? all.Where(x => x.IDGroup == id.Value).ToList() : all;

            int pageNumber = page ?? 1;
            return View(res.ToPagedList(pageNumber, pageSize));
        }
        // Details view la full viewer + minimap, content khong doi giua user.
        // Cache 5 phut per id - dodge SP lookup + layout queries.
        // 'scene' query param: hash de jump direct den scene cua Kuula tour
        //   vd ?scene=p%3D6 -> iframe URL append '#p=6' -> Kuula tu load scene index 6
        // Dung tu zone cards trong Intro page de "click la vao thang khu vuc do".
        // Admin list: tours co the calibrate (link tu menu)
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult CalibrateIndex()
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            var tours = db.Virtuals
                .OrderByDescending(a => a.Date)
                .Select(a => new VirtualValidation {
                    ID = a.ID,
                    Title = a.Title,
                    Images = a.Images,
                    URL = a.URL,
                    Date = (DateTime)a.Date
                })
                .ToList();
            // Inject so scene da calibrate per tour (cho hien thi tren list)
            var savedCounts = new Dictionary<int, int>();
            foreach (var t in tours)
            {
                var cid = KuulaCollectionFetcher.ExtractCollectionId(t.URL);
                if (string.IsNullOrEmpty(cid)) { savedCounts[t.ID] = 0; continue; }
                savedCounts[t.ID] = SceneCalibrationStore.Get(cid).Count;
            }
            ViewBag.SavedCounts = savedCounts;
            return View(tours);
        }

        // Admin page: same view nhung co panel cau hinh, khong cache
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Calibrate(int id, string scene = null)
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            ViewBag.AdminMode = true;
            return DetailsView(id, scene);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveCalibration(string collectionId, string uuid,
            double? offset, bool hidePin = false, string customTitle = null)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(uuid))
                return Json(new { ok = false, error = "missing collectionId or uuid" });

            var calib = new SceneCalibration {
                Offset = offset, HidePin = hidePin,
                CustomTitle = string.IsNullOrEmpty(customTitle) ? null : customTitle.Trim()
            };
            if (!SceneCalibrationStore.Save(collectionId, uuid, calib))
                return Json(new { ok = false, error = "DB save failed (check Debug Output / SQL connection)" });

            // Invalidate OutputCache cua Details cho moi tour cung collection.
            // Nguoc lai users thuong se thay HTML cu (SCENE_HEADING_OFFSET cu) toi 30p.
            try
            {
                var tourIds = db.Virtuals
                    .Where(v => v.URL != null && v.URL.Contains(collectionId))
                    .Select(v => v.ID)
                    .ToList();
                foreach (var tid in tourIds)
                {
                    var path = Url.Action("Details", "ListVirtual",
                        new { area = "View360", id = tid });
                    if (!string.IsNullOrEmpty(path))
                        Response.RemoveOutputCacheItem(path);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[SaveCalibration] cache invalidate err: " + ex.Message);
            }

            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTourConfig(string collectionId,
            int? pinSize, string pinColor, string selectedColor,
            string coneColor, double? coneFanDeg, int? coneRadius)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId)) return Json(new { ok = false, error = "missing collectionId" });
            var cfg = new TourConfig {
                PinSize = pinSize,
                PinColor = string.IsNullOrEmpty(pinColor) ? null : pinColor,
                SelectedColor = string.IsNullOrEmpty(selectedColor) ? null : selectedColor,
                ConeColor = string.IsNullOrEmpty(coneColor) ? null : coneColor,
                ConeFanDeg = coneFanDeg,
                ConeRadius = coneRadius
            };
            if (!SceneCalibrationStore.SaveTourConfig(collectionId, cfg))
                return Json(new { ok = false, error = "DB save failed (check Debug Output / SQL connection)" });

            // Invalidate Details cache (HTML embed CSS vars)
            try
            {
                var tourIds = db.Virtuals
                    .Where(v => v.URL != null && v.URL.Contains(collectionId))
                    .Select(v => v.ID).ToList();
                foreach (var tid in tourIds)
                {
                    var p = Url.Action("Details", "ListVirtual", new { area = "View360", id = tid });
                    if (!string.IsNullOrEmpty(p)) Response.RemoveOutputCacheItem(p);
                }
            }
            catch { }
            return Json(new { ok = true });
        }

        public ActionResult Details(int id, string scene = null)
        {
            View360AccessTracker.Log(MyAuthentication.ID,
                View360AccessTracker.ContentType.Virtual, id,
                Session != null ? Session.SessionID : null);

            ViewBag.AdminMode = false;
            return DetailsView(id, scene);
        }

        private ActionResult DetailsView(int id, string scene)
        {
            var DO = PopulateDetailsViewBag(this, db, id, scene);
            if (DO == null) return HttpNotFound();
            return View("Details", DO);
        }

        /// <summary>
        /// Build model + populate tat ca ViewBag entries ma Details.cshtml can.
        /// Public static de cac controller khac (ChatbotController.Configure...) reuse
        /// thay vi duplicate ~80 dong setup. Tra null neu khong tim thay Virtual.
        /// </summary>
        public static VirtualValidation PopulateDetailsViewBag(Controller ctrl, EPORTALEntities db, int id, string scene)
        {
            ctrl.ViewBag.SceneHash = scene;
            var res = (from a in db.Virtuals.Where(x => x.ID == id)
                       select new VirtualValidation
                       {
                           ID = a.ID,
                           URL = a.URL,
                           Date = (DateTime?)a.Date ?? DateTime.Now,
                           Note = a.Note,
                           IDPhongBan = (int)a.IDPhongBan,
                           FilePDF = a.FilePDF,
                           Images = a.Images,
                           Title = a.Title
                       }).ToList();
            if (res.Count == 0) return null;
            VirtualValidation DO = new VirtualValidation();
            foreach (var a in res)
            {
                DO.ID = a.ID;
                DO.URL = a.URL;
                DO.Date = a.Date;
                DO.IDPhongBan = a.IDPhongBan;
                DO.FilePDF = a.FilePDF;
                DO.Images = a.Images;
                DO.Title = a.Title;
                DO.Note = a.Note;
            }

            // Inject GPS data per scene tu Kuula share page (cached 24h server-side).
            // Khong can API token - data nam trong window.KUULA_COLLECTION.data base64.
            var collectionId = KuulaCollectionFetcher.ExtractCollectionId(DO.URL);
            if (!string.IsNullOrEmpty(collectionId))
            {
                var scenes = KuulaCollectionFetcher.GetScenes(collectionId);

                // Deep-link: neu URL co ?scene=<uuid>, resolve sang Kuula's short Id de
                // iframe load THANG scene do thay vi load default roi postMessage switch
                // (cach cu gay WebGL CONTEXT_LOST khi rapid switch).
                if (!string.IsNullOrEmpty(scene))
                {
                    var target = scenes.FirstOrDefault(s =>
                        string.Equals(s.Uuid, scene, StringComparison.OrdinalIgnoreCase));
                    if (target != null && !string.IsNullOrEmpty(target.Id))
                        ctrl.ViewBag.IframeStartSceneId = target.Id;
                }

                // Key by Uuid (stable cross-context). Kuula dung Id khac nhau giua share data
                // va iframe postMessage runtime - chi Uuid match duoc giua 2.
                var gpsMap = scenes
                    .Where(s => s.Lat.HasValue && s.Lng.HasValue && !string.IsNullOrEmpty(s.Uuid))
                    .GroupBy(s => s.Uuid).Select(g => g.First())
                    .ToDictionary(s => s.Uuid, s => new[] { s.Lat.Value, s.Lng.Value });
                ctrl.ViewBag.SceneGpsJson = JsonForHtml.Serialize(gpsMap);
                ctrl.ViewBag.SceneCount = scenes.Count;
                ctrl.ViewBag.SceneGpsCount = gpsMap.Count;

                // Calibration cung key by Uuid de match client lookup
                var calib = SceneHeadingCalibrator.Compute(scenes, gpsMap);

                // Merge DB calibration override (Offset + HidePin + CustomTitle)
                var dbCalib = SceneCalibrationStore.Get(collectionId);
                var hidePinMap   = new Dictionary<string, bool>();
                var customTitles = new Dictionary<string, string>();
                foreach (var kv in dbCalib)
                {
                    if (kv.Value.Offset.HasValue) calib.Offsets[kv.Key] = kv.Value.Offset.Value;
                    if (kv.Value.HidePin) hidePinMap[kv.Key] = true;
                    if (!string.IsNullOrEmpty(kv.Value.CustomTitle)) customTitles[kv.Key] = kv.Value.CustomTitle;
                }

                ctrl.ViewBag.CollectionId = collectionId;
                ctrl.ViewBag.SceneHeadingOffsetJson = JsonForHtml.Serialize(calib.Offsets);
                ctrl.ViewBag.SceneCalibDiagJson = JsonForHtml.Serialize(calib.Diagnostics);
                ctrl.ViewBag.SceneHidePinJson = JsonForHtml.Serialize(hidePinMap);
                ctrl.ViewBag.SceneCustomTitleJson = JsonForHtml.Serialize(customTitles);
                ctrl.ViewBag.SceneDbCalibJson = JsonForHtml.Serialize(dbCalib);
                ctrl.ViewBag.TourConfigJson = JsonForHtml.Serialize(
                    SceneCalibrationStore.GetTourConfig(collectionId));
                ctrl.ViewBag.SceneCalibratedCount = calib.Offsets.Count;

                // Chatbot enabled? -> set flag de Details.cshtml include widget CSS/JS.
                var chatbotInfo = ChatbotContentStore.GetTourInfo(collectionId);
                ctrl.ViewBag.ChatbotEnabled = chatbotInfo != null && chatbotInfo.IsEnabled;
            }
            else
            {
                ctrl.ViewBag.SceneGpsJson = "{}";
                ctrl.ViewBag.SceneHeadingOffsetJson = "{}";
                ctrl.ViewBag.SceneCalibDiagJson = "{}";
                ctrl.ViewBag.SceneHidePinJson = "{}";
                ctrl.ViewBag.SceneCustomTitleJson = "{}";
                ctrl.ViewBag.SceneDbCalibJson = "{}";
                ctrl.ViewBag.TourConfigJson = "{}";
                ctrl.ViewBag.CollectionId = "";
            }

            // KMZ drone imagery overlay manifest (LOD a+b+c, ~4.5 MB).
            // Manifest sinh ra tu doc.kml luc deploy, gom 16 tile boundedbox kem path.
            try
            {
                var manifestPath = ctrl.Server.MapPath("~/Content/view360-imagery/manifest.json");
                if (System.IO.File.Exists(manifestPath))
                {
                    ctrl.ViewBag.KmzManifestJson = System.IO.File.ReadAllText(manifestPath);
                }
                else
                {
                    ctrl.ViewBag.KmzManifestJson = "[]";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[View360] KMZ manifest load error: " + ex.Message);
                ctrl.ViewBag.KmzManifestJson = "[]";
            }

            return DO;
        }

        public ActionResult Intro(int id)
        {
            var res = (from a in db.Virtuals.Where(x => x.ID == id)
                       select new VirtualValidation
                       {
                           ID = a.ID,
                           URL = a.URL,
                           Date = (DateTime?)a.Date ?? DateTime.Now,
                           Note = a.Note,
                           IDPhongBan = (int)a.IDPhongBan,
                           FilePDF = a.FilePDF,
                           Images = a.Images,
                           Title = a.Title
                       }).ToList();
            VirtualValidation DO = new VirtualValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.ID = a.ID;
                    DO.URL = a.URL;
                    DO.Date = a.Date;
                    DO.IDPhongBan = a.IDPhongBan;
                    DO.FilePDF = a.FilePDF;
                    DO.Images = a.Images;
                    DO.Title = a.Title;
                    DO.Note = a.Note;
                }
            }
            else
            {
                HttpNotFound();
            }

            // Featured scenes (Khu vuc tham quan chinh) - render cards o duoi HUONG DAN SU DUNG.
            // Mix data: row trong V360_FeaturedScene + Kuula scene title fallback khi admin
            // chua override.
            var featuredCards = new List<FeaturedSceneCard>();
            var collectionId = KuulaCollectionFetcher.ExtractCollectionId(DO.URL);
            if (!string.IsNullOrEmpty(collectionId))
            {
                var saved = SceneCalibrationStore.GetFeatured(collectionId);
                if (saved.Count > 0)
                {
                    var scenes = KuulaCollectionFetcher.GetScenes(collectionId);
                    var byUuid = scenes
                        .Where(s => !string.IsNullOrEmpty(s.Uuid))
                        .GroupBy(s => s.Uuid).ToDictionary(g => g.Key, g => g.First());
                    // Title priority: calibration.CustomTitle (chung voi tooltip minimap)
                    //                 -> Kuula scene.Title -> "Khu vuc".
                    var calib = SceneCalibrationStore.Get(collectionId);
                    foreach (var f in saved)
                    {
                        KuulaCollectionFetcher.SceneInfo sc;
                        byUuid.TryGetValue(f.SceneUuid, out sc);
                        string title = null;
                        SceneCalibration cal;
                        if (calib.TryGetValue(f.SceneUuid, out cal) && !string.IsNullOrEmpty(cal.CustomTitle))
                            title = cal.CustomTitle;
                        if (string.IsNullOrEmpty(title) && sc != null && !string.IsNullOrEmpty(sc.Title))
                            title = sc.Title;
                        if (string.IsNullOrEmpty(title)) title = "Khu vực";

                        // Deep-link: pass scene uuid - Details.cshtml JS se postMessage('load')
                        // sau khi iframe ban tin 'frameloaded' (giong minimap pin click).
                        // Kuula iframe khong honor URL hash (#p=, #id=) cho collection embed.
                        featuredCards.Add(new FeaturedSceneCard
                        {
                            SceneUuid = f.SceneUuid,
                            Title     = title,
                            ImagePath = f.ImagePath,
                            SceneHash = f.SceneUuid
                        });
                    }
                }
            }
            ViewBag.FeaturedCards = featuredCards;
            ViewBag.CollectionId  = collectionId;

            return View(DO);
        }

        // ===== Featured scenes admin =====
        public ActionResult FeaturedIndex()
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            var tours = db.Virtuals
                .OrderByDescending(a => a.Date)
                .Select(a => new VirtualValidation {
                    ID = a.ID, Title = a.Title, Images = a.Images, URL = a.URL,
                    Date = (DateTime)a.Date
                }).ToList();
            var savedCounts = new Dictionary<int, int>();
            foreach (var t in tours)
            {
                var cid = KuulaCollectionFetcher.ExtractCollectionId(t.URL);
                savedCounts[t.ID] = string.IsNullOrEmpty(cid) ? 0 : SceneCalibrationStore.GetFeatured(cid).Count;
            }
            ViewBag.SavedCounts = savedCounts;
            return View(tours);
        }

        // Featured admin: dung Details viewer (iframe Kuula + Leaflet minimap) de admin co the
        // navigate scene, click pin tren map, va add scene hien tai vao list featured. Title cua
        // diem dung chung voi calibration store (V360_SceneCalibration.CustomTitle) - single
        // source of truth, edit o day cung apply tooltip minimap.
        public ActionResult Featured(int id, string scene = null)
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            ViewBag.AdminMode         = false;
            ViewBag.FeaturedAdminMode = true;

            // Load featured list trong cung action de view co data san khi render
            var tour = db.Virtuals.FirstOrDefault(x => x.ID == id);
            if (tour != null)
            {
                var cid = KuulaCollectionFetcher.ExtractCollectionId(tour.URL);
                ViewBag.FeaturedListJson = JsonForHtml.Serialize(
                    string.IsNullOrEmpty(cid) ? new List<FeaturedScene>()
                                              : SceneCalibrationStore.GetFeatured(cid));
            }
            else
            {
                ViewBag.FeaturedListJson = "[]";
            }
            return DetailsView(id, scene);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveFeatured(string collectionId, string itemsJson)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId))
                return Json(new { ok = false, error = "missing collectionId" });
            try
            {
                var items = Newtonsoft.Json.JsonConvert
                    .DeserializeObject<List<FeaturedScene>>(itemsJson ?? "[]")
                    ?? new List<FeaturedScene>();
                if (!SceneCalibrationStore.SaveFeatured(collectionId, items))
                    return Json(new { ok = false, error = "DB save failed" });
                return Json(new { ok = true, count = items.Count });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadFeaturedImage(string collectionId, string sceneUuid)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(sceneUuid))
                return Json(new { ok = false, error = "missing collectionId/sceneUuid" });
            var file = Request.Files != null && Request.Files.Count > 0 ? Request.Files[0] : null;
            if (file == null || file.ContentLength <= 0)
                return Json(new { ok = false, error = "no file" });
            if (file.ContentLength > FEATURED_UPLOAD_MAX_BYTES)
                return Json(new { ok = false, error = "file > " + (FEATURED_UPLOAD_MAX_BYTES / 1024 / 1024) + "MB" });
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (Array.IndexOf(allowed, ext) < 0)
                return Json(new { ok = false, error = "unsupported ext" });

            // MIME validation: decode anh thuc su (tranh upload file evil.jpg chua HTML/PHP).
            // System.Drawing.Image.FromStream throw neu khong phai image hop le.
            try
            {
                using (var probe = file.InputStream)
                {
                    var pos = probe.Position;
                    using (var img = System.Drawing.Image.FromStream(probe, validateImageData: false, useEmbeddedColorManagement: false))
                    {
                        // Match ext voi format thuc su (vd: jpg ext nhung PNG content -> reject)
                        var fmt = img.RawFormat;
                        bool match =
                            (ext == ".jpg" || ext == ".jpeg") && fmt.Equals(System.Drawing.Imaging.ImageFormat.Jpeg)
                            || (ext == ".png") && fmt.Equals(System.Drawing.Imaging.ImageFormat.Png)
                            || (ext == ".webp"); // System.Drawing khong nhan diện duoc webp -> skip check, du dua tren ext
                        if (!match && ext != ".webp")
                            return Json(new { ok = false, error = "file content khong khop extension" });
                    }
                    probe.Position = pos;  // rewind cho SaveAs sau day
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[UploadFeaturedImage] MIME check err: " + ex.Message);
                return Json(new { ok = false, error = "file khong phai anh hop le" });
            }

            // Sanitize uuid - chi cho a-z0-9 (tranh path traversal)
            var safeUuid = System.Text.RegularExpressions.Regex.Replace(sceneUuid, "[^A-Za-z0-9]", "");
            var safeCid  = System.Text.RegularExpressions.Regex.Replace(collectionId, "[^A-Za-z0-9]", "");
            if (string.IsNullOrEmpty(safeUuid) || string.IsNullOrEmpty(safeCid))
                return Json(new { ok = false, error = "invalid id chars" });

            var dir = Server.MapPath("~/Content/view360-featured/" + safeCid);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            var fileName = safeUuid + "_" + DateTime.UtcNow.Ticks + ext;
            var fullPath = System.IO.Path.Combine(dir, fileName);
            file.SaveAs(fullPath);
            var rel = "~/Content/view360-featured/" + safeCid + "/" + fileName;
            return Json(new { ok = true, path = rel, url = Url.Content(rel) });
        }

    }

    public class FeaturedSceneCard
    {
        public string SceneUuid { get; set; }
        public string Title     { get; set; }
        public string ImagePath { get; set; }
        public string SceneHash { get; set; }
    }

    public class FeaturedSceneOption
    {
        public string Uuid  { get; set; }
        public string Title { get; set; }
    }
}