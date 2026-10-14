using System.Security.Cryptography; // Fournit des secrets aléatoires et des signatures résistantes à la falsification.
using System.Text; // Convertit les textes en octets pour les signatures.
using System.Text.Json.Nodes; // Permet de lire les programmes pédagogiques versionnés.
using System.Text.Json; // Sérialise les instantanés officiels.
using Microsoft.AspNetCore.Identity; // Utilise le hachage de mots de passe du framework.
using Microsoft.EntityFrameworkCore; // Exécute les requêtes relationnelles.

namespace Iai; // Place les services dans le même espace métier.
public sealed class RuleException(string message, int status = 400) : Exception(message) // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public int Status { get; } = status; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
} // Une violation métier devient une réponse HTTP lisible.

public static class Security // Les méthodes de sécurité sont réunies pour éviter les duplications.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static string Secret() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16)); // Génère un secret de 128 bits non prévisible.
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))); // Les codes aléatoires sont stockés sous empreinte uniquement.
    public static string Password(User user, string password) => new PasswordHasher<User>().HashPassword(user, password); // Le sel et les paramètres de dérivation sont inclus dans le résultat.
    public static bool Verify(User user, string password) => new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed; // Compare sans déchiffrer de mot de passe.
    public static string Code(CourseSession session, DateTimeOffset now) // La séance et la tranche de deux minutes déterminent le code.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var slot = now.ToUnixTimeSeconds() / 120; // Divise le temps Unix en fenêtres de 120 secondes.
        var bytes = HMACSHA256.HashData(Convert.FromHexString(session.Secret), Encoding.UTF8.GetBytes($"{session.Id}:{slot}")); // Signe l'identité du cours et sa fenêtre.
        return Convert.ToHexString(bytes)[..8]; // Un code hexadécimal de huit caractères peut aussi être saisi au clavier.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static bool CodeValid(CourseSession s, string code, DateTimeOffset now) // Accepte uniquement une séance ouverte et un code récent.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        if (s.Closed != null || now - s.Opened > TimeSpan.FromHours(8)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            return false; // Une séance oubliée n'est pas exploitable indéfiniment.
        return Code(s, now) == code.ToUpperInvariant() || (now.ToUnixTimeSeconds() % 120 < 10 && Code(s, now.AddSeconds(-120)) == code.ToUpperInvariant()); // Une tolérance de dix secondes couvre le changement de fenêtre.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.

