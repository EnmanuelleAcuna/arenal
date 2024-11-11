using arenal.Models.Entities;

namespace arenal.Models.ViewModels;

public class BaseViewModel
{
	public BaseViewModel() { }

	public BaseViewModel(Base baseModel)
	{
		CreadoPor = baseModel.CreatedBy;
		CreadoEl = baseModel.DateCreated.ToString("dd/MM/yyyy");
		EditadoPor = baseModel.UpdatedBy;
		EditadoEl = baseModel.DateUpdated.HasValue ? baseModel.DateUpdated.Value.ToString("dd/MM/yyyy") : string.Empty;
	}

	public string CreadoPor { get; private set; }
	public string CreadoEl { get; private set; }
	public string EditadoPor { get; private set; } = string.Empty;
	public string EditadoEl { get; private set; } = string.Empty;
}
