using System;
using System.Web.Mvc;

namespace EPORTAL.Common
{
    /// <summary>
    /// CSRF defense cho JSON POST endpoints (khong dung form -> AntiForgeryToken khong work).
    /// Verify Origin/Referer header khop voi host hien tai. Browsers tu dong set 2 header nay
    /// cho cross-origin request, va attacker KHONG the override duoc tu JS code.
    ///
    /// Apply: [SameOriginOnly] tren action method.
    /// </summary>
    public class SameOriginOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;

            // Chi check POST (GET an toan theo dinh nghia CSRF).
            if (!string.Equals(req.HttpMethod, "POST", StringComparison.OrdinalIgnoreCase))
                return;

            var expectedHost = req.Url != null ? req.Url.Host : null;
            if (string.IsNullOrEmpty(expectedHost)) return;

            // Prefer Origin (chinh xac), fallback Referer (truong hop browser cu).
            var origin = req.Headers["Origin"];
            string actualHost = null;

            if (!string.IsNullOrEmpty(origin) && origin != "null")
            {
                if (Uri.TryCreate(origin, UriKind.Absolute, out var originUri))
                    actualHost = originUri.Host;
            }
            else
            {
                var referer = req.Headers["Referer"];
                if (!string.IsNullOrEmpty(referer)
                    && Uri.TryCreate(referer, UriKind.Absolute, out var refUri))
                    actualHost = refUri.Host;
            }

            // Block neu khong co Origin/Referer hoac mismatch host
            // (browser luon set 1 trong 2 cho cross-origin -> empty = same-origin form
            // hoac legit JS fetch tu Razor view; mismatch host = nghi van CSRF).
            if (string.IsNullOrEmpty(actualHost)) return;  // same-origin or local navigation OK
            if (!string.Equals(actualHost, expectedHost, StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new HttpStatusCodeResult(403, "Cross-origin POST refused");
            }
        }
    }
}
