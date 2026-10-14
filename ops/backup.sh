#!/usr/bin/env sh
# Quitte immédiatement si une commande échoue ou utilise une variable absente.
set -eu
# Le dossier de sortie est configurable et doit être sauvegardé hors du serveur.
backup_dir=${1:-./backups}
# Crée le répertoire sans modifier les sauvegardes existantes.
mkdir -p "$backup_dir"
# Le suffixe UTC distingue chaque sauvegarde.
backup_stamp=$(date -u +%Y%m%dT%H%M%SZ)
# Exporte PostgreSQL dans son format de restauration natif.
docker compose exec -T postgres pg_dump -U iai -d iai_portal -Fc > "$backup_dir/database-$backup_stamp.dump"
# Sauvegarde les fichiers et les clés nécessaires à la lecture des cookies et secrets MFA.
docker compose exec -T backend tar czf - storage keys > "$backup_dir/files-$backup_stamp.tar.gz"
# Affiche les deux artefacts à copier hors du serveur.
printf '%s\n' "$backup_dir/database-$backup_stamp.dump" "$backup_dir/files-$backup_stamp.tar.gz"