public record EcView(Guid Id, string Code, string Name, decimal Coefficient, decimal? Note); // Une ligne de relevé conserve le coefficient réellement employé.
public record UeView(Guid Id, string Code, string Name, decimal? Average, string Status, decimal? Credits, decimal Awarded, List<EcView> Ecs); // Le crédit inconnu reste null plutôt qu'être inventé.
public record ResultView(Guid EnrollmentId, int Semester, decimal? Average, decimal Awarded, decimal? TotalCredits, bool Complete, List<UeView> Ues); // Un semestre incomplet ne reçoit pas de moyenne trompeuse.
public static class GradeEngine // Ce moteur est pur : ses calculs peuvent être testés sans serveur.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static ResultView Calculate(Enrollment enrollment, int semester, IEnumerable<Grade> grades, IEnumerable<Rescue> rescues, IEnumerable<UeView>? acquired = null) // Calcule à partir de la version attachée à l'inscription.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var content = JsonNode.Parse(enrollment.Curriculum.Content)!; // Charge le programme conservé lors de l'inscription.
        var sem = content["semesters"]!.AsArray().SingleOrDefault(s => (int)s!["number"]! == semester) ?? throw new RuleException("Semestre absent du programme."); // Refuse un semestre d'un autre niveau.
        var notes = grades.ToDictionary(g => g.EcId, g => g.Value); // Retrouve chaque note par UUID, même si le code imprimé est dupliqué.
        var previous = (acquired ?? []).Where(u => u.Status.StartsWith("VALIDATED")).GroupBy(u => u.Id).ToDictionary(g => g.Key, g => g.First()); // Capitalise uniquement les UE publiées de la même version.
        foreach (var unit in previous.Values) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            foreach (var ec in unit.Ecs) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                if (ec.Note.HasValue && !notes.ContainsKey(ec.Id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    notes[ec.Id] = ec.Note.Value; // Réutilise les véritables notes des mêmes UUID d'EC, jamais des notes fabriquées.
        var rescued = rescues.Select(r => r.UeId).ToHashSet(); // Les décisions sont distinctes des valeurs des notes.
        var units = new List<UeView>(); // Prépare les unités du relevé.
        decimal numerator = 0, denominator = 0; // Accumule toutes les notes pondérées du semestre.
        bool complete = true; // Une note manquante rend le résultat provisoire.
        foreach (var unit in sem["ues"]!.AsArray()) // Parcourt les UE de la version choisie.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var u = unit!; // Les données validées garantissent une UE non nulle.
            var ecs = u["ecs"]!.AsArray().Select(e => new EcView(Guid.Parse((string)e!["id"]!), (string)e["code"]!, (string)e["name"]!, (decimal)e["coefficient"]!, notes.TryGetValue(Guid.Parse((string)e["id"]!), out var n) ? n : null)).ToList(); // Rattache les notes aux EC et conserve explicitement les cellules vides.
            var all = ecs.All(e => e.Note.HasValue); // Une UE complète possède une note pour chacun de ses EC.
            var average = all ? ecs.Sum(e => e.Note!.Value * e.Coefficient) / ecs.Sum(e => e.Coefficient) : (decimal? )null; // Le dénominateur vient des coefficients, jamais du nombre 30.
            var status = previous.ContainsKey(Guid.Parse((string)u["id"]!)) ? "VALIDATED_PREVIOUSLY" : rescued.Contains(Guid.Parse((string)u["id"]!)) ? "VALIDATED_BY_RESCUE" : average >= (decimal)content["threshold"]! ? "VALIDATED" : all ? "NOT_VALIDATED" : "INCOMPLETE"; // Décide sans arrondir préalablement la moyenne.
            decimal? credits = u["credits"]?.GetValue<decimal>(); // Les crédits manquants sont laissés inconnus.
            units.Add(new(Guid.Parse((string)u["id"]!), (string)u["code"]!, (string)u["name"]!, average, status, credits, status.StartsWith("VALIDATED") ? credits ?? 0 : 0, ecs)); // Prépare l'affichage et le total de crédits acquis.
            complete &= all; // Propage l'incomplétude d'une UE au semestre.
            numerator += ecs.Sum(e => (e.Note ?? 0) * e.Coefficient); // Additionne les notes présentes pondérées.
            denominator += ecs.Sum(e => e.Coefficient); // Additionne le poids de tous les EC attendus.
        } // Ferme ce bloc après l’exécution de ses instructions.

        return new(enrollment.Id, semester, complete ? numerator / denominator : null, units.Sum(u => u.Awarded), units.All(u => u.Credits.HasValue) ? units.Sum(u => u.Credits!.Value) : null, complete, units); // N'expose une moyenne officielle que si tous les EC sont renseignés.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public static decimal Discipline(IEnumerable<decimal> deltas) => Math.Clamp(20 + deltas.Sum(), 0, 20); // La discipline est bornée entre zéro et vingt.
    public static decimal Annual(IEnumerable<ResultView> results, string method) // La méthode annuelle est une propriété du programme.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var semesters = results.ToList(); // Matérialise les deux semestres une seule fois.
        if (semesters.Count != 2 || semesters.Any(s => !s.Complete)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Les deux semestres complets sont nécessaires."); // Interdit une moyenne annuelle partielle.
        return method == "ALL_EC_WEIGHTED" ? semesters.SelectMany(s => s.Ues).SelectMany(u => u.Ecs).Sum(e => e.Note!.Value * e.Coefficient) / semesters.SelectMany(s => s.Ues).SelectMany(u => u.Ecs).Sum(e => e.Coefficient) : semesters.Average(s => s.Average!.Value); // Choisit la pondération paramétrée.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.

public sealed class Access(PortalDb db, IHttpContextAccessor accessor, IWebHostEnvironment env) // Contrôle les permissions à chaque requête et non seulement à la connexion.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public User User { get; private set; } = null !; // Le compte courant est chargé depuis la base.

    public async Task Load() // Vérifie le cookie et sa version de révocation.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var principal = accessor.HttpContext!.User; // Lit l'identité authentifiée par ASP.NET.
        if (!Guid.TryParse(principal.FindFirst("uid")?.Value, out var id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Connexion requise.", 401); // Un cookie absent ne donne aucun accès.
        User = await db.Users.FindAsync(id) ?? throw new RuleException("Compte inconnu.", 401); // Recherche le compte actuel.
        if (!User.Active || User.Version.ToString() != principal.FindFirst("version")?.Value) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Session expirée. Reconnectez-vous.", 401); // La désactivation ou la nouvelle année invalide l'ancien cookie.
        if (!env.IsDevelopment() && principal.FindFirst("mfa")?.Value != "true") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var roles = JsonSerializer.Deserialize<Guid[]>(User.Roles)!; // Calcule ou prépare la variable roles, utilisée par les instructions suivantes.
            var privileged = User.StudentId == null || (await db.Roles.Where(r => roles.Contains(r.Id)).ToListAsync()).Any(r => JsonSerializer.Deserialize<string[]>(r.Permissions)!.Length > 0); // Charge depuis la base la variable privileged, utilisée par les instructions suivantes.
            if (privileged) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Reconnectez-vous avec votre second facteur pour ces nouveaux droits.", 401); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
        } // Une promotion de rôle ne transforme jamais un ancien cookie étudiant en session administrative sans MFA.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public async Task<bool> Can(string permission) // Résout les permissions modifiables de tous les rôles du compte.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var ids = JsonSerializer.Deserialize<Guid[]>(User.Roles)!; // Les affectations contiennent les UUID des rôles.
        var roles = await db.Roles.Where(r => ids.Contains(r.Id)).ToListAsync(); // Recharge les droits pour appliquer immédiatement une modification.
        return roles.Any(r => JsonSerializer.Deserialize<string[]>(r.Permissions)!.Any(p => p == "*" || p == permission || p == permission.Split('.')[0] + ".manage")); // Le rôle SuperAdmin utilise la permission universelle.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public async Task Require(string permission) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        if (!await Can(permission)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Permission insuffisante : " + permission, 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
    } // Arrête la requête avant toute écriture interdite.

    public async Task<bool> Member(Guid classId) => User.StudentId != null && await db.Enrollments.AnyAsync(e => e.StudentId == User.StudentId && e.ClassId == classId && e.Status == "ACTIVE"); // Un étudiant n'agit que dans sa classe active.
    public async Task<bool> Officer(Guid classId, bool canDelegate = false) => await Member(classId) && await db.Officers.AnyAsync(o => o.ClassId == classId && o.StudentId == User.StudentId && (!canDelegate || o.Position != "SECRETARY")); // Seuls le délégué et son adjoint délèguent.
    public async Task ManageAttendance(Guid classId) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        if (!await Can("attendance.manage") && !await Officer(classId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Gestion de présence non autorisée.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
    } // Les mandats restent limités à leur propre salle.

    public async Task Display(Guid classId) // Vérifie le mandat ou une délégation encore valable.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        if (await Can("attendance.manage") || await Officer(classId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            return; // Les responsables ont le droit direct d'affichage.
        if (!await Member(classId) || !await db.Delegations.AnyAsync(d => d.ClassId == classId && d.Grantee == User.StudentId && !d.Revoked && d.Expires > PortalClock.Now)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Affichage non autorisé.", 403); // Une délégation expirée ou hors classe n'accorde rien.
    } // Ferme ce bloc après l’exécution de ses instructions.

    public void Audit(string action, Guid resource, object details) // Ajoute un journal dans la même transaction que l'action métier.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        db.Audits.Add(new() { Actor = User.Id, Action = action, Resource = resource.ToString(), Details = JsonSerializer.Serialize(details), Ip = accessor.HttpContext!.Connection.RemoteIpAddress?.ToString() ?? "" }); // Ne journalise jamais un mot de passe ni un code secret.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.

public static class PortalClock // Regroupe les fonctions du module dans un type sans état partagé mutable.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static DateTimeOffset Now => DateTimeOffset.UtcNow; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
} // Une propriété applicative capture le temps en paramètre SQL au lieu d’exiger une traduction fournisseur.

