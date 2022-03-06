# sets the retention of the power values to 5 minutes. Retention Job is running every 30 min !
influx \
    -host 192.168.0.156 \
    -execute  " \
        CREATE DATABASE iot_short_term WITH DURATION 1h REPLICATION 1 SHARD DURATION 1h NAME retention_1h; \
        CREATE DATABASE iot_mid_term WITH DURATION 60d REPLICATION 1 SHARD DURATION 60d NAME retention_60d; \
        CREATE DATABASE iot_long_term WITH DURATION INF REPLICATION 1 SHARD DURATION INF; \
        CREATE DATABASE metrics WITH DURATION 15d REPLICATION 1 SHARD DURATION 15d NAME retention_15d; \
        
        CREATE USER iobroker WITH PASSWORD '$DB_PASSWORD'; \
        GRANT READ ON iot_short_term TO iobroker; \
        GRANT READ ON metrics TO iobroker; \
        GRANT WRITE ON iot_short_term TO iobroker; \
        GRANT WRITE ON metrics TO iobroker; \
          
        CREATE USER grafana WITH PASSWORD '$DB_PASSWORD'; \
        GRANT READ ON iot_short_term TO grafana; \
        GRANT READ ON iot_mid_term TO grafana; \
        GRANT READ ON iot_long_term TO grafana; \
        GRANT READ ON metrics TO grafana; \
    " \