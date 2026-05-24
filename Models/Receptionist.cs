using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Receptionist
    {
        [Key]
        public int ReceptionistId { get; set; }
        public string FullName { get; set; } = "";
        public string? Phone { get; set; }
        public int? UserId { get; set; }

        // Các cột quan trọng cho tính toán lương
        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseSalary { get; set; } = 500000;
        public DateTime HireDate { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}