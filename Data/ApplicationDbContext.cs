using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Models;

namespace QuanLyPhongKhamVaDatLich.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Doctor> Doctor { get; set; }
        public DbSet<Specialty> Specialty { get; set; }
        public DbSet<Appointment> Appointment { get; set; }
        public DbSet<MedicalRecord> MedicalRecord { get; set; }
        public DbSet<Medicine> Medicine { get; set; } // Thêm bảng này
        public DbSet<PrescriptionDetail> PrescriptionDetail { get; set; } // Thêm bảng này
        public DbSet<Invoice> Invoice { get; set; }
    }
}