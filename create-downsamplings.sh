# create a continuous query to shift all measurements from database 'iot_short_term' with a measurment period of 1 Second
# into the database 'iot_mid_term' wiht a measurement period of 1 Min.
#
# Continuous Query Syntax:
# CREATE CONTINUOUS QUERY <cq_name> ON <database_name>
# BEGIN
#   <cq_query>
# END
#
# cq_query Syntax:
# SELECT <function[s]> 
# INTO <database_name>.<retention_policy>.<measurement> 
# FROM <database_name>.<retention_policy>.<measurement> 
# [WHERE <stuff>] 
# GROUP BY time(<interval>)[,<tag_key[s]>]
#
# Hint: leave 'retention_policy' emtpy for default see .. qualifier below
# fill() is not working with basic continuous queries


influx \
    -host $IOT_HOST \
    -execute  " \
    DROP CONTINUOUS QUERY electric_power_1h ON iot_mid_term; \
    CREATE CONTINUOUS QUERY electric_power_1h ON iot_mid_term BEGIN \
        SELECT last(value) as value \
        INTO iot_mid_term..\"smartmeter.0.1-0:1_8_0__255.value\" \
        FROM iot_short_term..\"smartmeter.0.1-0:1_8_0__255.value\" \
        GROUP BY time(1h) \
    END; \
    DROP CONTINUOUS QUERY electric_power_1d ON iot_long_term; \
    CREATE CONTINUOUS QUERY electric_power_1d ON iot_long_term BEGIN \
        SELECT last(value) as value \
        INTO iot_long_term..\"smartmeter.0.1-0:1_8_0__255.value\" \
        FROM iot_mid_term..\"smartmeter.0.1-0:1_8_0__255.value\" \
        GROUP BY time(1d) \
    END; \
    SHOW CONTINUOUS QUERIES; \
    DROP CONTINUOUS QUERY electric_power_1h_ext ON iot_mid_term; \
    CREATE CONTINUOUS QUERY electric_power_1h_ext ON iot_mid_term BEGIN \
        SELECT last(value) as value \
        INTO iot_mid_term..\"smartmeter.0.1-0:1_8_0__255.value_ext\" \
        FROM iot_short_term..\"smartmeter.0.1-0:1_8_0__255.value\" \
        GROUP BY time(1h, 1m) \
    END; \
    "\