using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GestionCommerciale.Modules.Stock.Models;

namespace GestionCommerciale.Shared.Helpers;

/// <summary>
/// Commits a product on Enter in one keystroke.
/// Avalonia AutoCompleteBox only closes the dropdown on Enter and does not set SelectedItem,
/// so without this helper users must press Enter twice.
/// Also supports scanner-style exact barcode Enter.
/// </summary>
public static class ProductBarcodeEnter
{
    public static readonly AttachedProperty<bool> EnableProperty =
        AvaloniaProperty.RegisterAttached<AutoCompleteBox, bool>(
            "Enable",
            typeof(ProductBarcodeEnter));

    public static void SetEnable(AutoCompleteBox box, bool value) =>
        box.SetValue(EnableProperty, value);

    public static bool GetEnable(AutoCompleteBox box) =>
        box.GetValue(EnableProperty);

    static ProductBarcodeEnter()
    {
        EnableProperty.Changed.AddClassHandler<AutoCompleteBox>(OnEnableChanged);
    }

    private static void OnEnableChanged(AutoCompleteBox box, AvaloniaPropertyChangedEventArgs e)
    {
        box.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        if (e.NewValue is true)
        {
            // Tunnel so we run before AutoCompleteBox closes the dropdown without committing.
            box.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }
    }

    private static void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Return))
            return;
        if (sender is not AutoCompleteBox box)
            return;

        var text = box.Text?.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        var match = ResolveProduct(box, text);
        if (match is null)
            return;

        e.Handled = true;
        box.IsDropDownOpen = false;

        // Force change notification even if the same item was already selected.
        if (ReferenceEquals(box.SelectedItem, match))
            box.SelectedItem = null;
        box.SelectedItem = match;

        DocumentLineSearchHelper.ClearAfterCatalogPick(() =>
        {
            box.SelectedItem = null;
            box.Text = string.Empty;
        });
    }

    private static Produit? ResolveProduct(AutoCompleteBox box, string text)
    {
        var barcode = FindExactBarcode(box.ItemsSource, text);
        if (barcode is not null)
            return barcode;

        if (box.SelectedItem is Produit selected)
            return selected;

        return FindBestFilterMatch(box.ItemsSource, text);
    }

    private static Produit? FindExactBarcode(IEnumerable? items, string barcode)
    {
        if (items is null)
            return null;

        foreach (var item in items)
        {
            if (item is Produit p
                && !string.IsNullOrWhiteSpace(p.CodeBarre)
                && p.CodeBarre.Trim().Equals(barcode, StringComparison.OrdinalIgnoreCase))
                return p;
        }

        return null;
    }

    private static Produit? FindBestFilterMatch(IEnumerable? items, string text)
    {
        if (items is null)
            return null;

        Produit? firstContains = null;
        foreach (var item in items)
        {
            if (item is not Produit p)
                continue;
            if (!ProductAutoComplete.ItemFilter(text, p))
                continue;

            if (p.Reference?.Trim().Equals(text, StringComparison.OrdinalIgnoreCase) == true
                || p.Designation?.Trim().Equals(text, StringComparison.OrdinalIgnoreCase) == true)
                return p;

            firstContains ??= p;
        }

        return firstContains;
    }
}
