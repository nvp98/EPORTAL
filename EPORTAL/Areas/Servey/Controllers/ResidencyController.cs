using ClosedXML.Excel;
using EPORTAL.Models;
using EPORTAL.ModelsResidency;
using EPORTAL.ModelsView360;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EPORTAL.Controllers
{
    public class ResidencyController : Controller
    {
        private readonly string _baseApiUrl = "https://production.cas.so/address-kit";
        EPORTALEntities _context = new EPORTALEntities();

        // GET: /Residency
        public async Task<ActionResult> Index()
        {
            var config = _context.KhaiBao_CauHinh.FirstOrDefault();

            ViewBag.NgayDenHan = config.NgayDenHan;
            bool hetHan = (config.NgayDenHan.HasValue && DateTime.Now.Date > config.NgayDenHan.Value);
            ViewBag.HetHan = hetHan;

            var model = new ResidencyViewModel();
            model.Provinces = await LoadProvincesAsync();
            return View(model);
        }

        private async Task<List<Province>> LoadProvincesAsync()
        {
            using (var client = new HttpClient())
            {
                string url = $"{_baseApiUrl}/latest/provinces";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                // Deserialize
                var result = JsonConvert.DeserializeObject<ProvinceResponse>(json);
                return result?.provinces ?? new List<Province>();
            }
        }

        private async Task<List<Commune>> LoadCommunesAsync(string provinceId)
        {
            using (var client = new HttpClient())
            {
                string url = $"{_baseApiUrl}/latest/provinces/{provinceId}/communes";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();

                // Deserialize
                var result = JsonConvert.DeserializeObject<CommuneResponse>(json);
                return result?.communes ?? new List<Commune>();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCommunes(string provinceId)
        {
            if (string.IsNullOrEmpty(provinceId))
            {
                return Json(new { ok = false, message = "Thiếu provinceId" }, JsonRequestBehavior.AllowGet);
            }

            var communes = await LoadCommunesAsync(provinceId);
            return Json(new { ok = true, data = communes }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(ResidencyViewModel model)
        {
            try
            {
                var nhanVien = _context.NhanViens.FirstOrDefault(x => x.ID == MyAuthentication.ID);
                if (nhanVien == null)
                {
                    return Json(new { ok = false, message = "Không tìm thấy thông tin nhân viên." });
                }

                var existing = _context.KhaiBao_ThongTinCuTru
                    .FirstOrDefault(x => x.NhanVienID == MyAuthentication.ID);

                bool isNew = false;
                if (existing == null)
                {
                    existing = new KhaiBao_ThongTinCuTru
                    {
                        NhanVienID = MyAuthentication.ID
                    };
                    isNew = true;
                }

                existing.HoTen = nhanVien.HoTen;
                existing.MaNhanVien = nhanVien.MaNV;
                existing.NgayCapNhat = DateTime.Now;

                if (model.ChangeOption == "nochange")
                {
                    existing.TinhThuongTru = "Không thay đổi so với app nhân sự";
                    existing.MaTinhThuongTru = "";

                    existing.XaPhuongThuongTru = "Không thay đổi so với app nhân sự";
                    existing.MaXaPhuongThuongTru = "";

                    existing.SoNhaThuongTru = "Không thay đổi so với app nhân sự";
                    existing.ThonPhoThuongTru = "Không thay đổi so với app nhân sự";

                    existing.TinhHienTai = "Không thay đổi so với app nhân sự";
                    existing.MaTinhHienTai = "";
                    existing.XaPhuongHienTai = "Không thay đổi so với app nhân sự";
                    existing.MaXaPhuongHienTai = "";
                    existing.SoNhaHienTai = "Không thay đổi so với app nhân sự";
                    existing.ThonPhoHienTai = "Không thay đổi so với app nhân sự";

                    existing.CoThayDoi = false;
                }
                else
                {
                    existing.TinhThuongTru = model.PermanentProvinceName;
                    existing.MaTinhThuongTru = model.PermanentProvinceCode;

                    existing.XaPhuongThuongTru = model.PermanentCommuneName;
                    existing.MaXaPhuongThuongTru = model.PermanentCommuneCode;

                    existing.SoNhaThuongTru = model.PermanentAddress01;
                    existing.ThonPhoThuongTru = model.PermanentAddress02;

                    existing.TinhHienTai = model.CurrentProvinceName;
                    existing.MaTinhHienTai = model.CurrentProvinceCode;

                    existing.XaPhuongHienTai = model.CurrentCommuneName;
                    existing.MaXaPhuongHienTai = model.CurrentCommuneCode;

                    existing.SoNhaHienTai = model.CurrentAddress01;
                    existing.ThonPhoHienTai = model.CurrentAddress02;

                    existing.CoThayDoi = true;
                }

                existing.SDTCaNhan = model.PhonePersonal;
                existing.HoTenNguoiThan = model.RelativeName;
                existing.SDTNguoiThan = model.RelativePhone;

                existing.HoTenBo = model.FatherName;
                existing.NamSinhBo = model.FatherYob;
                existing.HoTenMe = model.MotherName;
                existing.NamSinhMe = model.MotherYob;
                existing.HoTenVoChong = model.SpouseName;
                existing.NamSinhVoChong = model.SpouseYob;

                _context.Entry(existing).State = isNew ? EntityState.Added : EntityState.Modified;

                _context.SaveChanges();

                var existingChildren = _context.KhaiBao_ThongTinCon
                    .Where(x => x.KhaiBaoID == existing.Id)
                    .ToList();

                _context.KhaiBao_ThongTinCon.RemoveRange(existingChildren);

                if (model.Children != null)
                {
                    foreach (var c in model.Children)
                    {
                        if (!string.IsNullOrEmpty(c.HoTen))
                        {
                            _context.KhaiBao_ThongTinCon.Add(new KhaiBao_ThongTinCon
                            {
                                KhaiBaoID = existing.Id,
                                HoTen = c.HoTen,
                                NamSinh = c.NamSinh
                            });
                        }
                    }
                }

                _context.SaveChanges();

                return Json(new { ok = true, message = "Lưu thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public ActionResult Details()
        {
            var nhanVienId = MyAuthentication.ID;
            var data = _context.KhaiBao_ThongTinCuTru
                .FirstOrDefault(x => x.NhanVienID == nhanVienId);

            if (data == null)
            {
                return RedirectToAction("Index");
            }

            var children = _context.KhaiBao_ThongTinCon
               .Where(c => c.KhaiBaoID == data.Id)
               .ToList();

            ViewBag.Children = children;

            return View(data);
        }

        public ActionResult List(string search, int page = 1)
        {
            int pageSize = 50;

            var query = from r in _context.KhaiBao_ThongTinCuTru
                        join e in _context.NhanViens on r.NhanVienID equals e.ID
                        join vt in _context.Vitris on e.IDViTri equals vt.IDViTri into vtGroup
                        from vt in vtGroup.DefaultIfEmpty()
                        join pb in _context.PhongBans on e.IDPhongBan equals pb.IDPhongBan into pbGroup
                        from pb in pbGroup.DefaultIfEmpty()
                        select new ResidencyViewModel
                        {
                            EmployeeId = e.ID,
                            EmployeeName = e.HoTen,
                            MaNhanVien = e.MaNV,

                            TenViTri = vt != null ? vt.TenViTri : "",
                            TenPhongBan = pb != null ? pb.TenPhongBan : "",

                            PermanentProvinceName = r.TinhThuongTru,
                            PermanentCommuneName = r.XaPhuongThuongTru,
                            PermanentAddress01 = r.SoNhaThuongTru,
                            PermanentAddress02 = r.ThonPhoThuongTru,

                            CurrentProvinceName = r.TinhHienTai,
                            CurrentCommuneName = r.XaPhuongHienTai,
                            CurrentAddress01 = r.SoNhaHienTai,
                            CurrentAddress02 = r.ThonPhoHienTai,

                            IsChange = (bool) r.CoThayDoi,

                            UpdateDate = r.NgayCapNhat,

                            PhonePersonal = r.SDTCaNhan,
                            RelativeName = r.HoTenNguoiThan,
                            RelativePhone = r.SDTNguoiThan,

                            FatherName = r.HoTenBo,
                            FatherYob = r.NamSinhBo,
                            MotherName = r.HoTenMe,
                            MotherYob = r.NamSinhMe,
                            SpouseName = r.HoTenVoChong,
                            SpouseYob = r.NamSinhVoChong,

                            Children = _context.KhaiBao_ThongTinCon
                            .Where(c => c.KhaiBaoID == r.Id)
                            .Select(c => new ChildModel
                            {
                                HoTen = c.HoTen,
                                NamSinh = c.NamSinh
                            })
                            .ToList()
                        };

            if (!string.IsNullOrEmpty(search))
            {
                string keyword = search.Trim().ToLower();
                query = query.Where(x => x.EmployeeName.ToLower().Contains(keyword) || x.MaNhanVien.ToLower().Contains(keyword));
            }

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var data = query
            .OrderByDescending(x => x.UpdateDate)
            .ThenBy(x => x.EmployeeName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(data);
        }

        public ActionResult Deadline()
        {
            var model = _context.KhaiBao_CauHinh.FirstOrDefault() ?? new KhaiBao_CauHinh();
            return View(model);
        }

        [HttpPost]
        public ActionResult Deadline(KhaiBao_CauHinh model)
        {
            var deadline = _context.KhaiBao_CauHinh.FirstOrDefault();

            if (deadline != null)
            {
                deadline.NgayDenHan = model.NgayDenHan;
                deadline.NgayCapNhat = DateTime.Now;
                _context.Entry(deadline).State = EntityState.Modified;
            }
            else
            {
                model.NgayCapNhat = DateTime.Now;
                _context.KhaiBao_CauHinh.Add(model);
            }

            _context.SaveChanges();

            TempData["msgSuccess"] = "<script>alert('Cập nhật ngày đến hạn thành công.');</script>";

            return RedirectToAction("Deadline");
        }

        public ActionResult ExportToExcel(string search = "")
        {
            var query = from r in _context.KhaiBao_ThongTinCuTru
                        join e in _context.NhanViens on r.NhanVienID equals e.ID
                        join vt in _context.Vitris on e.IDViTri equals vt.IDViTri into vtGroup
                        from vt in vtGroup.DefaultIfEmpty()
                        join pb in _context.PhongBans on e.IDPhongBan equals pb.IDPhongBan into pbGroup
                        from pb in pbGroup.DefaultIfEmpty()
                        select new
                        {
                            e.MaNV,
                            e.HoTen,
                            TenViTri = vt != null ? vt.TenViTri : "",
                            TenPhongBan = pb != null ? pb.TenPhongBan : "",

                            r.TinhThuongTru,
                            r.XaPhuongThuongTru,
                            r.SoNhaThuongTru,
                            r.ThonPhoThuongTru,

                            r.TinhHienTai,
                            r.XaPhuongHienTai,
                            r.SoNhaHienTai,
                            r.ThonPhoHienTai,

                            r.CoThayDoi,
                            r.NgayCapNhat,

                            r.SDTCaNhan,
                            r.HoTenNguoiThan,
                            r.SDTNguoiThan,

                            r.HoTenBo,
                            r.NamSinhBo,
                            r.HoTenMe,
                            r.NamSinhMe,
                            r.HoTenVoChong,
                            r.NamSinhVoChong,

                            Children = _context.KhaiBao_ThongTinCon
                            .Where(c => c.KhaiBaoID == r.Id)
                            .Select(c => new { c.HoTen, c.NamSinh })
                            .ToList()
                        };

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(x => x.HoTen.Contains(search) || x.MaNV.Contains(search));
            }

            var data = query.OrderByDescending(x => x.NgayCapNhat).ToList();

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("DanhSachKhaiBao");
                int row = 1;

                ws.Cell(row, 1).Value = "STT";
                ws.Cell(row, 2).Value = "Mã nhân viên";
                ws.Cell(row, 3).Value = "Họ và tên";
                ws.Cell(row, 4).Value = "Vị trí công việc";
                ws.Cell(row, 5).Value = "Đơn vị";
                ws.Cell(row, 6).Value = "Tỉnh/TP hộ khẩu";
                ws.Cell(row, 7).Value = "Xã/Phường hộ khẩu";
                ws.Cell(row, 8).Value = "Xóm/Số nhà hộ khẩu";
                ws.Cell(row, 9).Value = "Thôn/Phố hộ khẩu";
                ws.Cell(row, 10).Value = "Tỉnh/TP hiện tại";
                ws.Cell(row, 11).Value = "Xã/Phường hiện tại";
                ws.Cell(row, 12).Value = "Xóm/Số nhà hiện tại";
                ws.Cell(row, 13).Value = "Thôn/Phố hiện tại";
                ws.Cell(row, 14).Value = "SĐT cá nhân";
                ws.Cell(row, 15).Value = "Họ tên người thân";
                ws.Cell(row, 16).Value = "SĐT người thân";
                ws.Cell(row, 17).Value = "Họ tên bố";
                ws.Cell(row, 18).Value = "Năm sinh bố";
                ws.Cell(row, 19).Value = "Họ tên mẹ";
                ws.Cell(row, 20).Value = "Năm sinh mẹ";
                ws.Cell(row, 21).Value = "Họ tên vợ/chồng";
                ws.Cell(row, 22).Value = "Năm sinh vợ/chồng";
                ws.Cell(row, 23).Value = "Họ tên con";
                ws.Cell(row, 24).Value = "Năm sinh con";

                var headerRange = ws.Range(row, 1, row, 24);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Data
                int index = 1;
                foreach (var item in data)
                {
                    row++;
                    ws.Cell(row, 1).Value = index;
                    ws.Cell(row, 2).Value = item.MaNV;
                    ws.Cell(row, 3).Value = item.HoTen;
                    ws.Cell(row, 4).Value = item.TenViTri;
                    ws.Cell(row, 5).Value = item.TenPhongBan;

                    if (item.CoThayDoi == false)
                    {
                        // No change
                        for (int col = 6; col <= 24; col++)
                        {
                            ws.Cell(row, col).Value = "Không thay đổi so với app nhân sự";
                        }

                        // Format
                        ws.Range(row, 6, row, 24).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        ws.Range(row, 6, row, 24).Style.Font.Italic = true;
                        ws.Range(row, 6, row, 24).Style.Font.FontColor = XLColor.Gray;
                    }
                    else
                    {
                        // Change
                        ws.Cell(row, 6).Value = item.TinhThuongTru;
                        ws.Cell(row, 7).Value = item.XaPhuongThuongTru;
                        ws.Cell(row, 8).Value = item.SoNhaThuongTru;
                        ws.Cell(row, 9).Value = item.ThonPhoThuongTru;
                        ws.Cell(row, 10).Value = item.TinhHienTai;
                        ws.Cell(row, 11).Value = item.XaPhuongHienTai;
                        ws.Cell(row, 12).Value = item.SoNhaHienTai;
                        ws.Cell(row, 13).Value = item.ThonPhoHienTai;
                        ws.Cell(row, 14).Value = item.SDTCaNhan;
                        ws.Cell(row, 15).Value = item.HoTenNguoiThan;
                        ws.Cell(row, 16).Value = item.SDTNguoiThan;

                        ws.Cell(row, 17).Value = item.HoTenBo;
                        ws.Cell(row, 18).Value = item.NamSinhBo;
                        ws.Cell(row, 19).Value = item.HoTenMe;
                        ws.Cell(row, 20).Value = item.NamSinhMe;
                        ws.Cell(row, 21).Value = item.HoTenVoChong;
                        ws.Cell(row, 22).Value = item.NamSinhVoChong;

                        if (item.Children != null && item.Children.Any())
                        {
                            ws.Cell(row, 23).Value = string.Join("\n", item.Children.Select(c => c.HoTen));
                            ws.Cell(row, 24).Value = string.Join("\n", item.Children.Select(c => c.NamSinh));

                            ws.Cell(row, 23).Style.Alignment.WrapText = true;
                            ws.Cell(row, 24).Style.Alignment.WrapText = true;
                        }
                    }

                    index++;
                }

                ws.Columns().AdjustToContents();
                ws.SheetView.FreezeRows(1);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;
                    string fileName = $"KhaiBaoCuTru_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
                }
            }
        }
    }
}