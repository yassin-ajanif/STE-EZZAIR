using GestionCommerciale.Modules.Preparation.ViewModels;
using GestionCommerciale.Modules.Livraison.Models;
using GestionCommerciale.Shared.Database;

namespace GestionCommerciale.Modules.Preparation.Services;

public interface IBonPreparationBlLinkService
{
    Task<List<BonLivraison>> GetAvailableBlsForClientAsync(int clientId, int? excludeBonPreparationId = null, CancellationToken cancellationToken = default);
    Task<List<string>> ValidateBlsForFactureAsync(int clientId, IReadOnlyList<int> blIds, CancellationToken cancellationToken = default);
    Task<List<BonPreparationLineRow>> LoadBlLinesAsync(int blId, CancellationToken cancellationToken = default);
    Task AssignBlsToFactureAsync(AppDbContext db, int factureId, IReadOnlyList<int> blIds, CancellationToken cancellationToken = default);
    Task<List<BonLivraison>> GetLinkedBlsAsync(int factureId, CancellationToken cancellationToken = default);
    Task<string?> GetInvoicingStatusAsync(int blId, CancellationToken cancellationToken = default);
}
