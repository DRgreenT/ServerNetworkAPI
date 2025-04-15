async function loadDashboard() {
  setUpdate("dash");

  while (!isUpdateDashboardCancelled) {
    renderDashboard();
    await new Promise(resolve => setTimeout(resolve, 1000));
  }
}

function getInternetStatusText() {
  return uiData.isInternetAvailable ? 'Online' : 'Offline';
}

function getNmapStatusText() {
  return uiData.isNmapEnabled ? 'Active' : 'Inactive';
}

function color(isTrue) {
  return isTrue ? 'green' : 'red';
}

function renderDashboard() {
  const content = document.getElementById('content');

  content.innerHTML = `
    <div>
      <div id="dashboard" class="dashboard-grid">
        ${renderDeviceCard()}
        ${renderNewDeviceCard()}
        ${renderApiStatusCard()}
        ${renderScanStatusCard()}
      </div>
      <div class="dashboardChart">
        <div class="dashCartTitle">Scan history</div>
        <div class="dashChartInner">
          ${renderChart()}
        </div>
        <div class="dashboardFooter">
          <div class="dashCartLowerLeft">then</div>
          <div class="dashCartLowerRight">now</div>
        </div>
      </div>
      ${renderDeviceList()}
    </div>
  `;
}

function renderDeviceCard() {
  return `
    <div class="card">
      <div class="card-icon">🖧</div>
      <div class="card-title">Active devices</div>
      <div class="card-value" id="activeDevices">${uiData.activeDeviceCount}/${uiData.totalDeviceCount}</div>
    </div>`;
}

function renderNewDeviceCard() {
  return `
    <div class="card">
      <div class="card-icon">⚠️</div>
      <div class="card-title">New devices</div>
      <div class="card-value" id="newDevices">...</div>
      <a href="#" onclick="loadLogs()">Device list</a>
    </div>`;
}

function renderApiStatusCard() {
  return `
    <div class="card">
      <div class="card-icon">🖥️</div>
      <div class="card-title">API Status</div>
      <div class="card-badge" id="apiStatus">Checking...</div>
      <div class="card-subtext">Port: <span id="apiPort">5050</span></div>
      <div class="card-subtext">Letzter Scan: <span id="lastScan">${uiData.lastUpdateTime}</span></div>
      <div class="card-content" style="color:${color(uiData.isNmapEnabled)};">Nmap: ${getNmapStatusText()}</div>
    </div>`;
}

function renderScanStatusCard() {
  return `
    <div class="card">
      <div class="card-icon">🌐</div>
      <div class="card-title">Scanstatus</div>
      <div class="card-subtext">Uptime: <span id="apiUptime">${uiData.uptime}</span></div>
      <div class="card-subtext">Internet:<span id="internetStatus" style="color:${color(uiData.isInternetAvailable)};">${getInternetStatusText()}</span>
      </div>
    </div>`;
}

function renderDeviceList() {
  return `
    <div class="dash-deviceList-Grid">
      <div>
        ${getDeviceList()}
      </div>
    </div>
  `;
}

function getDeviceList() {
  if (devicesData.length === 0) {
    return `<p>No devices found</p>`;
  }

  return `
    <div>
      <table>
        <thead>
          <tr>
            <th>Status</th>
            <th>IP</th>
            <th>Hostname</th>
            <th>Last seen</th>
          </tr>
        </thead>
        <tbody>${generateTableRows(devicesData)}</tbody>
      </table>
    </div>
  `;
}

function generateTableRows(devices) {
  let rows = '';
  for (let i = 0; i < devices.length; i++) {
    rows += `
      <tr style="background-color: ${i % 2 === 0 ? '#d6d4d4' : '#ffffff'};">
        <td><span class="${devices[i].isOnline ? 'online' : 'offline'}"></span></td>
        <td>${devices[i].ip}</td>
        <td>${devices[i].hostname}</td>
        <td>${devices[i].lastSeen}</td>
      </tr>
    `;
  }
  return rows;
}

function renderChart() {
  let parentElement = document.getElementById('content');
  let parentWidth = parentElement.clientWidth;

  let totalHeight = 100;
  let html = '<div style="display: flex; flex-direction: row; justify-content: flex-start; align-items: flex-end; margin: 0; padding: 0;">';

  let start = 0;
  if (arrActiveDeviceCounts.length > parentWidth) {
    start = arrActiveDeviceCounts.length - 1300; 
  }

  for (let i = start; i < arrActiveDeviceCounts.length; i++) {
    let pixelHeight = (arrActiveDeviceCounts[i] / uiData.totalDeviceCount) * totalHeight;

    html += `
      <div id="chartPoint" style="width: 1px; height: ${pixelHeight}px; background-color: green; display: inline-block; vertical-align: bottom; margin-right: 1px; padding: 0;">
      </div>
    `;
  }

  html += '</div>';
  return html;
}
