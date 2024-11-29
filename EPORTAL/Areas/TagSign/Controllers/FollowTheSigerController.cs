using EPORTAL.ModelsTagSign;
using EPORTAL.ModelsView360;
using iTextSharp.text.pdf;
using iTextSharp.text;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Rotativa;
using System.Net;
using Rotativa;
using Rotativa.Options;
using DocumentFormat.OpenXml.Presentation;
using Font = iTextSharp.text.Font;
using static iTextSharp.text.Font;

namespace EPORTAL.Areas.TagSign.Controllers
{
    public class FollowTheSigerController : Controller
    {
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "FollowTheSiger";
        EPORTALEntities db = new EPORTALEntities();
        EPORTAL_REGISTEREntities db_dk = new EPORTAL_REGISTEREntities();
        // GET: TagSign/FollowTheSiger
        public ActionResult Index(int? page, string search, DateTime? begind, DateTime? endd)
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("VIEW_ALL"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Logout", "Login", new { area = "" });
            }
            if (search == null) search = "";
            ViewBag.search = search;

            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var nd = db.AuthorizationContractors.Where(x => x.IDNhanVien == MyAuthentication.ID).SingleOrDefault();

            var idnv = db.NhanViens.Where(x => x.ID == nd.IDNhanVien).FirstOrDefault();

