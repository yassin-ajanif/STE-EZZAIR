using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using GestionCommerciale.Modules.Reporting.ViewModels;

namespace GestionCommerciale.Modules.Reporting.Views;

public partial class ReportsListView : UserControl
{
    public ReportsListView()
    {
        InitializeComponent();
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);

    private void OnRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.Source is not Control source)
            return;

        // Keep focus when interacting with editable controls.
        if (source is TextBox or NumericUpDown
            || source.FindAncestorOfType<TextBox>() is not null
            || source.FindAncestorOfType<NumericUpDown>() is not null)
            return;

        TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus();
    }

    private void OnProfitFilterCardTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not ReportsListViewModel vm || sender is not Border border)
            return;

        switch (border.Tag as string)
        {
            case "Margin":
                vm.FilterProfitMarginCommand.Execute(null);
                break;
            case "Ventes":
                // "Total ventes" uses the same rows as the "Margin" filter (sale documents).
                vm.FilterProfitMarginCommand.Execute(null);
                break;
            case "AvoirsClient":
                vm.FilterProfitAvoirsClientCommand.Execute(null);
                break;
            case "Purchases":
                vm.FilterProfitPurchasesCommand.Execute(null);
                break;
            case "AvoirsFournisseur":
                vm.FilterProfitAvoirsFournisseurCommand.Execute(null);
                break;
            case "Charges":
                vm.FilterProfitChargesCommand.Execute(null);
                break;
            case "All":
                vm.FilterProfitAllCommand.Execute(null);
                break;
        }
    }
}
