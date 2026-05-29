using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using plani.Models.ViewModels;

namespace plani.Controllers;

public class ErrorController : Controller {
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public IActionResult Error() {
        IExceptionHandlerFeature exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        ErrorViewModel modelo = GetErrorViewModel(exceptionHandlerPathFeature: exceptionHandlerPathFeature, isDevelopment: false);
        return View(model: modelo);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public IActionResult ErrorDevelopment() {
        IExceptionHandlerFeature exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        ErrorViewModel modelo = GetErrorViewModel(exceptionHandlerPathFeature: exceptionHandlerPathFeature, isDevelopment: true);
        return View("Error", model: modelo);
    }

    private ErrorViewModel GetErrorViewModel(IExceptionHandlerFeature exceptionHandlerPathFeature, bool isDevelopment) {
        string exceptionMessage = exceptionHandlerPathFeature.Error.Message;
        if (exceptionHandlerPathFeature.Error.InnerException != null) {
            exceptionMessage += $"\n {exceptionHandlerPathFeature.Error.InnerException.Message}";
        }

        ErrorViewModel modelo = new() {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            ExceptionMessage = exceptionMessage,
            StackTrace = isDevelopment ? exceptionHandlerPathFeature.Error.StackTrace : string.Empty
        };
        return modelo;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [Route("/404")]
    public IActionResult Error404() {
        return View();
    }
}