#!/usr/bin/env bash
# Dnevni bekap SQLite baze. Namijenjeno za cron na produkcijskom serveru, npr:
#   0 3 * * * /path/to/api/scripts/backup-db.sh >> /var/log/rezervacije-backup.log 2>&1
#
# Kopira rezervacije.db u backups/ sa datumom u imenu i briše bekape starije od 30 dana.
# Za pravu zaštitu (server može otkazati), backups/ folder treba periodično sinhronizovati
# na neko cloud skladište (npr. rclone ka S3/Backblaze) - ovo je samo lokalni prvi sloj zaštite.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DB_PATH="$SCRIPT_DIR/../Rezervacije.Api/rezervacije.db"
BACKUP_DIR="$SCRIPT_DIR/../backups"
KEEP_DAYS=30

if [ ! -f "$DB_PATH" ]; then
  echo "Baza ne postoji: $DB_PATH" >&2
  exit 1
fi

mkdir -p "$BACKUP_DIR"

TIMESTAMP="$(date +%Y-%m-%d_%H-%M-%S)"
DEST="$BACKUP_DIR/rezervacije-$TIMESTAMP.db"

sqlite3 "$DB_PATH" ".backup '$DEST'"
echo "Bekap sačuvan: $DEST"

find "$BACKUP_DIR" -name "rezervacije-*.db" -mtime "+$KEEP_DAYS" -delete
