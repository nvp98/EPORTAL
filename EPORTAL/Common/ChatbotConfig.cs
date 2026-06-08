using System;
using System.Configuration;

namespace EPORTAL.Common
{
    /// <summary>
    /// Doc config chatbot theo thu tu uu tien: .env (Environment var) -> Web.config AppSettings -> default.
    /// Muc tieu: gom TAT CA cau hinh chatbot (gioi han/model/giong...) vao .env, KHONG can sua Web.config
    /// khi deploy production (Web.config giu nguyen nhu dev branch).
    ///
    /// Quy uoc ten: env key UPPER_SNAKE co tien to CHATBOT_ (vd CHATBOT_RATE_LIMIT_PER_HOUR),
    /// map sang AppSettings key cu "Chatbot.RateLimitPerHour" (van fallback duoc cho tuong thich).
    /// </summary>
    public static class ChatbotConfig
    {
        public static string Get(string envKey, string appSettingsKey, string fallback = null)
        {
            if (!string.IsNullOrEmpty(envKey))
            {
                var v = Environment.GetEnvironmentVariable(envKey);
                if (!string.IsNullOrEmpty(v)) return v;
            }
            if (!string.IsNullOrEmpty(appSettingsKey))
            {
                var v = ConfigurationManager.AppSettings[appSettingsKey];
                if (!string.IsNullOrEmpty(v)) return v;
            }
            return fallback;
        }

        public static int GetInt(string envKey, string appSettingsKey, int fallback)
            => int.TryParse(Get(envKey, appSettingsKey, null), out var n) ? n : fallback;

        public static long GetLong(string envKey, string appSettingsKey, long fallback)
            => long.TryParse(Get(envKey, appSettingsKey, null), out var n) ? n : fallback;
    }
}
