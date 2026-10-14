// Ce cache contient uniquement le code et les éléments publics de l'application.
const CACHE = 'iai-public-shell-v1';
// Installe le shell public sans mettre en cache une réponse contenant des notes ou un dossier.
self.addEventListener('install', event => { event.waitUntil(caches.open(CACHE).then(cache => cache.addAll(['/', '/icon.svg', '/manifest.webmanifest']))); });
// Une nouvelle version retire les anciens caches publics sans conserver de données de connexion.
self.addEventListener('activate', event => { event.waitUntil(caches.keys().then(keys => Promise.all(keys.filter(key => key !== CACHE).map(key => caches.delete(key))))); });
// Les appels privés de l'API passent toujours par le serveur pour vérifier la session et les droits.
self.addEventListener('fetch', event => {
  // L'URL doit appartenir à la même origine que le portail.
  const url = new URL(event.request.url);
  // Ignore toute mutation, ressource externe ou endpoint API privé.
  if (event.request.method !== 'GET' || url.origin !== self.location.origin || url.pathname.startsWith('/api')) return;
  // Seul le shell et les fichiers de code Nuxt sont éligibles à ce cache public.
  if (!['/', '/icon.svg', '/manifest.webmanifest'].includes(url.pathname) && !url.pathname.startsWith('/_nuxt/')) return;
  // Utilise le réseau si possible et conserve une copie de ses ressources publiques.
  event.respondWith(fetch(event.request).then(response => { if (response.ok) { const copy = response.clone(); caches.open(CACHE).then(cache => cache.put(event.request, copy)); } return response; }).catch(() => caches.match(event.request)));
});
