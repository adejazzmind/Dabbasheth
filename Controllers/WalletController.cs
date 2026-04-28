using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Data;

namespace Dabbasheth.Controllers
{
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;
        public WalletController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult FundWallet()
        {
            if (TempData.Peek("UserEmail") == null)
                return RedirectToAction("Login", "Account");
            return View();
        }
    }
}
