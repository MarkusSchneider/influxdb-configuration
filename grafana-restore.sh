BACKUP_DIR="./grafana.backup"
HOST="http://$IOT_HOST:3000"

# backup grafana data sources
PWD=$GRAFANA_PASSWORD
USR=$GRAFANA_USER

for i in /opt/grafana.backup/datasources/*; do \
    curl -X "POST" "http://$HOST/api/datasources" \
    -H "Content-Type: application/json" \
    --user $USR:$PWD \
    --data-binary @$i
done