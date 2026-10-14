using Microsoft.EntityFrameworkCore; // Fournit l'ORM qui traduit les opérations C# en SQL.

namespace Iai; // Regroupe les types métier du portail.
public abstract class Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid Id { get; set; } = Guid.NewGuid(); // Conserve identifiant technique UUID ; get lit sa valeur et set permet de l’affecter.
} // Chaque objet a un UUID indépendant de son code imprimé.

public class User : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public string MfaSecret { get; set; } = ""; // Conserve secret TOTP chiffré pour le second facteur ; get lit sa valeur et set permet de l’affecter.
    public long MfaStep { get; set; } = -1; // Conserve dernière fenêtre TOTP consommée pour empêcher son rejeu ; get lit sa valeur et set permet de l’affecter.
    public string Username { get; set; } = ""; // Conserve identifiant de connexion ; get lit sa valeur et set permet de l’affecter.
    public string PasswordHash { get; set; } = ""; // Conserve empreinte salée du mot de passe, jamais le mot de passe en clair ; get lit sa valeur et set permet de l’affecter.
    public Guid? StudentId { get; set; } // Conserve identifiant du dossier étudiant ; get lit sa valeur et set permet de l’affecter.
    public bool Active { get; set; } = true; // Conserve état d’activation du compte ; get lit sa valeur et set permet de l’affecter.
    public int Version { get; set; } // Conserve numéro de version utilisé pour conserver ou révoquer un état ; get lit sa valeur et set permet de l’affecter.
    public string Roles { get; set; } = "[]"; // Conserve liste des UUID de profils attribués au compte ; get lit sa valeur et set permet de l’affecter.
} // Le secret est haché ; Version permet la révocation immédiate des cookies.

public class Role : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public string SystemCode { get; set; } = ""; // Le rôle étudiant est reconnu par un code stable, même si son libellé est renommé.
    public string Name { get; set; } = ""; // Conserve libellé affiché aux utilisateurs ; get lit sa valeur et set permet de l’affecter.
    public string Permissions { get; set; } = "[]"; // Conserve liste des capacités autorisées par le profil ; get lit sa valeur et set permet de l’affecter.
} // Les permissions d'un rôle sont des données administrables.

public class Student : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public string Number { get; set; } = ""; // Conserve matricule étudiant permanent ; get lit sa valeur et set permet de l’affecter.
    public string FirstName { get; set; } = ""; // Conserve prénoms du titulaire ; get lit sa valeur et set permet de l’affecter.
    public string LastName { get; set; } = ""; // Conserve nom de famille du titulaire ; get lit sa valeur et set permet de l’affecter.
    public string Profile { get; set; } = "{}"; // Conserve coordonnées et personnes à prévenir, sérialisées en JSON ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "PRE_REGISTERED"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
    public string ActivationHash { get; set; } = ""; // Conserve empreinte du secret personnel d’activation ; get lit sa valeur et set permet de l’affecter.
    public string Track { get; set; } = ""; // Conserve parcours administratif, distinct de la salle ; get lit sa valeur et set permet de l’affecter.
} // Profile conserve les coordonnées et les personnes à prévenir.

public class Year : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public string Code { get; set; } = ""; // Conserve code métier imprimé ou saisi par le gestionnaire ; get lit sa valeur et set permet de l’affecter.
    public DateOnly Start { get; set; } // Conserve date de début de l’année académique ; get lit sa valeur et set permet de l’affecter.
    public DateOnly End { get; set; } // Conserve date de fin de l’année académique ; get lit sa valeur et set permet de l’affecter.
    public string Welcome { get; set; } = ""; // Conserve message d’accueil de la nouvelle année ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "DRAFT"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
} // Une année ouverte est la référence des nouvelles inscriptions.

public class SchoolClass : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid YearId { get; set; } // Conserve identifiant de l’année académique ; get lit sa valeur et set permet de l’affecter.
    public Year Year { get; set; } = null !; // Conserve la donnée Year ; get lit sa valeur et set permet de l’affecter.
    public string Name { get; set; } = ""; // Conserve libellé affiché aux utilisateurs ; get lit sa valeur et set permet de l’affecter.
    public string Level { get; set; } = ""; // Conserve niveau du cursus enregistré dans le référentiel ; get lit sa valeur et set permet de l’affecter.
    public string Speciality { get; set; } = ""; // Conserve spécialité du cursus enregistré dans le référentiel ; get lit sa valeur et set permet de l’affecter.
    public string Tracks { get; set; } = "[]"; // Conserve parcours administratifs autorisés dans cette salle ; get lit sa valeur et set permet de l’affecter.
    public string JoinHash { get; set; } = ""; // Conserve empreinte du code partagé de classe ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset? JoinExpires { get; set; } // Conserve date limite d’utilisation du code de classe ; get lit sa valeur et set permet de l’affecter.
} // Une salle peut recevoir plusieurs parcours administratifs.

