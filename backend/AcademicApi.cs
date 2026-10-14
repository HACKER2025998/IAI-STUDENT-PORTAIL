using System.Text.Json; // Sérialise les données pédagogiques et les contacts.
using System.Text.Json.Nodes; // Lit les formulaires et les versions de programmes.
using Microsoft.EntityFrameworkCore; // Exécute les requêtes et transactions relationnelles.

namespace Iai; // Module de scolarité.
public static class AcademicApi // Organise les opérations d'année, classe et inscription.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Installe les routes métier.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapGet("/api/years", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/years aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            return Results.Ok(await db.Years.OrderByDescending(y => y.Code).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les étudiants consultent aussi les années historiques.
        app.MapPost("/api/years", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/years aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            if (!DateOnly.TryParse(j.Text("start"), out var start) || !DateOnly.TryParse(j.Text("end"), out var end) || start >= end) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Dates d'année invalides."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var y = new Year // Calcule ou prépare la variable y, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Code = j.Required("code"), // Renseigne code métier imprimé ou saisi par le gestionnaire avec la valeur calculée ou validée à droite.
                Start = start, // Renseigne date de début de l’année académique avec la valeur calculée ou validée à droite.
                End = end, // Renseigne date de fin de l’année académique avec la valeur calculée ou validée à droite.
                Welcome = j.Required("welcome") // Renseigne message d’accueil de la nouvelle année avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Years.Add(y); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("YEAR_CREATED", y.Id, new { y.Code }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(y); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Une année est d'abord créée en brouillon.
        app.MapPost("/api/years/{id:guid}/open", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP POST /api/years/{id:guid}/open aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable); // Ouvre une transaction SQL afin qu’une erreur annule toute l’opération.
            var year = await db.Years.FindAsync(id) ?? throw new RuleException("Année inconnue.", 404); // Charge depuis la base la variable year, utilisée par les instructions suivantes.
            if (year.Status != "DRAFT") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Seule une année en brouillon peut être ouverte."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            foreach (var y in await db.Years.Where(y => y.Status == "OPEN").ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                y.Status = "CLOSED"; // Met à jour état courant du dossier ou de l’opération de l’objet y.
            foreach (var e in await db.Enrollments.Where(e => e.Status == "ACTIVE").ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                e.Status = "COMPLETED"; // Met à jour état courant du dossier ou de l’opération de l’objet e.
            foreach (var u in await db.Users.Where(u => u.StudentId != null).ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                u.Version++; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            year.Status = "OPEN"; // Met à jour état courant du dossier ou de l’opération de l’objet year.
            a.Audit("YEAR_OPENED", id, new { year.Code, sessionsRevoked = true }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            await tx.CommitAsync(); // Valide la transaction pour rendre tous ses changements persistants ensemble.
            return Results.Ok(year); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Ferme l'ancien cycle, conserve l'historique et révoque les sessions étudiantes atomiquement.
        app.MapPost("/api/years/{id:guid}/close", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP POST /api/years/{id:guid}/close aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            var y = await db.Years.FindAsync(id) ?? throw new RuleException("Année inconnue.", 404); // Charge depuis la base la variable y, utilisée par les instructions suivantes.
            y.Status = "CLOSED"; // Met à jour état courant du dossier ou de l’opération de l’objet y.
            foreach (var e in await db.Enrollments.Where(e => e.YearId == id && e.Status == "ACTIVE").ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                e.Status = "COMPLETED"; // Met à jour état courant du dossier ou de l’opération de l’objet e.
            a.Audit("YEAR_CLOSED", id, new { y.Code }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Clôture sans supprimer classe, note ou bulletin.
        app.MapGet("/api/curricula", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/curricula aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            return Results.Ok(await db.Curricula.ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le référentiel est lisible avec les avertissements de transcription.
        app.MapPost("/api/curricula", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/curricula aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            ValidateCurriculum(j); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            var c = new Curriculum // Calcule ou prépare la variable c, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Name = j.Required("name"), // Renseigne libellé affiché aux utilisateurs avec la valeur calculée ou validée à droite.
                Level = j.Required("level"), // Renseigne niveau du cursus enregistré dans le référentiel avec la valeur calculée ou validée à droite.
                Track = j.Required("track"), // Renseigne parcours administratif, distinct de la salle avec la valeur calculée ou validée à droite.
                Speciality = j.Required("speciality"), // Renseigne spécialité du cursus enregistré dans le référentiel avec la valeur calculée ou validée à droite.
                Version = j.Required("version"), // Renseigne numéro de version utilisé pour conserver ou révoquer un état avec la valeur calculée ou validée à droite.
                Status = j.Text("status") == "APPROVED" ? "APPROVED" : "DRAFT", // Renseigne état courant du dossier ou de l’opération avec la valeur calculée ou validée à droite.
                Content = j.ToJsonString() // Renseigne contenu figé et sérialisé de la version ou du relevé avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Curricula.Add(c); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            Catalogue.Attach(db, c); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            a.Audit("CURRICULUM_VERSION_CREATED", c.Id, new { c.Name, c.Version }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(c); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Toute modification pédagogique crée une nouvelle version indépendante.
        app.MapGet("/api/classes", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/classes aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("academic.view") || await a.Can("attendance.reports") || await a.Can("grades.view"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            var ids = await db.Enrollments.Where(e => e.StudentId == a.User.StudentId).Select(e => e.ClassId).ToListAsync(); // Charge depuis la base la variable ids, utilisée par les instructions suivantes.
            return Results.Ok(await db.Classes.Where(c => all || ids.Contains(c.Id)).Select(c => new { c.Id, c.Name, c.Level, c.Speciality, c.Tracks, c.YearId }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Un étudiant ne reçoit pas les autres salles ni les empreintes de code.
        app.MapPost("/api/classes", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/classes aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            var year = await db.Years.FindAsync(j.Id("yearId")) ?? throw new RuleException("Année inconnue."); // Charge depuis la base la variable year, utilisée par les instructions suivantes.
            if (year.Status != "OPEN" && year.Status != "DRAFT" && !(year.Status == "CLOSED" && j["historical"]?.GetValue<bool>() == true)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Année clôturée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var tracks = j["tracks"]!.Deserialize<string[]>()!; // Calcule ou prépare la variable tracks, utilisée par les instructions suivantes.
            var level = j.Required("level"); // Calcule ou prépare la variable level, utilisée par les instructions suivantes.
            var speciality = j.Required("speciality"); // Calcule ou prépare la variable speciality, utilisée par les instructions suivantes.
            if (tracks.Length == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Choisissez au moins un parcours."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            foreach (var t in tracks) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                if (!await db.Curricula.AnyAsync(c => c.Level == level && c.Speciality == speciality && c.Track == t)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Programme manquant pour " + t); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var c = new SchoolClass // Calcule ou prépare la variable c, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                YearId = year.Id, // Renseigne identifiant de l’année académique avec la valeur calculée ou validée à droite.
                Name = j.Required("name"), // Renseigne libellé affiché aux utilisateurs avec la valeur calculée ou validée à droite.
                Level = level, // Renseigne niveau du cursus enregistré dans le référentiel avec la valeur calculée ou validée à droite.
                Speciality = speciality, // Renseigne spécialité du cursus enregistré dans le référentiel avec la valeur calculée ou validée à droite.
                Tracks = JsonSerializer.Serialize(tracks) // Renseigne parcours administratifs autorisés dans cette salle avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Classes.Add(c); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("CLASS_CREATED", c.Id, new { c.Name }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { c.Id, c.Name }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les classes sont physiques et les parcours autorisés restent paramétrés.
        app.MapPost("/api/classes/{id:guid}/join-code", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP POST /api/classes/{id:guid}/join-code aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            var c = await db.Classes.FindAsync(id) ?? throw new RuleException("Classe inconnue.", 404); // Charge depuis la base la variable c, utilisée par les instructions suivantes.
            var code = Security.Secret(); // Calcule ou prépare la variable code, utilisée par les instructions suivantes.
            c.JoinHash = Security.Hash(code); // Met à jour empreinte du code partagé de classe de l’objet c.
            c.JoinExpires = PortalClock.Now.AddDays(7); // Met à jour date limite d’utilisation du code de classe de l’objet c.
            a.Audit("JOIN_CODE_REGENERATED", id, new { c.JoinExpires }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { code, expires = c.JoinExpires }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Une régénération rend immédiatement l'ancien code inutilisable.
        app.MapGet("/api/students", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/students aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.view"); // Exige la permission students.view contenue dans les rôles actuels du compte.
            return Results.Ok(await db.Students.Select(s => new { s.Id, s.Number, s.FirstName, s.LastName, s.Profile, s.Status, s.Track }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les codes d'activation ne sont jamais listés.
        app.MapPost("/api/students", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/students aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.create"); // Exige la permission students.create contenue dans les rôles actuels du compte.
            var track = j.Required("track"); // Calcule ou prépare la variable track, utilisée par les instructions suivantes.
            if (!await db.Curricula.AnyAsync(c => c.Track == track)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Parcours inconnu."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var code = Security.Secret(); // Calcule ou prépare la variable code, utilisée par les instructions suivantes.
            var s = new Student // Calcule ou prépare la variable s, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Number = "IAI-" + PortalClock.Now.Year + "-" + Security.Secret()[..10], // Renseigne matricule étudiant permanent avec la valeur calculée ou validée à droite.
                FirstName = j.Required("firstName"), // Renseigne prénoms du titulaire avec la valeur calculée ou validée à droite.
                LastName = j.Required("lastName"), // Renseigne nom de famille du titulaire avec la valeur calculée ou validée à droite.
                Track = track, // Renseigne parcours administratif, distinct de la salle avec la valeur calculée ou validée à droite.
                Profile = j["profile"]?.ToJsonString() ?? "{}", // Renseigne coordonnées et personnes à prévenir, sérialisées en JSON avec la valeur calculée ou validée à droite.
                ActivationHash = Security.Hash(code) // Renseigne empreinte du secret personnel d’activation avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Students.Add(s); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("STUDENT_CREATED", s.Id, new { s.Number }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { s.Id, s.Number, activationCode = code }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le matricule et le code d'activation personnel sont distincts.
        app.MapPost("/api/students/{id:guid}/reset-password", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/students/{id:guid}/reset-password aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.reset_password"); // Exige la permission students.reset_password contenue dans les rôles actuels du compte.
            var u = await db.Users.SingleOrDefaultAsync(u => u.StudentId == id) ?? throw new RuleException("Le compte étudiant n’est pas encore activé."); // Charge depuis la base la variable u, utilisée par les instructions suivantes.
            var password = j.Text("password"); // Calcule ou prépare la variable password, utilisée par les instructions suivantes.
            if (password == "") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                password = Security.Secret(); // Renseigne le champ password avec la valeur calculée ou validée à droite.
            if (password.Length < 12) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Mot de passe : douze caractères minimum."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            u.PasswordHash = Security.Password(u, password); // Met à jour empreinte salée du mot de passe, jamais le mot de passe en clair de l’objet u.
            u.Version++; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            a.Audit("STUDENT_PASSWORD_RESET", id, new { u.Username }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { u.Username, password }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les quatre profils de scolarité peuvent réinitialiser un compte étudiant sans gérer les comptes administratifs.
        app.MapPost("/api/students/{id:guid}/activation-code", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP POST /api/students/{id:guid}/activation-code aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.reset_password"); // Exige la permission students.reset_password contenue dans les rôles actuels du compte.
            if (await db.Users.AnyAsync(u => u.StudentId == id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Compte activé : utilisez la réinitialisation du mot de passe."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var student = await db.Students.FindAsync(id) ?? throw new RuleException("Étudiant inconnu."); // Charge depuis la base la variable student, utilisée par les instructions suivantes.
            var code = Security.Secret(); // Calcule ou prépare la variable code, utilisée par les instructions suivantes.
            student.ActivationHash = Security.Hash(code); // Met à jour empreinte du secret personnel d’activation de l’objet student.
            a.Audit("ACTIVATION_CODE_REGENERATED", id, new { student.Number }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { student.Number, activationCode = code }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le secret d'activation perdu est remplacé plutôt que récupéré.
        app.MapPut("/api/students/{id:guid}", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP PUT /api/students/{id:guid} aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.update"); // Exige la permission students.update contenue dans les rôles actuels du compte.
            var s = await db.Students.FindAsync(id) ?? throw new RuleException("Étudiant inconnu.", 404); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            s.FirstName = j.Required("firstName"); // Met à jour prénoms du titulaire de l’objet s.
            s.LastName = j.Required("lastName"); // Met à jour nom de famille du titulaire de l’objet s.
            s.Profile = j["profile"]?.ToJsonString() ?? s.Profile; // Met à jour coordonnées et personnes à prévenir, sérialisées en JSON de l’objet s.
            a.Audit("STUDENT_UPDATED", id, new { s.FirstName, s.LastName }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // La modification des coordonnées préserve le matricule et le cursus historique.
        app.MapPost("/api/students/{id:guid}/disable", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/students/{id:guid}/disable aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.disable"); // Exige la permission students.disable contenue dans les rôles actuels du compte.
            var s = await db.Students.FindAsync(id) ?? throw new RuleException("Étudiant inconnu.", 404); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            var status = j.Required("status"); // Calcule ou prépare la variable status, utilisée par les instructions suivantes.
            if (!new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "ACTIVE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "INACTIVE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "WITHDRAWN", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "SUSPENDED", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "GRADUATED" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            }.Contains(status)) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                throw new RuleException("Statut invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            s.Status = status; // Met à jour état courant du dossier ou de l’opération de l’objet s.
            foreach (var u in await db.Users.Where(u => u.StudentId == id).ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                u.Active = status == "ACTIVE"; // Met à jour état d’activation du compte de l’objet u.
                u.Version++; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            } // Ferme ce bloc après l’exécution de ses instructions.

            a.Audit("STUDENT_STATUS_CHANGED", id, new { status, reason = j.Required("reason") }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // L'abandon grise le dossier et conserve ses années précédentes.
        app.MapDelete("/api/students/{id:guid}", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP DELETE /api/students/{id:guid} aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.delete"); // Exige la permission students.delete contenue dans les rôles actuels du compte.
            if (await db.Enrollments.AnyAsync(e => e.StudentId == id) || await db.Users.AnyAsync(u => u.StudentId == id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Dossier avec historique : utilisez la désactivation."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var s = await db.Students.FindAsync(id) ?? throw new RuleException("Étudiant inconnu.", 404); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            db.Students.Remove(s); // Prépare la suppression de cet objet ; les contraintes relationnelles protègent ses références.
            a.Audit("EMPTY_STUDENT_DELETED", id, new { s.Number }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Seule une fiche créée par erreur sans historique peut être supprimée.
        app.MapGet("/api/enrollments", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/enrollments aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("students.view") || await a.Can("grades.view") || await a.Can("attendance.reports"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            return Results.Ok(await db.Enrollments.Where(e => all || e.StudentId == a.User.StudentId).Select(e => new { e.Id, e.StudentId, e.ClassId, e.YearId, e.CurriculumId, e.Attempt, e.Status, studentName = e.Student.FirstName + " " + e.Student.LastName, e.Student.Number, className = e.Class.Name, yearCode = e.Year.Code, e.Curriculum.Track, e.Curriculum.Level, e.Curriculum.Speciality }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // L'étudiant conserve l'accès à chacune de ses inscriptions historiques.
        app.MapPost("/api/enrollments/join", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/enrollments/join aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            if (a.User.StudentId == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Compte étudiant requis."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var hash = Security.Hash(j.Required("code")); // Calcule ou prépare la variable hash, utilisée par les instructions suivantes.
            var c = await db.Classes.Include(c => c.Year).SingleOrDefaultAsync(c => c.JoinHash == hash && c.JoinExpires > PortalClock.Now) ?? throw new RuleException("Code invalide ou expiré."); // Charge depuis la base la variable c, utilisée par les instructions suivantes.
            var student = await db.Students.FindAsync(a.User.StudentId) ?? throw new RuleException("Dossier absent."); // Charge depuis la base la variable student, utilisée par les instructions suivantes.
            var e = await Enroll(db, student, c, j.Id("curriculumId")); // Calcule ou prépare la variable e, utilisée par les instructions suivantes.
            a.Audit("CLASS_JOINED", e.Id, new { e.ClassId, e.Attempt }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { e.Id, e.Attempt }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // L'identité est issue de la session, jamais d'un nom choisi.
        app.MapPost("/api/enrollments", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/enrollments aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.manage"); // Exige la permission students.manage contenue dans les rôles actuels du compte.
            var s = await db.Students.FindAsync(j.Id("studentId")) ?? throw new RuleException("Étudiant inconnu."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            var c = await db.Classes.Include(c => c.Year).SingleAsync(c => c.Id == j.Id("classId")); // Charge depuis la base la variable c, utilisée par les instructions suivantes.
            var e = await Enroll(db, s, c, j.Id("curriculumId"), j["historical"]?.GetValue<bool>() == true); // Calcule ou prépare la variable e, utilisée par les instructions suivantes.
            a.Audit("STUDENT_ENROLLED", e.Id, new { e.StudentId, e.ClassId }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { e.Id, e.Attempt }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Permet également une inscription administrative contrôlée.
        app.MapPost("/api/enrollments/{id:guid}/transfer", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/enrollments/{id:guid}/transfer aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("students.transfer"); // Exige la permission students.transfer contenue dans les rôles actuels du compte.
            await using var tx = await db.Database.BeginTransactionAsync(); // Ouvre une transaction SQL afin qu’une erreur annule toute l’opération.
            var old = await db.Enrollment(id); // Charge depuis la base la variable old, utilisée par les instructions suivantes.
            var c = await db.Classes.Include(c => c.Year).SingleOrDefaultAsync(c => c.Id == j.Id("classId")) ?? throw new RuleException("Classe inconnue."); // Charge depuis la base la variable c, utilisée par les instructions suivantes.
            if (old.Status != "ACTIVE" || c.YearId != old.YearId || c.Level != old.Class.Level || c.Speciality != old.Class.Speciality) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Transfert limité à une classe du même niveau, spécialité et année."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            old.Status = "TRANSFERRED"; // Met à jour état courant du dossier ou de l’opération de l’objet old.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            var next = await Enroll(db, old.Student, c, old.CurriculumId); // Calcule ou prépare la variable next, utilisée par les instructions suivantes.
            next.Attempt = old.Attempt; // Met à jour numéro de tentative au même niveau de l’objet next.
            foreach (var grade in await db.Grades.Where(g => g.EnrollmentId == id).ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                db.Grades.Add(new() { EnrollmentId = next.Id, EcId = grade.EcId, Semester = grade.Semester, Value = grade.Value }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            foreach (var rescue in await db.Rescues.Where(r => r.EnrollmentId == id).ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                db.Rescues.Add(new() { EnrollmentId = next.Id, UeId = rescue.UeId, Reason = rescue.Reason, Actor = rescue.Actor }); // Le transfert conserve aussi les décisions de jury.
            var official = await db.Snapshots.Where(s => s.EnrollmentId == id).ToListAsync(); // Recherche les relevés déjà publiés avant le transfert.
            foreach (var group in official.GroupBy(s => s.Semester)) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var previous = group.OrderByDescending(s => s.Version).First(); // Calcule ou prépare la variable previous, utilisée par les instructions suivantes.
                var result = JsonSerializer.Deserialize<ResultView>(previous.Content)!with // Calcule ou prépare la variable result, utilisée par les instructions suivantes.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    EnrollmentId = next.Id // Renseigne identifiant de l’inscription annuelle avec la valeur calculée ou validée à droite.
                }; // Termine cette construction et revient à l’instruction qui l’utilise.
                var content = JsonSerializer.Serialize(result); // Calcule ou prépare la variable content, utilisée par les instructions suivantes.
                db.Snapshots.Add(new() { EnrollmentId = next.Id, Semester = result.Semester, Version = 1, Header = previous.Header, Content = content, Hash = Security.Hash(content) }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            } // Le nouvel emplacement scolaire conserve les résultats officiels sans supprimer leurs sources.

            a.Audit("STUDENT_TRANSFERRED", id, new { from = old.ClassId, to = c.Id, reason = j.Required("reason") }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            await tx.CommitAsync(); // Valide la transaction pour rendre tous ses changements persistants ensemble.
            return Results.Ok(new { next.Id }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Préserve l'ancienne inscription et reprend les notes dans une inscription active distincte.
        app.MapGet("/api/classes/{id:guid}/roster", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP GET /api/classes/{id:guid}/roster aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            if (!await a.Can("students.manage") && !await a.Can("attendance.manage") && !await a.Officer(id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Liste de classe réservée aux responsables.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            return Results.Ok(await db.Enrollments.Where(e => e.ClassId == id && e.Status == "ACTIVE").Select(e => new { e.StudentId, e.Student.Number, name = e.Student.FirstName + " " + e.Student.LastName }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le responsable voit les identités de sa salle sans les contacts personnels.
        app.MapPost("/api/officers", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/officers aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            var classId = j.Id("classId"); // Calcule ou prépare la variable classId, utilisée par les instructions suivantes.
            var studentId = j.Id("studentId"); // Calcule ou prépare la variable studentId, utilisée par les instructions suivantes.
            var position = j.Required("position"); // Calcule ou prépare la variable position, utilisée par les instructions suivantes.
            if (!new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "DELEGATE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "VICE_DELEGATE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "SECRETARY" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            }.Contains(position) || !await db.Enrollments.AnyAsync(e => e.ClassId == classId && e.StudentId == studentId && e.Status == "ACTIVE")) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                throw new RuleException("Mandat ou étudiant invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var o = await db.Officers.SingleOrDefaultAsync(o => o.ClassId == classId && o.Position == position); // Charge depuis la base la variable o, utilisée par les instructions suivantes.
            if (o == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                o = new() // Renseigne le champ o avec la valeur calculée ou validée à droite.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    ClassId = classId, // Renseigne identifiant de la classe physique avec la valeur calculée ou validée à droite.
                    Position = position // Renseigne fonction de responsable exercée dans la classe avec la valeur calculée ou validée à droite.
                }; // Termine cette construction et revient à l’instruction qui l’utilise.
                db.Officers.Add(o); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            } // Ferme ce bloc après l’exécution de ses instructions.

            o.StudentId = studentId; // Met à jour identifiant du dossier étudiant de l’objet o.
            a.Audit("OFFICER_ASSIGNED", o.Id, new { classId, studentId, position }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(o); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les responsables sont choisis parmi les membres actifs de la salle.
        app.MapGet("/api/officers", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/officers aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("academic.manage"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            var ids = await db.Enrollments.Where(e => e.StudentId == a.User.StudentId && e.Status == "ACTIVE").Select(e => e.ClassId).ToListAsync(); // Charge depuis la base la variable ids, utilisée par les instructions suivantes.
            return Results.Ok(await db.Officers.Where(o => all || ids.Contains(o.ClassId)).Select(o => new { o.Id, o.ClassId, o.StudentId, o.Position }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les mandats de sa classe sont visibles par les étudiants.
    } // Ferme ce bloc après l’exécution de ses instructions.

    static async Task<Enrollment> Enroll(PortalDb db, Student s, SchoolClass c, Guid curriculumId, bool historical = false) // Applique la même règle pour une inscription autonome ou administrative.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        if (s.Status != "ACTIVE" && s.Status != "PRE_REGISTERED") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Dossier étudiant inactif."); // Un dossier suspendu ne rejoint pas de nouvelle salle.
        if (historical && await db.Enrollments.AnyAsync(e => e.StudentId == s.Id && e.YearId == c.YearId && e.Status != "TRANSFERRED")) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Inscription historique déjà enregistrée."); // Empêche de dupliquer une même année lors des migrations.
        if ((!historical && c.Year.Status != "OPEN" || historical && c.Year.Status != "CLOSED") || await db.Enrollments.AnyAsync(e => e.StudentId == s.Id && e.YearId == c.YearId && e.Status == "ACTIVE")) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Année fermée ou inscription déjà active."); // Empêche les doubles inscriptions actives.
        var curriculum = await db.Curricula.FindAsync(curriculumId) ?? throw new RuleException("Programme inconnu."); // L'administration choisit explicitement la version pertinente.
        if (curriculum.Level != c.Level || curriculum.Speciality != c.Speciality || curriculum.Track != s.Track || !JsonSerializer.Deserialize<string[]>(c.Tracks)!.Contains(s.Track)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Programme incompatible avec la classe ou le parcours."); // Mélanger ITI et LPI dans une salle ne mélange pas leurs coefficients.
        var attempts = await db.Enrollments.Include(e => e.Curriculum).Where(e => e.StudentId == s.Id && e.Curriculum.Level == curriculum.Level && e.Year.Start < c.Year.Start).Select(e => e.YearId).Distinct().CountAsync(); // Compte les années antérieures du même niveau, et non les transferts.
        var e = new Enrollment // Calcule ou prépare la variable e, utilisée par les instructions suivantes.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            StudentId = s.Id, // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
            YearId = c.YearId, // Renseigne identifiant de l’année académique avec la valeur calculée ou validée à droite.
            ClassId = c.Id, // Renseigne identifiant de la classe physique avec la valeur calculée ou validée à droite.
            CurriculumId = curriculumId, // Renseigne identifiant de la version du programme avec la valeur calculée ou validée à droite.
            Attempt = attempts + 1, // Renseigne numéro de tentative au même niveau avec la valeur calculée ou validée à droite.
            Status = historical ? "COMPLETED" : "ACTIVE", // Renseigne état courant du dossier ou de l’opération avec la valeur calculée ou validée à droite.
            Created = historical ? new DateTimeOffset(c.Year.Start.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero) : PortalClock.Now // Renseigne date UTC de création ou de publication avec la valeur calculée ou validée à droite.
        }; // Le numéro de tentative rend le redoublement explicite.
        db.Enrollments.Add(e); // Les résultats historiques restent attachés à leurs anciennes inscriptions.
        return e; // Le contrôleur écrit ensuite l'audit et valide la transaction.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static void ValidateCurriculum(JsonObject j) // Contrôle les coefficients et identifiants des versions ajoutées.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        if (!new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            "SEMESTER_EQUAL_WEIGHT", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            "ALL_EC_WEIGHTED" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        }.Contains(j.Required("annualMethod"))) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            throw new RuleException("Méthode annuelle invalide."); // Évite une configuration silencieusement ignorée.
        if (j.Decimal("threshold") < 0 || j.Decimal("threshold") > 20) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Seuil invalide."); // Le seuil doit appartenir à l'échelle des notes.
        var semesters = j["semesters"]?.AsArray() ?? throw new RuleException("Semestres requis."); // Un programme ne peut pas être vide.
        if (semesters.Any(s => (int)s!["number"]! <= 0 || s["ues"]!.AsArray().Count == 0)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Un semestre doit avoir un numéro positif et au moins une UE."); // Une structure vide ne constitue pas un programme publiable.
        if (semesters.Count != 2 || semesters.Select(s => (int)s!["number"]!).Distinct().Count() != 2) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Deux semestres distincts requis."); // Les résultats annuels reposent sur deux semestres.
        if (j.Text("rankingScope") != "" && !new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            "YEAR_LEVEL", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            "YEAR_LEVEL_TRACK_SPECIALITY" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        }.Contains(j.Text("rankingScope"))) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            throw new RuleException("Regroupement de classement invalide."); // Le groupe de comparaison doit être une règle connue.
        if (j.Text("tieMethod") != "" && !new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            "COMPETITION", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            "DENSE" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        }.Contains(j.Text("tieMethod"))) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            throw new RuleException("Règle d’ex æquo invalide."); // Rend explicite le choix entre 1,2,2,4 et 1,2,2,3.
        var ids = new HashSet<Guid>(); // Détecte les collisions entre UE et EC de la nouvelle version.
        foreach (var s in semesters) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            foreach (var u in s!["ues"]!.AsArray()) // Parcourt toute la structure à valider.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                if (!Guid.TryParse((string? )u!["id"], out var id) || !ids.Add(id) || u["ecs"]!.AsArray().Count == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("UE vide ou UUID dupliqué."); // Un code imprimé répété reste autorisé, mais pas un UUID répété.
                if (j.Text("status") == "APPROVED" && (u["credits"] == null || (decimal)u["credits"]! < 0)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Crédits nécessaires avant approbation."); // Les fiches ITI incomplètes ne peuvent pas être publiées sans confirmation.
                foreach (var e in u["ecs"]!.AsArray()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                    if (!Guid.TryParse((string? )e!["id"], out var ec) || !ids.Add(ec) || (decimal)e["coefficient"]! <= 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                        throw new RuleException("EC ou coefficient invalide."); // Le calcul ne peut jamais diviser par un poids nul.
            } // Ferme ce bloc après l’exécution de ses instructions.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
