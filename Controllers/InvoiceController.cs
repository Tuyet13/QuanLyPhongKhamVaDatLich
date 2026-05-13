using Microsoft.AspNetCore.Mvc;

namespace QuanLyPhongKhamVaDatLich.Controllers
{
    public class InvoiceController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
