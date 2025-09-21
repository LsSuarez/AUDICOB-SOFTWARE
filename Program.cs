using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PROYECTO_AUDICOB.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));  // Cambié de UseSqlite a UseNpgsql para PostgreSQL
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();  // Solo en desarrollo, para ejecutar las migraciones
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // HSTS en producción
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// Configuración de rutas y vistas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();  // Si usas Razor Pages

app.Run();
