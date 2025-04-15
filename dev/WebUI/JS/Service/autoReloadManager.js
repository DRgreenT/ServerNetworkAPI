let isUpdateDashboardCancelled = false;
let isUpdateLogCancelled = false;

function setUpdate(name) {
    if (name === "dash") {
        isUpdateDashboardCancelled = false;
        isUpdateLogCancelled = true;
    } else if (name === "log") {
        isUpdateLogCancelled = false;
        isUpdateDashboardCancelled = true;
    } else if (!name) {
        isUpdateDashboardCancelled = true;
        isUpdateLogCancelled = true;
    }
}
