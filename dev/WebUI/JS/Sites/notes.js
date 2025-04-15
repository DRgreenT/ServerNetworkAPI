let allNotes = [];

function loadNotes() {
    setUpdate("");
    const content = document.getElementById('content');
    content.innerHTML = `<h2>Notification Logs</h2><p>Loading...</p>`;

    if (!Array.isArray(noteData) || noteData.length === 0) {
        content.innerHTML = `<h2>Notification Logs</h2><p>No data available.</p>`;
        return;
    }

    let html = `
        <h2>Notification Logs</h2>
        <div id="notes-table-container">
            <table>
                <thead>
                    <tr>
                        <th>Time</th>
                        <th>Message</th>
                    </tr>
                </thead>
                <tbody>
    `;

    for (const note of noteData) {
        html += `
            <tr>
                <td>${note.timeStamp}</td>
                <td>${note.message}</td>
            </tr>
        `;
    }

    html += `
                </tbody>
            </table>
        </div>
    `;

    content.innerHTML = html;
}
