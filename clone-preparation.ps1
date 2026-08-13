$root = "C:\Users\yassin\Desktop\sonlightining"
$src = Join-Path $root "Modules\Facturation"
$dst = Join-Path $root "Modules\Preparation"

New-Item -ItemType Directory -Force -Path @(
  "$dst\Models",
  "$dst\ViewModels",
  "$dst\Views",
  "$dst\Services"
) | Out-Null

$copies = @{
  "Models\Facture.cs" = "Models\BonPreparation.cs"
  "Models\FactureLigne.cs" = "Models\BonPreparationLigne.cs"
  "Models\Paiement.cs" = "Models\PaiementBonPreparation.cs"
  "ViewModels\FactureLineRow.cs" = "ViewModels\BonPreparationLineRow.cs"
  "ViewModels\FactureListRow.cs" = "ViewModels\BonPreparationListRow.cs"
  "ViewModels\FacturePaiementRowViewModel.cs" = "ViewModels\BonPreparationPaiementRowViewModel.cs"
  "ViewModels\FactureEditViewModel.cs" = "ViewModels\BonPreparationEditViewModel.cs"
  "ViewModels\FactureListViewModel.cs" = "ViewModels\BonPreparationListViewModel.cs"
  "Views\FactureEditView.axaml" = "Views\BonPreparationEditView.axaml"
  "Views\FactureEditView.axaml.cs" = "Views\BonPreparationEditView.axaml.cs"
  "Views\FactureListView.axaml" = "Views\BonPreparationListView.axaml"
  "Views\FactureListView.axaml.cs" = "Views\BonPreparationListView.axaml.cs"
  "Services\FactureWorkflowService.cs" = "Services\BonPreparationWorkflowService.cs"
  "Services\IFactureWorkflowService.cs" = "Services\IBonPreparationWorkflowService.cs"
  "Services\FactureBlLinkService.cs" = "Services\BonPreparationBlLinkService.cs"
  "Services\IFactureBlLinkService.cs" = "Services\IBonPreparationBlLinkService.cs"
  "Services\FactureBccLinkService.cs" = "Services\BonPreparationBccLinkService.cs"
  "Services\IFactureBccLinkService.cs" = "Services\IBonPreparationBccLinkService.cs"
}

foreach ($pair in $copies.GetEnumerator()) {
  Copy-Item (Join-Path $src $pair.Key) (Join-Path $dst $pair.Value) -Force
}

