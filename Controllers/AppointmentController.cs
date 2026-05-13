using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
using QuanLyPhongKhamVaDatLich.Models;
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
        // FORM ĐẶT LỊCH
        // =========================
        public IActionResult Create()
        {
            ViewBag.Patient = _context.Patient.ToList();

            ViewBag.Doctor = _context.Doctor.ToList();

            return View();
        }

        // =========================
        // LƯU LỊCH HẸN
        // =========================
        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            _context.Appointment.Add(appointment);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}