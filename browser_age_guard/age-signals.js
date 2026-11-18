/* Local-only, deliberately conservative detection. No form values are inspected. */
(function(root) {
  "use strict";
  const indicators = [
    /\b(?:verify|confirm|prove)\s+(?:your\s+)?age\b/i,
    /\bage\s+(?:verification|check|assurance)\b/i,
    /\b(?:must\s+be|are\s+you)\s+(?:at\s+least\s+)?18\b/i,
    /\b(?:over|older\s+than)\s+18\b/i,
    /\b18\s*\+\s*(?:only|verification|restricted)\b/i,
    /\b(?:verifica(?:re)?\s+(?:dell['’]?\s*)?et[àa]|conferma(?:re)?\s+(?:la\s+)?(?:tua\s+)?et[àa]|dimostra(?:re)?\s+(?:la\s+)?maggiore\s+et[àa]|riservat[oa]\s+ai\s+maggiorenni|hai\s+(?:più\s+di\s+)?18\s+anni)(?=$|[\s.,;:!?])/i,
    /\b(?:v[eé]rification\s+de\s+l['’]âge|v[eé]rif(?:ier|iez)\s+(?:votre\s+)?âge|r[eé]serv[eé]\s+aux\s+majeurs|avez.vous\s+18\s+ans|plus\s+de\s+18\s+ans)\b/i,
    /\b(?:verificaci[oó]n\s+de\s+edad|verificar\s+(?:tu\s+)?edad|confirmar\s+(?:tu\s+)?edad|mayores\s+de\s+18\s+años|tienes\s+18\s+años)\b/i
  ];
  const pathMarker = /(?:^|\/)(?:age[-_]verification|verify[-_]age|age[-_]check|verifica[-_]eta|controle[-_]age|verificacion[-_]edad)(?:\/|$)/i;
  function isAgeContext(text, pathname) {
    const sample = String(text || "").slice(0, 15000);
    return pathMarker.test(String(pathname || "")) ||
      indicators.some(re => re.test(sample));
  }
  const api = Object.freeze({isAgeContext});
  root.AdsKillAgeSignals = api;
  if (typeof module !== "undefined" && module.exports) module.exports = api;
})(typeof globalThis !== "undefined" ? globalThis : this);
