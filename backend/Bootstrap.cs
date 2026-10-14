using Microsoft.AspNetCore.DataProtection; // Protège le secret MFA de l’installation initiale.
using System.Text.Json; // Convertit les données de démarrage en JSON.
using System.Text.Json.Nodes; // Lit les programmes photographiés sans perdre leurs anomalies.
using Microsoft.EntityFrameworkCore; // Vérifie si des données ont déjà été initialisées.

namespace Iai; // Regroupe l'initialisation du portail.
public static class Bootstrap // Ne crée aucun compte avec un mot de passe public par défaut.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static async Task Initialize(PortalDb db, IConfiguration config, IDataProtectionProvider protection, IWebHostEnvironment env) // Initialise une base vide sans écraser l'historique.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        await db.Database.EnsureCreatedAsync(); // Crée les tables et contraintes lors d'une installation neuve.
        if (!await db.Curricula.AnyAsync()) // Les versions existantes ne sont jamais réimportées silencieusement.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var file = Path.Combine(AppContext.BaseDirectory, "curricula.json"); // Recherche la copie publiée du référentiel.
            foreach (var node in JsonNode.Parse(await File.ReadAllTextAsync(file))!.AsArray()) // Parcourt toutes les versions ITI et LPI.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var p = node!; // Chaque objet représente un cursus complet.
                var curriculum = new Curriculum() // Calcule ou prépare la variable curriculum, utilisée par les instructions suivantes.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    Id = Guid.Parse((string)p["id"]!), // Renseigne identifiant technique UUID avec la valeur calculée ou validée à droite.
                    Name = (string)p["name"]!, // Renseigne libellé affiché aux utilisateurs avec la valeur calculée ou validée à droite.
                    Level = (string)p["level"]!, // Renseigne niveau du cursus enregistré dans le référentiel avec la valeur calculée ou validée à droite.
                    Track = (string)p["track"]!, // Renseigne parcours administratif, distinct de la salle avec la valeur calculée ou validée à droite.
                    Speciality = (string)p["speciality"]!, // Renseigne spécialité du cursus enregistré dans le référentiel avec la valeur calculée ou validée à droite.
                    Version = (string)p["version"]!, // Renseigne numéro de version utilisé pour conserver ou révoquer un état avec la valeur calculée ou validée à droite.
                    Status = (string)p["status"]!, // Renseigne état courant du dossier ou de l’opération avec la valeur calculée ou validée à droite.
                    Content = p.ToJsonString() // Renseigne contenu figé et sérialisé de la version ou du relevé avec la valeur calculée ou validée à droite.
                }; // Enregistre le programme avec ses UUID propres.
                db.Curricula.Add(curriculum); // Ajoute la version au référentiel.
                Catalogue.Attach(db, curriculum); // Crée les références SQL de chaque UE et matière.
            } // Ferme ce bloc après l’exécution de ses instructions.
        } // Ferme ce bloc après l’exécution de ses instructions.

        if (!await db.Roles.AnyAsync()) // Crée les profils demandés une seule fois.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var academic = new[] // Calcule ou prépare la variable academic, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "academic.manage", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "students.manage", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "grades.manage", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "grades.publish", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "grades.rescue", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "attendance.manage", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "attendance.reports", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "discipline.manage", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "documents.manage", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "audit.view" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            }; // Les quatre profils académiques gèrent le cycle scolaire.
            foreach (var name in new[] // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "Super Administrateur", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "DAAS", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "Chargé des années scolaires", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "Représentant", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "CPE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "Surveillant", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "Étudiant" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            } // Ferme ce bloc après l’exécution de ses instructions.

            ) // Les fonctions de classe sont attribuées par mandat contextuel.
                db.Roles.Add(new() { Name = name, SystemCode = name == "Étudiant" ? "STUDENT" : name == "Super Administrateur" ? "SUPERADMIN" : "", Permissions = JsonSerializer.Serialize(name == "Super Administrateur" ? ["*"] : name == "Étudiant" ? Array.Empty<string>() : name is "CPE" or "Surveillant" ? ["attendance.manage", "attendance.reports", "discipline.manage"] : academic) }); // Conserve les autorisations dans la base, sans branchement par nom à l'exécution.
            await db.SaveChangesAsync(); // Les UUID des rôles sont disponibles pour créer le premier administrateur.
        } // Ferme ce bloc après l’exécution de ses instructions.

        if (!await db.Users.AnyAsync(u => u.StudentId == null)) // Le premier compte administratif vient des secrets de l'installation.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var password = config["Bootstrap:Password"]; // Lit un secret fourni par variable d'environnement.
            if (string.IsNullOrWhiteSpace(password) || password.Length < 12) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new InvalidOperationException("Définissez Bootstrap__Password avec au moins 12 caractères."); // Refuse les installations ouvertes avec un mot de passe faible.
            var role = await db.Roles.SingleAsync(r => r.SystemCode == "SUPERADMIN"); // Retrouve le rôle universel créé au démarrage.
            var user = new User // Calcule ou prépare la variable user, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Username = config["Bootstrap:Username"] ?? "admin", // Renseigne identifiant de connexion avec la valeur calculée ou validée à droite.
                Roles = JsonSerializer.Serialize(new[] { role.Id }) // Renseigne liste des UUID de profils attribués au compte avec la valeur calculée ou validée à droite.
            }; // Le nom initial est paramétrable.
            var mfa = config["Bootstrap:MfaSecret"]; // Le secret TOTP initial est fourni séparément du mot de passe.
            if (!env.IsDevelopment() && string.IsNullOrEmpty(mfa)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new InvalidOperationException("Bootstrap__MfaSecret est requis en production."); // Tout compte administratif déployé doit utiliser le second facteur.
            if (!string.IsNullOrEmpty(mfa)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Mfa.Code(mfa, 0); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                user.MfaSecret = protection.CreateProtector("iai-totp").Protect(mfa); // Met à jour secret TOTP chiffré pour le second facteur de l’objet user.
            } // Valide puis chiffre le secret initial.

            user.PasswordHash = Security.Password(user, password); // Stocke seulement le hachage Identity.
            db.Users.Add(user); // Prépare le compte pour la transaction finale.
        } // Ferme ce bloc après l’exécution de ses instructions.

        await db.SaveChangesAsync(); // Persiste l'ensemble du référentiel neuf.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.

