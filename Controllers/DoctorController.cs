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

        // ==========================================
        // DOCTOR PANEL
        // ==========================================

        private bool IsDoctor()
        {
            return HttpContext.Session.GetString("UserRole") == "Doctor";
        }

        // DASHBOARD
        public IActionResult Dashboard()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return RedirectToAction("Login", "Account");

            var totalAppointments = _context.Appointment
                .Count(a => a.DoctorId == doctor.DoctorId);

            var completedAppointments = _context.Appointment
                .Count(a => a.DoctorId == doctor.DoctorId
                         && a.Status == "Đã khám");

            ViewBag.TotalAppointments = totalAppointments;
            ViewBag.TotalPatients = _context.Patient.Count();
            ViewBag.CompletedAppointments = completedAppointments;

            int satisfactionRate = 0;

            if (totalAppointments > 0)
            {
                satisfactionRate = completedAppointments * 100 / totalAppointments;
            }

            int productivityRate = totalAppointments * 10;

            if (productivityRate > 100)
            {
                productivityRate = 100;
            }

            var model = new DoctorDashboardViewModel
            {
                DoctorName = doctor.FullName,

                Specialty = doctor.Specialty != null
                    ? doctor.Specialty.SpecialtyName
                    : "Chưa cập nhật",

                TodayAppointments = totalAppointments,

                TotalPatients = _context.Patient.Count(),

                SatisfactionRate = satisfactionRate,

                Productivity = productivityRate,

                TodaySchedules = _context.Appointment
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.DoctorId)
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(5)
                    .Select(a => new DoctorDashboardViewModel.ScheduleVM
                    {
                        PatientName = a.Patient != null
                            ? a.Patient.FullName ?? ""
                            : "",

                        AppointmentDate = a.AppointmentDate,

                        Status = a.Status ?? ""
                    })
                    .ToList(),

                RecentPatients = _context.Patient
                    .Take(5)
                    .Select(p => new DoctorDashboardViewModel.PatientVM
                    {
                        Id = p.PatientId,

                        FullName = p.FullName,

                        Phone = p.Phone,

                        Disease = "Đang khám"
                    })
                    .ToList()
            };
            return View("~/Views/Doctor/Dashboard.cshtml", model);
        }

        // LỊCH KHÁM
        public IActionResult Schedule()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .FirstOrDefault(d => d.UserId == userId);

            var appointments = _context.Appointment
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View("~/Views/Doctor/Schedule.cshtml", appointments);
        }

        // DANH SÁCH BỆNH NHÂN
        public IActionResult Patients()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .FirstOrDefault(d => d.UserId == userId);

            var patients = _context.Appointment
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .Select(a => a.Patient)
                .Distinct()
                .ToList();

            return View("Patients", patients);
        }

        // KHÁM BỆNH
        public IActionResult Examine(int id)
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var appointment = _context.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.AppointmentId == id);

            if (appointment == null)
                return NotFound();

            return View("Examine", appointment);
        }
        // ================= SAVE EXAMINATION =================

        [HttpPost]
        public IActionResult SaveExamination(
            int appointmentId,
            string symptom,
            string diagnosis,
            string note,
            string prescription,
            string medicineName,
            int quantity,
            string dosage,
            string instruction)
        {
            var appointment = _context.Appointment
                .FirstOrDefault(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                return NotFound();

            // ================= TẠO BỆNH ÁN =================

            MedicalRecord record = new MedicalRecord()
            {
                AppointmentId = appointmentId,
                Symptom = symptom,
                Diagnosis = diagnosis,
                Note = note,
                PrescriptionSummary = prescription,
                RecordDate = DateTime.Now
            };

            _context.MedicalRecord.Add(record);

            // lưu để lấy RecordId
            _context.SaveChanges();

            // ================= KÊ ĐƠN THUỐC =================

            if (!string.IsNullOrEmpty(medicineName))
            {
                Prescription medicine = new Prescription()
                {
                    RecordId = record.RecordId,
                    MedicineName = medicineName,
                    Quantity = quantity,
                    Dosage = dosage,
                    Instruction = instruction
                };

                _context.Prescription.Add(medicine);
            }

            // ================= ĐỔI TRẠNG THÁI =================

            appointment.Status = "Đã khám";

            _context.SaveChanges();

            TempData["Success"] = "Khám bệnh thành công!";

            return RedirectToAction("Schedule");
        }

        //================= LỊCH SỬ BỆNH ÁN =================
        public IActionResult PatientHistory(int id)
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var patient = _context.Patient
                .FirstOrDefault(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            var appointmentIds = _context.Appointment
                .Where(a => a.PatientId == id)
                .Select(a => a.AppointmentId)
                .ToList();

            var history = _context.MedicalRecord
                .Where(m => appointmentIds.Contains(m.AppointmentId))
                .OrderByDescending(m => m.RecordDate)
                .ToList();

            ViewBag.Patient = patient;

            return View("~/Views/Doctor/PatientHistory.cshtml", history);
        }

        public IActionResult MedicalRecord(int id)
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var patient = _context.Patient
                .FirstOrDefault(p => p.PatientId == id);

            if (patient == null)
                return NotFound();

            var records = _context.MedicalRecord
                .Include(r => r.Appointment)
                    .ThenInclude(a => a.Patient)
                .Where(r => r.Appointment != null &&
                            r.Appointment.PatientId == id)
                .OrderByDescending(r => r.RecordDate)
                .ToList();

            ViewBag.Patient = patient;

            return View("MedicalRecords", records);
        }

        // ================= KÊ ĐƠN THUỐC =================
        public IActionResult Prescription(int id)
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            ViewBag.Medicines = _context.Medicine.ToList();

            ViewBag.RecordId = id;

            return View("~/Views/Doctor/Prescription.cshtml");
        }

        // ================= DANH SÁCH BỆNH ÁN =================

        public IActionResult MedicalRecords()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .FirstOrDefault(d => d.UserId == userId);

            var records = _context.MedicalRecord
                .Include(r => r.Appointment)
                .ThenInclude(a => a.Patient)
                .Where(r => r.Appointment.DoctorId == doctor.DoctorId)
                .OrderByDescending(r => r.RecordDate)
                .ToList();

            return View("~/Views/Doctor/MedicalRecords.cshtml", records);
        }

        // ================= DANH SÁCH ĐƠN THUỐC =================

        public IActionResult PrescriptionList()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var prescriptions = _context.Prescription
                .OrderByDescending(x => x.RecordId)
                .ToList();

            return View("~/Views/Doctor/PrescriptionList.cshtml", prescriptions);
        }


        // ================= LỊCH SỬ KHÁM =================

        public IActionResult ExaminationHistory()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return RedirectToAction("Login", "Account");

            var appointments = _context.Appointment
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.DoctorId
                         && a.Status == "Đã khám")
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View("~/Views/Doctor/ExaminationHistory.cshtml", appointments);
        }



        // ================= HỒ SƠ BÁC SĨ =================
        public IActionResult Profile()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .Include(d => d.Specialty)
                .Include(d => d.User)
                .FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return RedirectToAction("Login", "Account");

            return View("~/Views/Doctor/Profile.cshtml", doctor);
        }

        public IActionResult AppointmentsToday()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            var doctor = _context.Doctor
                .FirstOrDefault(d => d.UserId == userId);

            var appointments = _context.Appointment
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.DoctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();
            return View("~/Views/Doctor/AppointmentsToday.cshtml", appointments);
        }

        public IActionResult Notifications()
        {
            var today = DateTime.Today;

            var appointmentsToday = _context.Appointment
                .Include(a => a.Patient)
                .Where(a => a.AppointmentDate.Date == today)
                .ToList();

            return View(appointmentsToday);
        }

        public IActionResult LeaveRequest()
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            return View("~/Views/Doctor/LeaveRequest.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LeaveRequest(DateTime leaveDate, string reason)
        {
            if (!IsDoctor())
                return RedirectToAction("Login", "Account");

            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var request = new LeaveRequest
            {
                UserId = userId.Value,
                LeaveDate = leaveDate,
                Reason = reason,
                Status = "Pending"
            };

            _context.LeaveRequest.Add(request);
            _context.SaveChanges();

            TempData["Success"] = "Đã gửi đơn nghỉ phép thành công!";

            return RedirectToAction(nameof(LeaveRequest));
        }
    }
}

