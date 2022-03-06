# show the 'tables' eg measurments of the influxdb
influx \
    -host 192.168.0.156 \
    -execute  " \
    SHOW MEASUREMENTS ON iot_short_term; \
    SHOW MEASUREMENTS ON iot_mid_term; \
    SHOW MEASUREMENTS ON iot_long_term; \
    SHOW MEASUREMENTS ON metrics; \
    "\