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
        [Authorize]
        public ActionResult Index()
        {

            if (string.IsNullOrEmpty(MyAuthentication.Username) || string.IsNullOrEmpty(MyAuthentication.TenNV))
            {
                TempData["msgError"] = "<script>alert('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');</script>";
                return RedirectToAction("Index");
            }

            ViewBag.Authentication = MyAuthentication.Username;

            var model = new KhaiBaoTongHopViewModel
            {
                NhanVien = new KhaiBaoNhanVienDto(),
                VoChong = new KhaiBaoVoChongDto(),
                DanhSachConCai = new List<KhaiBaoConCaiDto>()
            };

            int currentYear = DateTime.Now.Year;
            int maxYear = currentYear - 17;

            ViewBag.VoChong_NamSinhVoChong = Enumerable.Range(1950, maxYear - 1950 + 1)
                .Reverse()
                .Select(y => new SelectListItem
                {
                    Value = y.ToString(),
                    Text = y.ToString()
                }).ToList();

            var dataNhanVien = _context.KhaiBao_NhanVien
                .Where(x => x.MaNhanVien == MyAuthentication.Username)
                .FirstOrDefault();

            if (dataNhanVien != null)
            {
                ViewBag.DataNhanVien = dataNhanVien;
                ViewBag.NgayKhaiBao = dataNhanVien.NgayKhaiBao;
            }

            var nullNhanVien = _context.KhaiBao_NhanVien.Count(x => x.MaNhanVien == null);
            var nullVoChong = _context.KhaiBao_VoChong.Count(x => x.MaNhanVien == null);
            var nullConCai = _context.KhaiBao_ConCai.Count(x => x.MaNhanVien == null);

            ViewBag.NullNhanVienCount = nullNhanVien;
            ViewBag.NullVoChongCount = nullVoChong;
            ViewBag.NullConCaiCount = nullConCai;

            var maNhanViensKhongCoGiayKhaiSinh = (
                from con in _context.KhaiBao_ConCai
                join nv in _context.NhanViens on con.MaNhanVien equals nv.MaNV
                join pb in _context.PhongBans on nv.IDPhongBan equals pb.IDPhongBan
                where string.IsNullOrEmpty(con.GiayKhaiSinh)
                select nv.MaNV + " - " + pb.TenPhongBan
            ).Distinct().ToList();


            ViewBag.MaNhanVienKhongGiayKhaiSinh = string.Join(", ", maNhanViensKhongCoGiayKhaiSinh);

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(KhaiBaoTongHopViewModel model, IEnumerable<HttpPostedFileBase> DanhSachConCaiFiles)
        {
            try
            {
                var nhanvien = new KhaiBao_NhanVien();
                var vochong = new KhaiBao_VoChong();

                if (string.IsNullOrEmpty(MyAuthentication.Username) || string.IsNullOrEmpty(MyAuthentication.TenNV))
                {
                    TempData["msgError"] = "<script>alert('Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');</script>";
                    return RedirectToAction("Index");
                }

                bool daKhaiBao = _context.KhaiBao_NhanVien.Any(x => x.MaNhanVien == MyAuthentication.Username);
                if (daKhaiBao)
                {
                    TempData["msgError"] = "<script>alert('Bạn đã khai báo rồi. Không thể khai báo lại.');</script>";
                    return RedirectToAction("Index");
                }

                nhanvien.TenNhanVien = MyAuthentication.TenNV;
                nhanvien.MaNhanVien = MyAuthentication.Username;
                nhanvien.DaXacNhan = true;
                nhanvien.NgayKhaiBao = DateTime.Now;
                nhanvien.LoaiKhaiBao = model.NhanVien.LoaiKhaiBao;

                _context.KhaiBao_NhanVien.Add(nhanvien);

                if (model.NhanVien.LoaiKhaiBao == 1)
                {
                    vochong.TenVoChong = model.VoChong.TenVoChong;
                    vochong.NamSinhVoChong = model.VoChong.NamSinhVoChong;
                    vochong.MaNhanVien = nhanvien.MaNhanVien;
                    _context.KhaiBao_VoChong.Add(vochong);
                }

                var folderPath = Server.MapPath("~/Uploads/GiayKhaiSinh/");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                bool fileError = false;
                int index = 0;

                if (model.DanhSachConCai != null)
                {
                    foreach (var item in model.DanhSachConCai)
                    {
                        if (DanhSachConCaiFiles != null && DanhSachConCaiFiles.Count() > index)
                        {
                            var file = DanhSachConCaiFiles.ElementAt(index);
                            if (file != null && file.ContentLength > 0)
                            {
                                // Kiểm tra ContentType
                                if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                                {
                                    fileError = true;
                                }
                            }
                        }
                        index++;
                    }

                    if (fileError)
                    {
                        TempData["msgError"] = "<script>alert('Định dạng file không hợp lệ. Chỉ chấp nhận file ảnh');</script>";
                        return RedirectToAction("Index");
                    }

                    index = 0;
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
                            if (file != null && file.ContentLength > 0 && file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                            {
                                var ext = Path.GetExtension(file.FileName).ToLower();
                                var uniqueName = Guid.NewGuid().ToString() + ext;
                                var fullPath = Path.Combine(folderPath, uniqueName);
                                file.SaveAs(fullPath);

                                concai.GiayKhaiSinh = "/Uploads/GiayKhaiSinh/" + uniqueName;
                            }
                        }

                        _context.KhaiBao_ConCai.Add(concai);
                        index++;
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

        public ActionResult ListFamilyDeclarations(int page = 1, int pageSize = 20, string searchMaNV = "")
        {
            ViewBag.Authentication = MyAuthentication.Username;
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
                                .OrderByDescending(x => x.NgayKhaiBao)
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
                // EPPlus 4.5.3.2
                string templatePath = Server.MapPath("~/App_Data/FamilyTemplate.xlsx");
                var templateFile = new FileInfo(templatePath);

                if (!templateFile.Exists)
                    return Content("Không tìm thấy file template!");

                var nhanVienList = _context.KhaiBao_NhanVien.OrderByDescending(x => x.NgayKhaiBao).ToList();
                var maNVs = nhanVienList.Select(x => x.MaNhanVien).ToList();
                var voChongData = _context.KhaiBao_VoChong.Where(x => maNVs.Contains(x.MaNhanVien)).ToLookup(x => x.MaNhanVien);
                var conCaiData = _context.KhaiBao_ConCai.Where(x => maNVs.Contains(x.MaNhanVien)).ToLookup(x => x.MaNhanVien);

                using (var package = new ExcelPackage(templateFile))
                {
                    var worksheet = package.Workbook.Worksheets["DanhSach"];
                    if (worksheet == null)
                        return Content("Không tìm thấy worksheet 'DanhSach' trong template!");

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
                                worksheet.Cells[row, 4].Value = voChong != null ? voChong.TenVoChong : "";
                                worksheet.Cells[row, 5].Value = voChong != null ? voChong.NamSinhVoChong.ToString() : "";
                                worksheet.Cells[row, 6].Value = con.TenCon;
                                worksheet.Cells[row, 7].Value = con.NamSinhCon;
                                worksheet.Cells[row, 9].Value = nhanVien.NgayKhaiBao.HasValue
                                    ? nhanVien.NgayKhaiBao.Value.ToString("dd/MM/yyyy HH:mm:ss")
                                    : "";

                                if (!string.IsNullOrEmpty(con.GiayKhaiSinh))
                                {
                                    try
                                    {
                                        var imagePath = Server.MapPath(con.GiayKhaiSinh);
                                        if (System.IO.File.Exists(imagePath))
                                        {
                                            string fileUrl = Request.Url.GetLeftPart(UriPartial.Authority) + Url.Content(con.GiayKhaiSinh);

                                            worksheet.Cells[row, 8].Hyperlink = new Uri(fileUrl);
                                            worksheet.Cells[row, 8].Value = "Xem ảnh";
                                            worksheet.Cells[row, 8].Style.Font.UnderLine = true;
                                            worksheet.Cells[row, 8].Style.Font.Color.SetColor(Color.Blue);
                                        }
                                        else
                                        {
                                            worksheet.Cells[row, 8].Value = "[Không tìm thấy ảnh]";
                                        }
                                    }
                                    catch
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
                            worksheet.Cells[row, 4].Value = voChong != null ? voChong.TenVoChong : "";
                            worksheet.Cells[row, 5].Value = voChong != null ? voChong.NamSinhVoChong.ToString() : "";
                            worksheet.Cells[row, 6].Value = "";
                            worksheet.Cells[row, 7].Value = "";
                            worksheet.Cells[row, 8].Value = "";
                            worksheet.Cells[row, 9].Value = nhanVien.NgayKhaiBao.HasValue
                                ? nhanVien.NgayKhaiBao.Value.ToString("dd/MM/yyyy HH:mm:ss")
                                : "";

                            row++;
                        }
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    using (var stream = new MemoryStream())
                    {
                        package.SaveAs(stream);
                        stream.Position = 0;

                        return File(
                            stream.ToArray(),
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"DanhSachGiaDinh_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                return Content("Xảy ra lỗi khi xuất file: " + ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult CleanUpInvalidFamilyDeclarations()
        {
            try
            {
                var invalidConCai = _context.KhaiBao_ConCai
                    .Where(x => x.MaNhanVien == null)
                    .ToList();
                if (invalidConCai.Any())
                {
                    _context.KhaiBao_ConCai.RemoveRange(invalidConCai);
                }

                var invalidVoChong = _context.KhaiBao_VoChong
                    .Where(x => x.MaNhanVien == null)
                    .ToList();
                if (invalidVoChong.Any())
                {
                    _context.KhaiBao_VoChong.RemoveRange(invalidVoChong);
                }

                var invalidNhanVien = _context.KhaiBao_NhanVien
                    .Where(x => x.MaNhanVien == null)
                    .ToList();
                if (invalidNhanVien.Any())
                {
                    _context.KhaiBao_NhanVien.RemoveRange(invalidNhanVien);
                }

                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Đã xóa các bản ghi không hợp lệ thành công.');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgError"] = $"<script>alert('Lỗi khi xóa dữ liệu: {ex.Message}');</script>";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeleteAllFamilyDeclarations()
        {
            try
            {
                _context.KhaiBao_ConCai.RemoveRange(_context.KhaiBao_ConCai);
                _context.KhaiBao_VoChong.RemoveRange(_context.KhaiBao_VoChong);
                _context.KhaiBao_NhanVien.RemoveRange(_context.KhaiBao_NhanVien);

                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Đã xóa toàn bộ dữ liệu khai báo.');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgError"] = $"<script>alert('Lỗi khi xóa dữ liệu: {ex.Message}');</script>";
            }

            return RedirectToAction("Index");
        }

        public ActionResult XoaDataTest(string maNhanVien = "HPDQ32889")
        {
            if (string.IsNullOrEmpty(maNhanVien))
            {
                TempData["msgError"] = "<script>alert('Mã nhân viên không hợp lệ');</script>";
                return RedirectToAction("Index");
            }

            try
            {
                var conCaiList = _context.KhaiBao_ConCai.Where(x => x.MaNhanVien == maNhanVien).ToList();
                _context.KhaiBao_ConCai.RemoveRange(conCaiList);

                var voChongList = _context.KhaiBao_VoChong.Where(x => x.MaNhanVien == maNhanVien).ToList();
                _context.KhaiBao_VoChong.RemoveRange(voChongList);

                var nhanVienList = _context.KhaiBao_NhanVien.Where(x => x.MaNhanVien == maNhanVien).ToList();
                _context.KhaiBao_NhanVien.RemoveRange(nhanVienList);

                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Đã xóa tất cả bản ghi của nhân viên " + maNhanVien + "');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgError"] = $"<script>alert('Lỗi khi xóa: {ex.Message}');</script>";
            }

            return RedirectToAction("Index");
        }

        public ActionResult XoaTatCaTheoMaNhanVien(string maNhanVien)
        {
            ViewBag.Authentication = MyAuthentication.Username;

            if (string.IsNullOrEmpty(maNhanVien))
            {
                TempData["msgError"] = "<script>alert('Mã nhân viên không hợp lệ');</script>";
                return RedirectToAction("Index");
            }

            try
            {
                var conCaiList = _context.KhaiBao_ConCai.Where(x => x.MaNhanVien == maNhanVien).ToList();
                _context.KhaiBao_ConCai.RemoveRange(conCaiList);

                var voChongList = _context.KhaiBao_VoChong.Where(x => x.MaNhanVien == maNhanVien).ToList();
                _context.KhaiBao_VoChong.RemoveRange(voChongList);

                var nhanVienList = _context.KhaiBao_NhanVien.Where(x => x.MaNhanVien == maNhanVien).ToList();
                _context.KhaiBao_NhanVien.RemoveRange(nhanVienList);

                _context.SaveChanges();

                TempData["msgSuccess"] = "<script>alert('Đã xóa tất cả bản ghi của nhân viên " + maNhanVien + "');</script>";
            }
            catch (Exception ex)
            {
                TempData["msgError"] = $"<script>alert('Lỗi khi xóa: {ex.Message}');</script>";
            }

            return RedirectToAction("Index");
        }

    }
}