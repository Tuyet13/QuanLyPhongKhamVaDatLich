using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần thêm cái này để dùng .Include()
using QuanLyPhongKhamVaDatLich.Data;
using QuanLyPhongKhamVaDatLich.Models;
using System.Linq;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // INDEX: Hiển thị danh sách (Admin) hoặc Profile (Bệnh nhân)
        // Hiện tại tớ làm theo hướng Dashboard cho Bệnh nhân nhé
        // ==========================================================
        public IActionResult Index()
        {
            // Lấy danh sách kèm thông tin tài khoản để tránh lỗi null navigation property
            var patients = _context.Patient.Include(p => p.User).ToList();
            return View(patients);
        }

        // =========================
        // GIAO DIỆN THÊM BỆNH NHÂN
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Patient patient)
        {
            // Xóa check ModelState nếu mày không truyền UserId từ View (vì UserId là bắt buộc trong DB)
            // Cách tốt nhất là gán một User mặc định hoặc bỏ qua validation UserId ở đây
            _context.Patient.Add(patient);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // GIAO DIỆN SỬA
        // =========================
        public IActionResult Edit(int id)
        {
            var patient = _context.Patient.FirstOrDefault(p => p.PatientId == id);
            if (patient == null) return NotFound();
            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Patient patient)
        {
            if (id != patient.PatientId) return NotFound();

            try
            {
                _context.Patient.Update(patient);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Patient.Any(e => e.PatientId == patient.PatientId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // XÓA BỆNH NHÂN
        // =========================
        public IActionResult Delete(int id)
        {
            var patient = _context.Patient.Find(id);
            if (patient != null)
            {
                _context.Patient.Remove(patient);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}