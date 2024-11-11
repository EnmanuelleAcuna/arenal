using System;

namespace arenal.Models.Entities;

public class Base
{
	protected Base() { }

	public Base(string creadoPor, DateTime creadoEl)
	{
		DateCreated = creadoEl;
		CreatedBy = creadoPor;
	}

	public Base(string creadoPor, DateTime creadoEl, string editadoPor = null, DateTime? editadoEl = null)
	{
		DateCreated = creadoEl;
		CreatedBy = creadoPor;
		DateUpdated = editadoEl;
		UpdatedBy = editadoPor;
	}

	public DateTime DateCreated { get; set; }
	public string CreatedBy { get; set; }
	public DateTime? DateUpdated { get; set; }
	public string UpdatedBy { get; set; }
}
