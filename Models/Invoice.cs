using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Invoice
{
    [Key]
    public int InvoiceId { get; set; }
    public int AppointmentId { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }   // Tổng tiền thanh toán
    public DateTime? PaymentDate { get; set; } // Ngày thanh toán
    public string? PaymentStatus { get; set; } // Trạng thái: Đã thu, Chưa thu
}