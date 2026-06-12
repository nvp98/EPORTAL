using System;
using System.IO;
using System.Reflection;

namespace EPORTAL.Common
{
    /// <summary>
    /// Token version cho static asset (CSS/JS) - tinh MOT LAN luc app start.
    /// Dung thay cho `?v=DateTime.UtcNow.Ticks` (vot moi request -> trinh duyet
    /// KHONG BAO GIO cache duoc, tai lai CSS/JS moi page load -> web load lau).
    ///
    /// Gia tri = last-write-time cua assembly (doi moi lan build/deploy) ->
    /// cache giua cac deploy, tu bust khi deploy ban moi. Fallback: assembly
    /// version, roi timestamp luc app start neu khong doc duoc file.
    /// </summary>
    public static class AssetVersion
    {
        public static readonly string Token = Compute();

        private static string Compute()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var loc = asm.Location;
                if (!string.IsNullOrEmpty(loc) && File.Exists(loc))
                    return File.GetLastWriteTimeUtc(loc).Ticks.ToString();
            }
            catch { /* medium-trust / single-file -> fallback */ }

            try
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                if (v != null) return v.ToString();
            }
            catch { }

            // Khong doc duoc gi -> dung timestamp luc khoi tao (on dinh trong doi song app pool)
            return DateTime.UtcNow.Ticks.ToString();
        }
    }
}
