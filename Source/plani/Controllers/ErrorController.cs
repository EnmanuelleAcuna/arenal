using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using plani.Models;

namespace plani.Controllers;

public class ErrorController : Controller
{
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	[IgnoreAntiforgeryToken]
	public IActionResult Error()
	{
		IExceptionHandlerFeature exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
		ErrorViewModel modelo = GetErrorViewModel(exceptionHandlerPathFeature, false);
		return View(modelo);
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	[IgnoreAntiforgeryToken]
	public IActionResult ErrorDevelopment()
	{
		IExceptionHandlerFeature exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
		ErrorViewModel modelo = GetErrorViewModel(exceptionHandlerPathFeature, true);
		return View("Error", modelo);
	}

	private ErrorViewModel GetErrorViewModel(IExceptionHandlerFeature exceptionHandlerPathFeature, bool isDevelopment)
	{
		string exceptionMessage = exceptionHandlerPathFeature.Error.Message;
		if (exceptionHandlerPathFeature.Error.InnerException != null)
			exceptionMessage += $"\n {exceptionHandlerPathFeature.Error.InnerException.Message}";

		ErrorViewModel modelo = new()
		{
			RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
			ExceptionMessage = exceptionMessage,
			StackTrace = isDevelopment ? exceptionHandlerPathFeature.Error.StackTrace : string.Empty
		};
		return modelo;
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	[IgnoreAntiforgeryToken]
	[Route("/404")]
	public IActionResult Error404()
	{
		return View();
	}
}
