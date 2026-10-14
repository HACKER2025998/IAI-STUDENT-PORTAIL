// L'enregistrement reste réservé au navigateur et aux builds de production.
export default defineNuxtPlugin(() => {
  // Le mode développement conserve le comportement normal de Vite sans cache persistant.
  if (import.meta.dev || !('serviceWorker' in navigator)) return;
  // La fenêtre doit être chargée avant de lancer l'installation du shell public.
  window.addEventListener('load', () => {
    // Une erreur de connexion n'empêche pas d'utiliser le portail en ligne.
    navigator.serviceWorker.register('/sw.js').catch(() => {});
  });
});