public class Curriculum : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public string Name { get; set; } = ""; // Conserve libellé affiché aux utilisateurs ; get lit sa valeur et set permet de l’affecter.
    public string Level { get; set; } = ""; // Conserve niveau du cursus enregistré dans le référentiel ; get lit sa valeur et set permet de l’affecter.
    public string Track { get; set; } = ""; // Conserve parcours administratif, distinct de la salle ; get lit sa valeur et set permet de l’affecter.
    public string Speciality { get; set; } = ""; // Conserve spécialité du cursus enregistré dans le référentiel ; get lit sa valeur et set permet de l’affecter.
    public string Version { get; set; } = ""; // Conserve numéro de version utilisé pour conserver ou révoquer un état ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "DRAFT"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
    public string Content { get; set; } = "{}"; // Conserve contenu figé et sérialisé de la version ou du relevé ; get lit sa valeur et set permet de l’affecter.
} // Content est une version figée des semestres, UE, EC et règles de calcul.

public class Enrollment : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid StudentId { get; set; } // Conserve identifiant du dossier étudiant ; get lit sa valeur et set permet de l’affecter.
    public Student Student { get; set; } = null !; // Conserve la donnée Student ; get lit sa valeur et set permet de l’affecter.
    public Guid ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public SchoolClass Class { get; set; } = null !; // Conserve la donnée Class ; get lit sa valeur et set permet de l’affecter.
    public Guid YearId { get; set; } // Conserve identifiant de l’année académique ; get lit sa valeur et set permet de l’affecter.
    public Year Year { get; set; } = null !; // Conserve la donnée Year ; get lit sa valeur et set permet de l’affecter.
    public Guid CurriculumId { get; set; } // Conserve identifiant de la version du programme ; get lit sa valeur et set permet de l’affecter.
    public Curriculum Curriculum { get; set; } = null !; // Conserve la donnée Curriculum ; get lit sa valeur et set permet de l’affecter.
    public int Attempt { get; set; } // Conserve numéro de tentative au même niveau ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "ACTIVE"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Created { get; set; } = PortalClock.Now; // Conserve date UTC de création ou de publication ; get lit sa valeur et set permet de l’affecter.
} // Le transfert clôture une inscription et en crée une autre.

public class Grade : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid EnrollmentId { get; set; } // Conserve identifiant de l’inscription annuelle ; get lit sa valeur et set permet de l’affecter.
    public Enrollment Enrollment { get; set; } = null !; // Conserve la donnée Enrollment ; get lit sa valeur et set permet de l’affecter.
    public Guid EcId { get; set; } // Conserve identifiant de la matière ; get lit sa valeur et set permet de l’affecter.
    public int Semester { get; set; } // Conserve numéro de semestre concerné ; get lit sa valeur et set permet de l’affecter.
    public decimal Value { get; set; } // Conserve valeur décimale de la note sur vingt ; get lit sa valeur et set permet de l’affecter.
} // Une note référence l'UUID d'un EC de la version inscrite.

public class Rescue : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid EnrollmentId { get; set; } // Conserve identifiant de l’inscription annuelle ; get lit sa valeur et set permet de l’affecter.
    public Enrollment Enrollment { get; set; } = null !; // Conserve la donnée Enrollment ; get lit sa valeur et set permet de l’affecter.
    public Guid UeId { get; set; } // Conserve identifiant de l’unité d’enseignement ; get lit sa valeur et set permet de l’affecter.
    public string Reason { get; set; } = ""; // Conserve motif explicite de la décision ; get lit sa valeur et set permet de l’affecter.
    public Guid Actor { get; set; } // Conserve UUID du compte auteur de la décision ; get lit sa valeur et set permet de l’affecter.
} // La décision de repêchage n'altère jamais la note.

public class Publication : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public SchoolClass Class { get; set; } = null !; // Conserve la donnée Class ; get lit sa valeur et set permet de l’affecter.
    public int Semester { get; set; } // Conserve numéro de semestre concerné ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "PUBLISHED"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Created { get; set; } = PortalClock.Now; // Conserve date UTC de création ou de publication ; get lit sa valeur et set permet de l’affecter.
} // Une proclamation est distincte de l'import.

