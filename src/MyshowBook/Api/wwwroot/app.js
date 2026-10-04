const tokenKey = "myshowbook_token";
const roleKey = "myshowbook_role";
let selectedShowId = null;

const $ = id => document.getElementById(id);
const token = () => sessionStorage.getItem(tokenKey);
const headers = () => token()
  ? { Authorization: "Bearer " + token(), "Content-Type": "application/json" }
  : { "Content-Type": "application/json" };

function escapeHtml(value) {
  return String(value).replace(/[&<>"']/g, char => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
  }[char]));
}

function setMessage(value) {
  $("message").textContent = value || "";
}

async function api(path, options = {}) {
  const response = await fetch(path, {
    ...options,
    headers: { ...headers(), ...(options.headers || {}) }
  });
  const text = await response.text();
  let body = null;
  try { body = text ? JSON.parse(text) : null; } catch { body = text; }
  if (!response.ok) {
    const message = body?.error || body?.title || response.statusText;
    throw new Error(message);
  }
  return body;
}

function showDashboard() {
  const loggedIn = !!token();
  $("auth").classList.toggle("hidden", loggedIn);
  $("dashboard").classList.toggle("hidden", !loggedIn);
  $("logout").classList.toggle("hidden", !loggedIn);
  $("adminPanel").classList.toggle("hidden", sessionStorage.getItem(roleKey) !== "admin");
  if (loggedIn) loadShows();
}

$("loginForm").addEventListener("submit", async event => {
  event.preventDefault();
  try {
    const result = await api("/auth/token", {
      method: "POST",
      body: JSON.stringify({
        username: $("loginUsername").value,
        password: $("loginPassword").value
      })
    });
    sessionStorage.setItem(tokenKey, result.access_token);
    sessionStorage.setItem(roleKey, result.role);
    setMessage("Logged in as " + result.role);
    showDashboard();
  } catch (error) {
    setMessage(error.message);
  }
});

$("registerForm").addEventListener("submit", async event => {
  event.preventDefault();
  try {
    await api("/auth/register", {
      method: "POST",
      body: JSON.stringify({
        username: $("registerUsername").value,
        password: $("registerPassword").value,
        role: $("registerRole").value
      })
    });
    $("loginUsername").value = $("registerUsername").value;
    $("loginPassword").value = $("registerPassword").value;
    setMessage("Registered. You can now log in.");
  } catch (error) {
    setMessage(error.message);
  }
});

$("logout").addEventListener("click", () => {
  sessionStorage.removeItem(tokenKey);
  sessionStorage.removeItem(roleKey);
  selectedShowId = null;
  $("showDetails").classList.add("hidden");
  showDashboard();
  setMessage("Logged out.");
});

async function loadShows() {
  try {
    const shows = await api("/shows");
    $("shows").innerHTML = shows.length
      ? shows.map(show => `
        <div class="show">
          <div>
            <strong>${escapeHtml(show.name)}</strong>
            <div>₹${(show.price_paise / 100).toFixed(2)} · ${show.available_seats}/${show.total_seats} available</div>
          </div>
          <button onclick="loadShow('${escapeHtml(show.show_guid)}')">View</button>
        </div>`).join("")
      : "<p>No shows yet.</p>";
  } catch (error) {
    setMessage(error.message);
  }
}

async function loadShow(id) {
  try {
    const show = await api("/shows/" + encodeURIComponent(id));
    selectedShowId = id;
    $("detailTitle").textContent = show.name;
    $("detailSummary").textContent =
      "₹" + (show.price_paise / 100).toFixed(2) +
      " · Available " + show.available_seats +
      " · Held " + show.held_seats +
      " · Confirmed " + show.confirmed_seats;
    $("seatList").innerHTML = show.seats.map(seat =>
      `<span class="seat ${escapeHtml(seat.status)}">${escapeHtml(seat.seat_number)}: ${escapeHtml(seat.status)}</span>`).join("");
    $("bookingSeats").value = "";
    $("idempotencyKey").value = crypto.randomUUID();
    $("bookingResult").textContent = "";
    $("showDetails").classList.remove("hidden");
  } catch (error) {
    setMessage(error.message);
  }
}

$("bookingForm").addEventListener("submit", async event => {
  event.preventDefault();
  if (!selectedShowId) return;
  try {
    const seats = $("bookingSeats").value.split(",").map(x => x.trim()).filter(Boolean);
    const result = await api("/shows/" + encodeURIComponent(selectedShowId) + "/reserve", {
      method: "POST",
      body: JSON.stringify({ seats, idempotency_key: $("idempotencyKey").value })
    });
    $("bookingResult").textContent = JSON.stringify(result, null, 2);
    await loadShow(selectedShowId);
    await loadShows();
    setMessage("Reservation successful.");
  } catch (error) {
    $("bookingResult").textContent = error.message;
    setMessage("Booking failed.");
  }
});

$("refreshShows").addEventListener("click", loadShows);

$("viewLogs").addEventListener("click", async () => {
  try {
    const response = await fetch("/logs", { headers: { Authorization: "Bearer " + token() } });
    if (!response.ok) throw new Error(await response.text());
    $("logs").textContent = await response.text();
    $("logs").classList.remove("hidden");
  } catch (error) {
    setMessage(error.message);
  }
});

showDashboard();