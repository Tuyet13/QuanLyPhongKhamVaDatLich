using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public int UserId { get; set; }
        public bool IsActive { get; set; } = true;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}