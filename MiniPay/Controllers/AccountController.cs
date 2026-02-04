using Microsoft.AspNetCore.Mvc;
using MiniPay.Data;

namespace MiniPay.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users
                    .FirstOrDefault(x => x.Email == email && x.Password == password);
            if (user == null)
            {
                TempData["Error"] = "Invalid credentials";
                return RedirectToAction("Login");
            }
            HttpContext.Session.SetInt32("UserId", user.Id);
            return RedirectToAction("Index", "Users");

        }

        public IActionResult Logout() {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
