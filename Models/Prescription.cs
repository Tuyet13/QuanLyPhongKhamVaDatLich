using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class Prescription
    {
        [Key]
        public int PrescriptionId { get; set; }

        public int RecordId { get; set; }

        public string? MedicineName { get; set; }

        public int Quantity { get; set; }

        public string? Dosage { get; set; }

        public string? Instruction { get; set; }

        [ForeignKey("RecordId")]
        public virtual MedicalRecord? MedicalRecord { get; set; }
    }
}