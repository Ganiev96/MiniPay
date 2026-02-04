using Microsoft.EntityFrameworkCore;
using MiniPay.Data;
using MiniPay.Services;
using MiniPay.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<WalletService>();

var app = builder.Build();
using(var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!db.Users.Any())
    {
        var user = new User
        {
            FullName = "Admin User",
            Email = "admin@mail.com",
            Password = "123",
            Balance = 0
        };
        db.Users.Add(user);
        db.SaveChanges();

        db.Wallets.Add(new Wallet
        {
            UserId = user.Id,
            Balance = 0
        });
        db.SaveChanges();
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseSession();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
