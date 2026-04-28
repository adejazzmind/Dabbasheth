using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;
using Dabbasheth.Models;

var builder = WebApplication.CreateBuilder(args);

// ── 1. DATABASE ───────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=ep-ancient-cell-anc4zt6c-pooler.c-6.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_xpaKdTJ7q4ef;Port=5432;SslMode=Require;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(5), null);
        npgsql.CommandTimeout(60);
    }));

// ── 2. SERVICES ───────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ── 3. MIGRATION + SEEDING ────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Apply any pending migrations (safe if already applied)
        context.Database.Migrate();

        // ── Seed admin users ──────────────────────────────────────────
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            context.Users.AddRange(
                new User
                {
                    FullName = "Samson Mayowa Braimoh",
                    Email = "adejazzmind@gmail.com",
                    Password = "123",
                    Role = "Admin",
                    Status = "Active",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    FullName = "Tolulope Jumoke Samson",
                    Email = "tolubabe2k@gmail.com",
                    Password = "123",
                    Role = "Admin",
                    Status = "Active",
                    IsVerified = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
            context.SaveChanges();
            Console.WriteLine("✅ Admin users seeded.");
        }

        // ── Seed admin wallets ────────────────────────────────────────
        foreach (var email in new[] { "adejazzmind@gmail.com", "tolubabe2k@gmail.com" })
        {
            if (!context.Wallets.Any(w => w.UserEmail == email))
            {
                context.Wallets.Add(new Wallet
                {
                    UserEmail = email,
                    Balance = 10_000_000m,
                    Currency = "NGN",
                    WalletNumber = "CEO-" + new Random().Next(1000, 9999),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // ── Seed system settings ──────────────────────────────────────
        if (!context.SystemSettings.Any())
        {
            context.SystemSettings.Add(new SystemSetting
            {
                SettingKey = "WithdrawalFee",
                SettingValue = 100.00m,
                Description = "Platform service charge per bank withdrawal"
            });
        }

        context.SaveChanges();
        Console.WriteLine("✅ IES Urban Hub: DB sync & seeding complete.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Startup DB error: {ex.Message}");
        // App still starts — admin bypass login works without DB
    }
}

// ── 4. MIDDLEWARE ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();          // MUST be before MapControllerRoute
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();