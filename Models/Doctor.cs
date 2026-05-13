using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int SpecialtyId { get; set; }
        public string? Phone { get; set; }
        public int? UserId { get; set; }

        // Quan trọng: Tên phải là Specialty để đồng nhất
        [ForeignKey("SpecialtyId")]
        public virtual Specialty? Specialty { get; set; }
    }
}