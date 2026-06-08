using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Models;

namespace QuanLyPhongKhamVaDatLich.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // --- CÁC BẢNG QUẢN LÝ TÀI KHOẢN & NGƯỜI BỆNH ---
        public DbSet<User> User { get; set; }
        public DbSet<Patient> Patient { get; set; }

        // --- CÁC BẢNG QUẢN LÝ NHÂN SỰ PHÒNG KHÁM ---
        public DbSet<Doctor> Doctor { get; set; }
        public DbSet<Receptionist> Receptionist { get; set; } // Quản lý lễ tân
        public DbSet<LeaveRequest> LeaveRequest { get; set; } // Quản lý lịch nghỉ phép nhân sự

        // --- CÁC BẢNG QUẢN LÝ CHUYÊN KHOA & LỊCH HẸN ---
        public DbSet<Specialty> Specialty { get; set; }
        public DbSet<Appointment> Appointment { get; set; }

        // --- CÁC BẢNG QUẢN LÝ KHÁM BỆNH, THUỐC & ĐƠN THUỐC ---
        public DbSet<MedicalRecord> MedicalRecord { get; set; }
        public DbSet<Medicine> Medicine { get; set; }
        public DbSet<PrescriptionDetail> PrescriptionDetail { get; set; }
<<<<<<< HEAD
        public DbSet<Prescription> Prescription { get; set; }
=======
>>>>>>> 0c8a645e71df325eae5dda7c8fb73e0b24a272a1

        // --- CÁC BẢNG QUẢN LÝ TÀI CHÍNH ---
        public DbSet<Invoice> Invoice { get; set; }
    }
}