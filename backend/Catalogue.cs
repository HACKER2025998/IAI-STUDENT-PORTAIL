using System.Text.Json.Nodes; // Lit les versions figées qui définissent la hiérarchie pédagogique.

namespace Iai; // Module du référentiel relationnel.
public class CurriculumUnit : Entity // Une UE appartient à une version de programme et à un semestre.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid CurriculumId { get; set; } // Relie l'UE à la version immuable du cursus.
    public Curriculum Curriculum { get; set; } = null !; // Navigation relationnelle vers le cursus.
    public int Semester { get; set; } // Le numéro de semestre est une donnée pédagogique.
    public string Code { get; set; } = ""; // Le code imprimé reste une donnée métier potentiellement répétée.
    public string Name { get; set; } = ""; // Le nom est celui de la fiche transcrite ou approuvée.
    public decimal? Credits { get; set; } // Null signale les crédits absents des photos ITI.
} // Ferme ce bloc après l’exécution de ses instructions.

public class CurriculumElement : Entity // Un EC est la matière sur laquelle porte une note.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid UnitId { get; set; } // La clé étrangère garantit l'appartenance à une UE réelle.
    public CurriculumUnit Unit { get; set; } = null !; // Permet de remonter de la matière au programme.
    public string Code { get; set; } = ""; // Deux EC peuvent avoir le même code officiel mais jamais le même UUID.
    public string Name { get; set; } = ""; // Le libellé est conservé indépendamment du code.
    public decimal Coefficient { get; set; } // Le coefficient fournit le poids utilisé par les calculs.
    public int Cm { get; set; } // Volume des cours magistraux en heures.
    public int Td { get; set; } // Volume des travaux dirigés en heures.
    public int Tp { get; set; } // Volume des travaux pratiques en heures.
} // Ferme ce bloc après l’exécution de ses instructions.

public static class Catalogue // Maintient les références SQL de la version pédagogique.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public static void Attach(PortalDb db, Curriculum c) // Chaque version est insérée avec ses propres UE et EC.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        foreach (var semester in JsonNode.Parse(c.Content)!["semesters"]!.AsArray()) // Parcourt les deux semestres de la version.
            foreach (var unit in semester!["ues"]!.AsArray()) // Parcourt les unités d'enseignement du semestre.
            { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                var u = unit!; // Les objets de programme sont validés avant l'insertion.
                var entity = new CurriculumUnit // Calcule ou prépare la variable entity, utilisée par les instructions suivantes.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    Id = Guid.Parse((string)u["id"]!), // Renseigne identifiant technique UUID avec la valeur calculée ou validée à droite.
                    CurriculumId = c.Id, // Renseigne identifiant de la version du programme avec la valeur calculée ou validée à droite.
                    Semester = (int)semester["number"]!, // Renseigne numéro de semestre concerné avec la valeur calculée ou validée à droite.
                    Code = (string)u["code"]!, // Renseigne code métier imprimé ou saisi par le gestionnaire avec la valeur calculée ou validée à droite.
                    Name = (string)u["name"]!, // Renseigne libellé affiché aux utilisateurs avec la valeur calculée ou validée à droite.
                    Credits = u["credits"]?.GetValue<decimal>() // Renseigne nombre de crédits de l’UE, inconnu lorsque la photo ne le précise pas avec la valeur calculée ou validée à droite.
                }; // Crée la référence SQL sans changer les données photographiées.
                db.Set<CurriculumUnit>().Add(entity); // Prépare la clé étrangère vers la version de programme.
                foreach (var element in u["ecs"]!.AsArray()) // Ajoute les matières de cette UE.
                { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
                    var e = element!; // Chaque EC possède son UUID technique indépendant.
                    db.Set<CurriculumElement>().Add(new() { Id = Guid.Parse((string)e["id"]!), UnitId = entity.Id, Code = (string)e["code"]!, Name = (string)e["name"]!, Coefficient = (decimal)e["coefficient"]!, Cm = (int)e["cm"]!, Td = (int)e["td"]!, Tp = (int)e["tp"]! }); // Conserve les heures et coefficients dans les colonnes relationnelles.
                } // Ferme ce bloc après l’exécution de ses instructions.
            } // Ferme ce bloc après l’exécution de ses instructions.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
