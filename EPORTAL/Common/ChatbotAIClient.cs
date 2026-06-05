using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EPORTAL.Common
{
    public class ChatTurn
    {
        public string Role    { get; set; } // "user" | "assistant"
        public string Content { get; set; }
    }

    public class ChatbotRequest
    {
        public string         SystemPrompt { get; set; }
        public List<ChatTurn> History      { get; set; }
        public string         UserMessage  { get; set; }
        // Danh sach TEN diem da cau hinh -> dung lam enum cho tool navigate_to_scene.
        // Model chi duoc chon dung 1 ten trong day (khong tu bia keyword). Null/rong -> tool kieu cu (query).
        public List<string>   NavLabels    { get; set; }
    }

    public class ChatbotReply
    {
        public string Text         { get; set; }
        public string ActionType   { get; set; } // "navigate" | null
        public string ActionTarget { get; set; } // sceneUuid
        public int    TokensIn     { get; set; }
        public int    TokensOut    { get; set; }
        public bool   Ok           { get; set; }
        public string Error        { get; set; }
    }

    public interface IChatbotAIClient
    {
        Task<ChatbotReply> SendAsync(ChatbotRequest req);
        Task StreamAsync(ChatbotRequest req,
            Action<string> onTextDelta,
            Action<string, string> onToolCall,   // (name, argsJson) - full args after stream done
            Action<int, int, string> onComplete); // (tokensIn, tokensOut, error)
    }

    /// <summary>
    /// OpenAI Chat Completions implementation - su dung function calling de bot tu
    /// trigger navigate_to_scene thay vi parse text. Model + maxTokens cau hinh trong Web.config.
    /// API key doc tu OPENAI_API_KEY environment variable (load tu .env qua Global.asax.cs).
    /// </summary>
    public class OpenAIChatbotClient : IChatbotAIClient
    {
        // Static HttpClient - tranh socket exhaustion.
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(45)
        };

        private const string ENDPOINT = "https://api.openai.com/v1/chat/completions";

        // Tool spec dung chung cho SendAsync + StreamAsync (Chat Completions API format).
        // RealtimeSession (Realtime API) co schema khac - khong dung helper nay.
        //
        // Khi co navLabels (danh sach ten diem da cau hinh): tham so `scene` la ENUM cac ten do
        // -> model BUOC phai chon dung 1 ten co san (khong bia keyword, het lop bug fuzzy-match).
        // Khong co navLabels: fallback tool kieu cu nhan `query` (tu khoa tu do) -> server tu match.
        private static JArray BuildTools(List<string> navLabels)
        {
            JObject targetParam;
            JArray required;
            if (navLabels != null && navLabels.Count > 0)
            {
                var en = new JArray();
                foreach (var l in navLabels) en.Add(l);
                targetParam = new JObject {
                    ["scene"] = new JObject {
                        ["type"] = "string",
                        ["description"] = "TÊN CHÍNH XÁC của điểm đích — phải chọn đúng MỘT tên trong danh sách điểm của tour (không tự bịa, không dịch).",
                        ["enum"] = en
                    },
                    ["reason"] = new JObject {
                        ["type"] = "string",
                        ["description"] = "Câu xác nhận ngắn 1 dòng tiếng Việt, vd 'Đang đưa bạn tới Cảng tổng hợp.'"
                    }
                };
                required = new JArray { "scene", "reason" };
            }
            else
            {
                targetParam = new JObject {
                    ["query"] = new JObject {
                        ["type"] = "string",
                        ["description"] = "Từ khoá tiếng Việt user dùng để chỉ điểm đích (vd 'cảng', 'khu sản xuất'). KHÔNG dịch sang tiếng Anh."
                    },
                    ["reason"] = new JObject {
                        ["type"] = "string",
                        ["description"] = "Câu xác nhận ngắn 1 dòng tiếng Việt, vd 'Đang đưa bạn tới khu vực cảng.'"
                    }
                };
                required = new JArray { "query", "reason" };
            }

            return new JArray {
                new JObject {
                    ["type"] = "function",
                    ["function"] = new JObject {
                        ["name"] = "navigate_to_scene",
                        ["description"] = "GỌI FUNCTION NÀY khi user thể hiện ý định DI CHUYỂN tới điểm/khu vực khác trong tour 360° ('đi tới X', 'qua X', 'đưa tôi tới X', 'cho tôi xem X', 'mở X'...). KHÔNG gọi khi user chỉ HỎI THÔNG TIN về một điểm.",
                        ["parameters"] = new JObject {
                            ["type"] = "object",
                            ["properties"] = targetParam,
                            ["required"] = required
                        }
                    }
                }
            };
        }

        public async Task<ChatbotReply> SendAsync(ChatbotRequest req)
        {
            var reply = new ChatbotReply();
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    reply.Error = "OPENAI_API_KEY khong duoc set (kiem tra .env hoac IIS env var)";
                    return reply;
                }
                var model  = ChatbotConfig.Get("CHATBOT_MODEL", "Chatbot.Model", "gpt-4o-mini");
                int maxTok = ChatbotConfig.GetInt("CHATBOT_MAX_TOKENS", "Chatbot.MaxTokens", 800);

                var messages = new JArray();
                if (!string.IsNullOrEmpty(req.SystemPrompt))
                    messages.Add(new JObject { ["role"] = "system", ["content"] = req.SystemPrompt });
                if (req.History != null)
                {
                    foreach (var t in req.History)
                    {
                        if (string.IsNullOrEmpty(t.Content)) continue;
                        messages.Add(new JObject {
                            ["role"]    = t.Role == "assistant" ? "assistant" : "user",
                            ["content"] = t.Content
                        });
                    }
                }
                messages.Add(new JObject { ["role"] = "user", ["content"] = req.UserMessage ?? "" });

                var tools = BuildTools(req.NavLabels);

                var payload = new JObject {
                    ["model"]       = model,
                    ["messages"]    = messages,
                    ["tools"]       = tools,
                    ["max_tokens"]  = maxTok,
                    ["temperature"] = 0.5
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
                            reply.Error = "OpenAI " + (int)resp.StatusCode + ": " + TruncateError(body);
                            return reply;
                        }
                        var j = JObject.Parse(body);
                        var choice = (j["choices"] as JArray)?[0] as JObject;
                        var message = choice?["message"] as JObject;
                        if (message != null)
                        {
                            reply.Text = (string)message["content"] ?? "";

                            // Tool call -> set navigate action
                            var toolCalls = message["tool_calls"] as JArray;
                            if (toolCalls != null && toolCalls.Count > 0)
                            {
                                var fn = toolCalls[0]?["function"] as JObject;
                                var fnName = (string)fn?["name"];
                                System.Diagnostics.Debug.WriteLine("[OpenAIChatbotClient] tool call: " + fnName + " args=" + fn?["arguments"]);
                                if (fnName == "navigate_to_scene")
                                {
                                    var argsRaw = (string)fn["arguments"];
                                    try
                                    {
                                        var args = JObject.Parse(argsRaw ?? "{}");
                                        // Tool moi dung `scene` (ten chinh xac tu enum); fallback `query` (tool cu).
                                        var query = (string)args["scene"] ?? (string)args["query"];
                                        var reason = (string)args["reason"];
                                        if (!string.IsNullOrEmpty(query))
                                        {
                                            reply.ActionType   = "navigate";
                                            // ActionTarget luu TEN/keyword - controller resolve thanh uuid (exact map > FindSceneByQuery)
                                            reply.ActionTarget = query;
                                            reply.Text = !string.IsNullOrEmpty(reason)
                                                ? reason
                                                : "Đang đưa bạn tới điểm đó...";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine("[OpenAIChatbotClient] parse tool args err: " + ex.Message);
                                    }
                                }
                            }
                        }

                        var usage = j["usage"] as JObject;
                        if (usage != null)
                        {
                            reply.TokensIn  = (int?)usage["prompt_tokens"]     ?? 0;
                            reply.TokensOut = (int?)usage["completion_tokens"] ?? 0;
                        }
                        if (string.IsNullOrEmpty(reply.Text) && string.IsNullOrEmpty(reply.ActionType))
                            reply.Text = "Xin lỗi, tôi chưa có câu trả lời cho câu hỏi này.";
                        reply.Ok = true;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                reply.Error = "Request timeout (>45s)";
            }
            catch (Exception ex)
            {
                reply.Error = "Exception: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("[OpenAIChatbotClient] " + ex);
            }
            return reply;
        }

        private static string TruncateError(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Length > 400 ? s.Substring(0, 400) + "..." : s;
        }

        // ============================================================
        //  Streaming variant - SSE chunks tu OpenAI, fan ra callbacks.
        //  onTextDelta: goi cho moi chunk text moi (incremental).
        //  onToolCall:  goi 1 lan khi stream xong, accumulate args day du.
        //  onComplete:  goi sau khi stream done (cap usage neu OpenAI gui).
        // ============================================================
        public async Task StreamAsync(ChatbotRequest req,
            Action<string> onTextDelta,
            Action<string, string> onToolCall,
            Action<int, int, string> onComplete)
        {
            string error = null;
            int tokensIn = 0, tokensOut = 0;
            string toolName = null;
            var toolArgsBuf = new StringBuilder();
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey))
                {
                    error = "OPENAI_API_KEY khong duoc set";
                    onComplete?.Invoke(0, 0, error);
                    return;
                }
                var model  = ChatbotConfig.Get("CHATBOT_MODEL", "Chatbot.Model", "gpt-4.1-mini");
                int maxTok = ChatbotConfig.GetInt("CHATBOT_MAX_TOKENS", "Chatbot.MaxTokens", 800);

                var messages = new JArray();
                if (!string.IsNullOrEmpty(req.SystemPrompt))
                    messages.Add(new JObject { ["role"] = "system", ["content"] = req.SystemPrompt });
                if (req.History != null)
                {
                    foreach (var t in req.History)
                    {
                        if (string.IsNullOrEmpty(t.Content)) continue;
                        messages.Add(new JObject {
                            ["role"]    = t.Role == "assistant" ? "assistant" : "user",
                            ["content"] = t.Content
                        });
                    }
                }
                messages.Add(new JObject { ["role"] = "user", ["content"] = req.UserMessage ?? "" });

                var tools = BuildTools(req.NavLabels);

                var payload = new JObject {
                    ["model"]       = model,
                    ["messages"]    = messages,
                    ["tools"]       = tools,
                    ["max_tokens"]  = maxTok,
                    ["temperature"] = 0.5,
                    ["stream"]      = true,
                    ["stream_options"] = new JObject { ["include_usage"] = true }
                };

                using (var msg = new HttpRequestMessage(HttpMethod.Post, ENDPOINT))
                {
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    msg.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");

                    using (var resp = await _http.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        if (!resp.IsSuccessStatusCode)
                        {
                            var bodyErr = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                            error = "OpenAI " + (int)resp.StatusCode + ": " + TruncateError(bodyErr);
                            onComplete?.Invoke(0, 0, error);
                            return;
                        }
                        using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
                        {
                            string line;
                            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                            {
                                if (line.Length == 0) continue;
                                if (!line.StartsWith("data:", StringComparison.Ordinal)) continue;
                                var data = line.Substring(5).TrimStart();
                                if (data == "[DONE]") break;

                                JObject j;
                                try { j = JObject.Parse(data); }
                                catch { continue; }

                                // Usage chunk (cuoi stream khi stream_options.include_usage = true)
                                var usage = j["usage"] as JObject;
                                if (usage != null)
                                {
                                    tokensIn  = (int?)usage["prompt_tokens"]     ?? tokensIn;
                                    tokensOut = (int?)usage["completion_tokens"] ?? tokensOut;
                                }

                                var choices = j["choices"] as JArray;
                                if (choices == null || choices.Count == 0) continue;
                                var delta = choices[0]?["delta"] as JObject;
                                if (delta == null) continue;

                                var content = (string)delta["content"];
                                if (!string.IsNullOrEmpty(content))
                                {
                                    try { onTextDelta?.Invoke(content); }
                                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[StreamAsync] onTextDelta err: " + ex.Message); }
                                }

                                // Tool calls stream theo tung delta - accumulate
                                var toolCalls = delta["tool_calls"] as JArray;
                                if (toolCalls != null && toolCalls.Count > 0)
                                {
                                    var tc = toolCalls[0] as JObject;
                                    var fn = tc?["function"] as JObject;
                                    var fname = (string)fn?["name"];
                                    if (!string.IsNullOrEmpty(fname)) toolName = fname;
                                    var fargs = (string)fn?["arguments"];
                                    if (!string.IsNullOrEmpty(fargs)) toolArgsBuf.Append(fargs);
                                }
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(toolName))
                {
                    System.Diagnostics.Debug.WriteLine("[StreamAsync] tool: " + toolName + " args=" + toolArgsBuf);
                    try { onToolCall?.Invoke(toolName, toolArgsBuf.ToString()); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("[StreamAsync] onToolCall err: " + ex.Message); }
                }
            }
            catch (TaskCanceledException)
            {
                error = "Request timeout";
            }
            catch (Exception ex)
            {
                error = "Exception: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("[StreamAsync] " + ex);
            }
            onComplete?.Invoke(tokensIn, tokensOut, error);
        }
    }
}
