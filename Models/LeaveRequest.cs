using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; } // Khóa chính tự tăng

        [Required]
        public int UserId { get; set; } // ID của User (Bác sĩ hoặc Lễ tân) xin nghỉ phép

        [Required]
        [DataType(DataType.Date)]
        public DateTime LeaveDate { get; set; } // Ngày đăng ký nghỉ phép

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Trạng thái: Pending (Chờ duyệt), Approved (Đã duyệt), Rejected (Từ chối)

        [StringLength(255)]
        public string? Reason { get; set; } // Lý do xin nghỉ phép (nếu có)

        // Liên kết dữ liệu ngược lại bảng User để dễ dàng truy vấn thông tin tài khoản
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }
}