public class Snapshot : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public string Header { get; set; } = "{}"; // Fige le nom et le parcours du titulaire pour ne pas réécrire un ancien bulletin après modification du profil.
    public Guid EnrollmentId { get; set; } // Conserve identifiant de l’inscription annuelle ; get lit sa valeur et set permet de l’affecter.
    public Enrollment Enrollment { get; set; } = null !; // Conserve la donnée Enrollment ; get lit sa valeur et set permet de l’affecter.
    public int Semester { get; set; } // Conserve numéro de semestre concerné ; get lit sa valeur et set permet de l’affecter.
    public int Version { get; set; } // Conserve numéro de version utilisé pour conserver ou révoquer un état ; get lit sa valeur et set permet de l’affecter.
    public string Content { get; set; } = "{}"; // Conserve contenu figé et sérialisé de la version ou du relevé ; get lit sa valeur et set permet de l’affecter.
    public string Hash { get; set; } = ""; // Conserve empreinte SHA-256 du contenu conservé ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Created { get; set; } = PortalClock.Now; // Conserve date UTC de création ou de publication ; get lit sa valeur et set permet de l’affecter.
} // Les versions publiées restent immuables et vérifiables.

public class ImportBatch : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public SchoolClass Class { get; set; } = null !; // Conserve la donnée Class ; get lit sa valeur et set permet de l’affecter.
    public int Semester { get; set; } // Conserve numéro de semestre concerné ; get lit sa valeur et set permet de l’affecter.
    public string Rows { get; set; } = "[]"; // Conserve lignes contrôlées de l’import en JSON ; get lit sa valeur et set permet de l’affecter.
    public string Errors { get; set; } = "[]"; // Conserve erreurs détectées avant confirmation de l’import ; get lit sa valeur et set permet de l’affecter.
    public string Hash { get; set; } = ""; // Conserve empreinte SHA-256 du contenu conservé ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "PREVIEW"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
} // Une prévisualisation valide toutes les lignes avant écriture.

public class Officer : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public SchoolClass Class { get; set; } = null !; // Conserve la donnée Class ; get lit sa valeur et set permet de l’affecter.
    public Guid StudentId { get; set; } // Conserve identifiant du dossier étudiant ; get lit sa valeur et set permet de l’affecter.
    public Student Student { get; set; } = null !; // Conserve la donnée Student ; get lit sa valeur et set permet de l’affecter.
    public string Position { get; set; } = ""; // Conserve fonction de responsable exercée dans la classe ; get lit sa valeur et set permet de l’affecter.
} // La responsabilité étudiante est limitée à sa classe.

public class CourseSession : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public SchoolClass Class { get; set; } = null !; // Conserve la donnée Class ; get lit sa valeur et set permet de l’affecter.
    public Guid EcId { get; set; } // Conserve identifiant de la matière ; get lit sa valeur et set permet de l’affecter.
    public string Course { get; set; } = ""; // Conserve libellé de la matière suivie pendant la séance ; get lit sa valeur et set permet de l’affecter.
    public string Secret { get; set; } = ""; // Conserve clé secrète de signature du code dynamique du cours ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Opened { get; set; } = PortalClock.Now; // Conserve date UTC d’ouverture du cours ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset? Closed { get; set; } // Conserve date UTC de fermeture, absente tant que le cours est ouvert ; get lit sa valeur et set permet de l’affecter.
} // Chaque séance possède son propre secret de code dynamique.

public class Attendance : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid SessionId { get; set; } // Conserve identifiant du cours ouvert pour la présence ; get lit sa valeur et set permet de l’affecter.
    public CourseSession Session { get; set; } = null !; // Conserve la donnée Session ; get lit sa valeur et set permet de l’affecter.
    public Guid StudentId { get; set; } // Conserve identifiant du dossier étudiant ; get lit sa valeur et set permet de l’affecter.
    public Student Student { get; set; } = null !; // Conserve la donnée Student ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "PRESENT"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
    public string Method { get; set; } = "CODE"; // Conserve origine du pointage pour distinguer QR, code et correction administrative ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Checked { get; set; } = PortalClock.Now; // Conserve instant UTC d’enregistrement du pointage ; get lit sa valeur et set permet de l’affecter.
} // La contrainte unique empêche une double présence.

