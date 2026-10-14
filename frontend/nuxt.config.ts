// Nuxt sert l'interface Vue et relaie les appels API vers le backend sur la même origine.
export default defineNuxtConfig({
  // Cette date stabilise les comportements du framework pour le projet.
  compatibilityDate: "2026-10-05",
  // L'espace connecté n'a pas besoin de rendu serveur pour ses données privées.
  ssr: false,
  // Les outils de développement ne sont pas inclus dans les écrans du portail.
  devtools: { enabled: false },
  // Charge la feuille de style commune à tous les écrans.
  css: ["~/assets/main.css"],
  // Le proxy évite de mettre les cookies de session sur un domaine tiers.
  routeRules: {
    "/api/**": {
      proxy: `${process.env.API_URL || "http://127.0.0.1:5080"}/api/**`,
    },
  },
  // Présente correctement l'application sur mobile et dans les lecteurs d'écran.
  app: {
    head: {
      title: "IAI Student Portal",
      htmlAttrs: { lang: "fr" },
      meta: [
        { name: "viewport", content: "width=device-width, initial-scale=1" },
        { name: "theme-color", content: "#123d39" },
      ],
      link: [{ rel: "manifest", href: "/manifest.webmanifest" }],
    },
  },
  // Active le contrôle de types durant le développement.
  typescript: { strict: true },
});