$replacements = @(
  @("IFactureBccLinkService", "IBonPreparationBccLinkService"),
  @("IFactureBlLinkService", "IBonPreparationBlLinkService"),
  @("IFactureWorkflowService", "IBonPreparationWorkflowService"),
  @("FactureBccLinkService", "BonPreparationBccLinkService"),
  @("FactureBlLinkService", "BonPreparationBlLinkService"),
  @("FactureWorkflowService", "BonPreparationWorkflowService"),
  @("FacturePaiementRowViewModel", "BonPreparationPaiementRowViewModel"),
  @("FactureEditViewModel", "BonPreparationEditViewModel"),
  @("FactureListViewModel", "BonPreparationListViewModel"),
  @("FactureEditView", "BonPreparationEditView"),
  @("FactureListView", "BonPreparationListView"),
  @("FactureLineRow", "BonPreparationLineRow"),
  @("FactureListRow", "BonPreparationListRow"),
  @("NextFactureAsync", "NextBonPreparationAsync"),
  @("BuildFacturePdfAsync", "BuildBonPreparationPdfAsync"),
  @("SyncFactureTotalTtc", "SyncBonPreparationTotalTtc"),
  @("FactureTotals", "BonPreparationTotals"),
  @("FactureTtc", "BonPreparationTtc"),
  @("RefreshFactureUi", "RefreshBonPreparationUi"),
  @("UpdateFactureTotalLines", "UpdateBonPreparationTotalLines"),
  @("CanRemoveFacture", "CanRemoveBonPreparation"),
  @("RemoveFactureAsync", "RemoveBonPreparationAsync"),
  @("RemoveFactureCommand", "RemoveBonPreparationCommand"),
  @("MenuDeleteFacture", "MenuDeleteBonPreparation"),
  @("DeleteFactureAsync", "DeleteBonPreparationAsync"),
  @("FactureModifiable", "BonPreparationModifiable"),
  @("FactureLignes", "BonPreparationLignes"),
  @("FactureLigne", "BonPreparationLigne"),
  @("db.Factures", "db.BonsPreparation"),
  @("db.Paiements", "db.PaiementsBonPreparation"),
  @("Factures.Remove", "BonsPreparation.Remove"),
  @("Factures.Add", "BonsPreparation.Add"),
  @("FactureId", "BonPreparationId"),
  @("using GestionCommerciale.Modules.Facturation", "using GestionCommerciale.Modules.Preparation"),
  @("namespace GestionCommerciale.Modules.Facturation", "namespace GestionCommerciale.Modules.Preparation"),
  @("GestionCommerciale.Modules.Facturation.ViewModels", "GestionCommerciale.Modules.Preparation.ViewModels"),
  @("GestionCommerciale.Modules.Facturation.Views", "GestionCommerciale.Modules.Preparation.Views"),
  @("using FactureEntity = GestionCommerciale.Modules.Preparation.Models.Facture", "using BonPreparationEntity = GestionCommerciale.Modules.Preparation.Models.BonPreparation"),
  @("FactureEntity", "BonPreparationEntity"),
  @("class Facture ", "class BonPreparation "),
  @("class Paiement ", "class PaiementBonPreparation "),
  @("public Facture?", "public BonPreparation?"),
  @("new Facture", "new BonPreparation"),
  @("Facture entity", "BonPreparation entity"),
  @("(Facture ", "(BonPreparation "),
  @("IEnumerable<Paiement>", "IEnumerable<PaiementBonPreparation>"),
  @("List<Paiement>", "List<PaiementBonPreparation>"),
  @("new Paiement", "new PaiementBonPreparation"),
  @("Paiement paiement", "PaiementBonPreparation paiement"),
  @("Paiement p)", "PaiementBonPreparation p)"),
  @("Paiement p,", "PaiementBonPreparation p,"),
  @(".Facture ", ".BonPreparation "),
  @(".Facture.", ".BonPreparation."),
  @(".Facture}", ".BonPreparation}"),
  @("row.Facture", "row.BonPreparation"),
  @("Selected.Facture", "Selected.BonPreparation"),
  @("item.Facture", "item.BonPreparation"),
  @("LoadDocumentLineColumns(`"facture`"", "LoadDocumentLineColumns(`"bon_preparation`""),
  @("GetDocumentLineColumnVisibility(`"facture`"", "GetDocumentLineColumnVisibility(`"bon_preparation`""),
  @("`"`"Fact_", "`"`"Bp_"),
  @("`"`"FactList_", "`"`"BpList_"),
  @("Btn_NewFacture", "Btn_NewBonPreparation"),
  @("facture-overdue", "bp-overdue"),
  @("FactureListRoot", "BonPreparationListRoot"),
  @("public BonPreparation? Facture", "public BonPreparation? BonPreparation"),
  @("public List<Paiement> Paiements", "public List<PaiementBonPreparation> Paiements")
)

Get-ChildItem -Path $dst -Recurse -File | ForEach-Object {
  $c = [System.IO.File]::ReadAllText($_.FullName)
  foreach ($r in $replacements) {
    $c = $c.Replace($r[0], $r[1])
  }
  # Translation keys
  $c = $c.Replace('"Fact_', '"Bp_')
  $c = $c.Replace('"FactList_', '"BpList_')
  $c = $c.Replace('LoadDocumentLineColumns("facture"', 'LoadDocumentLineColumns("bon_preparation"')
  $c = $c.Replace('GetDocumentLineColumnVisibility("facture"', 'GetDocumentLineColumnVisibility("bon_preparation"')
  [System.IO.File]::WriteAllText($_.FullName, $c)
}

Write-Output "Copied $($copies.Count) files"
Get-ChildItem $dst -Recurse -File | ForEach-Object { $_.FullName.Replace($root + '\', '') }