public class Delegation : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public SchoolClass Class { get; set; } = null !; // Conserve la donnée Class ; get lit sa valeur et set permet de l’affecter.
    public Guid Granter { get; set; } // Conserve étudiant responsable qui accorde l’affichage temporaire ; get lit sa valeur et set permet de l’affecter.
    public Guid Grantee { get; set; } // Conserve étudiant qui reçoit uniquement le droit d’affichage QR ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Expires { get; set; } // Conserve instant au-delà duquel le droit temporaire est inutilisable ; get lit sa valeur et set permet de l’affecter.
    public bool Revoked { get; set; } // Conserve indicateur de révocation anticipée ; get lit sa valeur et set permet de l’affecter.
} // Le droit d'affichage expire sans tâche planifiée nécessaire.

public class Justification : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid SessionId { get; set; } // Conserve identifiant du cours ouvert pour la présence ; get lit sa valeur et set permet de l’affecter.
    public CourseSession Session { get; set; } = null !; // Conserve la donnée Session ; get lit sa valeur et set permet de l’affecter.
    public Guid StudentId { get; set; } // Conserve identifiant du dossier étudiant ; get lit sa valeur et set permet de l’affecter.
    public Student Student { get; set; } = null !; // Conserve la donnée Student ; get lit sa valeur et set permet de l’affecter.
    public string Reason { get; set; } = ""; // Conserve motif explicite de la décision ; get lit sa valeur et set permet de l’affecter.
    public string Status { get; set; } = "PENDING"; // Conserve état courant du dossier ou de l’opération ; get lit sa valeur et set permet de l’affecter.
    public string Decision { get; set; } = ""; // Conserve commentaire du responsable sur le justificatif ; get lit sa valeur et set permet de l’affecter.
    public Guid? DocumentId { get; set; } // Conserve référence de la pièce jointe privée ; get lit sa valeur et set permet de l’affecter.
} // Une absence acceptée devient EXCUSED, jamais PRESENT.

public class DisciplineEvent : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid EnrollmentId { get; set; } // Conserve identifiant de l’inscription annuelle ; get lit sa valeur et set permet de l’affecter.
    public Enrollment Enrollment { get; set; } = null !; // Conserve la donnée Enrollment ; get lit sa valeur et set permet de l’affecter.
    public int Semester { get; set; } // Conserve numéro de semestre concerné ; get lit sa valeur et set permet de l’affecter.
    public decimal Delta { get; set; } // Conserve variation de points ajoutée au registre disciplinaire ; get lit sa valeur et set permet de l’affecter.
    public string Reason { get; set; } = ""; // Conserve motif explicite de la décision ; get lit sa valeur et set permet de l’affecter.
    public Guid Actor { get; set; } // Conserve UUID du compte auteur de la décision ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Created { get; set; } = PortalClock.Now; // Conserve date UTC de création ou de publication ; get lit sa valeur et set permet de l’affecter.
} // La note est calculée sur ce registre traçable.

public class Document : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid? ClassId { get; set; } // Conserve identifiant de la classe physique ; get lit sa valeur et set permet de l’affecter.
    public Guid? OwnerStudentId { get; set; } // Conserve titulaire de la pièce jointe personnelle ; get lit sa valeur et set permet de l’affecter.
    public string Title { get; set; } = ""; // Conserve titre lisible du document ; get lit sa valeur et set permet de l’affecter.
    public string Category { get; set; } = ""; // Conserve nature pédagogique ou justificative du fichier ; get lit sa valeur et set permet de l’affecter.
    public string Metadata { get; set; } = "{}"; // Conserve classement du document par année, niveau et matière ; get lit sa valeur et set permet de l’affecter.
    public string Filename { get; set; } = ""; // Conserve nom du fichier présenté au téléchargement ; get lit sa valeur et set permet de l’affecter.
    public string StorageKey { get; set; } = ""; // Conserve nom technique généré côté serveur pour le stockage privé ; get lit sa valeur et set permet de l’affecter.
    public string Mime { get; set; } = ""; // Conserve type de contenu HTTP autorisé ; get lit sa valeur et set permet de l’affecter.
    public string Hash { get; set; } = ""; // Conserve empreinte SHA-256 du contenu conservé ; get lit sa valeur et set permet de l’affecter.
    public bool Approved { get; set; } // Conserve accord administratif avant diffusion du document ; get lit sa valeur et set permet de l’affecter.
} // Les fichiers privés sont servis après contrôle des droits.

