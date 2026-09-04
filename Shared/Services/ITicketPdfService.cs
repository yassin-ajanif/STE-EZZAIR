using GestionCommerciale.Modules.AvoirFournisseur.Models;
using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Modules.CommandeFournisseur.Models;
using GestionCommerciale.Modules.Devis.Models;
using GestionCommerciale.Modules.Facturation.Models;
using GestionCommerciale.Modules.FactureFournisseur.Models;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Modules.Preparation.Models;
using GestionCommerciale.Modules.Reception.Models;
using GestionCommerciale.Shared.Models.Pdf;

namespace GestionCommerciale.Shared.Services;

/// <summary>Thermal ticket PDFs (58/80 mm). Completely separate from A4 <see cref="IPdfService"/>.</summary>
public interface ITicketPdfService
{
    Task<byte[]> BuildDevisTicketAsync(Devis devis, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildBonLivraisonTicketAsync(BonLivraison bl, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildBonReceptionTicketAsync(BonReception br, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildBonCommandeTicketAsync(BonCommande bc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildBonCommandeClientTicketAsync(BonCommandeClient bc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildFactureTicketAsync(Facture facture, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildBonPreparationTicketAsync(BonPreparation doc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildFactureFournisseurTicketAsync(FactureFournisseur factureFournisseur, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildAvoirTicketAsync(Avoir avoir, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
    Task<byte[]> BuildAvoirFournisseurTicketAsync(AvoirFournisseur doc, DocumentPartyPdfInfo party, float widthMm, CancellationToken cancellationToken = default);
}
