# influxdb-configuration
Configuration for influxdb to collect and downsample power data
Data is measured by iobroker's smartmeter adapter and store in influxdb on raspberrypi

Data measured:
- smartmeter.0.1-0:16_7_0__255: current electrical power
- smartmeter.0.1-0:1_8_0__255: current electrical work

# iobroker install
curl -sLf https://iobroker.net/install.sh | bash -

# influxdb install
wget -qO- https://repos.influxdata.com/influxdb.key | sudo apt-key add - \
source /etc/os-release \
echo "deb https://repos.influxdata.com/debian $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/influxdb.list \
sudo apt-get update && sudo apt-get install -y influxdb \
sudo service influxdb start

# grafana install
wget -q -O - https://packages.grafana.com/gpg.key | sudo apt-key add - \
echo "deb https://packages.grafana.com/oss/deb stable main" | sudo tee -a /etc/apt/sources.list.d/grafana.list \
sudo apt-get update && sudo apt-get install -y adduser libfontconfig1 grafana \
sudo systemctl enable grafana-server.service

http://192.168.0.156:8086

# install influx-cli
echo "deb https://repos.influxdata.com/ubuntu $(lsb_release -cs) stable" | sudo tee /etc/apt/sources.list.d/influxdb.list
sudo apt-get install influxdb-client

# prepare environment
Scripts uses environment variables.
- create an '.iot-env' file with:
  ```
  export DB_PASSWORD=<password>
  export GRAFANA_USER=<user>
  export GRAFANA_PASSWORD=<password>
  export IOT_HOST=<ip-address>
  ```
- execute ```source ./.iot-env```

# enable continuous query
nano /etc/influxdb/influxdb.conf
section [continuous_queries]
- uncomment enabled = true
- log-enabled = true
- run-interval = "1s"

# validate continuous query is running
sudo journalctl -u influxdb.service | grep "continuous query"

<code>
Okt 30 09:01:00 HomeSrv influxd-systemd-start.sh[4083]: ts=2022-10-30T08:01:00.754528Z lvl=info msg="Executing continuous query" log_id=0dqzMtk0000 service=continuous_querier trace_id=0dqzNeKW000 op_name=continuous_querier_execute name=electric_power_1h_ext db_instance=iot_mid_term start=2022-10-30T07:01:00.000000Z end=2022-10-30T08:01:00.000000Z

Okt 30 09:01:00 HomeSrv influxd-systemd-start.sh[4083]: ts=2022-10-30T08:01:00.771732Z lvl=info msg="Finished continuous query" log_id=0dqzMtk0000 service=continuous_querier trace_id=0dqzNeKW000 op_name=continuous_querier_execute name=electric_power_1h_ext db_instance=iot_mid_term written=1 start=2022-10-30T07:01:00.000000Z end=2022-10-30T08:01:00.000000Z duration=17ms
</code>
