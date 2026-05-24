using System;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class StaffPerformanceViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Seniority { get; set; } = string.Empty;
        public string StatusToday { get; set; } = string.Empty;
        public int LeaveDaysThisMonth { get; set; }
        public decimal EstimatedSalary { get; set; }
        public double Rating { get; set; }
        public int TotalWorkCount { get; set; }
    }
}