using Microsoft.EntityFrameworkCore;
using PhotoGalleryApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Ensure DB created (for dev). For production use migrations.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Middleware
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

// Simple middleware to expose current Role and UserId via HttpContext.Items for convenience
app.Use(async (context, next) =>
{
    context.Items["Role"] = context.Session.GetString("Role") ?? "";
    context.Items["UserId"] = context.Session.GetInt32("UserId") ?? 0;
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Photo}/{action=Index}/{id?}");

app.Run();
