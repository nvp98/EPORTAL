using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EPORTAL.Common
{
    /// <summary>
    /// Lop bao ve tap trung cho RESPONSE tu dich vu ngoai (OpenAI, VBee).
    /// Nguyen tac: khong tin du lieu tu ben ngoai ke ca khi goi qua HTTPS chinh chu -
    /// response co the bi compromise (account hack, MITM noi bo, provider loi).
    ///
    /// Cac lop bao ve:
    ///  1. SanitizeText  - loc ky tu dieu khien/an (bidi override, zero-width) + cap do dai
    ///                     -> chong giau noi dung doc, terminal escape, log injection, DoS text khong lo.
    ///  2. IsSafeAudioUrl - chan SSRF: audioLink tu VBee chi duoc HTTPS + khong duoc tro ve
    ///                     IP private/loopback/link-local (ke ca sau DNS resolve).
    ///                     Tuy chon siet host qua env CHATBOT_VBEE_AUDIO_HOSTS.
    ///  3. IsLikelyMp3   - magic-byte check: bytes tu VBee phai dung la MP3 truoc khi
    ///                     cache vao DB va phat cho moi user (chong HTML/script gia dang audio).
    ///  4. SafeErrorDetail - body loi cua provider truoc khi tra ve client: sanitize + cat ngan.
    /// </summary>
    public static class ExternalContentGuard
    {
        // ====== Gioi han kich thuoc (chars) cho noi dung text tu model ======
        public const int MAX_REPLY_CHARS      = 8000;   // 1 cau tra loi non-stream
        public const int MAX_STREAM_CHARS     = 16000;  // tong text tich luy 1 luot stream
        public const int MAX_TOOL_ARGS_CHARS  = 4000;   // args JSON cua 1 tool call
        public const int MAX_TRANSCRIPT_CHARS = 1000;   // transcript STT
        public const int MAX_ACTION_CHARS     = 200;    // ten scene / query navigate
        public const int MAX_REASON_CHARS     = 300;    // cau xac nhan navigate

        /// <summary>Gioi han audio TTS (bytes). Default 20MB, override CHATBOT_TTS_MAX_AUDIO_BYTES.</summary>
        public static int MaxAudioBytes
        {
            get
            {
                var v = ChatbotConfig.GetInt("CHATBOT_TTS_MAX_AUDIO_BYTES", "Chatbot.TtsMaxAudioBytes", 20 * 1024 * 1024);
                return Math.Max(64 * 1024, Math.Min(100 * 1024 * 1024, v));
            }
        }

        // ==================================================================
        //  1. TEXT SANITIZE
        // ==================================================================

        /// <summary>
        /// Loc text tu dich vu ngoai truoc khi day ve client / luu DB:
        ///  - bo ky tu dieu khien C0/C1 (giu \r \n \t) -> chong terminal escape / log injection
        ///  - bo ky tu an: zero-width (U+200B-200F, U+FEFF), bidi override (U+202A-202E, U+2066-2069)
        ///    -> chong giau text doc / dao chieu hien thi danh lua nguoi dung
        ///  - cap do dai maxLen (&lt;=0 = khong cap)
        /// KHONG strip HTML - frontend da escape (escHtml) truoc khi render.
        /// </summary>
        public static string SanitizeText(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            var sb = new StringBuilder(Math.Min(s.Length, maxLen > 0 ? maxLen : s.Length));
            foreach (var ch in s)
            {
                if (maxLen > 0 && sb.Length >= maxLen) break;
                if (ch == '\r' || ch == '\n' || ch == '\t') { sb.Append(ch); continue; }
                if (ch < 0x20 || ch == 0x7F) continue;                    // C0 control + DEL
                if (ch >= 0x80 && ch <= 0x9F) continue;                   // C1 control
                if (ch >= 0x200B && ch <= 0x200F) continue;               // zero-width + LRM/RLM
                if (ch >= 0x202A && ch <= 0x202E) continue;               // bidi embedding/override
                if (ch >= 0x2066 && ch <= 0x2069) continue;               // bidi isolate
                if (ch == 0xFEFF) continue;                               // BOM/zero-width no-break
                sb.Append(ch);
            }
            return sb.ToString();
        }

        /// <summary>Body loi tu provider tra ve client: sanitize + cat 400 ky tu (khong lo noi bo).</summary>
        public static string SafeErrorDetail(string body)
        {
            var s = SanitizeText(body, 400);
            return s;
        }

        // ==================================================================
        //  2. SSRF GUARD cho audioLink (VBee TTS)
        // ==================================================================

        /// <summary>
        /// audioLink trong response VBee co duoc phep fetch khong:
        ///  - bat buoc URL tuyet doi, scheme HTTPS
        ///  - host khong phai localhost / khong resolve ve IP private, loopback, link-local
        ///  - neu env CHATBOT_VBEE_AUDIO_HOSTS duoc set (csv suffix, vd "vbee.vn,vbee.ai")
        ///    thi host con phai khop 1 suffix trong danh sach.
        /// </summary>
        public static bool IsSafeAudioUrl(string url)
        {
            Uri uri;
            if (string.IsNullOrEmpty(url) || !Uri.TryCreate(url, UriKind.Absolute, out uri)) return false;
            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) return false;

            var host = uri.Host;
            if (string.IsNullOrEmpty(host)) return false;
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase)) return false;
            if (host.EndsWith(".local", StringComparison.OrdinalIgnoreCase)
                || host.EndsWith(".internal", StringComparison.OrdinalIgnoreCase)) return false;

            // Allowlist tuy chon (suffix match, csv) - set trong .env khi muon siet chat.
            var allow = ChatbotConfig.Get("CHATBOT_VBEE_AUDIO_HOSTS", "Chatbot.VbeeAudioHosts", null);
            if (!string.IsNullOrEmpty(allow))
            {
                var ok = false;
                foreach (var rawSuffix in allow.Split(','))
                {
                    var suffix = rawSuffix.Trim().TrimStart('.');
                    if (suffix.Length == 0) continue;
                    if (host.Equals(suffix, StringComparison.OrdinalIgnoreCase)
                        || host.EndsWith("." + suffix, StringComparison.OrdinalIgnoreCase)) { ok = true; break; }
                }
                if (!ok)
                {
                    System.Diagnostics.Debug.WriteLine("[ExternalContentGuard] audioLink host ngoai allowlist: " + host);
                    return false;
                }
            }

            // Host la IP literal -> check truc tiep
            IPAddress literal;
            if (IPAddress.TryParse(host.Trim('[', ']'), out literal))
                return !IsPrivateOrLocal(literal);

            // Hostname -> resolve DNS, MOI dia chi tra ve phai public
            try
            {
                var addrs = Dns.GetHostAddresses(host);
                if (addrs == null || addrs.Length == 0) return false;
                foreach (var a in addrs)
                    if (IsPrivateOrLocal(a))
                    {
                        System.Diagnostics.Debug.WriteLine("[ExternalContentGuard] audioLink resolve ve IP noi bo: " + host + " -> " + a);
                        return false;
                    }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ExternalContentGuard] DNS fail cho audioLink host " + host + ": " + ex.Message);
                return false; // khong resolve duoc thi cung khong fetch duoc -> tu choi som
            }
        }

        private static bool IsPrivateOrLocal(IPAddress ip)
        {
            if (ip == null) return true;
            if (IPAddress.IsLoopback(ip)) return true;

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ip.IsIPv6LinkLocal || ip.IsIPv6SiteLocal || ip.IsIPv6Multicast) return true;
                var b16 = ip.GetAddressBytes();
                if ((b16[0] & 0xFE) == 0xFC) return true;          // fc00::/7 unique-local
                if (ip.IsIPv4MappedToIPv6) ip = ip.MapToIPv4();    // check tiep phan IPv4 ben duoi
                else return false;
            }

            var b = ip.GetAddressBytes();
            if (b.Length != 4) return false;
            if (b[0] == 10) return true;                            // 10.0.0.0/8
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true; // 172.16.0.0/12
            if (b[0] == 192 && b[1] == 168) return true;            // 192.168.0.0/16
            if (b[0] == 169 && b[1] == 254) return true;            // 169.254.0.0/16 (link-local/cloud metadata)
            if (b[0] == 127) return true;                           // 127.0.0.0/8
            if (b[0] == 0) return true;                             // 0.0.0.0/8
            return false;
        }

        // ==================================================================
        //  3. AUDIO CONTENT CHECK
        // ==================================================================

        /// <summary>
        /// Bytes co dang la MP3 hop le khong (truoc khi cache DB + phat cho user):
        /// chap nhan header "ID3" hoac MPEG frame sync (0xFF 0xEx/0xFx).
        /// Chan truong hop VBee/CDN tra ve HTML loi, JSON, hay payload khac gia dang audio.
        /// </summary>
        public static bool IsLikelyMp3(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4) return false;
            if (bytes.Length > MaxAudioBytes) return false;
            if (bytes[0] == 0x49 && bytes[1] == 0x44 && bytes[2] == 0x33) return true; // "ID3"
            if (bytes[0] == 0xFF && (bytes[1] & 0xE0) == 0xE0) return true;            // MPEG frame sync
            return false;
        }
    }
}
