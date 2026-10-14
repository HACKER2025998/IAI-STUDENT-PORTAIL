using Iai; // Importe les modules métier du portail.
using Microsoft.EntityFrameworkCore; // Configure la connexion à la base relationnelle.
using Microsoft.AspNetCore.Authentication.Cookies; // Authentifie par cookie protégé et HttpOnly.
using System.Threading.RateLimiting; // Limite les tentatives automatisées de connexion.
using System.Text.Json; // Uniformise les réponses JSON.
using Microsoft.AspNetCore.DataProtection; // Persiste les clés de chiffrement des cookies.

var builder = WebApplication.CreateBuilder(args); // Construit l'application et charge les variables d'environnement.
builder.Services.AddDbContext<PortalDb>(o => // Configure ce composant du serveur ou son comportement HTTP avant de recevoir les requêtes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    var c = builder.Configuration.GetConnectionString("Portal"); // Calcule ou prépare la variable c, utilisée par les instructions suivantes.
    if (c is null) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
        o.UseSqlite("Data Source=portal.db"); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    else // Exécute le cas alternatif lorsque la condition précédente est fausse.
        o.UseNpgsql(c); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
}); // PostgreSQL en déploiement ; SQLite permet le lancement autonome.
builder.Services.AddHttpContextAccessor(); // Rend le contexte HTTP accessible au contrôle des droits.
builder.Services.AddScoped<Access>(); // Chaque requête possède son propre service d'autorisation.
builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys"))); // Les cookies restent vérifiables après un redémarrage.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o => // Configure ce composant du serveur ou son comportement HTTP avant de recevoir les requêtes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    o.Cookie.Name = "iai_session"; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    o.Cookie.HttpOnly = true; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    o.Cookie.SameSite = SameSiteMode.Strict; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    o.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    o.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Met à jour la propriété ExpireTimeSpan de l’objet o.
    o.SlidingExpiration = false; // Met à jour la propriété SlidingExpiration de l’objet o.
    o.Events.OnRedirectToLogin = c => // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        c.Response.StatusCode = 401; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        return Task.CompletedTask; // Remet au code appelant la valeur construite par cette expression.
    }; // Termine cette construction et revient à l’instruction qui l’utilise.
}); // Une session dure trente minutes et ne fuit pas dans JavaScript.
builder.Services.AddRateLimiter(o => // Configure ce composant du serveur ou son comportement HTTP avant de recevoir les requêtes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    o.RejectionStatusCode = 429; // Met à jour la propriété RejectionStatusCode de l’objet o.
    o.AddPolicy("login", c => RateLimitPartition.GetFixedWindowLimiter(c.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new() { PermitLimit = builder.Environment.IsDevelopment() ? builder.Configuration.GetValue<int?>("Authentication:RateLimit") ?? 10 : 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 })); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
}); // Limite par adresse les essais de mots de passe et d'activation.
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase); // Le frontend reçoit des noms de propriétés cohérents.
var app = builder.Build(); // Termine la construction des services.
if (args.Contains("--schema")) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    using var schemaScope = app.Services.CreateScope(); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
    Console.WriteLine(schemaScope.ServiceProvider.GetRequiredService<PortalDb>().Database.GenerateCreateScript()); // Affiche ce résultat de diagnostic ou de vérification dans la sortie du processus.
    return; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
} // Exporte le schéma SQL sans se connecter ni créer de compte.

using (var scope = app.Services.CreateScope()) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    await Bootstrap.Initialize(scope.ServiceProvider.GetRequiredService<PortalDb>(), app.Configuration, scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>(), app.Environment); // Crée le schéma et les programmes de référence au premier démarrage.
app.Use(async (ctx, next) => // Configure ce composant du serveur ou son comportement HTTP avant de recevoir les requêtes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    try // Encadre les opérations qui peuvent échouer afin de traiter leurs erreurs proprement.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        await next(); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
    } // Ferme ce bloc après l’exécution de ses instructions.
    catch (RuleException e) // Intercepte le type d’erreur indiqué et applique la réponse prévue pour ce cas.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        ctx.Response.StatusCode = e.Status; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        await ctx.Response.WriteAsJsonAsync(new { error = e.Message }); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
    } // Ferme ce bloc après l’exécution de ses instructions.
    catch (DbUpdateException) // Intercepte le type d’erreur indiqué et applique la réponse prévue pour ce cas.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        ctx.Response.StatusCode = 409; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        await ctx.Response.WriteAsJsonAsync(new { error = "Conflit de données : vérifiez les doublons et les références." }); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
    } // Ferme ce bloc après l’exécution de ses instructions.
}); // Les erreurs métier n'exposent ni SQL ni secrets.
app.Use(async (ctx, next) => // Configure ce composant du serveur ou son comportement HTTP avant de recevoir les requêtes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    ctx.Response.Headers["X-Content-Type-Options"] = "nosniff"; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    ctx.Response.Headers["Referrer-Policy"] = "same-origin"; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    if (ctx.Request.Path.StartsWithSegments("/api") && !HttpMethods.IsGet(ctx.Request.Method) && ctx.Request.Headers["X-Portal-Request"] != "1") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        ctx.Response.StatusCode = 403; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
        await ctx.Response.WriteAsJsonAsync(new { error = "En-tête de protection CSRF requis." }); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
        return; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    } // Ferme ce bloc après l’exécution de ses instructions.

    await next(); // Attend la fin de cette opération asynchrone sans bloquer un thread pendant les entrées-sorties.
}); // Les mutations exigent un en-tête que les formulaires d'un site tiers ne peuvent pas envoyer.
app.UseRateLimiter(); // Active la limitation des routes publiques sensibles.
app.UseAuthentication(); // Déchiffre le cookie avant les contrôles métier.
app.MapGet("/health", () => Results.Ok(new { status = "ok" })); // Permet de vérifier la disponibilité du processus.
IdentityApi.Map(app); // Installe les routes de connexion et de gestion des comptes.
ProgressApi.Map(app); // Affiche les crédits capitalisés et les équivalences approuvées.
AcademicApi.Map(app); // Installe années, classes, programmes et inscriptions.
GradesApi.Map(app); // Installe imports, calculs, publications et classements.
AttendanceImportApi.Map(app); // Prévisualise et importe les listes de présence administratives.
AttendanceApi.Map(app); // Installe présences, délégations, justificatifs et discipline.
ExportsApi.Map(app); // Télécharge les listes et rapports en CSV, XLSX et PDF.
DocumentsApi.Map(app); // Installe la médiathèque privée et ses validations.
app.Run(); // Écoute les requêtes HTTP jusqu'à l'arrêt du serveur.
public partial class Program // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
} // Rend le point d'entrée accessible aux tests d'intégration.

