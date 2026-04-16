using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;
using Dabbasheth.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// 1. DATABASE CONNECTION (Neon PostgreSQL Engine)
// ============================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=ep-ancient-cell-anc4zt6c-pooler.c-6.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_xpaKdTJ7q4ef;Port=5432;SslMode=Require;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        // High-Stability Config: Essential for Neon Serverless/Cold-Starts
        npgsql.EnableRetryOnFailure(10, TimeSpan.FromSeconds(5), null);
        npgsql.CommandTimeout(60);
    });
});

// ============================================================
// 2. CORE SERVICES INITIALIZATION
// ============================================================
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ============================================================
// 3. AUTO-MIGRATION & EXECUTIVE SEEDING ENGINE
// ============================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // ✅ STEP A: Wake up Neon and sync the Schema
        context.Database.Migrate();

        // ✅ STEP B: Force creation of any missing tables (like 'wallets')
        context.Database.EnsureCreated();

        // ✅ STEP C: SEED ADMIN IDENTITIES
        var adminEmails = new[] { "adejazzmind@gmail.com", "tolubabe2k@gmail.com" };

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
            Console.WriteLine("✅ Admin Identities Created.");
        }

        // ✅ STEP D: SEED ADMIN VIRTUAL WALLETS
        foreach (var email in adminEmails)
        {
            if (!context.Wallets.Any(w => w.UserEmail == email))
            {
                context.Wallets.Add(new Wallet
                {
                    UserEmail = email,
                    Balance = 2500000m,
                    Currency = "NGN",
                    WalletNumber = "CEO-" + new Random().Next(1000, 9999),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        // ✅ STEP E: SEED REVENUE SETTINGS
        if (!context.SystemSettings.Any(s => s.SettingKey == "WithdrawalFee"))
        {
            context.SystemSettings.Add(new SystemSetting
            {
                SettingKey = "WithdrawalFee",
                SettingValue = 100.00m,
                Description = "Platform service charge for each bank withdrawal"
            });
        }

        context.SaveChanges();
        Console.WriteLine("✅ Database Sync & Seeding Successful.");
    }
    catch (Exception ex)
    {
        // Reveals technical reasons if the handshake fails
        Console.WriteLine("❌ Hub Seeding/Sync Error: " + ex.Message);
    }
}

// ============================================================
// 4. REQUEST PIPELINE (Middleware)
// ============================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// CRITICAL: Session MUST come before Authorization for TempData to work
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();