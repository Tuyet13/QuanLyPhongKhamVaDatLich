namespace QuanLyPhongKhamVaDatLich.Models
{
    public class LoginViewModel
    {
        // Phải có thuộc tính Email này thì Controller mới không báo lỗi đỏ nữa
        public string Email { get; set; }

        public string Password { get; set; }

        public string Role { get; set; }
    }
}