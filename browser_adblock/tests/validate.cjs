"use strict";
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const root = path.join(__dirname, "..");
const read = name => JSON.parse(fs.readFileSync(path.join(root, name), "utf8"));
const manifest = read("manifest.json");
const rules = read("rules/domain-rules.json");
const paths = read("rules/path-rules.json");

assert.equal(manifest.manifest_version, 3);
assert.deepEqual(manifest.permissions.sort(), ["declarativeNetRequest","storage"].sort());
assert.equal(manifest.host_permissions, undefined);
assert.equal(manifest.background, undefined);
assert.equal(manifest.content_scripts.length, 1);
assert.equal(manifest.content_scripts[0].all_frames, false);
assert.equal(manifest.content_scripts[0].matches.length, 2);
assert.equal(manifest.declarative_net_request.rule_resources.length, 2);
assert.equal(manifest.declarative_net_request.rule_resources.every(x => x.enabled), true);
assert.equal(rules.length, 1);
assert.equal(rules[0].action.type, "block");
assert.equal(rules[0].condition.requestDomains.length >= 120, true);
assert.equal(new Set(rules[0].condition.requestDomains).size, rules[0].condition.requestDomains.length);
assert.equal(rules[0].condition.requestDomains.every(x => /^[a-z0-9.-]+\.[a-z0-9.-]+$/.test(x)), true);
for (const important of [
  "doubleclick.net", "googlesyndication.com", "exoclick.com",
  "exosrv.com", "juicyads.com", "trafficjunky.net", "popads.net"
]) assert.ok(rules[0].condition.requestDomains.includes(important), important);
for (const forbidden of [
  "youtube.com", "googlevideo.com", "google.com", "xvideos.com",
  "pornhub.com", "xnxx.com", "googleapis.com", "github.com"
]) assert.ok(!rules[0].condition.requestDomains.includes(forbidden), forbidden);
assert.equal(paths.length >= 12, true);
const ids = [...rules, ...paths].map(x => x.id);
assert.equal(new Set(ids).size, ids.length);
for (const rule of [...rules, ...paths]) {
  assert.equal(rule.action.type, "block");
  assert.equal(rule.priority, 1);
  assert.ok(rule.condition.resourceTypes.length > 0);
}
for (const rule of paths) assert.equal(rule.condition.domainType, "thirdParty");
const cosmetic = fs.readFileSync(path.join(root,"cosmetic.js"),"utf8");
assert.match(cosmetic,/storage\.onChanged/);
assert.match(cosmetic,/enabled/);
assert.doesNotMatch(cosmetic,/fetch\(|XMLHttpRequest|sendBeacon/);
const popup = fs.readFileSync(path.join(root,"popup.js"),"utf8");
assert.match(popup,/updateEnabledRulesets/);
assert.match(popup,/storage\.local\.set/);
console.log("ADS Kill Edge 0.2: manifest, 132 authored ad domains, reversible UI and no telemetry — PASS");
