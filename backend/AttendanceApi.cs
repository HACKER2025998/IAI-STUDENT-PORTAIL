using System.Text.Json.Nodes; // Lit les demandes de présence et de sanction.
using Microsoft.EntityFrameworkCore; // Utilise les contraintes relationnelles contre les doublons.

namespace Iai; // Module Présences et discipline.
public static class AttendanceApi // Applique les règles de classe avant toute action étudiante.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Enregistre les routes du module.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapGet("/api/attendance/sessions", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/attendance/sessions aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("attendance.reports") || await a.Can("attendance.manage"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            var ids = await db.Enrollments.Where(e => e.StudentId == a.User.StudentId && e.Status == "ACTIVE").Select(e => e.ClassId).ToListAsync(); // Charge depuis la base la variable ids, utilisée par les instructions suivantes.
            return Results.Ok(await db.Sessions.Where(s => all || ids.Contains(s.ClassId)).Select(s => new { s.Id, s.ClassId, s.EcId, s.Course, s.Opened, s.Closed }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Ne divulgue jamais le secret HMAC de la séance.
        app.MapPost("/api/attendance/sessions", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/attendance/sessions aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var classId = j.Id("classId"); // Calcule ou prépare la variable classId, utilisée par les instructions suivantes.
            await a.ManageAttendance(classId); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
            var c = await db.Classes.Include(c => c.Year).SingleOrDefaultAsync(c => c.Id == classId) ?? throw new RuleException("Classe inconnue."); // Charge depuis la base la variable c, utilisée par les instructions suivantes.
            if (c.Year.Status != "OPEN") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Année fermée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var ecId = j.Id("ecId"); // Calcule ou prépare la variable ecId, utilisée par les instructions suivantes.
            var curricula = await db.Enrollments.Include(e => e.Curriculum).Where(e => e.ClassId == classId && e.Status == "ACTIVE").Select(e => e.Curriculum.Content).ToListAsync(); // Charge depuis la base la variable curricula, utilisée par les instructions suivantes.
            var ec = curricula.SelectMany(p => JsonNode.Parse(p)!["semesters"]!.AsArray()).SelectMany(s => s!["ues"]!.AsArray()).SelectMany(u => u!["ecs"]!.AsArray()).FirstOrDefault(e => Guid.Parse((string)e!["id"]!) == ecId) ?? throw new RuleException("Cours absent des programmes de la classe."); // Calcule ou prépare la variable ec, utilisée par les instructions suivantes.
            var s = new CourseSession // Calcule ou prépare la variable s, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                ClassId = classId, // Renseigne identifiant de la classe physique avec la valeur calculée ou validée à droite.
                EcId = ecId, // Renseigne identifiant de la matière avec la valeur calculée ou validée à droite.
                Course = (string)ec["name"]!, // Renseigne libellé de la matière suivie pendant la séance avec la valeur calculée ou validée à droite.
                Secret = Security.Secret() // Renseigne clé secrète de signature du code dynamique du cours avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Sessions.Add(s); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("ATTENDANCE_SESSION_OPENED", s.Id, new { s.ClassId, s.Course }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { s.Id, s.Course }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Un code dynamique appartient à un cours précis, jamais à une classe générique.
        app.MapGet("/api/attendance/sessions/{id:guid}/token", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP GET /api/attendance/sessions/{id:guid}/token aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var s = await db.Sessions.FindAsync(id) ?? throw new RuleException("Séance inconnue."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            await a.Display(s.ClassId); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
            if (s.Closed != null || PortalClock.Now - s.Opened > TimeSpan.FromHours(8)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Séance fermée ou expirée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var now = PortalClock.Now; // Calcule ou prépare la variable now, utilisée par les instructions suivantes.
            var code = Security.Code(s, now); // Calcule ou prépare la variable code, utilisée par les instructions suivantes.
            return Results.Ok(new { code, qr = $"iai:{s.Id}:{code}", expiresIn = 120 - now.ToUnixTimeSeconds() % 120 }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // QR et code texte passent par le même validateur serveur.
        app.MapPost("/api/attendance/sessions/{id:guid}/close", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP POST /api/attendance/sessions/{id:guid}/close aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var s = await db.Sessions.FindAsync(id) ?? throw new RuleException("Séance inconnue."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            await a.ManageAttendance(s.ClassId); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
            if (s.Closed != null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Séance déjà fermée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            s.Closed = PortalClock.Now; // Met à jour date UTC de fermeture, absente tant que le cours est ouvert de l’objet s.
            var students = await db.Enrollments.Where(e => e.ClassId == s.ClassId && e.Status == "ACTIVE").Select(e => e.StudentId).ToListAsync(); // Charge depuis la base la variable students, utilisée par les instructions suivantes.
            var checkedIds = await db.Attendances.Where(r => r.SessionId == id).Select(r => r.StudentId).ToListAsync(); // Charge depuis la base la variable checkedIds, utilisée par les instructions suivantes.
            foreach (var studentId in students.Except(checkedIds)) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                db.Attendances.Add(new() { SessionId = id, StudentId = studentId, Status = "ABSENT", Method = "SESSION_CLOSE" }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("ATTENDANCE_SESSION_CLOSED", id, new { absent = students.Except(checkedIds).Count() }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // La fermeture matérialise les absences des étudiants non pointés.
        app.MapPost("/api/attendance/check-in", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/attendance/check-in aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            if (a.User.StudentId == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Compte étudiant requis."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var s = await db.Sessions.FindAsync(j.Id("sessionId")) ?? throw new RuleException("Séance inconnue."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            if (!await a.Member(s.ClassId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Vous n'appartenez pas à cette classe.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var self = j.Text("method") == "OFFICER_SELF"; // Calcule ou prépare la variable self, utilisée par les instructions suivantes.
            if (self) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                if (!await a.Officer(s.ClassId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Mandat de classe requis.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
                if (s.Closed != null || PortalClock.Now - s.Opened > TimeSpan.FromHours(8)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Séance expirée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            } // Ferme ce bloc après l’exécution de ses instructions.
            else if (!Security.CodeValid(s, j.Required("code"), PortalClock.Now)) // Teste ce cas uniquement lorsque le cas précédent n’a pas été retenu.
                throw new RuleException("Code expiré ou incorrect."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            if (await db.Attendances.AnyAsync(r => r.SessionId == s.Id && r.StudentId == a.User.StudentId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Présence déjà enregistrée."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var record = new Attendance // Calcule ou prépare la variable record, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                SessionId = s.Id, // Renseigne identifiant du cours ouvert pour la présence avec la valeur calculée ou validée à droite.
                StudentId = a.User.StudentId.Value, // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
                Method = self ? "OFFICER_SELF" : j.Text("method") == "QR" ? "QR" : "CODE" // Renseigne origine du pointage pour distinguer QR, code et correction administrative avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Attendances.Add(record); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("ATTENDANCE_CHECKED", record.Id, new { record.Method, s.Course }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { record.Status, record.Checked, s.Course }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les responsables ont le bouton direct ; les autres fournissent un code récent.
        app.MapGet("/api/attendance/records", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/attendance/records aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("attendance.reports"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            return Results.Ok(await db.Attendances.Where(r => all || r.StudentId == a.User.StudentId).Select(r => new { r.Id, r.SessionId, r.StudentId, r.Session.ClassId, r.Session.Course, r.Status, r.Method, r.Checked, studentName = r.Student.FirstName + " " + r.Student.LastName }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les rapports peuvent ensuite être filtrés par jour ou semaine côté interface.
        app.MapPost("/api/attendance/manual", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/attendance/manual aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("attendance.manage"); // Exige la permission attendance.manage contenue dans les rôles actuels du compte.
            var s = await db.Sessions.FindAsync(j.Id("sessionId")) ?? throw new RuleException("Séance inconnue."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            var studentId = j.Id("studentId"); // Calcule ou prépare la variable studentId, utilisée par les instructions suivantes.
            if (!await db.Enrollments.AnyAsync(e => e.ClassId == s.ClassId && e.StudentId == studentId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Étudiant hors classe."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var status = j.Required("status"); // Calcule ou prépare la variable status, utilisée par les instructions suivantes.
            if (!new[] // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "PRESENT", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "ABSENT", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "LATE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "EXCUSED" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            }.Contains(status)) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                throw new RuleException("Statut invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var r = await db.Attendances.SingleOrDefaultAsync(r => r.SessionId == s.Id && r.StudentId == studentId); // Charge depuis la base la variable r, utilisée par les instructions suivantes.
            if (r == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                r = new() // Renseigne le champ r avec la valeur calculée ou validée à droite.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    SessionId = s.Id, // Renseigne identifiant du cours ouvert pour la présence avec la valeur calculée ou validée à droite.
                    StudentId = studentId // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
                }; // Termine cette construction et revient à l’instruction qui l’utilise.
                db.Attendances.Add(r); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            } // Ferme ce bloc après l’exécution de ses instructions.

            var before = r.Status; // Calcule ou prépare la variable before, utilisée par les instructions suivantes.
            r.Status = status; // Met à jour état courant du dossier ou de l’opération de l’objet r.
            r.Method = "ADMIN_MANUAL"; // Met à jour origine du pointage pour distinguer QR, code et correction administrative de l’objet r.
            a.Audit("ATTENDANCE_RECTIFIED", r.Id, new { before, status, reason = j.Required("reason") }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Une correction manuelle nécessite toujours un motif audité.
        app.MapGet("/api/delegations", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/delegations aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("attendance.manage"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            return Results.Ok(await db.Delegations.Where(d => all || d.Granter == a.User.StudentId || d.Grantee == a.User.StudentId).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // La date d'expiration permet de distinguer les anciens mandats.
        app.MapPost("/api/delegations", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/delegations aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var classId = j.Id("classId"); // Calcule ou prépare la variable classId, utilisée par les instructions suivantes.
            if (!await a.Officer(classId, true)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Seul le délégué ou sous-délégué peut déléguer.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var grantee = j.Id("studentId"); // Calcule ou prépare la variable grantee, utilisée par les instructions suivantes.
            if (grantee == a.User.StudentId || !await db.Enrollments.AnyAsync(e => e.ClassId == classId && e.StudentId == grantee && e.Status == "ACTIVE")) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Destinataire invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable); // Ouvre une transaction SQL afin qu’une erreur annule toute l’opération.
            if (await db.Delegations.CountAsync(d => d.Granter == a.User.StudentId && !d.Revoked && d.Expires > PortalClock.Now) >= 6) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Six délégations actives maximum."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var d = new Delegation // Calcule ou prépare la variable d, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                ClassId = classId, // Renseigne identifiant de la classe physique avec la valeur calculée ou validée à droite.
                Granter = a.User.StudentId!.Value, // Renseigne étudiant responsable qui accorde l’affichage temporaire avec la valeur calculée ou validée à droite.
                Grantee = grantee, // Renseigne étudiant qui reçoit uniquement le droit d’affichage QR avec la valeur calculée ou validée à droite.
                Expires = PortalClock.Now.AddHours(24) // Renseigne instant au-delà duquel le droit temporaire est inutilisable avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Delegations.Add(d); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("QR_DISPLAY_DELEGATED", d.Id, new { grantee, d.Expires }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            await tx.CommitAsync(); // Valide la transaction pour rendre tous ses changements persistants ensemble.
            return Results.Ok(d); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // La délégation donne uniquement l'affichage du QR pendant vingt-quatre heures.
        app.MapDelete("/api/delegations/{id:guid}", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP DELETE /api/delegations/{id:guid} aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var d = await db.Delegations.FindAsync(id) ?? throw new RuleException("Délégation inconnue."); // Charge depuis la base la variable d, utilisée par les instructions suivantes.
            if (!await a.Can("attendance.manage") && d.Granter != a.User.StudentId) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Révocation interdite.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            d.Revoked = true; // Met à jour indicateur de révocation anticipée de l’objet d.
            a.Audit("DELEGATION_REVOKED", id, new { d.Grantee }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Le délégué peut retirer uniquement les droits qu'il a lui-même accordés.
        app.MapGet("/api/justifications", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/justifications aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("attendance.manage"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            return Results.Ok(await db.Justifications.Where(j => all || j.StudentId == a.User.StudentId).Select(j => new { j.Id, j.SessionId, j.StudentId, j.Reason, j.Status, j.Decision, j.DocumentId, course = j.Session.Course }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // CPE et surveillant consultent les justificatifs à traiter.
        app.MapPost("/api/justifications", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/justifications aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            if (a.User.StudentId == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Compte étudiant requis."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var s = await db.Sessions.FindAsync(j.Id("sessionId")) ?? throw new RuleException("Séance inconnue."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            if (!await a.Member(s.ClassId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Classe inaccessible.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            Guid? documentId = j.Text("documentId") == "" ? null : j.Id("documentId"); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            if (documentId != null && !await db.Documents.AnyAsync(d => d.Id == documentId && d.OwnerStudentId == a.User.StudentId && d.Category == "JUSTIFICATION")) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Pièce jointe inaccessible."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var r = new Justification // Calcule ou prépare la variable r, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                SessionId = s.Id, // Renseigne identifiant du cours ouvert pour la présence avec la valeur calculée ou validée à droite.
                StudentId = a.User.StudentId.Value, // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
                Reason = j.Required("reason"), // Renseigne motif explicite de la décision avec la valeur calculée ou validée à droite.
                DocumentId = documentId // Renseigne référence de la pièce jointe privée avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Justifications.Add(r); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("ABSENCE_DECLARED", r.Id, new { s.Id }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { r.Id }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le document justificatif doit appartenir à l'étudiant concerné.
        app.MapPost("/api/justifications/{id:guid}/decision", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/justifications/{id:guid}/decision aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("attendance.manage"); // Exige la permission attendance.manage contenue dans les rôles actuels du compte.
            var r = await db.Justifications.FindAsync(id) ?? throw new RuleException("Justificatif inconnu."); // Charge depuis la base la variable r, utilisée par les instructions suivantes.
            if (r.Status != "PENDING") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Justificatif déjà traité."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var accepted = j["accepted"]!.GetValue<bool>(); // Calcule ou prépare la variable accepted, utilisée par les instructions suivantes.
            r.Status = accepted ? "ACCEPTED" : "REJECTED"; // Met à jour état courant du dossier ou de l’opération de l’objet r.
            r.Decision = j.Required("comment"); // Met à jour commentaire du responsable sur le justificatif de l’objet r.
            var record = await db.Attendances.SingleOrDefaultAsync(x => x.SessionId == r.SessionId && x.StudentId == r.StudentId); // Charge depuis la base la variable record, utilisée par les instructions suivantes.
            if (record != null && record.Status is "PRESENT" or "LATE") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Présence déjà enregistrée : rectifiez d'abord le pointage."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            if (record == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                record = new() // Renseigne le champ record avec la valeur calculée ou validée à droite.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    SessionId = r.SessionId, // Renseigne identifiant du cours ouvert pour la présence avec la valeur calculée ou validée à droite.
                    StudentId = r.StudentId, // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
                    Method = "JUSTIFICATION" // Renseigne origine du pointage pour distinguer QR, code et correction administrative avec la valeur calculée ou validée à droite.
                }; // Termine cette construction et revient à l’instruction qui l’utilise.
                db.Attendances.Add(record); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            } // Ferme ce bloc après l’exécution de ses instructions.

            record.Status = accepted ? "EXCUSED" : "ABSENT"; // Met à jour état courant du dossier ou de l’opération de l’objet record.
            db.Notifications.Add(new() { StudentId = r.StudentId, Message = "Votre justificatif est " + (accepted ? "accepté." : "rejeté.") }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("JUSTIFICATION_DECIDED", id, new { r.Status, r.Decision }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Une acceptation justifie l'absence sans fabriquer une présence.
        app.MapGet("/api/discipline", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/discipline aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("discipline.manage"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            var events = await db.Discipline.Include(d => d.Enrollment).Where(d => all || d.Enrollment.StudentId == a.User.StudentId).Select(d => new { d.Id, d.EnrollmentId, d.Semester, d.Delta, d.Reason, d.Created }).ToListAsync(); // Charge depuis la base la variable events, utilisée par les instructions suivantes.
            return Results.Ok(new { events, scores = events.GroupBy(e => new { e.EnrollmentId, e.Semester }).Select(g => new { g.Key.EnrollmentId, g.Key.Semester, score = GradeEngine.Discipline(g.Select(e => e.Delta)) }) }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le score est recalculé à partir du registre de sanctions.
        app.MapPost("/api/discipline", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/discipline aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("discipline.manage"); // Exige la permission discipline.manage contenue dans les rôles actuels du compte.
            var e = await db.Enrollment(j.Id("enrollmentId")); // Charge depuis la base la variable e, utilisée par les instructions suivantes.
            var semester = j.Int("semester"); // Calcule ou prépare la variable semester, utilisée par les instructions suivantes.
            if (e.Status != "ACTIVE" || !JsonNode.Parse(e.Curriculum.Content)!["semesters"]!.AsArray().Any(s => (int)s!["number"]! == semester)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Inscription ou semestre invalide."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var delta = j.Decimal("delta"); // Calcule ou prépare la variable delta, utilisée par les instructions suivantes.
            if (delta < -20 || delta > 20 || delta == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Variation entre -20 et 20, différente de zéro."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var d = new DisciplineEvent // Calcule ou prépare la variable d, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                EnrollmentId = e.Id, // Renseigne identifiant de l’inscription annuelle avec la valeur calculée ou validée à droite.
                Semester = semester, // Renseigne numéro de semestre concerné avec la valeur calculée ou validée à droite.
                Delta = delta, // Renseigne variation de points ajoutée au registre disciplinaire avec la valeur calculée ou validée à droite.
                Reason = j.Required("reason"), // Renseigne motif explicite de la décision avec la valeur calculée ou validée à droite.
                Actor = a.User.Id // Renseigne UUID du compte auteur de la décision avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            db.Discipline.Add(d); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            db.Notifications.Add(new() { StudentId = e.StudentId, Message = $"Événement disciplinaire : {delta} point(s). {d.Reason}" }); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("DISCIPLINE_EVENT_ADDED", d.Id, new { delta, d.Reason }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { d.Id }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // La discipline n'est pas automatiquement injectée dans Séminaire sans formule officielle.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
