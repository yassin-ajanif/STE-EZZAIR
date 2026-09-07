namespace GestionCommerciale.Modules.Facturation.Models;

public enum ModePaiement
{
    Credit = 0,
    Cheque = 1,
    Especes = 2,
    TPE = 3,
    Virement = 4,
    Effet = 5,
    /// <summary>Write-off / forgiven remaining amount (reduces client debt, not cash).</summary>
    Remise = 6
}
