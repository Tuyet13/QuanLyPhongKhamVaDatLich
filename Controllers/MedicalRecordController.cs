using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class MedicalRecordController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}