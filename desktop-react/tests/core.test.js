import test from "node:test";
import assert from "node:assert/strict";
import JSZip from "jszip";
import * as c from "../src/core.js";
const complexXml = `<?xml version="1.0" encoding="UTF-8"?>
<package version="2.2" id="{TEST}" target="Five"><metadata><name>Roundtrip &amp; test</name><author><displayName>A</displayName></author><description><![CDATA[<b>Keep this</b>]]></description></metadata><colors><headerBackground useBlackTextColor="False">0xFF445566</headerBackground></colors><!-- keep me --><content><archive path="update\\update.rpf" type="RPF7" createIfNotExist="False"><archive path="x64\\levels\\test.rpf" type="RPF7"><add source="a/model.bin">models\\test.bin</add><delete>old.bin</delete></archive><xml path="common\\data\\dlclist.xml"><add append="Last" xpath="/SMandatoryPacksData/Paths"><Item>dlcpacks:/test/</Item></add><remove xpath="/Root/Old"/></xml></archive><custom unknown="preserved"><child>content</child></custom></content></package>`;
const fixture = () => ({
  xml: complexXml,
  files: {
    "content/a/model.bin": new Uint8Array([0, 255, 128, 7, 42]),
    "icon.png": new Uint8Array([137, 80, 78, 71]),
    "readme.txt": c.encodeText("unreferenced root asset"),
  },
  name: "Roundtrip",
});
test("OIV import → metadata edit → project save/reopen → OIV rebuild preserves instructions and every binary", async () => {
  const original = fixture();
  const opened = await c.openArchive(await c.saveArchive(original));
  assert.equal(opened.xml, complexXml);
  assert.deepEqual(opened.files, original.files);
  opened.xml = c.setMetadata(opened.xml, "name", "Changed & improved");
  const reopened = await c.openArchive(await c.saveArchive(opened, true));
  assert.deepEqual(reopened.files, original.files);
  assert.equal(c.metadata(reopened.xml).name, "Changed & improved");
  const built = await c.openArchive(await c.saveArchive(reopened));
  assert.deepEqual(built.files, original.files);
  for (const fragment of [
    "<!-- keep me -->",
    "<![CDATA[<b>Keep this</b>]]>",
    '<custom unknown="preserved">',
    '<remove xpath="/Root/Old"/>',
    "<delete>old.bin</delete>",
  ])
    assert.ok(built.xml.includes(fragment), fragment);
  const zip = await JSZip.loadAsync(await c.saveArchive(reopened));
  assert.equal(zip.file(c.PROJECT_MARKER), null);
});
test("missing payload blocks OIV build but can be saved as an incomplete project", async () => {
  const p = fixture();
  delete p.files["content/a/model.bin"];
  assert.match(c.validation(p).errors[0], /Missing payload/);
  await assert.rejects(c.saveArchive(p), /Missing payload/);
  await c.saveArchive(p, true);
});
test("renaming payload updates source attributes without changing targets", () => {
  const p = c.renameFile(
    fixture(),
    "content/a/model.bin",
    "content/b/renamed.bin",
  );
  assert.ok(p.xml.includes('source="b/renamed.bin"'));
  assert.ok(p.xml.includes("models\\test.bin"));
  assert.ok(!p.files["content/a/model.bin"]);
  assert.deepEqual(
    p.files["content/b/renamed.bin"],
    fixture().files["content/a/model.bin"],
  );
});
test("all element attributes and leaf text can be edited; order and nesting remain editable", () => {
  let xml = c.newProject().xml;
  const doc = c.parseXml(xml),
    index = c
      .elements(doc.documentElement)
      .findIndex((n) => n.tagName === "content");
  xml = c.addNode(xml, [index], "archive", {
    path: "update\\update.rpf",
    type: "RPF7",
  });
  xml = c.addNode(xml, [index, 0], "delete", {}, "old.bin");
  xml = c.addNode(xml, [index, 0], "delete", {}, "second.bin");
  xml = c.moveNode(xml, [index, 0, 1], -1);
  assert.equal(
    c.nodeAt(c.parseXml(xml), [index, 0, 0]).textContent,
    "second.bin",
  );
  xml = c.updateNode(xml, [index, 0, 0], {
    tag: "add",
    attributes: { source: "file.bin" },
    text: "new.bin",
  });
  assert.equal(
    c.nodeAt(c.parseXml(xml), [index, 0, 0]).getAttribute("source"),
    "file.bin",
  );
  xml = c.removeNode(xml, [index, 0, 1]);
  assert.equal(c.elements(c.nodeAt(c.parseXml(xml), [index, 0])).length, 1);
  assert.throws(() => c.removeNode(xml, []), /root/);
});
test("malformed XML and DTD/entity declarations are rejected", () => {
  for (const xml of [
    "<package><content></package>",
    "<wrong/>",
    '<!DOCTYPE package [<!ENTITY x "test">]><package/>',
    "<package/><package/>",
  ])
    assert.throws(() => c.parseXml(xml));
});
test("unsafe paths and case-insensitive collisions are rejected", async () => {
  for (const path of [
    "../bad",
    "/bad",
    "C:/bad",
    "content/../bad",
    "content//bad",
  ])
    assert.throws(() => c.safePath(path));
  const p = fixture();
  p.files["content/A/model.bin"] = new Uint8Array();
  assert.match(c.validation(p).errors.join(), /Duplicate filename/);
  const z = new JSZip();
  z.file("assembly.xml", complexXml);
  z.file("../evil", "bad");
  await assert.rejects(
    c.openArchive(await z.generateAsync({ type: "uint8array" })),
    /unsafe/,
  );
});
test("unknown Studio manifest versions are rejected", async () => {
  const z = new JSZip();
  z.file("assembly.xml", complexXml);
  z.file(
    c.PROJECT_MARKER,
    JSON.stringify({ format: "magicogk-oiv-studio", version: 99 }),
  );
  await assert.rejects(
    c.openArchive(await z.generateAsync({ type: "uint8array" })),
    /version/,
  );
});
