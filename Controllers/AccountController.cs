using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using QuanLyPhongKhamVaDatLich.Data;
using QuanLyPhongKhamVaDatLich.Models;
using System;
using System.Linq;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================= XỬ LÝ ĐĂNG NHẬP =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View(model);
            }

            // Trim để tránh lỗi khoảng trắng khi copy-paste
            string loginInput = model.Email?.Trim();

            // KIỂM TRA: User phải khớp Username/Email VÀ Password VÀ Role đã chọn từ Dropdown
            var user = _context.User.FirstOrDefault(u =>
                (u.Email == loginInput || u.Username == loginInput)
                && u.Password == model.Password
                && u.Role == model.Role);

            if (user != null)
            {
                try
                {
                    // LƯU SESSION
                    HttpContext.Session.SetInt32("UserId", user.UserId);
                    HttpContext.Session.SetString("UserRole", user.Role ?? "Patient");
                    HttpContext.Session.SetString("UserName", user.Username ?? "User");

                    // Chuyển hướng theo Role (Chính xác từng chữ cái)
                    return user.Role switch
                    {
                        "Admin" => RedirectToAction("Index", "Admin"),
                        "Doctor" => RedirectToAction("Index", "Doctor"),
                        "Receptionist" => RedirectToAction("Index", "Receptionist"), // Trang cho Lễ tân
                        "Patient" => RedirectToAction("Index", "Home"),
                        _ => RedirectToAction("Index", "Home")
                    };
                }
                catch (Exception)
                {
                    ViewBag.Error = "Lỗi Session: Hãy đảm bảo đã cấu hình builder.Services.AddSession() trong Program.cs";
                    return View(model);
                }
            }

            ViewBag.Error = "Tài khoản, mật khẩu hoặc vai trò không chính xác!";
            return View(model);
        }

        // ================= XỬ LÝ ĐĂNG KÝ (CHO BỆNH NHÂN) =================

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string Username, string Password, string FullName, string Phone, string Email)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Email))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ các trường bắt buộc!";
                return View();
            }

            // Kiểm tra trùng lặp
            if (_context.User.Any(u => u.Email == Email || u.Username == Username))
            {
                ViewBag.Error = "Email hoặc tên đăng nhập đã tồn tại!";
                return View();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Tạo tài khoản User
                    var newUser = new User
                    {
                        Username = Username.Trim(),
                        Password = Password,
                        Role = "Patient", // Đăng ký mặc định là Bệnh nhân
                        Email = Email.Trim(),
                        IsActive = true
                    };
                    _context.User.Add(newUser);
                    _context.SaveChanges();

                    // 2. Tạo thông tin Bệnh nhân (Patient) liên kết với User vừa tạo
                    var newPatient = new Patient
                    {
                        FullName = FullName ?? "Người dùng mới",
                        Phone = Phone,
                        Email = Email.Trim(),
                        UserId = newUser.UserId,
                        IsActive = true
                    };
                    _context.Patient.Add(newPatient);
                    _context.SaveChanges();

                    transaction.Commit();
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ViewBag.Error = "Lỗi hệ thống: " + (ex.InnerException?.Message ?? ex.Message);
                    return View();
                }
            }
        }

        // ================= ĐĂNG XUẤT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa sạch Session
            return RedirectToAction("Login", "Account");
        }
    }
}