using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using plani.Identity;
using plani.Models.ViewModels;

namespace plani.ViewComponents;

/// <summary>
/// View Component para mostrar la información del usuario en el header.
/// Implementa caché para optimizar el rendimiento y evitar consultas repetitivas a la BD.
/// </summary>
public class UserHeaderViewComponent : ViewComponent
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationUserManager _userManager;
    private readonly IMemoryCache _cache;

    // Tiempo de caché: 5 minutos (ajustable según necesidad)
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public UserHeaderViewComponent(
        SignInManager<ApplicationUser> signInManager,
        ApplicationUserManager userManager,
        IMemoryCache cache)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _cache = cache;
    }

    /// <summary>
    /// Invoca el componente y retorna la vista con los datos del usuario.
    /// Usa caché para evitar consultas repetitivas a la base de datos.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var viewModel = new UserHeaderViewModel
        {
            IsAuthenticated = _signInManager.IsSignedIn(HttpContext.User)
        };

        if (!viewModel.IsAuthenticated)
        {
            return View(viewModel);
        }

        // Obtener ID del usuario para la clave de caché
        var userId = _userManager.GetUserId(HttpContext.User);
        var cacheKey = $"UserHeader_{userId}";

        // Intentar obtener del caché
        if (_cache.TryGetValue(cacheKey, out UserHeaderViewModel cachedModel) && cachedModel != null)
        {
            return View(cachedModel);
        }

        // Si no está en caché, cargar desde BD
        var user = await _userManager.GetUserAsync(HttpContext.User);

        if (user != null)
        {
            viewModel.UserInitials = CalculateInitials(user);
            viewModel.UserName = GetFullName(user);

            var roles = await _userManager.GetRolesAsync(user);
            viewModel.DisplayRole = DetermineDisplayRole(roles);

            // Guardar en caché con expiración deslizante
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(CacheDuration);

            _cache.Set(cacheKey, viewModel, cacheOptions);
        }

        return View(viewModel);
    }

    /// <summary>
    /// Calcula las iniciales del usuario (primera letra del nombre + primera letra del apellido).
    /// </summary>
    private static string CalculateInitials(ApplicationUser user)
    {
        var firstInitial = user.Name?.Substring(0, 1).ToUpper() ?? "";
        var lastInitial = user.FirstLastName?.Substring(0, 1).ToUpper() ?? "";
        return firstInitial + lastInitial;
    }

    /// <summary>
    /// Obtiene el nombre completo del usuario.
    /// </summary>
    private static string GetFullName(ApplicationUser user)
    {
        return $"{user.Name} {user.FirstLastName}".Trim();
    }

    /// <summary>
    /// Determina el rol de mayor jerarquía del usuario.
    /// Jerarquía: Administrador > Coordinador > Colaborador > Usuario
    /// </summary>
    private static string DetermineDisplayRole(IList<string> roles)
    {
        // Array de roles en orden jerárquico (de mayor a menor)
        var roleHierarchy = new[] { "Administrador", "Coordinador", "Colaborador" };

        foreach (var role in roleHierarchy)
        {
            if (roles.Contains(role))
                return role;
        }

        return "Usuario";
    }
}
