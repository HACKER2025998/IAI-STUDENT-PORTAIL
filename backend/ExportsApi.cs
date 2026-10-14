using System.Globalization; // Formate les notes indépendamment de la langue du serveur.
using System.IO.Compression; // Un classeur XLSX est une archive ZIP contenant des fichiers XML.
using System.Net; // Échappe les valeurs destinées au tableau PDF.
using System.Text; // Encode les exports CSV en UTF-8.
using System.Xml.Linq; // Construit le XML Excel sans concaténer des valeurs non échappées.
using Microsoft.EntityFrameworkCore; // Charge seulement les données autorisées.

namespace Iai; // Module des exports administratifs.
public static class ExportsApi // Les trois formats utilisent la même sélection de données.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Map(WebApplication app) // Installe le téléchargement des listes et rapports.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        app.MapGet("/api/exports", async (string kind, string format, Guid? classId, Guid? enrollmentId, int? semester, bool? annual, DateOnly? start, DateOnly? end, PortalDb db, Access a) => // Associe la requête HTTP GET /api/exports aux opérations métier du bloc suivant.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            await a.Auth(); // Chaque fichier exporté est protégé par une session active.
            var rows = new List<Dictionary<string, string>>(); // Prépare des colonnes lisibles et stables.
            if (kind == "students") // La liste des dossiers est réservée aux gestionnaires étudiants.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                await a.Require("students.view"); // Empêche la diffusion des identités à un étudiant ordinaire.
                var ids = classId == null ? null : await db.Enrollments.Where(e => e.ClassId == classId && e.Status != "TRANSFERRED").Select(e => e.StudentId).ToListAsync(); // Une classe peut limiter la liste exportée.
                foreach (var s in await db.Students.Where(s => ids == null || ids.Contains(s.Id)).ToListAsync()) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                    rows.Add(new() { ["Matricule"] = s.Number, ["Prénoms"] = s.FirstName, ["Nom"] = s.LastName, ["Parcours"] = s.Track, ["Statut"] = s.Status }); // Les contacts d'urgence ne sont pas mélangés au classement.
            } // Ferme ce bloc après l’exécution de ses instructions.
            else if (kind == "attendance") // Le rapport de présence accepte une période journalière ou hebdomadaire.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                await a.Require("attendance.reports"); // CPE, surveillant et responsables autorisés consultent le rapport.
                var records = await db.Attendances.Include(r => r.Student).Include(r => r.Session).Where(r => classId == null || r.Session.ClassId == classId).ToListAsync(); // Les dates sont ensuite comparées en UTC pour rester cohérentes entre fournisseurs.
                foreach (var r in records.Where(r => (start == null || DateOnly.FromDateTime(r.Checked.UtcDateTime) >= start) && (end == null || DateOnly.FromDateTime(r.Checked.UtcDateTime) <= end))) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                    rows.Add(new() { ["Matricule"] = r.Student.Number, ["Étudiant"] = r.Student.FirstName + " " + r.Student.LastName, ["Cours"] = r.Session.Course, ["Date UTC"] = r.Checked.ToString("u"), ["Statut"] = r.Status, ["Méthode"] = r.Method }); // L'absence justifiée conserve sa catégorie spécifique.
            } // Ferme ce bloc après l’exécution de ses instructions.
            else if (kind == "rankings") // Le classement applique le groupe académique de l'inscription choisie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                await a.Require("grades.view"); // L'export de toute une promotion est administratif.
                var e = await db.Enrollment(enrollmentId ?? throw new RuleException("Inscription requise.")); // Le groupe du classement vient de cette inscription.
                foreach (var r in await GradesApi.Rankings(db, e, semester ?? 1, annual == true, true)) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                    rows.Add(new() { ["Rang"] = r.Position.ToString(), ["Matricule"] = r.Number, ["Étudiant"] = r.Name, ["Moyenne"] = r.Average.ToString("0.00", CultureInfo.InvariantCulture), ["Effectif"] = r.Total.ToString() }); // Les ex æquo gardent la même position.
            } // Ferme ce bloc après l’exécution de ses instructions.
            else // Exécute le cas alternatif lorsque la condition précédente est fausse.
                throw new RuleException("Type d'export inconnu."); // Aucun accès arbitraire à une table n'est autorisé.
            if (rows.Count == 0) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                throw new RuleException("Aucune donnée dans cette sélection."); // Évite un fichier vide et sans en-têtes interprétables.
            var headers = rows[0].Keys.ToList(); // Toutes les lignes partagent les mêmes colonnes.
            if (format == "csv") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                return Results.File(Encoding.UTF8.GetBytes(string.Join("\r\n", new[] { string.Join(',', headers.Select(CsvCell)) }.Concat(rows.Select(r => string.Join(',', headers.Select(h => CsvCell(r[h]))))))), "text/csv", kind + ".csv"); // Les cellules sont protégées contre l'injection de formules.
            if (format == "xlsx") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
                return Results.File(Excel(headers, rows), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", kind + ".xlsx"); // Produit un vrai classeur Office Open XML.
            if (format == "pdf") // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var html = "<html lang='fr'><meta charset='utf-8'><style>body{font:11px Arial}table{border-collapse:collapse;width:100%}td,th{border:1px solid #aaa;padding:6px}</style><h1>IAI Togo — " + WebUtility.HtmlEncode(kind) + "</h1><table><thead><tr>" + string.Concat(headers.Select(h => "<th>" + WebUtility.HtmlEncode(h) + "</th>")) + "</tr></thead><tbody>" + string.Concat(rows.Select(r => "<tr>" + string.Concat(headers.Select(h => "<td>" + WebUtility.HtmlEncode(r[h]) + "</td>")) + "</tr>")) + "</tbody></table></html>"; // Calcule ou prépare la variable html, utilisée par les instructions suivantes.
                return Results.File(await PdfRenderer.Render(html), "application/pdf", kind + ".pdf"); // Renvoie les octets ou le fichier autorisé avec son type de contenu et son nom de téléchargement.
            } // Imprime exactement le même jeu de données que les exports CSV et XLSX.

            throw new RuleException("Format attendu : csv, xlsx ou pdf."); // Les formats sont explicitement limités.
        }); // Termine la définition de l’objet ou du gestionnaire de route.
    } // Ferme ce bloc après l’exécution de ses instructions.

    static string CsvCell(string text) => "\"" + (text.Length > 0 && "=+-@".Contains(text[0]) ? "'" + text : text).Replace("\"", "\"\"") + "\""; // Les guillemets sont doublés et les formules deviennent du texte.
    static byte[] Excel(List<string> headers, List<Dictionary<string, string>> rows) // Construit un classeur minimal compatible avec Excel et LibreOffice.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        using var memory = new MemoryStream(); // L'export ne crée pas de fichier temporaire public.
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true)) // Ferme l'archive avant de lire son contenu complet.
        { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
            Write(zip, "[Content_Types].xml", "<?xml version='1.0'?><Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'><Default Extension='rels' ContentType='application/vnd.openxmlformats-package.relationships+xml'/><Default Extension='xml' ContentType='application/xml'/><Override PartName='/xl/workbook.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml'/><Override PartName='/xl/worksheets/sheet1.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml'/></Types>"); // Déclare les parties nécessaires au format XLSX.
            Write(zip, "_rels/.rels", "<?xml version='1.0'?><Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument' Target='xl/workbook.xml'/></Relationships>"); // La racine de l'archive référence le classeur.
            Write(zip, "xl/workbook.xml", "<?xml version='1.0'?><workbook xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><sheets><sheet name='Rapport IAI' sheetId='1' r:id='rId1'/></sheets></workbook>"); // Crée une feuille au nom stable.
            Write(zip, "xl/_rels/workbook.xml.rels", "<?xml version='1.0'?><Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet' Target='worksheets/sheet1.xml'/></Relationships>"); // Relie la feuille à son fichier XML.
            XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main"; // Les cellules appartiennent au namespace du standard Excel.
            var data = new XElement(ns + "sheetData"); // Contient les lignes du tableau exporté.
            foreach (var values in new[] // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                headers // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
            }.Concat(rows.Select(r => headers.Select(h => r[h]).ToList()))) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
                data.Add(new XElement(ns + "row", values.Select(v => new XElement(ns + "c", new XAttribute("t", "inlineStr"), new XElement(ns + "is", new XElement(ns + "t", v)))))); // Les valeurs sont des chaînes XML échappées, jamais des formules exécutables.
            Write(zip, "xl/worksheets/sheet1.xml", new XDocument(new XElement(ns + "worksheet", data)).ToString()); // Écrit les données de la feuille unique.
        } // Ferme ce bloc après l’exécution de ses instructions.

        return memory.ToArray(); // Renvoie les octets de l'archive complète à ASP.NET.
    } // Ferme ce bloc après l’exécution de ses instructions.

    static void Write(ZipArchive zip, string name, string text) // Déclare cette opération avec les paramètres et le résultat indiqués par sa signature.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        using var writer = new StreamWriter(zip.CreateEntry(name).Open(), new UTF8Encoding(false)); // Crée une ressource qui sera automatiquement libérée à la fin du bloc.
        writer.Write(text); // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    } // Chaque partie de l'archive est encodée en UTF-8 sans BOM.
} // Ferme ce bloc après l’exécution de ses instructions.
