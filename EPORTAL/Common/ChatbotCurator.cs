using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EPORTAL.Common
{
    public class CurationResult
    {
        public bool   ShouldCache        { get; set; }
        public int    QualityScore       { get; set; }   // 0-100
        public double Confidence         { get; set; }   // 0-1
        public string Intent             { get; set; }   // info | navigate | smalltalk
        public string Category           { get; set; }
        public string CanonicalQuestion  { get; set; }
        public string Tags               { get; set; }   // csv
        public bool   Sensitive          { get; set; }
        public string Scope              { get; set; }   // tour | scene  (tai dung moi noi hay rieng scene hien tai)
        public bool   ContextDependent   { get; set; }   // true = phu thuoc hoi thoai truoc -> KHONG cache
        public string Reason             { get; set; }
    }

    /// <summary>
    /// LLM Curator: cham diem + phan loai + quyet dinh CACHE cau tra loi (chay async sau moi luot MISS).
    /// Chi cache khi: chinh xac & bam du lieu cung cap, day du, huu ich, KHONG nhay cam, KHONG la cau tu choi.
    /// (Phase 3 - knowledge-cache plan)
    /// </summary>
    public class ChatbotCurator
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
        private const string ENDPOINT = "https://api.openai.com/v1/chat/completions";

        public async Task<CurationResult> EvaluateAsync(string question, string answer, bool hadAction, bool hadError, string groundingHint, bool hadHistory = false)
        {
            if (string.IsNullOrWhiteSpace(question) || string.IsNullOrWhiteSpace(answer)) return null;
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey)) return null;
                if (ServiceHealth.IsDown(ServiceHealth.OPENAI)) return null;   // down -> khong cham diem, chi bo qua viec cache
                var model = ChatbotConfig.Get("CHATBOT_CURATOR_MODEL", "Chatbot.CuratorModel", "gpt-4.1-mini");

                var sys = new StringBuilder();
                sys.AppendLine("Bạn là TRỌNG TÀI CHẤT LƯỢNG cho chatbot hướng dẫn viên ảo của Khu Liên Hợp Hòa Phát Dung Quất.");
                sys.AppendLine("Nhiệm vụ: đánh giá 1 cặp (câu hỏi user → câu trả lời bot) có nên LƯU CACHE để tái dùng cho người khác không.");
                sys.AppendLine("CHỈ nên cache (shouldCache=true, qualityScore cao) khi câu trả lời:");
                sys.AppendLine("  - Chính xác & bám đúng dữ liệu được cung cấp (KHÔNG bịa, không suy đoán).");
                sys.AppendLine("  - Đầy đủ, rõ ràng, hữu ích, đúng chủ đề tour.");
                sys.AppendLine("  - KHÔNG phụ thuộc thời điểm / ngữ cảnh cá nhân / hội thoại trước.");
                sys.AppendLine("KHÔNG cache (shouldCache=false) khi: câu từ chối ('tôi chưa được cung cấp thông tin…'), có dấu hiệu bịa,");
                sys.AppendLine("  trả lời lỗi, nhạy cảm/riêng tư, hoặc chất lượng thấp.");
                sys.AppendLine("`canonicalQuestion`: viết lại câu hỏi thành dạng CHUẨN ngắn gọn nhưng GIỮ NGUYÊN tiếng Việt CÓ DẤU và giữ ý gốc (chỉ bỏ từ thừa, KHÔNG bỏ dấu, KHÔNG viết tắt).");
                sys.AppendLine("`intent`: 'navigate' nếu là yêu cầu di chuyển; 'smalltalk' nếu chào hỏi/vu vơ; còn lại 'info'.");
                sys.AppendLine("`scope`: 'scene' nếu câu trả lời phụ thuộc ĐIỂM ĐANG XEM ('ở đây/điểm này/chỗ này'); 'tour' nếu áp dụng chung toàn tour (giới thiệu tổng quan, hỏi về điểm có tên cụ thể, điều hướng).");
                sys.AppendLine("`contextDependent`: true nếu câu hỏi/câu trả lời PHỤ THUỘC HỘI THOẠI TRƯỚC (câu hỏi cụt như 'còn cái đó?', 'nó thế nào?', 'thêm nữa'; hoặc câu trả lời tham chiếu 'như đã nói', đại từ chỉ thứ chưa nêu rõ). false nếu cặp hỏi-đáp TỰ THÂN ĐẦY ĐỦ (hiểu được mà không cần lịch sử). KHÔNG cache khi contextDependent=true.");

                var user = new StringBuilder();
                user.AppendLine("CÂU HỎI: " + question);
                user.AppendLine("CÂU TRẢ LỜI: " + answer);
                user.AppendLine("Có hành động điều hướng: " + (hadAction ? "có" : "không"));
                user.AppendLine("Có hội thoại trước lượt này: " + (hadHistory ? "có" : "không") + " (nếu có, xét kỹ contextDependent).");
                user.AppendLine("Lượt này bị lỗi: " + (hadError ? "có" : "không"));
                if (!string.IsNullOrEmpty(groundingHint)) user.AppendLine("Ngữ cảnh dữ liệu (tóm tắt): " + groundingHint);

                var tool = new JObject {
                    ["type"] = "function",
                    ["function"] = new JObject {
                        ["name"] = "submit_evaluation",
                        ["description"] = "Nộp kết quả đánh giá chất lượng + phân loại câu trả lời.",
                        ["parameters"] = new JObject {
                            ["type"] = "object",
                            ["properties"] = new JObject {
                                ["shouldCache"]       = new JObject { ["type"] = "boolean" },
                                ["qualityScore"]      = new JObject { ["type"] = "integer", ["description"] = "0-100" },
                                ["confidence"]        = new JObject { ["type"] = "number",  ["description"] = "0.0-1.0" },
                                ["intent"]            = new JObject { ["type"] = "string", ["enum"] = new JArray { "info", "navigate", "smalltalk" } },
                                ["category"]          = new JObject { ["type"] = "string", ["description"] = "vd: tong-quan, ky-thuat, dieu-huong, gioi-thieu" },
                                ["canonicalQuestion"] = new JObject { ["type"] = "string" },
                                ["tags"]              = new JObject { ["type"] = "string", ["description"] = "vài tag, ngăn bởi dấu phẩy" },
                                ["sensitive"]         = new JObject { ["type"] = "boolean" },
                                ["scope"]             = new JObject { ["type"] = "string", ["enum"] = new JArray { "tour", "scene" } },
                                ["contextDependent"]  = new JObject { ["type"] = "boolean" },
                                ["reason"]            = new JObject { ["type"] = "string" }
                            },
                            ["required"] = new JArray { "shouldCache", "qualityScore", "confidence", "intent", "canonicalQuestion", "sensitive", "scope", "contextDependent" }
                        }
                    }
                };

                var payload = new JObject {
                    ["model"] = model,
                    ["temperature"] = 0,
                    ["max_tokens"] = 300,
                    ["messages"] = new JArray {
                        new JObject { ["role"] = "system", ["content"] = sys.ToString() },
                        new JObject { ["role"] = "user",   ["content"] = user.ToString() }
                    },
                    ["tools"] = new JArray { tool },
                    ["tool_choice"] = new JObject {
                        ["type"] = "function",
                        ["function"] = new JObject { ["name"] = "submit_evaluation" }
                    }
                };

                using (var msg = new HttpRequestMessage(HttpMethod.Post, ENDPOINT))
                {
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    msg.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                    using (var resp = await _http.SendAsync(msg).ConfigureAwait(false))
                    {
                        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!resp.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine("[Curator] OpenAI " + (int)resp.StatusCode);
                            return null;
                        }
                        var j = JObject.Parse(body);
                        var argsRaw = (string)j["choices"]?[0]?["message"]?["tool_calls"]?[0]?["function"]?["arguments"];
                        if (string.IsNullOrEmpty(argsRaw)) return null;
                        var a = JObject.Parse(argsRaw);
                        return new CurationResult {
                            ShouldCache       = (bool?)a["shouldCache"] ?? false,
                            QualityScore      = (int?)a["qualityScore"] ?? 0,
                            Confidence        = (double?)a["confidence"] ?? 0,
                            Intent            = (string)a["intent"],
                            Category          = (string)a["category"],
                            CanonicalQuestion = (string)a["canonicalQuestion"],
                            Tags              = (string)a["tags"],
                            Sensitive         = (bool?)a["sensitive"] ?? false,
                            Scope             = (string)a["scope"],
                            ContextDependent  = (bool?)a["contextDependent"] ?? false,
                            Reason            = (string)a["reason"]
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                if (ServiceHealth.IsConnectivityError(ex)) ServiceHealth.MarkDown(ServiceHealth.OPENAI);
                System.Diagnostics.Debug.WriteLine("[Curator] " + ex.Message);
                return null;
            }
        }
    }
}
