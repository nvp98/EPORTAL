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

        // Load `.env` file tu solution root (1 cap tren ~/) -> process env vars.
        // Format don gian: KEY=value moi dong, # = comment. Khong overwrite var da co
        // (he thong > .env -> deploy co the dat qua IIS app pool / system env).
        private static void LoadDotEnv()
        {
            try
            {
                var appRoot = HostingEnvironment.ApplicationPhysicalPath;
                if (string.IsNullOrEmpty(appRoot)) return;
                // Try ./../.env (solution root) first, then ./.env (project root fallback)
                var candidates = new[] {
                    Path.Combine(appRoot, "..", ".env"),
                    Path.Combine(appRoot, ".env")
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
                        var val = line.Substring(eq + 1).Trim();
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
