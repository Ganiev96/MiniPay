using MiniPay.Data;
using MiniPay.Models;

namespace MiniPay.Services
{
    public class WalletService
    {
        private readonly AppDbContext _context;

        public WalletService(AppDbContext context)
        {
            _context = context;
        }

        public bool TopUp(int userId, decimal amount)
        {
            if (amount <= 0) return false;

            using var db = _context.Database.BeginTransaction();

            try
            {
                var wallet = _context.Wallets.FirstOrDefault(x => x.UserId == userId);

                if (wallet == null)
                {
                    wallet = new Wallet { UserId = userId, Balance = 0 };
                    _context.Wallets.Add(wallet);
                    _context.SaveChanges();
                }

                wallet.Balance += amount;
                _context.Wallets.Update(wallet);

                var user = _context.Users.First(x => x.Id == userId);
                user.Balance = wallet.Balance;
                _context.Users.Update(user);

                _context.Transactions.Add(new Transaction
                {
                    UserId = userId,
                    Amount = amount,
                    CreatedAt = DateTime.Now,
                    Description = "Top up"
                });

                _context.SaveChanges();
                db.Commit();
                return true;
            }
            catch
            {
                db.Rollback();
                return false;
            }
        }

        public bool Withdraw(int userId, decimal amount)
        {
            if (amount <= 0) return false;

            using var db = _context.Database.BeginTransaction();

            try
            {
                var wallet = _context.Wallets.FirstOrDefault(x => x.UserId == userId);

                if (wallet == null || wallet.Balance < amount)
                    return false;

                wallet.Balance -= amount;
                _context.Wallets.Update(wallet);

                var user = _context.Users.First(x => x.Id == userId);
                user.Balance = wallet.Balance;
                _context.Users.Update(user);

                _context.Transactions.Add(new Transaction
                {
                    UserId = userId,
                    Amount = -amount,
                    CreatedAt = DateTime.Now,
                    Description = "Withdraw"
                });

                _context.SaveChanges();
                db.Commit();
                return true;
            }
            catch
            {
                db.Rollback();
                return false;
            }
        }

        public bool Transfer(int fromUserId, int toUserId, decimal amount)
        {
            if (amount <= 0 || fromUserId == toUserId) return false;

            using var db = _context.Database.BeginTransaction();

            try
            {
                var fromWallet = _context.Wallets.First(x => x.UserId == fromUserId);
                var toWallet = _context.Wallets.First(x => x.UserId == toUserId);

                if (fromWallet.Balance < amount) return false;

                fromWallet.Balance -= amount;
                toWallet.Balance += amount;

                _context.Wallets.Update(fromWallet);
                _context.Wallets.Update(toWallet);

                var fromUser = _context.Users.First(x => x.Id == fromUserId);
                var toUser = _context.Users.First(x => x.Id == toUserId);

                fromUser.Balance = fromWallet.Balance;
                toUser.Balance = toWallet.Balance;

                _context.Users.Update(fromUser);
                _context.Users.Update(toUser);

                _context.Transactions.Add(new Transaction
                {
                    UserId = fromUserId,
                    Amount = -amount,
                    CreatedAt = DateTime.Now,
                    Description = $"Transfer to {toUser.FullName}"
                });

                _context.Transactions.Add(new Transaction
                {
                    UserId = toUserId,
                    Amount = amount,
                    CreatedAt = DateTime.Now,
                    Description = $"Transfer from {fromUser.FullName}"
                });

                _context.SaveChanges();
                db.Commit();
                return true;
            }
            catch
            {
                db.Rollback();
                return false;
            }
        }

    }
}
