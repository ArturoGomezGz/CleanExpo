using ExpoMR.Clases;
using ExpoMR.Datos;
using ExpoMR.Servicios;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;


var builder = WebApplication.CreateBuilder(args);

//Configuración de Base de datos
builder.Services.AddDbContext<ApplicationDbContext>(opciones => 
    opciones.UseSqlServer(builder.Configuration.GetConnectionString("ExpoMRCS"))
);

//Se agrega el servicio de Identity a la aplicacion
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

//Url de Retorno
builder.Services.ConfigureApplicationCookie(option =>
{
    option.LoginPath = new PathString("/Cuentas/Acceso");
    option.AccessDeniedPath = new PathString("/Cuentas/Denegado");
    option.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    option.SlidingExpiration = true;
});

//Configuración de indentity
builder.Services.Configure<IdentityOptions>(options =>
{
    //Minimo de caraacteres
    //options.Password.RequiredLength = 5;
    // Configurar la política de contraseñas
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    //tiempo de bloqueo
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
    //Máximo de intentos para bloquear
    options.Lockout.MaxFailedAccessAttempts = 5;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

//variable de sesion
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de vida de la sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Se agrega el IEmailSender
builder.Services.AddTransient<IEmailSender, MailKitEmailSender>();
builder.Services.AddTransient<DB>();
var config1 = builder.Configuration["ConnectionStrings:ExpoMRCS"];

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession(); // Habilita el uso de sesiones

app.UseRouting();
//se agrega la autenticación
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();