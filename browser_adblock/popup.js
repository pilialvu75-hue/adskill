"use strict";
const idList = ["ads_domains", "ads_paths"];
const button = document.getElementById("switch");
const status = document.getElementById("status");
const message = document.getElementById("message");

async function refresh() {
  try {
    const enabled = await chrome.declarativeNetRequest.getEnabledRulesets();
    const on = idList.every(id => enabled.includes(id));
    status.textContent = on ? "● Protezione attiva" : "○ Protezione disattivata";
    status.className = "status " + (on ? "on" : "off");
    button.textContent = on ? "Disattiva protezione" : "Attiva protezione";
    button.disabled = false;
    button.dataset.current = on ? "on" : "off";
    return on;
  } catch (error) {
    status.textContent = "Impossibile leggere lo stato";
    message.textContent = String(error.message || error);
    button.disabled = true;
    return null;
  }
}

button.addEventListener("click", async () => {
  const currentlyOn = button.dataset.current === "on";
  button.disabled = true;
  message.textContent = "";
  try {
    await chrome.declarativeNetRequest.updateEnabledRulesets({
      enableRulesetIds: currentlyOn ? [] : idList,
      disableRulesetIds: currentlyOn ? idList : []
    });
    // Cosmetic hiding is independently reversible and follows the same state.
    await chrome.storage.local.set({enabled: !currentlyOn});
    message.textContent = "Operazione riuscita. Ricarica la pagina per confrontare.";
  } catch (error) {
    // Report only; never claim success after partial rule-toggle failure.
    message.textContent = "Modifica non riuscita: " + String(error.message || error);
  } finally {
    await refresh();
  }
});

refresh();
