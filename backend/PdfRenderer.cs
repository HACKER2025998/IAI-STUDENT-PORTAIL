using Microsoft.Playwright; // Pilote Chromium pour imprimer le même modèle HTML en PDF.

namespace Iai; // Module de production des relevés.
public static class PdfRenderer // Utilise un navigateur isolé sans requête vers des ressources externes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static async Task<byte[]> Render(string html) // Convertit le relevé déjà échappé et autorisé en fichier PDF.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        using var playwright = await Playwright.CreateAsync(); // Ouvre le pilote officiel du navigateur.
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true, Args = ["--no-sandbox"] }); // Exécute Chromium dans le conteneur dédié au backend.
        var page = await browser.NewPageAsync(); // Un onglet indépendant est créé pour ce relevé.
        await page.RouteAsync("**/*", route => route.AbortAsync()); // Aucun document externe n'est téléchargé pendant le rendu.
        await page.SetContentAsync(html); // Charge seulement le HTML généré par le serveur.
        return await page.PdfAsync(new() { Format = "A4", PrintBackground = true, Margin = new() { Top = "15mm", Bottom = "15mm", Left = "12mm", Right = "12mm" } }); // Produit des pages A4 prêtes à remettre à l'étudiant.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