public static class Inputs // Centralise les conversions et les validations élémentaires.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static string Text(this JsonObject j, string key) => j[key]?.GetValue<string>()?.Trim() ?? ""; // Un champ absent devient une chaîne vide contrôlable.
    public static string Required(this JsonObject j, string key) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var s = j.Text(key); // Calcule ou prépare la variable s, utilisée par les instructions suivantes.
        if (s.Length == 0 || s.Length > (key == "csv" ? 5_000_000 : 2000)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Champ requis ou trop long : " + key); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
        return s; // Remet au code appelant la valeur construite par cette expression.
    } // Limite aussi les tailles des textes libres.

    public static Guid Id(this JsonObject j, string key) => Guid.TryParse(j.Text(key), out var id) ? id : throw new RuleException("Identifiant invalide : " + key); // Un UUID mal formé est une erreur utilisateur.
    public static int Int(this JsonObject j, string key) => j[key]?.GetValue<int>() ?? throw new RuleException("Nombre requis : " + key); // Les semestres et tentatives sont des entiers.
    public static decimal Decimal(this JsonObject j, string key) => j[key]?.GetValue<decimal>() ?? throw new RuleException("Nombre requis : " + key); // Les notes utilisent une arithmétique décimale.
    public static async Task Auth(this Access a) => await a.Load(); // Rend explicite l'authentification au début de chaque route privée.
    public static async Task<Enrollment> Enrollment(this PortalDb db, Guid id) => await db.Enrollments.Include(e => e.Student).Include(e => e.Class).Include(e => e.Year).Include(e => e.Curriculum).SingleOrDefaultAsync(e => e.Id == id) ?? throw new RuleException("Inscription introuvable.", 404); // Charge toutes les références nécessaires aux calculs.
} // Ferme ce bloc après l’exécution de ses instructions.
