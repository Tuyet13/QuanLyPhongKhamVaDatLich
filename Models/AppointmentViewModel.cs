using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class AppointmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bác sĩ")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn giờ khám")]
        // bind from <input type="time"> as string and parse on server
        public string AppointmentTime { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Reason { get; set; }
    }
}