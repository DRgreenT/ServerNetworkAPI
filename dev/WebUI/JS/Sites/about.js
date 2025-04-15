function loadAbout() {
    const content = document.getElementById('content');
    setUpdate("");
    content.innerHTML = `
        <h2>About ServerNetworkAPI</h2>
        <div id="about-section">
            <p><strong>Version:</strong> ${getAppVersion()}</p>
            <p>This is a lightweight network monitoring tool built with .NET and designed for local Linux systems.</p>
            <ul>
                <li>Monitors active IPs</li>
                <li>Logs network activity</li>
                <li>Supports webhook notifications</li>
                <li>Provides Web API access and front-end UI</li>
            </ul>
            <p>Developed by <strong>Thomas Just</strong>, 2025</p>
            <p><a href="https://github.com/DRgreenT/ServerNetworkAPI" target="_blank">View on GitHub</a></p>
        </div>
    `;
}

function getAppVersion() {
    return version;
}
