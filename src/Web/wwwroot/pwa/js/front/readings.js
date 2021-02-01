jQuery($ => {
    window.CSM.readingsPage = {
        template: '#readingsPageTemplate',
        data: () => ({
            sensors: [],
            currentSensor: {
                readings: []
            },
            table: {
                collapsed: true
            },
            chart: {
                currentParameter: "cO2",
                instance: null,
                dataset: []
            },
            markers: [],
            map: null,
            hub: {
                instance: null,
                isActive: false
            }
        }),
        mounted: function () {
            const app = this;
            $.ajax({
                type: "GET",
                url: "api/sensors",
                dataType: "JSON",
                success: responce => {
                    app.sensors = app.sensors.concat(responce);
                    app.initHub();
                    ymaps.ready(() => {
                        app.initMap();
                        app.initMarkers();
                    });
                    app.initChart();
                    app.initDataset();
                }
            });
        },
        methods: {
            initHub: initHub,
            initMap: initMap,
            initMarkers: initMarkers,
            updateMarker: updateMarker,
            initChart: initChart,
            initDataset: initDataset,
            updateDataset: updateDataset,
            expandTable: function () {
                this.table.collapsed = false;
            },
            collapseTable: function () {
                this.table.collapsed = true;
            },
            setChartCurrentParameter: function (param) {
                this.chart.currentParameter = param;
                this.initDataset();
            },
            subscribeOnSensor: function () {
                const that = this;
                window.CSM.askForPermissioToReceiveNotifications().then(token => {
                    const sensors = JSON.parse(localStorage.getItem("subscribedOnSensors")) || [];
                    if (sensors.indexOf(that.currentSensor.id) == -1) {
                        $.ajax({
                            type: "POST",
                            url: "api/notifications",
                            contentType: "application/json",
                            data: JSON.stringify({
                                sensorId: that.currentSensor.id,
                                registrationToken: token
                            }),
                            success: responce => {
                                sensors.push(that.currentSensor.id);
                                localStorage.setItem("subscribedOnSensors", JSON.stringify(sensors));
                                alert("Успешно подписан");
                            },
                            error: error => {
                                alert("Ошибка");
                            }
                        });
                    }
                    else {
                        alert("Уже подписан!");
                    }
                });
            },
            unsubscribeFromSensor: function () {
                const that = this;
                window.CSM.askForPermissioToReceiveNotifications().then(token => {
                    const sensors = JSON.parse(localStorage.getItem("subscribedOnSensors")) || [];
                    if (sensors.indexOf(that.currentSensor.id) == -1) {
                        alert("Уже отписан");
                    }
                    $.ajax({
                        type: "DELETE",
                        url: "api/notifications",
                        contentType: "application/json",
                        data: JSON.stringify({
                            sensorId: that.currentSensor.id,
                            registrationToken: token
                        }),
                        success: responce => {
                            const index = sensors.indexOf(that.currentSensor.id);
                            if (index != -1) {
                                sensors.splice(index, 1);
                            }
                            localStorage.setItem("subscribedOnSensors", JSON.stringify(sensors));
                            alert("Успешно отписан");
                        },
                        error: error => {
                            alert("Ошибка");
                        }
                    });
                })
            }
        },
        computed: {
            isSubscribed: function () {
                const that = this;
                const sensors = JSON.parse(localStorage.getItem("subscribedOnSensors")) || [];
                return sensors.indexOf(that.currentSensor.id) != -1;
            }
        },
        watch: {
            currentSensor:
            {
                handler: function (val) {
                    this.updateDataset();
                },
                deep: true
            }
        },
        beforeRouteLeave: function (to, from, next) {
            const that = this;
            //that.hub.isActive = false;
            next();
        }
    };



    //HUB
    function initHub() {
        const that = this;
        that.hub.instance = new signalR.HubConnectionBuilder()
            .withUrl("/pwahub")
            .configureLogging(signalR.LogLevel.Information)
            .build();
        that.hub.instance.on("DispatchReadingAsync", readingModel => {
            const sensor = that.sensors.find((e, i, a) => {
                if (e.id == readingModel.sensorId) {
                    return e;
                }
            });
            if (sensor == null) {
                return;
            }
            sensor.readings.unshift(readingModel.reading);
            sensor.pollutionLevel = readingModel.pollutionLevel;
            if (sensor.readings.length == 11) {
                sensor.readings.pop();
            }
            that.updateMarker(sensor);
        });
        that.hub.instance.start().then(() => {
            that.hub.isActive = true;
            //$.connection.hub.disconnected(function () {
            //    if (!that.hub.isActive) {
            //        return;
            //    }
            //    setTimeout(function () {
            //        $.connection.hub.start();
            //    }, 5000);
            //});
        });
    }
    //MAP
    function initMap() {
        this.map = new ymaps.Map("map", {
            center: [53.904502, 27.561261],
            zoom: 11,
            controls: ["zoomControl"]
        },
            {
                searchControlProvider: 'yandex#search',
                //restrictMapArea: true
            });
    }

    function initMarkers() {
        const that = this;
        that.sensors.forEach((sensor, index, arrya) => {
            const reading = sensor.readings[0];
            if (typeof (reading) === "undefined") {
                return;
            }
            const marker = {
                sensorId: sensor.id,
                value: createMarker(sensor)
            };
            marker.value.events.add('click', () => {
                that.currentSensor = sensor;
                $('#sensor-details').modal('show')
            });
            that.markers.push(marker);
            that.map.geoObjects.add(marker.value);
        })
    }

    function createMarker(sensor) {
        const that = this;
        const marker = new ymaps.Circle([
            [sensor.latitude, sensor.longitude],
            1000
        ], {
            hintContent: "Уровень загрязнения " + sensor.pollutionLevel
        }, {
            draggable: false,
            fillColor: getFillColor(sensor.pollutionLevel),
            strokeColor: getStrokeColor(sensor.pollutionLevel),
            strokeOpacity: 0.8,
            fillOpacity: 0.6,
            strokeWidth: 3
        });
        return marker;
    }
    function updateMarker(sensor) {
        const that = this;
        const marker = that.markers.find(marker => marker.sensorId == sensor.id);
        if (marker == null) {
            return;
        }
        marker.value.properties.set({ hintContent: "Уровень загрязнения " + sensor.pollutionLevel });
        marker.value.options.set({ fillColor: getFillColor(sensor.pollutionLevel), strokeColor: getStrokeColor(sensor.pollutionLevel) });
    }

    function initChart() {
        const config = {
            type: 'line',
            data: {
                labels: [],
                datasets: []
            },
            options: {
                responsive: true,
                title: {
                    display: false
                },
                tooltips: {
                    mode: 'index',
                    intersect: false,
                },
                hover: {
                    mode: 'nearest',
                    intersect: true
                },
                scales: {
                    xAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Cнято'
                        }
                    }],
                    yAxes: [{
                        display: true,
                        scaleLabel: {
                            display: true,
                            labelString: 'Значение'
                        }
                    }]
                }
            }
        };
        const ctx = document.getElementById('chart').getContext('2d');
        this.chart.instance = new Chart(ctx, config);
    }

    function initDataset() {
        const that = this;
        if (that.chart.instance == null) {
            return;
        }
        that.chart.instance.data.datasets = [];
        that.chart.instance.data.labels = this.currentSensor.readings.map(reading => moment(reading.created).format('h:mm:ss'));
        that.chart.dataset = {
            label: this.chart.currentParameter.toUpperCase(),
            backgroundColor: "#fff",
            borderColor: "#c2c2c2c2",
            data: this.currentSensor.readings.map(reading => reading[that.chart.currentParameter]),
            fill: false,
        }
        that.chart.instance.data.datasets.push(that.chart.dataset);
        that.chart.instance.chart.update();
    }

    function updateDataset() {
        const that = this;
        const reading = that.currentSensor.readings[0];
        if (typeof (reading) === "undefined") {
            return;
        }
        that.chart.instance.data.labels.push(moment(reading.created).format('h:mm:ss'));
        that.chart.instance.data.labels.shift();
        that.chart.dataset.data.push(reading[this.chart.currentParameter]);
        that.chart.dataset.data.shift();
        that.chart.instance.update();
    }


    function getFillColor(pollutionLevel) {
        switch (pollutionLevel) {
            case 0:
                return "#1adb2d";
            case 1:
                return "#db971a";
            case 2:
                return "#e20000";
            case 3:
                return "#04e9f9";
        }
        return null;
    }

    function getStrokeColor(pollutionLevel) {
        switch (pollutionLevel) {
            case 0:
                return "#106319";
            case 1:
                return "#8c5e09";
            case 2:
                return "#770707";
            case 3:
                return "#036af9";
        }
        return null;
    }
});