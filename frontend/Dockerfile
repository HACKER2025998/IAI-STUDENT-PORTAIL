# Node est nécessaire à la compilation de Nuxt.
FROM node:24-alpine AS build
# Le frontend possède ses propres dépendances.
WORKDIR /app
# Installe exactement les versions du fichier verrouillé.
COPY frontend/package*.json ./
# npm ci rend les builds reproductibles.
RUN npm ci
# Copie le code de l'interface.
COPY frontend/ ./
# Le proxy Nuxt cible le service Docker backend.
ENV API_URL=http://backend:8080
# Construit le serveur de production Nuxt.
RUN npm run build
# L'image finale ne contient pas les dépendances de développement.
FROM node:24-alpine
# Le serveur publié est autonome.
WORKDIR /app
# Copie uniquement les sorties de production.
COPY --from=build /app/.output ./.output
# Le port Nuxt interne est 3000.
ENV PORT=3000 HOST=0.0.0.0
# Documente le port attendu par le proxy Nginx.
EXPOSE 3000
# Lance le serveur frontend Nitro.
CMD ["node", ".output/server/index.mjs"]
