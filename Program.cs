using Microsoft.EntityFrameworkCore;
using Dabbasheth.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. CONNECTION STRING: Updated with the exact Host from your Neon screenshot
// This uses the pooler address to ensure stable connections from Render.
var connectionString = "Host=ep-ancient-cell-anc4zt6c-pooler.us-east-1.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=npg_xpaKdTJ7q4ef;Port=5432;SslMode=Require;TrustServerCertificate=true;";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Retries connection if the network flickers
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
    }));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// 2. THE RENDER AUTO-MIGRATE: Runs on startup to build your tables in the new project
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
        Console.WriteLine("-----------------------------------------");
        Console.WriteLine("RENDER SUCCESS: Connected to ep-ancient-cell!");
        Console.WriteLine("-----------------------------------------");
    }
    catch (Exception ex)
    {
        Console.WriteLine("*****************************************");
        Console.WriteLine("RENDER DATABASE ERROR: " + ex.Message);
        if (ex.InnerException != null)
        {
            Console.WriteLine("INNER ERROR: " + ex.InnerException.Message);
        }
        Console.WriteLine("*****************************************");
    }
}

// 3. MIDDLEWARE PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();