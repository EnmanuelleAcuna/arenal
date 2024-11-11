using System;
using arenal.Models;
using arenal.Models.Core;
using arenal.Models.Entities;
using arenal.Models.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace arenal;

class Program
{
	public static void Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
		
		builder.Services.AddLogging();
		// builder.Services.AddApplicationInsightsTelemetry(options => options.ConnectionString = builderConfiguration["ApplicationInsights:ConnectionString"]);
		
		// ASP.Net Identity
		builder.Services.AddDbContext<IdentityDBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
		
		builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
		{
			options.User.RequireUniqueEmail = true;
			options.SignIn.RequireConfirmedAccount = false;
			options.Password.RequiredLength = 6;
		})
		.AddEntityFrameworkStores<IdentityDBContext>()
		.AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>(TokenOptions.DefaultProvider)
		.AddUserManager<ApplicationUserManager<ApplicationUser>>();

		builder.Services.Configure<CookiePolicyOptions>(options =>
		{
			// options.CheckConsentNeeded = _ => false; By default is false
			options.MinimumSameSitePolicy = SameSiteMode.Lax;
		});

		builder.Services.Configure<CookieOptions>(options =>
		{
			options.Expires = DateTime.Now.AddMinutes(20);
			options.SameSite = SameSiteMode.Strict;
			options.Secure = true;
		});

		builder.Services.ConfigureApplicationCookie(options =>
		{
			options.Cookie.Name = ".AspNetCore.Identity.Application";
			options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
			options.SlidingExpiration = true;
			options.LoginPath = "/Cuentas/IniciarSesion";
			options.Cookie.SameSite = SameSiteMode.Strict;
		});
		
		builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
		
		builder.Services.AddTransient<IBaseCore<TipoMaquina>, TiposMaquina>();
		builder.Services.AddTransient<IBaseCore<Maquina>, Maquinas>();
		builder.Services.AddTransient<IBaseCore<TipoEjercicio>, TiposEjercicio>();
		builder.Services.AddTransient<IBaseCore<Ejercicio>, Ejercicios>();
		builder.Services.AddTransient<IBaseCore<TipoMedida>, TiposMedida>();
		builder.Services.AddTransient<IBaseCore<GrupoMuscular>, GruposMusculares>();
		builder.Services.AddScoped<IEmailSender, EmailSender>();

		builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

		WebApplication app = builder.Build();

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
