const messageTypeMap = {
    0: { name: "Exception", color: "red" },
    1: { name: "Error", color: "red" },
    2: { name: "HardWarning", color: "orange" },
    3: { name: "Warning", color: "goldenrod" },
    4: { name: "Standard", color: "black" },
    5: { name: "Success", color: "green" }
};

function getMessageTypeInfo(type) {
    return messageTypeMap[type] || { name: "Standard", color: "black" };
}

async function loadLogs() {
    setUpdate("log");
    const content = document.getElementById('content');

    if (!document.getElementById('log-filter')) {
        renderLogFilterUI(content);
    }

    while (!isUpdateLogCancelled) {
        applyLogFilter();
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
}

function renderLogFilterUI(content) {
    content.innerHTML = `
        <div class="logContainer">
        <h2>System Logs</h2>
        <div id="filters">
            <label>Filter Timestamp: <input type="text" id="timestampFilter" placeholder="e.g. 09:13"></label>
            <label>Message Type: 
                <select id="messageTypeFilter">
                    <option value="">All</option>
                    <option value="0">Exception</option>
                    <option value="1">Error</option>
                    <option value="2">HardWarning</option>
                    <option value="3">Warning</option>
                    <option value="4">Standard</option>
                    <option value="5">Success</option>
                </select>
            </label>
        </div>
        <div id="log-table-container"></div>
    `;

    document.getElementById('timestampFilter').addEventListener('input', applyLogFilter);
    document.getElementById('messageTypeFilter').addEventListener('change', applyLogFilter);
}

function applyLogFilter() {
    const tsFilter = document.getElementById('timestampFilter')?.value.toLowerCase() || '';
    const typeFilter = document.getElementById('messageTypeFilter')?.value;

    const filtered = logData.filter(log => {
        const matchTs = log.timeStamp.toLowerCase().includes(tsFilter);
        const matchType = typeFilter === '' || log.messageType == typeFilter;
        return matchTs && matchType;
    });

    renderLogTable(filtered);
}

function renderLogTable(logs) {
    const container = document.getElementById("log-table-container");
    if (!container) return;

    let html = `
        <table>
            <thead>
                <tr>
                    <th>Time</th>
                    <th>Source</th>
                    <th>Message</th>
                    <th>Type</th>
                </tr>
            </thead>
            <tbody>
    `;

    for (const log of logs) {
        const logType = getMessageTypeInfo(log.messageType);
        html += `
            <tr>
                <td>${log.timeStamp}</td>
                <td>${log.source}</td>
                <td>${log.message}</td>
                <td style="color: ${logType.color}; font-weight: bold;">${logType.name}</td>
            </tr>
        `;
    }

    html += '</tbody></table></div>';
    container.innerHTML = html;
}
