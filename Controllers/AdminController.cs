using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
using QuanLyPhongKhamVaDatLich.Models;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        // 1. DASHBOARD
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            ViewBag.TotalPatients = await _context.Patient.CountAsync();
            ViewBag.TotalDoctors = await _context.Doctor.CountAsync();
            ViewBag.TotalReceptionists = await _context.Receptionist.CountAsync();

            var today = DateTime.Today;
            ViewBag.TotalAppointments = await _context.Appointment.CountAsync(a => a.AppointmentDate.Date == today);

            // Giả định doanh thu dựa trên số lượng lịch hẹn (có thể thay đổi logic tùy nhu cầu)
            int totalAppointmentsCount = await _context.Appointment.CountAsync();
            decimal totalRevenue = totalAppointmentsCount * 150000;
            ViewBag.TotalRevenue = totalRevenue.ToString("N0") + " đ";

            var todayAppointments = await _context.Appointment
                .Include(a => a.Doctor)
                .Where(a => a.AppointmentDate.Date == today)
                .OrderByDescending(a => a.AppointmentId)
                .Take(5)
                .ToListAsync();

            return View("~/Views/Admin/Index.cshtml", todayAppointments);
        }

        // 2. NHÂN SỰ CHUYÊN SÂU
        public async Task<IActionResult> StaffPerformance()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var doctorList = await _context.Doctor.ToListAsync();
            var receptionistList = await _context.Receptionist.ToListAsync();
            var performanceData = new List<StaffPerformanceViewModel>();

            // Xử lý Bác sĩ
            foreach (var doc in doctorList)
            {
                int totalLeave = await _context.LeaveRequest.CountAsync(l => l.UserId == doc.UserId && l.LeaveDate.Month == today.Month && l.Status == "Approved");
                int workingDays = Math.Max(0, 26 - totalLeave);

                performanceData.Add(new StaffPerformanceViewModel
                {
                    Id = doc.DoctorId,
                    FullName = doc.FullName,
                    Role = "Bác sĩ",
                    StatusToday = await _context.LeaveRequest.AnyAsync(l => l.UserId == doc.UserId && l.LeaveDate.Date == today && l.Status == "Approved") ? "Nghỉ phép ⛔" : "Đang làm việc ✅",
                    EstimatedSalary = workingDays * doc.BaseSalary,
                    TotalWorkCount = await _context.Appointment.CountAsync(a => a.DoctorId == doc.DoctorId),
                    Rating = doc.Rating
                });
            }

            // Xử lý Lễ tân
            foreach (var recep in receptionistList)
            {
                int totalLeave = await _context.LeaveRequest.CountAsync(l => l.UserId == recep.UserId && l.LeaveDate.Month == today.Month && l.Status == "Approved");
                int workingDays = Math.Max(0, 26 - totalLeave);

                performanceData.Add(new StaffPerformanceViewModel
                {
                    Id = recep.ReceptionistId,
                    FullName = recep.FullName,
                    Role = "Lễ tân",
                    StatusToday = await _context.LeaveRequest.AnyAsync(l => l.UserId == recep.UserId && l.LeaveDate.Date == today && l.Status == "Approved") ? "Nghỉ phép ⛔" : "Đang làm việc ✅",
                    EstimatedSalary = workingDays * recep.BaseSalary,
                    TotalWorkCount = await _context.Appointment.CountAsync(), // Nếu cần đếm cụ thể, hãy bổ sung khóa ngoại
                    Rating = 5.0
                });
            }

            return View("~/Views/Admin/StaffPerformance.cshtml", performanceData);
        }

        // 3. QUẢN LÝ BÁC SĨ
        public async Task<IActionResult> Doctors() => View("~/Views/Admin/doctor/Index.cshtml", await _context.Doctor.Include(d => d.Specialty).ToListAsync());

        public async Task<IActionResult> CreateDoctor()
        {
            ViewBag.Specialties = await _context.Specialty.ToListAsync();
            return View("~/Views/Admin/doctor/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(Doctor doctor, string Email, string Password)
        {
            if (await _context.User.AnyAsync(u => u.Email == Email))
            {
                TempData["Error"] = "Email đã tồn tại!";
                ViewBag.Specialties = await _context.Specialty.ToListAsync();
                return View("~/Views/Admin/doctor/Create.cshtml", doctor);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
<<<<<<< HEAD
                var user = new User { Username = Email, Email = Email, Password = Password, Role = "Doctor", IsActive = true };
=======
                var user = new User { Email = Email, Password = Password, Role = "Doctor", IsActive = true };
>>>>>>> 0c8a645e71df325eae5dda7c8fb73e0b24a272a1
                _context.User.Add(user);
                await _context.SaveChangesAsync();

                doctor.UserId = user.UserId;
                _context.Doctor.Add(doctor);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return RedirectToAction("Doctors");
            }
<<<<<<< HEAD
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                TempData["Error"] = ex.Message;

                ViewBag.Specialties = await _context.Specialty.ToListAsync();

                return View("~/Views/Admin/doctor/Create.cshtml", doctor);
            }
=======
            catch { await transaction.RollbackAsync(); return View(doctor); }
>>>>>>> 0c8a645e71df325eae5dda7c8fb73e0b24a272a1
        }

        // 4. QUẢN LÝ LỄ TÂN
        public async Task<IActionResult> Receptionists() => View("~/Views/Admin/leTan/Index.cshtml", await _context.Receptionist.Include(r => r.User).ToListAsync());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReceptionist(int id)
        {
            var recep = await _context.Receptionist.FindAsync(id);
            if (recep != null)
            {
                var user = await _context.User.FindAsync(recep.UserId);
                _context.Receptionist.Remove(recep);
                if (user != null) _context.User.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Receptionists");
        }

        // 5. QUẢN LÝ BỆNH NHÂN
        public async Task<IActionResult> Patients() => View("~/Views/Admin/benhNhan/Index.cshtml", await _context.Patient.ToListAsync());
    }
}