using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace EPORTAL
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            LoadDotEnv();
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Load `.env` tu thu muc ung dung (cung cap Global.asax/Web.config) -> process env vars.
        // Format don gian: KEY=value moi dong, # = comment. Khong overwrite var da co
        // (he thong > .env -> deploy co the dat qua IIS app pool / system env).
        private static void LoadDotEnv()
        {
            try
            {
                var appRoot = HostingEnvironment.ApplicationPhysicalPath;
                if (string.IsNullOrEmpty(appRoot)) return;
                // Production dat .env trong application root. Thu muc cha chi la fallback
                // cho cac may local cu dang dat .env o solution root.
                var candidates = new[] {
                    Path.Combine(appRoot, ".env"),
                    Path.GetFullPath(Path.Combine(appRoot, "..", ".env"))
                };
                foreach (var path in candidates)
                {
                    if (!File.Exists(path)) continue;
                    foreach (var raw in File.ReadAllLines(path))
                    {
                        var line = raw.Trim();
                        if (line.Length == 0 || line.StartsWith("#")) continue;
                        var eq = line.IndexOf('=');
                        if (eq <= 0) continue;
                        var key = line.Substring(0, eq).Trim();
                        var val = ParseDotEnvValue(line.Substring(eq + 1));
                        if (val.Length >= 2 && ((val[0] == '"' && val[val.Length-1] == '"')
                                              || (val[0] == '\'' && val[val.Length-1] == '\'')))
                            val = val.Substring(1, val.Length - 2);
                        if (Environment.GetEnvironmentVariable(key) == null)
                            Environment.SetEnvironmentVariable(key, val);
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[DotEnv] load err: " + ex.Message);
            }
        }

        private static string ParseDotEnvValue(string raw)
        {
            var value = (raw ?? string.Empty).Trim();
            char quote = '\0';
            for (var i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                if ((ch == '"' || ch == '\'') && (i == 0 || value[i - 1] != '\\'))
                {
                    if (quote == '\0') quote = ch;
                    else if (quote == ch) quote = '\0';
                    continue;
                }

                if (ch == '#' && quote == '\0' && (i == 0 || char.IsWhiteSpace(value[i - 1])))
                    return value.Substring(0, i).TrimEnd();
            }
            return value;
        }

        // Cac endpoint chatbot user-facing (SSE stream + TTS + STT + Realtime) KHONG dung Session
        // (auth doc tu Forms ticket qua User.Identity, khong qua Session). Mac dinh ASP.NET giu
        // EXCLUSIVE session write-lock moi request -> serialize MOI request cung 1 user.
        // Hau qua: TTS /Speak bi chan, khong chay duoc trong luc /AskStream (LLM SSE) con stream,
        // va prefetch nhieu cau bi serialize -> cau sau queue/timeout -> pipeline "phun" het text.
        // Tat Session cho cac endpoint nay de chung chay concurrent (giam tre + tranh dump).
        // Phai goi truoc AcquireRequestState -> dat o BeginRequest.
        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var ctx = HttpContext.Current;
            if (ctx == null) return;
            string path;
            try { path = ctx.Request.AppRelativeCurrentExecutionFilePath; } // ~/View360/Chatbot/Speak
            catch { return; }
            if (string.IsNullOrEmpty(path)) return;
            if (path.IndexOf("/Chatbot/", StringComparison.OrdinalIgnoreCase) < 0) return;
            if (path.IndexOf("/AskStream", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/Speak", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/Transcribe", StringComparison.OrdinalIgnoreCase) >= 0
                || path.IndexOf("/RealtimeSession", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                ctx.SetSessionStateBehavior(System.Web.SessionState.SessionStateBehavior.Disabled);
            }
        }

        // Cho phep [OutputCache(VaryByCustom = "User")] tao key cache rieng cho moi user.
        // Can thiet vi View360 SP tra ket qua theo permission tung user.
        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (string.Equals(custom, "User", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var name = context?.User?.Identity?.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        // Cookie raw format: "{ID};{MaNV};{HoTen};..."
                        var idx = name.IndexOf(';');
                        return idx > 0 ? name.Substring(0, idx) : name;
                    }
                }
                catch { /* fall through */ }
                return "anon";
            }
            return base.GetVaryByCustomString(context, custom);
        }
    }
}
