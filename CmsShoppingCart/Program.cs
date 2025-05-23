using CmsShoppingCart.Infrastructure;
using CmsShoppingCart.Models;
using CmsShoppingCart.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Globalization;

// configuracion de $
var cultura = new CultureInfo("es-MX");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

// Configuración
var configuration = builder.Configuration;
var services = builder.Services;
var env = builder.Environment;

// Servicios
services.AddMemoryCache();
services.AddSession();
services.AddRouting(options => options.LowercaseUrls = true);
services.AddControllersWithViews();

// Bases de datos
services.AddDbContext<CmsShoppingCartContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("CmsShoppingCartContext")));
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("CmsShoppingCartContext")));
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("CmsShoppingCartContext")));

// Identity
services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<CmsShoppingCartContext>()
.AddDefaultTokenProviders();

// Autenticación
services.AddAuthentication().AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Correos y PayPal
services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
services.AddTransient<IEmailSender, EmailSender>();
services.Configure<PayPalSettings>(configuration.GetSection("PayPalSettings"));
services.AddScoped<PayPalService>();
services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, CustomClaimsPrincipalFactory>();
services.AddHttpClient();

// Construir app
var app = builder.Build();

// Semilla de datos
using (var scope = app.Services.CreateScope())
{
    var scopedServices = scope.ServiceProvider;
    try
    {
        SeedData.Initialize(scopedServices);
    }
    catch (Exception)
    {
        throw;
    }
}

// Middleware
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media", "products")),
    RequestPath = "/media/products"
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Rutas
app.MapControllerRoute(
    name: "ubicacion",
    pattern: "ubicacion",
    defaults: new { controller = "Ubicacion", action = "Index" });

app.MapControllerRoute(
    name: "SobreNosotros",
    pattern: "Sobre Nosotros",
    defaults: new { controller = "Pages", action = "SobreNosotros" });

app.MapControllerRoute(
    name: "servicios",
    pattern: "servicios",
    defaults: new { controller = "Pages", action = "Servicios" });

app.MapControllerRoute(
    name: "Contactos",
    pattern: "contactos",
    defaults: new { controller = "Pages", action = "Contactos" });

app.MapControllerRoute(
    name: "mensajes",
    pattern: "contactos/mensajes",
    defaults: new { controller = "Contactos", action = "Mensajes" });

app.MapControllerRoute(
    name: "Pedidos",
    pattern: "pedido/pedidos",
    defaults: new { area = "Admin", controller = "Pedido", action = "Pedidos" });

app.MapControllerRoute(
    name: "products-all",
    pattern: "products/all",
    defaults: new { controller = "Products", action = "All" });

app.MapControllerRoute(
    name: "products",
    pattern: "products/{categorySlug}",
    defaults: new { controller = "Products", action = "ProductsByCategory" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "",
    defaults: new { controller = "Inicio", action = "Welcome" });

app.MapControllerRoute(
    name: "default-home",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "page",
    pattern: "pages/{action=Page}/{slug?}",
    defaults: new { controller = "Pages", action = "Page" });

app.Run();
