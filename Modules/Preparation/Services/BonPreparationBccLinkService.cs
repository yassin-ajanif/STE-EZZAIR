using GestionCommerciale.Modules.CommandeClient.Models;
using GestionCommerciale.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace GestionCommerciale.Modules.Preparation.Services;

public sealed class BonPreparationBccLinkService : IBonPreparationBccLinkService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public BonPreparationBccLinkService(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<List<BonCommandeClient>> GetAvailableBccsForClientAsync(int clientId, int? excludeBonPreparationId = null, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var query = db.BonsCommandeClient.AsNoTracking()
            .Where(b => b.ClientId == clientId && b.BonPreparationId == null);

        if (excludeBonPreparationId.HasValue)
            query = query.Where(b => b.BonPreparationId != excludeBonPreparationId.Value);

        return await query
            .Include(b => b.Lignes)
            .OrderBy(b => b.Date).ThenBy(b => b.Numero)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> ValidateBccsForFactureAsync(int clientId, IReadOnlyList<int> bccIds, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var bccs = await db.BonsCommandeClient.AsNoTracking()
            .Where(b => bccIds.Contains(b.Id))
            .ToListAsync(cancellationToken);

        var foundIds = bccs.Select(b => b.Id).ToHashSet();
        foreach (var id in bccIds)
        {
            if (!foundIds.Contains(id))
                errors.Add($"BC #{id} introuvable.");
        }

        foreach (var b in bccs.Where(b => b.ClientId != clientId))
            errors.Add($"{b.Numero} : le client ne correspond pas à celui de la facture.");

        foreach (var b in bccs.Where(b => b.BonPreparationId != null))
            errors.Add($"{b.Numero} est déjà facturé.");

        return errors;
    }

    public async Task AssignBccsToFactureAsync(AppDbContext db, int factureId, IReadOnlyList<int> bccIds, CancellationToken cancellationToken = default)
    {
        var currentBccs = await db.BonsCommandeClient.Where(b => b.BonPreparationId == factureId).ToListAsync(cancellationToken);
        var targetSet = bccIds.ToHashSet();

        foreach (var bcc in currentBccs)
        {
            if (!targetSet.Contains(bcc.Id))
                bcc.BonPreparationId = null;
        }

        foreach (var bccId in bccIds)
        {
            var bcc = await db.BonsCommandeClient.FindAsync(new object[] { bccId }, cancellationToken);
            if (bcc != null)
                bcc.BonPreparationId = factureId;
        }
    }

    public async Task<List<BonCommandeClient>> GetLinkedBccsAsync(int factureId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        return await db.BonsCommandeClient.AsNoTracking()
            .Where(b => b.BonPreparationId == factureId)
            .OrderBy(b => b.Date).ThenBy(b => b.Numero)
            .ToListAsync(cancellationToken);
    }
}
