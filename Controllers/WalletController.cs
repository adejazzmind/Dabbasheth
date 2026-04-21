using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Data;
using Microsoft.EntityFrameworkCore;

namespace Dabbasheth.Controllers
{
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WalletController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Wallet/FundWallet
        public IActionResult FundWallet()
        {
            return View();
        }
    }
}