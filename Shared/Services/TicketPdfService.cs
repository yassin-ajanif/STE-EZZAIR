using GestionCommerciale.Modules.AvoirFournisseur.Models;
using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Modules.CommandeFournisseur.Models;
using GestionCommerciale.Modules.Devis.Models;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.FactureFournisseur.Models;
using GestionCommerciale.Modules.Livraison;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Preparation.Models;
using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Shared.Database;
using GestionCommerciale.Shared.Helpers;
using GestionCommerciale.Shared.Models.Pdf;
using GestionCommerciale.Shared.Services.Pdf;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

namespace GestionCommerciale.Shared.Services;

public sealed class TicketPdfService : ITicketPdfService
{
    private readonly IAppSettingsService _settings;
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public TicketPdfService(
        IAppSettingsService settings,
        IDbContextFactory<AppDbContext> dbFactory)
    {
        _settings = settings;
        _dbFactory = dbFactory;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> BuildDevisTicketAsync(Devis devis, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.DevisTotals(devis.Lignes, devis.RemiseGlobale);
        var lines = devis.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.Quantite, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "DEVIS", devis.Numero, "Client", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildBonLivraisonTicketAsync(BonLivraison bl, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.BonLivraisonTotals(bl.Lignes);
        var lines = bl.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.QuantiteLivree, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.QuantiteLivree, l.PrixUnitaireHT, montant);
        }).ToList();

        var bccRef = await ResolveBonCommandeReferenceAsync(bl, cancellationToken);
        return Render(cfg, "BON DE LIVRAISON", bl.Numero, "Client", party.Nom, lines, totals.ht, widthMm,
            extraLabel: string.IsNullOrWhiteSpace(bccRef) ? null : "BC",
            extraValue: bccRef);
    }

    public async Task<byte[]> BuildBonReceptionTicketAsync(BonReception br, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.BonReceptionTotals(br.Lignes);
        var lines = br.Lignes.Select(l =>
        {
            var montant = l.QuantiteRecue * l.PrixUnitaireHT;
            return Line(l.Designation, l.QuantiteRecue, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "BON DE RÉCEPTION", br.Numero, "Fournisseur", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildBonCommandeTicketAsync(BonCommande bc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.BonCommandeTotals(bc.Lignes);
        var lines = bc.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.QuantiteCommandee, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.QuantiteCommandee, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "BON DE COMMANDE", bc.Numero, "Fournisseur", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildBonCommandeClientTicketAsync(BonCommandeClient bc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.BonCommandeClientTotals(bc.Lignes);
        var lines = bc.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.QuantiteCommandee, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.QuantiteCommandee, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "BON DE COMMANDE", bc.Numero, "Client", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildFactureTicketAsync(Facture facture, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.FactureTotals(facture.Lignes, facture.RemiseGlobale);
        var lines = facture.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.Quantite, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "FACTURE", facture.Numero, "Client", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildBonPreparationTicketAsync(BonPreparation doc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.BonPreparationTotals(doc.Lignes, doc.RemiseGlobale);
        var lines = doc.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.Quantite, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "BON DE PRÉPARATION", doc.Numero, "Client", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildFactureFournisseurTicketAsync(FactureFournisseur factureFournisseur, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.FactureFournisseurTotals(factureFournisseur.Lignes, factureFournisseur.RemiseGlobale);
        var lines = factureFournisseur.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.Quantite, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "FACTURE FOURNISSEUR", factureFournisseur.Numero, "Fournisseur", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildAvoirTicketAsync(Avoir avoir, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.AvoirTotals(avoir.Lignes);
        var lines = avoir.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.Quantite, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "AVOIR", avoir.Numero, "Client", party.Nom, lines, totals.ht, widthMm);
    }

    public async Task<byte[]> BuildAvoirFournisseurTicketAsync(AvoirFournisseur doc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default)
    {
        var cfg = await _settings.GetAsync(cancellationToken);
        var totals = DocumentTotalsHelper.AvoirFournisseurTotals(doc.Lignes);
        var lines = doc.Lignes.Select(l =>
        {
            var montant = DocumentTotalsHelper.LigneHT(l.Quantite, l.PrixUnitaireHT, l.Remise);
            return Line(l.Designation, l.Quantite, l.PrixUnitaireHT, montant);
        }).ToList();

        return Render(cfg, "AVOIR FOURNISSEUR", doc.Numero, "Fournisseur", party.Nom, lines, totals.ht, widthMm);
    }

    private async Task<string?> ResolveBonCommandeReferenceAsync(BonLivraison bl, CancellationToken cancellationToken)
    {
        var fromNote = BonCommandeReferenceStorage.ResolveForPdf(bl.Note);
        if (!string.IsNullOrWhiteSpace(fromNote))
            return fromNote;

        if (bl.BonCommandeClientId is not int bccId)
            return null;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.BonsCommandeClient.AsNoTracking()
            .Where(b => b.Id == bccId)
            .Select(b => b.Numero)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static TicketLinePdfModel Line(string designation, decimal qty, decimal pu, decimal montant) =>
        new()
        {
            Designation = designation,
            Quantite = qty,
            PrixUnitaire = pu,
            Montant = montant
        };

    private static byte[] Render(
        AppSettingsRow cfg,
        string kind,
        string numero,
        string partyLabel,
        string partyName,
        IReadOnlyList<TicketLinePdfModel> lines,
        decimal total,
        float widthMm,
        string? extraLabel = null,
        string? extraValue = null)
    {
        EnsureWidth(widthMm);
        var devise = string.IsNullOrWhiteSpace(cfg.Devise) ? "MAD" : cfg.Devise.Trim();
        var model = new TicketDocumentPdfModel
        {
            CompanyName = cfg.SocieteNom ?? string.Empty,
            CompanyInfoLines = BuildCompanyInfoLines(cfg),
            LogoBytes = TryLoadLogoBytes(cfg.SocieteLogoPath),
            DocumentKindLabel = kind,
            Numero = numero,
            DateText = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            ExtraInfoLabel = extraLabel,
            ExtraInfoValue = extraValue,
            PartyLabel = partyLabel,
            PartyName = string.IsNullOrWhiteSpace(partyName) ? "—" : partyName,
            Lines = lines,
            Total = total,
            Devise = devise,
            WidthMm = widthMm
        };
        return TicketPdfRenderer.Render(model);
    }

    private static IReadOnlyList<string> BuildCompanyInfoLines(AppSettingsRow cfg)
    {
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(cfg.SocieteAdresse))
            lines.Add(cfg.SocieteAdresse.Trim());
        if (!string.IsNullOrWhiteSpace(cfg.SocieteICE))
            lines.Add($"ICE : {cfg.SocieteICE.Trim()}");

        if (!string.IsNullOrWhiteSpace(cfg.SocieteMentionsLegales))
        {
            foreach (var part in cfg.SocieteMentionsLegales.Split(
                         '\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                lines.Add(part);
        }

        return lines;
    }

    private static byte[]? TryLoadLogoBytes(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        try
        {
            if (!File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }
        catch
        {
            return null;
        }
    }

    private static void EnsureWidth(float widthMm)
    {
        if (widthMm is not (58f or 80f))
            throw new ArgumentOutOfRangeException(nameof(widthMm), "Ticket width must be 58 or 80 mm.");
    }
}
