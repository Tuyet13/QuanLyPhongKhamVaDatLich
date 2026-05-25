using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        // INDEX: Giao diện Dashboard chính của từng bệnh nhân cụ thể
        // ==========================================================
        public IActionResult Index()
        {
            // 1. Kiểm tra quyền hạn bằng Session công thủ công
            int? userId = HttpContext.Session.GetInt32("UserId");
            string userRole = HttpContext.Session.GetString("UserRole");

            if (userId == null || userRole != "Patient")
            {
                // Chưa đăng nhập hoặc sai quyền -> Đá thẳng về trang Login
                return RedirectToAction("Login", "Account");
            }

            // 2. Tìm đúng bệnh nhân sở hữu UserId đang lưu trong Session
            var patient = _context.Patient
                .Include(p => p.User) // Kết hợp dữ liệu tài khoản
                .FirstOrDefault(p => p.UserId == userId.Value);

            if (patient == null)
            {
                return NotFound("Không tìm thấy dữ liệu hồ sơ cá nhân cho tài khoản này.");
            }

            return View(patient);
        }

        // =========================
        // GIAO DIỆN SỬA THÔNG TIN
        // =========================
        public IActionResult Edit(int id)
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            string userRole = HttpContext.Session.GetString("UserRole");

            if (currentUserId == null || userRole != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = _context.Patient.FirstOrDefault(p => p.PatientId == id);
            if (patient == null) return NotFound();

            // Chặn đứng hành vi hack URL: Bệnh nhân chỉ được quyền sửa hồ sơ của CHÍNH MÌNH
            if (patient.UserId != currentUserId.Value)
            {
                return Forbid();
            }

            return View(patient);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Patient patient)
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null || id != patient.PatientId) return NotFound();

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

        [HttpGet]
        public IActionResult MedicalHistory()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            string userRole = HttpContext.Session.GetString("UserRole");
            if (userId == null || userRole != "Patient")
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = _context.Patient.FirstOrDefault(p => p.UserId == userId.Value);
            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load medical records with appointment -> doctor
            var records = _context.MedicalRecord
                .Include(m => m.Appointment)
                    .ThenInclude(a => a.Doctor)
                .Where(m => m.Appointment != null && m.Appointment.PatientId == patient.PatientId)
                .OrderByDescending(m => m.RecordDate)
                .ToList();

            var result = new List<MedicalHistoryViewModel>();

            foreach (var r in records)
            {
                var vm = new MedicalHistoryViewModel
                {
                    RecordId = r.RecordId,
                    RecordDate = r.RecordDate,
                    Diagnosis = r.Diagnosis,
                    Note = r.Note,
                    DoctorName = r.Appointment?.Doctor?.FullName ?? "N/A"
                };

                // Load prescriptions for this medical record
                var pres = _context.PrescriptionDetail
                    .Include(pd => pd.Medicine)
                    .Where(pd => pd.MedicalRecordId == r.RecordId)
                    .Select(pd => new PrescriptionItem
                    {
                        MedicineName = pd.Medicine != null ? pd.Medicine.MedicineName : "N/A",
                        Quantity = pd.Quantity,
                        Dosage = pd.Dosage
                    })
                    .ToList();

                vm.Prescriptions = pres;
                result.Add(vm);
            }

            return View(result); // Views/Patient/MedicalHistory.cshtml
        }
    }
}