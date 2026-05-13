using System.ComponentModel.DataAnnotations;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }
        public int RecordId { get; set; }
        public string MedicineName { get; set; } = ""; // Tên thuốc
        public int Quantity { get; set; } // Số lượng
    }
}