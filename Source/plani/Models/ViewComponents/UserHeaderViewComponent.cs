using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using plani.Identity;
using plani.Models.ViewModels;

namespace plani.Models.ViewComponents;

/// <summary>
///     View Component para mostrar la información del usuario en el header.
///     Implementa caché para optimizar el rendimiento y evitar consultas repetitivas a la BD.
/// </summary>
public class UserHeaderViewComponent : ViewComponent {
    // Tiempo de caché: 5 minutos (ajustable según necesidad)
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(value: 5);
    private readonly IMemoryCache _cache;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationUserManager _userManager;

    public UserHeaderViewComponent(
        SignInManager<ApplicationUser> signInManager,
        ApplicationUserManager userManager,
        IMemoryCache cache) {
        _signInManager = signInManager;
        _userManager = userManager;
        _cache = cache;
    }

    /// <summary>
    ///     Invoca el componente y retorna la vista con los datos del usuario.
    ///     Usa caché para evitar consultas repetitivas a la base de datos.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync() {
        UserHeaderViewModel viewModel = new() {
            IsAuthenticated = _signInManager.IsSignedIn(principal: HttpContext.User)
        };

        if (!viewModel.IsAuthenticated) {
            return View(model: viewModel);
        }

        // Obtener ID del usuario para la clave de caché
        string userId = _userManager.GetUserId(principal: HttpContext.User);
        string cacheKey = $"UserHeader_{userId}";

        // Intentar obtener del caché
        if (_cache.TryGetValue(key: cacheKey, out UserHeaderViewModel cachedModel) && cachedModel != null) {
            return View(model: cachedModel);
        }

        // Si no está en caché, cargar desde BD
        ApplicationUser user = await _userManager.GetUserAsync(principal: HttpContext.User);

        if (user != null) {
            viewModel.UserInitials = CalculateInitials(user: user);
            viewModel.UserName = GetFullName(user: user);

            IList<string> roles = await _userManager.GetRolesAsync(user: user);
            viewModel.DisplayRole = DetermineDisplayRole(roles: roles);

            // Guardar en caché con expiración deslizante
            MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(offset: CacheDuration);

            _cache.Set(key: cacheKey, value: viewModel, options: cacheOptions);
        }

        return View(model: viewModel);
    }

    /// <summary>
    ///     Calcula las iniciales del usuario (primera letra del nombre + primera letra del apellido).
    /// </summary>
    private static string CalculateInitials(ApplicationUser user) {
        string firstInitial = user.Name?.Substring(startIndex: 0, length: 1).ToUpper() ?? "";
        string lastInitial = user.FirstLastName?.Substring(startIndex: 0, length: 1).ToUpper() ?? "";
        return firstInitial + lastInitial;
    }

    /// <summary>
    ///     Obtiene el nombre completo del usuario.
    /// </summary>
    private static string GetFullName(ApplicationUser user) {
        return $"{user.Name} {user.FirstLastName}".Trim();
    }

    /// <summary>
    ///     Determina el rol de mayor jerarquía del usuario.
    ///     Jerarquía: Administrador > Coordinador > Colaborador > Usuario
    /// </summary>
    private static string DetermineDisplayRole(IList<string> roles) {
        // Array de roles en orden jerárquico (de mayor a menor)
        string[] roleHierarchy = { "Administrador", "Coordinador", "Colaborador" };

        foreach (string role in roleHierarchy) {
            if (roles.Contains(item: role)) {
                return role;
            }
        }

        return "Usuario";
    }
}