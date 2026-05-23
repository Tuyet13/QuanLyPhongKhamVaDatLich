using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
using QuanLyPhongKhamVaDatLich.Models;
using Microsoft.AspNetCore.Http;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền truy cập (Chỉ Admin mới có quyền quản lý danh sách này)
        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // GET: Doctors hoặc Doctors/Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Lấy danh sách bác sĩ kèm theo thông tin Chuyên khoa (Specialty) và Tài khoản (User)
            var doctors = await _context.Doctor
                .Include(d => d.Specialty)
                .ToListAsync();

            // Trỏ trực tiếp đến file View nằm trong thư mục Views/Doctors/Index.cshtml
            return View("~/Views/Doctors/Index.cshtml", doctors);
        }

        // GET: Doctors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();

            var doctor = await _context.Doctor
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(m => m.DoctorId == id);

            if (doctor == null) return NotFound();

            // Lấy thêm email hiển thị từ bảng User
            var user = await _context.User.FindAsync(doctor.UserId);
            ViewBag.Email = user?.Email ?? "Chưa thiết lập";

            return View("~/Views/Doctors/Details.cshtml", doctor);
        }

        // GET: Doctors/Create
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.Specialties = _context.Specialty.ToList();
            return View("~/Views/Doctors/Create.cshtml");
        }

        // POST: Doctors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Doctor doctor, string Email, string Password)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (_context.User.Any(u => u.Email == Email))
            {
                TempData["Error"] = "Email tài khoản này đã tồn tại!";
                ViewBag.Specialties = _context.Specialty.ToList();
                return View("~/Views/Doctors/Create.cshtml", doctor);
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. Tạo User hệ thống
                    var newUser = new User
                    {
                        Username = Email.Split('@')[0],
                        Email = Email.Trim(),
                        Password = Password,
                        Role = "Doctor",
                        IsActive = true
                    };
                    _context.User.Add(newUser);
                    await _context.SaveChangesAsync();

                    // 2. Liên kết UserId sang thực thể Doctor
                    doctor.UserId = newUser.UserId;
                    _context.Doctor.Add(doctor);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["Success"] = "Thêm mới bác sĩ và cấp tài khoản thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Có lỗi hệ thống xảy ra khi lưu thông tin.");
                }
            }

            ViewBag.Specialties = _context.Specialty.ToList();
            return View("~/Views/Doctors/Create.cshtml", doctor);
        }

        // GET: Doctors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (id == null) return NotFound();

            var doctor = await _context.Doctor.FindAsync(id);
            if (doctor == null) return NotFound();

            ViewBag.Specialties = _context.Specialty.ToList();
            return View("~/Views/Doctors/Edit.cshtml", doctor);
        }

        // POST: Doctors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Doctor doctor)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            if (id != doctor.DoctorId) return NotFound();

            try
            {
                var existingDoctor = await _context.Doctor.FindAsync(id);
                if (existingDoctor != null)
                {
                    existingDoctor.FullName = doctor.FullName;
                    existingDoctor.SpecialtyId = doctor.SpecialtyId;
                    existingDoctor.Phone = doctor.Phone;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin bác sĩ thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Không thể cập nhật cơ sở dữ liệu.";
            }

            ViewBag.Specialties = _context.Specialty.ToList();
            return View("~/Views/Doctors/Edit.cshtml", doctor);
        }

        // POST: Doctors/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var doctor = await _context.Doctor.FindAsync(id);
            if (doctor != null)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        var userId = doctor.UserId;

                        // Xóa bác sĩ trước
                        _context.Doctor.Remove(doctor);
                        await _context.SaveChangesAsync();

                        // Xóa tài khoản liên kết sau
                        var user = await _context.User.FindAsync(userId);
                        if (user != null)
                        {
                            _context.User.Remove(user);
                        }

                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        TempData["Success"] = "Đã gỡ bỏ hồ sơ bác sĩ và tài khoản hệ thống.";
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Không thể xóa! Nhân sự đã có dữ liệu lịch hẹn hoặc bệnh án phát sinh.";
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}