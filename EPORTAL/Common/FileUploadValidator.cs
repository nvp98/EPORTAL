using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;

namespace EPORTAL.Common
{
    // Validate file upload theo extension whitelist + size limit, sinh ten file an toan.
    // Su dung trong cac controller co upload (View360: Album, Video, Virtual, Projects, DocumentLibrary).
    public static class FileUploadValidator
    {
        private static readonly HashSet<string> AllowedImageExt =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

        private static readonly HashSet<string> AllowedPdfExt =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".pdf" };

        private static readonly HashSet<string> AllowedDocExt =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt" };

        private static readonly HashSet<string> AllowedVideoExt =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".mp4", ".webm", ".mov", ".m4v" };

        private static readonly HashSet<string> AllowedExcelExt =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".xls", ".xlsx" };

        public const long MaxImageBytes = 10L * 1024 * 1024;     // 10 MB
        public const long MaxPdfBytes   = 50L * 1024 * 1024;     // 50 MB
        public const long MaxDocBytes   = 100L * 1024 * 1024;    // 100 MB
        public const long MaxVideoBytes = 500L * 1024 * 1024;    // 500 MB
        public const long MaxExcelBytes = 20L * 1024 * 1024;     // 20 MB

        // Tra ve null neu OK, neu khong tra ve message loi.
        public static string ValidateImage(HttpPostedFileBase file)
            => Validate(file, AllowedImageExt, MaxImageBytes, "anh");

        public static string ValidatePdf(HttpPostedFileBase file)
            => Validate(file, AllowedPdfExt, MaxPdfBytes, "PDF");

        public static string ValidateDocument(HttpPostedFileBase file)
            => Validate(file, AllowedDocExt, MaxDocBytes, "tai lieu");

        public static string ValidateVideo(HttpPostedFileBase file)
            => Validate(file, AllowedVideoExt, MaxVideoBytes, "video");

        public static string ValidateExcel(HttpPostedFileBase file)
            => Validate(file, AllowedExcelExt, MaxExcelBytes, "Excel");

        private static string Validate(HttpPostedFileBase file, HashSet<string> allowedExt, long maxBytes, string label)
        {
            if (file == null || file.ContentLength == 0) return null; // optional file - bo qua

            var ext = Path.GetExtension(file.FileName ?? string.Empty);
            if (string.IsNullOrEmpty(ext) || !allowedExt.Contains(ext))
            {
                return $"File {label} khong hop le. Chi chap nhan: {string.Join(", ", allowedExt)}";
            }

            if (file.ContentLength > maxBytes)
            {
                return $"File {label} qua lon (max {maxBytes / 1024 / 1024} MB).";
            }

            // Path traversal guard - filename khong duoc chua / \ ..
            var name = Path.GetFileName(file.FileName ?? string.Empty);
            if (name.Contains("..") || name.Contains("/") || name.Contains("\\"))
            {
                return $"Ten file chua ky tu khong hop le.";
            }

            return null;
        }

        // Sinh ten file an toan:
        //   - Strip path (chi giu basename)
        //   - Replace ky tu khong an toan bang '_'
        //   - Prepend timestamp de tranh trung
        //   - Giu nguyen extension (luc nay da pass validate)
        public static string SafeFileName(string originalFileName, string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(originalFileName))
            {
                return $"{prefix ?? DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bin";
            }

            var name = Path.GetFileName(originalFileName);
            var ext = Path.GetExtension(name).ToLowerInvariant();
            var baseName = Path.GetFileNameWithoutExtension(name);

            // Diacritic -> ASCII don gian
            baseName = RemoveDiacritics(baseName);
            // Chi giu chu/so/dash/underscore
            baseName = Regex.Replace(baseName, @"[^a-zA-Z0-9_-]", "_");
            // Khong cho phep rong
            if (string.IsNullOrEmpty(baseName)) baseName = "file";
            // Cat ngan
            if (baseName.Length > 80) baseName = baseName.Substring(0, 80);

            var ts = prefix ?? DateTime.Now.ToString("yyyyMMddHHmmssfff");
            return $"{ts}_{baseName}{ext}";
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (var c in normalized)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                    != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC)
                                 .Replace('đ', 'd').Replace('Đ', 'D');
        }
    }
}
