using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Hàm hỗ trợ kiểm tra quyền Admin từ Session.
        /// Được đồng bộ với logic "Admin" => RedirectToAction("Index", "Admin") trong AccountController.
        /// </summary>
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return !string.IsNullOrEmpty(role) && role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        // ================= TRANG CHỦ ADMIN (DASHBOARD) =================
        public IActionResult Index()
        {
            // Bảo mật: Nếu không phải Admin thì đá về trang Login
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // 1. Thống kê số lượng dựa trên Role trong bảng User
            ViewBag.TotalPatients = _context.User.Count(u => u.Role == "Patient");
            ViewBag.TotalDoctors = _context.User.Count(u => u.Role == "Doctor");

            // 2. Thống kê lịch hẹn trong ngày hôm nay
            var today = DateTime.Today;
            ViewBag.TotalAppointments = _context.Appointment.Count(a => a.AppointmentDate.Date == today);

            // 3. Tính doanh thu tháng hiện tại
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Dùng decimal? để tránh lỗi Sum khi danh sách rỗng (Null)
            var revenue = _context.Appointment
                .Where(a => a.AppointmentDate.Month == currentMonth && a.AppointmentDate.Year == currentYear)
                .Sum(a => (decimal?)a.Price) ?? 0;

            ViewBag.TotalRevenue = revenue.ToString("N0", new CultureInfo("vi-VN")) + " VNĐ";

            // 4. Lấy 5 hoạt động (Lịch hẹn) gần đây nhất để hiển thị bảng
            var recentActivities = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .ToList();

            return View(recentActivities);
        }

        // ================= QUẢN LÝ DANH SÁCH BÁC SĨ =================
        public IActionResult Doctors()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Lấy danh sách bác sĩ kèm theo thông tin Chuyên khoa (Specialty)
            var doctors = _context.Doctor
                .Include(d => d.Specialty)
                .ToList();

            return View(doctors);
        }

        // ================= QUẢN LÝ TÀI KHOẢN NGƯỜI DÙNG =================
        public IActionResult UserList()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Hiển thị danh sách User mới nhất lên đầu
            var users = _context.User
                .OrderByDescending(u => u.UserId)
                .ToList();

            return View(users);
        }

        // ================= CÁC HÀM XỬ LÝ KHÁC (VÍ DỤ: XÓA/KHÓA USER) =================

        [HttpPost]
        public IActionResult ToggleUserStatus(int id)
        {
            if (!IsAdmin()) return Unauthorized();

            var user = _context.User.Find(id);
            if (user != null)
            {
                // Đảo ngược trạng thái hoạt động (Active/Inactive)
                user.IsActive = !user.IsActive;
                _context.SaveChanges();
            }

            return RedirectToAction("UserList");
        }
    }
}