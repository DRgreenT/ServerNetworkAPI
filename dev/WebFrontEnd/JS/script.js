var localHostIP = "192.168.178.10";
var localHostApiPort = "5050";
var ApiControllerName = "network"

async function fetchDevices() {
    try {
      const response = await fetch("http://" + localHostIP + ":" + localHostApiPort + "/" + ApiControllerName);
      if (!response.ok) throw new Error("Error...API or IP or not found. Make sure the right IP is set and API is runnig. Also the set port needs to be opened in the firewall.");

      const devices = await response.json();
      render(devices);
    } catch (err) {
      document.getElementById("results").innerHTML = `<p>Error: ${err}</p>`;
    }
  }

function render(devices) {
    if (devices.length === 0) {
      document.getElementById("results").innerHTML = "<p>No devices found</p>";
      return;
    }

    const rows = devices.map(d => `
      <tr>
        <td><span class="${d.isOnline ? 'online' : 'offline'}"></span></td>
        <td>${d.ip}</td>
        <td>${d.hostname}</td>
        <td>${d.os}</td>
        <td>
          ${d.ports && d.ports.length > 0 ? `
            <details>
              <summary>Ports (${d.ports.length})</summary>
              ${d.ports.map(p => `<div class="port-entry">${p.port}/${p.protocolType} (${p.service})</div>`).join('')}
            </details>` : "<i>-</i>"}
        </td>
      </tr>
    `).join("");

    const html = `
      <table>
        <thead>
          <tr>
            <th>Status</th>
            <th>IP</th>
            <th>Hostname</th>
            <th>OS</th>
            <th>Ports</th>
          </tr>
        </thead>
        <tbody>${rows}</tbody>
      </table>
    `;

    document.getElementById("results").innerHTML = html;
}

fetchDevices();
setInterval(fetchDevices, 15000);