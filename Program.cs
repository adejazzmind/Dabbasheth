using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;
using Dabbasheth.Models;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// 1. DATABASE CONNECTION (Neon PostgreSQL)
// ================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=ep-ancient-cell-anc4zt6c-pooler.c-6.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_xpaKdTJ7q4ef;Port=5432;SslMode=Require;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(5), null);
        npgsql.CommandTimeout(60);
    });
});

// ================================================
// 2. SERVICES
// ================================================
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ================================================
// 3. AUTO MIGRATION + EXECUTIVE SEEDING
// ================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Apply any pending migrations automatically
        context.Database.Migrate();

        // ✅ SEED ADMINS (CEO & MD)
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            context.Users.AddRange(
                new User
                {
                    FullName = "Samson Mayowa Braimoh",
                    Email = "adejazzmind@gmail.com",
                    Password = "123",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Tolulope Jumoke Samson",
                    Email = "tolubabe2k@gmail.com",
                    Password = "123",
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
        }

        // ✅ SEED ADMIN WALLETS
        var adminEmails = new[] { "adejazzmind@gmail.com", "tolubabe2k@gmail.com" };
        foreach (var email in adminEmails)
        {
            if (!context.Wallets.Any(w => w.UserEmail == email))
            {
                context.Wallets.Add(new Wallet
                {
                    UserEmail = email,
                    Balance = 2500000m,
                    Currency = "NGN",
                    WalletNumber = "CEO-" + new Random().Next(1000, 9999), // Special ID for Admins
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        context.SaveChanges();
        Console.WriteLine("✅ Database Seeding Successful.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ DB Seeding Error: " + ex.Message);
    }
}

// ================================================
// 4. MIDDLEWARE PIPELINE
// ================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();