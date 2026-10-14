using System.Globalization; // Parse les nombres avec une convention stable pour les CSV.
using System.Text; // Produit les fichiers téléchargeables en UTF-8.
using System.Text.Json; // Fige les résultats dans des instantanés.
using System.Text.Json.Nodes; // Lit les structures de programme.
using System.Net; // Échappe le texte des relevés imprimables.
using CsvHelper; // Gère correctement les virgules, guillemets et retours à la ligne dans le CSV.
using Microsoft.EntityFrameworkCore; // Exécute les opérations de notes en transaction.

namespace Iai; // Module Notes.
public record ImportRow(Guid EnrollmentId, Dictionary<Guid, decimal> Notes); // Une ligne valide référence une inscription et ses EC par UUID.
public static class GradesApi // Garde l'import, la confirmation et la publication séparés.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Enregistre les endpoints de notes.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapGet("/api/classes/{id:guid}/grades/template", async (Guid id, int semester, Guid curriculumId, PortalDb db, Access a) => // Associe la requête HTTP GET /api/classes/{id:guid}/grades/template aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.import"); // Exige la permission grades.import contenue dans les rôles actuels du compte.
            var c = await db.Classes.FindAsync(id) ?? throw new RuleException("Classe inconnue."); // Charge depuis la base la variable c, utilisée par les instructions suivantes.
            var curriculum = await db.Curricula.FindAsync(curriculumId) ?? throw new RuleException("Programme inconnu."); // Charge depuis la base la variable curriculum, utilisée par les instructions suivantes.
            if (c.Level != curriculum.Level || c.Speciality != curriculum.Speciality) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Programme incompatible."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var headers = Headers(curriculum, semester); // Calcule ou prépare la variable headers, utilisée par les instructions suivantes.
            using var writer = new StringWriter(); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            csv.WriteField("student_code"); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            csv.WriteField("student_name"); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            foreach (var h in headers.Keys) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                csv.WriteField(h); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            csv.NextRecord(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            foreach (var e in await db.Enrollments.Include(e => e.Student).Where(e => e.ClassId == id && e.CurriculumId == curriculumId && e.Status != "TRANSFERRED").ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                csv.WriteField(e.Student.Number); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                csv.WriteField(e.Student.FirstName + " " + e.Student.LastName); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                foreach (var h in headers) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                    csv.WriteField(""); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                csv.NextRecord(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
            } // Ferme ce bloc après l’exécution de ses instructions.

            return Results.File(Encoding.UTF8.GetBytes(writer.ToString()), "text/csv", $"notes-S{semester}.csv"); // Renvoie les octets ou le fichier autorisé avec son type de contenu et son nom de téléchargement.
        }); // Le modèle est propre au parcours pour éviter de mélanger ITI et LPI.
        app.MapPost("/api/classes/{id:guid}/grades/import", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/classes/{id:guid}/grades/import aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.import"); // Exige la permission grades.import contenue dans les rôles actuels du compte.
            var semester = j.Int("semester"); // Calcule ou prépare la variable semester, utilisée par les instructions suivantes.
            var curriculumId = j.Id("curriculumId"); // Calcule ou prépare la variable curriculumId, utilisée par les instructions suivantes.
            var curriculum = await db.Curricula.FindAsync(curriculumId) ?? throw new RuleException("Programme inconnu."); // Charge depuis la base la variable curriculum, utilisée par les instructions suivantes.
            var headers = Headers(curriculum, semester); // Calcule ou prépare la variable headers, utilisée par les instructions suivantes.
            var enrollments = await db.Enrollments.Include(e => e.Student).Where(e => e.ClassId == id && e.CurriculumId == curriculumId && e.Status != "TRANSFERRED").ToListAsync(); // Charge depuis la base la variable enrollments, utilisée par les instructions suivantes.
            if (enrollments.Count == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Aucune inscription pour ce programme."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var text = j.Required("csv"); // Calcule ou prépare la variable text, utilisée par les instructions suivantes.
            var rows = new List<ImportRow>(); // Calcule ou prépare la variable rows, utilisée par les instructions suivantes.
            var errors = new List<string>(); // Calcule ou prépare la variable errors, utilisée par les instructions suivantes.
            var seen = new HashSet<Guid>(); // Calcule ou prépare la variable seen, utilisée par les instructions suivantes.
            using var reader = new StringReader(text); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            try // Encadre les opérations qui peuvent échouer afin de traiter leurs erreurs proprement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                csv.Read(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                csv.ReadHeader(); // Effectue cette étape de lecture ou d’écriture CSV sur la ligne courante.
                if (csv.HeaderRecord == null || !csv.HeaderRecord.Contains("student_code") || !csv.HeaderRecord.Contains("student_name") || headers.Keys.Any(h => !csv.HeaderRecord.Contains(h)) || csv.HeaderRecord.Distinct().Count() != csv.HeaderRecord.Length) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("En-têtes incompatibles : utilisez le modèle téléchargé."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
                while (csv.Read()) // Répète le traitement tant que la condition reste vraie, notamment pendant la lecture du CSV.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    var line = csv.Parser.Row; // Calcule ou prépare la variable line, utilisée par les instructions suivantes.
                    var code = csv.GetField("student_code")?.Trim() ?? ""; // Calcule ou prépare la variable code, utilisée par les instructions suivantes.
                    var name = csv.GetField("student_name")?.Trim() ?? ""; // Calcule ou prépare la variable name, utilisée par les instructions suivantes.
                    var candidates = enrollments.Where(e => code != "" ? e.Student.Number == code : e.Student.FirstName + " " + e.Student.LastName == name).ToList(); // Calcule ou prépare la variable candidates, utilisée par les instructions suivantes.
                    if (candidates.Count != 1) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                        errors.Add($"Ligne {line} : étudiant inconnu ou ambigu."); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                        continue; // Passe au prochain élément après avoir traité ce cas de ligne.
                    } // Ferme ce bloc après l’exécution de ses instructions.

                    var e = candidates[0]; // Calcule ou prépare la variable e, utilisée par les instructions suivantes.
                    if (!seen.Add(e.Id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                        errors.Add($"Ligne {line} : étudiant dupliqué."); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                        continue; // Passe au prochain élément après avoir traité ce cas de ligne.
                    } // Ferme ce bloc après l’exécution de ses instructions.

                    var notes = new Dictionary<Guid, decimal>(); // Calcule ou prépare la variable notes, utilisée par les instructions suivantes.
                    foreach (var h in headers) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                        var value = csv.GetField(h.Key); // Calcule ou prépare la variable value, utilisée par les instructions suivantes.
                        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var n) || n < 0 || n > 20) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                            errors.Add($"Ligne {line}, {h.Key} : note vide ou hors de 0 à 20."); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                        else // Exécute le cas alternatif lorsque la condition précédente est fausse.
                            notes.Add(h.Value, n); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                    } // Ferme ce bloc après l’exécution de ses instructions.

                    if (notes.Count == headers.Count) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                        rows.Add(new(e.Id, notes)); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                } // Ferme ce bloc après l’exécution de ses instructions.
            } // Ferme ce bloc après l’exécution de ses instructions.
            catch (CsvHelperException) // Intercepte le type d’erreur indiqué et applique la réponse prévue pour ce cas.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                throw new RuleException("CSV mal formé. Utilisez le séparateur virgule du modèle."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            } // Ferme ce bloc après l’exécution de ses instructions.

            var batch = new ImportBatch // Calcule ou prépare la variable batch, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                ClassId = id, // Renseigne identifiant de la classe physique avec la valeur calculée ou validée à droite.
                Semester = semester, // Renseigne numéro de semestre concerné avec la valeur calculée ou validée à droite.
                Rows = JsonSerializer.Serialize(rows), // Renseigne lignes contrôlées de l’import en JSON avec la valeur calculée ou validée à droite.
                Errors = JsonSerializer.Serialize(errors), // Renseigne erreurs détectées avant confirmation de l’import avec la valeur calculée ou validée à droite.
                Hash = Security.Hash(text) // Renseigne empreinte SHA-256 du contenu conservé avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Imports.Add(batch); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("GRADE_IMPORT_PREVIEW", batch.Id, new { valid = rows.Count, errors = errors.Count }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { batch.Id, valid = rows.Count, errors }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Toute erreur bloque la confirmation ; aucune ligne n'est importée en silence.
        app.MapPost("/api/grade-imports/{id:guid}/confirm", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/grade-imports/{id:guid}/confirm aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.import"); // Exige la permission grades.import contenue dans les rôles actuels du compte.
            var batch = await db.Imports.FindAsync(id) ?? throw new RuleException("Import inconnu."); // Charge depuis la base la variable batch, utilisée par les instructions suivantes.
            if (batch.Status != "PREVIEW" || JsonSerializer.Deserialize<string[]>(batch.Errors)!.Length > 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Corrigez les erreurs avant confirmation."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            if (await db.Publications.AnyAsync(p => p.ClassId == batch.ClassId && p.Semester == batch.Semester)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Résultats déjà publiés : utilisez la rectification individuelle."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            await using var tx = await db.Database.BeginTransactionAsync(); // Ouvre une transaction SQL afin qu’une erreur annule toute l’opération.
            foreach (var row in JsonSerializer.Deserialize<List<ImportRow>>(batch.Rows)!) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var e = await db.Enrollment(row.EnrollmentId); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
                if (e.Status == "TRANSFERRED" || e.ClassId != batch.ClassId) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Inscription modifiée depuis la prévisualisation."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
                foreach (var note in row.Notes) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    var grade = await db.Grades.SingleOrDefaultAsync(g => g.EnrollmentId == row.EnrollmentId && g.EcId == note.Key); // Charge depuis la base la variable grade, utilisée par les instructions suivantes.
                    decimal? previous = grade?.Value; // Une première saisie a une valeur précédente absente, pas une note zéro.
                    if (grade == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                        grade = new() // Renseigne le champ grade avec la valeur calculée ou validée à droite.
                        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                            EnrollmentId = row.EnrollmentId, // Renseigne identifiant de l’inscription annuelle avec la valeur calculée ou validée à droite.
                            EcId = note.Key, // Renseigne identifiant de la matière avec la valeur calculée ou validée à droite.
                            Semester = batch.Semester // Renseigne numéro de semestre concerné avec la valeur calculée ou validée à droite.
                        }; // Termine cette construction et revient à l’instruction qui l’utilise.
                        db.Grades.Add(grade); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
                    } // Ferme ce bloc après l’exécution de ses instructions.

                    grade.Value = note.Value; // Met à jour valeur décimale de la note sur vingt de l’objet grade.
                    a.Audit("GRADE_IMPORTED", grade.Id, new { before = previous, after = note.Value, batch = batch.Id, row.EnrollmentId, ec = note.Key }); // Chaque modification de note conserve ses deux valeurs dans le journal.
                } // Ferme ce bloc après l’exécution de ses instructions.
            } // Ferme ce bloc après l’exécution de ses instructions.

            batch.Status = "IMPORTED"; // Met à jour état courant du dossier ou de l’opération de l’objet batch.
            a.Audit("GRADE_IMPORT_CONFIRMED", id, new { batch.Hash }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            await tx.CommitAsync(); // Valide la transaction pour rendre tous ses changements persistants ensemble.
            return Results.Ok(new { batch.Status }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le lot entier est confirmé atomiquement et seulement une fois.
        app.MapGet("/api/enrollments/{id:guid}/results", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP GET /api/enrollments/{id:guid}/results aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var e = await db.Enrollment(id); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
            var manager = await a.Can("grades.view"); // Calcule ou prépare la variable manager, utilisée par les instructions suivantes.
            if (!manager && e.StudentId != a.User.StudentId) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Relevé privé.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            if (manager) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                return Results.Ok(await AllResults(db, e)); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
            var snapshots = await db.Snapshots.Where(s => s.EnrollmentId == id).ToListAsync(); // Charge depuis la base la variable snapshots, utilisée par les instructions suivantes.
            return Results.Ok(snapshots.GroupBy(s => s.Semester).Select(g => JsonNode.Parse(g.OrderByDescending(s => s.Version).First().Content))); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Un étudiant ne voit que ses propres instantanés officiellement publiés.
        app.MapPost("/api/enrollments/{id:guid}/grades", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/enrollments/{id:guid}/grades aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.update"); // Exige la permission grades.update contenue dans les rôles actuels du compte.
            await using var transaction = await db.Database.BeginTransactionAsync(); // La note, son journal et son nouvel instantané sont validés ensemble.
            var e = await db.Enrollment(id); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
            var semester = j.Int("semester"); // Calcule ou prépare la variable semester, utilisée par les instructions suivantes.
            var ec = j.Id("ecId"); // Calcule ou prépare la variable ec, utilisée par les instructions suivantes.
            if (!Headers(e.Curriculum, semester).Values.Contains(ec)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("EC absent du programme."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var value = j.Decimal("value"); // Calcule ou prépare la variable value, utilisée par les instructions suivantes.
            if (value < 0 || value > 20) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Note entre 0 et 20 requise."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var p = await db.Publications.SingleOrDefaultAsync(p => p.ClassId == e.ClassId && p.Semester == semester); // Charge depuis la base la variable p, utilisée par les instructions suivantes.
            if (p?.Status == "LOCKED") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Résultats verrouillés."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var reason = j.Required("reason"); // Calcule ou prépare la variable reason, utilisée par les instructions suivantes.
            var g = await db.Grades.SingleOrDefaultAsync(g => g.EnrollmentId == id && g.EcId == ec); // Charge depuis la base la variable g, utilisée par les instructions suivantes.
            decimal? before = g?.Value; // Calcule ou prépare la variable before, utilisée par les instructions suivantes.
            if (g == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                g = new() // Renseigne le champ g avec la valeur calculée ou validée à droite.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    EnrollmentId = id, // Renseigne identifiant de l’inscription annuelle avec la valeur calculée ou validée à droite.
                    EcId = ec, // Renseigne identifiant de la matière avec la valeur calculée ou validée à droite.
                    Semester = semester // Renseigne numéro de semestre concerné avec la valeur calculée ou validée à droite.
                }; // Termine cette construction et revient à l’instruction qui l’utilise.
                db.Grades.Add(g); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            } // Ferme ce bloc après l’exécution de ses instructions.

            g.Value = value; // Met à jour valeur décimale de la note sur vingt de l’objet g.
            a.Audit("GRADE_RECTIFIED", g.Id, new { before, after = value, reason }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            if (p != null || await db.Snapshots.AnyAsync(s => s.EnrollmentId == e.Id && s.Semester == semester)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                await Snapshot(db, e, semester); // Ajoute une nouvelle version officielle du relevé sans supprimer les versions antérieures.
            await transaction.CommitAsync(); // Rend la rectification atomique avant la réponse.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Une rectification publiée produit une nouvelle version du relevé.
        app.MapPost("/api/enrollments/{id:guid}/rescue", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/enrollments/{id:guid}/rescue aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.rescue"); // Exige la permission grades.rescue contenue dans les rôles actuels du compte.
            await using var transaction = await db.Database.BeginTransactionAsync(); // La note, son journal et son nouvel instantané sont validés ensemble.
            var e = await db.Enrollment(id); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
            var semester = j.Int("semester"); // Calcule ou prépare la variable semester, utilisée par les instructions suivantes.
            var result = await Result(db, e, semester); // Calcule ou prépare la variable result, utilisée par les instructions suivantes.
            var ue = result.Ues.SingleOrDefault(u => u.Id == j.Id("ueId")) ?? throw new RuleException("UE inconnue."); // Calcule ou prépare la variable ue, utilisée par les instructions suivantes.
            if (ue.Status != "NOT_VALIDATED") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Seule une UE complète non validée peut être repêchée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var p = await db.Publications.SingleOrDefaultAsync(p => p.ClassId == e.ClassId && p.Semester == semester); // Charge depuis la base la variable p, utilisée par les instructions suivantes.
            if (p?.Status == "LOCKED") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Résultats verrouillés."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var r = new Rescue // Calcule ou prépare la variable r, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                EnrollmentId = id, // Renseigne identifiant de l’inscription annuelle avec la valeur calculée ou validée à droite.
                UeId = ue.Id, // Renseigne identifiant de l’unité d’enseignement avec la valeur calculée ou validée à droite.
                Reason = j.Required("reason"), // Renseigne motif explicite de la décision avec la valeur calculée ou validée à droite.
                Actor = a.User.Id // Renseigne UUID du compte auteur de la décision avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Rescues.Add(r); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("UE_RESCUED", r.Id, new { ue.Average, ue.Status, after = "VALIDATED_BY_RESCUE", r.Reason }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            if (p != null || await db.Snapshots.AnyAsync(s => s.EnrollmentId == e.Id && s.Semester == semester)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                await Snapshot(db, e, semester); // Ajoute une nouvelle version officielle du relevé sans supprimer les versions antérieures.
            await transaction.CommitAsync(); // Rend la rectification atomique avant la réponse.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // La moyenne de 9 reste 9, le crédit est accordé par décision.
        app.MapPost("/api/classes/{id:guid}/publish", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/classes/{id:guid}/publish aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.publish"); // Exige la permission grades.publish contenue dans les rôles actuels du compte.
            var semester = j.Int("semester"); // Calcule ou prépare la variable semester, utilisée par les instructions suivantes.
            var entries = await db.Enrollments.Include(e => e.Curriculum).Include(e => e.Student).Include(e => e.Class).Include(e => e.Year).Where(e => e.ClassId == id && e.Status != "TRANSFERRED").ToListAsync(); // Charge depuis la base la variable entries, utilisée par les instructions suivantes.
            if (entries.Count == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Classe vide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            if (await db.Publications.AnyAsync(p => p.ClassId == id && p.Semester == semester)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Semestre déjà publié."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            foreach (var e in entries) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                if (e.Curriculum.Status != "APPROVED" || !(await Result(db, e, semester)).Complete) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Programme non approuvé ou notes incomplètes pour " + e.Student.Number); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            await using var tx = await db.Database.BeginTransactionAsync(); // Ouvre une transaction SQL afin qu’une erreur annule toute l’opération.
            db.Publications.Add(new() { ClassId = id, Semester = semester }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            foreach (var e in entries) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                await Snapshot(db, e, semester); // Ajoute une nouvelle version officielle du relevé sans supprimer les versions antérieures.
                db.Notifications.Add(new() { StudentId = e.StudentId, Message = $"Vos résultats du semestre {semester} sont publiés." }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            } // Ferme ce bloc après l’exécution de ses instructions.

            a.Audit("RESULTS_PUBLISHED", id, new { semester, count = entries.Count }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            await tx.CommitAsync(); // Valide la transaction pour rendre tous ses changements persistants ensemble.
            return Results.Ok(new { published = entries.Count }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les données manquantes ITI bloquent la publication, pas la préparation des notes.
        app.MapPost("/api/classes/{id:guid}/lock", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/classes/{id:guid}/lock aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("grades.publish"); // Exige la permission grades.publish contenue dans les rôles actuels du compte.
            var p = await db.Publications.SingleOrDefaultAsync(p => p.ClassId == id && p.Semester == j.Int("semester")) ?? throw new RuleException("Publication absente."); // Charge depuis la base la variable p, utilisée par les instructions suivantes.
            p.Status = "LOCKED"; // Met à jour état courant du dossier ou de l’opération de l’objet p.
            a.Audit("RESULTS_LOCKED", id, new { p.Semester }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Un semestre verrouillé interdit les rectifications ultérieures.
        app.MapGet("/api/rankings", async (Guid enrollmentId, int semester, bool annual, PortalDb db, Access a) => // Associe la requête HTTP GET /api/rankings aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var target = await db.Enrollment(enrollmentId); // Charge depuis la base la variable target, utilisée par les instructions suivantes.
            var all = await a.Can("grades.view"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            if (!all && target.StudentId != a.User.StudentId) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Classement privé.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var ranks = await Rankings(db, target, semester, annual, all); // Calcule ou prépare la variable ranks, utilisée par les instructions suivantes.
            return Results.Ok(all ? ranks : ranks.Where(r => r.EnrollmentId == target.Id)); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le classement L3 regroupe année, niveau, spécialité et parcours à travers toutes les salles.
        app.MapGet("/api/enrollments/{id:guid}/transcript", async (Guid id, bool? pdf, PortalDb db, Access a) => // Associe la requête HTTP GET /api/enrollments/{id:guid}/transcript aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var e = await db.Enrollment(id); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
            if (!await a.Can("grades.view") && e.StudentId != a.User.StudentId) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Bulletin privé.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var snapshots = await db.Snapshots.Where(s => s.EnrollmentId == id).ToListAsync(); // Charge depuis la base la variable snapshots, utilisée par les instructions suivantes.
            if (snapshots.Count == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Aucun résultat publié."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var header = JsonNode.Parse(snapshots.OrderByDescending(s => s.Created).First().Header)!; // Le relevé conserve son identité publiée même si le dossier courant est modifié.
            var results = snapshots.GroupBy(s => s.Semester).Select(g => JsonSerializer.Deserialize<ResultView>(g.OrderByDescending(x => x.Version).First().Content)!).OrderBy(s => s.Semester).ToList(); // Calcule ou prépare la variable results, utilisée par les instructions suivantes.
            var html = new StringBuilder("<!doctype html><html lang='fr'><meta charset='utf-8'><title>Relevé IAI</title><style>body{font:14px Arial;margin:40px;color:#152e38}table{border-collapse:collapse;width:100%;margin:20px 0}td,th{border:1px solid #aaa;padding:8px}button{padding:12px}@media print{button{display:none}}</style><button onclick='print()'>Imprimer / Enregistrer en PDF</button><h1>Institut Africain d’Informatique — Togo</h1>"); // Calcule ou prépare la variable html, utilisée par les instructions suivantes.
            html.Append($"<h2>Relevé de notes — {WebUtility.HtmlEncode(e.Year.Code)}</h2><p>{WebUtility.HtmlEncode(header["name"]?.GetValue<string>() ?? e.Student.FirstName + " " + e.Student.LastName)} · {e.Student.Number} · {WebUtility.HtmlEncode(header["track"]?.GetValue<string>() ?? e.Curriculum.Track)}</p>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
            foreach (var r in results) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                html.Append($"<h3>Semestre {r.Semester}</h3><table><tr><th>UE / EC</th><th>Note</th><th>Coefficient / crédits</th><th>Décision</th></tr>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
                foreach (var u in r.Ues) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    html.Append($"<tr><th>{WebUtility.HtmlEncode(u.Name)}</th><th>{u.Average:F2}</th><th>{u.Awarded}/{u.Credits}</th><th>{u.Status}</th></tr>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
                    foreach (var x in u.Ecs) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                        html.Append($"<tr><td>{WebUtility.HtmlEncode(x.Name)}</td><td>{x.Note:F2}</td><td>{x.Coefficient}</td><td></td></tr>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
                } // Ferme ce bloc après l’exécution de ses instructions.

                html.Append($"</table><p>Moyenne : {r.Average:F2}/20 · Crédits : {r.Awarded}/{r.TotalCredits}</p>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
            } // Ferme ce bloc après l’exécution de ses instructions.

            if (results.Count == 2) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                html.Append($"<h3>Moyenne annuelle : {GradeEngine.Annual(results, (string)JsonNode.Parse(e.Curriculum.Content)!["annualMethod"]!):F2}/20</h3>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
            html.Append("<p>Résultats issus des versions publiées conservées dans le SI.</p></html>"); // Ajoute cette portion échappée au document de relevé destiné à l’impression PDF.
            if (pdf == true) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                return Results.File(await PdfRenderer.Render(html.ToString()), "application/pdf", $"releve-{e.Student.Number}-{e.Year.Code}.pdf"); // Renvoie les octets ou le fichier autorisé avec son type de contenu et son nom de téléchargement.
            return Results.Content(html.ToString(), "text/html", Encoding.UTF8); // Renvoie le document texte généré avec le type HTTP indiqué.
        }); // Le navigateur fournit le PDF imprimable en attendant le modèle officiel demandé.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static Dictionary<string, Guid> Headers(Curriculum c, int semester) // Les codes dupliqués sont désambiguïsés dans le CSV.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var sem = JsonNode.Parse(c.Content)!["semesters"]!.AsArray().SingleOrDefault(s => (int)s!["number"]! == semester) ?? throw new RuleException("Semestre inconnu."); // Sélectionne le semestre de la version.
        var ecs = sem["ues"]!.AsArray().SelectMany(u => u!["ecs"]!.AsArray()).ToList(); // Aplatit la hiérarchie pour produire les colonnes.
        return ecs.ToDictionary(e => ecs.Count(x => (string)x!["code"]! == (string)e!["code"]!) > 1 ? (string)e!["code"]! + "__" + ((string)e["id"]!)[..8] : (string)e!["code"]!, e => Guid.Parse((string)e!["id"]!)); // Le projet et le sport SPO_6 ne partagent jamais une cellule.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static async Task<ResultView> Result(PortalDb db, Enrollment e, int semester) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var previousIds = await db.Enrollments.Where(p => p.StudentId == e.StudentId && p.YearId != e.YearId && p.Created < e.Created && p.CurriculumId == e.CurriculumId).Select(p => p.Id).ToListAsync(); // Recherche les tentatives antérieures dans la même version de programme.
        var snapshots = await db.Snapshots.Where(s => previousIds.Contains(s.EnrollmentId) && s.Semester == semester).ToListAsync(); // Seules les notes publiées peuvent être capitalisées.
        var acquired = snapshots.GroupBy(s => s.EnrollmentId).Select(g => g.OrderByDescending(s => s.Version).First()).SelectMany(s => JsonSerializer.Deserialize<ResultView>(s.Content)!.Ues).Where(u => u.Status.StartsWith("VALIDATED")).ToList(); // Conserve les UE validées naturellement ou par décision de jury.
        return GradeEngine.Calculate(e, semester, await db.Grades.Where(g => g.EnrollmentId == e.Id && g.Semester == semester).ToListAsync(), await db.Rescues.Where(r => r.EnrollmentId == e.Id).ToListAsync(), acquired); // Les EC non acquis doivent toujours être notés dans la nouvelle année.
    } // Charge les données puis appelle le moteur pur.

    static async Task<List<ResultView>> AllResults(PortalDb db, Enrollment e) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var output = new List<ResultView>(); // Calcule ou prépare la variable output, utilisée par les instructions suivantes.
        foreach (var s in JsonNode.Parse(e.Curriculum.Content)!["semesters"]!.AsArray()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            output.Add(await Result(db, e, (int)s!["number"]!)); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        return output; // Remet au code appelant la valeur construite par cette expression.
    } // Les gestionnaires peuvent voir les brouillons des deux semestres.

    static async Task Snapshot(PortalDb db, Enrollment e, int semester) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var result = await Result(db, e, semester); // Calcule ou prépare la variable result, utilisée par les instructions suivantes.
        var content = JsonSerializer.Serialize(result); // Calcule ou prépare la variable content, utilisée par les instructions suivantes.
        var versions = await db.Snapshots.Where(s => s.EnrollmentId == e.Id && s.Semester == semester).Select(s => s.Version).ToListAsync(); // Charge depuis la base la variable versions, utilisée par les instructions suivantes.
        db.Snapshots.Add(new() { EnrollmentId = e.Id, Semester = semester, Version = versions.DefaultIfEmpty(0).Max() + 1, Header = JsonSerializer.Serialize(new { name = e.Student.FirstName + " " + e.Student.LastName, number = e.Student.Number, year = e.Year.Code, track = e.Curriculum.Track, curriculumVersion = e.Curriculum.Version }), Content = content, Hash = Security.Hash(content) }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
        await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
    } // Chaque correction ajoute une version sans écraser la précédente.

    public record Rank(Guid EnrollmentId, string Number, string Name, decimal Average, int Position, int Total); // Une ligne de classement décrit un ex æquo avec le même rang.
    public static async Task<List<Rank>> Rankings(PortalDb db, Enrollment target, int semester, bool annual, bool draft) // Compare uniquement des groupes académiques compatibles.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var scoped = JsonNode.Parse(target.Curriculum.Content)!["rankingScope"]?.GetValue<string>() == "YEAR_LEVEL_TRACK_SPECIALITY"; // Le regroupement du classement est une règle du programme, pas un nom de niveau codé en dur.
        var entries = await db.Enrollments.Include(e => e.Curriculum).Include(e => e.Student).Include(e => e.Class).Where(e => e.YearId == target.YearId && e.Status != "TRANSFERRED" && e.Curriculum.Level == target.Curriculum.Level && (!scoped || e.Curriculum.Track == target.Curriculum.Track && e.Curriculum.Speciality == target.Curriculum.Speciality)).ToListAsync(); // Les salles physiques n'interviennent pas dans le classement L3.
        var scores = new List<(Enrollment E, decimal Score)>(); // Prépare les moyennes avant le tri.
        foreach (var e in entries) // Parcourt le groupe de comparaison.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var values = draft ? await AllResults(db, e) : (await db.Snapshots.Where(s => s.EnrollmentId == e.Id).ToListAsync()).GroupBy(s => s.Semester).Select(g => JsonSerializer.Deserialize<ResultView>(g.OrderByDescending(s => s.Version).First().Content)!).ToList(); // Pour les étudiants, les valeurs proviennent exclusivement des publications.
            if (annual && values.Count == 2 && values.All(v => v.Complete)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                scores.Add((e, GradeEngine.Annual(values, (string)JsonNode.Parse(e.Curriculum.Content)!["annualMethod"]!))); // Classe l'année seulement quand ses deux semestres sont complets.
            else if (!annual && values.SingleOrDefault(v => v.Semester == semester)?.Average is decimal score) // Teste ce cas uniquement lorsque le cas précédent n’a pas été retenu.
                scores.Add((e, score)); // Ignore les moyennes incomplètes plutôt que les remplacer par zéro.
        } // Ferme ce bloc après l’exécution de ses instructions.

        var sorted = scores.OrderByDescending(s => s.Score).ToList(); // Trie sur la précision complète, avant l'arrondi d'affichage.
        return sorted.Select(s => new Rank(s.E.Id, s.E.Student.Number, s.E.Student.FirstName + " " + s.E.Student.LastName, s.Score, (JsonNode.Parse(target.Curriculum.Content)!["tieMethod"]?.GetValue<string>() == "DENSE" ? sorted.Where(x => x.Score > s.Score).Select(x => x.Score).Distinct().Count() : sorted.Count(x => x.Score > s.Score)) + 1, sorted.Count)).ToList(); // Produit le classement de compétition 1, 2, 2, 4.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
