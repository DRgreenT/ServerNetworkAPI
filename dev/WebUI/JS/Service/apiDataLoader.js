let logData = [];
let noteData = [];
let devicesData = [];
let uiData = [];
let systemData = [];
let arrActiveDeviceCounts = [];
let version;


let isGetData = true;

async function getValues() {
    allLogs = logData;
    allDashboardData = uiData;
}

async function fetchApiData() {
    while (isGetData) {
        try {
            const response = await fetch('/api/data');

            if (!response.ok) {
                throw new Error(`HTTP error ${response.status}`);
            }

            let allApiData = await response.json();

            version = allApiData.version;
            noteData = allApiData.notifications;
            devicesData = allApiData.devices;
            logData = allApiData.log;
            arrActiveDeviceCounts = allApiData.activeDevicesCounts;
            systemData = allApiData.systemInfo;

            if (!uiData.activeDeviceCount) uiData.activeDeviceCount = 0;
            if (!uiData.totalDeviceCount) uiData.totalDeviceCount = 0;
            if (!uiData.lastUpdateTime) uiData.lastUpdateTime = "---";
            if (!uiData.uptime) uiData.uptime = "---";
            if (uiData.isInternetAvailable === undefined) uiData.isInternetAvailable = false;
            if (uiData.isNmapEnabled === undefined) uiData.isNmapEnabled = false;

            uiData.activeDeviceCount = allApiData.activeDeviceCount;
            uiData.totalDeviceCount = allApiData.totalDeviceCount;
            uiData.lastUpdateTime = allApiData.lastUpdateTime;
            uiData.uptime = allApiData.uptime;
            uiData.isInternetAvailable = allApiData.isInternetAvailable;
            uiData.isNmapEnabled = allApiData.isNmapEnabled;

        } catch (err) {
            console.error('Error fetching or processing data:', err);
        }
        await new Promise(resolve => setTimeout(resolve, 2000));
    }
}
