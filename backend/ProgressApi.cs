using System.Text.Json; // Lit les relevés historiques sans recalculer les anciennes notes.
using System.Text.Json.Nodes; // Trouve les UE de la nouvelle version.
using Microsoft.EntityFrameworkCore; // Charge les inscriptions et les décisions d'équivalence.

namespace Iai; // Module de capitalisation des crédits.
public class Equivalence : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid SourceUe { get; set; } // Conserve UE validée dans une ancienne version ; get lit sa valeur et set permet de l’affecter.
    public Guid TargetUe { get; set; } // Conserve UE reconnue dans la version de destination ; get lit sa valeur et set permet de l’affecter.
    public string Reason { get; set; } = ""; // Conserve motif explicite de la décision ; get lit sa valeur et set permet de l’affecter.
    public Guid Actor { get; set; } // Conserve UUID du compte auteur de la décision ; get lit sa valeur et set permet de l’affecter.
} // Une équivalence n'est utilisable qu'après une décision administrative explicite.

public static class ProgressApi // Les crédits antérieurs sont présentés sans inventer de nouvelles notes d'EC.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Installe les routes de suivi des UE.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapPost("/api/equivalences", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/equivalences aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("academic.manage"); // Exige la permission academic.manage contenue dans les rôles actuels du compte.
            var source = j.Id("sourceUe"); // Calcule ou prépare la variable source, utilisée par les instructions suivantes.
            var target = j.Id("targetUe"); // Calcule ou prépare la variable target, utilisée par les instructions suivantes.
            var contents = await db.Curricula.Select(c => c.Content).ToListAsync(); // Charge depuis la base la variable contents, utilisée par les instructions suivantes.
            var ids = contents.SelectMany(c => JsonNode.Parse(c)!["semesters"]!.AsArray()).SelectMany(s => s!["ues"]!.AsArray()).Select(u => Guid.Parse((string)u!["id"]!)).ToHashSet(); // Calcule ou prépare la variable ids, utilisée par les instructions suivantes.
            if (source == target || !ids.Contains(source) || !ids.Contains(target)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("UE source ou cible invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var e = new Equivalence // Calcule ou prépare la variable e, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                SourceUe = source, // Renseigne UE validée dans une ancienne version avec la valeur calculée ou validée à droite.
                TargetUe = target, // Renseigne UE reconnue dans la version de destination avec la valeur calculée ou validée à droite.
                Reason = j.Required("reason"), // Renseigne motif explicite de la décision avec la valeur calculée ou validée à droite.
                Actor = a.User.Id // Renseigne UUID du compte auteur de la décision avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Set<Equivalence>().Add(e); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("UE_EQUIVALENCE_APPROVED", e.Id, new { source, target, e.Reason }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(e); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Aucun rapprochement de nom ou de code n'est réalisé arbitrairement.
        app.MapGet("/api/enrollments/{id:guid}/progress", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP GET /api/enrollments/{id:guid}/progress aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var e = await db.Enrollment(id); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
            if (!await a.Can("grades.view") && !await a.Can("students.view") && e.StudentId != a.User.StudentId) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Suivi privé.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var previous = await db.Enrollments.Include(x => x.Curriculum).Where(x => x.StudentId == e.StudentId && x.Id != id && x.YearId != e.YearId && x.Created < e.Created && x.Curriculum.Level == e.Curriculum.Level && x.Curriculum.Track == e.Curriculum.Track && x.Curriculum.Speciality == e.Curriculum.Speciality).Select(x => x.Id).ToListAsync(); // Charge depuis la base la variable previous, utilisée par les instructions suivantes.
            var snapshots = await db.Snapshots.Where(s => previous.Contains(s.EnrollmentId)).ToListAsync(); // Charge depuis la base la variable snapshots, utilisée par les instructions suivantes.
            var units = snapshots.GroupBy(s => new { s.EnrollmentId, s.Semester }).Select(g => g.OrderByDescending(x => x.Version).First()).SelectMany(s => JsonSerializer.Deserialize<ResultView>(s.Content)!.Ues.Where(u => u.Status.StartsWith("VALIDATED")).Select(u => new { Unit = u, s.EnrollmentId })).ToList(); // Calcule ou prépare la variable units, utilisée par les instructions suivantes.
            var equivalents = await db.Set<Equivalence>().ToListAsync(); // Charge depuis la base la variable equivalents, utilisée par les instructions suivantes.
            var current = JsonNode.Parse(e.Curriculum.Content)!["semesters"]!.AsArray().SelectMany(s => s!["ues"]!.AsArray()).Select(u => new { Id = Guid.Parse((string)u!["id"]!), Name = (string)u["name"]!, Credits = u["credits"]?.GetValue<decimal>() }); // Calcule ou prépare la variable current, utilisée par les instructions suivantes.
            return Results.Ok(current.Select(u => // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var found = units.FirstOrDefault(p => p.Unit.Id == u.Id || equivalents.Any(eq => eq.SourceUe == p.Unit.Id && eq.TargetUe == u.Id)); // Calcule ou prépare la variable found, utilisée par les instructions suivantes.
                return new // Remet au code appelant la valeur construite par cette expression.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    u.Id, // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                    u.Name, // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                    u.Credits, // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                    status = found == null ? "TO_VALIDATE" : "VALIDATED_PREVIOUSLY", // Renseigne le champ status avec la valeur calculée ou validée à droite.
                    previousAverage = found?.Unit.Average, // Renseigne le champ previousAverage avec la valeur calculée ou validée à droite.
                    awarded = found == null ? 0 : u.Credits ?? 0, // Renseigne le champ awarded avec la valeur calculée ou validée à droite.
                    sourceEnrollment = found?.EnrollmentId // Renseigne le champ sourceEnrollment avec la valeur calculée ou validée à droite.
                }; // Termine cette construction et revient à l’instruction qui l’utilise.
            })); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        }); // Le suivi montre les UE acquises et celles qui restent à valider dans la nouvelle année.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
