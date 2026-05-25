using System;
using System.Collections.Generic;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class MedicalHistoryViewModel
    {
        public int RecordId { get; set; }
        public DateTime RecordDate { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string? Note { get; set; }

        public List<PrescriptionItem> Prescriptions { get; set; } = new();
    }

    public class PrescriptionItem
    {
        public string MedicineName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Dosage { get; set; }
    }
}