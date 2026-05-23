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
            // Nếu đã đăng nhập bằng Session rồi thì tự động điều hướng đi luôn
            var currentRole = HttpContext.Session.GetString("UserRole");
            if (!string.IsNullOrEmpty(currentRole))
            {
                return RedirectBasedOnRole(currentRole);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Email/Username và Mật khẩu!";
                return View(model);
            }

            string loginInput = model.Email?.Trim();

            var user = _context.User.FirstOrDefault(u => u.Email == loginInput || u.Username == loginInput);

            if (user == null)
            {
                ViewBag.Error = "Tài khoản (Email hoặc Username) không tồn tại trên hệ thống!";
                return View(model);
            }

            if (user.Password != model.Password)
            {
                ViewBag.Error = "Mật khẩu không chính xác. Vui lòng thử lại!";
                return View(model);
            }

            if (user.Role != model.Role)
            {
                ViewBag.Error = $"Tài khoản này không thuộc nhóm quyền '{model.Role}'. Vui lòng kiểm tra lại!";
                return View(model);
            }

            if (user.IsActive == false)
            {
                ViewBag.Error = "Tài khoản của bạn hiện đang bị khóa!";
                return View(model);
            }

            try
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserRole", user.Role ?? "Patient");
                HttpContext.Session.SetString("UserName", user.Username ?? "User");

                // SỬA TẠI ĐÂY: Chuyển hướng "Patient" sang đúng Patient Controller
                return RedirectBasedOnRole(user.Role);
            }
            catch (Exception)
            {
                ViewBag.Error = "Lỗi hệ thống khi lưu Session. Hãy kiểm tra cấu hình Program.cs!";
                return View(model);
            }
        }

        // Hàm hỗ trợ điều hướng tập trung tránh lặp code
        private IActionResult RedirectBasedOnRole(string role)
        {
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Admin"),
                "Doctor" => RedirectToAction("Dashboard", "Doctor"),
                "Receptionist" => RedirectToAction("Index", "Receptionist"),
                "Patient" => RedirectToAction("Index", "Patient"),
                _ => RedirectToAction("Index", "Home")
            };
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
                ViewBag.Error = "Vui lòng nhập đầy đủ các trường bắt buộc (*)";
                return View();
            }

            if (_context.User.Any(u => u.Email == Email || u.Username == Username))
            {
                ViewBag.Error = "Email hoặc tên đăng nhập này đã được sử dụng!";
                return View();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var newUser = new User
                    {
                        Username = Username.Trim(),
                        Password = Password,
                        Role = "Patient",
                        Email = Email.Trim(),
                        IsActive = true
                    };
                    _context.User.Add(newUser);
                    _context.SaveChanges();

                    var newPatient = new Patient
                    {
                        FullName = FullName ?? "Người dùng mới",
                        Phone = Phone, // Chú ý trường này trong DB của bạn là Phone hay PhoneNumber nhé
                        Email = Email.Trim(),
                        UserId = newUser.UserId,
                        IsActive = true
                    };
                    _context.Patient.Add(newPatient);
                    _context.SaveChanges();

                    transaction.Commit();

                    TempData["Success"] = "Đăng ký tài khoản thành công!";
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
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}