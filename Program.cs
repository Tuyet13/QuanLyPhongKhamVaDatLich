using Microsoft.EntityFrameworkCore;
using QuanLyPhongKhamVaDatLich.Data;
// Thêm namespace này nếu dùng Authentication
using Microsoft.AspNetCore.Authentication.Cookies;

namespace QuanLyPhongKhamVaDatLich
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // --- 1. CẤU HÌNH DỊCH VỤ (SERVICES) ---
            builder.Services.AddControllersWithViews();

            // Kết nối CSDL
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );

            // Cấu hình Authentication bằng Cookie (Rất nên có cho đồ án quản lý)
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login"; // Đường dẫn trang đăng nhập
                    options.AccessDeniedPath = "/Account/AccessDenied"; // Trang khi vào nhầm quyền
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            // Cấu hình Session & Accessor
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // --- 2. CẤU HÌNH PIPELINE (MIDDLEWARE) ---
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Kích hoạt Session
            app.UseSession();

            // --- QUAN TRỌNG: Thứ tự Authentication trước Authorization ---
            app.UseAuthentication();
            app.UseAuthorization();

            // --- 3. ĐỊNH TUYẾN (ROUTING) ---
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}