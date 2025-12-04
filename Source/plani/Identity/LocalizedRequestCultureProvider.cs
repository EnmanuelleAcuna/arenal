using Microsoft.AspNetCore.Localization;

namespace plani.Identity;

public class LocalizedRequestCultureProvider : RequestCultureProvider
{
    private readonly string _culture;

    public LocalizedRequestCultureProvider(string culture)
    {
        _culture = culture;
    }

    public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        return Task.FromResult(new ProviderCultureResult(_culture));
    }
}