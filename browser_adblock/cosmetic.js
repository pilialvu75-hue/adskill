/* Cosmetic ad hiding. Runs locally: no collection of URLs or user data.
 * Rules are intentionally narrow to avoid hiding video players or age gates.
 */
(() => {
  "use strict";
  const styleId = "adskill-browser-shield-style";
  const css = [
    "ins.adsbygoogle",
    "[data-ad-client]",
    "[data-ad-slot]",
    "[id^='div-gpt-ad-']",
    "[id^='google_ads_iframe_']",
    "iframe[src*='doubleclick.net']",
    "iframe[src*='googlesyndication.com']",
    "iframe[src*='exoclick.com']",
    "iframe[src*='exosrv.com']",
    "iframe[src*='juicyads.com']",
    "iframe[src*='trafficjunky.net']",
    "iframe[src*='popads.net']",
    ".banner-ad",
    ".ad-banner",
    ".ad-slot",
    ".ad-unit",
    ".ad-container",
    ".ad-wrapper",
    ".advertisement",
    "[class~='adsbygoogle']",
    "[data-testid='ad-slot']",
    "[aria-label='Advertisement']",
    "[aria-label='Pubblicità']",
    "[aria-label='Publicidad']"
  ].join(",\n") + "{display:none !important;visibility:hidden !important;}";

  function apply(on) {
    let node = document.getElementById(styleId);
    if (!on) { node?.remove(); return; }
    if (node) return;
    if (!document.documentElement) return;
    node = document.createElement("style");
    node.id = styleId;
    node.textContent = css;
    (document.head || document.documentElement).appendChild(node);
  }

  let on = true;
  chrome.storage.local.get({enabled:true}).then(data => {
    on = data.enabled !== false;
    apply(on);
  }).catch(() => apply(false));
  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => apply(on), {once:true});
  }
  chrome.storage.onChanged.addListener((changes, area) => {
    if (area !== "local" || !changes.enabled) return;
    on = changes.enabled.newValue !== false;
    apply(on);
  });
})();
