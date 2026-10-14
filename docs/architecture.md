# Modèle du SI implémenté

Les modules `AcademicApi`, `IdentityApi`, `GradesApi`, `ProgressApi`, `AttendanceApi`, `AttendanceImportApi`, `DocumentsApi` et `ExportsApi` partagent une base relationnelle et des transactions. Le contrôle central `Access` recharge les permissions en base à chaque requête. Les programmes sont des versions immuables : une copie JSON conserve leur structure et des tables UE/EC garantissent les clés étrangères SQL.

```mermaid
erDiagram
    User }o--o| Student : identite
    Year ||--o{ SchoolClass : classes
    Student ||--o{ Enrollment : historique
    SchoolClass ||--o{ Enrollment : salle
    Curriculum ||--o{ Enrollment : version
    Curriculum ||--o{ CurriculumUnit : semestres
    CurriculumUnit ||--o{ CurriculumElement : matieres
    Enrollment ||--o{ Grade : notes
    CurriculumElement ||--o{ Grade : coefficient
    Enrollment ||--o{ Rescue : decisions
    CurriculumUnit ||--o{ Rescue : unite
    Enrollment ||--o{ Snapshot : versions_publiees
    SchoolClass ||--o{ Publication : proclamation
    SchoolClass ||--o{ Officer : responsables
    SchoolClass ||--o{ CourseSession : cours
    CourseSession ||--o{ Attendance : pointages
    Student ||--o{ Attendance : titulaire
    CourseSession ||--o{ Justification : absences
    Enrollment ||--o{ DisciplineEvent : sanctions
```

Les affectations de rôles et les listes de permissions sont des tableaux JSON d'identifiants/capacités contrôlés par les endpoints. Le code métier ne donne pas un droit en fonction d'un nom comme « DAAS ». Les fonctions de délégué, adjoint et secrétaire sont des mandats contextuels et ne permettent pas d'agir dans une autre classe.

`grades.view`, `grades.import`, `grades.update`, `grades.publish` et `grades.rescue` sont indépendantes. Une permission de groupe comme `grades.manage` regroupe les opérations de son module. Les droits étudiants séparent consultation, création, modification, désactivation, suppression de fiches vides, transfert et réinitialisation d'identifiants.

```mermaid
sequenceDiagram
    actor Gestionnaire
    participant API
    participant Base
    actor Etudiant
    Gestionnaire->>API: Ouvrir une nouvelle année
    API->>Base: Fermer l'ancienne année et ses inscriptions
    API->>Base: Incrémenter la version des sessions étudiantes
    Etudiant->>API: Requête avec l'ancien cookie
    API-->>Etudiant: 401, reconnexion requise
    Etudiant->>API: Connexion puis code partagé de nouvelle classe
    API->>Base: Inscription dans la version du bon parcours
    API-->>Etudiant: Tentative et UE acquises auparavant
```

```mermaid
sequenceDiagram
    actor Gestionnaire
    participant API
    participant Moteur
    participant Base
    actor Etudiant
    Gestionnaire->>API: Prévisualiser le CSV du parcours
    API-->>Gestionnaire: Lignes reconnues et erreurs précises
    Gestionnaire->>API: Confirmer le fichier corrigé
    API->>Base: Notes et audit dans une transaction
    Gestionnaire->>API: Publier le semestre complet
    API->>Moteur: Notes × coefficients réels
    Moteur-->>API: UE, crédits, moyenne du semestre
    API->>Base: Instantanés et notifications
    Etudiant->>API: Consulter son relevé
    API-->>Etudiant: Dernière version publiée de ses propres résultats
```

Une rectification publiée ajoute une version du relevé dans la même transaction que la note et l'audit. Une décision de repêchage conserve la valeur de la note. Le PDF reprend les notes, noms de matières et identité figés à la publication. Les transferts conservent les anciens dossiers, notes, décisions et relevés.
