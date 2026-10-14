using Microsoft.AspNetCore.DataProtection; // Chiffre les secrets du second facteur avec les clés persistées.
using System.Security.Claims; // Les claims identifient le compte et la version du cookie.
using System.Text.Json; // Sérialise les affectations de rôles.
using System.Text.Json.Nodes; // Lit les formulaires JSON.
using Microsoft.AspNetCore.Authentication; // Crée et détruit les cookies protégés.
using Microsoft.AspNetCore.Authentication.Cookies; // Désigne le mécanisme d'authentification.
using Microsoft.EntityFrameworkCore; // Interroge les comptes en base.

namespace Iai; // Module Identity.
public static class IdentityApi // Les routes publiques sont séparées des routes d'administration.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Enregistre les opérations du module.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapPost("/api/auth/login", async (JsonObject j, PortalDb db, HttpContext ctx, IDataProtectionProvider protection, IWebHostEnvironment env) => // Associe la requête HTTP POST /api/auth/login aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var u = await db.Users.SingleOrDefaultAsync(u => u.Username == j.Text("username")); // Charge depuis la base la variable u, utilisée par les instructions suivantes.
            if (u == null || !u.Active || !Security.Verify(u, j.Required("password"))) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Identifiants incorrects.", 401); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var ids = JsonSerializer.Deserialize<Guid[]>(u.Roles)!; // Résout les droits réels, indépendamment du fait que le compte soit lié à un étudiant.
            var privileged = u.StudentId == null || (await db.Roles.Where(r => ids.Contains(r.Id)).ToListAsync()).Any(r => JsonSerializer.Deserialize<string[]>(r.Permissions)!.Length > 0); // Un rôle donnant des droits administratifs exige également le second facteur.
            var mfaVerified = false; // Aucun cookie ne prétend avoir vérifié le second facteur avant sa validation.
            if (privileged && (!env.IsDevelopment() || u.MfaSecret != "")) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                if (u.MfaSecret == "") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Second facteur non configuré pour ce compte.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
                u.MfaStep = Mfa.Validate(protection.CreateProtector("iai-totp").Unprotect(u.MfaSecret), j.Required("otp"), u.MfaStep); // Met à jour dernière fenêtre TOTP consommée pour empêcher son rejeu de l’objet u.
                mfaVerified = true; // La validation TOTP réussie est attestée dans le cookie.
                await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            } // Ferme ce bloc après l’exécution de ses instructions.

            await SignIn(ctx, u, mfaVerified); // Établit le cookie protégé du compte après vérification de son identité.
            return Results.Ok(new { u.Username }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }).RequireRateLimiting("login"); // Ne distingue pas compte inconnu et mauvais mot de passe.
        app.MapPost("/api/auth/logout", async (HttpContext ctx) => // Associe la requête HTTP POST /api/auth/logout aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await ctx.SignOutAsync(); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Efface le cookie de session du navigateur.
        app.MapGet("/api/me", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/me aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var roles = JsonSerializer.Deserialize<Guid[]>(a.User.Roles)!; // Calcule ou prépare la variable roles, utilisée par les instructions suivantes.
            return Results.Ok(new { a.User.Id, a.User.Username, a.User.StudentId, permissions = (await db.Roles.Where(r => roles.Contains(r.Id)).ToListAsync()).SelectMany(r => JsonSerializer.Deserialize<string[]>(r.Permissions)!).Distinct(), student = await db.Students.Where(s => s.Id == a.User.StudentId).Select(s => new { s.Id, s.Number, s.FirstName, s.LastName, s.Track, s.Status, s.Profile }).SingleOrDefaultAsync() }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Retourne l'identité et les permissions sans hachage de mot de passe.
        app.MapGet("/api/roles", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/roles aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("roles.manage"); // Exige la permission roles.manage contenue dans les rôles actuels du compte.
            return Results.Ok(await db.Roles.ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Liste les profils administrables.
        app.MapPost("/api/roles", async (JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/roles aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("roles.manage"); // Exige la permission roles.manage contenue dans les rôles actuels du compte.
            var r = new Role // Calcule ou prépare la variable r, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Name = j.Required("name"), // Renseigne libellé affiché aux utilisateurs avec la valeur calculée ou validée à droite.
                Permissions = j["permissions"]?.ToJsonString() ?? "[]" // Renseigne liste des capacités autorisées par le profil avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            ValidatePermissions(r.Permissions); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            db.Roles.Add(r); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("ROLE_CREATED", r.Id, new { r.Name, r.Permissions }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(r); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Crée un profil sans modifier le programme.
        app.MapPut("/api/roles/{id:guid}", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP PUT /api/roles/{id:guid} aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("roles.manage"); // Exige la permission roles.manage contenue dans les rôles actuels du compte.
            var r = await db.Roles.FindAsync(id) ?? throw new RuleException("Profil inconnu.", 404); // Charge depuis la base la variable r, utilisée par les instructions suivantes.
            if (JsonSerializer.Deserialize<Guid[]>(a.User.Roles)!.Contains(id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Modifiez les droits de votre propre profil depuis un autre compte administrateur."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            r.Name = j.Required("name"); // Met à jour libellé affiché aux utilisateurs de l’objet r.
            r.Permissions = j["permissions"]!.ToJsonString(); // Met à jour liste des capacités autorisées par le profil de l’objet r.
            ValidatePermissions(r.Permissions); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            a.Audit("ROLE_UPDATED", id, new { r.Name, r.Permissions }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(r); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Évite de retirer accidentellement tous les droits de son propre compte.
        app.MapGet("/api/users", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/users aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("users.manage"); // Exige la permission users.manage contenue dans les rôles actuels du compte.
            return Results.Ok(await db.Users.Select(u => new { u.Id, u.Username, u.StudentId, u.Active, u.Roles }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Aucun secret n'est retourné dans la liste des comptes.
        app.MapPost("/api/users", async (JsonObject j, PortalDb db, Access a, IDataProtectionProvider protection) => // Associe la requête HTTP POST /api/users aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("users.manage"); // Exige la permission users.manage contenue dans les rôles actuels du compte.
            var roles = j["roles"]!.Deserialize<Guid[]>()!; // Calcule ou prépare la variable roles, utilisée par les instructions suivantes.
            if (roles.Length == 0 || await db.Roles.CountAsync(r => roles.Contains(r.Id)) != roles.Distinct().Count()) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Profils invalides."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var u = new User // Calcule ou prépare la variable u, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Username = j.Required("username"), // Renseigne identifiant de connexion avec la valeur calculée ou validée à droite.
                Roles = JsonSerializer.Serialize(roles) // Renseigne liste des UUID de profils attribués au compte avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            var password = j.Text("password"); // Calcule ou prépare la variable password, utilisée par les instructions suivantes.
            if (password == "") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                password = Security.Secret(); // Renseigne le champ password avec la valeur calculée ou validée à droite.
            if (password.Length < 12) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Mot de passe : au moins 12 caractères."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var mfaSecret = Mfa.NewSecret(); // Calcule ou prépare la variable mfaSecret, utilisée par les instructions suivantes.
            u.MfaSecret = protection.CreateProtector("iai-totp").Protect(mfaSecret); // Met à jour secret TOTP chiffré pour le second facteur de l’objet u.
            u.PasswordHash = Security.Password(u, password); // Met à jour empreinte salée du mot de passe, jamais le mot de passe en clair de l’objet u.
            db.Users.Add(u); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("USER_CREATED", u.Id, new { u.Username }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { u.Id, u.Username, password, mfaSecret, otpUri = $"otpauth://totp/IAI:{Uri.EscapeDataString(u.Username)}?secret={mfaSecret}&issuer=IAI" });
        }); // Affiche le nouveau secret une seule fois.
        app.MapPut("/api/users/{id:guid}", async (Guid id, JsonObject j, PortalDb db, Access a, IDataProtectionProvider protection) => // Associe la requête HTTP PUT /api/users/{id:guid} aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("users.manage"); // Exige la permission users.manage contenue dans les rôles actuels du compte.
            if (id == a.User.Id) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Votre propre compte ne peut pas être désactivé ici."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var u = await db.Users.FindAsync(id) ?? throw new RuleException("Compte inconnu.", 404); // Charge depuis la base la variable u, utilisée par les instructions suivantes.
            var roles = j["roles"]!.Deserialize<Guid[]>()!; // Calcule ou prépare la variable roles, utilisée par les instructions suivantes.
            if (await db.Roles.CountAsync(r => roles.Contains(r.Id)) != roles.Distinct().Count()) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Profil inconnu."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            string? mfaSecret = null; // La promotion peut nécessiter un nouveau secret personnel.
            if (u.MfaSecret == "" && (await db.Roles.Where(r => roles.Contains(r.Id)).ToListAsync()).Any(r => JsonSerializer.Deserialize<string[]>(r.Permissions)!.Length > 0)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                mfaSecret = Mfa.NewSecret(); // Renseigne le champ mfaSecret avec la valeur calculée ou validée à droite.
                u.MfaSecret = protection.CreateProtector("iai-totp").Protect(mfaSecret); // Met à jour secret TOTP chiffré pour le second facteur de l’objet u.
            } // Prépare le second facteur avant d’attribuer les droits administratifs.

            u.Roles = JsonSerializer.Serialize(roles); // Met à jour liste des UUID de profils attribués au compte de l’objet u.
            u.Active = j["active"]!.GetValue<bool>(); // Met à jour état d’activation du compte de l’objet u.
            u.Version++; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            a.Audit("USER_UPDATED", id, new { u.Active, u.Roles }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { u.Username, mfaSecret, otpUri = mfaSecret == null ? null : $"otpauth://totp/IAI:{Uri.EscapeDataString(u.Username)}?secret={mfaSecret}&issuer=IAI" }); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Toute modification révoque les sessions du compte concerné.
        app.MapPost("/api/users/{id:guid}/reset-mfa", async (Guid id, PortalDb db, Access a, IDataProtectionProvider protection) => // Associe la requête HTTP POST /api/users/{id:guid}/reset-mfa aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("users.manage"); // Exige la permission users.manage contenue dans les rôles actuels du compte.
            if (id == a.User.Id) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("La récupération de votre MFA doit être effectuée depuis un autre compte administratif."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var u = await db.Users.FindAsync(id) ?? throw new RuleException("Compte inconnu."); // Charge depuis la base la variable u, utilisée par les instructions suivantes.
            var secret = Mfa.NewSecret(); // Calcule ou prépare la variable secret, utilisée par les instructions suivantes.
            u.MfaSecret = protection.CreateProtector("iai-totp").Protect(secret); // Met à jour secret TOTP chiffré pour le second facteur de l’objet u.
            u.MfaStep = -1; // Met à jour dernière fenêtre TOTP consommée pour empêcher son rejeu de l’objet u.
            u.Version++; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            a.Audit("MFA_RESET", id, new { u.Username }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { u.Username, mfaSecret = secret, otpUri = $"otpauth://totp/IAI:{Uri.EscapeDataString(u.Username)}?secret={secret}&issuer=IAI" });
        }); // Le nouveau second facteur est remis une seule fois sans révéler l’ancien secret.
        app.MapPost("/api/users/{id:guid}/reset-password", async (Guid id, JsonObject j, PortalDb db, Access a) => // Associe la requête HTTP POST /api/users/{id:guid}/reset-password aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("users.manage"); // Exige la permission users.manage contenue dans les rôles actuels du compte.
            var u = await db.Users.FindAsync(id) ?? throw new RuleException("Compte inconnu.", 404); // Charge depuis la base la variable u, utilisée par les instructions suivantes.
            var password = j.Text("password"); // Calcule ou prépare la variable password, utilisée par les instructions suivantes.
            if (password == "") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                password = Security.Secret(); // Renseigne le champ password avec la valeur calculée ou validée à droite.
            if (password.Length < 12) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Mot de passe trop court."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            u.PasswordHash = Security.Password(u, password); // Met à jour empreinte salée du mot de passe, jamais le mot de passe en clair de l’objet u.
            u.Version++; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            a.Audit("PASSWORD_RESET", id, new { u.Username }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { u.Username, password }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le journal conserve la réinitialisation mais jamais son secret.
        app.MapPost("/api/auth/activate", async (JsonObject j, PortalDb db, HttpContext ctx) => // Associe la requête HTTP POST /api/auth/activate aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            var hash = Security.Hash(j.Required("activationCode")); // Calcule ou prépare la variable hash, utilisée par les instructions suivantes.
            var s = await db.Students.SingleOrDefaultAsync(s => s.Number == j.Text("studentNumber") && s.ActivationHash == hash) ?? throw new RuleException("Activation invalide."); // Charge depuis la base la variable s, utilisée par les instructions suivantes.
            if (await db.Users.AnyAsync(u => u.StudentId == s.Id)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Compte déjà activé."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var role = await db.Roles.SingleAsync(r => r.SystemCode == "STUDENT"); // Charge depuis la base la variable role, utilisée par les instructions suivantes.
            var password = Security.Secret(); // Calcule ou prépare la variable password, utilisée par les instructions suivantes.
            var u = new User // Calcule ou prépare la variable u, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                Username = s.Number.ToLowerInvariant(), // Renseigne identifiant de connexion avec la valeur calculée ou validée à droite.
                StudentId = s.Id, // Renseigne identifiant du dossier étudiant avec la valeur calculée ou validée à droite.
                Roles = JsonSerializer.Serialize(new[] { role.Id }) // Renseigne liste des UUID de profils attribués au compte avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            u.PasswordHash = Security.Password(u, password); // Met à jour empreinte salée du mot de passe, jamais le mot de passe en clair de l’objet u.
            s.ActivationHash = ""; // Met à jour empreinte du secret personnel d’activation de l’objet s.
            s.Status = "ACTIVE"; // Met à jour état courant du dossier ou de l’opération de l’objet s.
            db.Users.Add(u); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            await SignIn(ctx, u); // Établit le cookie protégé du compte après vérification de son identité.
            return Results.Ok(new { u.Username, password }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }).RequireRateLimiting("login"); // L'activation personnelle à usage unique empêche l'usurpation par sélection d'un nom.
        app.MapGet("/api/audit", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/audit aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("audit.view"); // Exige la permission audit.view contenue dans les rôles actuels du compte.
            return Results.Ok(await db.Audits.OrderByDescending(x => x.Created).Take(500).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Le représentant autorisé peut consulter les actions récentes.
        app.MapGet("/api/notifications", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/notifications aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            return Results.Ok(await db.Notifications.Where(n => n.StudentId == a.User.StudentId).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Une notification ne peut pas être lue par un autre étudiant.
    } // Ferme ce bloc après l’exécution de ses instructions.

    static Task SignIn(HttpContext ctx, User u, bool mfaVerified = false) => ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("uid", u.Id.ToString()), new Claim("version", u.Version.ToString()), new Claim("mfa", mfaVerified ? "true" : "false") }, CookieAuthenticationDefaults.AuthenticationScheme))); // Le cookie ne contient aucune permission susceptible de devenir obsolète.
    static void ValidatePermissions(string json) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        var p = JsonSerializer.Deserialize<string[]>(json); // Calcule ou prépare la variable p, utilisée par les instructions suivantes.
        if (p == null || p.Any(x => !PermissionList.Contains(x))) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            throw new RuleException("Permission inconnue."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
    } // Valide le catalogue des capacités du logiciel.

    public static readonly string[] PermissionList = ["*", "roles.manage", "users.manage", "academic.manage", "academic.view", "students.manage", "students.view", "students.create", "students.update", "students.disable", "students.delete", "students.transfer", "students.reset_password", "grades.manage", "grades.view", "grades.import", "grades.update", "grades.publish", "grades.rescue", "attendance.manage", "attendance.reports", "discipline.manage", "documents.manage", "audit.view"]; // Les rôles assemblent librement ces capacités.
} // Ferme ce bloc après l’exécution de ses instructions.

