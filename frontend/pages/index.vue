<script setup lang="ts">
// Les types du portail décrivent les données JSON reçues de l'API.
type Row = Record<string, any>; // Décrit la forme et le type attendus pour cette donnée TypeScript.
// Un champ décrit un contrôle de formulaire réutilisable avec son libellé français.
type Field = { // Décrit la forme et le type attendus pour cette donnée TypeScript.
  // Décrit la forme et le type attendus pour cette donnée TypeScript.
  key: string; // Nomme le champ transmis au serveur : string;.
  label: string; // Donne à ce contrôle son libellé visible par l’utilisateur.
  type?: string; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  options?: { value: any; label: string }[]; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  required?: boolean; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
}; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
// Les cases à cocher présentent les capacités disponibles sans demander des UUID ou du JSON.
const permissionOptions = [ // Prépare cette variable ou cette fonction pour le traitement en cours.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "*", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Tous les droits", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "roles.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Créer et modifier les profils", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "users.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Gérer les comptes administratifs", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "academic.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Gérer années, classes et programmes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "academic.view", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Consulter toutes les classes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Gérer tous les dossiers étudiants", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.view", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Consulter les dossiers étudiants", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.create", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Créer un dossier étudiant", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.update", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Modifier les coordonnées étudiantes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.disable", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Désactiver ou réactiver un dossier", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.delete", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Supprimer les fiches sans historique", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.transfer", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Transférer un étudiant entre classes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "students.reset_password", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Réinitialiser les identifiants étudiants", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "grades.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Gérer toutes les opérations de notes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "grades.view", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Consulter les notes et classements", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "grades.import", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Importer les notes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "grades.update", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Saisir et rectifier les notes", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "grades.publish", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Publier et verrouiller les résultats", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "grades.rescue", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Valider une UE par repêchage", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "attendance.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Gérer les présences et justificatifs", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "attendance.reports", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Consulter les rapports de présence", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "discipline.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Enregistrer les sanctions et corrections", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "documents.manage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Téléverser et valider les documents", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  { // Ouvre cette construction dont les éléments sont définis juste en dessous.
    value: "audit.view", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    label: "Consulter le journal des opérations", // Donne à ce contrôle son libellé visible par l’utilisateur.
  }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
]; // Les permissions sont séparées pour permettre des profils de consultation sans droit de modification.
// L'éditeur pédagogique modifie une copie qui deviendra une nouvelle version immutable.
const programmeEditor = ref<Row | null>(null); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le compte courant reste en mémoire ; aucun secret de session n'est placé dans localStorage.
const me = ref<Row | null>(null); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La page active pilote le menu latéral et l'affichage du contenu.
const page = ref("Accueil"); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les collections sont chargées selon les permissions du compte.
type Store = { // Décrit la forme et le type attendus pour cette donnée TypeScript.
  // Décrit la forme et le type attendus pour cette donnée TypeScript.
  years: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  classes: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  curricula: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  students: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  enrollments: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  sessions: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  records: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  justifications: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  documents: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  roles: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  users: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  audit: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  officers: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  delegations: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  notifications: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  discipline: Row[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  [key: string]: Row[]; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
}; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
// Les clés connues sont obligatoires pour éviter des listes absentes dans les écrans.
const store = reactive<Store>({ // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  years: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  classes: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  curricula: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  students: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  enrollments: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  sessions: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  records: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  justifications: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  documents: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  roles: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  users: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  audit: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  officers: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  delegations: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  notifications: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  discipline: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les erreurs et les confirmations sont annoncées dans une zone accessible.
const message = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le drapeau bloque les doubles validations de formulaire.
const busy = ref(false); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les résultats chargés appartiennent à l'inscription sélectionnée.
const results = ref<Row[]>([]); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les classements sont distincts des notes détaillées.
const ranks = ref<Row[]>([]); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le format exporté suit le type de classement réellement consulté.
const rankAnnual = ref(false); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les UE capitalisées sont distinctes des nouvelles notes, pour ne pas fabriquer de notes d’EC.
const progress = ref<Row[]>([]); // Prépare cette variable ou cette fonction pour le traitement en cours.
// L'inscription sélectionnée permet de passer des années courantes aux années historiques.
const selectedEnrollment = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La sélection de classe filtre les actions administratives et les présences.
const selectedClass = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le semestre choisi sert aux imports et proclamations.
const semester = ref(1); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le programme choisi différencie notamment les modèles CSV ITI et LPI.
const selectedCurriculum = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La recherche est appliquée aux listes visibles sans requête supplémentaire.
const search = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les identifiants de connexion ne restent pas stockés après la connexion.
const login = reactive({ username: "", password: "", otp: "" }); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le mode activation exige le matricule et un secret personnel à usage unique.
const activation = reactive({ studentNumber: "", activationCode: "" }); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Affiche le formulaire d'activation au lieu de la connexion lorsque nécessaire.
const activateMode = ref(false); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les secrets créés sont visibles uniquement dans cette fenêtre temporaire.
const credentials = ref<Row | null>(null); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le formulaire modal reçoit des champs et une fonction métier.
const modal = ref<{ // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  title: string; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  fields: Field[]; // Décrit la forme et le type attendus pour cette donnée TypeScript.
  submit: (data: Row) => Promise<void>; // Décrit la forme et le type attendus pour cette donnée TypeScript.
} | null>(null); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
// Les valeurs du formulaire sont séparées de sa définition.
const form = reactive<Row>({}); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le lot prévisualisé doit être confirmé explicitement après son contrôle.
const preview = ref<Row | null>(null); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le code de présence et son image QR sont rafraîchis seulement durant l'affichage.
const token = ref<Row | null>(null); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le responsable choisit les délégations dans la liste limitée à sa classe.
const roster = ref<Row[]>([]); // Prépare cette variable ou cette fonction pour le traitement en cours.
// L'image QR est générée localement depuis le jeton autorisé du serveur.
const qrImage = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La séance choisie détermine le QR et le pointage.
const selectedSession = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La catégorie documentaire filtre les épreuves, TD et TP.
const documentCategory = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La date filtre les rapports journaliers ; l'export utilise la même vue.
const reportDate = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le début de semaine permet d'exporter les sept jours correspondants.
const weekStart = ref(""); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Un temporisateur sera libéré quand l'utilisateur quitte le composant.
let timer: ReturnType<typeof setInterval> | undefined; // Prépare cette variable ou cette fonction pour le traitement en cours.
// Les permissions servent à l'interface ; le serveur les vérifie à nouveau.
const can = (permission: string) => // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  Boolean( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    me.value?.permissions.includes("*") || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    me.value?.permissions.includes(permission) || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    me.value?.permissions.includes(permission.split(".")[0] + ".manage"), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Un compte lié à un étudiant affiche les parcours d'usage étudiants.
const studentMode = computed(() => Boolean(me.value?.studentId)); // Prépare cette variable ou cette fonction pour le traitement en cours.
// Le menu rassemble uniquement les modules accessibles au profil.
const menu = computed(() => [ // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  "Accueil", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  ...(can("academic.view") ? ["Années", "Classes", "Programmes"] : []), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ...(can("students.manage") // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ? ["Étudiants", "Inscriptions"] // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    : studentMode.value // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ? ["Ma scolarité"] // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      : []), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  "Notes & bulletins", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  "Présences", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  "Justificatifs", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  "Discipline", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  "Documents & épreuves", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  ...(can("users.manage") ? ["Utilisateurs"] : []), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ...(can("roles.manage") ? ["Rôles & permissions"] : []), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ...(can("audit.view") ? ["Journal d’audit"] : []), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
]); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
// L'année ouverte porte le message de bienvenue.
const activeYear = computed(() => store.years.find((y) => y.status === "OPEN")); // Prépare cette variable ou cette fonction pour le traitement en cours.
// La classe sélectionnée est recherchée par UUID.
const currentClass = computed( // Prépare cette variable ou cette fonction pour le traitement en cours.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    store.classes.find((c) => c.id === selectedClass.value), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'inscription sélectionnée est recherchée par UUID.
const currentEnrollment = computed( // Prépare cette variable ou cette fonction pour le traitement en cours.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    store.enrollments.find((e) => e.id === selectedEnrollment.value), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Un mandat de classe autorise les boutons réservés aux responsables.
const officer = computed( // Prépare cette variable ou cette fonction pour le traitement en cours.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    store.officers.find( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      (o) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        o.classId === selectedClass.value && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        o.studentId === me.value?.studentId, // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La permission d'affichage peut aussi venir d'une délégation non expirée.
const displayAllowed = computed( // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    can("attendance.manage") || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    Boolean(officer.value) || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    store.delegations.some( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      (d) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        d.classId === selectedClass.value && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        d.grantee === me.value?.studentId && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        !d.revoked && // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        new Date(d.expires).getTime() > Date.now(), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les programmes compatibles sont filtrés selon la salle physique et le parcours du dossier.
const compatibleCurricula = computed( // Prépare cette variable ou cette fonction pour le traitement en cours.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    store.curricula.filter( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      (c) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        (!currentClass.value || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          (c.level === currentClass.value.level && // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            c.speciality === currentClass.value.speciality)) && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        (!studentMode.value || c.track === me.value?.student?.track), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les semestres et UE sont lus dans le programme versionné.
const curriculumContent = computed(() => { // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  const c = store.curricula.find((c) => c.id === selectedCurriculum.value); // Prépare cette variable ou cette fonction pour le traitement en cours.
  return c ? JSON.parse(c.content) : null; // Renvoie cette valeur au composant ou à la fonction qui l’a demandée.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les EC servent au choix d'un cours pour une séance de présence.
const ecs = computed< // Prépare cette variable ou cette fonction pour le traitement en cours.
  Row[] // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
> // Prépare cette variable ou cette fonction pour le traitement en cours.
( // Ouvre cette construction dont les éléments sont définis juste en dessous.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    curriculumContent.value?.semesters.flatMap( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      (s: Row) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        s.ues.flatMap((u: Row) => u.ecs), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ) || [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les lignes d'appel peuvent être filtrées par classe, date et semaine.
const reportRows = computed( // Prépare cette variable ou cette fonction pour le traitement en cours.
  () => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    store.records.filter( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      (r) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        (!selectedClass.value || r.classId === selectedClass.value) && // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        (!reportDate.value || r.checked.slice(0, 10) === reportDate.value) && // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        (!weekStart.value || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          (new Date(r.checked).getTime() >= // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            new Date(weekStart.value).getTime() && // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            new Date(r.checked).getTime() < // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
              new Date(weekStart.value).getTime() + 7 * 86400000)), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les collections sont filtrées par texte en conservant les objets originaux.
const filtered = (rows: Row[]) => // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  rows.filter( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    (r) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      JSON.stringify(r).toLowerCase().includes(search.value.toLowerCase()), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Certaines structures figées sont sérialisées en PascalCase ; on les normalise pour Vue.
function normalize(value: any): any { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  if (Array.isArray(value)) return value.map(normalize); // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  if (value && typeof value === "object") // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    return Object.fromEntries( // Renvoie cette valeur au composant ou à la fonction qui l’a demandée.
      // Renvoie cette valeur au composant ou à la fonction qui l’a demandée.
      Object.entries(value).map(([k, v]) => [ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        k[0]!.toLowerCase() + k.slice(1), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        normalize(v), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ]), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  return value; // Renvoie cette valeur au composant ou à la fonction qui l’a demandée.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Une erreur API est transformée en message utile, et une session expirée retourne à la connexion.
async function api(path: string, method = "GET", body?: any): Promise<any> { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  try { // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
    // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
    return await $fetch("/api" + path, { // Renvoie cette valeur au composant ou à la fonction qui l’a demandée.
      // Renvoie cette valeur au composant ou à la fonction qui l’a demandée.
      method: method as any, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      body, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      headers: { "X-Portal-Request": "1" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  } catch (e: any) { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    if (e.status === 401) me.value = null; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    throw new Error(e.data?.error || e.message || "Échec de la requête"); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Encadre chaque action pour bloquer les doubles clics et rendre les erreurs visibles.
async function run(action: () => Promise<void>) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  if (busy.value) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  busy.value = true; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  message.value = ""; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  try { // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
    // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
    await action(); // Attend la réponse de cette opération avant de poursuivre le traitement.
  } catch (e: any) { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    message.value = e.message; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  } finally { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    busy.value = false; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Recharge les données accessibles en inspectant chaque réponse indépendamment.
async function refresh() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Le serveur donne une identité à jour et ses permissions actuelles.
  me.value = await api("/me"); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  // Le socle est commun aux comptes étudiants et administratifs.
  const paths: Record<string, string> = { // Prépare cette variable ou cette fonction pour le traitement en cours.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    years: "/years", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    classes: "/classes", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    curricula: "/curricula", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    enrollments: "/enrollments", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    sessions: "/attendance/sessions", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    records: "/attendance/records", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    justifications: "/justifications", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    documents: "/documents", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    officers: "/officers", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    delegations: "/delegations", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    notifications: "/notifications", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  }; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  // Les dossiers privés des autres étudiants exigent le droit administratif.
  if (can("students.view")) paths.students = "/students"; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  // Les listes des comptes ne sont chargées que pour les administrateurs autorisés.
  if (can("users.manage")) paths.users = "/users"; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  // Le catalogue de profils exige le droit de gestion des rôles.
  if (can("roles.manage")) paths.roles = "/roles"; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  // Le journal exige sa propre permission de consultation.
  if (can("audit.view")) paths.audit = "/audit"; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  // Les requêtes indépendantes partent en parallèle sans bloquer les autres modules.
  const responses = await Promise.allSettled( // Prépare cette variable ou cette fonction pour le traitement en cours.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    Object.entries(paths).map(async ([key, path]) => { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      store[key] = await api(path); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  // Une erreur de chargement reste visible au lieu d'être ignorée.
  for (const response of responses) // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    if (response.status === "rejected") message.value = response.reason.message; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  // Le registre de discipline retourne aussi des scores calculés.
  const discipline = await api("/discipline"); // Prépare cette variable ou cette fonction pour le traitement en cours.
  store.discipline = discipline.events; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  // Sélectionne une inscription active ou la dernière inscription historique.
  if (!store.enrollments.some((e) => e.id === selectedEnrollment.value)) // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    selectedEnrollment.value = // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      store.enrollments.find((e) => e.status === "ACTIVE")?.id || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      store.enrollments[0]?.id || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ""; // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  // Sélectionne la classe de cette inscription ou une classe de l'année ouverte.
  if (!store.classes.some((c) => c.id === selectedClass.value)) // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    selectedClass.value = // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      currentEnrollment.value?.classId || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      store.classes.find((c) => c.yearId === activeYear.value?.id)?.id || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      store.classes[0]?.id || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ""; // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  // Sélectionne un programme adapté à la classe et aux choix académiques.
  if (!compatibleCurricula.value.some((c) => c.id === selectedCurriculum.value)) // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    selectedCurriculum.value = compatibleCurricula.value[0]?.id || ""; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La connexion efface le secret du formulaire après réception du cookie.
async function signIn() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    await api("/auth/login", "POST", login); // Attend la réponse de cette opération avant de poursuivre le traitement.
    login.password = ""; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    login.otp = ""; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'activation affiche une fois les identifiants générés par le serveur.
async function activate() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    credentials.value = await api("/auth/activate", "POST", activation); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    activation.activationCode = ""; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La déconnexion vide les collections privées avant de revenir à l'écran public.
async function signOut() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    await api("/auth/logout", "POST"); // Attend la réponse de cette opération avant de poursuivre le traitement.
    me.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    for (const key of Object.keys(store)) store[key] = []; // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    token.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    results.value = []; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    credentials.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Ouvre un formulaire avec des valeurs initiales et sa fonction de validation.
function open( // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  title: string, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  fields: Field[], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  submit: (data: Row) => Promise<void>, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  initial: Row = {}, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
) { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  for (const key of Object.keys(form)) delete form[key]; // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
  for (const field of fields) // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    form[field.key] = // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      initial[field.key] ?? (field.type === "multiselect" ? [] : ""); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  modal.value = { title, fields, submit }; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Enregistre le formulaire puis recharge l'état confirmé par le serveur.
async function submitModal() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    if (!modal.value) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    const data = { ...form }; // Prépare cette variable ou cette fonction pour le traitement en cours.
    for (const field of modal.value.fields) { // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
      // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
      if (field.type === "number") data[field.key] = Number(data[field.key]); // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      if (field.type === "json") // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
        // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
        data[field.key] = JSON.parse(data[field.key] || "{}"); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    await modal.value.submit(data); // Attend la réponse de cette opération avant de poursuivre le traitement.
    modal.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
    message.value = "Opération enregistrée."; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Transforme une collection en choix accessibles de formulaire.
const options = (rows: Row[], label: string) => // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  rows.map((r) => ({ value: r.id, label: r[label] })); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
// Prépare la création d'une année avec ses dates et son accueil.
function createYear() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Créer une année académique", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "code", label: "Année (ex. 2026-2027)", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "start", label: "Début", type: "date", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "end", label: "Fin", type: "date", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "welcome", // Nomme le champ transmis au serveur : "welcome".
        label: "Message de bienvenue", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "textarea", // Choisit le type de contrôle adapté à cette donnée.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/years", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le niveau et la spécialité proposés viennent des programmes enregistrés.
function createClass() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const unique = (key: string) => // Prépare cette variable ou cette fonction pour le traitement en cours.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    [...new Set(store.curricula.map((c) => c[key]))].map((x) => ({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      value: x, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      label: x, // Donne à ce contrôle son libellé visible par l’utilisateur.
    })); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Créer une classe physique", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "yearId", // Nomme le champ transmis au serveur : "yearId".
        label: "Année", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options( // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          store.years.filter( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            (y) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
              // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
              ["DRAFT", "OPEN", "CLOSED"].includes(y.status), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
          "code", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "name", label: "Nom de la salle / classe", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "historical", // Nomme le champ transmis au serveur : "historical".
        label: "Classe d’une ancienne année clôturée", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: [ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          { value: false, label: "Non" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          { value: true, label: "Oui : migration historique" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "level", // Nomme le champ transmis au serveur : "level".
        label: "Niveau", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: unique("level"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "speciality", // Nomme le champ transmis au serveur : "speciality".
        label: "Spécialité", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: unique("speciality"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "tracks", // Nomme le champ transmis au serveur : "tracks".
        label: "Parcours autorisés", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "multiselect", // Choisit le type de contrôle adapté à cette donnée.
        options: unique("track"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/classes", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      yearId: activeYear.value?.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      historical: false, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      tracks: [...new Set(store.curricula.map((c) => c.track))], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les coordonnées et personnes à prévenir sont regroupées dans un objet lisible.
function createStudent() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Créer un dossier étudiant", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "firstName", label: "Prénoms", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "lastName", label: "Nom", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "track", // Nomme le champ transmis au serveur : "track".
        label: "Parcours", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: [...new Set(store.curricula.map((c) => c.track))].map((x) => ({ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          value: x, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          label: x, // Donne à ce contrôle son libellé visible par l’utilisateur.
        })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "email", label: "Email", type: "email" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "dateOfBirth", label: "Date de naissance", type: "date" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "placeOfBirth", label: "Lieu de naissance" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "gender", label: "Sexe" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "phone", label: "Téléphone" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "nationality", label: "Nationalité" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "address", label: "Adresse" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "district", label: "Quartier" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "city", label: "Ville" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "emergencyName", label: "Personne à prévenir" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "emergencyPhone", label: "Téléphone de la personne à prévenir" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "emergencyRelationship", label: "Lien avec cette personne" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      const { firstName, lastName, track, ...profile } = d; // Prépare cette variable ou cette fonction pour le traitement en cours.
      credentials.value = await api("/students", "POST", { // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        firstName, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        lastName, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        track, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        profile, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La modification d'un dossier conserve son matricule permanent.
function editStudent(s: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const labels: Row = { // Prépare cette variable ou cette fonction pour le traitement en cours.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    email: "Email", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    dateOfBirth: "Date de naissance", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    placeOfBirth: "Lieu de naissance", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    gender: "Sexe", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    phone: "Téléphone", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    nationality: "Nationalité", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    address: "Adresse", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    district: "Quartier", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    city: "Ville", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    emergencyName: "Personne à prévenir", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    emergencyPhone: "Téléphone du contact d’urgence", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    emergencyRelationship: "Lien avec cette personne", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  }; // Associe chaque donnée à son libellé compréhensible.
  const fields: Field[] = [ // Prépare cette variable ou cette fonction pour le traitement en cours.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    { key: "firstName", label: "Prénoms", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    { key: "lastName", label: "Nom", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ...Object.entries(labels).map(([key, label]) => ({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      key, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      label: String(label), // Donne à ce contrôle son libellé visible par l’utilisateur.
      type: key === "dateOfBirth" ? "date" : key === "email" ? "email" : "text", // Choisit le type de contrôle adapté à cette donnée.
    })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ]; // Les mêmes coordonnées restent modifiables après inscription.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Modifier le dossier étudiant", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    fields, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      const { firstName, lastName, ...profile } = d; // Prépare cette variable ou cette fonction pour le traitement en cours.
      await api(`/students/${s.id}`, "PUT", { firstName, lastName, profile }); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { ...JSON.parse(s.profile), firstName: s.firstName, lastName: s.lastName }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Le matricule et le parcours sont conservés sans risque de modifier les anciens bulletins.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'abandon et la reprise sont des changements de statut traçables.
function statusStudent(s: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Changer le statut du dossier", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "status", // Nomme le champ transmis au serveur : "status".
        label: "Statut", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: [ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          "ACTIVE", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "INACTIVE", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "WITHDRAWN", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "SUSPENDED", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "GRADUATED", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        ].map((x) => ({ value: x, label: x })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "reason", label: "Motif", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/students/${s.id}/disable`, "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { status: s.status }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Une inscription administrative choisit un programme précis pour le dossier.
function enrollStudent() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Inscrire un étudiant", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "studentId", // Nomme le champ transmis au serveur : "studentId".
        label: "Étudiant", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: store.students.map((s) => ({ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          value: s.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          label: `${s.number} · ${s.firstName} ${s.lastName}`, // Donne à ce contrôle son libellé visible par l’utilisateur.
        })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "classId", // Nomme le champ transmis au serveur : "classId".
        label: "Classe", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options(store.classes, "name"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "curriculumId", // Nomme le champ transmis au serveur : "curriculumId".
        label: "Version du programme", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options(store.curricula, "name"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "historical", // Nomme le champ transmis au serveur : "historical".
        label: "Importer une inscription dans une année clôturée", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: [ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          { value: false, label: "Non : inscription courante" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          { value: true, label: "Oui : historique" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/enrollments", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      historical: false, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      classId: selectedClass.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      curriculumId: selectedCurriculum.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Un étudiant connu n'a qu'à fournir le code partagé et choisir son programme compatible.
function joinClass() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Intégrer ma nouvelle classe", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "code", label: "Code partagé de classe", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "curriculumId", // Nomme le champ transmis au serveur : "curriculumId".
        label: "Programme de mon parcours", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options( // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          store.curricula.filter((c) => c.track === me.value?.student?.track), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          "name", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/enrollments/join", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'ouverture d'année est une opération explicitement décrite avant validation.
function openYear(y: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    `Ouvrir ${y.code} : clôturer l’ancienne année et déconnecter les étudiants`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    async () => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/years/${y.id}/open`, "POST"); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le code partagé est affiché une seule fois après sa régénération.
async function shareClass(c: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    credentials.value = await api(`/classes/${c.id}/join-code`, "POST"); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Un mandat est choisi parmi les étudiants actifs de la salle.
function assignOfficer() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Désigner un responsable de classe", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "classId", // Nomme le champ transmis au serveur : "classId".
        label: "Classe", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options(store.classes, "name"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "studentId", // Nomme le champ transmis au serveur : "studentId".
        label: "Étudiant", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: store.enrollments // Définit les choix autorisés présentés par ce champ.
          .filter( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            (e) => e.classId === selectedClass.value && e.status === "ACTIVE", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          ) // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          .map((e) => ({ value: e.studentId, label: e.studentName })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "position", // Nomme le champ transmis au serveur : "position".
        label: "Responsabilité", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: ["DELEGATE", "VICE_DELEGATE", "SECRETARY"].map((x) => ({ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          value: x, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          label: x, // Donne à ce contrôle son libellé visible par l’utilisateur.
        })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/officers", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { classId: selectedClass.value }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Un transfert ajoute une inscription et conserve l'ancienne.
function transfer(e: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Transférer vers une autre classe", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "classId", // Nomme le champ transmis au serveur : "classId".
        label: "Classe cible", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options( // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          store.classes.filter( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            (c) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
              // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
              c.yearId === e.yearId && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
              c.level === e.level && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
              c.speciality === e.speciality && // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
              c.id !== e.classId, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
          "name", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "reason", label: "Motif du transfert", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/enrollments/${e.id}/transfer`, "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les résultats sont normalisés après la lecture des instantanés JSON.
async function loadResults() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    if (selectedEnrollment.value) // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      results.value = normalize( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        await api(`/enrollments/${selectedEnrollment.value}/results`), // Attend la réponse de cette opération avant de poursuivre le traitement.
      ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    progress.value = await api( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      `/enrollments/${selectedEnrollment.value}/progress`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ranks.value = []; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le classement semestriel ou annuel suit les paramètres de l'inscription choisie.
async function loadRanks(annual = false) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    rankAnnual.value = annual; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    ranks.value = await api( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      `/rankings?enrollmentId=${selectedEnrollment.value}&semester=${semester.value}&annual=${annual}`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La correction d'une note doit toujours fournir un motif.
function editGrade(ec: Row, sem: number) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    `Rectifier ${ec.name}`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "value", label: "Note / 20", type: "number", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "reason", label: "Motif de rectification", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/enrollments/${selectedEnrollment.value}/grades`, "POST", { // Attend la réponse de cette opération avant de poursuivre le traitement.
        // Attend la réponse de cette opération avant de poursuivre le traitement.
        ...d, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        semester: sem, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        ecId: ec.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      results.value = normalize( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        await api(`/enrollments/${selectedEnrollment.value}/results`), // Attend la réponse de cette opération avant de poursuivre le traitement.
      ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { value: ec.note }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le repêchage explique sa décision sans changer les notes.
function rescue(ue: Row, sem: number) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    `Repêcher l’UE ${ue.name}`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [{ key: "reason", label: "Motif de la décision", required: true }], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/enrollments/${selectedEnrollment.value}/rescue`, "POST", { // Attend la réponse de cette opération avant de poursuivre le traitement.
        // Attend la réponse de cette opération avant de poursuivre le traitement.
        ...d, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        ueId: ue.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        semester: sem, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      results.value = normalize( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        await api(`/enrollments/${selectedEnrollment.value}/results`), // Attend la réponse de cette opération avant de poursuivre le traitement.
      ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'import lit le fichier choisi et demande une prévisualisation au serveur.
async function importCsv(event: Event) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const file = (event.target as HTMLInputElement).files?.[0]; // Prépare cette variable ou cette fonction pour le traitement en cours.
  if (!file) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    preview.value = await api( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      `/classes/${selectedClass.value}/grades/import`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      "POST", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        semester: semester.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        curriculumId: selectedCurriculum.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        csv: await file.text(), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'import de présence vérifie les matricules et statuts avant de proposer sa confirmation.
async function importAttendance(event: Event, sessionId: string) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const file = (event.target as HTMLInputElement).files?.[0]; // Prépare cette variable ou cette fonction pour le traitement en cours.
  if (!file) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    const csv = await file.text(); // Prépare cette variable ou cette fonction pour le traitement en cours.
    const checked = await api( // Prépare cette variable ou cette fonction pour le traitement en cours.
      // Prépare cette variable ou cette fonction pour le traitement en cours.
      `/attendance/sessions/${sessionId}/import`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      "POST", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
      { csv, confirm: false }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    preview.value = { ...checked, kind: "attendance", sessionId, csv }; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Une confirmation de présence repasse par les validations du serveur et fournit un motif.
async function confirmPreview() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    if (!preview.value) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    if (preview.value.kind === "attendance") // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      await api( // Attend la réponse de cette opération avant de poursuivre le traitement.
        // Attend la réponse de cette opération avant de poursuivre le traitement.
        `/attendance/sessions/${preview.value.sessionId}/import`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        "POST", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        { // Ouvre cette construction dont les éléments sont définis juste en dessous.
          // Ouvre cette construction dont les éléments sont définis juste en dessous.
          csv: preview.value.csv, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          confirm: true, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          reason: "Import administratif depuis le modèle de présence", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    else await api(`/grade-imports/${preview.value.id}/confirm`, "POST", {}); // Traite le cas alternatif lorsque le précédent ne s’applique pas.
    const attendance = preview.value.kind === "attendance"; // Prépare cette variable ou cette fonction pour le traitement en cours.
    preview.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
    message.value = attendance // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      ? "Présences importées et changements inscrits au journal." // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      : "Notes importées en brouillon. Publiez séparément après vérification."; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La proclamation est une validation séparée de l'import.
function publish() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    `Publier les résultats du semestre ${semester.value} pour toute la classe`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    async () => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/classes/${selectedClass.value}/publish`, "POST", { // Attend la réponse de cette opération avant de poursuivre le traitement.
        // Attend la réponse de cette opération avant de poursuivre le traitement.
        semester: semester.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les programmes sont clonés avec de nouveaux UUID afin de préserver les versions précédentes.
function cloneCurriculum(c: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const content = JSON.parse(c.content); // La version existante reste inchangée dans la base.
  content.id = crypto.randomUUID(); // La copie représente un autre programme.
  content.version = `${content.version}-nouvelle`; // Le responsable définit ensuite son libellé de version.
  for (const semester of content.semesters) // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    for (const unit of semester.ues) { // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
      // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
      unit.id = crypto.randomUUID(); // Chaque UE reçoit une clé technique propre à la nouvelle version.
      for (const element of unit.ecs) element.id = crypto.randomUUID(); // Les notes anciennes restent liées aux anciennes matières.
    } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  programmeEditor.value = content; // Ouvre l'éditeur de coefficients, crédits et volumes horaires.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Enregistre l'ensemble des règles comme une nouvelle version après validation serveur.
async function saveProgramme() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    for (const s of programmeEditor.value!.semesters) // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
      // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
      for (const u of s.ues) if (u.credits === "") u.credits = null; // Parcourt les éléments indiqués pour appliquer le même traitement à chacun.
    await api("/curricula", "POST", programmeEditor.value); // Attend la réponse de cette opération avant de poursuivre le traitement.
    programmeEditor.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
    message.value = "Nouvelle version du programme enregistrée."; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'ajout d'une UE fournit des valeurs initiales modifiables par le responsable.
function addUnit(s: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  s.ues.push({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    id: crypto.randomUUID(), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    code: "", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    name: "Nouvelle UE", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    credits: null, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ecs: [], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'ajout d'une matière propose un coefficient positif avant sa validation.
function addElement(u: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  u.ecs.push({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    id: crypto.randomUUID(), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    code: "", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    name: "Nouvelle matière", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    cm: 0, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    td: 0, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    tp: 0, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    coefficient: 1, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les équivalences de crédits sont approuvées explicitement par un responsable académique.
function approveEquivalence() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const units = store.curricula.flatMap( // Prépare cette variable ou cette fonction pour le traitement en cours.
    (c) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Prépare cette variable ou cette fonction pour le traitement en cours.
      JSON.parse(c.content).semesters.flatMap( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        (s: Row) => // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          s.ues.map((u: Row) => ({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            value: u.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            label: `${c.name} · S${s.number} · ${u.name}`, // Donne à ce contrôle son libellé visible par l’utilisateur.
          })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Approuver une équivalence d’UE", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "sourceUe", label: "Ancienne UE", options: units, required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "targetUe", // Nomme le champ transmis au serveur : "targetUe".
        label: "UE de la nouvelle version", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: units, // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "reason", // Nomme le champ transmis au serveur : "reason".
        label: "Référence de la décision pédagogique", // Donne à ce contrôle son libellé visible par l’utilisateur.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/equivalences", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'ouverture d'une séance choisit un EC réel du programme de la classe.
function openSession() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Ouvrir une séance de cours", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "ecId", // Nomme le champ transmis au serveur : "ecId".
        label: "Cours", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: ecs.value.map((e) => ({ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          value: e.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          label: `${e.code} · ${e.name}`, // Donne à ce contrôle son libellé visible par l’utilisateur.
        })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/attendance/sessions", "POST", { // Attend la réponse de cette opération avant de poursuivre le traitement.
        // Attend la réponse de cette opération avant de poursuivre le traitement.
        ...d, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        classId: selectedClass.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// L'affichage du QR est réservé aux responsables et aux personnes déléguées.
async function showToken(id: string) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    selectedSession.value = id; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    token.value = await api(`/attendance/sessions/${id}/token`); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    const QRCode = await import("qrcode"); // Prépare cette variable ou cette fonction pour le traitement en cours.
    qrImage.value = await QRCode.toDataURL(token.value!.qr, { // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      width: 240, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      margin: 2, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le pointage manuel saisit exactement le même code que celui contenu dans le QR.
function checkIn(id: string, self = false) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    self ? "Je suis présent à ce cours" : "Enregistrer ma présence", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    self // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ? [] // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      : [ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          { // Ouvre cette construction dont les éléments sont définis juste en dessous.
            // Ouvre cette construction dont les éléments sont définis juste en dessous.
            key: "code", // Nomme le champ transmis au serveur : "code".
            label: "Code dynamique affiché en classe", // Donne à ce contrôle son libellé visible par l’utilisateur.
            required: true, // Indique si le navigateur doit exiger une valeur avant validation.
          }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/attendance/check-in", "POST", { // Attend la réponse de cette opération avant de poursuivre le traitement.
        // Attend la réponse de cette opération avant de poursuivre le traitement.
        ...d, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        sessionId: id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        method: self ? "OFFICER_SELF" : "CODE", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le scanner utilise la caméra uniquement après l'action explicite de l'étudiant.
async function scanQr() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    const { BrowserQRCodeReader } = await import("@zxing/browser"); // Prépare cette variable ou cette fonction pour le traitement en cours.
    const video = document.querySelector<HTMLVideoElement>("#scanner")!; // Prépare cette variable ou cette fonction pour le traitement en cours.
    video.hidden = false; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    const reader = new BrowserQRCodeReader(); // Prépare cette variable ou cette fonction pour le traitement en cours.
    const controls = await reader.decodeFromVideoDevice( // Prépare cette variable ou cette fonction pour le traitement en cours.
      // Prépare cette variable ou cette fonction pour le traitement en cours.
      undefined, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      video, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      async (result, _error, controls) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
        // Définit l’action exécutée lorsque le formulaire est confirmé.
        if (!result) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
        controls.stop(); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        video.hidden = true; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        const pieces = result.getText().split(":"); // Prépare cette variable ou cette fonction pour le traitement en cours.
        if (pieces.length !== 3 || pieces[0] !== "iai") { // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
          // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
          message.value = "QR non reconnu."; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
          return; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        try { // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
          // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
          await api("/attendance/check-in", "POST", { // Attend la réponse de cette opération avant de poursuivre le traitement.
            // Attend la réponse de cette opération avant de poursuivre le traitement.
            sessionId: pieces[1], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            code: pieces[2], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            method: "QR", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
          await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
          message.value = "Présence enregistrée."; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        } catch (e: any) { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          message.value = e.message; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    setTimeout(() => { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      controls.stop(); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      video.hidden = true; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    }, 60000); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le mandat temporaire est limité à l'affichage du QR et expire côté serveur.
async function delegate() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    roster.value = await api(`/classes/${selectedClass.value}/roster`); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Déléguer l’affichage QR pour 24 h", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "studentId", // Nomme le champ transmis au serveur : "studentId".
        label: "Étudiant de la classe", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: roster.value.map((s) => ({ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          value: s.studentId, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          label: `${s.name} · ${s.number}`, // Donne à ce contrôle son libellé visible par l’utilisateur.
        })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/delegations", "POST", { ...d, classId: selectedClass.value }); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le justificatif est lié au cours et à une éventuelle pièce jointe déjà téléversée.
function justify() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Déclarer une absence", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "sessionId", // Nomme le champ transmis au serveur : "sessionId".
        label: "Séance de cours", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: options(store.sessions, "course"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "reason", // Nomme le champ transmis au serveur : "reason".
        label: "Motif de l’absence", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "textarea", // Choisit le type de contrôle adapté à cette donnée.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "documentId", // Nomme le champ transmis au serveur : "documentId".
        label: "Pièce jointe (facultatif)", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: [ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          { value: "", label: "Aucune" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          ...options( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            store.documents.filter((d) => d.category === "JUSTIFICATION"), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
            "title", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          ), // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/justifications", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le traitement CPE conserve le commentaire de décision.
function decide(j: Row, accepted: boolean) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    accepted ? "Accepter le justificatif" : "Rejeter le justificatif", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [{ key: "comment", label: "Commentaire de décision", required: true }], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(`/justifications/${j.id}/decision`, "POST", { ...d, accepted }); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Une sanction ajoute un événement au registre de discipline.
function sanction() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Ajouter un événement disciplinaire", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "enrollmentId", // Nomme le champ transmis au serveur : "enrollmentId".
        label: "Étudiant inscrit", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: store.enrollments.map((e) => ({ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          value: e.id, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          label: `${e.studentName} · ${e.className}`, // Donne à ce contrôle son libellé visible par l’utilisateur.
        })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "semester", label: "Semestre", type: "number", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "delta", // Nomme le champ transmis au serveur : "delta".
        label: "Points (ex. -1)", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "number", // Choisit le type de contrôle adapté à cette donnée.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "reason", label: "Motif", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api("/discipline", "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { semester: semester.value, delta: -1 }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le dépôt de fichier attend des métadonnées pédagogiques lisibles.
function uploadDocument() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Téléverser un document", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "title", label: "Titre", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "category", // Nomme le champ transmis au serveur : "category".
        label: "Nature", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: [ // Définit les choix autorisés présentés par ce champ.
          // Définit les choix autorisés présentés par ce champ.
          "COURSE", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "TD", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "TP", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "DEVOIR", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "EXAMEN", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "RATTRAPAGE", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "CORRIGE", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "SUPPORT", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
          "JUSTIFICATION", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        ].map((x) => ({ value: x, label: x })), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "year", // Nomme le champ transmis au serveur : "year".
        label: "Année", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: store.years.map((y) => ({ value: y.code, label: y.code })), // Définit les choix autorisés présentés par ce champ.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { key: "level", label: "Niveau" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "speciality", label: "Spécialité" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { key: "semester", label: "Semestre", type: "number" }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "subject", // Nomme le champ transmis au serveur : "subject".
        label: "Matière", // Donne à ce contrôle son libellé visible par l’utilisateur.
        options: ecs.value.map((e) => ({ value: e.name, label: e.name })), // Définit les choix autorisés présentés par ce champ.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "file", // Nomme le champ transmis au serveur : "file".
        label: "Fichier (10 Mo maximum)", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "file", // Choisit le type de contrôle adapté à cette donnée.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      const body = new FormData(); // Prépare cette variable ou cette fonction pour le traitement en cours.
      body.append("file", d.file); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      body.append("title", d.title); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      body.append("category", d.category); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      body.append("classId", selectedClass.value); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      body.append( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        "metadata", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
        JSON.stringify({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          year: d.year, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          level: d.level, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          speciality: d.speciality, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          semester: d.semester, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          subject: d.subject, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        }), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      await api("/documents", "POST", body); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      category: studentMode.value ? "JUSTIFICATION" : "TD", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      year: activeYear.value?.code, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      level: currentClass.value?.level, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      speciality: currentClass.value?.speciality, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      semester: semester.value, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les rôles sont édités par une liste JSON de permissions du catalogue documenté.
function roleForm(r?: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    r ? "Modifier le rôle" : "Créer un rôle", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "name", label: "Nom du profil", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "permissions", // Nomme le champ transmis au serveur : "permissions".
        label: "Permissions", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "multiselect", // Choisit le type de contrôle adapté à cette donnée.
        options: permissionOptions, // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      await api(r ? `/roles/${r.id}` : "/roles", r ? "PUT" : "POST", d); // Attend la réponse de cette opération avant de poursuivre le traitement.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      name: r?.name, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      permissions: r ? JSON.parse(r.permissions) : ["academic.manage"], // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La création de compte affiche le mot de passe uniquement après sa génération.
function createUser() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  open( // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    // Ouvre le formulaire métier avec les champs et la validation définis ci-dessous.
    "Créer un utilisateur", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      { key: "username", label: "Identifiant", required: true }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "roles", // Nomme le champ transmis au serveur : "roles".
        label: "Profils du compte", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "multiselect", // Choisit le type de contrôle adapté à cette donnée.
        options: options(store.roles, "name"), // Définit les choix autorisés présentés par ce champ.
        required: true, // Indique si le navigateur doit exiger une valeur avant validation.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
      { // Ouvre cette construction dont les éléments sont définis juste en dessous.
        // Ouvre cette construction dont les éléments sont définis juste en dessous.
        key: "password", // Nomme le champ transmis au serveur : "password".
        label: "Mot de passe (vide = généré)", // Donne à ce contrôle son libellé visible par l’utilisateur.
        type: "password", // Choisit le type de contrôle adapté à cette donnée.
      }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    ], // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    async (d) => { // Définit l’action exécutée lorsque le formulaire est confirmé.
      // Définit l’action exécutée lorsque le formulaire est confirmé.
      credentials.value = await api("/users", "POST", d); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
    }, // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    { roles: [] }, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La réinitialisation révoque les cookies du compte et affiche une fois son nouveau secret.
async function resetPassword(u: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    credentials.value = await api(`/users/${u.id}/reset-password`, "POST", {}); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les gestionnaires de scolarité peuvent remettre un nouveau secret au titulaire du dossier.
async function resetStudent(s: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    credentials.value = await api( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      `/students/${s.id}/reset-password`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      "POST", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
      {}, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le code personnel perdu avant activation est remplacé et affiché une seule fois.
async function regenerateActivation(s: Row) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  await run(async () => { // Attend la réponse de cette opération avant de poursuivre le traitement.
    // Attend la réponse de cette opération avant de poursuivre le traitement.
    credentials.value = await api(`/students/${s.id}/activation-code`, "POST"); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le fichier JSON des identifiants est créé localement sans stockage supplémentaire côté serveur.
function downloadCredentials() { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  download( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    "identifiants-iai.json", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    JSON.stringify(credentials.value, null, 2), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    "application/json", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// La fonction de téléchargement libère l'URL temporaire après utilisation.
function download(name: string, text: string, mime: string) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const url = URL.createObjectURL(new Blob([text], { type: mime })); // Prépare cette variable ou cette fonction pour le traitement en cours.
  const link = document.createElement("a"); // Prépare cette variable ou cette fonction pour le traitement en cours.
  link.href = url; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  link.download = name; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  link.click(); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  URL.revokeObjectURL(url); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les liens d'export natif respectent la classe, la période et le classement consultés.
function exportUrl(kind: string, format: string) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  const parameters = new URLSearchParams({ kind, format }); // Encode proprement les paramètres de téléchargement.
  if (kind === "attendance" && selectedClass.value) // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    parameters.set("classId", selectedClass.value); // Limite le rapport à la salle choisie.
  if (kind === "attendance" && (reportDate.value || weekStart.value)) { // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    const start = reportDate.value || weekStart.value; // Prépare cette variable ou cette fonction pour le traitement en cours.
    parameters.set("start", start); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    parameters.set( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      "end", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
      reportDate.value || // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        new Date(new Date(start).getTime() + 6 * 86400000) // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          .toISOString() // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
          .slice(0, 10), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  } // Le bilan hebdomadaire couvre exactement sept jours UTC.
  if (kind === "rankings") { // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
    parameters.set("enrollmentId", selectedEnrollment.value); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    parameters.set("semester", String(semester.value)); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    parameters.set("annual", String(rankAnnual.value)); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  } // Les quatre groupes L3 viennent du programme de cette inscription.
  return `/api/exports?${parameters}`; // Le navigateur télécharge le fichier après les contrôles de l'API.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le CSV exporté protège les cellules contre les formules interprétées par un tableur.
function exportRows(rows: Row[], filename: string) { // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  // Déclare cette action de l’interface ; son nom décrit l’opération métier.
  if (!rows.length) return; // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
  const keys = Object.keys(rows[0]!); // Prépare cette variable ou cette fonction pour le traitement en cours.
  const escape = (v: any) => // Prépare cette variable ou cette fonction pour le traitement en cours.
    // Prépare cette variable ou cette fonction pour le traitement en cours.
    '"' + // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
    String(v ?? "") // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      .replace(/^[=+@-]/, "'$&") // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      .replaceAll('"', '""') + // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    '"'; // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  download( // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    filename, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    [ // Ouvre cette construction dont les éléments sont définis juste en dessous.
      // Ouvre cette construction dont les éléments sont définis juste en dessous.
      keys.map(escape).join(","), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ...rows.map((r) => keys.map((k) => escape(r[k])).join(",")), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ].join("\r\n"), // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    "text/csv", // Fournit cette valeur ou ce libellé au contrôle défini autour de cette liste.
  ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
} // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Les statuts techniques ont des libellés français pour les étudiants.
const statusLabel = (s: string) => // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Ouvre cette construction dont les éléments sont définis juste en dessous.
  ( // Ouvre cette construction dont les éléments sont définis juste en dessous.
    ({ // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      VALIDATED: "Validée", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      VALIDATED_BY_RESCUE: "Validée par repêchage", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      VALIDATED_PREVIOUSLY: "Acquise antérieurement", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      NOT_VALIDATED: "Non validée", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      INCOMPLETE: "Notes incomplètes", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      OPEN: "Ouverte", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      CLOSED: "Clôturée", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      DRAFT: "Brouillon", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ACTIVE: "Actif", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      PRESENT: "Présent", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ABSENT: "Absent", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      EXCUSED: "Absence justifiée", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      PENDING: "En attente", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      ACCEPTED: "Accepté", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      REJECTED: "Rejeté", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      APPROVED: "Approuvé", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      WITHDRAWN: "Abandon", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      INACTIVE: "Inactif", // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    }) as Row // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  ) // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
  [s] || s; // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
// L'affichage des nombres conserve deux décimales sans changer les calculs métier.
const number = (n: any) => // Prépare cette variable ou cette fonction pour le traitement en cours.
  // Prépare cette variable ou cette fonction pour le traitement en cours.
  n === null || n === undefined // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    ? "—" // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    : Number(n).toLocaleString("fr-FR", { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        maximumFractionDigits: 2, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        minimumFractionDigits: 2, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
      }); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Changer de classe réinitialise le programme et le QR associés à la précédente.
watch(selectedClass, () => { // Observe une sélection réactive pour synchroniser les données qui en dépendent.
  // Observe une sélection réactive pour synchroniser les données qui en dépendent.
  selectedCurriculum.value = compatibleCurricula.value[0]?.id || ""; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  token.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le premier semestre est lu du programme sélectionné et jamais supposé être S1.
watch(selectedCurriculum, () => { // Observe une sélection réactive pour synchroniser les données qui en dépendent.
  // Observe une sélection réactive pour synchroniser les données qui en dépendent.
  semester.value = curriculumContent.value?.semesters[0]?.number || 1; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Changer d'inscription efface les notes précédentes pour empêcher une confusion de dossier.
watch(selectedEnrollment, () => { // Observe une sélection réactive pour synchroniser les données qui en dépendent.
  // Observe une sélection réactive pour synchroniser les données qui en dépendent.
  results.value = []; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  ranks.value = []; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  progress.value = []; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Une tentative de reprise de session est effectuée à l'ouverture de l'interface.
onMounted(async () => { // Lance ce traitement lorsque la page est affichée dans le navigateur.
  // Lance ce traitement lorsque la page est affichée dans le navigateur.
  try { // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
    // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
    await refresh(); // Attend la réponse de cette opération avant de poursuivre le traitement.
  } catch { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    me.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
  } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  timer = setInterval(async () => { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
    if (token.value && me.value) { // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
      try { // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
        // Encadre l’appel susceptible d’échouer pour rendre son erreur lisible.
        token.value = await api( // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
          // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
          `/attendance/sessions/${selectedSession.value}/token`, // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        ); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
        const QRCode = await import("qrcode"); // Prépare cette variable ou cette fonction pour le traitement en cours.
        qrImage.value = await QRCode.toDataURL(token.value!.qr); // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      } catch (e: any) { // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
        token.value = null; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
        message.value = e.message; // Met à jour cette donnée réactive ; Vue répercute la nouvelle valeur dans l’écran.
      } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
    } // Termine ce bloc ou cette liste avant de poursuivre l’opération.
  }, 5000); // Complète cette expression avec la valeur, la sélection ou les paramètres indiqués.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
// Le temporisateur ne continue pas après la destruction de la page.
onUnmounted(() => { // Libère les ressources lorsque la page est quittée.
  // Libère les ressources lorsque la page est quittée.
  if (timer) clearInterval(timer); // Applique ce traitement uniquement lorsque la condition indiquée est vraie.
}); // Termine ce bloc ou cette liste avant de poursuivre l’opération.
</script>

<template>
  <!-- La connexion expose deux parcours : compte existant ou activation individuelle. -->
  <!-- Affiche une zone principale de la page. Condition d’affichage : !me. -->
<main v-if="!me" class="login-layout">
    <!-- Le panneau institutionnel présente le portail sans données de démonstration. -->
    <!-- Affiche une section de contenu ou formulaire. -->
<section class="login-story">
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="brand">
        <!-- Affiche une élément de texte ou badge. -->
<span class="brand-mark">IAI</span
        ><!-- Affiche une élément de texte ou badge. -->
<span>Institut Africain<!-- Affiche une zone br. -->
<br />d’Informatique · Togo</span>
      </div>
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
        <!-- Affiche une texte de contexte ou message. -->
<p class="eyebrow">VOTRE SCOLARITÉ, AU MÊME ENDROIT</p>
        <!-- Affiche une titre principal de l’écran. -->
<h1>Apprendre.<!-- Affiche une zone br. -->
<br />Progresser.<!-- Affiche une zone br. -->
<br /><!-- Affiche une zone em. -->
<em>Réussir.</em></h1>
        <!-- Affiche une texte de contexte ou message. -->
<p>
          Notes, crédits, présences et ressources pédagogiques : votre parcours
          académique vous accompagne, année après année.
        </p>
      </div>
      <!-- Affiche une information complémentaire. -->
<small>IAI Student Portal · Lomé, Togo</small>
    </section>
    <!-- Un formulaire accessible fournit les champs attendus par l'API. -->
    <!-- Affiche une section de contenu ou formulaire. -->
<section class="login-form">
      <!-- Affiche une texte de contexte ou message. -->
<p class="eyebrow">ESPACE ACADÉMIQUE</p>
      <!-- Affiche une titre du module. -->
<h2>
        {{
          activateMode ? "Activer mon compte" : "Bienvenue sur votre portail"
        }}
      </h2>
      <!-- Affiche une texte de contexte ou message. -->
<p class="muted">
        {{
          activateMode
            ? "Utilisez le matricule et le code personnel remis par la scolarité."
            : "Connectez-vous pour retrouver votre espace."
        }}
      </p>
      <!-- Affiche une formulaire de validation. Condition d’affichage : !activateMode. À l’événement submit, exécute signIn. -->
<form v-if="!activateMode" @submit.prevent="signIn">
        <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Identifiant<!-- Affiche une contrôle de saisie. Synchronise la saisie avec login.username. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
            v-model="login.username"
            autocomplete="username"
            required /></label
        ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Mot de passe<!-- Affiche une contrôle de saisie. Synchronise la saisie avec login.password. Le type de contrôle est password. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
            v-model="login.password"
            type="password"
            autocomplete="current-password"
            required /></label
        ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Code MFA (comptes administratifs)<!-- Affiche une contrôle de saisie. Synchronise la saisie avec login.otp. -->
<input
            v-model="login.otp"
            inputmode="numeric"
            autocomplete="one-time-code"
            maxlength="6" /></label
        ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis busy. -->
<button class="primary" :disabled="busy">
          Se connecter <!-- Affiche une élément de texte ou badge. -->
<span>→</span>
        </button>
      </form>
      <!-- Affiche une formulaire de validation. Utilise cette branche lorsque la branche précédente ne s’affiche pas. À l’événement submit, exécute activate. -->
<form v-else @submit.prevent="activate">
        <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Matricule<!-- Affiche une contrôle de saisie. Synchronise la saisie avec activation.studentNumber. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="activation.studentNumber" required /></label
        ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Code d’activation personnel<!-- Affiche une contrôle de saisie. Synchronise la saisie avec activation.activationCode. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
            v-model="activation.activationCode"
            required /></label
        ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis busy. -->
<button class="primary" :disabled="busy">Activer mon espace →</button>
      </form>
      <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute activateMode = !activateMode. -->
<button class="text-button" @click="activateMode = !activateMode">
        {{
          activateMode
            ? "J’ai déjà un compte"
            : "Première connexion ? Activer mon compte"
        }}
      </button>
      <!-- Affiche une texte de contexte ou message. -->
<p class="muted small">
        Mot de passe oublié ? Contactez l’administration pour une
        réinitialisation.
      </p>
      <!-- Affiche une texte de contexte ou message. Condition d’affichage : message. -->
<p v-if="message" role="alert" class="notice error">{{ message }}</p>
    </section>
  </main>
  <!-- L'espace privé utilise une navigation responsive et des données réelles. -->
  <!-- Affiche une groupe de contrôles ou de contenu. Utilise cette branche lorsque la branche précédente ne s’affiche pas. -->
<div v-else class="workspace">
    <!-- La barre latérale reste commune aux profils tout en adaptant leurs modules. -->
    <!-- Affiche une barre de navigation latérale. -->
<aside class="sidebar">
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="brand">
        <!-- Affiche une élément de texte ou badge. -->
<span class="brand-mark">IAI</span><!-- Affiche une élément de texte ou badge. -->
<span>STUDENT<!-- Affiche une zone br. -->
<br />PORTAL</span>
      </div>
      <!-- Affiche une texte de contexte ou message. -->
<p class="sidebar-caption">
        {{ studentMode ? "MON ESPACE ÉTUDIANT" : "ADMINISTRATION" }}
      </p>
      <!-- Affiche une menu de navigation. -->
<nav>
        <!-- Affiche une commande déclenchée par un clic. Répète cette zone pour : (item, index) in menu. Calcule l’attribut key depuis item. Calcule l’attribut class depuis { active: page === item }. À l’événement click, exécute  page = item; search = ''; . -->
<button
          v-for="(item, index) in menu"
          :key="item"
          :class="{ active: page === item }"
          @click="
            page = item;
            search = '';
          "
        >
          <!-- Affiche une élément de texte ou badge. -->
<span class="nav-icon" aria-hidden="true">{{
            ["◈", "▦", "▤", "◎", "▧", "◇"][index % 6]
          }}</span
          >{{ item }}
        </button>
      </nav>
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="sidebar-bottom">
        <!-- Affiche une élément de texte ou badge. -->
<span class="avatar">{{ me.username.slice(0, 2).toUpperCase() }}</span>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
          <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ me.username }}</strong
          ><!-- Affiche une information complémentaire. -->
<small>{{
            studentMode ? "Étudiant" : "Compte administratif"
          }}</small>
        </div>
        <!-- Affiche une commande déclenchée par un clic. Le libellé accessible est « Déconnexion ». À l’événement click, exécute signOut. -->
<button aria-label="Déconnexion" @click="signOut">↪</button>
      </div>
    </aside>
    <!-- L'en-tête indique l'année et propose un rafraîchissement explicite. -->
    <!-- Affiche une zone principale de la page. -->
<main class="main-content">
      <!-- Affiche une en-tête de contexte. -->
<header class="topbar">
        <!-- Affiche une élément de texte ou badge. -->
<span
          >Institut Africain d’Informatique <!-- Affiche une zone b. -->
<b>/ {{ page }}</b></span
        >
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
          <!-- Affiche une élément de texte ou badge. -->
<span class="year-pill"
            >◷ {{ activeYear?.code || "Aucune année ouverte" }}</span
          ><!-- Affiche une commande déclenchée par un clic. Le libellé accessible est « Actualiser ». À l’événement click, exécute run(refresh). -->
<button
            class="icon-button"
            aria-label="Actualiser"
            @click="run(refresh)"
          >
            ↻
          </button>
        </div>
      </header>
      <!-- Le titre principal fournit le contexte de l'écran. -->
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="page-heading">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
          <!-- Affiche une texte de contexte ou message. -->
<p class="eyebrow">
            {{ studentMode ? "MON PARCOURS" : "GESTION ACADÉMIQUE" }}
          </p>
          <!-- Affiche une titre principal de l’écran. -->
<h1>{{ page }}</h1>
          <!-- Affiche une texte de contexte ou message. -->
<p class="muted">
            {{
              studentMode
                ? "Retrouvez les informations de votre scolarité."
                : "Pilotez la scolarité et accompagnez chaque étudiant."
            }}
          </p>
        </div>
        <!-- Affiche une élément de texte ou badge. -->
<span class="date-label">IAI · TOGO</span>
      </div>
      <!-- Les retours d'action sont annoncés aux technologies d'assistance. -->
      <!-- Affiche une texte de contexte ou message. Condition d’affichage : message. -->
<p v-if="message" role="status" class="notice">{{ message }}</p>
      <!-- L'accueil synthétise les informations de l'année active. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Accueil'. -->
<section v-if="page === 'Accueil'">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="welcome-card">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une élément de texte ou badge. -->
<span class="welcome-chip"
              >ANNÉE ACADÉMIQUE {{ activeYear?.code || "À OUVRIR" }}</span
            >
            <!-- Affiche une titre du module. -->
<h2>
              {{
                studentMode
                  ? `Bonjour ${me.student?.firstName || me.username} 👋`
                  : "Chaque parcours compte."
              }}
            </h2>
            <!-- Affiche une texte de contexte ou message. -->
<p>
              {{
                activeYear?.welcome ||
                "Créez et ouvrez une année académique pour commencer les inscriptions."
              }}
            </p>
            <!-- Affiche une commande déclenchée par un clic. Condition d’affichage :  studentMode && !store.enrollments.some((e) => e.status === 'ACTIVE') . À l’événement click, exécute joinClass. -->
<button
              v-if="
                studentMode &&
                !store.enrollments.some((e) => e.status === 'ACTIVE')
              "
              @click="joinClass"
            >
              Intégrer ma nouvelle classe →</button
            ><!-- Affiche une commande déclenchée par un clic. Utilise cette branche lorsque la branche précédente ne s’affiche pas. À l’événement click, exécute page = studentMode ? 'Notes & bulletins' : 'Classes'. -->
<button
              v-else
              @click="page = studentMode ? 'Notes & bulletins' : 'Classes'"
            >
              {{
                studentMode ? "Consulter mes résultats" : "Accéder aux classes"
              }}
              →
            </button>
          </div>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="welcome-art" aria-hidden="true">▱<!-- Affiche une élément de texte ou badge. -->
<span>IAI</span></div>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="stats-grid">
          <!-- Affiche une carte de données métier. -->
<article>
            <!-- Affiche une élément de texte ou badge. -->
<span>ÉTUDIANTS / INSCRIPTIONS</span
            ><!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{
              studentMode ? store.enrollments.length : store.students.length
            }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>{{
              studentMode
                ? "Années dans votre historique"
                : "Dossiers enregistrés"
            }}</small>
          </article>
          <!-- Affiche une carte de données métier. -->
<article>
            <!-- Affiche une élément de texte ou badge. -->
<span>CLASSES</span><!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ store.classes.length }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>Salles physiques accessibles</small>
          </article>
          <!-- Affiche une carte de données métier. -->
<article>
            <!-- Affiche une élément de texte ou badge. -->
<span>PRÉSENCES</span
            ><!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{
              store.records.filter((r) => r.status === "PRESENT").length
            }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>Pointages enregistrés</small>
          </article>
          <!-- Affiche une carte de données métier. -->
<article>
            <!-- Affiche une élément de texte ou badge. -->
<span>JUSTIFICATIFS</span
            ><!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{
              store.justifications.filter((j) => j.status === "PENDING").length
            }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>En attente de décision</small>
          </article>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="two-columns">
          <!-- Affiche une section de contenu ou formulaire. -->
<section class="panel">
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
              <!-- Affiche une titre du module. -->
<h2>{{ studentMode ? "Ma scolarité" : "Classes de l’année" }}</h2>
              <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute page = studentMode ? 'Ma scolarité' : 'Classes'. -->
<button
                class="text-button"
                @click="page = studentMode ? 'Ma scolarité' : 'Classes'"
              >
                Tout voir →
              </button>
            </div>
            <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : c in store.classes .filter((c) => c.yearId === activeYear?.id) .slice(0, 5). Calcule l’attribut key depuis c.id. -->
<div
              v-for="c in store.classes
                .filter((c) => c.yearId === activeYear?.id)
                .slice(0, 5)"
              :key="c.id"
              class="list-item"
            >
              <!-- Affiche une élément de texte ou badge. -->
<span class="tile-icon">▦</span>
              <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
                <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ c.name }}</strong
                ><!-- Affiche une information complémentaire. -->
<small>{{ c.level }} · {{ c.speciality }}</small>
              </div>
              <!-- Affiche une élément de texte ou badge. -->
<span class="badge">{{ JSON.parse(c.tracks).join(" / ") }}</span>
            </div>
            <!-- Affiche une texte de contexte ou message. Condition d’affichage : !store.classes.length. -->
<p v-if="!store.classes.length" class="empty">
              Aucune classe pour le moment.
            </p>
          </section>
          <!-- Affiche une section de contenu ou formulaire. -->
<section class="panel">
            <!-- Affiche une titre du module. -->
<h2>{{ studentMode ? "Mes notifications" : "À suivre" }}</h2>
            <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : n in store.notifications.slice(-5).reverse(). Calcule l’attribut key depuis n.id. -->
<div
              v-for="n in store.notifications.slice(-5).reverse()"
              :key="n.id"
              class="list-item"
            >
              <!-- Affiche une élément de texte ou badge. -->
<span class="tile-icon">◇</span>
              <!-- Affiche une texte de contexte ou message. -->
<p>{{ n.message }}</p>
            </div>
            <!-- Affiche une texte de contexte ou message. Condition d’affichage : !store.notifications.length. -->
<p v-if="!store.notifications.length" class="empty">
              Les publications et décisions apparaîtront ici.
            </p>
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="quick-actions">
              <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute page = 'Présences'. -->
<button @click="page = 'Présences'">Présences ↗</button
              ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute page = 'Documents & épreuves'. -->
<button @click="page = 'Documents & épreuves'">
                Ressources ↗
              </button>
            </div>
          </section>
        </div>
      </section>
      <!-- Les sélecteurs de classe et programme sont partagés par les modules pédagogiques. -->
      <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage :  [ 'Classes', 'Inscriptions', 'Notes & bulletins', 'Présences', 'Documents & épreuves', ].includes(page) . -->
<div
        v-if="
          [
            'Classes',
            'Inscriptions',
            'Notes & bulletins',
            'Présences',
            'Documents & épreuves',
          ].includes(page)
        "
        class="toolbar panel compact"
      >
        <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Classe<!-- Affiche une liste de choix. Synchronise la saisie avec selectedClass. -->
<select v-model="selectedClass">
            <!-- Affiche une choix proposé dans la liste. Répète cette zone pour : c in store.classes. Calcule l’attribut key depuis c.id. Calcule l’attribut value depuis c.id. -->
<option v-for="c in store.classes" :key="c.id" :value="c.id">
              {{ c.name }}
            </option>
          </select></label
        ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Programme<!-- Affiche une liste de choix. Synchronise la saisie avec selectedCurriculum. -->
<select v-model="selectedCurriculum">
            <!-- Affiche une choix proposé dans la liste. Répète cette zone pour : c in compatibleCurricula. Calcule l’attribut key depuis c.id. Calcule l’attribut value depuis c.id. -->
<option v-for="c in compatibleCurricula" :key="c.id" :value="c.id">
              {{ c.name }}
            </option>
          </select></label
        ><!-- Affiche une libellé associé à un contrôle de saisie. Condition d’affichage : curriculumContent. -->
<label v-if="curriculumContent"
          >Semestre<!-- Affiche une liste de choix. Synchronise la saisie avec semester. -->
<select v-model="semester">
            <!-- Affiche une choix proposé dans la liste. Répète cette zone pour : s in curriculumContent.semesters. Calcule l’attribut key depuis s.number. Calcule l’attribut value depuis s.number. -->
<option
              v-for="s in curriculumContent.semesters"
              :key="s.number"
              :value="s.number"
            >
              Semestre {{ s.number }}
            </option>
          </select></label
        >
      </div>
      <!-- Les années passent d'un brouillon à une ouverture explicite. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Années'. -->
<section v-if="page === 'Années'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Années académiques</h2>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute createYear. -->
<button class="primary" @click="createYear">+ Créer une année</button>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : y in store.years. Calcule l’attribut key depuis y.id. -->
<div v-for="y in store.years" :key="y.id" class="list-item">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ y.code }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>{{ y.start }} → {{ y.end }}</small>
            <!-- Affiche une texte de contexte ou message. -->
<p>{{ y.welcome }}</p>
          </div>
          <!-- Affiche une élément de texte ou badge. -->
<span class="badge">{{ statusLabel(y.status) }}</span
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : y.status === 'DRAFT'. À l’événement click, exécute openYear(y). -->
<button v-if="y.status === 'DRAFT'" @click="openYear(y)">
            Ouvrir l’année</button
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : y.status === 'OPEN'. À l’événement click, exécute  run(async () => { await api(`/years/${y.id}/close`, 'POST'); await refresh(); }) . -->
<button
            v-if="y.status === 'OPEN'"
            @click="
              run(async () => {
                await api(`/years/${y.id}/close`, 'POST');
                await refresh();
              })
            "
          >
            Clôturer
          </button>
        </div>
        <!-- Affiche une texte de contexte ou message. Condition d’affichage : !store.years.length. -->
<p v-if="!store.years.length" class="empty">
          Créez votre première année académique.
        </p>
      </section>
      <!-- Les classes restent indépendantes des parcours ITI et LPI. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Classes'. -->
<section v-if="page === 'Classes'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Classes physiques</h2>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute createClass. -->
<button class="primary" @click="createClass">
            + Créer une classe
          </button>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : c in store.classes. Calcule l’attribut key depuis c.id. -->
<div v-for="c in store.classes" :key="c.id" class="list-item">
          <!-- Affiche une élément de texte ou badge. -->
<span class="tile-icon">▦</span>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ c.name }}</strong
            ><!-- Affiche une information complémentaire. -->
<small
              >{{ c.level }} · {{ c.speciality }} ·
              {{ JSON.parse(c.tracks).join(" / ") }}</small
            >
          </div>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute shareClass(c). -->
<button @click="shareClass(c)">Partager / régénérer le code</button>
        </div>
        <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : selectedClass. À l’événement click, exécute assignOfficer. -->
<button v-if="selectedClass" @click="assignOfficer">
          Désigner délégué, adjoint ou secrétaire
        </button>
      </section>
      <!-- Les programmes révèlent les données transcrites et les points à confirmer. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Programmes'. -->
<section v-if="page === 'Programmes'">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="toolbar panel">
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute approveEquivalence. -->
<button @click="approveEquivalence">
            Approuver une équivalence d’UE
          </button>
        </div>
        <!-- Affiche une carte de données métier. Répète cette zone pour : c in store.curricula. Calcule l’attribut key depuis c.id. -->
<article
          v-for="c in store.curricula"
          :key="c.id"
          class="panel curriculum"
        >
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
              <!-- Affiche une titre du module. -->
<h2>{{ c.name }}</h2>
              <!-- Affiche une élément de texte ou badge. -->
<span class="badge">{{ statusLabel(c.status) }}</span>
            </div>
            <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute cloneCurriculum(c). -->
<button @click="cloneCurriculum(c)">
              Créer une nouvelle version
            </button>
          </div>
          <!-- Affiche une texte de contexte ou message. Répète cette zone pour : note in JSON.parse(c.content).notes. Calcule l’attribut key depuis note. -->
<p
            v-for="note in JSON.parse(c.content).notes"
            :key="note"
            class="notice"
          >
            {{ note }}
          </p>
          <!-- Affiche une détail de programme dépliable. Répète cette zone pour : s in JSON.parse(c.content).semesters. Calcule l’attribut key depuis s.number. -->
<details v-for="s in JSON.parse(c.content).semesters" :key="s.number">
            <!-- Affiche une titre du détail dépliable. -->
<summary>Semestre {{ s.number }} · {{ s.ues.length }} UE</summary>
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="table-wrap">
              <!-- Affiche une tableau de données. -->
<table>
                <!-- Affiche une en-têtes de colonnes. -->
<thead>
                  <!-- Affiche une ligne de tableau. -->
<tr>
                    <!-- Affiche une titre de colonne. -->
<th>UE / EC</th>
                    <!-- Affiche une titre de colonne. -->
<th>Code officiel</th>
                    <!-- Affiche une titre de colonne. -->
<th>CM</th>
                    <!-- Affiche une titre de colonne. -->
<th>TD</th>
                    <!-- Affiche une titre de colonne. -->
<th>TP</th>
                    <!-- Affiche une titre de colonne. -->
<th>Coef.</th>
                    <!-- Affiche une titre de colonne. -->
<th>Crédits</th>
                  </tr>
                </thead>
                <!-- Affiche une lignes du tableau. Répète cette zone pour : u in s.ues. Calcule l’attribut key depuis u.id. -->
<tbody v-for="u in s.ues" :key="u.id">
                  <!-- Affiche une ligne de tableau. -->
<tr class="unit-row">
                    <!-- Affiche une cellule de données. -->
<td>{{ u.name }}</td>
                    <!-- Affiche une cellule de données. -->
<td>{{ u.code }}</td>
                    <!-- Affiche une cellule de données. -->
<td colspan="4"></td>
                    <!-- Affiche une cellule de données. -->
<td>{{ u.credits ?? "À confirmer" }}</td>
                  </tr>
                  <!-- Affiche une ligne de tableau. Répète cette zone pour : e in u.ecs. Calcule l’attribut key depuis e.id. -->
<tr v-for="e in u.ecs" :key="e.id">
                    <!-- Affiche une cellule de données. -->
<td>{{ e.name }}</td>
                    <!-- Affiche une cellule de données. -->
<td>{{ e.code }}</td>
                    <!-- Affiche une cellule de données. -->
<td>{{ e.cm }}</td>
                    <!-- Affiche une cellule de données. -->
<td>{{ e.td }}</td>
                    <!-- Affiche une cellule de données. -->
<td>{{ e.tp }}</td>
                    <!-- Affiche une cellule de données. -->
<td>{{ e.coefficient }}</td>
                    <!-- Affiche une cellule de données. -->
<td></td>
                  </tr>
                </tbody>
              </table>
            </div>
          </details>
        </article>
      </section>
      <!-- Les étudiants sont identifiés par matricule et conservés même après abandon. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Étudiants'. -->
<section v-if="page === 'Étudiants'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Dossiers étudiants</h2>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="toolbar">
            <!-- Affiche une lien de téléchargement ou de navigation. -->
<a class="button" href="/api/exports?kind=students&format=xlsx"
              >Excel ↓</a
            ><!-- Affiche une lien de téléchargement ou de navigation. -->
<a class="button" href="/api/exports?kind=students&format=pdf"
              >PDF ↓</a
            >
          </div>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute exportRows(store.students, 'etudiants.csv'). -->
<button @click="exportRows(store.students, 'etudiants.csv')">
              Exporter CSV</button
            ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('students.create'). À l’événement click, exécute createStudent. -->
<button
              v-if="can('students.create')"
              class="primary"
              @click="createStudent"
            >
              + Ajouter
            </button>
          </div>
        </div>
        <!-- Affiche une contrôle de saisie. Synchronise la saisie avec search. Le libellé accessible est « Rechercher un étudiant ». -->
<input
          v-model="search"
          placeholder="Rechercher un nom ou matricule…"
          aria-label="Rechercher un étudiant"
        />
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="table-wrap">
          <!-- Affiche une tableau de données. -->
<table>
            <!-- Affiche une en-têtes de colonnes. -->
<thead>
              <!-- Affiche une ligne de tableau. -->
<tr>
                <!-- Affiche une titre de colonne. -->
<th>Étudiant</th>
                <!-- Affiche une titre de colonne. -->
<th>Matricule</th>
                <!-- Affiche une titre de colonne. -->
<th>Parcours</th>
                <!-- Affiche une titre de colonne. -->
<th>Statut</th>
                <!-- Affiche une titre de colonne. -->
<th>Actions</th>
              </tr>
            </thead>
            <!-- Affiche une lignes du tableau. -->
<tbody>
              <!-- Affiche une ligne de tableau. Répète cette zone pour : s in filtered(store.students). Calcule l’attribut key depuis s.id. Calcule l’attribut class depuis { inactive: !['ACTIVE', 'PRE_REGISTERED'].includes(s.status), }. -->
<tr
                v-for="s in filtered(store.students)"
                :key="s.id"
                :class="{
                  inactive: !['ACTIVE', 'PRE_REGISTERED'].includes(s.status),
                }"
              >
                <!-- Affiche une cellule de données. -->
<td>
                  <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ s.firstName }} {{ s.lastName }}</strong>
                </td>
                <!-- Affiche une cellule de données. -->
<td>{{ s.number }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ s.track }}</td>
                <!-- Affiche une cellule de données. -->
<td>
                  <!-- Affiche une élément de texte ou badge. -->
<span class="badge">{{ statusLabel(s.status) }}</span>
                </td>
                <!-- Affiche une cellule de données. -->
<td>
                  <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('students.update'). À l’événement click, exécute editStudent(s). -->
<button v-if="can('students.update')" @click="editStudent(s)">
                    Modifier</button
                  ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('students.disable'). À l’événement click, exécute statusStudent(s). -->
<button
                    v-if="can('students.disable')"
                    @click="statusStudent(s)"
                  >
                    Statut</button
                  ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : s.status === 'PRE_REGISTERED'. À l’événement click, exécute  open( 'Supprimer cette fiche sans historique', [], async () => { await api(`/students/${s.id}`, 'DELETE'); }, ) . -->
<button
                    v-if="s.status === 'PRE_REGISTERED'"
                    @click="
                      open(
                        'Supprimer cette fiche sans historique',
                        [],
                        async () => {
                          await api(`/students/${s.id}`, 'DELETE');
                        },
                      )
                    "
                  >
                    Supprimer la fiche vide
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <!-- Affiche une texte de contexte ou message. Condition d’affichage : !store.students.length. -->
<p v-if="!store.students.length" class="empty">
          Aucun dossier enregistré.
        </p>
      </section>
      <!-- Les inscriptions gardent les tentatives et les années passées. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : ['Inscriptions', 'Ma scolarité'].includes(page). -->
<section
        v-if="['Inscriptions', 'Ma scolarité'].includes(page)"
        class="panel"
      >
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Historique scolaire</h2>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute studentMode ? joinClass() : enrollStudent(). -->
<button
            class="primary"
            @click="studentMode ? joinClass() : enrollStudent()"
          >
            {{
              studentMode
                ? "Intégrer ma nouvelle classe"
                : "+ Inscrire un étudiant"
            }}
          </button>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="table-wrap">
          <!-- Affiche une tableau de données. -->
<table>
            <!-- Affiche une en-têtes de colonnes. -->
<thead>
              <!-- Affiche une ligne de tableau. -->
<tr>
                <!-- Affiche une titre de colonne. -->
<th>Étudiant</th>
                <!-- Affiche une titre de colonne. -->
<th>Année</th>
                <!-- Affiche une titre de colonne. -->
<th>Classe</th>
                <!-- Affiche une titre de colonne. -->
<th>Parcours</th>
                <!-- Affiche une titre de colonne. -->
<th>Tentative</th>
                <!-- Affiche une titre de colonne. -->
<th>Statut</th>
                <!-- Affiche une titre de colonne. -->
<th>Actions</th>
              </tr>
            </thead>
            <!-- Affiche une lignes du tableau. -->
<tbody>
              <!-- Affiche une ligne de tableau. Répète cette zone pour : e in store.enrollments. Calcule l’attribut key depuis e.id. -->
<tr v-for="e in store.enrollments" :key="e.id">
                <!-- Affiche une cellule de données. -->
<td>{{ e.studentName }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ e.yearCode }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ e.className }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ e.track }}</td>
                <!-- Affiche une cellule de données. -->
<td>
                  {{
                    e.attempt > 1
                      ? `Redoublant · ${e.attempt}e inscription`
                      : "Première inscription"
                  }}
                </td>
                <!-- Affiche une cellule de données. -->
<td>{{ statusLabel(e.status) }}</td>
                <!-- Affiche une cellule de données. -->
<td>
                  <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute  selectedEnrollment = e.id; page = 'Notes & bulletins'; loadResults(); . -->
<button
                    @click="
                      selectedEnrollment = e.id;
                      page = 'Notes & bulletins';
                      loadResults();
                    "
                  >
                    Résultats</button
                  ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('students.transfer') && e.status === 'ACTIVE'. À l’événement click, exécute transfer(e). -->
<button
                    v-if="can('students.transfer') && e.status === 'ACTIVE'"
                    @click="transfer(e)"
                  >
                    Transférer
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
      <!-- Les notes publiées et les brouillons administratifs utilisent le même relevé détaillé. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Notes & bulletins'. -->
<section v-if="page === 'Notes & bulletins'">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel toolbar">
          <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Dossier / année<!-- Affiche une liste de choix. Synchronise la saisie avec selectedEnrollment. -->
<select v-model="selectedEnrollment">
              <!-- Affiche une choix proposé dans la liste. Répète cette zone pour : e in store.enrollments. Calcule l’attribut key depuis e.id. Calcule l’attribut value depuis e.id. -->
<option v-for="e in store.enrollments" :key="e.id" :value="e.id">
                {{ e.studentName }} · {{ e.yearCode }} · {{ e.track }}
              </option>
            </select></label
          ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis !selectedEnrollment. À l’événement click, exécute loadResults. -->
<button
            class="primary"
            :disabled="!selectedEnrollment"
            @click="loadResults"
          >
            Consulter les notes</button
          ><!-- Affiche une lien de téléchargement ou de navigation. Condition d’affichage : selectedEnrollment. Calcule l’attribut href depuis `/api/enrollments/${selectedEnrollment}/transcript?pdf=true`. -->
<a
            v-if="selectedEnrollment"
            class="button"
            :href="`/api/enrollments/${selectedEnrollment}/transcript?pdf=true`"
            target="_blank"
            >Relevé / bulletin PDF ↗</a
          ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis !selectedEnrollment. À l’événement click, exécute loadRanks(false). -->
<button :disabled="!selectedEnrollment" @click="loadRanks(false)">
            Rang semestre</button
          ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis !selectedEnrollment. À l’événement click, exécute loadRanks(true). -->
<button :disabled="!selectedEnrollment" @click="loadRanks(true)">
            Rang annuel
          </button>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : can('grades.import') || can('grades.publish'). -->
<div
          v-if="can('grades.import') || can('grades.publish')"
          class="panel toolbar"
        >
          <!-- Affiche une lien de téléchargement ou de navigation. Condition d’affichage : can('grades.import') && selectedClass && selectedCurriculum. Calcule l’attribut href depuis `/api/classes/${selectedClass}/grades/template?semester=${semester}&curriculumId=${selectedCurriculum}`. -->
<a
            v-if="can('grades.import') && selectedClass && selectedCurriculum"
            class="button"
            :href="`/api/classes/${selectedClass}/grades/template?semester=${semester}&curriculumId=${selectedCurriculum}`"
            >Modèle CSV ↓</a
          ><!-- Affiche une libellé associé à un contrôle de saisie. Condition d’affichage : can('grades.import'). -->
<label v-if="can('grades.import')" class="button"
            >Importer CSV<!-- Affiche une contrôle de saisie. Le type de contrôle est file. À l’événement change, exécute importCsv. -->
<input
              type="file"
              accept=".csv"
              @change="importCsv" /></label
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('grades.publish'). Calcule l’attribut disabled depuis !selectedClass. À l’événement click, exécute publish. -->
<button
            v-if="can('grades.publish')"
            class="primary"
            :disabled="!selectedClass"
            @click="publish"
          >
            Publier les résultats</button
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('grades.publish'). À l’événement click, exécute  open('Verrouiller définitivement ce semestre', [], async () => { await api(`/classes/${selectedClass}/lock`, 'POST', { semester, }); }) . -->
<button
            v-if="can('grades.publish')"
            @click="
              open('Verrouiller définitivement ce semestre', [], async () => {
                await api(`/classes/${selectedClass}/lock`, 'POST', {
                  semester,
                });
              })
            "
          >
            Verrouiller
          </button>
        </div>
        <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : progress.some((u) => u.status === 'VALIDATED_PREVIOUSLY'). -->
<section
          v-if="progress.some((u) => u.status === 'VALIDATED_PREVIOUSLY')"
          class="panel"
        >
          <!-- Affiche une titre du module. -->
<h2>UE acquises antérieurement</h2>
          <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : u in progress. Calcule l’attribut key depuis u.id. -->
<div v-for="u in progress" :key="u.id" class="list-item">
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ u.name }}</strong
            ><!-- Affiche une élément de texte ou badge. -->
<span class="badge">{{
              u.status === "VALIDATED_PREVIOUSLY"
                ? "Acquise antérieurement"
                : "À valider"
            }}</span
            ><!-- Affiche une élément de texte ou badge. -->
<span>{{ u.awarded }} / {{ u.credits ?? "?" }} crédits</span>
          </div>
        </section>
        <!-- Affiche une carte de données métier. Répète cette zone pour : r in results. Calcule l’attribut key depuis r.semester. -->
<article v-for="r in results" :key="r.semester" class="panel">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
            <!-- Affiche une titre du module. -->
<h2>Semestre {{ r.semester }}</h2>
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="result-total">
              <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ number(r.average) }} <!-- Affiche une information complémentaire. -->
<small>/ 20</small></strong
              ><!-- Affiche une élément de texte ou badge. -->
<span
                >{{ r.awarded }} / {{ r.totalCredits ?? "?" }} crédits</span
              >
            </div>
          </div>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="table-wrap">
            <!-- Affiche une tableau de données. -->
<table>
              <!-- Affiche une en-têtes de colonnes. -->
<thead>
                <!-- Affiche une ligne de tableau. -->
<tr>
                  <!-- Affiche une titre de colonne. -->
<th>UE / Éléments constitutifs</th>
                  <!-- Affiche une titre de colonne. -->
<th>Coefficient</th>
                  <!-- Affiche une titre de colonne. -->
<th>Note / 20</th>
                  <!-- Affiche une titre de colonne. -->
<th>Validation</th>
                </tr>
              </thead>
              <!-- Affiche une lignes du tableau. Répète cette zone pour : u in r.ues. Calcule l’attribut key depuis u.id. -->
<tbody v-for="u in r.ues" :key="u.id">
                <!-- Affiche une ligne de tableau. -->
<tr class="unit-row">
                  <!-- Affiche une cellule de données. -->
<td>
                    <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ u.name }}</strong
                    ><!-- Affiche une information complémentaire. -->
<small
                      >{{ u.code }} · {{ u.awarded }}/{{
                        u.credits ?? "?"
                      }}
                      crédits</small
                    >
                  </td>
                  <!-- Affiche une cellule de données. -->
<td></td>
                  <!-- Affiche une cellule de données. -->
<td>{{ number(u.average) }}</td>
                  <!-- Affiche une cellule de données. -->
<td>
                    <!-- Affiche une élément de texte ou badge. Calcule l’attribut class depuis { success: u.status.startsWith('VALIDATED') }. -->
<span
                      class="badge"
                      :class="{ success: u.status.startsWith('VALIDATED') }"
                      >{{ statusLabel(u.status) }}</span
                    ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage :  can('grades.rescue') && u.status === 'NOT_VALIDATED' . À l’événement click, exécute rescue(u, r.semester). -->
<button
                      v-if="
                        can('grades.rescue') && u.status === 'NOT_VALIDATED'
                      "
                      @click="rescue(u, r.semester)"
                    >
                      Repêcher
                    </button>
                  </td>
                </tr>
                <!-- Affiche une ligne de tableau. Répète cette zone pour : e in u.ecs. Calcule l’attribut key depuis e.id. -->
<tr v-for="e in u.ecs" :key="e.id">
                  <!-- Affiche une cellule de données. -->
<td>{{ e.name }}</td>
                  <!-- Affiche une cellule de données. -->
<td>{{ e.coefficient }}</td>
                  <!-- Affiche une cellule de données. -->
<td>{{ number(e.note) }}</td>
                  <!-- Affiche une cellule de données. -->
<td>
                    <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('grades.update'). À l’événement click, exécute editGrade(e, r.semester). -->
<button
                      v-if="can('grades.update')"
                      @click="editGrade(e, r.semester)"
                    >
                      Saisir / rectifier
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </article>
        <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : !results.length. -->
<div v-if="!results.length" class="panel empty">
          Choisissez une inscription et consultez ses résultats. Seules les
          notes publiées sont visibles dans l’espace étudiant.
        </div>
        <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : ranks.length. -->
<section v-if="ranks.length" class="panel">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
            <!-- Affiche une titre du module. -->
<h2>
              Classement · {{ currentEnrollment?.track }}
              {{ currentEnrollment?.speciality }}
            </h2>
            <!-- Affiche une lien de téléchargement ou de navigation. Condition d’affichage : can('grades.manage'). Calcule l’attribut href depuis exportUrl('rankings', 'xlsx'). -->
<a
              v-if="can('grades.manage')"
              class="button"
              :href="exportUrl('rankings', 'xlsx')"
              >Excel ↓</a
            ><!-- Affiche une lien de téléchargement ou de navigation. Condition d’affichage : can('grades.manage'). Calcule l’attribut href depuis exportUrl('rankings', 'pdf'). -->
<a
              v-if="can('grades.manage')"
              class="button"
              :href="exportUrl('rankings', 'pdf')"
              >PDF ↓</a
            ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute exportRows(ranks, 'classement.csv'). -->
<button @click="exportRows(ranks, 'classement.csv')">
              Exporter CSV
            </button>
          </div>
          <!-- Affiche une tableau de données. -->
<table>
            <!-- Affiche une en-têtes de colonnes. -->
<thead>
              <!-- Affiche une ligne de tableau. -->
<tr>
                <!-- Affiche une titre de colonne. -->
<th>Rang</th>
                <!-- Affiche une titre de colonne. -->
<th>Étudiant</th>
                <!-- Affiche une titre de colonne. -->
<th>Moyenne</th>
                <!-- Affiche une titre de colonne. -->
<th>Effectif</th>
              </tr>
            </thead>
            <!-- Affiche une lignes du tableau. -->
<tbody>
              <!-- Affiche une ligne de tableau. Répète cette zone pour : r in ranks. Calcule l’attribut key depuis r.enrollmentId. -->
<tr v-for="r in ranks" :key="r.enrollmentId">
                <!-- Affiche une cellule de données. -->
<td>{{ r.position }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ r.name }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ number(r.average) }}</td>
                <!-- Affiche une cellule de données. -->
<td>{{ r.total }}</td>
              </tr>
            </tbody>
          </table>
        </section>
      </section>
      <!-- Les présences combinent cours, code dynamique, QR et rapports filtrés. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Présences'. -->
<section v-if="page === 'Présences'">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel toolbar">
          <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('attendance.manage') || officer. À l’événement click, exécute openSession. -->
<button
            v-if="can('attendance.manage') || officer"
            class="primary"
            @click="openSession"
          >
            + Ouvrir une séance</button
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : officer && officer.position !== 'SECRETARY'. À l’événement click, exécute delegate. -->
<button
            v-if="officer && officer.position !== 'SECRETARY'"
            @click="delegate"
          >
            Déléguer l’affichage QR</button
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : studentMode. À l’événement click, exécute scanQr. -->
<button v-if="studentMode" @click="scanQr">Scanner un QR</button
          ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute exportRows(reportRows, 'presences.csv'). -->
<button @click="exportRows(reportRows, 'presences.csv')">
            Exporter le rapport CSV</button
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label>Jour<!-- Affiche une contrôle de saisie. Synchronise la saisie avec reportDate. Le type de contrôle est date. -->
<input v-model="reportDate" type="date" /></label
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Semaine à partir du<!-- Affiche une contrôle de saisie. Synchronise la saisie avec weekStart. Le type de contrôle est date. -->
<input v-model="weekStart" type="date"
          /></label>
        </div>
        <!-- Affiche une aperçu de la caméra de scan. -->
<video id="scanner" hidden playsinline class="scanner"></video>
        <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : token. -->
<div v-if="token" class="panel qr-panel">
          <!-- Affiche une image du QR autorisé. Calcule l’attribut src depuis qrImage. -->
<img :src="qrImage" alt="QR de pointage du cours" />
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une texte de contexte ou message. -->
<p class="eyebrow">CODE DE PRÉSENCE</p>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong class="presence-code">{{ token.code }}</strong>
            <!-- Affiche une texte de contexte ou message. -->
<p>Expire dans {{ token.expiresIn }} secondes</p>
            <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute token = null. -->
<button @click="token = null">Masquer</button>
          </div>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel">
          <!-- Affiche une titre du module. -->
<h2>Séances de cours</h2>
          <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : s in store.sessions.filter( (s) => !selectedClass || s.classId === selectedClass, ). Calcule l’attribut key depuis s.id. -->
<div
            v-for="s in store.sessions.filter(
              (s) => !selectedClass || s.classId === selectedClass,
            )"
            :key="s.id"
            class="list-item"
          >
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
              <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ s.course }}</strong
              ><!-- Affiche une information complémentaire. -->
<small
                >{{ new Date(s.opened).toLocaleString("fr-FR") }} ·
                {{ s.closed ? "Fermée" : "Ouverte" }}</small
              >
            </div>
            <!-- Affiche une branche ou regroupement conditionnel sans élément HTML supplémentaire. Condition d’affichage : !s.closed. -->
<template v-if="!s.closed"
              ><!-- Affiche une branche ou regroupement conditionnel sans élément HTML supplémentaire. Condition d’affichage : can('attendance.manage'). -->
<template v-if="can('attendance.manage')"
                ><!-- Affiche une lien de téléchargement ou de navigation. Calcule l’attribut href depuis `/api/attendance/sessions/${s.id}/template`. -->
<a
                  class="button"
                  :href="`/api/attendance/sessions/${s.id}/template`"
                  >Modèle CSV ↓</a
                ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label class="button"
                  >Importer présence CSV<!-- Affiche une contrôle de saisie. Le type de contrôle est file. À l’événement change, exécute  importAttendance($event, s.id) . -->
<input
                    type="file"
                    accept=".csv"
                    @change="
                      importAttendance($event, s.id)
                    " /></label></template
              ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : displayAllowed. À l’événement click, exécute showToken(s.id). -->
<button v-if="displayAllowed" @click="showToken(s.id)">
                Afficher QR / code</button
              ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : studentMode. À l’événement click, exécute checkIn(s.id, Boolean(officer)). -->
<button
                v-if="studentMode"
                @click="checkIn(s.id, Boolean(officer))"
              >
                {{ officer ? "Je suis présent" : "Saisir le code" }}</button
              ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('attendance.manage') || officer. À l’événement click, exécute  run(async () => { await api(`/attendance/sessions/${s.id}/close`, 'POST'); await refresh(); }) . -->
<button
                v-if="can('attendance.manage') || officer"
                @click="
                  run(async () => {
                    await api(`/attendance/sessions/${s.id}/close`, 'POST');
                    await refresh();
                  })
                "
              >
                Fermer la séance
              </button></template
            >
          </div>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel">
          <!-- Affiche une titre du module. -->
<h2>Rapport de présence</h2>
          <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : can('attendance.reports'). -->
<div v-if="can('attendance.reports')" class="toolbar">
            <!-- Affiche une lien de téléchargement ou de navigation. Calcule l’attribut href depuis exportUrl('attendance', 'xlsx'). -->
<a class="button" :href="exportUrl('attendance', 'xlsx')">Excel ↓</a
            ><!-- Affiche une lien de téléchargement ou de navigation. Calcule l’attribut href depuis exportUrl('attendance', 'pdf'). -->
<a class="button" :href="exportUrl('attendance', 'pdf')">PDF ↓</a>
          </div>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="table-wrap">
            <!-- Affiche une tableau de données. -->
<table>
              <!-- Affiche une en-têtes de colonnes. -->
<thead>
                <!-- Affiche une ligne de tableau. -->
<tr>
                  <!-- Affiche une titre de colonne. -->
<th>Étudiant</th>
                  <!-- Affiche une titre de colonne. -->
<th>Cours</th>
                  <!-- Affiche une titre de colonne. -->
<th>Date / heure</th>
                  <!-- Affiche une titre de colonne. -->
<th>Statut</th>
                  <!-- Affiche une titre de colonne. -->
<th>Méthode</th>
                </tr>
              </thead>
              <!-- Affiche une lignes du tableau. -->
<tbody>
                <!-- Affiche une ligne de tableau. Répète cette zone pour : r in reportRows. Calcule l’attribut key depuis r.id. -->
<tr v-for="r in reportRows" :key="r.id">
                  <!-- Affiche une cellule de données. -->
<td>{{ r.studentName }}</td>
                  <!-- Affiche une cellule de données. -->
<td>{{ r.course }}</td>
                  <!-- Affiche une cellule de données. -->
<td>{{ new Date(r.checked).toLocaleString("fr-FR") }}</td>
                  <!-- Affiche une cellule de données. -->
<td>{{ statusLabel(r.status) }}</td>
                  <!-- Affiche une cellule de données. -->
<td>{{ r.method }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : store.delegations.length. -->
<div class="panel" v-if="store.delegations.length">
          <!-- Affiche une titre du module. -->
<h2>Délégations QR</h2>
          <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : d in store.delegations. Calcule l’attribut key depuis d.id. -->
<div v-for="d in store.delegations" :key="d.id" class="list-item">
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
              <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ d.grantee }}</strong
              ><!-- Affiche une information complémentaire. -->
<small
                >Expire le
                {{ new Date(d.expires).toLocaleString("fr-FR") }}</small
              >
            </div>
            <!-- Affiche une élément de texte ou badge. -->
<span>{{
              d.revoked
                ? "Révoquée"
                : new Date(d.expires).getTime() > Date.now()
                  ? "Active"
                  : "Expirée"
            }}</span
            ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage : !d.revoked. À l’événement click, exécute  run(async () => { await api(`/delegations/${d.id}`, 'DELETE'); await refresh(); }) . -->
<button
              v-if="!d.revoked"
              @click="
                run(async () => {
                  await api(`/delegations/${d.id}`, 'DELETE');
                  await refresh();
                })
              "
            >
              Révoquer
            </button>
          </div>
        </div>
      </section>
      <!-- Les justificatifs proposent une décision uniquement aux profils habilités. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Justificatifs'. -->
<section v-if="page === 'Justificatifs'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Absences et justificatifs</h2>
          <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : studentMode. À l’événement click, exécute justify. -->
<button v-if="studentMode" class="primary" @click="justify">
            + Déclarer une absence
          </button>
        </div>
        <!-- Affiche une texte de contexte ou message. -->
<p class="muted">
          Les pièces jointes se déposent dans Documents, catégorie
          JUSTIFICATION.
        </p>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : j in store.justifications. Calcule l’attribut key depuis j.id. -->
<div v-for="j in store.justifications" :key="j.id" class="list-item">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ j.course }}</strong>
            <!-- Affiche une texte de contexte ou message. -->
<p>{{ j.reason }}</p>
            <!-- Affiche une information complémentaire. -->
<small>{{ j.decision }}</small>
          </div>
          <!-- Affiche une élément de texte ou badge. -->
<span class="badge">{{ statusLabel(j.status) }}</span
          ><!-- Affiche une lien de téléchargement ou de navigation. Condition d’affichage : j.documentId. Calcule l’attribut href depuis `/api/documents/${j.documentId}/download`. -->
<a
            v-if="j.documentId"
            class="button"
            :href="`/api/documents/${j.documentId}/download`"
            >Pièce jointe ↓</a
          ><!-- Affiche une branche ou regroupement conditionnel sans élément HTML supplémentaire. Condition d’affichage : can('attendance.manage') && j.status === 'PENDING'. -->
<template v-if="can('attendance.manage') && j.status === 'PENDING'"
            ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute decide(j, true). -->
<button @click="decide(j, true)">Accepter</button
            ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute decide(j, false). -->
<button @click="decide(j, false)">Rejeter</button></template
          >
        </div>
        <!-- Affiche une texte de contexte ou message. Condition d’affichage : !store.justifications.length. -->
<p v-if="!store.justifications.length" class="empty">
          Aucun justificatif.
        </p>
      </section>
      <!-- La discipline montre le registre et une note calculée, pas une valeur écrasée. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Discipline'. -->
<section v-if="page === 'Discipline'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Registre de discipline</h2>
          <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('discipline.manage'). À l’événement click, exécute sanction. -->
<button
            v-if="can('discipline.manage')"
            class="primary"
            @click="sanction"
          >
            + Ajouter un événement
          </button>
        </div>
        <!-- Affiche une texte de contexte ou message. -->
<p class="notice">
          Base : 20 points. Chaque événement est ajouté au registre ; la note
          reste entre 0 et 20. Le lien avec l’EC Séminaire doit être défini
          selon le règlement officiel.
        </p>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : e in store.enrollments. Calcule l’attribut key depuis e.id. -->
<div v-for="e in store.enrollments" :key="e.id" class="list-item">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ e.studentName }} · {{ e.yearCode }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>{{ e.className }}</small>
          </div>
          <!-- Affiche une élément de texte ou badge. -->
<span class="badge"
            >{{
              number(
                Math.max(
                  0,
                  Math.min(
                    20,
                    20 +
                      store.discipline
                        .filter(
                          (d) =>
                            d.enrollmentId === e.id && d.semester === semester,
                        )
                        .reduce((n, d) => n + d.delta, 0),
                  ),
                ),
              )
            }}
            / 20 · S{{ semester }}</span
          >
        </div>
        <!-- Affiche une tableau de données. -->
<table>
          <!-- Affiche une en-têtes de colonnes. -->
<thead>
            <!-- Affiche une ligne de tableau. -->
<tr>
              <!-- Affiche une titre de colonne. -->
<th>Date</th>
              <!-- Affiche une titre de colonne. -->
<th>Semestre</th>
              <!-- Affiche une titre de colonne. -->
<th>Points</th>
              <!-- Affiche une titre de colonne. -->
<th>Motif</th>
            </tr>
          </thead>
          <!-- Affiche une lignes du tableau. -->
<tbody>
            <!-- Affiche une ligne de tableau. Répète cette zone pour : d in store.discipline. Calcule l’attribut key depuis d.id. -->
<tr v-for="d in store.discipline" :key="d.id">
              <!-- Affiche une cellule de données. -->
<td>{{ new Date(d.created).toLocaleDateString("fr-FR") }}</td>
              <!-- Affiche une cellule de données. -->
<td>{{ d.semester }}</td>
              <!-- Affiche une cellule de données. -->
<td>{{ d.delta }}</td>
              <!-- Affiche une cellule de données. -->
<td>{{ d.reason }}</td>
            </tr>
          </tbody>
        </table>
      </section>
      <!-- Les ressources sont classées par catégorie et métadonnées pédagogiques. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Documents & épreuves'. -->
<section v-if="page === 'Documents & épreuves'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Bibliothèque pédagogique</h2>
          <!-- Affiche une commande déclenchée par un clic. Condition d’affichage : can('documents.manage') || officer || studentMode. À l’événement click, exécute uploadDocument. -->
<button
            v-if="can('documents.manage') || officer || studentMode"
            class="primary"
            @click="uploadDocument"
          >
            + Téléverser
          </button>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="toolbar">
          <!-- Affiche une contrôle de saisie. Synchronise la saisie avec search. Le libellé accessible est « Rechercher un document ». -->
<input
            v-model="search"
            placeholder="Rechercher une matière, année, titre…"
            aria-label="Rechercher un document"
          /><!-- Affiche une liste de choix. Synchronise la saisie avec documentCategory. Le libellé accessible est « Nature du document ». -->
<select v-model="documentCategory" aria-label="Nature du document">
            <!-- Affiche une choix proposé dans la liste. -->
<option value="">Toutes les natures</option>
            <!-- Affiche une choix proposé dans la liste. Répète cette zone pour : c in [ 'COURSE', 'TD', 'TP', 'DEVOIR', 'EXAMEN', 'RATTRAPAGE', 'JUSTIFICATION', ]. Calcule l’attribut key depuis c. -->
<option
              v-for="c in [
                'COURSE',
                'TD',
                'TP',
                'DEVOIR',
                'EXAMEN',
                'RATTRAPAGE',
                'JUSTIFICATION',
              ]"
              :key="c"
            >
              {{ c }}
            </option>
          </select>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : d in filtered(store.documents).filter( (d) => (!documentCategory || d.category === documentCategory) && (!selectedClass || d.classId === selectedClass), ). Calcule l’attribut key depuis d.id. -->
<div
          v-for="d in filtered(store.documents).filter(
            (d) =>
              (!documentCategory || d.category === documentCategory) &&
              (!selectedClass || d.classId === selectedClass),
          )"
          :key="d.id"
          class="list-item"
        >
          <!-- Affiche une élément de texte ou badge. -->
<span class="tile-icon">▤</span>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ d.title }}</strong
            ><!-- Affiche une information complémentaire. -->
<small
              >{{ d.filename }} · {{ d.category }} ·
              {{ d.approved ? "Validé" : "En attente" }}</small
            ><!-- Affiche une information complémentaire. -->
<small>{{
              Object.values(JSON.parse(d.metadata)).join(" · ")
            }}</small>
          </div>
          <!-- Affiche une lien de téléchargement ou de navigation. Calcule l’attribut href depuis `/api/documents/${d.id}/download`. -->
<a class="button" :href="`/api/documents/${d.id}/download`"
            >Télécharger ↓</a
          ><!-- Affiche une commande déclenchée par un clic. Condition d’affichage :  can('documents.manage') && !d.approved && d.category !== 'JUSTIFICATION' . À l’événement click, exécute  run(async () => { await api(`/documents/${d.id}/approve`, 'POST'); await refresh(); }) . -->
<button
            v-if="
              can('documents.manage') &&
              !d.approved &&
              d.category !== 'JUSTIFICATION'
            "
            @click="
              run(async () => {
                await api(`/documents/${d.id}/approve`, 'POST');
                await refresh();
              })
            "
          >
            Approuver
          </button>
        </div>
      </section>
      <!-- L'administration des comptes ne peut jamais révéler un mot de passe existant. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Utilisateurs'. -->
<section v-if="page === 'Utilisateurs'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Comptes utilisateurs</h2>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute createUser. -->
<button class="primary" @click="createUser">+ Créer un compte</button>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : u in store.users. Calcule l’attribut key depuis u.id. -->
<div v-for="u in store.users" :key="u.id" class="list-item">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ u.username }}</strong
            ><!-- Affiche une information complémentaire. -->
<small
              >{{ u.active ? "Actif" : "Désactivé" }} ·
              {{ u.studentId ? "Étudiant" : "Administration" }}</small
            >
          </div>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute run(async () => { credentials = await api(`/users/${u.id}/reset-mfa`, 'POST'); }). -->
<button @click="run(async () => { credentials = await api(`/users/${u.id}/reset-mfa`, 'POST'); })">Réinitialiser MFA</button><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute resetPassword(u). -->
<button @click="resetPassword(u)">
            Réinitialiser le mot de passe</button
          ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute  open( 'Modifier le compte', [ { key: 'roles', label: 'UUID des rôles (JSON)', type: 'json', }, { key: 'active', label: 'Actif', options: [ { value: true, label: 'Oui' }, { value: false, label: 'Non' }, ], }, ], async (d. -->
<button
            @click="
              open(
                'Modifier le compte',
                [
                  {
                    key: 'roles',
                    label: 'UUID des rôles (JSON)',
                    type: 'json',
                  },
                  {
                    key: 'active',
                    label: 'Actif',
                    options: [
                      { value: true, label: 'Oui' },
                      { value: false, label: 'Non' },
                    ],
                  },
                ],
                async (d) => {
                  const updated = await api(`/users/${u.id}`, 'PUT', d); if (updated.mfaSecret) credentials = updated;
                },
                { roles: JSON.parse(u.roles), active: u.active },
              )
            "
          >
            Droits / statut
          </button>
        </div>
      </section>
      <!-- Les droits sont des données modifiables plutôt que des noms de rôles codés dans les écrans. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Rôles & permissions'. -->
<section v-if="page === 'Rôles & permissions'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Profils et permissions</h2>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute roleForm(). -->
<button class="primary" @click="roleForm()">+ Créer un rôle</button>
        </div>
        <!-- Affiche une texte de contexte ou message. -->
<p class="notice">
          Catalogue : academic.manage, students.manage, grades.manage,
          grades.publish, grades.rescue, attendance.manage, attendance.reports,
          discipline.manage, documents.manage, audit.view, users.manage,
          roles.manage. * donne tous les droits.
        </p>
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : r in store.roles. Calcule l’attribut key depuis r.id. -->
<div v-for="r in store.roles" :key="r.id" class="list-item">
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ r.name }}</strong
            ><!-- Affiche une information complémentaire. -->
<small>{{ r.id }}</small>
            <!-- Affiche une texte de contexte ou message. -->
<p class="small">{{ JSON.parse(r.permissions).join(" · ") }}</p>
          </div>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute roleForm(r). -->
<button @click="roleForm(r)">Modifier les droits</button>
        </div>
      </section>
      <!-- L'audit affiche auteur, action et détails sans secret d'authentification. -->
      <!-- Affiche une section de contenu ou formulaire. Condition d’affichage : page === 'Journal d’audit'. -->
<section v-if="page === 'Journal d’audit'" class="panel">
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
          <!-- Affiche une titre du module. -->
<h2>Journal des opérations</h2>
          <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute exportRows(store.audit, 'audit.csv'). -->
<button @click="exportRows(store.audit, 'audit.csv')">
            Exporter CSV
          </button>
        </div>
        <!-- Affiche une contrôle de saisie. Synchronise la saisie avec search. Le libellé accessible est « Rechercher dans le journal ». -->
<input
          v-model="search"
          placeholder="Filtrer les opérations…"
          aria-label="Rechercher dans le journal"
        />
        <!-- Affiche une groupe de contrôles ou de contenu. Répète cette zone pour : log in filtered(store.audit). Calcule l’attribut key depuis log.id. -->
<div
          v-for="log in filtered(store.audit)"
          :key="log.id"
          class="list-item"
        >
          <!-- Affiche une élément de texte ou badge. -->
<span class="tile-icon">◎</span>
          <!-- Affiche une groupe de contrôles ou de contenu. -->
<div>
            <!-- Affiche une valeur ou nom mis en évidence. -->
<strong>{{ log.action }}</strong
            ><!-- Affiche une information complémentaire. -->
<small
              >{{ new Date(log.created).toLocaleString("fr-FR") }} ·
              {{ log.actor }} · {{ log.ip }}</small
            ><!-- Affiche une détails de journal échappés par Vue. -->
<code>{{ log.details }}</code>
          </div>
        </div>
      </section>
    </main>
  </div>
  <!-- Les coefficients et crédits sont édités sous forme de champs métier, sans exposer la structure JSON. -->
  <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : programmeEditor. -->
<div v-if="programmeEditor" class="overlay">
    <!-- Affiche une section de contenu ou formulaire. Le libellé accessible est « Nouvelle version du programme ». -->
<section
      class="modal programme-modal"
      role="dialog"
      aria-modal="true"
      aria-label="Nouvelle version du programme"
    >
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
        <!-- Affiche une titre du module. -->
<h2>Nouvelle version du programme</h2>
        <!-- Affiche une commande déclenchée par un clic. Le libellé accessible est « Fermer ». À l’événement click, exécute programmeEditor = null. -->
<button aria-label="Fermer" @click="programmeEditor = null">×</button>
      </div>
      <!-- Affiche une formulaire de validation. À l’événement submit, exécute saveProgramme. -->
<form @submit.prevent="saveProgramme">
        <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
          >Nom du programme<!-- Affiche une contrôle de saisie. Synchronise la saisie avec programmeEditor.name. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="programmeEditor.name" required
        /></label>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="toolbar">
          <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label>Niveau<!-- Affiche une contrôle de saisie. Synchronise la saisie avec programmeEditor.level. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="programmeEditor.level" required /></label
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Parcours<!-- Affiche une contrôle de saisie. Synchronise la saisie avec programmeEditor.track. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="programmeEditor.track" required /></label
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Spécialité<!-- Affiche une contrôle de saisie. Synchronise la saisie avec programmeEditor.speciality. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
              v-model="programmeEditor.speciality"
              required /></label
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Version<!-- Affiche une contrôle de saisie. Synchronise la saisie avec programmeEditor.version. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="programmeEditor.version" required
          /></label>
        </div>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="toolbar">
          <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Seuil de validation<!-- Affiche une contrôle de saisie. Synchronise la saisie avec programmeEditor.threshold. Le type de contrôle est number. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
              v-model.number="programmeEditor.threshold"
              type="number"
              step="any"
              min="0"
              max="20"
              required /></label
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Moyenne annuelle<!-- Affiche une liste de choix. Synchronise la saisie avec programmeEditor.annualMethod. -->
<select v-model="programmeEditor.annualMethod">
              <!-- Affiche une choix proposé dans la liste. -->
<option value="SEMESTER_EQUAL_WEIGHT">
                Même poids pour les deux semestres
              </option>
              <!-- Affiche une choix proposé dans la liste. -->
<option value="ALL_EC_WEIGHTED">Tous les EC pondérés</option>
            </select></label
          ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
            >Validation du programme<!-- Affiche une liste de choix. Synchronise la saisie avec programmeEditor.status. -->
<select v-model="programmeEditor.status">
              <!-- Affiche une choix proposé dans la liste. -->
<option value="DRAFT">Brouillon — données à confirmer</option>
              <!-- Affiche une choix proposé dans la liste. -->
<option value="APPROVED">Approuvé par la scolarité</option>
            </select></label
          >
        </div>
        <!-- Affiche une section de contenu ou formulaire. Répète cette zone pour : sem in programmeEditor.semesters. Calcule l’attribut key depuis sem.number. -->
<section
          v-for="sem in programmeEditor.semesters"
          :key="sem.number"
          class="panel"
        >
          <!-- Affiche une titre de semestre. -->
<h3>Semestre {{ sem.number }}</h3>
          <!-- Affiche une carte de données métier. Répète cette zone pour : u in sem.ues. Calcule l’attribut key depuis u.id. -->
<article v-for="u in sem.ues" :key="u.id">
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="toolbar">
              <!-- Affiche une libellé associé à un contrôle de saisie. -->
<label>Code UE<!-- Affiche une contrôle de saisie. Synchronise la saisie avec u.code. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="u.code" required /></label
              ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label>Nom UE<!-- Affiche une contrôle de saisie. Synchronise la saisie avec u.name. Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="u.name" required /></label
              ><!-- Affiche une libellé associé à un contrôle de saisie. -->
<label
                >Crédits UE<!-- Affiche une contrôle de saisie. Synchronise la saisie avec u.credits. Le type de contrôle est number. -->
<input
                  v-model.number="u.credits"
                  type="number"
                  step="any"
                  min="0" /></label
              ><!-- Affiche une commande déclenchée par un clic. Le type de contrôle est button. À l’événement click, exécute  sem.ues = sem.ues.filter((unit: Row) => unit.id !== u.id) . -->
<button
                type="button"
                @click="
                  sem.ues = sem.ues.filter((unit: Row) => unit.id !== u.id)
                "
              >
                Retirer UE
              </button>
            </div>
            <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="table-wrap">
              <!-- Affiche une tableau de données. -->
<table>
                <!-- Affiche une en-têtes de colonnes. -->
<thead>
                  <!-- Affiche une ligne de tableau. -->
<tr>
                    <!-- Affiche une titre de colonne. -->
<th>Code EC</th>
                    <!-- Affiche une titre de colonne. -->
<th>Matière</th>
                    <!-- Affiche une titre de colonne. -->
<th>CM</th>
                    <!-- Affiche une titre de colonne. -->
<th>TD</th>
                    <!-- Affiche une titre de colonne. -->
<th>TP</th>
                    <!-- Affiche une titre de colonne. -->
<th>Coefficient</th>
                    <!-- Affiche une titre de colonne. -->
<th></th>
                  </tr>
                </thead>
                <!-- Affiche une lignes du tableau. -->
<tbody>
                  <!-- Affiche une ligne de tableau. Répète cette zone pour : ec in u.ecs. Calcule l’attribut key depuis ec.id. -->
<tr v-for="ec in u.ecs" :key="ec.id">
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une contrôle de saisie. Synchronise la saisie avec ec.code. Le libellé accessible est « Code EC ». Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input v-model="ec.code" aria-label="Code EC" required />
                    </td>
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une contrôle de saisie. Synchronise la saisie avec ec.name. Le libellé accessible est « Nom de matière ». Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
                        v-model="ec.name"
                        aria-label="Nom de matière"
                        required
                      />
                    </td>
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une contrôle de saisie. Synchronise la saisie avec ec.cm. Le type de contrôle est number. Le libellé accessible est « Heures CM ». Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
                        v-model.number="ec.cm"
                        type="number"
                        min="0"
                        aria-label="Heures CM"
                        required
                      />
                    </td>
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une contrôle de saisie. Synchronise la saisie avec ec.td. Le type de contrôle est number. Le libellé accessible est « Heures TD ». Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
                        v-model.number="ec.td"
                        type="number"
                        min="0"
                        aria-label="Heures TD"
                        required
                      />
                    </td>
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une contrôle de saisie. Synchronise la saisie avec ec.tp. Le type de contrôle est number. Le libellé accessible est « Heures TP ». Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
                        v-model.number="ec.tp"
                        type="number"
                        min="0"
                        aria-label="Heures TP"
                        required
                      />
                    </td>
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une contrôle de saisie. Synchronise la saisie avec ec.coefficient. Le type de contrôle est number. Le libellé accessible est « Coefficient EC ». Le navigateur exige une valeur avant l’envoi du formulaire. -->
<input
                        v-model.number="ec.coefficient"
                        type="number"
                        min="0.01"
                        step="any"
                        aria-label="Coefficient EC"
                        required
                      />
                    </td>
                    <!-- Affiche une cellule de données. -->
<td>
                      <!-- Affiche une commande déclenchée par un clic. Le type de contrôle est button. À l’événement click, exécute  u.ecs = u.ecs.filter( (element: Row) => element.id !== ec.id, ) . -->
<button
                        type="button"
                        @click="
                          u.ecs = u.ecs.filter(
                            (element: Row) => element.id !== ec.id,
                          )
                        "
                      >
                        Retirer
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            <!-- Affiche une commande déclenchée par un clic. Le type de contrôle est button. À l’événement click, exécute addElement(u). -->
<button type="button" @click="addElement(u)">
              + Ajouter une matière
            </button>
          </article>
          <!-- Affiche une commande déclenchée par un clic. Le type de contrôle est button. À l’événement click, exécute addUnit(sem). -->
<button type="button" @click="addUnit(sem)">+ Ajouter une UE</button>
        </section>
        <!-- Affiche une texte de contexte ou message. Condition d’affichage : message. -->
<p v-if="message" class="notice error" role="alert">{{ message }}</p>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="modal-actions">
          <!-- Affiche une commande déclenchée par un clic. Le type de contrôle est button. À l’événement click, exécute programmeEditor = null. -->
<button type="button" @click="programmeEditor = null">Annuler</button
          ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis busy. -->
<button class="primary" :disabled="busy">
            Enregistrer cette nouvelle version
          </button>
        </div>
      </form>
    </section>
  </div>
  <!-- La fenêtre de formulaire est explicitement validée ou fermée. -->
  <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : modal. À l’événement click, exécute modal = null. -->
<div v-if="modal" class="overlay" @click.self="modal = null">
    <!-- Affiche une section de contenu ou formulaire. Calcule l’attribut aria-label depuis modal.title. -->
<section
      class="modal"
      role="dialog"
      aria-modal="true"
      :aria-label="modal.title"
    >
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="panel-heading">
        <!-- Affiche une titre du module. -->
<h2>{{ modal.title }}</h2>
        <!-- Affiche une commande déclenchée par un clic. Le libellé accessible est « Fermer ». À l’événement click, exécute modal = null. -->
<button aria-label="Fermer" @click="modal = null">×</button>
      </div>
      <!-- Affiche une formulaire de validation. À l’événement submit, exécute submitModal. -->
<form @submit.prevent="submitModal">
        <!-- Affiche une libellé associé à un contrôle de saisie. Répète cette zone pour : f in modal.fields. Calcule l’attribut key depuis f.key. -->
<label v-for="f in modal.fields" :key="f.key"
          >{{ f.label }}
          <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : f.type === 'multiselect'. -->
<div v-if="f.type === 'multiselect'" class="checklist">
            <!-- Affiche une libellé associé à un contrôle de saisie. Répète cette zone pour : o in f.options. Calcule l’attribut key depuis String(o.value). -->
<label v-for="o in f.options" :key="String(o.value)"
              ><!-- Affiche une contrôle de saisie. Synchronise la saisie avec form[f.key]. Le type de contrôle est checkbox. Calcule l’attribut value depuis o.value. -->
<input
                v-model="form[f.key]"
                type="checkbox"
                :value="o.value"
              />{{ o.label }}</label
            >
          </div>
          <!-- Affiche une liste de choix. Teste le cas alternatif : f.options. Synchronise la saisie avec form[f.key]. Calcule l’attribut required depuis f.required. -->
<select
            v-else-if="f.options"
            v-model="form[f.key]"
            :required="f.required"
          >
            <!-- Affiche une choix proposé dans la liste. -->
<option disabled value="">Choisir…</option>
            <!-- Affiche une choix proposé dans la liste. Répète cette zone pour : o in f.options. Calcule l’attribut key depuis String(o.value). Calcule l’attribut value depuis o.value. -->
<option
              v-for="o in f.options"
              :key="String(o.value)"
              :value="o.value"
            >
              {{ o.label }}
            </option></select
          ><!-- Affiche une saisie de texte sur plusieurs lignes. Teste le cas alternatif : ['textarea', 'json'].includes(f.type || ''). Synchronise la saisie avec form[f.key]. Calcule l’attribut rows depuis f.type === 'json' ? 9 : 3. Calcule l’attribut required depuis f.required. -->
<textarea
            v-else-if="['textarea', 'json'].includes(f.type || '')"
            v-model="form[f.key]"
            :rows="f.type === 'json' ? 9 : 3"
            :required="f.required"
          ></textarea
          ><!-- Affiche une contrôle de saisie. Teste le cas alternatif : f.type === 'file'. Le type de contrôle est file. Calcule l’attribut required depuis f.required. À l’événement change, exécute  form[f.key] = ($event.target as HTMLInputElement).files?.[0] . -->
<input
            v-else-if="f.type === 'file'"
            type="file"
            accept=".pdf,.png,.jpg,.jpeg,.docx,.xlsx,.pptx"
            :required="f.required"
            @change="
              form[f.key] = ($event.target as HTMLInputElement).files?.[0]
            " /><!-- Affiche une contrôle de saisie. Utilise cette branche lorsque la branche précédente ne s’affiche pas. Synchronise la saisie avec form[f.key]. Calcule l’attribut type depuis f.type || 'text'. Calcule l’attribut step depuis f.type === 'number' ? 'any' : undefined. Calcule l’attribut required depuis f.required. -->
<input
            v-else
            v-model="form[f.key]"
            :type="f.type || 'text'"
            :step="f.type === 'number' ? 'any' : undefined"
            :required="f.required"
        /></label>
        <!-- Affiche une texte de contexte ou message. Condition d’affichage : !modal.fields.length. -->
<p v-if="!modal.fields.length" class="notice">
          Cette action sera enregistrée dans le journal du SI.
        </p>
        <!-- Affiche une texte de contexte ou message. Condition d’affichage : message. -->
<p v-if="message" role="alert" class="notice error">{{ message }}</p>
        <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="modal-actions">
          <!-- Affiche une commande déclenchée par un clic. Le type de contrôle est button. À l’événement click, exécute modal = null. -->
<button type="button" @click="modal = null">Annuler</button
          ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis busy. -->
<button class="primary" :disabled="busy">
            {{ busy ? "Enregistrement…" : "Confirmer" }}
          </button>
        </div>
      </form>
    </section>
  </div>
  <!-- Les secrets nouvellement générés ne sont conservés que le temps de leur remise. -->
  <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : credentials. -->
<div v-if="credentials" class="overlay">
    <!-- Affiche une section de contenu ou formulaire. Le libellé accessible est « Identifiants générés ». -->
<section
      class="modal"
      role="dialog"
      aria-modal="true"
      aria-label="Identifiants générés"
    >
      <!-- Affiche une titre du module. -->
<h2>Informations à remettre personnellement</h2>
      <!-- Affiche une texte de contexte ou message. -->
<p class="muted">
        Copiez ou téléchargez ces informations avant de fermer. Les secrets ne
        sont pas récupérables ensuite.
      </p>
      <!-- Affiche une présentation des identifiants générés. -->
<pre>{{ JSON.stringify(credentials, null, 2) }}</pre>
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="modal-actions">
        <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute downloadCredentials. -->
<button @click="downloadCredentials">Télécharger JSON ↓</button
        ><!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute credentials = null. -->
<button class="primary" @click="credentials = null">
          J’ai conservé ces informations
        </button>
      </div>
    </section>
  </div>
  <!-- La prévisualisation montre toutes les erreurs avant toute écriture de notes. -->
  <!-- Affiche une groupe de contrôles ou de contenu. Condition d’affichage : preview. -->
<div v-if="preview" class="overlay">
    <!-- Affiche une section de contenu ou formulaire. Le libellé accessible est « Prévisualisation de l’import ». -->
<section
      class="modal"
      role="dialog"
      aria-modal="true"
      aria-label="Prévisualisation de l’import"
    >
      <!-- Affiche une titre du module. -->
<h2>Contrôle de l’import CSV</h2>
      <!-- Affiche une texte de contexte ou message. -->
<p>
        {{ preview.valid }} ligne(s) complète(s) ·
        {{ preview.errors.length }} erreur(s)
      </p>
      <!-- Affiche une texte de contexte ou message. Répète cette zone pour : error in preview.errors. Calcule l’attribut key depuis error. -->
<p v-for="error in preview.errors" :key="error" class="notice error">
        {{ error }}
      </p>
      <!-- Affiche une groupe de contrôles ou de contenu. -->
<div class="modal-actions">
        <!-- Affiche une commande déclenchée par un clic. À l’événement click, exécute preview = null. -->
<button @click="preview = null">Fermer / corriger le CSV</button
        ><!-- Affiche une commande déclenchée par un clic. Calcule l’attribut disabled depuis busy || preview.errors.length > 0 || preview.valid === 0. À l’événement click, exécute confirmPreview. -->
<button
          class="primary"
          :disabled="busy || preview.errors.length > 0 || preview.valid === 0"
          @click="confirmPreview"
        >
          Confirmer l’import
        </button>
      </div>
    </section>
  </div>
</template>
