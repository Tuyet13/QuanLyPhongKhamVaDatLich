using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Specialty
    {
        [Key]
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty; // Khớp với Database của mày
    }
}