function loadScript(path) {
    return new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = path;
        script.defer = true;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`Failed to load ${path}`));
        document.head.appendChild(script);
    });
}

const scriptsToLoad = [
    "./JS/Service/apiDataLoader.js",
    "./JS/Service/autoReloadManager.js",
    "./JS/Sites/dashboard.js",
    "./JS/Sites/devices.js",
    "./JS/Sites/log.js",
    "./JS/Sites/about.js",
    "./JS/Sites/notes.js"
];

Promise.all(scriptsToLoad.map(loadScript))
    .then(() => {
        console.log("All scripts loaded");
        fetchApiData();
        loadDashboard();
    })
    .catch(err => console.error(err));
