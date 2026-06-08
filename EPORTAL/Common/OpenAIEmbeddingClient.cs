using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace EPORTAL.Common
{
    /// <summary>
    /// Goi OpenAI /v1/embeddings -> float[] cho semantic match (knowledge cache).
    /// Mac dinh text-embedding-3-small (1536d, re). Tra null neu loi -> caller fallback normalized match.
    /// </summary>
    public class OpenAIEmbeddingClient
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private const string ENDPOINT = "https://api.openai.com/v1/embeddings";

        public async Task<float[]> EmbedAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                if (string.IsNullOrEmpty(apiKey)) return null;
                if (ServiceHealth.IsDown(ServiceHealth.OPENAI)) return null;   // down -> bo qua semantic, KB van fallback exact-norm
                var model = ChatbotConfig.Get("CHATBOT_EMBED_MODEL", "Chatbot.EmbedModel", "text-embedding-3-small");

                var payload = new JObject { ["model"] = model, ["input"] = text };
                using (var msg = new HttpRequestMessage(HttpMethod.Post, ENDPOINT))
                {
                    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    msg.Content = new StringContent(payload.ToString(), Encoding.UTF8, "application/json");
                    using (var resp = await _http.SendAsync(msg).ConfigureAwait(false))
                    {
                        var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                        if (!resp.IsSuccessStatusCode)
                        {
                            System.Diagnostics.Debug.WriteLine("[Embed] OpenAI " + (int)resp.StatusCode + " " + (body ?? "").Substring(0, Math.Min(200, (body ?? "").Length)));
                            return null;
                        }
                        var arr = JObject.Parse(body)["data"]?[0]?["embedding"] as JArray;
                        if (arr == null || arr.Count == 0) return null;
                        var v = new float[arr.Count];
                        for (int i = 0; i < arr.Count; i++) v[i] = (float)arr[i];
                        ServiceHealth.MarkUp(ServiceHealth.OPENAI);
                        return v;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ServiceHealth.IsConnectivityError(ex)) ServiceHealth.MarkDown(ServiceHealth.OPENAI);
                System.Diagnostics.Debug.WriteLine("[Embed] " + ex.Message);
                return null;
            }
        }
    }
}
