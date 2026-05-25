using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
using QuanLyPhongKhamVaDatLich.Models;
using System;
using System.Linq;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // DANH SÁCH LỊCH HẸN
        // =========================
        public IActionResult Index()
        {
            var appointments = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .ToList();

            return View(appointments);
        }

        // =========================
        // FORM ĐẶT LỊCH (HttpGet)
        // =========================
        public IActionResult Create()
        {
            // ensure user logged in as Patient
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");
            if (userId == null || role != "Patient") return RedirectToAction("Login", "Account");

            // prepare dropdowns
            ViewBag.Doctors = _context.Doctor.Include(d => d.Specialty).ToList();

            // return form bound to viewmodel
            return View(new AppointmentViewModel { AppointmentDate = DateTime.Today });
        }

        // =========================
        // LƯU LỊCH HẸN (HttpPost)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AppointmentViewModel model)
        {
            // ensure user logged in as Patient
            int? userId = HttpContext.Session.GetInt32("UserId");
            string? role = HttpContext.Session.GetString("UserRole");
            if (userId == null || role != "Patient") return RedirectToAction("Login", "Account");

            // re-populate dropdowns if returning view
            ViewBag.Doctors = _context.Doctor.Include(d => d.Specialty).ToList();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // find patient record for current user
            var patient = _context.Patient.FirstOrDefault(p => p.UserId == userId.Value);
            if (patient == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy hồ sơ bệnh nhân. Vui lòng cập nhật hồ sơ.");
                return View(model);
            }

            // combine date + time (expecting model.AppointmentTime like "14:30")
            DateTime appointmentDateTime = model.AppointmentDate.Date;
            if (!string.IsNullOrWhiteSpace(model.AppointmentTime))
            {
                if (TimeSpan.TryParse(model.AppointmentTime, out var ts))
                {
                    appointmentDateTime = model.AppointmentDate.Date + ts;
                }
                else
                {
                    ModelState.AddModelError(nameof(model.AppointmentTime), "Giờ khám không hợp lệ");
                    return View(model);
                }
            }

            // create and save appointment
            var appointment = new Appointment
            {
                PatientId = patient.PatientId,
                DoctorId = model.DoctorId,
                AppointmentDate = appointmentDateTime,
                Status = "Pending",
                Price = 0m
            };

            _context.Appointment.Add(appointment);
            _context.SaveChanges();

            TempData["Success"] = "Đặt lịch thành công.";
            return RedirectToAction("AppointmentList", "Patient");
        }
    }
}