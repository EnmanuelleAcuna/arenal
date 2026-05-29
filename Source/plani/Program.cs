using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Data;
using plani.Models.Managers;

namespace plani;

internal class Program {
    public static async Task Main(string[] args) {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args: args);

        //builder.Services.AddLogging(); // This is called automatically by WebApplication.CreateBuilder(args)

        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
        builder.Services.Configure<RequestLocalizationOptions>(options => {
            CultureInfo spanishCultureInfo = new("es-ES");

            options.SetDefaultCulture(defaultCulture: spanishCultureInfo.Name);
            options.DefaultRequestCulture = new RequestCulture(culture: spanishCultureInfo);
            options.SupportedCultures = new List<CultureInfo> { spanishCultureInfo };
            options.SupportedUICultures = new List<CultureInfo> { spanishCultureInfo };
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new LocalizedRequestCultureProvider(culture: spanishCultureInfo.Name));
        });

        builder.Services.AddDbContext<IdentityDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options => {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<IdentityDBContext>()
            .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(providerName: TokenOptions.DefaultProvider)
            .AddUserManager<ApplicationUserManager>()
            .AddRoleManager<ApplicationRoleManager>()
            .AddErrorDescriber<LocalizedIdentityErrorDescriber>();

        builder.Services.Configure<CookiePolicyOptions>(options => {
            // options.CheckConsentNeeded = _ => false; By default is false
            options.MinimumSameSitePolicy = SameSiteMode.Strict;
        });

        builder.Services.ConfigureApplicationCookie(options => {
            options.Cookie.Name = ".AspNetCore.Identity.Application";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.SlidingExpiration = true;
            options.LoginPath = "/Cuentas/IniciarSesion";
        });

        builder.Services.AddSingleton(TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica"));

        builder.Services.AddScoped<IEmailSender, EmailSender>();

        builder.Services.AddScoped<AreasManager>();
        builder.Services.AddScoped<ModalidadesManager>();
        builder.Services.AddScoped<ServiciosManager>();
        builder.Services.AddScoped<DashboardManager>();
        builder.Services.AddScoped<SesionesManager>();
        builder.Services.AddScoped<ProyectosManager>();
        builder.Services.AddScoped<ColaboradoresManager>();
        builder.Services.AddScoped<ClientesManager>();

        builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

        WebApplication app = builder.Build();

        app.UseRequestLocalization();

        if (app.Environment.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error/Error");
        }

        app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCookiePolicy();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllerRoute("default", "{controller=Home}/{action=Administracion}/{id?}");

        // Seed: crear roles y usuario administrador por defecto si la BD está vacía
        if (app.Environment.IsDevelopment()) {
            using IServiceScope scope = app.Services.CreateScope();
            ApplicationUserManager userManager = scope.ServiceProvider.GetRequiredService<ApplicationUserManager>();
            ApplicationRoleManager roleManager = scope.ServiceProvider.GetRequiredService<ApplicationRoleManager>();

            if (!userManager.Users.Any()) {
                IConfigurationSection config = app.Configuration.GetSection("Identity:ApplicationUser");

                // Crear roles
                ApplicationRole rolAdministrador = new(Guid.NewGuid().ToString(), "Administrador", "Administrador del sistema.");
                await roleManager.CreateAsync(role: rolAdministrador);

                ApplicationRole rolCoordinador = new(Guid.NewGuid().ToString(), "Coordinador", "Coordinador.");
                await roleManager.CreateAsync(role: rolCoordinador);

                ApplicationRole rolColaborador = new(Guid.NewGuid().ToString(), "Colaborador", "Colaborador.");
                await roleManager.CreateAsync(role: rolColaborador);

                // Crear usuario administrador desde appsettings.Development.json
                ApplicationUser usuario = new(
                    Guid.NewGuid().ToString(),
                    config["Correo"],
                    config["Nombre"],
                    config["PrimerApellido"],
                    config["SegundoApellido"],
                    config["Identificacion"],
                    activo: true);

                await userManager.CreateAsync(user: usuario, config["Contrasena"]);
                await userManager.AddToRolesAsync(user: usuario, new[] { rolAdministrador.Name, rolCoordinador.Name, rolColaborador.Name });
            }
        }

        await app.RunAsync();
    }
}