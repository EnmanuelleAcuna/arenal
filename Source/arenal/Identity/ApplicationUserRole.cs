using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace arenal.Identity;

[Keyless]
public class ApplicationUserRole : IdentityUserRole<string>
{
	public virtual ApplicationUser User { get; set; }
	public virtual ApplicationRole Role { get; set; }
}
