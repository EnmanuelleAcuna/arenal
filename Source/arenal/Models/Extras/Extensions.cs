using System;
using System.Collections.Generic;
using System.Linq;
using arenal.Models.Identity;

namespace arenal.Models.Extras;

public static class Extensions
{
	public static bool IdValido(this string id)
	{
		if (String.IsNullOrEmpty(id)) return false;
		if (id.Equals("0", StringComparison.OrdinalIgnoreCase)) return false;
		if (!int.TryParse(id, out _)) return false;

		return true;
	}

	public static List<ApplicationUser> GetUsuariosDisponiblesParainstructor(this IList<ApplicationUser> usuarios, IList<ApplicationUser> usuariosInstructor)
	{
		List<ApplicationUser> usuariosDisponiblesParaSerInstructores = new();

		foreach (ApplicationUser usuario in usuarios)
		{
			if (!usuariosInstructor.Any(x => x.Id == usuario.Id))
			{
				usuariosDisponiblesParaSerInstructores.Add(usuario);
			}
		}

		return usuariosDisponiblesParaSerInstructores;
	}
}
