using Microsoft.AspNetCore.Mvc;
using MiniPay.Data;
using MiniPay.Models;
using MiniPay.Services;

namespace MiniPay.Controllers
{
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly WalletService _walletService;

        public UsersController(AppDbContext context, WalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(User user)
        {
            user.Balance = 0;
            _context.Users.Add(user);
            _context.SaveChanges();

            var wallet = new Wallet
            {
                UserId = user.Id,
                Balance = 0
            };
            _context.Wallets.Add(wallet);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            return View(user);
        }
        [HttpPost]
        public IActionResult Edit(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(x => x.Id == id);
            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult TopUp(int id)
        {
            return View(id);
        }

        [HttpPost]
        public IActionResult TopUp(int userId, decimal amount)
        {
           var ok = _walletService.TopUp(userId, amount);
            if (!ok)
            {
                TempData["Error"] = "Invalid amount";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Transactions(int id)
        {
            var list = _context.Transactions
                .Where(x => x.UserId == id)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            ViewBag.UserId = id;
            return View(list);
        }

        public IActionResult Withdraw(int id)
        {
            return View(id);
        }

        [HttpPost]
        public IActionResult Withdraw(int userId, decimal amount)
        {
            var success = _walletService.Withdraw(userId, amount);

            if (!success)
            {
                TempData["Error"] = "Insuffient balance or invalid amount.";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Transfer(int id)
        {
            var sender = _context.Users.First(x=>x.Id == id);
            ViewBag.FromUserId = id;
            ViewBag.SenderBalance = sender.Balance;

            ViewBag.Users = _context.Users
                .Where(u=>u.Id!=id)
                .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Transfer(int fromUserId, int toUserId, decimal amount)
        {
            var success = _walletService.Transfer(fromUserId, toUserId, amount);
            if (!success)
            {
                TempData["Error"] = "Tranfer failed. Check balance or amount.";
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
