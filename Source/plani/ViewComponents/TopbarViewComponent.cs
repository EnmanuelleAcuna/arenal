using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using plani.Identity;

namespace plani.ViewComponents;

/// <summary>
/// View Component para el topbar (header).
/// Solo se muestra si el usuario está autenticado.
/// </summary>
public class TopbarViewComponent : ViewComponent
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public TopbarViewComponent(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    public IViewComponentResult Invoke()
    {
        // Si el usuario no está autenticado, no mostrar el topbar
        if (!_signInManager.IsSignedIn(HttpContext.User))
        {
            return Content(string.Empty);
        }

        return View();
    }
}
