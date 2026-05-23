using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class PrescriptionDetail
    {
        [Key]
        public int PrescriptionDetailId { get; set; }

        [Required]
        public int MedicalRecordId { get; set; } // Mã hồ sơ bệnh án liên kết

        [Required]
        public int MedicineId { get; set; } // Mã thuốc liên kết từ bảng Medicine

        [Required(ErrorMessage = "Số lượng thuốc không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng thuốc phải lớn hơn 0")]
        public int Quantity { get; set; } // Số lượng thuốc cấp

        [StringLength(255)]
        public string? Dosage { get; set; } // Cách dùng/Liều lượng (Ví dụ: Uống ngày 2 lần, sau ăn)

        // --- RELATIONSHIPS (LIÊN KẾT KHÓA NGOẠI) ---

        [ForeignKey("MedicalRecordId")]
        public virtual MedicalRecord? MedicalRecord { get; set; }

        [ForeignKey("MedicineId")]
        public virtual Medicine? Medicine { get; set; }
    }
}