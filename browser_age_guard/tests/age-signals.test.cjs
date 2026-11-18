"use strict";
const test = require("node:test");
const assert = require("node:assert/strict");
const {isAgeContext} = require("../age-signals.js");

const positives = [
  "Please verify your age to continue",
  "Age verification required",
  "You must be 18 to continue",
  "Only for users over 18",
  "Verifica dell'età necessaria",
  "Conferma la tua età",
  "Riservato ai maggiorenni",
  "Hai più di 18 anni?",
  "Vérification de l'âge requise",
  "Vérifiez votre âge pour continuer",
  "Verificación de edad",
  "Confirmar tu edad",
];
const negatives = [
  "Guarda il tuo video preferito",
  "Age of Empires III",
  "Le notizie di oggi",
  "Google Maps",
  "Termini di servizio e privacy",
  "Car Jam",
  "Duolingo",
  "YouTube - video"
];
test("detect real verification prompts in multiple languages", () => {
  for (const value of positives) {
    assert.equal(isAgeContext(value, "/"), true, value);
  }
});
test("ignore ordinary app pages to reduce false positives", () => {
  for (const value of negatives) {
    assert.equal(isAgeContext(value, "/"), false, value);
  }
});
test("recognize explicit age verification routes", () => {
  assert.equal(isAgeContext("", "/age-verification"), true);
  assert.equal(isAgeContext("", "/account/verify-age/"), true);
  assert.equal(isAgeContext("", "/shop"), false);
});
test("do not treat bare age or numbers as proof of a gate", () => {
  assert.equal(isAgeContext("18", "/"), false);
  assert.equal(isAgeContext("age", "/"), false);
  assert.equal(isAgeContext("1998", "/"), false);
});
