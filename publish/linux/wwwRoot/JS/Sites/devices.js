function loadDetailDevicePage()
{
    setUpdate("device")
    DetailDeviceList()
}

function DetailDeviceList()
{
    const content = document.getElementById('content');
    content.innerHTML = `
    <div class="dash-deviceList-Grid">
      <div class="logContainer">
        <h2>Detailed device list</h2>
        ${getDetailDeviceList()}
      </div>
    </div>
    `

}

function getDetailDeviceList() {
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
              <th style="padding:5px">MAC address</th>
              <th style="padding:5px">Hostname</th>
              <th style="padding:5px">Last seen</th>
              <th style="padding:5px">OS</th>
              <th style="padding:5px">Ports</th>
            </tr>
          </thead>
          <tbody>${generateDetailTableRows(devicesData)}</tbody>
        </table>
      </div>
    `;
  }
  
  function generateDetailTableRows(devices) {
    let rows = '';
    for (let i = 0; i < devices.length; i++) {
        let ports = '<div style="display: flex; flex-direction: column; gap: 1px; width: 7rem; font-size: 0.85em">';
        for (let j = 0; j < devices[i].ports.length; j++) {
          let object = devices[i].ports[j];
          ports += `
            <div style="display: flex; justify-content: space-between;">
              <div style="text-align: left; width: 1rem;">${object.protocolType}</div>
              <div style="text-align: right; width: 2rem;">${object.port}</div>
              <div style="text-align: left; width: 2rem;">${object.service}</div>
            </div>`;
        }
        ports += '</div>';
      rows += `
        <tr style="background-color: ${i % 2 === 0 ? '#d6d4d4' : '#ffffff'};">
          <td class="center-cell"><span class="${devices[i].isOnline ? 'online' : 'offline'}"></span></td>
          <td>${devices[i].ip}</td>
          <td>${devices[i].macAddress}</td>
          <td style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 150px;">${devices[i].hostname}</td>
          <td style="max-width: 60px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; text-align: left;">${devices[i].lastSeen}</td>
          <td style="max-width: 60px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; text-align: left;">${devices[i].os}</td>
          <td style="max-width: 60px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">${ports}</td>
        </tr>
      `;
    }
    return rows;
  }
