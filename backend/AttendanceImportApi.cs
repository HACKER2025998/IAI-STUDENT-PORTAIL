using System.Globalization; // Parse le CSV avec le même séparateur que les modèles de notes.
using System.Text; // Encode le modèle téléchargeable en UTF-8.
using System.Text.Json.Nodes; // Lit le contenu CSV et le choix de confirmation.
using CsvHelper; // Gère les valeurs entourées de guillemets et les lignes mal formées.
using Microsoft.EntityFrameworkCore; // Écrit le lot de pointages dans une transaction unique.

namespace Iai; // Module d'import administratif des présences.
public static class AttendanceImportApi // L'import manuel conserve toujours son origine dans le journal.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Installe le modèle et le contrôle des présences importées.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapGet("/api/attendance/sessions/{id:guid}/template", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP GET /api/attendance/sessions/{id:guid}/template aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("attendance.manage"); // Exige la permission attendance.manage contenue dans les rôles actuels du compte.
            var s = await db.Sessions.FindAsync(id) ?? throw new RuleException("Séance inconnue."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            var students = await db.Enrollments.Include(e => e.Student).Where(e => e.ClassId == s.ClassId).Select(e => e.Student).Distinct().ToListAsync(); // Charge depuis la base la variable students, utilisée par les instructions suivantes.
            using var output = new StringWriter(); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            using var csv = new CsvWriter(output, CultureInfo.InvariantCulture); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            foreach (var header in new[] // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "student_code", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "status" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            } // Ferme ce bloc après l’exécution de ses instructions.

            ) // Termine cette construction et revient à l’instruction qui l’utilise.
                csv.WriteField(header); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            csv.NextRecord(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            foreach (var student in students) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                csv.WriteField(student.Number); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                csv.WriteField(""); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                csv.NextRecord(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            } // Ferme ce bloc après l’exécution de ses instructions.

            return Results.File(Encoding.UTF8.GetBytes(output.ToString()), "text/csv", "presences.csv"); // Renvoie les octets ou le fichier autorisé avec son type de contenu et son nom de téléchargement.
        }); // Les statuts attendus sont PRESENT, ABSENT, LATE ou EXCUSED.
        app.MapPost("/api/attendance/sessions/{id:guid}/import", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/attendance/sessions/{id:guid}/import aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Un compte connecté est requis même pour la prévisualisation.
            await a.Require("attendance.manage"); // Les responsables étudiants ne peuvent pas importer des pointages arbitraires.
            var session = await db.Sessions.FindAsync(id) ?? throw new RuleException("Séance inconnue."); // Identifie précisément le cours concerné.
            var students = await db.Enrollments.Include(e => e.Student).Where(e => e.ClassId == session.ClassId).Select(e => e.Student).Distinct().ToListAsync(); // La résolution du matricule reste limitée à cette classe.
            var text = j.Required("csv"); // Limite la taille et refuse un contenu vide.
            var rows = new List<(Guid StudentId, string Status)>(); // Prépare les lignes valides sans écrire de présence.
            var errors = new List<string>(); // Toutes les erreurs sont regroupées pour la correction du fichier.
            var seen = new HashSet<Guid>(); // Détecte les matricules répétés dans le lot.
            using var input = new StringReader(text); // Le CSV est lu depuis la mémoire de la requête.
            using var csv = new CsvReader(input, CultureInfo.InvariantCulture); // Le modèle utilise la virgule comme séparateur.
            try // Les erreurs de syntaxe du CSV deviennent des messages métier.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                csv.Read(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                csv.ReadHeader(); // La première ligne définit les noms des colonnes.
                if (csv.HeaderRecord == null || !csv.HeaderRecord.Contains("student_code") || !csv.HeaderRecord.Contains("status") || csv.HeaderRecord.Distinct().Count() != csv.HeaderRecord.Length) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Utilisez le modèle student_code,status."); // Refuse les colonnes manquantes ou répétées.
                while (csv.Read()) // Valide les lignes avant toute écriture.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    var student = students.SingleOrDefault(s => s.Number == csv.GetField("student_code")?.Trim()); // Un nom ressemblant n'est pas une preuve d'identité.
                    var status = csv.GetField("status")?.Trim() ?? ""; // Les statuts sont contrôlés explicitement.
                    if (student == null || !seen.Add(student.Id) || !new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                        "PRESENT", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                        "ABSENT", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                        "LATE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                        "EXCUSED" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                    }.Contains(status)) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                        errors.Add($"Ligne {csv.Parser.Row} : matricule inconnu ou dupliqué, ou statut invalide."); // Explique précisément la ligne à corriger.
                    else // Exécute le cas alternatif lorsque la condition précédente est fausse.
                        rows.Add((student.Id, status)); // Conserve uniquement les données entièrement reconnues.
                } // Ferme ce bloc après l’exécution de ses instructions.
            } // Ferme ce bloc après l’exécution de ses instructions.
            catch (CsvHelperException) // Intercepte le type d’erreur indiqué et applique la réponse prévue pour ce cas.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                throw new RuleException("CSV mal formé."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            } // Aucun détail technique du parseur n'est exposé.

            if (j["confirm"]?.GetValue<bool>() != true) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                return Results.Ok(new { valid = rows.Count, errors }); // Le premier appel renvoie uniquement le contrôle du fichier.
            if (errors.Count > 0 || rows.Count == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Corrigez les erreurs avant confirmation."); // La confirmation revalide le fichier au lieu de faire confiance au navigateur.
            var reason = j.Required("reason"); // L'origine de l'import doit être documentée.
            await using var transaction = await db.Database.BeginTransactionAsync(); // Le lot est atomique.
            foreach (var row in rows) // Met à jour ou crée chaque pointage validé.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var record = await db.Attendances.SingleOrDefaultAsync(r => r.SessionId == id && r.StudentId == row.StudentId); // Retrouve l'éventuel pointage existant.
                var previous = record?.Status; // Garde la valeur avant correction pour l'audit.
                if (record == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    record = new() // Renseigne le champ record avec la valeur calculée ou validée à droite.
                    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                        SessionId = id, // Renseigne identifiant du cours ouvert pour la présence avec la valeur calculée ou validée à droite.
                        StudentId = row.StudentId // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
                    }; // Termine cette construction et revient à l’instruction qui l’utilise.
                    db.Attendances.Add(record); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
                } // Prépare le pointage manquant.

                record.Status = row.Status; // Met à jour état courant du dossier ou de l’opération de l’objet record.
                record.Method = "ADMIN_IMPORT"; // Marque explicitement l'origine administrative.
                a.Audit("ATTENDANCE_IMPORTED", record.Id, new { previous, current = row.Status, reason, fileHash = Security.Hash(text) }); // Journalise les changements sans stocker le fichier dans le journal.
            } // Ferme ce bloc après l’exécution de ses instructions.

            await db.SaveChangesAsync(); // Applique les contraintes SQL de présence unique.
            await transaction.CommitAsync(); // Rend le lot entier visible seulement après réussite.
            return Results.Ok(new { imported = rows.Count }); // Retourne le nombre de pointages effectivement traités.
        }); // Termine la définition de l’objet ou du gestionnaire de route.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
