using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EPORTAL.Common
{
    /// <summary>
    /// Circuit breaker nhe cho dich vu NGOAI (OpenAI, VBee). Khi 1 dich vu loi ket noi
    /// (firewall production chan / timeout / thieu key), danh dau "down" trong N giay ->
    /// cac call sau FAST-FAIL (khong cho timeout lai) -> chatbot/tour degrade muot,
    /// KHONG treo va KHONG gay loi toan cuc.
    /// </summary>
    public static class ServiceHealth
    {
        public const string OPENAI = "openai";
        public const string VBEE   = "vbee";

        private static readonly ConcurrentDictionary<string, DateTime> _downUntil =
            new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private const int DEFAULT_COOLDOWN_SEC = 60;

        /// <summary>Dich vu dang trong cua so "down" (vua loi gan day) -> nen bo qua, fast-fail.</summary>
        public static bool IsDown(string svc)
        {
            return _downUntil.TryGetValue(svc, out var until) && until > DateTime.UtcNow;
        }

        public static void MarkDown(string svc, int seconds = DEFAULT_COOLDOWN_SEC)
        {
            _downUntil[svc] = DateTime.UtcNow.AddSeconds(seconds);
            System.Diagnostics.Debug.WriteLine("[ServiceHealth] " + svc + " marked DOWN " + seconds + "s");
        }

        /// <summary>Goi khi call thanh cong -> mo lai circuit (cho cac call khac dung tiep).</summary>
        public static void MarkUp(string svc)
        {
            _downUntil.TryRemove(svc, out _);
        }

        /// <summary>True neu exception la loi KET NOI / timeout (nghi do firewall chan) -> nen MarkDown.</summary>
        public static bool IsConnectivityError(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e is System.Net.Http.HttpRequestException) return true;
                if (e is System.Net.WebException) return true;
                if (e is System.Net.Sockets.SocketException) return true;
                if (e is TaskCanceledException || e is OperationCanceledException) return true; // HttpClient timeout
            }
            return false;
        }
    }
}
