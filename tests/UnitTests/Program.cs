using Iai; // Importe le moteur de calcul réellement utilisé par les routes.
using System.Text.Json.Nodes; // Prépare une version minimale de programme pour les tests.

void Assert(bool condition, string name) // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    if (!condition) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
        throw new Exception("Échec : " + name); // Interrompt l’opération et renvoie l’erreur métier ou de configuration décrite par ce message.
    Console.WriteLine("OK : " + name); // Affiche ce résultat de diagnostic ou de vérification dans la sortie du processus.
} // Une assertion fausse arrête le processus avec un code d'erreur.

var ec1 = Guid.NewGuid(); // Identifie le premier EC indépendamment de son code imprimé.
var ec2 = Guid.NewGuid(); // Identifie le deuxième EC indépendamment de son nom.
var ue = Guid.NewGuid(); // L'UE possède également une clé technique permanente dans sa version.
var content = new JsonObject // Calcule ou prépare la variable content, utilisée par les instructions suivantes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    ["threshold"] = 10, // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
    ["semesters"] = new JsonArray(new JsonObject { ["number"] = 1, ["ues"] = new JsonArray(new JsonObject { ["id"] = ue.ToString(), ["code"] = "UE", ["name"] = "Unité", ["credits"] = 6, ["ecs"] = new JsonArray(new JsonObject { ["id"] = ec1.ToString(), ["code"] = "EC", ["name"] = "Premier", ["coefficient"] = 3 }, new JsonObject { ["id"] = ec2.ToString(), ["code"] = "EC", ["name"] = "Second", ["coefficient"] = 2 }) }) }) // Ajoute cette valeur littérale à la liste de choix ou de données en cours de construction.
}; // Le code EC est volontairement dupliqué pour vérifier le recours aux UUID.
var enrollment = new Enrollment // Calcule ou prépare la variable enrollment, utilisée par les instructions suivantes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    Curriculum = new Curriculum // Renseigne le champ Curriculum avec la valeur calculée ou validée à droite.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        Content = content.ToJsonString() // Renseigne contenu figé et sérialisé de la version ou du relevé avec la valeur calculée ou validée à droite.
    } // Ferme ce bloc après l’exécution de ses instructions.
}; // Le test fixe la version pédagogique attachée à l'inscription.
var notes = new[] // Calcule ou prépare la variable notes, utilisée par les instructions suivantes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    new Grade // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        EcId = ec1, // Renseigne identifiant de la matière avec la valeur calculée ou validée à droite.
        Value = 14 // Renseigne valeur décimale de la note sur vingt avec la valeur calculée ou validée à droite.
    }, // Termine cette construction et revient à l’instruction qui l’utilise.
    new Grade // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        EcId = ec2, // Renseigne identifiant de la matière avec la valeur calculée ou validée à droite.
        Value = 10 // Renseigne valeur décimale de la note sur vingt avec la valeur calculée ou validée à droite.
    } // Ferme ce bloc après l’exécution de ses instructions.
}; // Prépare l'exemple pondéré du cahier des charges.
var result = GradeEngine.Calculate(enrollment, 1, notes, []); // Calcule le résultat sur les coefficients 3 et 2.
Assert(result.Average == 12.4m && result.Awarded == 6, "moyenne pondérée 12,4 et crédits validés"); // Vérifie la formule complète et l'attribution de crédits.
Assert(GradeEngine.Calculate(enrollment, 1, notes.Take(1), []).Average == null, "note manquante : aucune moyenne officielle"); // Une cellule vide n'est jamais assimilée à zéro.
notes[0].Value = 9; // Poursuit cette expression avec les paramètres, valeurs ou opérations explicitement indiqués.
notes[1].Value = 9; // Place l'UE au-dessous du seuil normal de validation.
var rescued = GradeEngine.Calculate(enrollment, 1, notes, [new Rescue { UeId = ue }]); // Applique une décision de jury distincte des notes.
Assert(rescued.Average == 9 && rescued.Ues[0].Status == "VALIDATED_BY_RESCUE" && rescued.Awarded == 6, "repêchage sans changement de note"); // Contrôle que la moyenne n'a pas été artificiellement remontée.
var carried = GradeEngine.Calculate(enrollment, 1, [], [], rescued.Ues); // Le redoublant réutilise ses notes publiées de la même version.
Assert(carried.Complete && carried.Average == 9 && carried.Ues[0].Status == "VALIDATED_PREVIOUSLY", "capitalisation des UE acquises avec leurs vraies notes"); // Le semestre reste calculable sans repasser une UE acquise.
Assert(GradeEngine.Discipline([-1m, -2m]) == 17 && GradeEngine.Discipline([-30m]) == 0 && GradeEngine.Discipline([5m]) == 20, "discipline bornée entre zéro et vingt"); // Vérifie l'accumulation et les bornes du registre.
Assert(Mfa.Code("GEZDGNBVGY3TQOJQGEZDGNBVGY3TQOJQ", 1) == "287082", "vecteur TOTP RFC 6238 à six chiffres"); // Le vecteur indépendant du standard prouve le calcul du second facteur.
var session = new CourseSession // Calcule ou prépare la variable session, utilisée par les instructions suivantes.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    Secret = Security.Secret(), // Renseigne clé secrète de signature du code dynamique du cours avec la valeur calculée ou validée à droite.
    Opened = DateTimeOffset.FromUnixTimeSeconds(1_000_000) // Renseigne date UTC d’ouverture du cours avec la valeur calculée ou validée à droite.
}; // Une séance déterministe permet de tester les frontières de fenêtres.
var now = session.Opened.AddSeconds(30); // Fixe un instant après le début du cours.
Assert(Security.CodeValid(session, Security.Code(session, now), now), "code de présence courant accepté"); // Le code récent est valide dans sa séance.
Assert(!Security.CodeValid(session, Security.Code(session, now), now.AddMinutes(5)), "code expiré refusé"); // Le même code ne fonctionne plus cinq minutes après.
session.Closed = now; // Ferme la séance sans modifier son secret.
Assert(!Security.CodeValid(session, Security.Code(session, now), now), "séance fermée : code refusé"); // La fermeture prime sur la validité cryptographique.
Console.WriteLine("Tous les tests métier sont passés."); // Le résumé est émis seulement si toutes les assertions ont réussi.
var secondEc = new EcView(Guid.NewGuid(), "E2", "Autre semestre", 1, 20); // Un second semestre plus léger distingue les méthodes annuelles.
var secondUnit = new UeView(Guid.NewGuid(), "U2", "UE annuelle", 20, "VALIDATED", 6, 6, [secondEc]); // Le semestre possède une moyenne de vingt avec un coefficient total de un.
var second = new ResultView(enrollment.Id, 2, 20, 6, 6, true, [secondUnit]); // Prépare le résultat de l'autre semestre.
Assert(GradeEngine.Annual([result, second], "SEMESTER_EQUAL_WEIGHT") == 16.2m, "moyenne annuelle à poids semestriels égaux"); // Vérifie (12,4 + 20) / 2.
Assert(GradeEngine.Annual([result, second], "ALL_EC_WEIGHTED") == 82m / 6m, "moyenne annuelle pondérée sur tous les EC"); // Vérifie le numérateur 62 + 20 et le poids 5 + 1.
Assert(GradesApi.Headers(enrollment.Curriculum, 1).Count == 2 && GradesApi.Headers(enrollment.Curriculum, 1).Keys.All(k => k.StartsWith("EC__")), "colonnes CSV distinctes pour codes imprimés dupliqués"); // Les deux UUID restent indépendants dans le modèle d'import.

