#!/usr/bin/bash
set -o errexit
set -o pipefail
set -o nounset

BACKUP_DIR="./grafana.backup"
HOST="$IOT_HOST:3000"

PWD=$GRAFANA_PASSWORD
USR=$GRAFANA_USER

# -------------------------------------------------------------------------------------------------------
# --- Data Sources
# -------------------------------------------------------------------------------------------------------
echo "Exporting Grafana datasources from $HOST"

if [ ! -d $BACKUP_DIR/datasources ] ; then
    mkdir -p $BACKUP_DIR/datasources
fi

curl -s "http://$HOST/api/datasources" -u $USR:$PWD | jq -c -M '.[]'|split -l 1 - $BACKUP_DIR/datasources/

# -------------------------------------------------------------------------------------------------------
# --- Dashboard
# -------------------------------------------------------------------------------------------------------
echo "Exporting Grafana dashboards from $HOST"
if [ ! -d $BACKUP_DIR/dashboards ] ; then
    mkdir -p $BACKUP_DIR/dashboards
fi

for dash in $(curl -s "http://$HOST/api/search" -u $USR:$PWD | jq -r '.[] | select(.type == "dash-db") | .uid'); do
        curl -s "http://$HOST/api/dashboards/uid/$dash" -u $USR:$PWD > $BACKUP_DIR/dashboards/${dash}.json
        slug=$(cat $BACKUP_DIR/dashboards/${dash}.json | jq -r '.meta.slug')
        mv $BACKUP_DIR/dashboards/${dash}.json $BACKUP_DIR/dashboards/${dash}-${slug}.json
done