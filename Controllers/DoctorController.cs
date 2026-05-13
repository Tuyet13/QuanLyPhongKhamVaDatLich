using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
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

        public IActionResult Index()
        {
            // Lấy UserId từ Session khi bác sĩ đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            // Tìm DoctorId tương ứng với UserId này
            var doctor = _context.Doctor.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            // Lấy danh sách lịch hẹn của bác sĩ này
            var appointments = _context.Appointment
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(appointments);
        }

        // Trang lập bệnh án cho một lịch hẹn cụ thể
        public IActionResult CreateRecord(int id)
        {
            ViewBag.AppointmentId = id;
            return View();
        }
    }
}