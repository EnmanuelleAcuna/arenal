namespace arenal.Models;

public class Base
{
    protected Base()
    {
    }

    public DateTime DateCreated { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime? DateUpdated { get; private set; }
    public string UpdatedBy { get; private set; }
    public bool IsDeleted { get; set; }
    public string DeletedBy { get; private set; }
    public DateTime? DateDeleted { get; private set; }
    
    public void Eliminar(string eliminadoPor)
    {
        if (IsDeleted)
            throw new InvalidOperationException(Utils.MensajeErrorObjetoYaEliminado);
        
        IsDeleted = true;
        RegistrarEliminacion(eliminadoPor, DateTime.UtcNow);
    }

    public void RegristrarCreacion(string creadoPor, DateTime creadoEl)
    {
        CreatedBy = creadoPor;
        DateCreated = creadoEl;
    }

    public void RegistrarActualizacion(string actualizadoPor, DateTime actualizadoEl)
    {
        UpdatedBy = actualizadoPor;
        DateUpdated = actualizadoEl;
    }

    public void RegistrarEliminacion(string eliminadoPor, DateTime eliminadoEl)
    {
        DeletedBy = eliminadoPor;
        DateDeleted = eliminadoEl;
    }
}

public class BaseModel
{
    public BaseModel()
    {
    }

    public BaseModel(Base baseModel)
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