            var res = (from kd in db_dk.TK_CardRegistrationInfor.Where(x => x.NhanVienID == idnv.ID && x.TinhTrangID != 0)
                       join ca in db_dk.DK_CardRegistrationInfor on kd.DKTID equals ca.IDDKT
                       select new TK_CardRegistrationInforValidation
                       {
                           IDTKDKT = (int)kd.IDTKDKT,
                           DKTID = (int)kd.DKTID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayDangKy ?? default,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       });
            if (begind == null && endd == null)
            {
                res = res.Where(x => x.NgayDuyet >= startDay && x.NgayDuyet <= endDay);
            }
            else
            {
                res = res.Where(x => x.NgayDuyet >= begind && x.NgayDuyet <= endd);
            }

            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.OrderByDescending(x => x.NgayDuyet).ToList().ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Show(int? page, int? IDDKT)
        {
            var res = (from kd in db_dk.TK_CardRegistrationInfor.Where(x => x.DKTID == IDDKT)
                       join ca in db_dk.DK_CardRegistrationInfor on kd.DKTID equals ca.IDDKT
                       select new TK_CardRegistrationInforValidation
                       {
                           IDTKDKT = (int)kd.IDTKDKT,
                           DKTID = (int)kd.DKTID,
                           NoiDung = ca.NoiDung,
                           NgayTrinh = (DateTime?)ca.NgayDangKy ?? default,
                           CapDuyet = (int)kd.CapDuyet,
                           TinhTrangID = (int)kd.TinhTrangID,
                           NhanVienID = (int)kd.NhanVienID,
                           NgayDuyet = (DateTime?)kd.NgayDuyet ?? default,
                           GhiChu = kd.GhiChu
                       }).ToList();
            if (page == null) page = 1;
            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(res.ToList().ToPagedList(pageNumber, pageSize));
        }
        public ActionResult TestView(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardRegistrationInfor.Where(x => x.IDDKT == id && x.GhiChu == "")
                       select new DK_Detail_ListCardRegisInforNTValidation
                       {
                           IDCTDK = (int)a.IDCTDK,
                           HoTen = a.HoTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           HoKhau = a.HoKhau,
                           ChucVu = (int)a.ChucVu,
                           SoDienThoai = a.SoDienThoai,
                           TheLuuTru = a.TheLuuTru,
                           TheRaVaoKLH = a.TheRaVaoKLH,
                           DienThoaiTM = a.DienThoaiTM,
                           RaVaoCang = a.RaVaoCang,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           Cong = a.Cong,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           IDDKT = (int)a.IDDKT
                       }).ToList();
            DK_Detail_ListCardRegisInforNTValidation DO = new DK_Detail_ListCardRegisInforNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCTDK = (int)a.IDCTDK;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.HoKhau = a.HoKhau;
                    DO.ChucVu = (int)a.ChucVu;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.TheLuuTru = a.TheLuuTru;
                    DO.TheRaVaoKLH = a.TheRaVaoKLH;
                    DO.DienThoaiTM = a.DienThoaiTM;
                    DO.RaVaoCang = a.RaVaoCang;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.KhuVucLamViec = a.KhuVucLamViec;
                    DO.Cong = a.Cong;
                    DO.NhomNT = (int)a.NhomNT;
                    DO.GhiChu = a.GhiChu;
                    DO.IDDKT = (int)a.IDDKT;
                }
                return View(DO);
            }
            else
            {
                return RedirectToAction("Index", "FollowTheSiger");
            }
          
        }

        public ActionResult PrintTestView(int? id)
        {
            var res = (from a in db_dk.DK_DetailCardRegistrationInfor.Where(x => x.IDDKT == id && x.GhiChu == "")
                       select new DK_Detail_ListCardRegisInforNTValidation
                       {
                           IDCTDK = (int)a.IDCTDK,
                           HoTen = a.HoTen,
                           CCCD = a.CCCD,
                           NgaySinh = (DateTime)a.NgaySinh,
                           HoKhau = a.HoKhau,
                           ChucVu = (int)a.ChucVu,
                           SoDienThoai = a.SoDienThoai,
                           TheLuuTru = a.TheLuuTru,
                           TheRaVaoKLH = a.TheRaVaoKLH,
                           DienThoaiTM = a.DienThoaiTM,
                           RaVaoCang = a.RaVaoCang,
                           ThoiHanThe = (DateTime)a.ThoiHanThe,
                           KhuVucLamViec = a.KhuVucLamViec,
                           Cong = a.Cong,
                           NhomNT = (int)a.NhomNT,
                           GhiChu = a.GhiChu,
                           IDDKT = (int)a.IDDKT
                       }).ToList();
            DK_Detail_ListCardRegisInforNTValidation DO = new DK_Detail_ListCardRegisInforNTValidation();
            if (res.Count > 0)
            {
                foreach (var a in res)
                {
                    DO.IDCTDK = (int)a.IDCTDK;
                    DO.HoTen = a.HoTen;
                    DO.CCCD = a.CCCD;
                    DO.NgaySinh = (DateTime)a.NgaySinh;
                    DO.HoKhau = a.HoKhau;
                    DO.ChucVu = (int)a.ChucVu;
                    DO.SoDienThoai = a.SoDienThoai;
                    DO.TheLuuTru = a.TheLuuTru;
                    DO.TheRaVaoKLH = a.TheRaVaoKLH;
                    DO.DienThoaiTM = a.DienThoaiTM;
                    DO.RaVaoCang = a.RaVaoCang;
                    DO.ThoiHanThe = (DateTime)a.ThoiHanThe;
                    DO.KhuVucLamViec = a.KhuVucLamViec;
                    DO.Cong = a.Cong;
                    DO.NhomNT = (int)a.NhomNT;
                    DO.GhiChu = a.GhiChu;
                    DO.IDDKT = (int)a.IDDKT;
                }
                return View(DO);
            }
            else
            {
                return RedirectToAction("Index", "FollowTheSiger");
            }

        }

        public void tempFilePdf(int? id, String path)
        {
            //var root = Server.MapPath("~/PDF/");
            //var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            //var path = Path.Combine(root, pdfname);
            //path = Path.GetFullPath(path);
            string html = Server.MapPath("~/Views/Shared/footer.html");
            //string html1 = Server.MapPath("~/Views/Shared/header.html");
            string footer = string.Format(
                        "--footer-html \"{0}\" " +
                        "--footer-spacing \"5\" " +
                        "--footer-font-size \"5\" " +
                        "--footer-line --encoding utf-8", html);
            //string footer = "--footer-right \"Page: [page] of [toPage]\" " + "--footer-center \"Tài liệu này thuộc sở hữu của Hòa Phát Dung Quất. Việc phát tán, sử dụng trái phép bị nghiêm cấm\" --footer-line --footer-font-size \"9\" --footer-spacing 5 --footer-font-name \"Arial Unicode MS\" --encoding \"utf-8\"";
            var actionPDF = new ActionAsPdf("PrintTestView", new { id = id})
            {
                //FileName = "cv.pdf",
                ////PageOrientation = Orientation.Portrait,
                PageSize = Size.A4,
                PageMargins = new Margins(13, 5, 10, 5),
                PageWidth = 500,
                PageHeight = 300,
                ////IsLowQuality = true ,
                CustomSwitches = footer,
                MinimumFontSize = 20,
                //SaveOnServerPath = path
            };
            var applicationPDFData = actionPDF.BuildPdf(this.ControllerContext);
            System.IO.File.WriteAllBytes(path, applicationPDFData);

        }

        public ActionResult ExportToPdfsss(int? id)
        {
           

            var root = Server.MapPath("~/UploadedFiles/PDFDangKyThe/");
            var pdfname = String.Format("{0}.pdf", Guid.NewGuid().ToString());
            var path = Path.Combine(root, pdfname);
            path = Path.GetFullPath(path); 

            string destinationpath = string.Empty;

            tempFilePdf(id, path);
            var IDDK = db_dk.DK_CardRegistrationInfor.Where(x => x.IDDKT == id).FirstOrDefault();
            var IDNT = db.NT_Partner.Where(x => x.ID == IDDK.NTID).FirstOrDefault();
            // Destination File path
            destinationpath = IDNT.FullName + ".pdf";
            //Source File Path
            string originalFile = path;



            // Read file from file location
            PdfReader reader = new PdfReader(originalFile);

            //define font size and style
            Font font = new Font(Font.FontFamily.HELVETICA, 16, Font.NORMAL, BaseColor.LIGHT_GRAY);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (var pdfStamper = new PdfStamper(reader, memoryStream, '\0'))
                {
                    // Getting total number of pages of the Existing Document
                    int pageCount = reader.NumberOfPages;

                    // Create two New Layer for Watermark
                    PdfLayer layer = new PdfLayer("WatermarkLayer", pdfStamper.Writer);
                    //PdfLayer layer2 = new PdfLayer("WatermarkLayer2", pdfStamper.Writer);

                    // Loop through each Page

                    string layerwarkmarktxt = "";// define text for 
                    //string Layer2warkmarktxt = "Confidential";
                    for (int i = 1; i <= pageCount; i++)
                    {
                        // Getting the Page Size
                        Rectangle rect = reader.GetPageSize(i);
                        // Get the ContentByte object
                        PdfContentByte cb = pdfStamper.GetUnderContent(i);
                        // Tell the cb that the next commands should be "bound" to this new layer
                        // Start Layer
                        cb.BeginLayer(layer);
                        PdfGState gState = new PdfGState();
                        gState.FillOpacity = 0.1f; // define opacity level
                        cb.SetGState(gState);
                        // set font size and style for layer water mark text
                        cb.SetFontAndSize(BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED), 10);
                        List<string> watermarkList = new List<string>();
                        float singleWaterMarkWidth = cb.GetEffectiveStringWidth(layerwarkmarktxt, false);
                        float fontHeight = 10;
                        //Work out the Watermark for a Single Line on the Page based on the Page Width
                        float currentWaterMarkWidth = 0;
                        while (currentWaterMarkWidth + singleWaterMarkWidth < rect.Width)
                        {
                            watermarkList.Add(layerwarkmarktxt);
                            currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        }
                        //watermarkList.Add(layerwarkmarktxt);
                        //currentWaterMarkWidth = cb.GetEffectiveStringWidth(string.Join(" ", watermarkList), false);
                        //Fill the Page with Lines of Watermarks
                        float currentYPos = rect.Height;
                        //cb.BeginText();
                        //var font = new Font(Font.FontFamily.HELVETICA, 60, Font.NORMAL, BaseColor.LIGHT_GRAY);
                        //ColumnText.ShowTextAligned(pdfWriter.DirectContent, Element.ALIGN_CENTER, new Phrase("PAID", font), 300, 400, 45);
                        while (currentYPos > 0)
                        {
                            //ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 550, 400, 45);
                            ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER, new Phrase(string.Join(" ", watermarkList), font), 200, 200, 30);
                            currentYPos -= fontHeight;
                        }
                        cb.EndLayer();
                    }

                }
                reader.Close();
                // Save file to destination location if required
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                //System.IO.File.WriteAllBytes(destinationpath, memoryStream.ToArray());
                return File(memoryStream.ToArray(), "application/pdf", destinationpath);
            }

            // send file to browse to open it from destination location.


        }
    }
}