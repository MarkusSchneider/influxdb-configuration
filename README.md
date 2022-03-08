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

# grafana installieren
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
  export GRAFANA_USER=<uder>
  export GRAFANA_PASSWORD=<password>
  export IOT_HOST=<ip-address>
  ```
- execute ```source ./.iot-env```
