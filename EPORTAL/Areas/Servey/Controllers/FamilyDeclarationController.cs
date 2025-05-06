using EPORTAL.Models;
using EPORTAL.ModelsView360;
using EPORTAL.ModelsView360.DTOs;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EPORTAL.Areas.Servey.Controllers
{
    public class FamilyDeclarationController : Controller
    {
        private EPORTALEntities _context = new EPORTALEntities();
        int IDQuyenHT = EPORTAL.Models.MyAuthentication.IDQuyenHT;
        String controll = "FamilyDeclaration";

        // GET: FamilyDeclaration
        public ActionResult Index()
        {

            var model = new KhaiBaoTongHopViewModel
            {
                NhanVien = new KhaiBaoNhanVienDto(),
                VoChong = new KhaiBaoVoChongDto(),
                DanhSachConCai = new List<KhaiBaoConCaiDto>()
            };

            var dataNhanVien = _context.KhaiBao_NhanVien
                .Where(x => x.MaNhanVien == MyAuthentication.Username)
                .FirstOrDefault();

            if (dataNhanVien != null)
            {
                ViewBag.DataNhanVien = dataNhanVien;
                ViewBag.NgayKhaiBao = dataNhanVien.NgayKhaiBao;
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(KhaiBaoTongHopViewModel model, IEnumerable<HttpPostedFileBase> DanhSachConCaiFiles)
        {
            try
            {
                var nhanvien = new KhaiBao_NhanVien();
                var vochong = new KhaiBao_VoChong();

                if (model.NhanVien.LoaiKhaiBao == 0)
                {
                    nhanvien.TenNhanVien = MyAuthentication.TenNV;
                    nhanvien.MaNhanVien = MyAuthentication.Username;
                    nhanvien.DaXacNhan = true;
                    nhanvien.NgayKhaiBao = DateTime.Now;
                    nhanvien.LoaiKhaiBao = 0;

                    _context.KhaiBao_NhanVien.Add(nhanvien);
                }
                else
                {
                    nhanvien.TenNhanVien = MyAuthentication.TenNV;
                    nhanvien.MaNhanVien = MyAuthentication.Username;
                    nhanvien.DaXacNhan = true;
                    nhanvien.NgayKhaiBao = DateTime.Now;
                    nhanvien.LoaiKhaiBao = 1;

                    _context.KhaiBao_NhanVien.Add(nhanvien);

                    vochong.TenVoChong = model.VoChong.TenVoChong;
                    vochong.NamSinhVoChong = model.VoChong.NamSinhVoChong;
                    vochong.MaNhanVien = nhanvien.MaNhanVien;

                    _context.KhaiBao_VoChong.Add(vochong);

                    var folderPath = Server.MapPath("~/Uploads/GiayKhaiSinh/");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    int index = 0;
                    if (model.DanhSachConCai != null)
                    {
                        foreach (var item in model.DanhSachConCai)
                        {
                            var concai = new KhaiBao_ConCai
                            {
                                TenCon = item.TenCon,
                                NamSinhCon = item.NamSinhCon,
                                MaNhanVien = nhanvien.MaNhanVien
                            };

                            if (DanhSachConCaiFiles != null && DanhSachConCaiFiles.Count() > index)
                            {
                                var file = DanhSachConCaiFiles.ElementAt(index);
                                if (file != null && file.ContentLength > 0)
                                {
                                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                                    var ext = Path.GetExtension(file.FileName).ToLower();

                                    if (allowedExtensions.Contains(ext))
                                    {
                                        var uniqueName = Guid.NewGuid().ToString() + ext;
                                        var fullPath = Path.Combine(folderPath, uniqueName);
                                        file.SaveAs(fullPath);

                                        concai.GiayKhaiSinh = "/Uploads/GiayKhaiSinh/" + uniqueName;
                                    }
                                }
                            }
                            _context.KhaiBao_ConCai.Add(concai);
                            index++;
                        }
                    }
                }

                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["msgError"] = $"<script>alert('Có lỗi xảy ra: {ex.Message}');</script>";
                return RedirectToAction("Index");
            }
        }

        public ActionResult ListFamilyDeclarations(int page = 1, int pageSize = 10, string searchMaNV = "")
        {
            var ListQuyen = new Models.MyAuthentication().GetPermisionCN(IDQuyenHT, controll);
            ViewBag.QUYENCN = ListQuyen;
            if (!ListQuyen.Contains("EDIT"))
            {
                TempData["msgError"] = "<script>alert('Bạn không có quyền thực hiện chức năng này');</script>";
                return RedirectToAction("Index");
            }
            var nhanVienQuery = _context.KhaiBao_NhanVien.AsQueryable();

            if (!string.IsNullOrEmpty(searchMaNV))
            {
                nhanVienQuery = nhanVienQuery.Where(x => x.MaNhanVien.Contains(searchMaNV));
            }

            var totalItems = nhanVienQuery.Count();

            var nhanVienPage = nhanVienQuery
                                .OrderBy(x => x.MaNhanVien)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();

            var maNVs = nhanVienPage.Select(x => x.MaNhanVien).ToList();

            var voChongDict = _context.KhaiBao_VoChong
                                .Where(x => maNVs.Contains(x.MaNhanVien))
                                .ToDictionary(x => x.MaNhanVien, x => x);

            var conCaiList = _context.KhaiBao_ConCai
                                .Where(x => maNVs.Contains(x.MaNhanVien))
                                .ToList();

            var viewDataList = nhanVienPage.Select(nv => new KhaiBaoTongHopViewModel
            {
                NhanVien = new KhaiBaoNhanVienDto
                {
                    KhaiBaoID = nv.KhaiBaoID,
                    MaNhanVien = nv.MaNhanVien,
                    TenNhanVien = nv.TenNhanVien,
                    LoaiKhaiBao = nv.LoaiKhaiBao,
                    NgayKhaiBao = nv.NgayKhaiBao
                },
                VoChong = voChongDict.ContainsKey(nv.MaNhanVien) ? new KhaiBaoVoChongDto
                {
                    VoChongID = voChongDict[nv.MaNhanVien].VoChongID,
                    MaNhanVien = voChongDict[nv.MaNhanVien].MaNhanVien,
                    TenVoChong = voChongDict[nv.MaNhanVien].TenVoChong,
                    NamSinhVoChong = voChongDict[nv.MaNhanVien].NamSinhVoChong
                } : null,
                DanhSachConCai = conCaiList
                    .Where(c => c.MaNhanVien == nv.MaNhanVien)
                    .Select(c => new KhaiBaoConCaiDto
                    {
                        ConCaiID = c.ConCaiID,
                        MaNhanVien = c.MaNhanVien,
                        TenCon = c.TenCon,
                        NamSinhCon = c.NamSinhCon,
                        GiayKhaiSinh = c.GiayKhaiSinh
                    }).ToList()
            }).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SearchMaNV = searchMaNV;
            ViewBag.ExistData = viewDataList.Count > 0;

            return View(viewDataList);
        }

        public ActionResult ExportExcel()
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                //ExcelPackage.Configuration.UseFastStreamProvider = false;

                var nhanVienList = _context.KhaiBao_NhanVien
                                        .OrderBy(x => x.MaNhanVien)
                                        .ToList();

                var maNVs = nhanVienList.Select(x => x.MaNhanVien).ToList();

                var voChongData = _context.KhaiBao_VoChong
                                        .Where(x => maNVs.Contains(x.MaNhanVien))
                                        .ToLookup(x => x.MaNhanVien);

                var conCaiData = _context.KhaiBao_ConCai
                                        .Where(x => maNVs.Contains(x.MaNhanVien))
                                        .ToLookup(x => x.MaNhanVien);

                using (var excelPackage = new ExcelPackage())
                {
                    var worksheet = excelPackage.Workbook.Worksheets.Add("DanhSach");

                    string[] headers = { "STT", "Mã NV", "Tên NV", "Vợ/Chồng", "Năm sinh Vợ/Chồng", "Tên con", "Năm sinh con", "Giấy khai sinh", "Ngày xác nhận" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    int row = 2;
                    int stt = 1;
                    foreach (var nhanVien in nhanVienList)
                    {
                        var voChong = voChongData[nhanVien.MaNhanVien].FirstOrDefault();
                        var conCaiList = conCaiData[nhanVien.MaNhanVien].ToList();

                        if (conCaiList.Any())
                        {
                            foreach (var con in conCaiList)
                            {
                                worksheet.Cells[row, 1].Value = stt++;
                                worksheet.Cells[row, 2].Value = nhanVien.MaNhanVien;
                                worksheet.Cells[row, 3].Value = nhanVien.TenNhanVien;
                                worksheet.Cells[row, 4].Value = voChong?.TenVoChong;
                                worksheet.Cells[row, 5].Value = voChong?.NamSinhVoChong;
                                worksheet.Cells[row, 6].Value = con.TenCon;
                                worksheet.Cells[row, 7].Value = con.NamSinhCon;
                                worksheet.Cells[row, 9].Value = nhanVien.NgayKhaiBao.Value.ToString("dd/MM/yyyy HH:mm:ss");

                                if (!string.IsNullOrEmpty(con.GiayKhaiSinh))
                                {
                                    try
                                    {
                                        var filePath = Server.MapPath(con.GiayKhaiSinh);
                                        if (System.IO.File.Exists(filePath))
                                        {
                                            var fileInfo = new FileInfo(filePath);
                                            worksheet.Column(8).Width = 33;
                                            worksheet.Row(row).Height = 220;
                                            var picture = worksheet.Drawings.AddPicture(
                                                $"img_{row}_{Guid.NewGuid()}",
                                                fileInfo
                                            );
                                            picture.SetPosition(row - 1, 0, 7, 3);
                                            picture.SetSize(360, 270);
                                        }
                                        else
                                        {
                                            worksheet.Cells[row, 8].Value = "[Không tìm thấy ảnh]";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        worksheet.Cells[row, 8].Value = "[Ảnh lỗi]";
                                    }
                                }
                                row++;
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, 1].Value = stt++;
                            worksheet.Cells[row, 2].Value = nhanVien.MaNhanVien;
                            worksheet.Cells[row, 3].Value = nhanVien.TenNhanVien;
                            worksheet.Cells[row, 4].Value = "";
                            worksheet.Cells[row, 5].Value = "";
                            worksheet.Cells[row, 6].Value = "";
                            worksheet.Cells[row, 7].Value = "";
                            worksheet.Cells[row, 9].Value = nhanVien.NgayKhaiBao.Value.ToString("dd/MM/yyyy HH:mm:ss");
                            row++;
                        }
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    using (var memoryStream = new MemoryStream())
                    {
                        excelPackage.SaveAs(memoryStream);
                        memoryStream.Position = 0;

                        return File(
                            memoryStream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"DanhSachGiaDinh_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi xuất Excel: {ex}");
                return Content("Đã xảy ra lỗi khi xuất file");
            }
        }

    }
}