import JSZip from "jszip";
import { DOMParser, XMLSerializer } from "@xmldom/xmldom";
const encoder = new TextEncoder(),
  decoder = new TextDecoder("utf-8", { fatal: true });
export const PROJECT_MARKER = "__oiv_studio_project.json";
export const MAX_BYTES = 1024 * 1024 * 1024;
export function safePath(path) {
  const normalized = path.replaceAll("\\", "/");
  if (
    !normalized ||
    normalized.startsWith("/") ||
    /^[a-z]:/i.test(normalized) ||
    /[\x00-\x1f]/.test(normalized) ||
    normalized.split("/").some((p) => !p || p === "." || p === "..")
  )
    throw new Error(`Invalid package path: ${path}`);
  return normalized;
}
export function parseXml(xml) {
  if (typeof xml !== "string" || /<!DOCTYPE|<!ENTITY/i.test(xml))
    throw new Error("DTD and entity declarations are not supported.");
  const errors = [];
  const doc = new DOMParser({
    errorHandler: {
      warning: (m) => errors.push(m),
      error: (m) => errors.push(m),
      fatalError: (m) => errors.push(m),
    },
  }).parseFromString(xml, "text/xml");
  if (errors.length || doc.documentElement?.tagName !== "package")
    throw new Error(
      "assembly.xml must be valid XML with a <package> root. " +
        (errors[0] || ""),
    );
  if (Array.from(doc.childNodes).filter((n) => n.nodeType === 1).length !== 1)
    throw new Error("XML must have exactly one root element.");
  return doc;
}
export const serializeXml = (doc) => new XMLSerializer().serializeToString(doc);
export const elements = (node) =>
  Array.from(node.childNodes).filter((n) => n.nodeType === 1);
export const child = (node, tag) =>
  elements(node).find((n) => n.tagName === tag);
