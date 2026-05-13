using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "Patient";
        public string? Email { get; set; } // Thêm dòng này
        public bool IsActive { get; set; } = true; // Thêm dòng này
    }
}