using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class PrescriptionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}