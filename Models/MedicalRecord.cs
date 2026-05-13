using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class MedicalRecord
    {
        [Key]
        public int RecordId { get; set; }

        public int AppointmentId { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime RecordDate { get; set; } = DateTime.Now;

        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }
    }
}