"use strict";
const site = document.getElementById("site");
const status = document.getElementById("state");
const result = document.getElementById("result");
const enable = document.getElementById("enable");
const disable = document.getElementById("disable");

let selectedTab = null;
let originPattern = null;
let registrationId = null;

function scriptId(origin) {
  // Stable and scoped to this exact origin; no browsing history collected.
  let hash = 2166136261;
  for (const ch of origin) {
    hash ^= ch.charCodeAt(0);
    hash = Math.imul(hash, 16777619);
  }
  return "adskill_age_" + (hash >>> 0).toString(16);
}

async function inspect() {
  const [tab] = await chrome.tabs.query({active: true, currentWindow: true});
  const parsed = tab && tab.url ? new URL(tab.url) : null;
  if (!parsed || !["http:", "https:"].includes(parsed.protocol)
      || (parsed.port && !["80", "443"].includes(parsed.port))) {
    site.textContent = "Pagina non supportata";
    status.textContent = "Apri un normale sito HTTPS/HTTP in questa scheda.";
    return;
  }
  selectedTab = tab;
  originPattern = parsed.protocol + "//" + parsed.hostname + "/*";
  registrationId = scriptId(parsed.origin);
  site.textContent = parsed.hostname;
  const registered = await chrome.scripting.getRegisteredContentScripts({ids:[registrationId]});
  const active = registered.some(r => r.id === registrationId);
  status.textContent = active
    ? "Protezione attiva per questo sito. Nessuna pagina bloccata."
    : "Protezione disattivata. L'accesso ai siti resta libero.";
  enable.disabled = active;
  disable.disabled = !active;
}

enable.addEventListener("click", async () => {
  if (!originPattern || !selectedTab || !registrationId) return;
  result.textContent = "";
  try {
    // Must be called directly from this user click.
    const granted = await chrome.permissions.request({origins:[originPattern]});
    if (!granted) {
      result.textContent = "Autorizzazione negata: nessuna modifica.";
      return;
    }
    await chrome.scripting.registerContentScripts([{
      id: registrationId,
      matches: [originPattern],
      js: ["age-signals.js", "content.js"],
      runAt: "document_start",
      allFrames: false,
      persistAcrossSessions: true
    }]);
    try {
      await chrome.scripting.executeScript({
        target: {tabId: selectedTab.id},
        files: ["age-signals.js", "content.js"]
      });
    } catch (e) {
      // Existing pages can be protected on the next reload.
      result.textContent = "Attivato. Ricarica la pagina per applicarlo.";
    }
    if (!result.textContent) result.textContent = "Attivato: rilevamento locale.";
    await inspect();
  } catch (error) {
    result.textContent = "Attivazione non riuscita: " + String(error.message || error);
  }
});

disable.addEventListener("click", async () => {
  if (!originPattern || !selectedTab || !registrationId) return;
  result.textContent = "";
  try {
    await chrome.scripting.unregisterContentScripts({ids:[registrationId]});
    try {
      await chrome.tabs.sendMessage(selectedTab.id, {action:"adskill_stop_guard"});
    } catch (ignored) {}
    await chrome.permissions.remove({origins:[originPattern]});
    result.textContent = "Disattivato. Ricarica se il sito mostra ancora il pannello.";
    await inspect();
  } catch (error) {
    result.textContent = "Disattivazione non riuscita: " + String(error.message || error);
  }
});

inspect().catch(error => {
  status.textContent = "Errore di inizializzazione.";
  result.textContent = String(error.message || error);
});
