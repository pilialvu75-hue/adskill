/* ADS Kill Privacy & Age (MVP), isolated content script.
 * Does not read input values, filenames, identity data, cookies or browsing history.
 * It neither verifies age nor bypasses website gates.
 */
(() => {
  "use strict";
  if (globalThis.__adsKillAgeGuardLoaded) return;
  globalThis.__adsKillAgeGuardLoaded = true;
  const detector = globalThis.AdsKillAgeSignals;
  if (!detector || typeof detector.isAgeContext !== "function") return;

  let enabled = true;
  let consentForTab = false;
  let panelHidden = false;
  let host = null;
  let panelStatus = null;
  let observer = null;
  let scanTimeout = null;
  let lastDetection = false;

  function ageEvidence(node) {
    const doc = document;
    const path = location.pathname; // Never save/send full URLs or query parameters.
    const title = (doc.title || "").slice(0, 250);
    const headings = Array.from(doc.querySelectorAll(
      "h1,h2,h3,[role='heading'],legend,[role='dialog']"
    )).slice(0, 35).map(el => (el.textContent || "").slice(0, 350)).join(" ");
    const local = node && typeof node.closest === "function"
      ? node.closest("form,dialog,[role='dialog'],section") : null;
    const localHeading = local
      ? (local.querySelector("h1,h2,h3,legend")?.textContent || "").slice(0, 350)
      : "";
    return detector.isAgeContext(title + " " + headings + " " + localHeading, path);
  }

  function isProtectedUpload(node) {
    return enabled && !consentForTab && (lastDetection || ageEvidence(node));
  }

  function blockMessage(message) {
    panelHidden = false;
    if (!host) showPanel();
    if (panelStatus) panelStatus.textContent = message;
  }

  function onFileChange(event) {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || input.type !== "file"
        || !input.files || input.files.length === 0 || !isProtectedUpload(input)) return;
    // Clearing before website's target/bubble change handlers is best-effort
    // protection, NOT a guaranteed network-level upload firewall.
    input.value = "";
    event.preventDefault();
    event.stopImmediatePropagation();
    blockMessage("Selezione annullata: nessun file è stato letto da ADS Kill.");
  }

  function onDrop(event) {
    if (!event.dataTransfer?.files?.length || !isProtectedUpload(event.target)) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    blockMessage("Rilascio di file impedito nel modulo di verifica dell’età.");
  }

  function onSubmit(event) {
    if (!isProtectedUpload(event.target)) return;
    const form = event.target;
    if (!(form instanceof HTMLFormElement)) return;
    const hasFiles = Array.from(form.querySelectorAll("input[type=file]"))
      .some(input => input.files && input.files.length);
    if (!hasFiles) return;
    event.preventDefault();
    event.stopImmediatePropagation();
    blockMessage("Invio file interrotto. Puoi scegliere un metodo di verifica meno invasivo.");
  }

  document.addEventListener("change", onFileChange, true);
  document.addEventListener("drop", onDrop, true);
  document.addEventListener("submit", onSubmit, true);

  const styles = `
    :host { all: initial; color-scheme: dark; }
    .panel{box-sizing:border-box;background:#092038;color:#effcff;border:2px solid #27cdd8;
      border-radius:15px;font:14px/1.45 system-ui,Segoe UI,sans-serif;
      box-shadow:0 12px 35px #0009;padding:14px;max-width:345px;width:calc(100vw - 24px)}
    .title{color:#66f8cf;font-size:16px;font-weight:750;margin-bottom:7px}
    .note{font-size:12px;color:#c9dce6;margin:8px 0}
    button{background:#35dec5;color:#041f29;border:0;border-radius:9px;
      font:600 12px system-ui,sans-serif;padding:9px 10px;margin:4px 5px 0 0;cursor:pointer}
    button.secondary{background:#24455b;color:#fff}
    .status{font-size:12px;min-height:20px;color:#ffe6a0;line-height:1.4}
  `;

  function showPanel() {
    if (!enabled || !document.body || panelHidden || host) return;
    host = document.createElement("div");
    host.setAttribute("data-adskill-age-panel", "");
    Object.assign(host.style, {
      position:"fixed", zIndex:"2147483647", right:"12px", bottom:"12px",
      maxWidth:"345px", pointerEvents:"auto"
    });
    const shadow = host.attachShadow({mode:"closed"});
    const style = document.createElement("style");
    style.textContent = styles;
    shadow.appendChild(style);
    const box = document.createElement("aside");
    box.className = "panel";
    box.setAttribute("aria-label","ADS Kill Privacy & Age");
    const heading = document.createElement("div");
    heading.className = "title";
    heading.textContent = "⬡ ADS Kill · Privacy & Age";
    box.appendChild(heading);
    const description = document.createElement("div");
    description.textContent = "Possibile verifica dell’età. Preferisci una prova «maggiorenne: sì» se il sito la supporta, anziché fornire documenti direttamente al sito.";
    box.appendChild(description);
    const note = document.createElement("div");
    note.className = "note";
    note.textContent = "Navigazione libera: ADS Kill non approva né aggira i controlli. Blocca in modo sperimentale file allegati a queste schermate, non webcam o richieste HTTPS.";
    box.appendChild(note);
    panelStatus = document.createElement("div");
    panelStatus.className = "status";
    panelStatus.setAttribute("role","status");
    panelStatus.textContent = consentForTab
      ? "Consenso file attivo soltanto per questa scheda."
      : "Protezione file attiva; non blocca il resto della pagina.";
    box.appendChild(panelStatus);
    const consent = document.createElement("button");
    consent.textContent = consentForTab ? "Proteggi nuovamente" : "Consenti file in questa scheda";
    consent.className = "secondary";
    consent.addEventListener("click", () => {
      if (!consentForTab) {
        const confirmed = window.confirm(
          "Consentire al sito l'uso di file per questa scheda? " +
          "ADS Kill non può garantire cosa invierà o conserverà il sito. " +
          "Non inviare documenti senza aver verificato il destinatario."
        );
        if (!confirmed) return;
      }
      consentForTab = !consentForTab;
      consent.textContent = consentForTab
        ? "Proteggi nuovamente" : "Consenti file in questa scheda";
      panelStatus.textContent = consentForTab
        ? "Consenso attivo. La verifica del sito rimane una tua scelta."
        : "Protezione file ripristinata.";
    });
    box.appendChild(consent);
    const hide = document.createElement("button");
    hide.className = "secondary";
    hide.textContent = "Nascondi avviso";
    hide.addEventListener("click", () => {
      panelHidden = true;
      host?.remove();
      host = null;
      panelStatus = null;
    });
    box.appendChild(hide);
    shadow.appendChild(box);
    document.body.appendChild(host);
  }

  function scan() {
    if (!enabled || !document.body) return;
    lastDetection = ageEvidence(null);
    if (lastDetection && !panelHidden) showPanel();
    else if (!lastDetection && host) {
      host.remove();
      host = null;
      panelStatus = null;
    }
  }

  function scheduleScan() {
    if (scanTimeout) clearTimeout(scanTimeout);
    scanTimeout = setTimeout(scan, 700);
  }

  observer = new MutationObserver(scheduleScan);
  observer.observe(document.documentElement || document, {
    childList: true, subtree: true
  });
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", scan, {once:true});
  } else scan();

  chrome.runtime.onMessage.addListener(message => {
    if (message?.action !== "adskill_stop_guard") return;
    enabled = false;
    observer?.disconnect();
    if (scanTimeout) clearTimeout(scanTimeout);
    host?.remove();
    host = null;
    document.removeEventListener("change", onFileChange, true);
    document.removeEventListener("drop", onDrop, true);
    document.removeEventListener("submit", onSubmit, true);
  });
})();
