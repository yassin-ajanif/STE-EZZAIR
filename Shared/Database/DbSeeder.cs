namespace GestionCommerciale.Shared.Database;

public static class DbSeeder
{
    public const string DefaultAdminEmail = "admin@local";
    public const string DefaultAdminPassword = "admin";
    public const string DefaultClientName = "Client Comptoire";
    public const string DefaultSocieteMentionsLegales =
        "Drouguerie EZZAIR\nLot errahma S16  Guercif\n06.27.13.13.45 - 06.58.46.39.65 - 06.03.03.73.13";

    public static void Seed(AppDbContext db)
    {
        if (!db.AppSettings.Any())
        {
            db.AppSettings.Add(new AppSettingsRow
            {
                Id = 1,
                SocieteMentionsLegales = DefaultSocieteMentionsLegales
            });
            db.SaveChanges();
        }
        else
        {
            var settings = db.AppSettings.First(s => s.Id == 1);
            var changed = false;
            if (string.IsNullOrWhiteSpace(settings.SocieteMentionsLegales))
            {
                settings.SocieteMentionsLegales = DefaultSocieteMentionsLegales;
                changed = true;
            }

            // Company identity lives in Mentions légales only — clear dedicated nom/adresse.
            if (!string.IsNullOrWhiteSpace(settings.SocieteNom))
            {
                settings.SocieteNom = string.Empty;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(settings.SocieteAdresse))
            {
                settings.SocieteAdresse = string.Empty;
                changed = true;
            }

            if (changed)
                db.SaveChanges();
        }

        if (!db.Tiers.Any(t => t.Nom == DefaultClientName))
        {
            db.Tiers.Add(new GestionCommerciale.Modules.Tiers.Models.Tiers
            {
                Nom = DefaultClientName,
                Type = GestionCommerciale.Modules.Tiers.Models.TypeTiers.Client,
                Categorie = GestionCommerciale.Modules.Tiers.Models.CategorieTiers.Comptoir,
                Actif = true
            });
            db.SaveChanges();
        }
    }
}
