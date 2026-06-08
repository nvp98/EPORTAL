using Newtonsoft.Json;

namespace EPORTAL.Common
{
    /// <summary>
    /// JSON serialization an toan de embed vao <script> tag trong Razor view.
    /// StringEscapeHandling.EscapeHtml dat ky tu &lt; &gt; &amp; &apos; &quot; thanh \uXXXX
    /// trong string values -> ke gian inject "</script>" vao DB cung khong break out
    /// duoc khoi script block. JSON.parse client van decode binh thuong.
    ///
    /// Dung: @Html.Raw(EPORTAL.Common.JsonForHtml.Serialize(model))
    /// </summary>
    public static class JsonForHtml
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            StringEscapeHandling = StringEscapeHandling.EscapeHtml
        };

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Settings);
        }
    }
}
