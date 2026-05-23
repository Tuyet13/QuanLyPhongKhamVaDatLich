using System;
using System.Collections.Generic;

namespace QuanLyPhongKhamVaDatLich.Models
{
    public class DoctorDashboardViewModel
    {
        public string? DoctorName { get; set; }

        public string? Specialty { get; set; }

        public int TodayAppointments { get; set; }

        public int TotalPatients { get; set; }

        public int SatisfactionRate { get; set; }

        public int Productivity { get; set; }

        public List<ScheduleVM> TodaySchedules { get; set; } = new();

        public List<PatientVM> RecentPatients { get; set; } = new();

        public class ScheduleVM
        {
            public string PatientName { get; set; }

            public DateTime AppointmentDate { get; set; }

            public string Status { get; set; }
        }

        public class PatientVM
        {
            public int Id { get; set; }

            public string FullName { get; set; } = string.Empty;

            public string Phone { get; set; } = string.Empty;

            public string Disease { get; set; } = string.Empty;
        }
    }
}

