using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Họ tên bác sĩ không được để trống")]
        public string FullName { get; set; } = string.Empty;

        public int SpecialtyId { get; set; }

        public string? Phone { get; set; }

        public int? UserId { get; set; }


        [Required]
        [Column("HireDate")] // Khai báo tường minh tên cột dưới DB
        public DateTime HireDate { get; set; } = DateTime.Now; // Ngày vào làm để tính thâm niên

        [Required]
        [Column("BaseSalary", TypeName = "decimal(18,2)")] // Khai báo tường minh cả tên cột và kiểu dữ liệu
        public decimal BaseSalary { get; set; } = 500000; // Lương cơ bản tính theo ngày công

        [Required]
        [Column("Rating")] // Khai báo tường minh tên cột dưới DB
        public double Rating { get; set; } = 5.0; // Điểm đánh giá trung bình từ bệnh nhân

        // --- RELATIONSHIPS (LIÊN KẾT KHÓA NGOẠI) ---

        [ForeignKey("SpecialtyId")]
        public virtual Specialty? Specialty { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}