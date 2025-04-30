let isUpdateDashboardCancelled = false;
let isUpdateLogCancelled = false;
let isDeviceListCancelled = false;

function setUpdate(name) {
    if (name === "dash") {
        isUpdateDashboardCancelled = false;
        isUpdateLogCancelled = true;
        isDeviceListCancelled = true;}
         
    else if (name === "log") {
        isUpdateLogCancelled = false;
        isUpdateDashboardCancelled = true;
        isDeviceListCancelled = true;}

    else if (name== "device"){
        isUpdateDashboardCancelled = true;
        isUpdateLogCancelled = true;
        isDeviceListCancelled = false;}

    else if (!name) {
        isUpdateDashboardCancelled = true;
        isUpdateLogCancelled = true;
        isDeviceListCancelled = true;}
}
