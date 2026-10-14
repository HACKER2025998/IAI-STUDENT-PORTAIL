# Le SDK compile le backend sans être conservé dans l'image finale.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
# Le répertoire de travail contient les sources nécessaires à la compilation.
WORKDIR /src
# Copie les fichiers du backend et les programmes photographiés.
COPY backend/ backend/
# Le fichier csproj référence le dossier data situé à côté du backend.
COPY data/ data/
# Publie une version optimisée pour l'exécution sur le serveur.
RUN dotnet publish backend/backend.csproj -c Release -o /out
# Le runtime ASP.NET 10 complète l’image Chromium utilisée pour les PDF.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
# Chromium et ses bibliothèques système sont fournis par l’image Playwright officielle.
FROM mcr.microsoft.com/playwright/dotnet:v1.56.0-noble
# Les clés, fichiers et paramètres sont relatifs à ce répertoire.
WORKDIR /app
# Fournit le runtime .NET 10 utilisé par l’application.
COPY --from=runtime /usr/share/dotnet/ /usr/share/dotnet/
# Copie uniquement le résultat publié de l'étape précédente.
COPY --from=build /out .
# Le serveur écoute sur le port interne 8080.
ENV ASPNETCORE_HTTP_PORTS=8080
# Documente le port utilisé par Docker Compose.
EXPOSE 8080
# Lance l'application métier compilée.
ENTRYPOINT ["dotnet", "backend.dll"]
