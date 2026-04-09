using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;
using Dabbasheth.Models;

var builder = WebApplication.CreateBuilder(args);

// ================================================
// 1. DATABASE CONNECTION (Pooled - Recommended for Neon)
// ================================================
var connectionString = "Host=ep-ancient-cell-anc4zt6c-pooler.c-6.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_xpaKdTJ7q4ef;Port=5432;SslMode=Require;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsql =>
    {
        npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
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

// ================================================
// 3. BUILD THE APP
// ================================================
var app = builder.Build();

// ================================================
// 4. AUTO MIGRATION + SEEDING
// ================================================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    try
    {
        context.Database.Migrate();
        Console.WriteLine("✅ Database migrated successfully.");

        // Seed Admins if none exist
        if (!context.Users.Any(u => u.Role == "Admin"))
        {
            context.Users.AddRange(
                new User
                {
                    FullName = "Samson Mayowa Braimoh",
                    Email = "adejazzmind@gmail.com",
                    Password = "123",
                    PhoneNumber = "08000000000",
                    Role = "Admin"
                },
                new User
                {
                    FullName = "Tolulope Jumoke Samson",
                    Email = "tolubabe2k@gmail.com",
                    Password = "123",
                    PhoneNumber = "08000000000",
                    Role = "Admin"
                }
            );
            context.SaveChanges();
            Console.WriteLine("✅ Admin users seeded.");
        }

        // Seed Wallets for Admins
        if (!context.Wallets.Any(w => w.UserEmail == "adejazzmind@gmail.com"))
        {
            context.Wallets.Add(new Wallet
            {
                UserEmail = "adejazzmind@gmail.com",
                Balance = 2500000m,
                Currency = "NGN",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!context.Wallets.Any(w => w.UserEmail == "tolubabe2k@gmail.com"))
        {
            context.Wallets.Add(new Wallet
            {
                UserEmail = "tolubabe2k@gmail.com",
                Balance = 2500000m,
                Currency = "NGN",
                CreatedAt = DateTime.UtcNow
            });
        }
        context.SaveChanges();
        Console.WriteLine("✅ Admin wallets seeded.");

        // Seed one test Customer (for demo)
        if (!context.Users.Any(u => u.Role == "Customer"))
        {
            context.Users.Add(new User
            {
                FullName = "Eniola Apelogun",
                Email = "test@me.com",
                Password = "123",
                PhoneNumber = "08012345678",
                Role = "Customer"
            });
            context.SaveChanges();
            Console.WriteLine("✅ Test customer seeded.");
        }

        // Seed wallet for test customer
        if (!context.Wallets.Any(w => w.UserEmail == "test@me.com"))
        {
            context.Wallets.Add(new Wallet
            {
                UserEmail = "test@me.com",
                Balance = 15000m,
                Currency = "NGN",
                CreatedAt = DateTime.UtcNow
            });
            context.SaveChanges();
        }

        // Final summary
        Console.WriteLine($"Final Users Count: {context.Users.Count()}");
        Console.WriteLine($"Final Wallets Count: {context.Wallets.Count()}");
        Console.WriteLine($"Final Total Balance: ₦{context.Wallets.Sum(w => (decimal?)w.Balance) ?? 0:N2}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ CRITICAL DATABASE ERROR: " + ex.Message);
        if (ex.InnerException != null)
            Console.WriteLine("Inner: " + ex.InnerException.Message);
    }
}

// ================================================
// 5. MIDDLEWARE PIPELINE
// ================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();           // Must be before Authorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();