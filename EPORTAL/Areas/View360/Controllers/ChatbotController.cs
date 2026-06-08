using EPORTAL.Common;
using EPORTAL.Models;
using EPORTAL.ModelsView360;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPORTAL.Areas.View360.Controllers
{
    /// <summary>
    /// Admin: cau hinh giai thieu tour + tung scene cho AI (Configure/SaveTourInfo/SaveSceneInfo).
    /// User: endpoint /Ask/Realtime/Speak/Transcribe de chatbot widget tren Details goi.
    ///
    /// Admin actions check permission "Projects" (re-use). User actions check IsAuthenticated +
    /// rate limit (RateLimitPerHour) chong abuse.
    /// </summary>
    public class ChatbotController : Controller
    {
        EPORTALEntities db = new EPORTALEntities();
        PhanQuyenHTEntities dbP = new PhanQuyenHTEntities();
        int IDQuyenHT = MyAuthentication.IDQuyenHT;
        const string AdminPermKey = "Projects";

        // ===== Tunable constants =====
        private const int CACHE_MIN = 5;                  // MemoryCache TTL cho tour/scene info
        private const int MAX_MESSAGE_LEN = 500;          // user message length cap (Ask/AskStream)
        private const int MAX_AUDIO_BYTES = 10 * 1024 * 1024;   // 10MB cap cho STT upload + debug
        private const int TTS_SYNC_MAX_CHARS = 300;       // VBee sync TTS character limit
        private const int TTS_ASYNC_MAX_CHARS = 10000;    // VBee async TTS character limit
        private const int CHAT_MAX_HISTORY_TURNS = 10;    // bounded conversation history

        private bool HasAdminPerm(string action)
        {
            try { return dbP.A_CheckQuyen(IDQuyenHT, AdminPermKey, action).First() != 0; }
            catch { return false; }
        }

        // Static HttpClient: tranh socket exhaustion (new HttpClient moi request -> leak TIME_WAIT
        // socket, sau ~28k request port exhaustion). Timeout per-call qua CancellationTokenSource.
        private static readonly System.Net.Http.HttpClient _http = new System.Net.Http.HttpClient
        {
            Timeout = System.Threading.Timeout.InfiniteTimeSpan
        };

        // Rate limit per-user per-endpoint, bucketed by UTC hour (no DB hit).
        // Returns true if exceeded. Increments counter on first non-blocked call.
        // Cac endpoint dot tien (OpenAI Realtime / VBee TTS / VBee STT) phai goi de tranh abuse.
        private static bool IsRateLimited(string endpoint, int userId, int limitPerHour)
        {
            var hourBucket = DateTime.UtcNow.ToString("yyyyMMddHH");
            var key = "chatbot_rate_" + endpoint + "_" + userId + "_" + hourBucket;
            var current = MemoryCache.Default.Get(key) as int? ?? 0;
            if (current >= limitPerHour) return true;
            MemoryCache.Default.Set(key, current + 1, DateTimeOffset.UtcNow.AddMinutes(70));
            return false;
        }

        // Tran TOKEN/NGAY (reset 0h gio server) - chong dot quota OpenAI khi treo/loi/abuse.
        // Tong token lay tu bang log (song sot qua IIS recycle, khac MemoryCache theo gio).
        //   Chatbot.DailyTokensPerUser  : tran moi user/ngay (mac dinh 1.000.000; <=0 = tat).
        //   Chatbot.DailyTokensGlobal   : tran toan he thong/ngay (mac dinh 0 = tat).
        // Tra ve thong bao loi neu vuot, null neu con han muc. Soft-cap: cau lam vuot nguong van chay xong.
        private static string CheckDailyTokenCap(int userId)
        {
            long perUser = ChatbotConfig.GetLong("CHATBOT_DAILY_TOKENS_PER_USER", "Chatbot.DailyTokensPerUser", 1000000L);
            if (perUser > 0 && userId > 0 && ChatbotContentStore.SumTokensToday(userId) >= perUser)
                return "Bạn đã dùng hết hạn mức trò chuyện AI trong ngày. Vui lòng thử lại vào ngày mai.";

            long global = ChatbotConfig.GetLong("CHATBOT_DAILY_TOKENS_GLOBAL", "Chatbot.DailyTokensGlobal", 0L);
            if (global > 0 && ChatbotContentStore.SumTokensToday(null) >= global)
                return "Hệ thống đã đạt giới hạn sử dụng AI trong ngày. Vui lòng thử lại sau.";

            return null;
        }

        // ===== Cache layer for tour info / scene info (5 min) =====
        // Giam DB load khi co nhieu user hoi cung 1 tour.
        private static T GetCached<T>(string key, Func<T> loader) where T : class
        {
            var cached = MemoryCache.Default.Get(key) as T;
            if (cached != null) return cached;
            var fresh = loader();
            if (fresh != null)
                MemoryCache.Default.Set(key, fresh, DateTimeOffset.UtcNow.AddMinutes(CACHE_MIN));
            return fresh;
        }
        private static void InvalidateCache(string collectionId)
        {
            MemoryCache.Default.Remove("chatbot_tour_" + collectionId);
            MemoryCache.Default.Remove("chatbot_scenes_" + collectionId);
        }

        // ==================================================================
        //   ADMIN
        // ==================================================================

        // GET: View360/Chatbot - list tour de chon configure
        public ActionResult Index()
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            var tours = db.Virtuals
                .OrderByDescending(a => a.Date)
                .Select(a => new VirtualValidation {
                    ID = a.ID, Title = a.Title, Images = a.Images, URL = a.URL,
                    Date = (DateTime)a.Date
                }).ToList();

            // Inject "X/Y scenes da cau hinh" badge per tour
            var counts = new Dictionary<int, Tuple<int, int>>(); // tourId -> (configured, totalScenesWithGps)
            foreach (var t in tours)
            {
                var cid = KuulaCollectionFetcher.ExtractCollectionId(t.URL);
                if (string.IsNullOrEmpty(cid)) { counts[t.ID] = Tuple.Create(0, 0); continue; }
                var configured = ChatbotContentStore.GetConfiguredSceneCount(cid);
                var total = KuulaCollectionFetcher.GetScenes(cid)?.Count(s => s.Lat.HasValue) ?? 0;
                counts[t.ID] = Tuple.Create(configured, total);
            }
            ViewBag.ChatbotCounts = counts;
            return View(tours);
        }

        // GET: View360/Chatbot/Configure/{id}
        // Reuse ListVirtual/Details.cshtml (iframe Kuula + Leaflet minimap + sync da co)
        // qua flag ChatbotAdminMode, giong cach Calibrate dung AdminMode. Panel chatbot
        // duoc render trong Details.cshtml o block @if (ChatbotAdminMode).
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Configure(int id, string scene = null)
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();

            ViewBag.ChatbotAdminMode = true;
            var model = ListVirtualController.PopulateDetailsViewBag(this, db, id, scene);
            if (model == null) return HttpNotFound("Tour khong ton tai");

            // Pull chatbot-specific data; CollectionId da duoc set boi PopulateDetailsViewBag
            var cid = (string)ViewBag.CollectionId;
            if (string.IsNullOrEmpty(cid))
                return Content("Tour khong phai Kuula collection - khong configure chatbot duoc.");

            var tourInfo = ChatbotContentStore.GetTourInfo(cid) ?? new ChatbotTourInfo { IsEnabled = true };
            var sceneInfo = ChatbotContentStore.GetSceneInfo(cid);
            ViewBag.ChatbotTourInfo = tourInfo;
            ViewBag.ChatbotSceneInfoJson = JsonForHtml.Serialize(sceneInfo);
            ViewBag.ChatbotConfiguredCount = ChatbotContentStore.GetConfiguredSceneCount(cid);

            return View("~/Areas/View360/Views/ListVirtual/Details.cshtml", model);
        }

        // POST: Save tour-level info
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveTourInfo(string collectionId, string overview, string systemPrompt, bool isEnabled)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId)) return Json(new { ok = false, error = "missing collectionId" });
            var info = new ChatbotTourInfo {
                Overview     = string.IsNullOrEmpty(overview) ? null : overview.Trim(),
                SystemPrompt = string.IsNullOrEmpty(systemPrompt) ? null : systemPrompt.Trim(),
                IsEnabled    = isEnabled
            };
            if (!ChatbotContentStore.SaveTourInfo(collectionId, info, MyAuthentication.ID))
                return Json(new { ok = false, error = "DB save failed" });
            InvalidateCache(collectionId);
            KnowledgeStore.MarkStale(collectionId);   // noi dung tour doi -> KB cu coi nhu het hieu luc
            return Json(new { ok = true });
        }

        // POST: Save per-scene info.
        // CustomTitle (Title hover) sync sang V360_SceneCalibration (cung dung cho hover minimap).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SaveSceneInfo(string collectionId, string uuid,
            string shortIntro, string detailContent, string customTitle)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(uuid))
                return Json(new { ok = false, error = "missing collectionId or uuid" });

            var info = new ChatbotSceneInfo {
                ShortIntro    = string.IsNullOrEmpty(shortIntro)    ? null : shortIntro.Trim(),
                DetailContent = string.IsNullOrEmpty(detailContent) ? null : detailContent.Trim()
            };
            if (!ChatbotContentStore.SaveSceneInfo(collectionId, uuid, info, MyAuthentication.ID))
                return Json(new { ok = false, error = "DB save failed" });

            // Sync CustomTitle sang V360_SceneCalibration. Preserve Offset + HidePin neu da co.
            var existingCalib = SceneCalibrationStore.Get(collectionId);
            SceneCalibration calib = null;
            if (existingCalib.TryGetValue(uuid, out var c)) calib = c;
            calib = calib ?? new SceneCalibration();
            calib.CustomTitle = string.IsNullOrEmpty(customTitle) ? null : customTitle.Trim();
            SceneCalibrationStore.Save(collectionId, uuid, calib);

            InvalidateCache(collectionId);
            KnowledgeStore.MarkStale(collectionId, uuid);   // noi dung scene doi -> KB lien quan het hieu luc
            return Json(new {
                ok = true,
                configuredCount = ChatbotContentStore.GetConfiguredSceneCount(collectionId)
            });
        }

        // ==================================================================
        //   ADMIN - KNOWLEDGE BASE (quan ly cau tra loi da cache)
        // ==================================================================

        // GET: View360/Chatbot/KnowledgeBase - trang quan ly KB
        public ActionResult KnowledgeBase(string collectionId = null, string status = "active", string q = null)
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            var rows = KnowledgeStore.ListForAdmin(collectionId, status, q);
            ViewBag.FilterCollection = collectionId;
            ViewBag.FilterStatus = status;
            ViewBag.FilterQ = q;
            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KbSetStatus(long id, string collectionId, string status)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            var ok = status == "active" || status == "disabled" || status == "pending" || status == "stale";
            if (!ok) return Json(new { ok = false, error = "status không hợp lệ" });
            KnowledgeStore.SetStatus(id, collectionId, status);
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KbDelete(long id, string collectionId)
        {
            if (!HasAdminPerm(A_Constants.DELETE)) return new HttpUnauthorizedResult();
            KnowledgeStore.Delete(id, collectionId);
            return Json(new { ok = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KbEdit(long id, string collectionId, string canonicalQuestion, string answerText, int qualityScore, string suggestions)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            if (string.IsNullOrWhiteSpace(answerText)) return Json(new { ok = false, error = "Câu trả lời trống" });
            KnowledgeStore.UpdateAnswer(id, collectionId, canonicalQuestion, answerText, qualityScore, suggestions);
            return Json(new { ok = true });
        }

        // GET: nghe audio cua 1 cau tra loi - ghep cac doan audio cache (theo tung cau) lai.
        [HttpGet]
        public ActionResult KbAudio(long id)
        {
            if (!HasAdminPerm(A_Constants.VIEW_ALL)) return new HttpUnauthorizedResult();
            var answer = KnowledgeStore.GetAnswerById(id);
            if (string.IsNullOrEmpty(answer)) return HttpNotFound();
            var hashes = AnswerAudioHashes(answer);
            if (hashes.Count == 0) return new HttpStatusCodeResult(404, "Không có nội dung đọc");
            var map = AudioCacheStore.GetMany(hashes);
            using (var ms = new System.IO.MemoryStream())
            {
                int found = 0;
                foreach (var h in hashes)
                    if (map.TryGetValue(h, out var bytes) && bytes != null) { ms.Write(bytes, 0, bytes.Length); found++; }
                if (found == 0) return new HttpStatusCodeResult(404, "Chưa có audio (mở chế độ Đọc 1 lần để tạo)");
                return new FileContentResult(ms.ToArray(), "audio/mpeg");
            }
        }

        // POST: xoa audio cache cua 1 cau tra loi -> lan sau o che do Doc se tao + cache lai.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult KbClearAudio(long id)
        {
            if (!HasAdminPerm(A_Constants.EDIT)) return new HttpUnauthorizedResult();
            var answer = KnowledgeStore.GetAnswerById(id);
            if (string.IsNullOrEmpty(answer)) return Json(new { ok = false, error = "Không có câu trả lời" });
            int n = AudioCacheStore.DeleteMany(AnswerAudioHashes(answer));
            return Json(new { ok = true, deleted = n });
        }

        // Tach 1 cau tra loi thanh cac hash audio cache (theo giong/toc do/maxChars HIEN TAI) - mirror
        // client TTS pipeline (popSentence + cleanTextForTts) de trung dung hash da cache khi user nghe.
        private static List<string> AnswerAudioHashes(string answer)
        {
            var voiceCode = GetChatbotCfg("CHATBOT_VBEE_VOICE", "Chatbot.VbeeVoice") ?? "hn_female_ngochuyen_full_48k-fhg";
            var speedStr  = GetChatbotCfg("CHATBOT_VBEE_SPEED", "Chatbot.VbeeSpeed") ?? "1.0";
            double speed  = double.TryParse(speedStr, System.Globalization.NumberStyles.Float,
                                            System.Globalization.CultureInfo.InvariantCulture, out var sp) ? sp : 1.0;
            var maxChars = GetVbeeTtsMaxChars(voiceCode);
            var hashes = new List<string>();
            foreach (var sent in SplitTtsSentences(answer))
            {
                var tts = CleanTextForTts(sent);
                if (string.IsNullOrEmpty(tts)) continue;
                if (maxChars > 0 && tts.Length > maxChars) tts = tts.Substring(0, maxChars);
                hashes.Add(AudioCacheStore.HashFor(voiceCode, speed, tts));
            }
            return hashes;
        }

        // Tach cau giong popSentence client: bien gioi [.!?…]+space hoac newline (giu dau cau).
        private static List<string> SplitTtsSentences(string text)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(text)) return list;
            int last = 0;
            var rx = new System.Text.RegularExpressions.Regex(@"[.!?…]\s|\n");
            foreach (System.Text.RegularExpressions.Match m in rx.Matches(text))
            {
                bool isNl = m.Value == "\n";
                int keepEnd = isNl ? m.Index : m.Index + 1;
                var s = text.Substring(last, keepEnd - last).Trim();
                if (s.Length > 0) list.Add(s);
                last = m.Index + m.Length;
            }
            if (last < text.Length) { var rest = text.Substring(last).Trim(); if (rest.Length > 0) list.Add(rest); }
            return list;
        }

        // Mirror cleanTextForTts client.
        private static string CleanTextForTts(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            s = System.Text.RegularExpressions.Regex.Replace(s, @"[*_`~]", "");
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\[.*?\]", "");
            s = System.Text.RegularExpressions.Regex.Replace(s, @"\([\s\S]*?\)", mm => mm.Value.Length < 60 ? mm.Value : "");
            return s.Trim();
        }

        // ==================================================================
        //   USER - ASK
        // ==================================================================

        // ==================================================================
        //   REALTIME VOICE - OpenAI Realtime API + WebRTC
        // ==================================================================
        // Mint ephemeral session token cho browser. Browser sau do dung token nay
        // de WebRTC truc tiep voi OpenAI - audio in/out direct, latency thap.
        // KHONG bao gio expose OPENAI_API_KEY ra browser.
        //
        // Refs:
        //   https://platform.openai.com/docs/guides/realtime
        //   https://platform.openai.com/docs/api-reference/realtime-sessions
        [HttpPost]
        [SameOriginOnly]
        public async Task<ActionResult> RealtimeSession()
        {
            if (!Request.IsAuthenticated) return new HttpUnauthorizedResult();

            // Rate limit: voice session dat ~$0.06/phut, mac dinh cap 10/h/user.
            var rtLimit = ChatbotConfig.GetInt("CHATBOT_REALTIME_SESSIONS_PER_HOUR", "Chatbot.RealtimeSessionsPerHour", 10);
            if (IsRateLimited("realtime", MyAuthentication.ID, rtLimit))
                return Json(new { ok = false, error = "Đã đạt giới hạn " + rtLimit + " phiên thoại/giờ. Vui lòng thử lại sau." });

            string raw;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
                raw = reader.ReadToEnd();
            JObject body;
            try { body = JObject.Parse(raw ?? "{}"); }
            catch { return Json(new { ok = false, error = "invalid JSON" }); }

            var collectionId = (string)body["collectionId"];
            var sceneUuid    = (string)body["sceneUuid"];
            if (string.IsNullOrEmpty(collectionId))
                return Json(new { ok = false, error = "missing collectionId" });

            // Load tour info + scene info de build instructions
            var tourInfo = ChatbotContentStore.GetTourInfo(collectionId);
            if (tourInfo == null || !tourInfo.IsEnabled)
                return Json(new { ok = false, error = "Chatbot chưa được kích hoạt cho tour này." });

            var sceneInfo = ChatbotContentStore.GetSceneInfo(collectionId);
            var allScenes = KuulaCollectionFetcher.GetScenes(collectionId) ?? new List<KuulaCollectionFetcher.SceneInfo>();
            var customTitles = SceneCalibrationStore.Get(collectionId)
                .Where(kv => !string.IsNullOrEmpty(kv.Value.CustomTitle))
                .ToDictionary(kv => kv.Key, kv => kv.Value.CustomTitle);

            // Build instructions cho voice (ngan hon text mode - voice can nhanh)
            var instructions = BuildVoiceInstructions(tourInfo, sceneInfo, allScenes, customTitles, sceneUuid);

            // Read config
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                return Json(new { ok = false, error = "OPENAI_API_KEY chua duoc cau hinh" });

            var model = ChatbotConfig.Get("CHATBOT_REALTIME_MODEL", "Chatbot.RealtimeModel", "gpt-realtime-2");
            var voice = ChatbotConfig.Get("CHATBOT_REALTIME_VOICE", "Chatbot.RealtimeVoice", "nova");
            var vadType = ChatbotConfig.Get("CHATBOT_REALTIME_VAD", "Chatbot.RealtimeVAD", "semantic_vad");

            // Endpoint MOI cho gpt-realtime-2: /v1/realtime/client_secrets voi nested session payload.
            // Endpoint cu /v1/realtime/sessions chi work voi gpt-4o-realtime-preview.
            // Ref: https://platform.openai.com/docs/api-reference/realtime-sessions
            var sessionPayload = new JObject {
                ["session"] = new JObject {
                    ["type"] = "realtime",
                    ["model"] = model,
                    ["instructions"] = instructions,
                    ["audio"] = new JObject {
                        ["input"] = new JObject {
                            ["format"] = new JObject {
                                ["type"] = "audio/pcm",
                                ["rate"] = 24000
                            },
                            ["transcription"] = new JObject {
                                ["model"] = "whisper-1"
                            },
                            ["turn_detection"] = new JObject {
                                ["type"] = vadType,
                                ["eagerness"] = "medium",
                                ["create_response"] = true,
                                ["interrupt_response"] = true
                            }
                        },
                        ["output"] = new JObject {
                            ["format"] = new JObject {
                                ["type"] = "audio/pcm",
                                ["rate"] = 24000
                            },
                            ["voice"] = voice
                        }
                    },
                    ["tools"] = new JArray {
                        new JObject {
                            ["type"] = "function",
                            ["name"] = "navigate_to_scene",
                            ["description"] = "Khi user yêu cầu di chuyển tới điểm/khu vực khác. Truyền `query` là từ khoá tiếng Việt user dùng.",
                            ["parameters"] = new JObject {
                                ["type"] = "object",
                                ["properties"] = new JObject {
                                    ["query"] = new JObject {
                                        ["type"] = "string",
                                        ["description"] = "Từ khoá tiếng Việt user dùng để chỉ điểm đích"
                                    }
                                },
                                ["required"] = new JArray { "query" }
                            }
                        }
                    },
                    ["tool_choice"] = "auto"
                }
            };

            try
            {
                using (var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(15)))
                using (var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post,
                                     "https://api.openai.com/v1/realtime/client_secrets"))
                {
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
                    req.Content = new System.Net.Http.StringContent(
                        sessionPayload.ToString(), System.Text.Encoding.UTF8, "application/json");
                    var resp = await _http.SendAsync(req, cts.Token);
                    var respBody = await resp.Content.ReadAsStringAsync();
                    if (!resp.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine("[RealtimeSession] OpenAI " + (int)resp.StatusCode + " " + respBody);
                        return Json(new { ok = false, error = "OpenAI " + (int)resp.StatusCode + ": " + respBody });
                    }
                    var j = JObject.Parse(respBody);
                    // Response moi: { value: "ek_...", expires_at: ..., session: {...} }
                    // Backward compat: cung check client_secret.value (endpoint cu)
                    var clientSecret = j["value"]?.ToString()
                                     ?? j["client_secret"]?["value"]?.ToString();
                    if (string.IsNullOrEmpty(clientSecret))
                        return Json(new { ok = false, error = "OpenAI trả về không có client_secret" });

                    // Log session start
                    try
                    {
                        ChatbotContentStore.LogMessage(new ChatbotMessageLog {
                            SessionGuid = Guid.NewGuid(),
                            NhanVienID = MyAuthentication.ID,
                            CollectionId = collectionId,
                            SceneUuid = sceneUuid,
                            Role = 2,  // system
                            Content = "[VOICE SESSION START]",
                            Action = "voice"
                        });
                    }
                    catch { }

                    return Json(new {
                        ok = true,
                        clientSecret = clientSecret,
                        model = model,
                        expiresAt = (long?)j["client_secret"]?["expires_at"] ?? 0
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[RealtimeSession] EX " + ex);
                return Json(new { ok = false, error = "Exception: " + ex.Message });
            }
        }

        // POST: TTS - text -> audio (mp3) cho mode "Text -> Voice".
        // VBee sync hien chi support mot so voice; neu voice khong support sync
        // thi fallback sang batch async va poll requestId de lay audioLink.
        [HttpPost]
        [SameOriginOnly]
        public async Task<ActionResult> Speak()
        {
            if (!Request.IsAuthenticated) return new HttpUnauthorizedResult();

            // Rate limit: VBee TTS bill per-CHAR (khong phai per-call), nen pipeline tach cau
            // (nhieu call/cau tra loi, tong ky tu khong doi) khong tang chi phi. Cap theo SO CALL
            // de chong abuse -> default cao (600/h) vi moi cau tra loi = nhieu call ngan.
            var ttsLimit = ChatbotConfig.GetInt("CHATBOT_TTS_CALLS_PER_HOUR", "Chatbot.TtsCallsPerHour", 600);
            if (IsRateLimited("tts", MyAuthentication.ID, ttsLimit))
                return new HttpStatusCodeResult(429, "Đã đạt giới hạn " + ttsLimit + " TTS/giờ");

            // Tran NGAY (DB counter -> song sot qua IIS recycle). <=0 = tat.
            var ttsDaily = ChatbotConfig.GetInt("CHATBOT_DAILY_TTS_CALLS_PER_USER", "Chatbot.DailyTtsCallsPerUser", 3000);
            if (ttsDaily > 0 && ChatbotContentStore.GetDailyUsageCalls(MyAuthentication.ID, "tts") >= ttsDaily)
                return new HttpStatusCodeResult(429, "Đã đạt giới hạn đọc (TTS) trong ngày");

            string raw;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
                raw = reader.ReadToEnd();
            JObject body;
            try { body = JObject.Parse(raw ?? "{}"); }
            catch { return new HttpStatusCodeResult(400); }

            var text = (string)body["text"];
            if (string.IsNullOrEmpty(text)) return new HttpStatusCodeResult(400);

            var vbeeToken = Environment.GetEnvironmentVariable("VBEE_API_TOKEN");
            var vbeeAppId = Environment.GetEnvironmentVariable("VBEE_ID_APP");
            if (string.IsNullOrEmpty(vbeeToken) || string.IsNullOrEmpty(vbeeAppId))
            {
                // Thieu key -> 503 (client tu fallback ve text, KHONG retry). KHONG loi toan cuc.
                System.Diagnostics.Debug.WriteLine("[Speak] VBEE_API_TOKEN hoac VBEE_ID_APP chua duoc set");
                return new HttpStatusCodeResult(503, "TTS chưa cấu hình");
            }
            if (ServiceHealth.IsDown(ServiceHealth.VBEE))
                return new HttpStatusCodeResult(503, "TTS tạm thời không khả dụng");

            var voiceCode = GetChatbotCfg("CHATBOT_VBEE_VOICE", "Chatbot.VbeeVoice")
                         ?? "hn_female_ngochuyen_full_48k-fhg";   // Ngoc Huyen - flagship nu Bac
            var speedStr  = GetChatbotCfg("CHATBOT_VBEE_SPEED", "Chatbot.VbeeSpeed") ?? "1.0";
            double speed  = double.TryParse(speedStr, System.Globalization.NumberStyles.Float,
                                            System.Globalization.CultureInfo.InvariantCulture, out var sp) ? sp : 1.0;

            var maxChars = GetVbeeTtsMaxChars(voiceCode);
            if (maxChars > 0 && text.Length > maxChars)
            {
                System.Diagnostics.Debug.WriteLine("[Speak/VBee] truncate textLen=" + text.Length + " max=" + maxChars + " voice=" + voiceCode);
                text = text.Substring(0, maxChars);
            }

            // === Audio cache (DB): cung text+giong+toc do -> tai dung, BO QUA VBee + khong tinh usage ===
            var ttsHash = AudioCacheStore.HashFor(voiceCode, speed, text);
            var cachedAudio = AudioCacheStore.Get(ttsHash);
            if (cachedAudio != null)
            {
                System.Diagnostics.Debug.WriteLine("[Speak] audio cache HIT " + ttsHash.Substring(0, 8) + " bytes=" + cachedAudio.Length);
                return new FileContentResult(cachedAudio, "audio/mpeg");
            }

            // Dem usage TTS theo ngay (call +1, units = so ky tu thuc gui VBee). Chi tinh khi THUC SU goi VBee.
            ChatbotContentStore.IncrementDailyUsage(MyAuthentication.ID, "tts", text.Length);

            var payload = new JObject {
                ["text"]         = text,
                ["mode"]         = "sync",
                ["voiceCode"]    = voiceCode,
                ["outputFormat"] = "mp3",
                ["bitrate"]      = 128,
                ["speed"]        = speed
            };
            System.Diagnostics.Debug.WriteLine("[Speak/VBee] voice=" + voiceCode + " textLen=" + text.Length);

            // NOTE: Speak path tao HttpClient moi per request (per-call timeout=90s).
            // Static _http khong dung duoc o day vi SpeakVbeeAsync chia se DefaultRequestHeaders
            // (Bearer + App-Id) - race condition neu request khac thay header giua chung.
            // Refactor lon hon (rebuild HttpRequestMessage per call) deo lai sau.
            try
            {
                using (var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(90) })
                {
                    http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vbeeToken);
                    http.DefaultRequestHeaders.Add("App-Id", vbeeAppId);

                    if (ShouldUseAsyncTtsFirst(voiceCode))
                    {
                        System.Diagnostics.Debug.WriteLine("[Speak/VBee] async first voice=" + voiceCode);
                        return await SpeakVbeeAsync(http, text, voiceCode, speed);
                    }

                    var content = new System.Net.Http.StringContent(
                        payload.ToString(), System.Text.Encoding.UTF8, "application/json");
                    var resp = await http.PostAsync("https://api.vbee.vn/v1/tts", content);
                    ServiceHealth.MarkUp(ServiceHealth.VBEE);   // co response = ket noi OK (du content co the loi)
                    if (!resp.IsSuccessStatusCode)
                    {
                        var err = await resp.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine("[Speak/VBee] " + (int)resp.StatusCode + " " + err);
                        if (ShouldFallbackToAsyncTts(err))
                        {
                            System.Diagnostics.Debug.WriteLine("[Speak/VBee] sync unsupported, fallback async voice=" + voiceCode);
                            return await SpeakVbeeAsync(http, text, voiceCode, speed);
                        }
                        Response.StatusCode = (int)resp.StatusCode;
                        Response.ContentType = "application/json";
                        return Content("{\"error\":\"VBee " + (int)resp.StatusCode + "\",\"detail\":"
                            + JsonConvert.SerializeObject(err) + "}", "application/json");
                    }

                    return await ReturnVbeeTtsAudio(http, resp, "[Speak/VBee]", voiceCode, speed, text);
                }
            }
            catch (Exception ex)
            {
                // Loi ket noi (firewall chan VBee) -> mark down + 503; client tu fallback ve text.
                if (ServiceHealth.IsConnectivityError(ex)) ServiceHealth.MarkDown(ServiceHealth.VBEE);
                System.Diagnostics.Debug.WriteLine("[Speak/VBee] EX " + ex);
                return new HttpStatusCodeResult(503, "TTS lỗi kết nối");
            }
        }

        [HttpPost]
        public ActionResult VbeeTtsCallback()
        {
            return Json(new { ok = true });
        }

        private static bool ShouldFallbackToAsyncTts(string err)
        {
            if (string.IsNullOrEmpty(err)) return false;
            return err.IndexOf("does not support TTS sync", StringComparison.OrdinalIgnoreCase) >= 0
                || err.IndexOf("support TTS sync", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool ShouldUseAsyncTtsFirst(string voiceCode)
        {
            return !string.IsNullOrEmpty(voiceCode)
                && voiceCode.EndsWith("-phg", StringComparison.OrdinalIgnoreCase);
        }

        // Doc config chatbot theo thu tu uu tien: .env (Environment) -> Web.config AppSettings -> null.
        // Cho phep gom config VBee (giong/toc do/gioi han ky tu) vao .env cung VBEE_API_TOKEN.
        private static string GetChatbotCfg(string envKey, string appSettingsKey)
            => ChatbotConfig.Get(envKey, appSettingsKey, null);

        private static int GetVbeeTtsMaxChars(string voiceCode)
        {
            var isAsyncVoice = ShouldUseAsyncTtsFirst(voiceCode);
            var envKey = isAsyncVoice ? "CHATBOT_VBEE_ASYNC_MAX_CHARS" : "CHATBOT_VBEE_SYNC_MAX_CHARS";
            var appKey = isAsyncVoice ? "Chatbot.VbeeAsyncMaxChars" : "Chatbot.VbeeSyncMaxChars";
            var fallback = isAsyncVoice ? 4000 : 300;
            var maxStr = GetChatbotCfg(envKey, appKey);
            int maxChars;
            if (!int.TryParse(maxStr, out maxChars)) maxChars = fallback;

            return isAsyncVoice
                ? Math.Max(300, Math.Min(10000, maxChars))
                : Math.Max(1, Math.Min(300, maxChars));
        }

        private async Task<ActionResult> SpeakVbeeAsync(System.Net.Http.HttpClient http, string text, string voiceCode, double speed)
        {
            var callbackUrl = Url.Action("VbeeTtsCallback", "Chatbot", new { area = "View360" }, Request.Url.Scheme);
            if (string.IsNullOrEmpty(callbackUrl))
                callbackUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Action("VbeeTtsCallback", "Chatbot", new { area = "View360" });

            var payload = new JObject {
                ["text"]         = text,
                ["mode"]         = "async",
                ["webhookUrl"]   = callbackUrl,
                ["voiceCode"]    = voiceCode,
                ["outputFormat"] = "mp3",
                ["bitrate"]      = 128,
                ["speed"]        = speed
            };

            var content = new System.Net.Http.StringContent(
                payload.ToString(), System.Text.Encoding.UTF8, "application/json");
            var resp = await http.PostAsync("https://api.vbee.vn/v1/tts", content);
            ServiceHealth.MarkUp(ServiceHealth.VBEE);   // co response = ket noi OK
            var respBody = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine("[Speak/VBee async] " + (int)resp.StatusCode + " " + respBody);
                Response.StatusCode = (int)resp.StatusCode;
                Response.ContentType = "application/json";
                return Content("{\"error\":\"VBee async " + (int)resp.StatusCode + "\",\"detail\":"
                    + JsonConvert.SerializeObject(respBody) + "}", "application/json");
            }

            var j = JObject.Parse(respBody);
            var requestId = (string)j["requestId"] ?? (string)j["result"]?["requestId"];
            if (string.IsNullOrEmpty(requestId))
            {
                Response.StatusCode = 502;
                Response.ContentType = "application/json";
                return Content("{\"error\":\"VBee async response missing requestId\",\"detail\":"
                    + JsonConvert.SerializeObject(respBody) + "}", "application/json");
            }

            int maxPollSeconds = ChatbotConfig.GetInt("CHATBOT_VBEE_ASYNC_POLL_SECONDS", "Chatbot.VbeeAsyncPollSeconds", 60);
            maxPollSeconds = Math.Max(15, Math.Min(120, maxPollSeconds));

            var deadline = DateTime.UtcNow.AddSeconds(maxPollSeconds);
            var attempt = 0;
            var lastStatus = "";
            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(attempt < 4 ? 500 : 1000);
                attempt++;
                var poll = await http.GetAsync("https://api.vbee.vn/v1/tts/requests/" + Uri.EscapeDataString(requestId));
                var pollBody = await poll.Content.ReadAsStringAsync();
                if (!poll.IsSuccessStatusCode)
                {
                    lastStatus = "HTTP " + (int)poll.StatusCode;
                    System.Diagnostics.Debug.WriteLine("[Speak/VBee async poll] " + (int)poll.StatusCode + " " + pollBody);
                    continue;
                }

                var pj = JObject.Parse(pollBody);
                var status = ((string)pj["status"] ?? (string)pj["result"]?["status"] ?? (string)pj["data"]?["status"] ?? "").ToUpperInvariant();
                lastStatus = status;
                var audioLink = (string)pj["audioLink"] ?? (string)pj["audio_link"]
                             ?? (string)pj["result"]?["audioLink"]
                             ?? (string)pj["result"]?["audio_link"]
                             ?? (string)pj["data"]?["audioLink"]
                             ?? (string)pj["data"]?["audio_link"];
                if (!string.IsNullOrEmpty(audioLink))
                {
                    var audioRes = await http.GetAsync(audioLink);
                    var audioBytes = await audioRes.Content.ReadAsByteArrayAsync();
                    if (!audioRes.IsSuccessStatusCode || audioBytes.Length == 0)
                    {
                        Response.StatusCode = 502;
                        return Content("{\"error\":\"VBee audioLink fetch failed\"}", "application/json");
                    }
                    System.Diagnostics.Debug.WriteLine("[Speak/VBee async] OK bytes=" + audioBytes.Length);
                    AudioCacheStore.Save(AudioCacheStore.HashFor(voiceCode, speed, text), voiceCode, speed, text, audioBytes);
                    return new FileContentResult(audioBytes, "audio/mpeg");
                }
                if (status == "FAILED" || status == "FAILURE" || status == "ERROR" || status == "CANCELED")
                {
                    Response.StatusCode = 502;
                    Response.ContentType = "application/json";
                    return Content("{\"error\":\"VBee async failed\",\"detail\":"
                        + JsonConvert.SerializeObject(pollBody) + "}", "application/json");
                }
            }

            Response.StatusCode = 504;
            Response.ContentType = "application/json";
            return Content("{\"error\":\"VBee async timeout\",\"requestId\":"
                + JsonConvert.SerializeObject(requestId)
                + ",\"lastStatus\":" + JsonConvert.SerializeObject(lastStatus)
                + ",\"timeoutSeconds\":" + maxPollSeconds + "}", "application/json");
        }

        private async Task<ActionResult> ReturnVbeeTtsAudio(System.Net.Http.HttpClient http, System.Net.Http.HttpResponseMessage resp, string logPrefix,
            string voiceCode, double speed, string text)
        {
            var contentType = resp.Content.Headers.ContentType?.MediaType ?? "";
            if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            {
                var audio = await resp.Content.ReadAsByteArrayAsync();
                System.Diagnostics.Debug.WriteLine(logPrefix + " OK direct audio bytes=" + audio.Length);
                AudioCacheStore.Save(AudioCacheStore.HashFor(voiceCode, speed, text), voiceCode, speed, text, audio);
                return new FileContentResult(audio, "audio/mpeg");
            }

            var respBody = await resp.Content.ReadAsStringAsync();
            var j = JObject.Parse(respBody);
            var audioLink = (string)j["audio_link"] ?? (string)j["audioLink"]
                         ?? (string)j["result"]?["audio_link"]
                         ?? (string)j["result"]?["audioLink"];
            if (string.IsNullOrEmpty(audioLink))
            {
                System.Diagnostics.Debug.WriteLine(logPrefix + " JSON khong co audio_link: " + respBody);
                Response.StatusCode = 502;
                Response.ContentType = "application/json";
                return Content("{\"error\":\"VBee response missing audio_link\",\"detail\":"
                    + JsonConvert.SerializeObject(respBody) + "}", "application/json");
            }

            var audioRes = await http.GetAsync(audioLink);
            var audioBytes = await audioRes.Content.ReadAsByteArrayAsync();
            System.Diagnostics.Debug.WriteLine(logPrefix + " OK via link bytes=" + audioBytes.Length);
            AudioCacheStore.Save(AudioCacheStore.HashFor(voiceCode, speed, text), voiceCode, speed, text, audioBytes);
            return new FileContentResult(audioBytes, "audio/mpeg");
        }

        // POST: STT push-to-talk. Browser upload audio blob (multipart "audio" field),
        // server forward sang VBee /v1/stt (sync mode, <=10s), tra transcript.
        [HttpPost]
        [SameOriginOnly]
        public async Task<ActionResult> Transcribe()
        {
            if (!Request.IsAuthenticated) return new HttpUnauthorizedResult();

            // Rate limit: VBee STT bill per-second, mac dinh cap 60 STT calls/h/user.
            var sttLimit = ChatbotConfig.GetInt("CHATBOT_STT_CALLS_PER_HOUR", "Chatbot.SttCallsPerHour", 60);
            if (IsRateLimited("stt", MyAuthentication.ID, sttLimit))
                return Json(new { ok = false, limited = true, error = "Đã đạt giới hạn " + sttLimit + " lượt nói/giờ. Vui lòng thử lại sau." });

            // Tran NGAY (DB counter -> song sot qua IIS recycle). <=0 = tat.
            var sttDaily = ChatbotConfig.GetInt("CHATBOT_DAILY_STT_CALLS_PER_USER", "Chatbot.DailySttCallsPerUser", 300);
            if (sttDaily > 0 && ChatbotContentStore.GetDailyUsageCalls(MyAuthentication.ID, "stt") >= sttDaily)
                return Json(new { ok = false, limited = true, error = "Đã đạt giới hạn nhận giọng nói trong ngày. Vui lòng thử lại vào ngày mai." });

            var file = Request.Files["audio"];
            if (file == null || file.ContentLength == 0)
                return Json(new { ok = false, error = "Khong co audio" });
            if (file.ContentLength > MAX_AUDIO_BYTES)
                return Json(new { ok = false, error = "File qua lon (>10MB)" });

            // Dem usage STT theo ngay (call +1, units = giay audio neu client gui).
            double sttDur;
            double.TryParse(Request.Form["clientDurationSeconds"], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out sttDur);
            ChatbotContentStore.IncrementDailyUsage(MyAuthentication.ID, "stt", (long)Math.Ceiling(sttDur));

            var vbeeToken = Environment.GetEnvironmentVariable("VBEE_API_TOKEN");
            var vbeeAppId = Environment.GetEnvironmentVariable("VBEE_ID_APP");
            if (string.IsNullOrEmpty(vbeeToken) || string.IsNullOrEmpty(vbeeAppId))
                return Json(new { ok = false, error = "Nhận giọng nói chưa được cấu hình." });
            if (ServiceHealth.IsDown(ServiceHealth.VBEE))
                return Json(new { ok = false, error = "Nhận giọng nói tạm thời không khả dụng." });

            try
            {
                byte[] audioBytes;
                using (var ms = new System.IO.MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    audioBytes = ms.ToArray();
                }
                System.Diagnostics.Debug.WriteLine("[Transcribe] received " + audioBytes.Length + " bytes type=" + file.ContentType);
                var debugAudio = TrySaveSttDebugAudio(audioBytes, file);

                // Transcribe: tuong tu Speak - giu HttpClient per-call de tranh race tren
                // DefaultRequestHeaders (Bearer + App-Id) duoc set per-instance.
                using (var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(30) })
                {
                    http.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", vbeeToken);
                    http.DefaultRequestHeaders.Add("App-Id", vbeeAppId);

                    using (var form = new System.Net.Http.MultipartFormDataContent())
                    {
                        var audioContent = new System.Net.Http.ByteArrayContent(audioBytes);
                        // MediaTypeHeaderValue ctor KHONG accept ;codecs=... params -> phai dung Parse,
                        // fallback strip codec part neu Parse fail.
                        var rawCt = file.ContentType ?? "audio/webm";
                        System.Net.Http.Headers.MediaTypeHeaderValue parsedCt;
                        if (!System.Net.Http.Headers.MediaTypeHeaderValue.TryParse(rawCt, out parsedCt))
                        {
                            var semi = rawCt.IndexOf(';');
                            var bareType = semi > 0 ? rawCt.Substring(0, semi).Trim() : rawCt.Trim();
                            if (string.IsNullOrEmpty(bareType)) bareType = "audio/webm";
                            parsedCt = new System.Net.Http.Headers.MediaTypeHeaderValue(bareType);
                        }
                        audioContent.Headers.ContentType = parsedCt;
                        form.Add(audioContent, "audioContent", file.FileName ?? "audio.webm");
                        form.Add(new System.Net.Http.StringContent("sync"), "mode");
                        form.Add(new System.Net.Http.StringContent("vi-VN"), "languageCode");

                        var resp = await http.PostAsync("https://api.vbee.vn/v1/stt", form);
                        ServiceHealth.MarkUp(ServiceHealth.VBEE);   // co response = ket noi OK
                        var respBody = await resp.Content.ReadAsStringAsync();
                        if (!resp.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine("[Transcribe/VBee] " + (int)resp.StatusCode + " " + respBody);
                            return Json(new { ok = false, error = "VBee " + (int)resp.StatusCode + ": " + TruncateForLog(respBody), debugAudio = debugAudio });
                        }
                        var j = JObject.Parse(respBody);
                        // Tim transcript - thu nhieu field name variations
                        var transcript = (string)j["transcript"]
                                      ?? (string)j["text"]
                                      ?? (string)j["result"]?["transcript"]
                                      ?? (string)j["result"]?["text"]
                                      ?? (string)j["data"]?["transcript"]
                                      ?? (string)j["data"]?["text"];
                        if (string.IsNullOrEmpty(transcript))
                        {
                            System.Diagnostics.Debug.WriteLine("[Transcribe/VBee] khong tim thay transcript: " + respBody);
                            return Json(new { ok = false, error = "Khong tim thay transcript", raw = TruncateForLog(respBody), debugAudio = debugAudio });
                        }
                        return Json(new { ok = true, transcript = transcript, debugAudio = debugAudio });
                    }
                }
            }
            catch (Exception ex)
            {
                if (ServiceHealth.IsConnectivityError(ex)) ServiceHealth.MarkDown(ServiceHealth.VBEE);
                System.Diagnostics.Debug.WriteLine("[Transcribe] EX " + ex);
                return Json(new { ok = false, error = "Lỗi nhận giọng nói. Vui lòng thử lại." });
            }
        }

        private static string TruncateForLog(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length > 400 ? s.Substring(0, 400) + "..." : s;
        }

        [HttpGet]
        public ActionResult SttDebugAudio(string id)
        {
            if (!Request.IsAuthenticated) return new HttpUnauthorizedResult();

            var safeName = System.IO.Path.GetFileName(id ?? "");
            if (string.IsNullOrEmpty(safeName) || !string.Equals(safeName, id, StringComparison.Ordinal))
                return new HttpStatusCodeResult(400);

            var dir = Server.MapPath("~/App_Data/ChatbotSttDebug");
            var path = System.IO.Path.Combine(dir, safeName);
            if (!System.IO.File.Exists(path)) return HttpNotFound("Debug audio not found");

            return File(path, GuessAudioContentType(path), safeName);
        }

        private sealed class SttDebugAudioFile
        {
            public string Kind { get; set; }
            public string Url { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
            public int Bytes { get; set; }
        }

        private sealed class SttDebugAudioResult
        {
            public bool Saved { get; set; }
            public string Error { get; set; }
            public string ClientSampleRate { get; set; }
            public string ClientDurationSeconds { get; set; }
            public SttDebugAudioFile Wav { get; set; }
            public SttDebugAudioFile Raw { get; set; }
        }

        private SttDebugAudioResult TrySaveSttDebugAudio(byte[] vbeeAudioBytes, System.Web.HttpPostedFileBase vbeeFile)
        {
            if (!IsSttDebugSaveEnabled()) return null;

            var result = new SttDebugAudioResult {
                ClientSampleRate = Request.Form["clientSampleRate"],
                ClientDurationSeconds = Request.Form["clientDurationSeconds"]
            };

            try
            {
                var dir = Server.MapPath("~/App_Data/ChatbotSttDebug");
                System.IO.Directory.CreateDirectory(dir);
                CleanupSttDebugAudio(dir);

                var stem = DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff", System.Globalization.CultureInfo.InvariantCulture)
                         + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);

                result.Wav = SaveSttDebugFile(dir, stem + "_vbee", "wav", vbeeAudioBytes,
                    vbeeFile.FileName, vbeeFile.ContentType, "wav");

                var raw = Request.Files["debugAudio"];
                if (raw != null && raw.ContentLength > 0 && raw.ContentLength <= MAX_AUDIO_BYTES)
                {
                    byte[] rawBytes;
                    using (var ms = new System.IO.MemoryStream())
                    {
                        raw.InputStream.CopyTo(ms);
                        rawBytes = ms.ToArray();
                    }
                    var rawContentType = Request.Form["debugAudioType"];
                    if (string.IsNullOrEmpty(rawContentType)) rawContentType = raw.ContentType;

                    result.Raw = SaveSttDebugFile(dir, stem + "_raw", null, rawBytes,
                        raw.FileName, rawContentType, "raw");
                }

                result.Saved = result.Wav != null || result.Raw != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Transcribe.DebugAudio] " + ex);
                result.Error = ex.Message;
            }
            return result;
        }

        private bool IsSttDebugSaveEnabled()
        {
            var setting = ChatbotConfig.Get("CHATBOT_STT_DEBUG_SAVE_AUDIO", "Chatbot.SttDebugSaveAudio", null);
            return string.Equals(setting, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Request.Form["saveSttDebug"], "1", StringComparison.OrdinalIgnoreCase);
        }

        private SttDebugAudioFile SaveSttDebugFile(string dir, string stem, string forcedExt, byte[] bytes,
            string originalName, string contentType, string kind)
        {
            var ext = string.IsNullOrEmpty(forcedExt)
                ? AudioExtensionFrom(originalName, contentType, "bin")
                : forcedExt.TrimStart('.').ToLowerInvariant();
            var fileName = stem + "." + ext;
            var path = System.IO.Path.Combine(dir, fileName);
            System.IO.File.WriteAllBytes(path, bytes);
            return new SttDebugAudioFile {
                Kind = kind,
                FileName = fileName,
                Url = Url.Action("SttDebugAudio", "Chatbot", new { area = "View360", id = fileName }),
                ContentType = NormalizeAudioContentType(contentType, ext),
                Bytes = bytes.Length
            };
        }

        private static string AudioExtensionFrom(string fileName, string contentType, string fallback)
        {
            var ext = (System.IO.Path.GetExtension(fileName ?? "") ?? "").TrimStart('.').ToLowerInvariant();
            if (ext == "mp3" || ext == "wav" || ext == "webm" || ext == "ogg" || ext == "m4a" || ext == "mp4")
                return ext == "mp4" ? "m4a" : ext;

            var type = (contentType ?? "").ToLowerInvariant();
            var semi = type.IndexOf(';');
            if (semi >= 0) type = type.Substring(0, semi).Trim();
            if (type == "audio/mpeg" || type == "audio/mp3") return "mp3";
            if (type == "audio/wav" || type == "audio/x-wav" || type == "audio/wave") return "wav";
            if (type == "audio/webm" || type == "video/webm") return "webm";
            if (type == "audio/ogg" || type == "application/ogg") return "ogg";
            if (type == "audio/mp4" || type == "audio/aac" || type == "video/mp4") return "m4a";
            return fallback;
        }

        private static string NormalizeAudioContentType(string contentType, string ext)
        {
            var type = (contentType ?? "").Trim();
            if (!string.IsNullOrEmpty(type)) return type;
            switch ((ext ?? "").ToLowerInvariant())
            {
                case "mp3": return "audio/mpeg";
                case "wav": return "audio/wav";
                case "webm": return "audio/webm";
                case "ogg": return "audio/ogg";
                case "m4a": return "audio/mp4";
                default: return "application/octet-stream";
            }
        }

        private static string GuessAudioContentType(string path)
        {
            return NormalizeAudioContentType(null, (System.IO.Path.GetExtension(path) ?? "").TrimStart('.'));
        }

        private static void CleanupSttDebugAudio(string dir)
        {
            int hours = ChatbotConfig.GetInt("CHATBOT_STT_DEBUG_MAX_AGE_HOURS", "Chatbot.SttDebugMaxAgeHours", 48);
            if (hours <= 0) return;

            var cutoff = DateTime.UtcNow.AddHours(-hours);
            foreach (var path in System.IO.Directory.GetFiles(dir))
            {
                try
                {
                    if (System.IO.File.GetCreationTimeUtc(path) < cutoff)
                        System.IO.File.Delete(path);
                }
                catch { }
            }
        }

        /// <summary>Voice-optimized instructions: ngan hon text mode, conversational.</summary>
        private static string BuildVoiceInstructions(
            ChatbotTourInfo tourInfo,
            Dictionary<string, ChatbotSceneInfo> sceneInfo,
            List<KuulaCollectionFetcher.SceneInfo> allScenes,
            Dictionary<string, string> customTitles,
            string currentSceneUuid)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Bạn là hướng dẫn viên ảo của Khu Liên Hợp HPDQ, trò chuyện qua GIỌNG NÓI tiếng Việt với user đang xem tour 360°.");
            sb.AppendLine();
            sb.AppendLine("=== QUY TẮC GIỌNG NÓI ===");
            sb.AppendLine("- Trả lời ngắn, tự nhiên (1-3 câu). Không liệt kê dài.");
            sb.AppendLine("- User có thể có giọng miền Trung/Quảng Ngãi. Nếu nghe không rõ tên riêng/số/địa chỉ, HỎI XÁC NHẬN lại.");
            sb.AppendLine("- KHÔNG bịa số liệu, KHÔNG suy đoán. Nếu không có thông tin: nói 'Tôi chưa được cung cấp thông tin về điều này.'");
            sb.AppendLine("- Tự nhiên, thân thiện. Tránh nghe như đọc văn bản.");
            sb.AppendLine();
            sb.AppendLine("=== KHÔNG VIẾT TẮT (đọc đầy đủ vì TTS đọc chữ cái rời sai) ===");
            sb.AppendLine("- Luôn đọc/nói TÊN ĐẦY ĐỦ:");
            sb.AppendLine("  + 'KLH'    → 'Khu Liên Hợp'");
            sb.AppendLine("  + 'HPDQ'   → 'Hòa Phát Dung Quất'");
            sb.AppendLine("  + 'KLH HPDQ' → 'Khu Liên Hợp Hòa Phát Dung Quất'");
            sb.AppendLine("  + 'Cty/CT' → 'Công ty'; 'TNHH' → 'Trách Nhiệm Hữu Hạn'");
            sb.AppendLine("  + 'SX' → 'sản xuất'; 'NM' → 'nhà máy'");
            sb.AppendLine("- Đơn vị: 'mét vuông' không 'm²'; 'tấn mỗi tháng' không 'tấn/tháng'.");
            sb.AppendLine("- Năm/số quan trọng đọc rõ từng chữ số khi cần (vd '2019' = 'hai-nghìn-không-trăm-mười-chín' hoặc 'năm hai nghìn mười chín').");
            sb.AppendLine();
            sb.AppendLine("=== NAVIGATION (QUAN TRỌNG) ===");
            sb.AppendLine("Khi user nói muốn đi tới điểm khác (vd 'đưa tôi tới cảng', 'chuyển sang khu A', 'qua phòng họp'):");
            sb.AppendLine("- GỌI function `navigate_to_scene` với `query` = từ khoá user dùng (vd 'cảng', 'khu A').");
            sb.AppendLine("- Đồng thời nói 1 câu ngắn xác nhận như 'Đang đưa bạn tới cảng'.");
            sb.AppendLine("KHÔNG navigate khi user chỉ HỎI THÔNG TIN (vd 'cảng có đặc điểm gì', 'giới thiệu về cảng', 'cảng có gì'):");
            sb.AppendLine("- Đây là câu hỏi thông tin → TRẢ LỜI bằng lời, KHÔNG gọi navigate_to_scene. Nhắc tên điểm KHÔNG phải ý định di chuyển.");
            sb.AppendLine();
            if (!string.IsNullOrEmpty(tourInfo?.Overview))
            {
                sb.AppendLine("=== TỔNG QUAN TOUR ===");
                sb.AppendLine(tourInfo.Overview);
                sb.AppendLine();
            }
            if (!string.IsNullOrEmpty(currentSceneUuid))
            {
                var curName = customTitles.ContainsKey(currentSceneUuid)
                    ? customTitles[currentSceneUuid]
                    : (allScenes.FirstOrDefault(x => x.Uuid == currentSceneUuid)?.Title ?? "(không tên)");
                sb.AppendLine("=== ĐIỂM USER ĐANG XEM ===");
                sb.AppendLine("Tên: " + curName);
                if (sceneInfo.ContainsKey(currentSceneUuid))
                {
                    var ci = sceneInfo[currentSceneUuid];
                    if (!string.IsNullOrEmpty(ci.ShortIntro))    sb.AppendLine("Giới thiệu: " + ci.ShortIntro);
                    if (!string.IsNullOrEmpty(ci.DetailContent)) sb.AppendLine("Chi tiết: " + ci.DetailContent);
                }
                sb.AppendLine();
            }
            sb.AppendLine("=== DANH SÁCH CÁC ĐIỂM (cho ngữ cảnh navigate) ===");
            int n = 0;
            foreach (var s in allScenes
                .Where(s => !string.IsNullOrEmpty(s.Uuid) && s.Uuid != currentSceneUuid)
                .OrderByDescending(s => customTitles.ContainsKey(s.Uuid) ? 1 : 0))
            {
                if (++n > 40) { sb.AppendLine("...(còn nữa)"); break; }
                var name = customTitles.ContainsKey(s.Uuid) ? customTitles[s.Uuid] : s.Title;
                sb.Append("- ").Append(name ?? "(chưa đặt tên)");
                if (sceneInfo.ContainsKey(s.Uuid) && !string.IsNullOrEmpty(sceneInfo[s.Uuid].ShortIntro))
                    sb.Append(" — ").Append(sceneInfo[s.Uuid].ShortIntro);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        // POST: User chat endpoint - STREAMING SSE.
        // Client (view360-chatbot.js) doc tung chunk va render bubble incrementally.
        // Event types: 'text' (delta), 'done' (action + tokens), 'error' (message).
        [HttpPost]
        [SameOriginOnly]
        public async Task AskStream()
        {
            var resp = System.Web.HttpContext.Current.Response;
            if (!Request.IsAuthenticated) { resp.StatusCode = 401; resp.End(); return; }

            // ===== Parse body =====
            string raw;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
                raw = reader.ReadToEnd();
            JObject body;
            try { body = JObject.Parse(raw ?? "{}"); }
            catch { resp.StatusCode = 400; resp.End(); return; }

            var collectionId = (string)body["collectionId"];
            var sceneUuid    = (string)body["sceneUuid"];
            var sessionGuidStr = (string)body["sessionGuid"];
            var message      = (string)body["message"];
            var historyArr   = body["history"] as JArray;

            // ===== Setup SSE response =====
            resp.Buffer = false;
            resp.BufferOutput = false;
            resp.ContentType = "text/event-stream; charset=utf-8";
            resp.Headers["Cache-Control"] = "no-cache, no-transform";
            resp.Headers["X-Accel-Buffering"] = "no"; // nginx hint, IIS bo qua nhung khong sao
            resp.Headers["Connection"] = "keep-alive";

            Action<string, object> writeEvent = (evt, data) => {
                try
                {
                    resp.Write("event: " + evt + "\n");
                    resp.Write("data: " + JsonConvert.SerializeObject(data) + "\n\n");
                    resp.Flush();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("[AskStream.writeEvent] " + ex.Message);
                }
            };

            // ===== Validation =====
            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(message))
            { writeEvent("error", new { error = "missing collectionId or message" }); resp.End(); return; }
            if (message.Length > MAX_MESSAGE_LEN)
            { writeEvent("error", new { error = "Câu hỏi quá dài (max " + MAX_MESSAGE_LEN + " ký tự)" }); resp.End(); return; }

            Guid sessionGuid;
            if (!Guid.TryParse(sessionGuidStr, out sessionGuid)) sessionGuid = Guid.NewGuid();

            // Rate limit
            int rateLimit = ChatbotConfig.GetInt("CHATBOT_RATE_LIMIT_PER_HOUR", "Chatbot.RateLimitPerHour", 60);
            int recent = ChatbotContentStore.CountRecentUserMessages(MyAuthentication.ID, 60);
            if (recent >= rateLimit)
            { writeEvent("error", new { error = "Đã đạt giới hạn " + rateLimit + " câu/giờ. Vui lòng thử lại sau." }); resp.End(); return; }

            // Tran token/ngay (chong dot quota OpenAI khi treo/loi/abuse)
            var dailyCapMsg = CheckDailyTokenCap(MyAuthentication.ID);
            if (dailyCapMsg != null)
            { writeEvent("error", new { error = dailyCapMsg }); resp.End(); return; }

            // Load tour info
            var tourInfo = GetCached("chatbot_tour_" + collectionId,
                () => ChatbotContentStore.GetTourInfo(collectionId));
            if (tourInfo == null || !tourInfo.IsEnabled)
            { writeEvent("error", new { error = "Chatbot chưa được kích hoạt cho tour này." }); resp.End(); return; }

            var sceneInfo = GetCached("chatbot_scenes_" + collectionId,
                () => ChatbotContentStore.GetSceneInfo(collectionId))
                ?? new Dictionary<string, ChatbotSceneInfo>();

            var allScenes = KuulaCollectionFetcher.GetScenes(collectionId) ?? new List<KuulaCollectionFetcher.SceneInfo>();
            var customTitles = SceneCalibrationStore.Get(collectionId)
                .Where(kv => !string.IsNullOrEmpty(kv.Value.CustomTitle))
                .ToDictionary(kv => kv.Key, kv => kv.Value.CustomTitle);

            // Danh sach diem da cau hinh -> enum cho tool + render trong prompt + map ten->uuid.
            var navList = BuildNavList(allScenes, sceneInfo, customTitles, sceneUuid);
            var navMap  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var nv in navList) navMap[nv.Label] = nv.Uuid;
            var sysPrompt = BuildSystemPrompt(tourInfo, sceneInfo, allScenes, customTitles, sceneUuid, navList);

            var history = new List<ChatTurn>();
            if (historyArr != null)
            {
                foreach (var h in historyArr.OfType<JObject>())
                {
                    var role = (string)h["role"];
                    var content = (string)h["content"];
                    if (string.IsNullOrEmpty(content)) continue;
                    history.Add(new ChatTurn { Role = role, Content = content });
                }
                if (history.Count > 10) history = history.Skip(history.Count - 10).ToList();
            }

            // ===== Stream =====
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var fullText  = new StringBuilder();
            string actionType = null, actionTarget = null;
            string[] suggestions = null;

            // ===== Suggestion delimiter parsing state =====
            // AI ket thuc text response bang: "\n---SUGGEST---\nq1|q2|q3".
            // Buffer last 16 chars cho truong hop delimiter spans chunks.
            const string SUGG_SENT = "---SUGGEST---";
            var holdBuf = new StringBuilder();
            var suggestBuf = new StringBuilder();
            bool inSuggest = false;

            // ===== Knowledge cache (retrieval-first): cau hoi DOC LAP (history rong) -> thu tra loi tu KB =====
            // Hit -> stream tra loi cache (+ audio cache), BO QUA OpenAI. Loi KB -> bo qua, chay luong thuong.
            bool kbEnabled = ChatbotConfig.Get("CHATBOT_KB_ENABLED", "Chatbot.KbEnabled", "true") == "true";
            if (kbEnabled)
            {
                try
                {
                    double simT = double.TryParse(ChatbotConfig.Get("CHATBOT_KB_SIM_THRESHOLD", "Chatbot.KbSimThreshold", "0.88"),
                        System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var st) ? st : 0.92;
                    int kbMinScore = ChatbotConfig.GetInt("CHATBOT_KB_MIN_SCORE", "Chatbot.KbMinScore", 85);

                    // B1: exact-normalized (tuc thi, khong goi embedding) - hop cho cau mac dinh / lap nguyen van.
                    var hit = KnowledgeStore.Lookup(collectionId, sceneUuid, null, message, simT, kbMinScore);
                    // B2: chua trung -> semantic match qua embedding.
                    if (hit == null)
                    {
                        var qEmb = await new OpenAIEmbeddingClient().EmbedAsync(message);
                        if (qEmb != null) hit = KnowledgeStore.Lookup(collectionId, sceneUuid, qEmb, message, simT, kbMinScore);
                    }

                    // Nav cache: chong Kuula uuid DRIFT -> uuid dich khong con trong tour thi BO hit,
                    // de OpenAI resolve lai (ten -> uuid moi) + tu cache lai dung. (drift tu lanh)
                    if (hit != null && hit.ActionType == "navigate"
                        && !(! string.IsNullOrEmpty(hit.ActionTarget) && allScenes.Any(x => x.Uuid == hit.ActionTarget)))
                    {
                        System.Diagnostics.Debug.WriteLine("[AskStream] KB nav hit BO (uuid stale): " + hit.ActionTarget);
                        hit = null;
                    }

                    if (hit != null && !string.IsNullOrEmpty(hit.AnswerText))
                    {
                        writeEvent("text", new { delta = hit.AnswerText });
                        string hitName = (hit.ActionType == "navigate" && !string.IsNullOrEmpty(hit.ActionTarget))
                            ? GetSceneDisplayName(hit.ActionTarget, customTitles, allScenes) : null;
                        writeEvent("done", new {
                            action = (hit.ActionType == "navigate" && !string.IsNullOrEmpty(hit.ActionTarget))
                                ? (object)new { type = "navigate", target = hit.ActionTarget, name = hitName }
                                : null,
                            suggestions = hit.Suggestions,
                            tokensIn = 0, tokensOut = 0,
                            latencyMs = (int)sw.ElapsedMilliseconds,
                            cached = true
                        });
                        System.Diagnostics.Debug.WriteLine("[AskStream] KB HIT id=" + hit.Id + " (" + sw.ElapsedMilliseconds + "ms)");
                        try
                        {
                            ChatbotContentStore.LogMessage(new ChatbotMessageLog { SessionGuid = sessionGuid, NhanVienID = MyAuthentication.ID, CollectionId = collectionId, SceneUuid = sceneUuid, Role = 0, Content = message });
                            ChatbotContentStore.LogMessage(new ChatbotMessageLog { SessionGuid = sessionGuid, NhanVienID = MyAuthentication.ID, CollectionId = collectionId, SceneUuid = sceneUuid, Role = 1, Content = hit.AnswerText, Action = (hit.ActionType == "navigate" ? ("navigate:" + hit.ActionTarget) : "cache"), TokensIn = 0, TokensOut = 0, LatencyMs = (int)sw.ElapsedMilliseconds });
                        }
                        catch { }
                        try { resp.End(); } catch { }
                        return;
                    }
                }
                catch (Exception exKb) { System.Diagnostics.Debug.WriteLine("[AskStream.KB lookup] " + exKb.Message); }
            }

            var client = new OpenAIChatbotClient();
            await client.StreamAsync(new ChatbotRequest {
                SystemPrompt = sysPrompt,
                History      = history,
                UserMessage  = message,
                NavLabels    = navList.Select(x => x.Label).ToList()
            },
            onTextDelta: delta => {
                fullText.Append(delta);
                if (inSuggest)
                {
                    suggestBuf.Append(delta);
                    return;
                }
                holdBuf.Append(delta);
                var holdStr = holdBuf.ToString();
                var idx = holdStr.IndexOf(SUGG_SENT, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    // Emit text truoc delimiter (loai bo trailing newline)
                    var before = holdStr.Substring(0, idx).TrimEnd('\r', '\n');
                    if (before.Length > 0) writeEvent("text", new { delta = before });
                    // Sau delimiter -> suggest buffer
                    suggestBuf.Append(holdStr.Substring(idx + SUGG_SENT.Length));
                    holdBuf.Clear();
                    inSuggest = true;
                }
                else if (holdStr.Length > SUGG_SENT.Length)
                {
                    // Emit phan an toan (giu lai last N chars phong delimiter spans chunk)
                    var keep = SUGG_SENT.Length;
                    var emitNow = holdStr.Substring(0, holdStr.Length - keep);
                    writeEvent("text", new { delta = emitNow });
                    holdBuf.Clear();
                    holdBuf.Append(holdStr.Substring(holdStr.Length - keep));
                }
            },
            onToolCall: (name, argsJson) => {
                if (name != "navigate_to_scene") return;
                try
                {
                    var args = JObject.Parse(argsJson ?? "{}");
                    // Tool moi: `scene` = ten chinh xac tu enum. Fallback `query` (tool cu).
                    var scene = (string)args["scene"] ?? (string)args["query"];
                    var reason = (string)args["reason"];
                    if (string.IsNullOrEmpty(scene)) return;

                    // Uu tien lookup ten chinh xac -> uuid; neu lech thi FindSceneByQuery (fuzzy) lam luoi do.
                    string mUuid, mName;
                    if (navMap.TryGetValue(scene.Trim(), out var exactUuid))
                    {
                        mUuid = exactUuid; mName = scene.Trim();
                    }
                    else
                    {
                        var match = FindSceneByQuery(scene, allScenes, sceneInfo, customTitles, sceneUuid);
                        mUuid = match.uuid; mName = match.name;
                    }

                    if (mUuid != null)
                    {
                        actionType   = "navigate";
                        actionTarget = mUuid;
                        System.Diagnostics.Debug.WriteLine("[AskStream] navigate '" + scene + "' -> " + mUuid + " (" + mName + ")");

                        // Bubble text: dung reason; them ten thuc te neu khac
                        var bubbleText = reason ?? "Đang đưa bạn tới điểm đó.";
                        if (fullText.Length == 0)
                        {
                            fullText.Append(bubbleText);
                            writeEvent("text", new { delta = bubbleText });
                        }
                    }
                    else
                    {
                        // Khong tim thay scene - tra text fallback
                        var notFound = "Tôi không tìm thấy điểm \"" + scene + "\" trong tour. Bạn có thể thử tên khác hoặc chọn từ minimap.";
                        if (fullText.Length == 0)
                        {
                            fullText.Append(notFound);
                            writeEvent("text", new { delta = notFound });
                        }
                    }
                }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[AskStream.onTool] " + ex.Message); }
            },
            onComplete: (tokensIn, tokensOut, error) => {
                sw.Stop();
                if (!string.IsNullOrEmpty(error))
                {
                    writeEvent("error", new { error });
                }
                else
                {
                    // Flush phan con lai trong holdBuf neu khong thay delimiter
                    if (!inSuggest && holdBuf.Length > 0)
                    {
                        writeEvent("text", new { delta = holdBuf.ToString() });
                        holdBuf.Clear();
                    }

                    // Parse suggestions tu AI (Q&A case)
                    if (inSuggest && suggestBuf.Length > 0)
                    {
                        var suggRaw = suggestBuf.ToString().Trim();
                        // Bo dau ngoac kep/khoang trang/newlines
                        suggRaw = suggRaw.Trim('\r', '\n', ' ', '\t');
                        if (suggRaw.Length > 0)
                        {
                            suggestions = suggRaw.Split('|')
                                .Select(s => s.Trim().Trim('"', '\'', ' ', '-', '*'))
                                .Where(s => s.Length > 0 && s.Length <= 60)
                                .Take(3)
                                .ToArray();
                            if (suggestions.Length == 0) suggestions = null;
                        }
                    }

                    string actionName = null;
                    if (actionType == "navigate" && !string.IsNullOrEmpty(actionTarget))
                    {
                        actionName = GetSceneDisplayName(actionTarget, customTitles, allScenes);
                        // Navigate case: server generate suggestions cho scene dich
                        if (suggestions == null || suggestions.Length == 0)
                            suggestions = BuildNavigateSuggestions(actionTarget, actionName, sceneInfo);
                    }

                    writeEvent("done", new {
                        action = actionType == "navigate"
                            ? (object)new { type = "navigate", target = actionTarget, name = actionName }
                            : null,
                        suggestions = suggestions,
                        tokensIn,
                        tokensOut,
                        latencyMs = (int)sw.ElapsedMilliseconds
                    });
                }

                // Log both turns - fire and forget
                try
                {
                    ChatbotContentStore.LogMessage(new ChatbotMessageLog {
                        SessionGuid = sessionGuid,
                        NhanVienID  = MyAuthentication.ID,
                        CollectionId = collectionId,
                        SceneUuid    = sceneUuid,
                        Role = 0, Content = message
                    });
                    ChatbotContentStore.LogMessage(new ChatbotMessageLog {
                        SessionGuid = sessionGuid,
                        NhanVienID  = MyAuthentication.ID,
                        CollectionId = collectionId,
                        SceneUuid    = sceneUuid,
                        Role = 1,
                        Content = string.IsNullOrEmpty(error) ? fullText.ToString() : ("[ERR] " + error),
                        Action = actionType == "navigate" ? ("navigate:" + actionTarget) : null,
                        TokensIn  = tokensIn,
                        TokensOut = tokensOut,
                        LatencyMs = (int)sw.ElapsedMilliseconds
                    });
                }
                catch { /* dont fail */ }

                // ===== Knowledge cache: Curator cham diem + (co the) luu - chay NEN, khong chan response =====
                if (kbEnabled && string.IsNullOrEmpty(error))
                {
                    var qCap   = message;
                    var rawAns = fullText.ToString();
                    var miSug  = rawAns.IndexOf(SUGG_SENT, StringComparison.Ordinal);
                    var aClean = (miSug >= 0 ? rawAns.Substring(0, miSug) : rawAns).TrimEnd('\r', '\n', ' ');
                    var actCap = actionType; var tgtCap = actionTarget; var suggCap = suggestions;
                    var collCap = collectionId; var sceneCap = sceneUuid;
                    var hadHist = history.Count > 1;   // co hoi thoai truoc -> Curator xet ky contextDependent
                    var curModel = ChatbotConfig.Get("CHATBOT_CURATOR_MODEL", "Chatbot.CuratorModel", "gpt-4.1-mini");
                    int curMin   = ChatbotConfig.GetInt("CHATBOT_KB_MIN_SCORE", "Chatbot.KbMinScore", 85);
                    if (!string.IsNullOrWhiteSpace(aClean))
                    {
                        System.Web.Hosting.HostingEnvironment.QueueBackgroundWorkItem(async ct =>
                        {
                            try
                            {
                                // Lenh DIEU HUONG: xac dinh + hay lap -> cache THANG (bo qua Curator).
                                // canonical = cau lenh user; target = uuid (serve se validate chong drift).
                                if (actCap == "navigate" && !string.IsNullOrEmpty(tgtCap))
                                {
                                    var navEmb = await new OpenAIEmbeddingClient().EmbedAsync(qCap);
                                    var navSugg = (suggCap != null && suggCap.Length > 0) ? string.Join("|", suggCap) : null;
                                    KnowledgeStore.Upsert(collCap, null, "navigate", qCap, aClean,
                                        "navigate", tgtCap, navSugg, "dieu-huong", null, 90, 1.0, "nav", navEmb);
                                    System.Diagnostics.Debug.WriteLine("[AskStream] KB cached NAV '" + qCap + "' -> " + tgtCap);
                                    return;
                                }

                                var cur = await new ChatbotCurator().EvaluateAsync(qCap, aClean, false, false, null, hadHist);
                                // Cache moi cau du tot + TU THAN DAY DU (khong phu thuoc hoi thoai truoc).
                                if (cur != null && cur.ShouldCache && !cur.Sensitive && !cur.ContextDependent && cur.QualityScore >= curMin)
                                {
                                    var canon = string.IsNullOrEmpty(cur.CanonicalQuestion) ? qCap : cur.CanonicalQuestion;
                                    var emb = await new OpenAIEmbeddingClient().EmbedAsync(canon);
                                    var suggPipe = (suggCap != null && suggCap.Length > 0) ? string.Join("|", suggCap) : null;
                                    var sceneScope = (cur.Scope == "scene") ? sceneCap : null;
                                    KnowledgeStore.Upsert(collCap, sceneScope, cur.Intent, canon, aClean,
                                        actCap, tgtCap, suggPipe, cur.Category, cur.Tags,
                                        cur.QualityScore, cur.Confidence, curModel, emb);
                                    System.Diagnostics.Debug.WriteLine("[AskStream] KB cached '" + canon + "' score=" + cur.QualityScore);
                                }
                                else if (cur != null)
                                    System.Diagnostics.Debug.WriteLine("[AskStream] KB skip cache: score=" + cur.QualityScore + " reason=" + cur.Reason);
                            }
                            catch (Exception exCur) { System.Diagnostics.Debug.WriteLine("[AskStream.KB curate] " + exCur.Message); }
                        });
                    }
                }
            });

            try { resp.End(); } catch { /* client may have disconnected */ }
        }

        // POST: User chat endpoint (non-streaming, giu lai cho fallback / test bang curl)
        [HttpPost]
        [SameOriginOnly]
        public async Task<ActionResult> Ask()
        {
            if (!Request.IsAuthenticated) return new HttpUnauthorizedResult();

            // Parse JSON body (Request.Form khong work cho application/json)
            string raw;
            using (var reader = new System.IO.StreamReader(Request.InputStream))
                raw = reader.ReadToEnd();
            JObject body;
            try { body = JObject.Parse(raw ?? "{}"); }
            catch { return Json(new { ok = false, error = "invalid JSON" }); }

            var collectionId = (string)body["collectionId"];
            var sceneUuid    = (string)body["sceneUuid"];
            var sessionGuidStr = (string)body["sessionGuid"];
            var message      = (string)body["message"];
            var historyArr   = body["history"] as JArray;

            if (string.IsNullOrEmpty(collectionId) || string.IsNullOrEmpty(message))
                return Json(new { ok = false, error = "missing collectionId or message" });
            if (message.Length > MAX_MESSAGE_LEN)
                return Json(new { ok = false, error = "Câu hỏi quá dài (max " + MAX_MESSAGE_LEN + " ký tự)" });

            Guid sessionGuid;
            if (!Guid.TryParse(sessionGuidStr, out sessionGuid)) sessionGuid = Guid.NewGuid();

            // Rate limit per user
            int rateLimit = ChatbotConfig.GetInt("CHATBOT_RATE_LIMIT_PER_HOUR", "Chatbot.RateLimitPerHour", 60);
            int recent = ChatbotContentStore.CountRecentUserMessages(MyAuthentication.ID, 60);
            if (recent >= rateLimit)
                return Json(new { ok = false, error = "Đã đạt giới hạn " + rateLimit + " câu/giờ. Vui lòng thử lại sau." });

            // Tran token/ngay (chong dot quota OpenAI)
            var dailyCapMsg = CheckDailyTokenCap(MyAuthentication.ID);
            if (dailyCapMsg != null)
                return Json(new { ok = false, error = dailyCapMsg });

            // Load tour info + scene info (cached 5min)
            var tourInfo = GetCached("chatbot_tour_" + collectionId,
                () => ChatbotContentStore.GetTourInfo(collectionId));
            if (tourInfo == null || !tourInfo.IsEnabled)
                return Json(new { ok = false, error = "Chatbot chưa được kích hoạt cho tour này." });

            var sceneInfo = GetCached("chatbot_scenes_" + collectionId,
                () => ChatbotContentStore.GetSceneInfo(collectionId))
                ?? new Dictionary<string, ChatbotSceneInfo>();

            // Load scene list tu Kuula (cached 24h o KuulaCollectionFetcher)
            var allScenes = KuulaCollectionFetcher.GetScenes(collectionId) ?? new List<KuulaCollectionFetcher.SceneInfo>();
            // Custom titles override
            var customTitles = SceneCalibrationStore.Get(collectionId)
                .Where(kv => !string.IsNullOrEmpty(kv.Value.CustomTitle))
                .ToDictionary(kv => kv.Key, kv => kv.Value.CustomTitle);

            // Danh sach diem da cau hinh -> enum tool + render prompt + map ten->uuid.
            var navList = BuildNavList(allScenes, sceneInfo, customTitles, sceneUuid);
            var navMap  = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var nv in navList) navMap[nv.Label] = nv.Uuid;

            // Build system prompt
            var sysPrompt = BuildSystemPrompt(tourInfo, sceneInfo, allScenes, customTitles, sceneUuid, navList);

            // Build history
            var history = new List<ChatTurn>();
            if (historyArr != null)
            {
                foreach (var h in historyArr.OfType<JObject>())
                {
                    var role = (string)h["role"];
                    var content = (string)h["content"];
                    if (string.IsNullOrEmpty(content)) continue;
                    history.Add(new ChatTurn { Role = role, Content = content });
                }
                // Cap toi da 10 turn de tranh prompt qua dai
                if (history.Count > 10) history = history.Skip(history.Count - 10).ToList();
            }

            var client = new OpenAIChatbotClient();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var reply = await client.SendAsync(new ChatbotRequest {
                SystemPrompt = sysPrompt,
                History      = history,
                UserMessage  = message,
                NavLabels    = navList.Select(x => x.Label).ToList()
            });
            sw.Stop();

            // Log both turns (user + assistant) - fire and forget OK
            try
            {
                ChatbotContentStore.LogMessage(new ChatbotMessageLog {
                    SessionGuid = sessionGuid,
                    NhanVienID  = MyAuthentication.ID,
                    CollectionId = collectionId,
                    SceneUuid    = sceneUuid,
                    Role = 0, Content = message
                });
                ChatbotContentStore.LogMessage(new ChatbotMessageLog {
                    SessionGuid = sessionGuid,
                    NhanVienID  = MyAuthentication.ID,
                    CollectionId = collectionId,
                    SceneUuid    = sceneUuid,
                    Role = 1,
                    Content = reply.Ok ? (reply.Text ?? "") : ("[ERR] " + (reply.Error ?? "unknown")),
                    Action = reply.ActionType == "navigate" ? ("navigate:" + reply.ActionTarget) : null,
                    TokensIn  = reply.TokensIn,
                    TokensOut = reply.TokensOut,
                    LatencyMs = (int)sw.ElapsedMilliseconds
                });
            }
            catch { /* dont fail request on log err */ }

            if (!reply.Ok)
                return Json(new { ok = false, error = reply.Error ?? "AI provider error" });

            // Resolve ten -> uuid: uu tien lookup ten chinh xac (tu enum), fallback FindSceneByQuery.
            string actionTarget = null, actionName = null;
            if (reply.ActionType == "navigate" && !string.IsNullOrEmpty(reply.ActionTarget))
            {
                if (navMap.TryGetValue(reply.ActionTarget.Trim(), out var exactUuid))
                {
                    actionTarget = exactUuid;
                    actionName = reply.ActionTarget.Trim();
                }
                else
                {
                    var matched = FindSceneByQuery(reply.ActionTarget, allScenes, sceneInfo, customTitles, sceneUuid);
                    actionTarget = matched.uuid;
                    actionName = matched.name;
                }
                if (actionTarget == null)
                {
                    // Khong tim thay -> reply text fallback, khong navigate
                    reply.Text = "Tôi không tìm thấy điểm \"" + reply.ActionTarget + "\" trong tour. Bạn có thể thử tên khác.";
                    reply.ActionType = null;
                }
            }

            return Json(new {
                ok = true,
                reply = reply.Text ?? "",
                action = (reply.ActionType == "navigate" && actionTarget != null)
                    ? (object)new { type = "navigate", target = actionTarget, name = actionName }
                    : null,
                tokensIn  = reply.TokensIn,
                tokensOut = reply.TokensOut,
                latencyMs = (int)sw.ElapsedMilliseconds
            });
        }

        // ==================================================================
        //   HELPERS: scene name + fuzzy match
        // ==================================================================

        /// <summary>Bo dau tieng Viet + lowercase de match accent-insensitive.</summary>
        private static string NormalizeForSearch(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var norm = s.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new StringBuilder(norm.Length);
            foreach (var ch in norm)
            {
                var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != System.Globalization.UnicodeCategory.NonSpacingMark) sb.Append(ch);
            }
            var result = sb.ToString().Normalize(System.Text.NormalizationForm.FormC)
                     .ToLowerInvariant()
                     .Replace('đ', 'd');
            // Gop khoang trang thua (space doi, tab, newline, nbsp) -> 1 space + trim.
            // Tranh truong hop title "Cang  tong hop" khong khop query "cang tong hop".
            return System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ").Trim();
        }

        /// <summary>True neu MOI tu trong tokens deu khop NGUYEN MOT TU trong text
        /// (text da normalize, cac tu cach nhau bang 1 space). Cho phep match cum nhieu tu
        /// khong can lien nhau / dung thu tu (vd "cang tong hop" khop "cang ... tong hop"),
        /// nhung KHONG match chuoi con giua tu (tranh "ho" lot vao "hop", "ca" lot vao "cang").
        /// Chi ap dung khi query co >= 2 tu.</summary>
        private static bool AllTokensContained(string[] tokens, string text)
        {
            if (tokens == null || tokens.Length < 2 || string.IsNullOrEmpty(text)) return false;
            var words = text.Split(' ');
            for (int i = 0; i < tokens.Length; i++)
            {
                bool found = false;
                for (int w = 0; w < words.Length; w++)
                {
                    if (words[w] == tokens[i]) { found = true; break; }
                }
                if (!found) return false;
            }
            return true;
        }

        /// <summary>Lay ten hien thi cho scene: CustomTitle (uu tien) > Kuula title.</summary>
        private static string GetSceneDisplayName(string uuid,
            Dictionary<string, string> customTitles,
            List<KuulaCollectionFetcher.SceneInfo> allScenes)
        {
            if (string.IsNullOrEmpty(uuid)) return null;
            if (customTitles != null && customTitles.TryGetValue(uuid, out var ct) && !string.IsNullOrEmpty(ct))
                return ct;
            var s = allScenes?.FirstOrDefault(x => x.Uuid == uuid);
            return s?.Title;
        }

        /// <summary>
        /// Tim scene match voi query nguoi dung (tu khoa tieng Viet).
        /// Match accent-insensitive, substring OK. Uu tien:
        ///   1. CustomTitle (Title hover override - admin set)
        ///   2. ShortIntro
        ///   3. Kuula default title
        /// Excludes current scene de tranh "navigate ve cho dang dung".
        /// </summary>
        private static (string uuid, string name) FindSceneByQuery(
            string query,
            List<KuulaCollectionFetcher.SceneInfo> allScenes,
            Dictionary<string, ChatbotSceneInfo> sceneInfo,
            Dictionary<string, string> customTitles,
            string currentSceneUuid)
        {
            if (string.IsNullOrEmpty(query) || allScenes == null) return (null, null);
            var q = NormalizeForSearch(query.Trim());
            if (q.Length < 2) return (null, null);
            // Tach tu de match "du tu khoa" khi cum lien khong khop (xem AllTokensContained).
            var qTokens = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                           .Where(t => t.Length >= 2).ToArray();

            string bestUuid = null;
            string bestName = null;
            int bestScore = 0;

            foreach (var s in allScenes)
            {
                if (string.IsNullOrEmpty(s.Uuid) || s.Uuid == currentSceneUuid) continue;

                int score = 0;
                // 1. CustomTitle - cao nhat
                if (customTitles != null && customTitles.TryGetValue(s.Uuid, out var ct) && !string.IsNullOrEmpty(ct))
                {
                    var ctN = NormalizeForSearch(ct);
                    if (ctN == q) score = Math.Max(score, 200);                  // exact
                    else if (ctN.StartsWith(q)) score = Math.Max(score, 150);    // prefix
                    else if (ctN.Contains(q)) score = Math.Max(score, 120);      // substring
                    else if (AllTokensContained(qTokens, ctN)) score = Math.Max(score, 110); // du tu khoa
                }
                // 2. ShortIntro
                if (sceneInfo != null && sceneInfo.TryGetValue(s.Uuid, out var info) && info != null)
                {
                    if (!string.IsNullOrEmpty(info.ShortIntro))
                    {
                        var n = NormalizeForSearch(info.ShortIntro);
                        if (n.Contains(q)) score = Math.Max(score, 80);
                        // KHONG token-match tren ShortIntro: la van xuoi dai -> de match bua.
                    }
                }
                // 3. Kuula title - thap nhat (thuong la filename xau)
                if (!string.IsNullOrEmpty(s.Title))
                {
                    var tN = NormalizeForSearch(s.Title);
                    if (tN.Contains(q)) score = Math.Max(score, 30);
                    else if (AllTokensContained(qTokens, tN)) score = Math.Max(score, 25);
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestUuid  = s.Uuid;
                    bestName  = GetSceneDisplayName(s.Uuid, customTitles, allScenes);
                }
            }
            return (bestUuid, bestName);
        }

        /// <summary>
        /// Sau khi navigate -> 3 suggestion contextual cho scene dich.
        /// Su dung scene name + xem co content hay khong de generate cau hoi phu hop.
        /// </summary>
        private static string[] BuildNavigateSuggestions(string uuid, string sceneName,
            Dictionary<string, ChatbotSceneInfo> sceneInfo)
        {
            var name = string.IsNullOrEmpty(sceneName) ? "điểm này" : sceneName;
            var hasDetail = sceneInfo != null
                && sceneInfo.TryGetValue(uuid, out var info)
                && info != null
                && !string.IsNullOrEmpty(info.DetailContent);
            if (hasDetail)
            {
                return new[] {
                    "Giới thiệu về " + name,
                    "Có gì đặc biệt?",
                    "Đi tới khu khác"
                };
            }
            return new[] {
                "Tour này có những khu nào?",
                "Khu vực nổi bật khác?",
                "Đi tới khu vực khác"
            };
        }

        /// <summary>1 diem dieu huong: ten hien thi (duy nhat) + uuid + noi dung.</summary>
        private sealed class NavItem
        {
            public string Uuid { get; set; }
            public string Label { get; set; }
            public string ShortIntro { get; set; }
            public string Detail { get; set; }
        }

        /// <summary>
        /// Danh sach diem DA CAU HINH (co CustomTitle hoac co ShortIntro/DetailContent) de:
        ///   - render "DANH SACH CAC DIEM" trong system prompt
        ///   - lam enum `scene` cho tool navigate_to_scene (LLM chon dung 1 ten, khong bia)
        ///   - map Label -> uuid khi resolve tool call (lookup thang, khong fuzzy)
        /// Label duy nhat (trung thi them " (2)"), loai tru scene hien tai, uu tien co CustomTitle, cap 60.
        /// </summary>
        private static List<NavItem> BuildNavList(
            List<KuulaCollectionFetcher.SceneInfo> allScenes,
            Dictionary<string, ChatbotSceneInfo> sceneInfo,
            Dictionary<string, string> customTitles,
            string currentSceneUuid)
        {
            var list = new List<NavItem>();
            if (allScenes == null) return list;
            var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Func<string, bool> hasName = uuid =>
                customTitles != null && customTitles.TryGetValue(uuid, out var ct) && !string.IsNullOrEmpty(ct);
            Func<string, ChatbotSceneInfo> infoOf = uuid =>
                (sceneInfo != null && sceneInfo.TryGetValue(uuid, out var ci)) ? ci : null;
            Func<string, bool> hasInfo = uuid =>
            {
                var ci = infoOf(uuid);
                return ci != null && (!string.IsNullOrEmpty(ci.ShortIntro) || !string.IsNullOrEmpty(ci.DetailContent));
            };

            var ordered = allScenes
                .Where(s => !string.IsNullOrEmpty(s.Uuid) && s.Uuid != currentSceneUuid)
                .Where(s => hasName(s.Uuid) || hasInfo(s.Uuid))      // CHI diem da cau hinh
                .OrderByDescending(s => hasName(s.Uuid) ? 1 : 0);

            foreach (var s in ordered)
            {
                var baseLabel = (hasName(s.Uuid) ? customTitles[s.Uuid] : s.Title) ?? "Điểm";
                baseLabel = baseLabel.Trim();
                if (baseLabel.Length == 0) baseLabel = "Điểm";
                var label = baseLabel;
                int dup = 2;
                while (used.Contains(label)) { label = baseLabel + " (" + dup + ")"; dup++; }
                used.Add(label);

                var info = infoOf(s.Uuid);
                list.Add(new NavItem {
                    Uuid = s.Uuid,
                    Label = label,
                    ShortIntro = info?.ShortIntro,
                    Detail = info?.DetailContent
                });
                if (list.Count >= 60) break;
            }
            return list;
        }

        private static string BuildSystemPrompt(
            ChatbotTourInfo tourInfo,
            Dictionary<string, ChatbotSceneInfo> sceneInfo,
            List<KuulaCollectionFetcher.SceneInfo> allScenes,
            Dictionary<string, string> customTitles,
            string currentSceneUuid,
            List<NavItem> navList)
        {
            var sb = new System.Text.StringBuilder();

            // ==== Persona ====
            sb.AppendLine("Bạn là HƯỚNG DẪN VIÊN ẢO của Khu Liên Hợp HPDQ, hỗ trợ người dùng đang xem tour 360° trên hệ thống nội bộ.");
            sb.AppendLine();
            // ==== NAVIGATION RULES - DAT LEN DAU + EMPHATIC ====
            sb.AppendLine("=== QUY TẮC #1 — NAVIGATION (QUAN TRỌNG NHẤT) ===");
            sb.AppendLine("Khi câu hỏi của user có Ý ĐỊNH DI CHUYỂN tới điểm/khu vực khác → BẮT BUỘC gọi function `navigate_to_scene` với:");
            sb.AppendLine("  - `scene`: TÊN CHÍNH XÁC của điểm đích — CHỌN ĐÚNG MỘT tên trong '=== DANH SÁCH CÁC ĐIỂM TRONG TOUR ===' bên dưới (chép NGUYÊN VĂN, KHÔNG tự chế tên mới, KHÔNG dịch).");
            sb.AppendLine("  - `reason`: câu xác nhận ngắn (vd 'Đang đưa bạn tới Cảng tổng hợp.').");
            sb.AppendLine("Nếu nơi user muốn tới KHÔNG có trong danh sách → ĐỪNG gọi navigate; hãy trả lời rằng tour chưa có điểm đó.");
            sb.AppendLine();
            sb.AppendLine("Trigger phrases chỉ ý định di chuyển:");
            sb.AppendLine("  - 'chuyển tới X', 'đi tới X', 'qua X', 'đến X', 'tới X', 'sang X'");
            sb.AppendLine("  - 'đưa tôi tới X', 'dẫn tôi tới X', 'cho tôi xem X', 'mở X', 'hiện X'");
            sb.AppendLine("  - 'tôi muốn xem X', 'tôi muốn đến X'");
            sb.AppendLine("  - HỎI VỊ TRÍ điểm CÓ trong danh sách: 'X ở đâu', 'X nằm ở đâu', 'vị trí của X', 'tới X bằng cách nào', 'đường tới X'");
            sb.AppendLine("    → Đây là tour 360, ĐƯA TỚI TẬN NƠI chính là cách chỉ vị trí → COI LÀ Ý ĐỊNH DI CHUYỂN, gọi navigate_to_scene tới X.");
            sb.AppendLine();
            sb.AppendLine("KHÔNG NAVIGATE khi user chỉ HỎI THÔNG TIN về một điểm (dù câu hỏi có nhắc tên điểm đó):");
            sb.AppendLine("  - 'X có đặc điểm gì?', 'X có gì?', 'giới thiệu về X', 'thông tin về X', 'mô tả X', 'X là gì?', 'X rộng/lớn bao nhiêu?', 'kể về X', 'X hoạt động thế nào?'");
            sb.AppendLine("  → Đây là câu hỏi THÔNG TIN: phải TRẢ LỜI bằng text theo QUY TẮC #2. TUYỆT ĐỐI KHÔNG gọi navigate_to_scene.");
            sb.AppendLine("LƯU Ý: user NHẮC TÊN một điểm KHÔNG phải là ý định di chuyển. Chỉ navigate khi user rõ ràng muốn ĐI/CHUYỂN/XEM TẬN NƠI (đúng các trigger phía trên).");
            sb.AppendLine();
            sb.AppendLine("=== QUY TẮC #2 — TRẢ LỜI CÂU HỎI ===");
            sb.AppendLine("- LUÔN trả lời bằng tiếng Việt, ngắn gọn (2-4 câu), thân thiện.");
            sb.AppendLine("- CHỈ dựa trên thông tin được cung cấp. KHÔNG bịa số liệu, không suy đoán.");
            sb.AppendLine("- Nếu không có thông tin để trả lời câu hỏi: 'Tôi chưa được cung cấp thông tin về điều này. Bạn có thể hỏi quản trị viên.'");
            sb.AppendLine("- NGOẠI LỆ về VỊ TRÍ: nếu user hỏi 'X ở đâu / nằm đâu / vị trí X' mà X CÓ trong DANH SÁCH CÁC ĐIỂM → TUYỆT ĐỐI KHÔNG nói 'chưa có thông tin về vị trí'; hãy gọi navigate_to_scene đưa tới X (xem QUY TẮC #1).");
            sb.AppendLine("- KHÔNG trả lời ngoài chủ đề Khu Liên Hợp Hòa Phát Dung Quất và tour này.");
            sb.AppendLine("- 'ở đây / tại đây / chỗ này / khu này / nơi này / điểm này' LUÔN chỉ ĐIỂM NGƯỜI DÙNG ĐANG XEM (mục '=== ĐIỂM NGƯỜI DÙNG ĐANG XEM ==='), KHÔNG phải điểm vừa nhắc ở câu hỏi trước.");
            sb.AppendLine("- User có thể hỏi về BẤT KỲ điểm nào trong '=== DANH SÁCH CÁC ĐIỂM TRONG TOUR ===' (không riêng điểm đang xem). Hãy dùng phần 'Chi tiết' của ĐÚNG điểm user hỏi để trả lời — KHÔNG nói 'chưa có thông tin' nếu điểm đó có mô tả trong danh sách.");
            sb.AppendLine("- ĐẾM / LIỆT KÊ các điểm dựa trên DANH SÁCH CÁC ĐIỂM bên dưới KHÔNG phải là bịa số liệu: khi user hỏi 'tour có bao nhiêu khu/điểm', 'có những khu nào' → hãy đếm và trả lời theo danh sách đó.");
            sb.AppendLine();
            sb.AppendLine("=== QUY TẮC #2.5 — KHÔNG VIẾT TẮT (câu trả lời có thể được TTS đọc to) ===");
            sb.AppendLine("- KHÔNG dùng từ viết tắt. Luôn viết đầy đủ tiếng Việt:");
            sb.AppendLine("  + 'KLH'    → 'Khu Liên Hợp'");
            sb.AppendLine("  + 'HPDQ'   → 'Hòa Phát Dung Quất'");
            sb.AppendLine("  + 'KLH HPDQ' → 'Khu Liên Hợp Hòa Phát Dung Quất'");
            sb.AppendLine("  + 'CN', 'CT', 'Cty' → 'Công ty'");
            sb.AppendLine("  + 'TNHH' → 'Trách Nhiệm Hữu Hạn'");
            sb.AppendLine("  + 'KV', 'KCN' → 'Khu vực', 'Khu Công Nghiệp'");
            sb.AppendLine("  + 'SX' → 'sản xuất'; 'NM' → 'nhà máy'; 'CB-CNV' → 'cán bộ công nhân viên'");
            sb.AppendLine("- Số viết bằng chữ khi ngắn (1-10), bằng số khi dài (>=11). VD: 'năm 2019' (giữ), 'hai cảng' (KHÔNG '2 cảng').");
            sb.AppendLine("- Đơn vị đo: viết đầy đủ. 'm²' → 'mét vuông', 'm³' → 'mét khối', 'km' → 'ki-lô-mét', 'tấn/tháng' → 'tấn mỗi tháng'.");
            sb.AppendLine();
            sb.AppendLine("=== QUY TẮC #3 — GỢI Ý CÂU HỎI TIẾP THEO ===");
            sb.AppendLine("Khi trả lời TEXT (không phải gọi navigate_to_scene), BẮT BUỘC kết thúc câu trả lời bằng MỘT DÒNG TRỐNG, rồi DÒNG cuối có cấu trúc:");
            sb.AppendLine("---SUGGEST---");
            sb.AppendLine("Câu hỏi gợi ý 1|Câu hỏi gợi ý 2|Câu hỏi gợi ý 3");
            sb.AppendLine();
            sb.AppendLine("Quy tắc gợi ý:");
            sb.AppendLine("- Đúng 3 câu hỏi, ngăn cách bởi dấu | (pipe).");
            sb.AppendLine("- Mỗi câu ngắn (max 30 ký tự), tiếng Việt tự nhiên, không lặp lại câu user vừa hỏi.");
            sb.AppendLine("- Phù hợp ngữ cảnh: hỏi sâu hơn về scene hiện tại, hoặc gợi ý đi tới scene liên quan, hoặc thông tin tour.");
            sb.AppendLine("- VD: 'Có gì đặc biệt?', 'Hoạt động chính ở đây?', 'Đi tới khu kho', 'Cảng có gì?', 'Tour này có bao nhiêu khu?'");
            sb.AppendLine("- KHI gọi navigate_to_scene → BỎ QUA dòng ---SUGGEST--- (server tự thêm).");
            sb.AppendLine();

            // ==== Tour overview ====
            if (!string.IsNullOrEmpty(tourInfo?.Overview))
            {
                sb.AppendLine("=== GIỚI THIỆU TỔNG QUAN TOUR ===");
                sb.AppendLine(tourInfo.Overview);
                sb.AppendLine();
            }

            // ==== Current scene ====
            if (!string.IsNullOrEmpty(currentSceneUuid))
            {
                var curName = customTitles.ContainsKey(currentSceneUuid) ? customTitles[currentSceneUuid] : null;
                if (string.IsNullOrEmpty(curName))
                {
                    var s = allScenes.FirstOrDefault(x => x.Uuid == currentSceneUuid);
                    curName = s?.Title ?? "(scene không có tên)";
                }
                sb.AppendLine("=== ĐIỂM NGƯỜI DÙNG ĐANG XEM ===");
                sb.AppendLine("Tên: " + curName);
                sb.AppendLine("UUID: " + currentSceneUuid);
                if (sceneInfo.ContainsKey(currentSceneUuid))
                {
                    var ci = sceneInfo[currentSceneUuid];
                    if (!string.IsNullOrEmpty(ci.ShortIntro))    sb.AppendLine("Giới thiệu: " + ci.ShortIntro);
                    if (!string.IsNullOrEmpty(ci.DetailContent)) sb.AppendLine("Chi tiết:\n" + ci.DetailContent);
                }
                else
                {
                    sb.AppendLine("(Chưa có nội dung cấu hình cho điểm này.)");
                }
                sb.AppendLine();
            }

            // ==== Danh sach diem da cau hinh (navList) — vua la ngu canh tra loi, vua la
            //      tap hop ten hop le cho `scene` cua navigate_to_scene (LLM phai chon dung 1 ten). ====
            sb.AppendLine("=== DANH SÁCH CÁC ĐIỂM TRONG TOUR ===");
            sb.AppendLine("(Đây là TẤT CẢ điểm có thể tới. Khi navigate, `scene` PHẢI là một trong các tên dưới đây — chép đúng nguyên văn.)");
            if (navList == null || navList.Count == 0)
            {
                sb.AppendLine("(Chưa có điểm nào được cấu hình để điều hướng.)");
            }
            else
            {
                foreach (var item in navList)
                {
                    sb.Append("- ").Append(item.Label);
                    if (!string.IsNullOrEmpty(item.ShortIntro))
                        sb.Append(" — ").Append(item.ShortIntro);
                    // Kèm "Chi tiết" (cắt 600 ký tự) để bot trả lời được câu hỏi về điểm KHÔNG phải
                    // điểm đang xem (vd đang ở Cảng nhưng hỏi "Tòa nhà hành chính có gì").
                    if (!string.IsNullOrEmpty(item.Detail))
                    {
                        var detail = item.Detail.Trim();
                        if (detail.Length > 600) detail = detail.Substring(0, 600) + "…";
                        sb.AppendLine();
                        sb.Append("    Chi tiết: ").Append(detail);
                    }
                    sb.AppendLine();
                }
            }

            // ==== Optional admin-defined override ====
            if (!string.IsNullOrEmpty(tourInfo?.SystemPrompt))
            {
                sb.AppendLine();
                sb.AppendLine("=== CHỈ DẪN BỔ SUNG TỪ QUẢN TRỊ VIÊN ===");
                sb.AppendLine(tourInfo.SystemPrompt);
            }
            return sb.ToString();
        }
    }
}
