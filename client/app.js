const API_BASE = 'http://localhost:5000';
const statusPill = document.getElementById('status-pill');
const statusOutput = document.getElementById('status-output');
const refreshButton = document.getElementById('refresh-btn');

async function checkBackend() {
  statusPill.textContent = 'checking';
  statusOutput.textContent = 'Đang gọi backend...';

  try {
    const response = await fetch(`${API_BASE}/health`);

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }

    const payload = await response.json();
    statusPill.textContent = 'online';
    statusOutput.textContent = JSON.stringify(payload, null, 2);
  } catch (error) {
    statusPill.textContent = 'offline';
    statusOutput.textContent = `Không gọi được backend tại ${API_BASE}/health\n${error.message}`;
  }
}

refreshButton.addEventListener('click', checkBackend);
checkBackend();
