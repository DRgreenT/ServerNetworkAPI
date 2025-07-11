﻿async function loadDashboard() {
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
    <div style="display: flex; flex-direction: column; align-items: center;">
      <div id="dashboard" class="dashboard-grid">
        ${renderDeviceCard()}
        ${renderNewDeviceCard()}
        ${renderApiStatusCard()}
        ${renderServerStatusCard()}
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
      <div class="systemCard">
        <div>
        
        <div class="card-subtextL"">Letzter Scan:</span></div>
        <div class="card-subtextL"">API uptime:</div>
        <div class="card-subtextL" >Nmap:</div>
        </div>
        <div>
        <div class="card-subtextR"><span id="lastScan">${uiData.lastUpdateTime}</span></div>
        <div class="card-subtextR"><span id="ApiUptime">${uiData.uptime}</span></div>
        <div class="card-subtextR" style="color:${color(uiData.isNmapEnabled)};">${getNmapStatusText()}</div>
        </div>
      </div>
    </div>`;
}

function renderServerStatusCard() {
  return `
    <div class="card">
      <div class="card-icon">🌐</div>
        <div class="card-title">Server status</div>
        <div class="systemCard">
          <div >
            <div class="card-subtextL">Uptime:</div>
            <div class="card-subtextL">CPU:</div>
            <div class="card-subtextL">Memory:</div>
            <div class="card-subtextL">Internet:</div>
          </div>
          <div>
            <div class="card-subtextR"><span id="ServerUptime">${systemData.uptime}</span></div>
            <div class="card-subtextR"><span id="CpuUsage">${systemData.cpuUsage}%</span></div>
            <div class="card-subtextR"><span id="MemUsage">${systemData.memoryUsage}</span></div>
            <div class="card-subtextR"><span id="internetStatus" style="color:${color(uiData.isInternetAvailable)};">${getInternetStatusText()}</span></div>
          </div>
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
            <th class="center-cell">Status</th>
            <th style="padding:5px">IP</th>
            <th style="padding:5px">Hostname</th>
            <th style="padding:5px">Last seen</th>
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
        <td class="center-cell"><span class="${devices[i].isOnline ? 'online' : 'offline'}"></span></td>
        <td>${devices[i].ip}</td>
        <td style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 150px;">${devices[i].hostname}</td>
        <td style="max-width: 60px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; direction: rtl; text-align: left;">${devices[i].lastSeen}</td>
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
