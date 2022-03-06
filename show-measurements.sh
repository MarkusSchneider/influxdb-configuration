# show the 'tables' eg measurments of the influxdb
influx \
    -host $IOT_HOST \
    -execute  " \
    SHOW MEASUREMENTS ON iot_short_term; \
    SHOW MEASUREMENTS ON iot_mid_term; \
    SHOW MEASUREMENTS ON iot_long_term; \
    SHOW MEASUREMENTS ON metrics; \
    "\