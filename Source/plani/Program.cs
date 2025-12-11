using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models;
using plani.Models.Data;

namespace plani;

class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        //builder.Services.AddLogging(); // This is called automatically by WebApplication.CreateBuilder(args)

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var spanishCultureInfo = new CultureInfo("es-ES");
            
            options.SetDefaultCulture(spanishCultureInfo.Name);
            options.DefaultRequestCulture = new RequestCulture(spanishCultureInfo);
            options.SupportedCultures = new List<CultureInfo> { spanishCultureInfo };
            options.SupportedUICultures = new List<CultureInfo> { spanishCultureInfo };
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new LocalizedRequestCultureProvider(spanishCultureInfo.Name));
        });

        builder.Services.AddDbContext<IdentityDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<IdentityDBContext>()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
            .AddUserManager<ApplicationUserManager>()
            .AddRoleManager<ApplicationRoleManager>()
            .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

        builder.Services.Configure<CookiePolicyOptions>(options =>
        {
            // options.CheckConsentNeeded = _ => false; By default is false
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = ".AspNetCore.Identity.Application";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            // options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
            options.SlidingExpiration = true;
            options.LoginPath = "/Cuentas/IniciarSesion";
        });

        builder.Services.AddScoped<IEmailSender, EmailSender>();

        // Managers
        builder.Services.AddScoped<AreasManager>();
        builder.Services.AddScoped<ModalidadesManager>();
        builder.Services.AddScoped<ServiciosManager>();
        builder.Services.AddScoped<DashboardManager>();
        builder.Services.AddScoped<SesionesManager>();

        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

        WebApplication app = builder.Build();

        app.UseRequestLocalization();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Error/Error");
        }

        app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Administracion}/{id?}");
        app.Run();
    }
}