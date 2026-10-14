#!/usr/bin/env sh
# La restauration doit se faire sur une installation de secours à base vide.
set -eu
# Exige le fichier de base et le fichier documentaire d'une même sauvegarde.
: "${1:?Fichier .dump requis}" "${2:?Archive fichiers requise}"
# Restaure le schéma et les données sans effacer une base existante.
docker compose exec -T postgres pg_restore -U iai -d iai_portal --no-owner --exit-on-error < "$1"
# Restaure les documents et les clés privées dans le volume du backend.
docker compose exec -T backend tar xzf - < "$2"
