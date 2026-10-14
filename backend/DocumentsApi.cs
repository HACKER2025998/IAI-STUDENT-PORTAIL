using System.Security.Cryptography; // Calcule les empreintes des fichiers conservés.
using System.Text.Json; // Conserve les filtres pédagogiques en JSON.
using Microsoft.EntityFrameworkCore; // Interroge les inscriptions pour autoriser les téléchargements.

namespace Iai; // Module Documents.
public static class DocumentsApi // Stocke les fichiers hors du dossier public du serveur.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    static readonly Dictionary<string, string> Types = new() // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        [".pdf"] = "application/pdf", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        [".png"] = "image/png", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        [".jpg"] = "image/jpeg", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        [".jpeg"] = "image/jpeg", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
    }; // Les extensions exécutables et les archives arbitraires ne sont pas acceptées.
    public static void Map(WebApplication app) // Installe le catalogue et le téléchargement privé.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapGet("/api/documents", async (PortalDb db, Access a) => // Associe la requête HTTP GET /api/documents aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var all = await a.Can("documents.manage") || await a.Can("attendance.manage"); // Calcule ou prépare la variable all, utilisée par les instructions suivantes.
            var classes = await db.Enrollments.Where(e => e.StudentId == a.User.StudentId).Select(e => e.ClassId).ToListAsync(); // Charge depuis la base la variable classes, utilisée par les instructions suivantes.
            return Results.Ok(await db.Documents.Where(d => all || d.OwnerStudentId == a.User.StudentId || d.Category != "JUSTIFICATION" && d.Approved && d.ClassId != null && classes.Contains(d.ClassId.Value)).Select(d => new { d.Id, d.Title, d.Category, d.Metadata, d.ClassId, d.Filename, d.Approved, d.OwnerStudentId }).ToListAsync()); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }); // Les justificatifs privés ne deviennent jamais des documents pédagogiques.
        app.MapPost("/api/documents", async (HttpRequest request, PortalDb db, Access a, IWebHostEnvironment env) => // Associe la requête HTTP POST /api/documents aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var form = await request.ReadFormAsync(); // Calcule ou prépare la variable form, utilisée par les instructions suivantes.
            var file = form.Files.GetFile("file") ?? throw new RuleException("Fichier requis."); // Calcule ou prépare la variable file, utilisée par les instructions suivantes.
            var category = form["category"].ToString(); // Calcule ou prépare la variable category, utilisée par les instructions suivantes.
            var categories = new[] // Calcule ou prépare la variable categories, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                "COURSE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "TD", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "TP", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "DEVOIR", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "EXAMEN", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "RATTRAPAGE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "CORRIGE", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "SUPPORT", // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
                "JUSTIFICATION" // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            if (!categories.Contains(category)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Catégorie inconnue."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var classId = Guid.TryParse(form["classId"], out var c) ? c : (Guid? )null; // Calcule ou prépare la variable classId, utilisée par les instructions suivantes.
            var manager = await a.Can("documents.manage"); // Calcule ou prépare la variable manager, utilisée par les instructions suivantes.
            if (category == "JUSTIFICATION") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                if (a.User.StudentId == null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                    throw new RuleException("Compte étudiant requis."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            } // Ferme ce bloc après l’exécution de ses instructions.
            else if (classId == null || !manager && !await a.Officer(classId.Value)) // Teste ce cas uniquement lorsque le cas précédent n’a pas été retenu.
                throw new RuleException("Téléversement non autorisé.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            if (classId != null && !await db.Classes.AnyAsync(c => c.Id == classId)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Classe inconnue."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant(); // Calcule ou prépare la variable ext, utilisée par les instructions suivantes.
            if (!Types.TryGetValue(ext, out var mime) || file.Length <= 0 || file.Length > 10 * 1024 * 1024) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Format non autorisé ou fichier supérieur à 10 Mo."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            using var memory = new MemoryStream(); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
            await file.CopyToAsync(memory); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
            var bytes = memory.ToArray(); // Calcule ou prépare la variable bytes, utilisée par les instructions suivantes.
            var valid = ext == ".pdf" ? bytes.AsSpan().StartsWith("%PDF-"u8) : ext == ".png" ? bytes.AsSpan().StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) : ext is ".jpg" or ".jpeg" ? bytes.AsSpan().StartsWith(new byte[] { 255, 216, 255 }) : bytes.AsSpan().StartsWith("PK"u8); // Calcule ou prépare la variable valid, utilisée par les instructions suivantes.
            if (!valid) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Le contenu ne correspond pas au format annoncé."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            var key = Guid.NewGuid().ToString("N") + ext; // Calcule ou prépare la variable key, utilisée par les instructions suivantes.
            var root = Path.Combine(env.ContentRootPath, "storage"); // Calcule ou prépare la variable root, utilisée par les instructions suivantes.
            Directory.CreateDirectory(root); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            await File.WriteAllBytesAsync(Path.Combine(root, key), bytes); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
            var d = new Document // Calcule ou prépare la variable d, utilisée par les instructions suivantes.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                ClassId = classId, // Renseigne identifiant de la classe physique avec la valeur calculée ou validée à droite.
                OwnerStudentId = a.User.StudentId, // Renseigne titulaire de la pièce jointe personnelle avec la valeur calculée ou validée à droite.
                Title = form["title"].ToString(), // Renseigne titre lisible du document avec la valeur calculée ou validée à droite.
                Category = category, // Renseigne nature pédagogique ou justificative du fichier avec la valeur calculée ou validée à droite.
                Filename = Path.GetFileName(file.FileName), // Renseigne nom du fichier présenté au téléchargement avec la valeur calculée ou validée à droite.
                StorageKey = key, // Renseigne nom technique généré côté serveur pour le stockage privé avec la valeur calculée ou validée à droite.
                Mime = mime, // Renseigne type de contenu HTTP autorisé avec la valeur calculée ou validée à droite.
                Hash = Convert.ToHexString(SHA256.HashData(bytes)), // Renseigne empreinte SHA-256 du contenu conservé avec la valeur calculée ou validée à droite.
                Metadata = form["metadata"].ToString()is { Length: > 0 } metadata ? JsonDocument.Parse(metadata).RootElement.GetRawText() : "{}", // Renseigne classement du document par année, niveau et matière avec la valeur calculée ou validée à droite.
                Approved = manager // Renseigne accord administratif avant diffusion du document avec la valeur calculée ou validée à droite.
            }; // Termine cette construction et revient à l’instruction qui l’utilise.
            if (d.Title.Length == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Titre requis."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            db.Documents.Add(d); // Ajoute cet objet au contexte EF ; l’écriture SQL aura lieu lors de SaveChangesAsync.
            a.Audit("DOCUMENT_UPLOADED", d.Id, new { d.Title, d.Category, d.Hash }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.Ok(new { d.Id, d.Approved }); // Confirme la réussite avec HTTP 200 et ne renvoie que les données sélectionnées ici.
        }).DisableAntiforgery(); // La protection CSRF par en-tête est déjà appliquée globalement, y compris aux formulaires multipart.
        app.MapPost("/api/documents/{id:guid}/approve", async (Guid id, PortalDb db, Access a) => // Associe la requête HTTP POST /api/documents/{id:guid}/approve aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            await a.Require("documents.manage"); // Exige la permission documents.manage contenue dans les rôles actuels du compte.
            var d = await db.Documents.FindAsync(id) ?? throw new RuleException("Document inconnu."); // Charge depuis la base la variable d, utilisée par les instructions suivantes.
            if (d.Category == "JUSTIFICATION") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Utilisez la décision sur le justificatif."); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            d.Approved = true; // Met à jour accord administratif avant diffusion du document de l’objet d.
            a.Audit("DOCUMENT_APPROVED", id, new { d.Title }); // Prépare le journal de cette décision avec son auteur, la ressource et les données de changement.
            await db.SaveChangesAsync(); // Enregistre en base les objets ajoutés ou modifiés et le journal préparés par cette opération.
            return Results.NoContent(); // Confirme la réussite avec HTTP 204, sans corps de réponse.
        }); // Un téléversement étudiant doit être validé avant diffusion.
        app.MapGet("/api/documents/{id:guid}/download", async (Guid id, PortalDb db, Access a, IWebHostEnvironment env) => // Associe la requête HTTP GET /api/documents/{id:guid}/download aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Vérifie le cookie, le compte actif et la version de session avant de lire ou modifier des données.
            var d = await db.Documents.FindAsync(id) ?? throw new RuleException("Document inconnu.", 404); // Charge depuis la base la variable d, utilisée par les instructions suivantes.
            var manager = await a.Can("documents.manage") || await a.Can("attendance.manage"); // Calcule ou prépare la variable manager, utilisée par les instructions suivantes.
            var member = d.ClassId != null && await db.Enrollments.AnyAsync(e => e.ClassId == d.ClassId && e.StudentId == a.User.StudentId); // Charge depuis la base la variable member, utilisée par les instructions suivantes.
            if (!manager && d.OwnerStudentId != a.User.StudentId && !(d.Category != "JUSTIFICATION" && d.Approved && member)) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Document privé.", 403); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
            return Results.File(Path.Combine(env.ContentRootPath, "storage", d.StorageKey), d.Mime, d.Filename); // Renvoie les octets ou le fichier autorisé avec son type de contenu et son nom de téléchargement.
        }); // Le chemin physique est construit depuis une clé serveur, pas depuis le nom fourni par l'utilisateur.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
