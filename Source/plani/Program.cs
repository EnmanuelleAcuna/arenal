using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using plani.Identity;
using plani.Models.Data;

namespace plani;

class Program
{
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		
		builder.Services.AddLogging();
		// builder.Services.AddApplicationInsightsTelemetry(options => options.ConnectionString = builderConfiguration["ApplicationInsights:ConnectionString"]);
		
		// ASP.Net Identity
		builder.Services.AddDbContext<IdentityDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
		
		builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
		
		builder.Services.Configure<RequestLocalizationOptions>(options =>
		{
			var spanishCulture = new CultureInfo("es-ES");
			var supportedCultures = new[] { "es-ES" };
			options.SetDefaultCulture(supportedCultures[0]);
			options.DefaultRequestCulture = new RequestCulture("es-ES");
			options.SupportedCultures = new List<CultureInfo> { spanishCulture };
			options.SupportedUICultures = new List<CultureInfo> { spanishCulture };
			options.RequestCultureProviders.Clear();
			options.RequestCultureProviders.Add(new FixedCultureProvider("es-ES"));
		});
		
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
			options.MinimumSameSitePolicy = SameSiteMode.Lax;
		});

		builder.Services.Configure<CookieOptions>(options =>
		{
			// options.Expires = DateTime.UtcNow.AddMinutes(20);
			options.SameSite = SameSiteMode.Strict;
			options.Secure = true;
		});

		builder.Services.ConfigureApplicationCookie(options =>
		{
			// options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
			options.Cookie.Name = ".AspNetCore.Identity.Application";
			options.SlidingExpiration = true;
			options.LoginPath = "/Cuentas/IniciarSesion";
			options.Cookie.SameSite = SameSiteMode.Strict;
		});
		
		builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
		
		builder.Services.AddScoped<IEmailSender, EmailSender>();

		builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

		WebApplication app = builder.Build();

		app.UseRequestLocalization();
		app.UseExceptionHandler(app.Environment.IsDevelopment() ? "/Error/ErrorDevelopment" : "/Error/Error");
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

public class FixedCultureProvider : RequestCultureProvider
{
	private readonly string _culture;

	public FixedCultureProvider(string culture)
	{
		_culture = culture;
	}

	public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
	{
		return Task.FromResult(new ProviderCultureResult(_culture));
	}
}