export function ensureChild(doc, node, tag) {
  let value = child(node, tag);
  if (!value) {
    value = doc.createElement(tag);
    node.appendChild(value);
  }
  return value;
}
export function newProject() {
  const id = globalThis.crypto.randomUUID().toUpperCase();
  return {
    xml: `<?xml version="1.0" encoding="UTF-8"?>\n<package version="2.2" id="{${id}}" target="Five">\n  <metadata>\n    <name>Untitled mod</name>\n    <version><major>1</major><minor>0</minor><tag>STABLE</tag></version>\n    <author><displayName></displayName><webPage></webPage></author>\n    <description><![CDATA[]]></description>\n  </metadata>\n  <colors><headerBackground useBlackTextColor="False">0xFF4058D6</headerBackground><iconBackground>0xFF171E33</iconBackground></colors>\n  <content></content>\n</package>`,
    files: {},
    name: "Untitled mod",
  };
}
export function metadata(xml) {
  const doc = parseXml(xml),
    meta = child(doc.documentElement, "metadata");
  const value = (...tags) => {
    let n = meta;
    for (const tag of tags) n = n && child(n, tag);
    return n?.textContent || "";
  };
  return {
    name: value("name"),
    author: value("author", "displayName"),
    website: value("author", "webPage"),
    major: value("version", "major"),
    minor: value("version", "minor"),
    tag: value("version", "tag"),
    description: value("description"),
  };
}
export function setMetadata(xml, field, value) {
  const tags = {
    name: ["name"],
    author: ["author", "displayName"],
    website: ["author", "webPage"],
    major: ["version", "major"],
    minor: ["version", "minor"],
    tag: ["version", "tag"],
    description: ["description"],
  }[field];
  if (!tags) throw new Error("Unknown metadata field");
  const doc = parseXml(xml);
  let node = ensureChild(doc, doc.documentElement, "metadata");
  for (const tag of tags) node = ensureChild(doc, node, tag);
  node.textContent = value;
  return serializeXml(doc);
}
export function nodeAt(doc, indexes) {
  return indexes.reduce((n, i) => n && elements(n)[i], doc.documentElement);
}
export function updateNode(xml, indexes, { tag, attributes, text }) {
  const doc = parseXml(xml);
  let node = nodeAt(doc, indexes);
  if (!node) throw new Error("Select an instruction first.");
  if (tag !== undefined && tag !== node.tagName) {
    if (!/^[A-Za-z_][\w.:-]*$/.test(tag))
      throw new Error("Enter a valid XML element name.");
    const replacement = doc.createElement(tag);
    for (const attr of Array.from(node.attributes))
      replacement.setAttribute(attr.name, attr.value);
    while (node.firstChild) replacement.appendChild(node.firstChild);
    node.parentNode.replaceChild(replacement, node);
    node = replacement;
  }
  if (attributes !== undefined) {
    for (const key of Object.keys(attributes))
      if (!/^[A-Za-z_][\w.:-]*$/.test(key))
        throw new Error(`Invalid attribute: ${key}`);
    for (const attr of Array.from(node.attributes))
      node.removeAttribute(attr.name);
    for (const [key, value] of Object.entries(attributes))
      node.setAttribute(key, value);
  }
  if (text !== undefined) {
    if (elements(node).length)
      throw new Error(
        "Edit child elements or use the XML editor for mixed content.",
      );
    node.textContent = text;
  }
  const result = serializeXml(doc);
  parseXml(result);
  return result;
}
export function addNode(xml, indexes, tag, attributes = {}, text = "") {
  const doc = parseXml(xml),
    parent = nodeAt(doc, indexes);
  if (!parent) throw new Error("Select a parent element.");
  if (!/^[A-Za-z_][\w.:-]*$/.test(tag))
    throw new Error("Invalid element name.");
  const node = doc.createElement(tag);
  for (const [key, value] of Object.entries(attributes))
    node.setAttribute(key, value);
  if (text) node.appendChild(doc.createTextNode(text));
  parent.appendChild(node);
  const result = serializeXml(doc);
  parseXml(result);
  return result;
}
export function removeNode(xml, indexes) {
  if (!indexes.length) throw new Error("The package root cannot be removed.");
  const doc = parseXml(xml),
    node = nodeAt(doc, indexes);
  node.parentNode.removeChild(node);
  return serializeXml(doc);
}
export function moveNode(xml, indexes, direction) {
  if (!indexes.length) return xml;
  const doc = parseXml(xml),
    node = nodeAt(doc, indexes),
    siblings = elements(node.parentNode),
    index = siblings.indexOf(node),
    other = siblings[index + direction];
  if (other)
    node.parentNode.insertBefore(
      direction < 0 ? node : other,
      direction < 0 ? other : node,
    );
  return serializeXml(doc);
}
export function validation(project) {
  const errors = [],
    warnings = [];
  let doc;
  try {
    doc = parseXml(project.xml);
  } catch (e) {
    return { errors: [e.message], warnings };
  }
  if (!metadata(project.xml).name.trim())
    errors.push("Give the package a name.");
  if (!child(doc.documentElement, "content"))
    errors.push("The package needs a <content> element.");
  const names = Object.keys(project.files),
    insensitive = new Set();
  for (const name of names) {
    try {
      safePath(name);
    } catch (e) {
      errors.push(e.message);
    }
    if (name === "assembly.xml" || name === PROJECT_MARKER)
      errors.push(`Reserved filename: ${name}`);
    if (insensitive.has(name.toLowerCase()))
      errors.push(`Duplicate filename on Windows: ${name}`);
    insensitive.add(name.toLowerCase());
  }
  const used = new Set();
  for (const node of Array.from(doc.getElementsByTagName("*")))
    if (node.hasAttribute("source")) {
      const source = node.getAttribute("source").replaceAll("\\", "/"),
        name = `content/${source}`;
      used.add(name);
      if (!names.includes(name))
        errors.push(`Missing payload for source="${source}" (${name}).`);
    }
  if (!names.includes("icon.png"))
    warnings.push("No icon.png. You can add a 128 × 128 PNG in Package files.");
  for (const name of names)
    if (name.startsWith("content/") && !used.has(name))
      warnings.push(`Unreferenced payload: ${name}`);
  return { errors, warnings };
}
export async function openArchive(bytes, name = "") {
  if (bytes.byteLength > MAX_BYTES)
    throw new Error(
      "This alpha supports archives up to 1 GB. Use the original builder for larger packages.",
    );
  const zip = await JSZip.loadAsync(bytes, { checkCRC32: true }),
    entries = Object.values(zip.files).filter((f) => !f.dir);
  if (
    entries.some((f) => f.unsafeOriginalName && f.unsafeOriginalName !== f.name)
  )
    throw new Error("The archive contains unsafe paths.");
  if (!zip.file("assembly.xml"))
    throw new Error(
      "The archive has no assembly.xml at its root. Export legacy .mogk JSON projects as .oiv in the original builder first.",
    );
  if (zip.file(PROJECT_MARKER)) {
    const manifest = JSON.parse(await zip.file(PROJECT_MARKER).async("string"));
    if (manifest.format !== "magicogk-oiv-studio" || manifest.version !== 1)
      throw new Error("Unsupported Studio project version.");
  }
  const xml = await zip.file("assembly.xml").async("string");
  parseXml(xml);
  const files = {};
  let total = encoder.encode(xml).byteLength;
  for (const entry of entries) {
    if (entry.name === "assembly.xml" || entry.name === PROJECT_MARKER)
      continue;
    const path = safePath(entry.name),
      data = await entry.async("uint8array");
    total += data.byteLength;
    if (total > MAX_BYTES)
      throw new Error("The uncompressed archive exceeds the 1 GB alpha limit.");
    Object.defineProperty(files, path, {
      value: data,
      enumerable: true,
      configurable: true,
      writable: true,
    });
  }
  return { xml, files, name: metadata(xml).name || name };
}
export async function saveArchive(project, isProject = false) {
  parseXml(project.xml);
  if (!isProject) {
    const { errors } = validation(project);
    if (errors.length) throw new Error(errors.join("\n"));
  }
  if (
    Object.values(project.files).reduce(
      (total, data) => total + data.byteLength,
      encoder.encode(project.xml).byteLength,
    ) > MAX_BYTES
  )
    throw new Error("The project exceeds the 1 GB alpha limit.");
  const zip = new JSZip();
  zip.file("assembly.xml", project.xml);
  for (const [name, bytes] of Object.entries(project.files)) {
    safePath(name);
    if (name === "assembly.xml" || name === PROJECT_MARKER)
      throw new Error(`Reserved filename: ${name}`);
    zip.file(name, bytes);
  }
  if (isProject)
    zip.file(
      PROJECT_MARKER,
      JSON.stringify({ format: "magicogk-oiv-studio", version: 1 }, null, 2),
    );
  return zip.generateAsync({
    type: "uint8array",
    compression: "DEFLATE",
    compressionOptions: { level: 6 },
  });
}
export function decodeText(bytes) {
  return decoder.decode(bytes);
}
export function encodeText(text) {
  return encoder.encode(text);
}
export function renameFile(project, from, to) {
  to = safePath(to);
  if (to === "assembly.xml" || to === PROJECT_MARKER)
    throw new Error("Reserved filename.");
  if (from === to) return project;
  if (
    Object.keys(project.files).some((p) => p.toLowerCase() === to.toLowerCase())
  )
    throw new Error("A file already exists at that path.");
  const doc = parseXml(project.xml);
  if (from.startsWith("content/")) {
    if (!to.startsWith("content/"))
      throw new Error("Keep payload files inside content/.");
    for (const node of Array.from(doc.getElementsByTagName("*")))
      if (node.getAttribute("source")?.replaceAll("\\", "/") === from.slice(8))
        node.setAttribute("source", to.slice(8));
  }
  const files = { ...project.files };
  Object.defineProperty(files, to, {
    value: files[from],
    enumerable: true,
    configurable: true,
    writable: true,
  });
  delete files[from];
  return { ...project, xml: serializeXml(doc), files };
}
