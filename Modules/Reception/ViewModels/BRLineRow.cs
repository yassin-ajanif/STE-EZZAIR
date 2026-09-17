using CommunityToolkit.Mvvm.ComponentModel;
using GestionCommerciale.Modules.Stock.Models;
using GestionCommerciale.Shared.Helpers;

namespace GestionCommerciale.Modules.Reception.ViewModels;

public partial class BRLineRow : ObservableObject
{
    [ObservableProperty] private int _produitId;
    [ObservableProperty] private string _reference = string.Empty;
    [ObservableProperty] private string _designation = string.Empty;
    [ObservableProperty] private string _conditionnement = string.Empty;
    [ObservableProperty] private decimal _quantiteRecue;
    [ObservableProperty] private decimal _prixUnitaireHt;
    [ObservableProperty] private decimal _tauxTva;

    public decimal MontantHt => QuantiteRecue * PrixUnitaireHt;
    public decimal MontantTtc => MontantHt * (1 + TauxTva / 100m);

    public decimal PrixUnitaireTtc
    {
        get => DocumentTotalsHelper.PrixUnitaireTtc(PrixUnitaireHt, TauxTva);
        set
        {
            var ht = DocumentTotalsHelper.PrixUnitaireHtFromTtc(value, TauxTva);
            if (PrixUnitaireHt == ht)
                OnPropertyChanged(nameof(PrixUnitaireTtc));
            else
                PrixUnitaireHt = ht;
        }
    }

    partial void OnQuantiteRecueChanged(decimal value) => NotifyMontants();
    partial void OnPrixUnitaireHtChanged(decimal value) => NotifyMontants();
    partial void OnTauxTvaChanged(decimal value) => NotifyMontants();

    public void ApplyCatalogProduct(Produit p)
    {
        ProduitId = p.Id;
        Reference = p.Reference;
        Designation = p.Designation;
        Conditionnement = p.Unite;
        PrixUnitaireHt = p.PrixAchatHT;
        TauxTva = p.TauxTVA;
        NotifyMontants();
    }

    private void NotifyMontants()
    {
        OnPropertyChanged(nameof(MontantHt));
        OnPropertyChanged(nameof(MontantTtc));
        OnPropertyChanged(nameof(PrixUnitaireTtc));
    }
}