public class Audit : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid Actor { get; set; } // Conserve UUID du compte auteur de la décision ; get lit sa valeur et set permet de l’affecter.
    public string Action { get; set; } = ""; // Conserve nom de l’action enregistrée dans le journal ; get lit sa valeur et set permet de l’affecter.
    public string Resource { get; set; } = ""; // Conserve identifiant de la ressource concernée par l’audit ; get lit sa valeur et set permet de l’affecter.
    public string Details { get; set; } = "{}"; // Conserve données de l’opération sans mot de passe ni code secret ; get lit sa valeur et set permet de l’affecter.
    public string Ip { get; set; } = ""; // Conserve adresse réseau de l’auteur pour la traçabilité ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Created { get; set; } = PortalClock.Now; // Conserve date UTC de création ou de publication ; get lit sa valeur et set permet de l’affecter.
} // Les actions sensibles sont inscrites avec leur auteur et date.

public class Notification : Entity // Déclare ce type C# pour représenter un objet du domaine ou un service métier.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public Guid StudentId { get; set; } // Conserve identifiant du dossier étudiant ; get lit sa valeur et set permet de l’affecter.
    public string Message { get; set; } = ""; // Conserve texte de notification remis au destinataire ; get lit sa valeur et set permet de l’affecter.
    public bool Read { get; set; } // Conserve indicateur de lecture de la notification ; get lit sa valeur et set permet de l’affecter.
    public DateTimeOffset Created { get; set; } = PortalClock.Now; // Conserve date UTC de création ou de publication ; get lit sa valeur et set permet de l’affecter.
} // Les messages sont uniquement visibles par leur destinataire.

public class PortalDb(DbContextOptions<PortalDb> options) : DbContext(options) // Représente une unité de travail transactionnelle.
{ // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
    public DbSet<User> Users => Set<User>(); // Comptes de connexion.
    public DbSet<Role> Roles => Set<Role>(); // Profils et droits.
    public DbSet<Student> Students => Set<Student>(); // Identités étudiantes permanentes.
    public DbSet<Year> Years => Set<Year>(); // Historique des années.
    public DbSet<SchoolClass> Classes => Set<SchoolClass>(); // Salles physiques.
    public DbSet<Curriculum> Curricula => Set<Curriculum>(); // Versions pédagogiques.
    public DbSet<Enrollment> Enrollments => Set<Enrollment>(); // Scolarité par année.
    public DbSet<Grade> Grades => Set<Grade>(); // Notes en brouillon.
    public DbSet<Rescue> Rescues => Set<Rescue>(); // Décisions exceptionnelles.
    public DbSet<Publication> Publications => Set<Publication>(); // Proclamations.
    public DbSet<Snapshot> Snapshots => Set<Snapshot>(); // Relevés officiels figés.
    public DbSet<ImportBatch> Imports => Set<ImportBatch>(); // Lots d'import contrôlés.
    public DbSet<Officer> Officers => Set<Officer>(); // Mandats de classe.
    public DbSet<CourseSession> Sessions => Set<CourseSession>(); // Cours ouverts à l'appel.
    public DbSet<Attendance> Attendances => Set<Attendance>(); // Présences et absences.
    public DbSet<Delegation> Delegations => Set<Delegation>(); // Affichage temporaire.
    public DbSet<Justification> Justifications => Set<Justification>(); // Motifs des absences.
    public DbSet<DisciplineEvent> Discipline => Set<DisciplineEvent>(); // Registre disciplinaire.
    public DbSet<Document> Documents => Set<Document>(); // Catalogue des fichiers.
    public DbSet<Audit> Audits => Set<Audit>(); // Journal administratif.
    public DbSet<Notification> Notifications => Set<Notification>(); // Informations étudiantes.

