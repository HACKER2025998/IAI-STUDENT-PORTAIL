# IAI Student Portal

SI académique en français : ASP.NET Core 10, Entity Framework Core, PostgreSQL et Nuxt/Vue/TypeScript. Les commentaires expliquent les opérations métier. Les huit programmes ITI/LPI de L1, L2, L3 ASR et L3 GLSI proviennent des six photographies.

## Lancement local

Installer le SDK **et le runtime ASP.NET 10**, Node 24, puis exécuter depuis la racine :

```sh
dotnet restore backend
npm ci --prefix frontend
dotnet build backend
```

Installer Chromium pour les PDF :

```sh
backend/bin/Debug/net10.0/.playwright/node/linux-x64/node backend/bin/Debug/net10.0/.playwright/package/cli.js install chromium
```

Premier terminal :

```sh
export Bootstrap__Username=admin
export Bootstrap__Password='remplacer-par-un-secret-personnel'
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project backend --no-launch-profile --urls http://127.0.0.1:5080
```

Deuxième terminal :

```sh
npm run dev --prefix frontend -- --port 3000
```

Ouvrir http://localhost:3000. Le mode local utilise SQLite. Pour PostgreSQL, définir `ConnectionStrings__Portal='Host=localhost;Database=iai_portal;Username=iai;Password=...'` avant de démarrer le backend. Aucune donnée étudiante de démonstration n'est créée au démarrage.

## Utilisation

1. Créer une année, saisir son accueil, puis l'ouvrir.
2. Créer les classes physiques avec leurs parcours autorisés.
3. Créer les dossiers étudiants et remettre personnellement matricule et code d'activation.
4. L'étudiant active son compte, conserve ses identifiants JSON, puis intègre sa classe avec le code partagé et son programme.
5. Choisir classe, programme et semestre ; télécharger et remplir le modèle CSV.
6. Prévisualiser, corriger toutes les erreurs, confirmer l'import, vérifier et publier séparément.
7. Consulter les relevés et bulletins PDF, classements, UE acquises et historique.
8. Désigner les responsables de classe, ouvrir les séances, afficher le QR/code, pointer, puis fermer l'appel.
9. Traiter les justificatifs, enregistrer les sanctions et valider les documents téléversés.

Les codes de classe sont régénérables et expirent après sept jours. Les codes de présence changent toutes les 120 secondes. Les délégations d'affichage sont limitées à six par délégué ou sous-délégué et expirent après 24 heures. Un mandat ne donne des droits que dans sa propre classe.

## Données et décisions institutionnelles

Les crédits des fiches **ITI L3** ne figurent pas dans les photos : ces programmes sont en brouillon et leur publication est bloquée. Les coefficients de projet ITI sont 10 ; ceux de LPI sont 8. Créer une version approuvée après confirmation des crédits, avec l'éditeur pédagogique. Le système conserve les codes imprimés répétés et utilise des UUID comme clés.

La méthode annuelle par défaut donne le même poids aux deux semestres ; l'éditeur propose aussi la pondération de tous les EC. Les listes et rapports sont exportables en CSV, XLSX et PDF. Le modèle PDF est fonctionnel, mais son apparence reste à adapter au bulletin officiel annoncé dans le cahier des charges. La discipline est un registre indépendant : aucune formule de liaison avec « Séminaire + Discipline » n'a été inventée.

Pour migrer une ancienne année : créer son année et ses classes, la clôturer, choisir « historique » lors des inscriptions puis importer et publier ses vrais CSV de notes. L’import de présence possède aussi un modèle, une prévisualisation et une confirmation auditée.

Les UE acquises de la même version reprennent leurs vraies notes publiées. Les équivalences entre versions donnent le suivi des crédits ; la reprise des notes entre EC différents demande une règle pédagogique explicite et n'est pas automatisée.

## Vérification

```sh
dotnet build tests/UnitTests
# Exécuter les tests sur un poste disposant du runtime ASP.NET 10 :
dotnet run --project tests/UnitTests --no-build
npm run typecheck --prefix frontend
npm run build --prefix frontend
```

Les tests HTTP **créent et clôturent des années et des dossiers**. Les exécuter exclusivement sur une base de test dédiée. Démarrer son backend en Development avec `Authentication__RateLimit=100`, puis :

```sh
PORTAL_PASSWORD='mot-de-passe-de-la-base-de-test' python3 tests/integration.py
CHROMIUM_PATH='/chemin/vers/chrome' PORTAL_PASSWORD='mot-de-passe-de-test' node tests/browser.mjs
```

`integration.py` vérifie activation unique, confidentialité des relevés, modèle CSV, confirmation unique, repêchage, publication, PDF, rangs, pointage, limite des délégations, justificatifs, sanctions, révocation annuelle et redoublement. `browser.mjs` vérifie les écrans de bureau et mobile.

## Déploiement et sauvegarde

Copier `.env.example` en `.env`, remplacer les secrets, configurer le secret MFA Base32 dans une application TOTP, déposer le certificat et sa clé dans `ops/certs/fullchain.pem` et `ops/certs/privkey.pem`, puis :

```sh
docker compose up --build -d
```

En production, les comptes administratifs exigent le TOTP. Les nouveaux comptes administratifs reçoivent leur secret MFA une seule fois avec leurs identifiants. Conserver une copie sécurisée des volumes `cookie_keys`, `document_data` et `postgres_data`.

`ops/backup.sh` exporte PostgreSQL, les documents et les clés ; `ops/restore.sh` restaure dans une installation de secours à base vide. Le schéma initial est dans `docs/schema-postgresql.sql`. Le démarrage crée une base neuve avec EF `EnsureCreated` ; une évolution future du schéma SQL devra introduire des migrations avant de modifier une base existante.

Les documents utilisent actuellement un volume privé local. Les services optionnels de l'architecture proposée (MinIO/S3, Redis, Hangfire, monitoring complet, antivirus, migration automatisée des anciens dossiers) ne sont pas inclus. Les fichiers Office sont contrôlés par signature ZIP ; un antivirus est nécessaire avant une ouverture à des sources non maîtrisées. Les dépendances Nuxt de construction signalent des avis de sécurité sans correctif compatible lors de la vérification ; ne pas exposer le serveur de développement.
