// Le test réutilise le navigateur Chromium installé pour les relevés PDF.
import { chromium } from '../frontend/node_modules/playwright/index.mjs';
// Le chemin est configurable pour les postes locaux et la CI.
const executablePath = process.env.CHROMIUM_PATH || '/tmp/iai-browsers/chromium-1194/chrome-linux/chrome';
// Le navigateur est exécuté sans interface graphique dans l'environnement de test.
const browser = await chromium.launch({ executablePath, headless: true, args: ['--no-sandbox'] });
// Une session isolée vérifie l'affichage de bureau sans reprendre de cookies existants.
const page = await browser.newPage({ viewport: { width: 1440, height: 1000 } });
// Capture les erreurs JavaScript pour ne pas confondre une page visible avec une page fonctionnelle.
const errors = [];
// Toute exception du script de l'interface fait échouer le test final.
page.on('pageerror', error => errors.push(error.message));
// Ouvre l'écran de connexion réellement servi par Nuxt.
await page.goto(process.env.FRONTEND_URL || 'http://127.0.0.1:3000');
// Vérifie le titre de connexion plutôt qu'un texte générique du serveur.
await page.getByRole('heading', { name: 'Bienvenue sur votre portail' }).waitFor();
// Conserve une capture du rendu de bureau pour inspection visuelle.
await page.screenshot({ path: '/tmp/iai-login-desktop.png', fullPage: true });
// Renseigne le compte de test créé dans la base dédiée.
await page.getByLabel('Identifiant', { exact: true }).fill(process.env.PORTAL_ADMIN || 'admin');
// Le mot de passe du test n'est pas écrit dans les captures d'écran.
await page.getByLabel('Mot de passe', { exact: true }).fill(process.env.PORTAL_PASSWORD || 'LocalTestOnly2026!');
// Le cookie HttpOnly est établi par l'API derrière le proxy Nuxt.
await page.getByRole('button', { name: 'Se connecter' }).click();
// L'accueil doit être réellement chargé avec les données de l'API.
await page.getByRole('heading', { name: 'Accueil', exact: true }).waitFor();
// Capture le dashboard pour vérifier cartes, navigation et alignements.
await page.screenshot({ path: '/tmp/iai-dashboard-desktop.png', fullPage: true });
// Vérifie les écrans principaux administratifs sans recharger artificiellement la page.
for (const item of ['Étudiants', 'Programmes', 'Notes & bulletins', 'Présences', 'Rôles & permissions']) {
  // Les boutons de navigation sont sélectionnés dans la barre latérale uniquement.
  await page.locator('nav').getByRole('button', { name: item, exact: true }).click();
  // Chaque page doit produire son titre principal.
  await page.getByRole('heading', { name: item, exact: true }).first().waitFor();
}
// Le contrôle mobile utilise la même session pour vérifier le contenu responsive.
await page.setViewportSize({ width: 390, height: 844 });
// Retourne à l'accueil pour inspecter la disposition mobile des cartes.
await page.locator('nav').getByRole('button', { name: 'Accueil', exact: true }).click();
// Une capture mobile complète montre la navigation horizontale et les panneaux empilés.
await page.screenshot({ path: '/tmp/iai-dashboard-mobile.png', fullPage: true });
// La page ne doit pas déborder horizontalement sur le téléphone.
const overflow = await page.evaluate(() => document.documentElement.scrollWidth > window.innerWidth);
// Signale séparément les erreurs de script et les débordements de mise en page.
if (errors.length || overflow) throw new Error(JSON.stringify({ errors, overflow }));
// Ferme le navigateur de test et libère ses ressources.
await browser.close();
// Le message est affiché seulement après toutes les vérifications réussies.
console.log('OK : connexion, dashboard, modules administratifs et écran mobile sans débordement ni erreur JavaScript.');
