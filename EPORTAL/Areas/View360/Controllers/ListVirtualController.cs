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

        // Session-timeout guard. Showcase user-facing khong dung [Authorize] global
        // (FilterConfig chi co HandleErrorAttribute). Khi Forms ticket het han (timeout
        // 180' sliding) -> User.Identity het auth -> MyAuthentication.ID = 0 -> truoc day
        // hien trang RONG ("0 tour") thay vi ve Login. Check "da dang nhap chua" -> redirect
        // Logout/Login (auto dang xuat). Admin actions van check HasAdminPerm rieng.
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.User;
            if (user == null || user.Identity == null || !user.Identity.IsAuthenticated
                || MyAuthentication.ID == 0)
            {
                filterContext.Result = RedirectToAction("Logout", "Login", new { area = "" });
                return;
            }
            base.OnActionExecuting(filterContext);
        }

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

        private static string FeaturedImageContentType(string extension)
        {
            switch ((extension ?? "").ToLowerInvariant())
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".webp":
                    return "image/webp";
                default:
                    return null;
            }
        }

        private static bool IsValidFeaturedImage(byte[] data, string extension)
        {
            if (data == null || data.Length == 0) return false;
            var ext = (extension ?? "").ToLowerInvariant();
            if (ext == ".webp")
            {
                return data.Length >= 12
                    && data[0] == (byte)'R' && data[1] == (byte)'I'
                    && data[2] == (byte)'F' && data[3] == (byte)'F'
                    && data[8] == (byte)'W' && data[9] == (byte)'E'
                    && data[10] == (byte)'B' && data[11] == (byte)'P';
            }

            try
            {
                using (var stream = new System.IO.MemoryStream(data))
                using (var image = System.Drawing.Image.FromStream(
                    stream, validateImageData: true, useEmbeddedColorManagement: false))
                {
                    var format = image.RawFormat;
                    return ((ext == ".jpg" || ext == ".jpeg")
                            && format.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                           || (ext == ".png"
                               && format.Equals(System.Drawing.Imaging.ImageFormat.Png));
                }
            }
            catch
            {
                return false;
            }
        }

        private const string FEATURED_DIR = "~/Content/view360-featured/";

        // Lam sach 1 segment (collectionId / sceneUuid) de dung an toan trong ten thu muc/file.
        private static string SafeSeg(string s)
        {
            if (string.IsNullOrEmpty(s)) return "_";
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (var c in s) sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_');
            var r = sb.ToString();
            return r.Length > 80 ? r.Substring(0, 80) : r;
        }

        // Xoa file featured cu (guard: chi cho phep trong ~/Content/view360-featured/) - tranh rac dia.
        private void TryDeleteFeaturedFile(string relPath)
        {
            try
            {
                if (string.IsNullOrEmpty(relPath)
                    || !relPath.StartsWith(FEATURED_DIR, StringComparison.OrdinalIgnoreCase)) return;
                var root = System.IO.Path.GetFullPath(Server.MapPath(FEATURED_DIR));
                var full = System.IO.Path.GetFullPath(Server.MapPath(relPath));
                var prefix = root.TrimEnd(System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
                if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
                if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[FeaturedImage] delete old file err: " + ex.Message); }
        }

        private static void PopulateFeaturedImageUrls(Controller ctrl, string collectionId,
            IEnumerable<FeaturedScene> items)
        {
            if (ctrl == null || string.IsNullOrEmpty(collectionId) || items == null) return;
            foreach (var item in items)
            {
                if (item == null || !item.HasImage || string.IsNullOrEmpty(item.SceneUuid)) continue;
                // Serve qua action FeaturedImage (action doc FILE tren server hoac BLOB cu, tu set
                // content-type + cache 1 ngay) -> khong phu thuoc MIME map IIS (vd .webp), khong sua Web.config.
                item.ImageUrl = ctrl.Url.Action("FeaturedImage", "ListVirtual", new
                {
                    area = "View360",
                    collectionId = collectionId,
                    sceneUuid = item.SceneUuid,
                    v = item.ImageVersion
                });
            }
        }

        private bool TryReadLegacyFeaturedImage(string imagePath, out byte[] data,
            out string contentType, out string fileName)
        {
            data = null;
            contentType = null;
            fileName = null;
            if (string.IsNullOrEmpty(imagePath)
                || !imagePath.StartsWith("~/Content/view360-featured/", StringComparison.OrdinalIgnoreCase))
                return false;

            try
            {
                var root = System.IO.Path.GetFullPath(Server.MapPath("~/Content/view360-featured/"));
                var fullPath = System.IO.Path.GetFullPath(Server.MapPath(imagePath));
                var rootPrefix = root.TrimEnd(
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar;
                if (!fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
                    return false;

                var info = new System.IO.FileInfo(fullPath);
                if (!info.Exists || info.Length <= 0 || info.Length > FEATURED_UPLOAD_MAX_BYTES)
                    return false;

                var extension = info.Extension.ToLowerInvariant();
                contentType = FeaturedImageContentType(extension);
                if (string.IsNullOrEmpty(contentType)) return false;

                data = System.IO.File.ReadAllBytes(fullPath);
                if (!IsValidFeaturedImage(data, extension))
                {
                    data = null;
                    contentType = null;
                    return false;
                }
                fileName = info.Name;
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[FeaturedImage] Legacy image read err: " + ex.Message);
                return false;
            }
        }

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

            // id = 0 la sentinel "Tat ca tour" (link /Index/0). HasValue=true nhung KHONG phai
            // mot VirtualGroup that (IDGroup bat dau tu 1) -> phai hieu la "khong filter", neu khong
            // se loc IDGroup==0 va loai sach moi tour (bug "0 tour" du da co quyen).
            var res = (id.HasValue && id.Value != 0) ? all.Where(x => x.IDGroup == id.Value).ToList() : all;

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
            string coneColor, double? coneFanDeg, int? coneRadius, bool? showMinimap)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId)) return Json(new { ok = false, error = "missing collectionId" });
            var cfg = new TourConfig {
                PinSize = pinSize,
                PinColor = string.IsNullOrEmpty(pinColor) ? null : pinColor,
                SelectedColor = string.IsNullOrEmpty(selectedColor) ? null : selectedColor,
                ConeColor = string.IsNullOrEmpty(coneColor) ? null : coneColor,
                ConeFanDeg = coneFanDeg,
                ConeRadius = coneRadius,
                ShowMinimap = showMinimap   // null = default (hien); false = an cho nguoi xem
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
                PopulateFeaturedImageUrls(this, collectionId, saved);
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
                            ImageUrl  = f.ImageUrl,
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
                var featured = string.IsNullOrEmpty(cid)
                    ? new List<FeaturedScene>()
                    : SceneCalibrationStore.GetFeatured(cid);
                PopulateFeaturedImageUrls(this, cid, featured);
                ViewBag.FeaturedListJson = JsonForHtml.Serialize(featured);
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
            if (collectionId.Length > 50 || sceneUuid.Length > 100)
                return Json(new { ok = false, error = "collectionId/sceneUuid too long" });
            var file = Request.Files != null && Request.Files.Count > 0 ? Request.Files[0] : null;
            if (file == null || file.ContentLength <= 0)
                return Json(new { ok = false, error = "no file" });
            if (file.ContentLength > FEATURED_UPLOAD_MAX_BYTES)
                return Json(new { ok = false, error = "file > " + (FEATURED_UPLOAD_MAX_BYTES / 1024 / 1024) + "MB" });
            var ext = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (Array.IndexOf(allowed, ext) < 0)
                return Json(new { ok = false, error = "unsupported ext" });

            byte[] imageData;
            try
            {
                using (var buffer = new System.IO.MemoryStream())
                {
                    file.InputStream.CopyTo(buffer);
                    imageData = buffer.ToArray();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[UploadFeaturedImage] Read err: " + ex.Message);
                return Json(new { ok = false, error = "cannot read file" });
            }

            if (imageData.Length == 0 || imageData.Length > FEATURED_UPLOAD_MAX_BYTES)
                return Json(new { ok = false, error = "invalid file size" });
            if (!IsValidFeaturedImage(imageData, ext))
                return Json(new { ok = false, error = "file khong phai anh hop le hoac khong khop extension" });

            var fileName = System.IO.Path.GetFileName(file.FileName);
            if (string.IsNullOrEmpty(fileName)) fileName = "featured" + ext;
            if (fileName.Length > 255) fileName = fileName.Substring(fileName.Length - 255);

            // RULE: luu ANH thanh FILE tren server, DB chi giu DUONG DAN (khong luu BLOB vao DB).
            var relDir  = FEATURED_DIR + SafeSeg(collectionId);
            var storedName = SafeSeg(sceneUuid) + "_" + DateTime.UtcNow.Ticks + ext;
            var relPath = relDir + "/" + storedName;
            try
            {
                var absDir = Server.MapPath(relDir);
                if (!System.IO.Directory.Exists(absDir)) System.IO.Directory.CreateDirectory(absDir);
                System.IO.File.WriteAllBytes(Server.MapPath(relPath), imageData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[UploadFeaturedImage] write file err: " + ex.Message);
                return Json(new { ok = false, error = "Không ghi được file ảnh lên server (kiểm tra quyền ghi thư mục Content/view360-featured)" });
            }

            // Xoa file cu (neu lan truoc cung luu file) -> tranh rac.
            var oldImg = SceneCalibrationStore.GetFeaturedImage(collectionId, sceneUuid);
            if (oldImg != null && !string.IsNullOrEmpty(oldImg.LegacyImagePath)
                && !oldImg.LegacyImagePath.Equals(relPath, StringComparison.OrdinalIgnoreCase))
                TryDeleteFeaturedFile(oldImg.LegacyImagePath);

            if (!SceneCalibrationStore.SaveFeaturedImagePath(collectionId, sceneUuid, relPath, fileName))
            {
                TryDeleteFeaturedFile(relPath);   // rollback file vua ghi
                return Json(new { ok = false, error = "DB save failed" });
            }

            var imageUrl = Url.Action("FeaturedImage", "ListVirtual", new
            {
                area = "View360",
                collectionId = collectionId,
                sceneUuid = sceneUuid,
                v = DateTime.UtcNow.Ticks
            });
            return Json(new { ok = true, url = imageUrl, hasImage = true });
        }

        [HttpGet]
        public ActionResult FeaturedImage(string collectionId, string sceneUuid)
        {
            if (string.IsNullOrEmpty(collectionId) || collectionId.Length > 50
                || string.IsNullOrEmpty(sceneUuid) || sceneUuid.Length > 100)
                return HttpNotFound();

            var image = SceneCalibrationStore.GetFeaturedImage(collectionId, sceneUuid);
            if (image == null || string.IsNullOrEmpty(image.LegacyImagePath)) return HttpNotFound();

            // RULE: anh luu FILE tren server -> doc file tu duong dan (ImagePath) roi serve. KHONG co BLOB trong DB.
            byte[] data;
            string contentType;
            string fileName;
            if (!TryReadLegacyFeaturedImage(image.LegacyImagePath, out data, out contentType, out fileName))
                return HttpNotFound();
            if (string.IsNullOrEmpty(contentType)) contentType = "application/octet-stream";

            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetMaxAge(TimeSpan.FromDays(1));
            Response.Cache.SetSlidingExpiration(false);
            return File(data, contentType);
        }

    }

    public class FeaturedSceneCard
    {
        public string SceneUuid { get; set; }
        public string Title     { get; set; }
        public string ImageUrl  { get; set; }
        public string SceneHash { get; set; }
    }

    public class FeaturedSceneOption
    {
        public string Uuid  { get; set; }
        public string Title { get; set; }
    }
}