    protected override void OnModelCreating(ModelBuilder m) // Les règles SQL s'appliquent aussi en cas de concurrence.
    { // Ouvre le bloc qui regroupe les instructions de cette classe, méthode ou condition.
        m.Entity<Grade>().ToTable(t => t.HasCheckConstraint("CK_Grade_Range", "CAST(\"Value\" AS NUMERIC) >= 0 AND CAST(\"Value\" AS NUMERIC) <= 20")); // Le SQL refuse également une note hors de zéro à vingt.
        m.Entity<CurriculumElement>().ToTable(t => t.HasCheckConstraint("CK_Ec_Coefficient", "CAST(\"Coefficient\" AS NUMERIC) > 0")); // Tout EC conserve un poids positif, même lors d’une écriture SQL directe.
        m.Entity<CurriculumUnit>().ToTable(t => t.HasCheckConstraint("CK_Ue_Credits", "\"Credits\" IS NULL OR CAST(\"Credits\" AS NUMERIC) >= 0")); // Une valeur inconnue reste autorisée en brouillon, mais pas un crédit négatif.
        m.Entity<Year>().ToTable(t => t.HasCheckConstraint("CK_Year_Dates", "\"Start\" < \"End\"")); // La date de fin d’une année suit obligatoirement son début.
        m.Entity<Grade>().HasOne<CurriculumElement>().WithMany().HasForeignKey(g => g.EcId); // Une note ne peut pas référencer une matière inexistante.
        m.Entity<Rescue>().HasOne<CurriculumUnit>().WithMany().HasForeignKey(r => r.UeId); // Le repêchage concerne obligatoirement une UE connue.
        m.Entity<CourseSession>().HasOne<CurriculumElement>().WithMany().HasForeignKey(s => s.EcId); // Le cours ouvert pour l'appel doit appartenir au catalogue.
        m.Entity<Equivalence>().HasOne<CurriculumUnit>().WithMany().HasForeignKey(e => e.SourceUe); // Vérifie la présence de l'ancienne UE dans le référentiel.
        m.Entity<Equivalence>().HasOne<CurriculumUnit>().WithMany().HasForeignKey(e => e.TargetUe); // Vérifie la présence de l'UE destinataire de la capitalisation.
        m.Entity<Equivalence>().HasIndex(x => new { x.SourceUe, x.TargetUe }).IsUnique(); // Une correspondance approuvée ne se répète pas.
        m.Entity<User>().HasOne<Student>().WithMany().HasForeignKey(u => u.StudentId); // Lie le compte au dossier permanent avec une vraie clé étrangère.
        m.Entity<User>().HasIndex(u => u.StudentId).IsUnique(); // Un étudiant possède un seul compte.
        m.Entity<User>().HasIndex(x => x.Username).IsUnique(); // Deux comptes ne partagent pas le même identifiant.
        m.Entity<Student>().HasIndex(x => x.Number).IsUnique(); // Le matricule est permanent et unique.
        m.Entity<Year>().HasIndex(x => x.Code).IsUnique(); // Une année scolaire n'est créée qu'une fois.
        m.Entity<Year>().HasIndex(x => x.Status).IsUnique().HasFilter("\"Status\" = 'OPEN'"); // Une seule année est ouverte.
        m.Entity<Enrollment>().HasIndex(x => new { x.StudentId, x.YearId }).IsUnique().HasFilter("\"Status\" = 'ACTIVE'"); // Une seule inscription active par année.
        m.Entity<Snapshot>().HasIndex(s => new { s.EnrollmentId, s.Semester, s.Version }).IsUnique(); // Une version de relevé ne peut pas être écrasée par une rectification concurrente.
        m.Entity<Grade>().HasIndex(x => new { x.EnrollmentId, x.EcId }).IsUnique(); // Une note courante par EC.
        m.Entity<Rescue>().HasIndex(x => new { x.EnrollmentId, x.UeId }).IsUnique(); // Une décision courante par UE.
        m.Entity<Attendance>().HasIndex(x => new { x.SessionId, x.StudentId }).IsUnique(); // Une ligne de présence par cours et étudiant.
        m.Entity<Officer>().HasIndex(x => new { x.ClassId, x.Position }).IsUnique(); // Un mandat par fonction et classe.
        m.Entity<Publication>().HasIndex(x => new { x.ClassId, x.Semester }).IsUnique(); // Une proclamation courante par semestre.
        if (Database.IsSqlite()) // Teste cette condition ; l’instruction suivante s’exécute uniquement si elle est vraie.
            foreach (var property in m.Model.GetEntityTypes().SelectMany(t => t.GetProperties()).Where(p => p.ClrType == typeof(DateTimeOffset) || p.ClrType == typeof(DateTimeOffset? ))) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
                property.SetValueConverter(new Microsoft.EntityFrameworkCore.Storage.ValueConversion.DateTimeOffsetToBinaryConverter()); // SQLite compare les dates sous forme numérique pendant les tests locaux.
        foreach (var fk in m.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys())) // Parcourt un à un les éléments de la collection indiquée pour appliquer le même traitement.
            fk.DeleteBehavior = DeleteBehavior.Restrict; // Les suppressions ne détruisent pas les historiques liés.
    } // Ferme ce bloc après l’exécution de ses instructions.
} // Ferme ce bloc après l’exécution de ses instructions.
