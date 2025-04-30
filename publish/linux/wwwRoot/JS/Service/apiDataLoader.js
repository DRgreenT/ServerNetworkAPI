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
            const responseUiData = await fetch('/api/data');
            const responseNetworkData = await fetch('/api/network');

            if (!responseUiData.ok) {
                throw new Error(`UiData: HTTP error ${responseUiData.status}`);
            }
            if (!responseNetworkData.ok) {
                throw new Error(`NetworkData: HTTP error ${responseNetworkData.status}`);
            }

            let allUiData = await responseUiData.json();
            let allNetworkData = await responseNetworkData.json();

            version = allUiData.version;
            noteData = allUiData.notifications;
            devicesData = allNetworkData;
            logData = allUiData.log;
            arrActiveDeviceCounts = allUiData.activeDevicesCounts;
            systemData = allUiData.systemInfo;

            if (!uiData.activeDeviceCount) uiData.activeDeviceCount = 0;
            if (!uiData.totalDeviceCount) uiData.totalDeviceCount = 0;
            if (!uiData.lastUpdateTime) uiData.lastUpdateTime = "---";
            if (!uiData.uptime) uiData.uptime = "---";
            if (uiData.isInternetAvailable === undefined) uiData.isInternetAvailable = false;
            if (uiData.isNmapEnabled === undefined) uiData.isNmapEnabled = false;

            uiData.activeDeviceCount = allUiData.activeDeviceCount;
            uiData.totalDeviceCount = allUiData.totalDeviceCount;
            uiData.lastUpdateTime = allUiData.lastUpdateTime;
            uiData.uptime = allUiData.uptime;
            uiData.isInternetAvailable = allUiData.isInternetAvailable;
            uiData.isNmapEnabled = allUiData.isNmapEnabled;

        } catch (err) {
            console.error('Error fetching or processing data:', err);
        }
        await new Promise(resolve => setTimeout(resolve, 2000));
    }
}
