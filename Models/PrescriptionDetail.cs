using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class PrescriptionDetail
    {
        [Key]
        public int PrescriptionDetailId { get; set; }

        public int RecordId { get; set; }
        public int MedicineId { get; set; }
        public int Quantity { get; set; }
        public string? Dosage { get; set; }

        [ForeignKey("RecordId")]
        public virtual MedicalRecord? MedicalRecord { get; set; }

        [ForeignKey("MedicineId")]
        public virtual Medicine? Medicine { get; set; }
    }
}