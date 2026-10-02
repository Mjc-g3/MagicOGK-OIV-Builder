import React, { useEffect, useMemo, useRef, useState } from "react";
import { createRoot } from "react-dom/client";
import {
  Box,
  FolderOpen,
  Save,
  Hammer,
  Plus,
  FileCode2,
  Files,
  SlidersHorizontal,
  CheckCircle2,
  AlertTriangle,
  ChevronRight,
  ChevronDown,
  Trash2,
  ArrowUp,
  ArrowDown,
  Package,
  Search,
  Download,
  RotateCcw,
  Upload,
  X,
} from "lucide-react";
import * as core from "./core.js";
import "./styles.css";
const size = (bytes) =>
  bytes < 1024
    ? `${bytes} B`
    : bytes < 1048576
      ? `${(bytes / 1024).toFixed(1)} KB`
      : `${(bytes / 1048576).toFixed(1)} MB`;
const tabs = [
  ["overview", "Overview", SlidersHorizontal],
  ["instructions", "Install instructions", FolderOpen],
  ["files", "Package files", Files],
  ["xml", "XML editor", FileCode2],
  ["checks", "Build checks", CheckCircle2],
];
function Button({ icon: Icon, children, ...props }) {
  return (
    <button {...props}>
      {Icon && <Icon size={16} />} {children}
    </button>
  );
}
function Field({ label, ...props }) {
  return (
    <label className="field">
      <span>{label}</span>
      <input {...props} />
    </label>
  );
}
function Tree({ node, path = [], selected, onSelect, depth = 0 }) {
  const [expanded, setExpanded] = useState(true),
    kids = core.elements(node),
    active = JSON.stringify(path) === JSON.stringify(selected);
  const label =
    node.getAttribute("path") ||
    node.getAttribute("source") ||
    (kids.length ? "" : node.textContent.trim());
  return (
    <>
      <div
        className={`tree-row ${active ? "active" : ""}`}
        style={{ paddingLeft: 12 + depth * 18 }}
      >
        <button
          className="tree-toggle"
          aria-label={`${expanded ? "Collapse" : "Expand"} ${node.tagName}`}
          onClick={() => setExpanded(!expanded)}
          disabled={!kids.length}
        >
          {kids.length ? (
            expanded ? (
              <ChevronDown size={13} />
            ) : (
              <ChevronRight size={13} />
            )
          ) : (
            <span />
          )}
        </button>
        <button className="tree-label" aria-label={`${node.tagName}${label ? " " + label : ""}`} onClick={() => onSelect(path)}>
          <span className={`node-type ${node.tagName}`}>{node.tagName}</span>
          <span className="ellipsis">{label}</span>
        </button>
      </div>
      {expanded &&
        kids.map((n, i) => (
          <Tree
            key={i}
            node={n}
            path={[...path, i]}
            selected={selected}
            onSelect={onSelect}
            depth={depth + 1}
          />
        ))}
    </>
  );
}
function NodeEditor({
  node,
  path,
  onUpdate,
  onAdd,
  onRemove,
  onMove,
  run,
  onPending,
}) {
  const [tag, setTag] = useState(node.tagName),
    [attrs, setAttrs] = useState(
      Array.from(node.attributes).map((a) => [a.name, a.value]),
    ),
    [text, setText] = useState(node.textContent),
    [newTag, setNewTag] = useState("add");
  const hasKids = core.elements(node).length > 0;
  useEffect(() => {
    onPending(false);
  }, []);
  return (
    <div className="inspector">
      <div className="section-title">
        <div>
          <span className="eyebrow">ELEMENT PROPERTIES</span>
          <h2>{node.tagName}</h2>
        </div>
        <div className="button-row">
          <Button
            icon={ArrowUp}
            title="Move earlier"
            onClick={() => onMove(-1)}
            disabled={!path.length}
          />
          <Button
            icon={ArrowDown}
            title="Move later"
            onClick={() => onMove(1)}
            disabled={!path.length}
          />
          <Button
            icon={Trash2}
            title="Remove element"
            className="danger"
            disabled={!path.length}
            onClick={onRemove}
          />
        </div>
      </div>
      <form
        onChange={() => onPending(true)}
        onSubmit={(e) => {
          e.preventDefault();
          run(() => {
            const keys = attrs.map((a) => a[0]);
            if (keys.some((k) => !k) || new Set(keys).size !== keys.length)
              throw new Error("Attribute names must be unique and nonempty.");
            onUpdate({
              tag,
              attributes: Object.fromEntries(attrs),
              ...(!hasKids ? { text } : {}),
            });
          });
        }}
      >
        <Field
          label="Element name"
          disabled={!path.length}
          value={tag}
          onChange={(e) => setTag(e.target.value)}
        />
        <div className="section-label">
          Attributes{" "}
          <Button
            icon={Plus}
            type="button"
            onClick={() => {
              setAttrs([...attrs, ["", ""]]);
              onPending(true);
            }}
          >
            Add
          </Button>
        </div>
        {attrs.map(([key, value], i) => (
          <div className="attribute" key={i}>
            <input
              aria-label={`Attribute ${i + 1} name`}
              value={key}
              placeholder="name"
              onChange={(e) =>
                setAttrs(
                  attrs.map((a, j) => (j === i ? [e.target.value, a[1]] : a)),
                )
              }
            />
            <input
              aria-label={`Attribute ${i + 1} value`}
              value={value}
              placeholder="value"
              onChange={(e) =>
                setAttrs(
                  attrs.map((a, j) => (j === i ? [a[0], e.target.value] : a)),
                )
              }
            />
            <Button
              icon={X}
              title="Remove attribute"
              type="button"
              onClick={() => {
                setAttrs(attrs.filter((_, j) => i !== j));
                onPending(true);
              }}
            />
          </div>
        ))}
        {!hasKids && (
          <label className="field">
            <span>Text / installation target</span>
            <textarea
              rows={5}
              value={text}
              onChange={(e) => setText(e.target.value)}
              spellCheck="false"
            />
          </label>
        )}
        {hasKids && (
          <p className="muted">
            Select a child to edit its value. The XML editor also supports
            comments, CDATA, and mixed content.
          </p>
        )}
        <Button className="primary" icon={CheckCircle2} type="submit">
          Apply element changes
        </Button>
      </form>
      <div className="divider" />
      <h3>Add a child instruction</h3>
      <p className="muted">
        Children retain their order in the exported package.
      </p>
      <div className="button-row">
        <select
          aria-label="New element type"
          value={newTag}
          onChange={(e) => setNewTag(e.target.value)}
        >
          {["add", "archive", "delete", "xml", "Item", "replace", "remove"].map(
            (t) => (
              <option key={t}>{t}</option>
            ),
          )}
        </select>
        <Button icon={Plus} onClick={() => onAdd(newTag)}>
          Add child
        </Button>
      </div>
      <p className="hint">
        For an archive, set path, type="RPF7", and createIfNotExist. For a file
        addition, source is relative to content/ and its text is the destination
        inside the selected archive.
      </p>
    </div>
  );
}
function App() {
  const [project, setProject] = useState(core.newProject),
    [tab, setTab] = useState("overview"),
    [dirty, setDirty] = useState(false),
    [busy, setBusy] = useState(false),
    [error, setError] = useState(""),
    [notice, setNotice] = useState(""),
    [selected, setSelected] = useState([]),
    [query, setQuery] = useState(""),
    [file, setFile] = useState(""),
    [xmlDraft, setXmlDraft] = useState(null),
    [textDraft, setTextDraft] = useState(null),
    [importMode, setImportMode] = useState("content"),
    [replaceTarget, setReplaceTarget] = useState(null),
    [pendingElement, setPendingElement] = useState(false);
  const openRef = useRef(),
    addRef = useRef();
  const meta = useMemo(() => core.metadata(project.xml), [project.xml]),
    doc = useMemo(() => core.parseXml(project.xml), [project.xml]),
    checks = useMemo(() => core.validation(project), [project]);
  const node = core.nodeAt(doc, selected) || doc.documentElement,
    paths = Object.keys(project.files),
    bytes = Object.values(project.files).reduce((n, b) => n + b.byteLength, 0),
    pendingXml = xmlDraft !== null && xmlDraft !== project.xml,
    pendingText = textDraft !== null;
  const change = (p) => {
    setProject(p);
    setDirty(true);
    setNotice("");
  };
  const xmlChange = (xml) => change({ ...project, xml });
  async function run(fn) {
    setError("");
    try {
      return await fn();
    } catch (e) {
      setError(e.message);
    }
  }
  const confirmDiscard = () =>
    !(dirty || pendingXml || pendingText || pendingElement) ||
    window.confirm("Discard unsaved project or editor changes?");
  const load = async (bytes, name) => {
    const p = await core.openArchive(bytes, name);
    setProject(p);
    setDirty(false);
    setSelected([]);
    setFile("");
    setXmlDraft(null);
    setTextDraft(null);
    setTab("overview");
    setPendingElement(false);
    setNotice(`Opened ${name}`);
  };
  const navigate = (action) => {
    if (
      (pendingXml || pendingText || pendingElement) &&
      !window.confirm("Discard unapplied editor changes?")
    )
      return;
    setXmlDraft(null);
    setTextDraft(null);
    setPendingElement(false);
    action();
  };
  const open = () =>
    run(async () => {
      if (!confirmDiscard()) return;
      if (window.studio) {
        setBusy(true);
        try {
          const data = await window.studio.open();
          if (data) await load(data.bytes, data.name);
        } finally {
          setBusy(false);
        }
      } else openRef.current.click();
    });
  const download = async (data, name, isProject) => {
    if (window.studio)
      return window.studio.save({ bytes: data, name, project: isProject });
    const url = URL.createObjectURL(
        new Blob([data], { type: "application/zip" }),
      ),
      a = document.createElement("a");
    a.href = url;
    a.download = name;
    a.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
    return true;
  };
  const save = (isProject) =>
    run(async () => {
      if (pendingXml || pendingText || pendingElement)
        throw new Error(
          "Apply or discard pending element, XML, or file text edits first.",
        );
      setBusy(true);
      try {
        const data = await core.saveArchive(project, isProject),
          name =
            (meta.name || "MyMod").replace(/[<>:"/\\|?*\x00-\x1f]/g, "_") +
            (isProject ? ".mogk2" : ".oiv");
        if (await download(data, name, isProject)) {
          if (isProject) setDirty(false);
          setNotice(
            isProject
              ? "Project saved with all package files."
              : "OIV package built successfully.",
          );
        }
      } finally {
        setBusy(false);
      }
    });
  useEffect(() => {
    const guard = (e) => {
      if (dirty || pendingXml || pendingText || pendingElement) {
        e.preventDefault();
        e.returnValue = "";
      }
    };
    window.addEventListener("beforeunload", guard);
    return () => window.removeEventListener("beforeunload", guard);
  }, [dirty, pendingXml, pendingText, pendingElement]);
  useEffect(() => {
    const handler = (e) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "s") {
        e.preventDefault();
        if (!busy) save(true);
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  });
  const selectFile = (path) => {
    if (pendingText && !window.confirm("Discard unapplied file text edits?"))
      return;
    setFile(path);
    setTextDraft(null);
  };
  const addFiles = (e) =>
    run(async () => {
      const chosen = Array.from(e.target.files);
      e.target.value = "";
      const files = { ...project.files };
      for (const f of chosen) {
        const path =
          replaceTarget ||
          `${importMode === "content" ? "content/" : ""}${f.webkitRelativePath || f.name}`;
        core.safePath(path);
        if (path === "assembly.xml" || path === core.PROJECT_MARKER)
          throw new Error("Edit assembly.xml in the XML editor.");
        const existing = Object.keys(files).find(
          (p) => p.toLowerCase() === path.toLowerCase(),
        );
        if (existing && !replaceTarget)
          throw new Error(
            `${path} already exists. Select it and use Replace binary.`,
          );
        if (replaceTarget && pendingText)
          throw new Error(
            "Apply or discard text edits before replacing this file.",
          );
        Object.defineProperty(files, path, {
          value: new Uint8Array(await f.arrayBuffer()),
          enumerable: true,
          configurable: true,
          writable: true,
        });
      }
      change({ ...project, files });
      setReplaceTarget(null);
      setNotice(
        `Added ${chosen.length} file(s). Add installation instructions to reference new payloads.`,
      );
    });
  const addInstruction = (tag) =>
    run(() => {
      if (pendingElement)
        throw new Error("Apply element changes before adding a child.");
      const attrs =
        tag === "archive"
          ? {
              path: "update\\update.rpf",
              type: "RPF7",
              createIfNotExist: "False",
            }
          : tag === "add"
            ? {
                source:
                  paths.find((p) => p.startsWith("content/"))?.slice(8) ||
                  "filename.ext",
              }
            : tag === "xml"
              ? { path: "common\\data\\dlclist.xml" }
              : {};
      xmlChange(core.addNode(project.xml, selected, tag, attrs));
    });
  let textFile = null;
  if (
    file &&
    /\.(xml|txt|json|ini|cfg|lua|cs|md|log|csv)$/i.test(file) &&
    project.files[file]?.byteLength < 2 * 1024 * 1024
  ) {
    try {
      textFile = core.decodeText(project.files[file]);
    } catch {}
  }
  return (
    <div className="app">
      <input
        ref={openRef}
        type="file"
        accept=".oiv,.mogk2"
        hidden
        onChange={(e) =>
          run(async () => {
            const f = e.target.files[0];
            e.target.value = "";
            if (!f) return;
            setBusy(true);
            try {
              await load(new Uint8Array(await f.arrayBuffer()), f.name);
            } finally {
              setBusy(false);
            }
          })
        }
      />
      <input
        ref={addRef}
        type="file"
        multiple={!replaceTarget}
        hidden
        onChange={addFiles}
      />
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">
            <Box size={25} />
          </div>
          <div>
            MagicOGK<span>OIV STUDIO</span>
          </div>
        </div>
        <span className="alpha">REACT PREVIEW · 2.0</span>
        <div className="side-label">WORKSPACE</div>
        <nav>
          {tabs.map(([id, label, Icon]) => (
            <button
              key={id}
              className={tab === id ? "nav active" : "nav"}
              onClick={() => navigate(() => setTab(id))}
            >
              <Icon size={18} />
              {label}
              {id === "checks" && checks.errors.length > 0 && (
                <span className="badge">{checks.errors.length}</span>
              )}
            </button>
          ))}
        </nav>
        <div className="side-label">PROJECT</div>
        <Button
          icon={Plus}
          className="nav"
          disabled={busy}
          onClick={() => {
            if (confirmDiscard()) {
              setProject(core.newProject());
              setDirty(false);
              setSelected([]);
              setXmlDraft(null);
              setTextDraft(null);
              setFile("");
              setError("");
              setNotice("");
              setTab("overview");
            }
          }}
        >
          New project
        </Button>
        <Button
          icon={FolderOpen}
          className="nav"
          disabled={busy}
          onClick={open}
        >
          Open OIV / project
        </Button>
        <Button
          icon={Save}
          className="nav"
          disabled={busy}
          onClick={() => save(true)}
        >
          Save project
        </Button>
        <div className="sidebar-bottom">
          <div className="status-dot" />
          Local workspace<span>Files stay on this computer</span>
        </div>
      </aside>
      <main>
        <header>
          <div>
            <span className="breadcrumb">
              Workspace <ChevronRight size={12} /> {meta.name}
            </span>
            <h1>{tabs.find((t) => t[0] === tab)[1]}</h1>
          </div>
          <div className="button-row">
            <span className="save-status">
              {dirty || pendingXml || pendingText || pendingElement
                ? "● Unsaved changes"
                : "Saved / clean"}
            </span>
            <Button icon={Save} disabled={busy} onClick={() => save(true)}>
              Save project
            </Button>
            <Button
              className="primary"
              icon={Hammer}
              disabled={busy}
              onClick={() => save(false)}
            >
              {busy ? "Working…" : "Build OIV"}
            </Button>
          </div>
        </header>
        {(error || notice) && (
          <div
            role={error ? "alert" : "status"}
            className={`message ${error ? "error" : "success"}`}
          >
            {error ? <AlertTriangle size={18} /> : <CheckCircle2 size={18} />}
            <span>{error || notice}</span>
            <button
              title="Dismiss"
              onClick={() => {
                setError("");
                setNotice("");
              }}
            >
              <X size={16} />
            </button>
          </div>
        )}
        <div className="content">
          {tab === "overview" && (
            <>
              <div className="intro">
                <span className="eyebrow">PACKAGE WORKSPACE</span>
                <h2>Build something worth installing.</h2>
                <p>
                  Edit your mod, organize its files, and export a package for
                  OpenIV.
                </p>
              </div>
              <div className="stats">
                <div>
                  <Package />
                  <span>Package format</span>
                  <strong>
                    {doc.documentElement.getAttribute("version")}{" "}
                    <small>OpenIV</small>
                  </strong>
                </div>
                <div>
                  <Files />
                  <span>Package files</span>
                  <strong>
                    {paths.length} <small>{size(bytes)}</small>
                  </strong>
                </div>
                <div>
                  <CheckCircle2 />
                  <span>Build status</span>
                  <strong>
                    {checks.errors.length ? "Needs attention" : "Ready"}{" "}
                    <small>{checks.warnings.length} notes</small>
                  </strong>
                </div>
              </div>
              <div className="overview-grid">
                <section className="card">
                  <div className="section-title">
                    <div>
                      <span className="eyebrow">IDENTITY</span>
                      <h2>Package details</h2>
                    </div>
                    <SlidersHorizontal size={20} />
                  </div>
                  <Field
                    label="Mod name"
                    value={meta.name}
                    onChange={(e) =>
                      xmlChange(
                        core.setMetadata(project.xml, "name", e.target.value),
                      )
                    }
                  />
                  <div className="two-col">
                    <Field
                      label="Author"
                      value={meta.author}
                      onChange={(e) =>
                        xmlChange(
                          core.setMetadata(
                            project.xml,
                            "author",
                            e.target.value,
                          ),
                        )
                      }
                    />
                    <Field
                      label="Website"
                      value={meta.website}
                      placeholder="https://"
                      onChange={(e) =>
                        xmlChange(
                          core.setMetadata(
                            project.xml,
                            "website",
                            e.target.value,
                          ),
                        )
                      }
                    />
                  </div>
                  <div className="three-col">
                    {["major", "minor", "tag"].map((k) => (
                      <Field
                        key={k}
                        label={k === "tag" ? "Version tag" : `${k} version`}
                        value={meta[k]}
                        onChange={(e) =>
                          xmlChange(
                            core.setMetadata(project.xml, k, e.target.value),
                          )
                        }
                      />
                    ))}
                  </div>
                  <label className="field">
                    <span>Description</span>
                    <textarea
                      rows={6}
                      value={meta.description}
                      placeholder="Tell users what this mod changes…"
                      onChange={(e) =>
                        xmlChange(
                          core.setMetadata(
                            project.xml,
                            "description",
                            e.target.value,
                          ),
                        )
                      }
                    />
                  </label>
                </section>
                <section className="card workflow">
                  <span className="eyebrow">YOUR WORKFLOW</span>
                  <h2>From files to finished mod.</h2>
                  {[
                    [
                      "01",
                      "Add your payload",
                      "Add files in Package files. RPFs remain binary payloads.",
                    ],
                    [
                      "02",
                      "Define installation",
                      "Create ordered add, delete, archive, and XML instructions.",
                    ],
                    [
                      "03",
                      "Check & build",
                      "Resolve missing files, save your project, then build an OIV.",
                    ],
                  ].map(([n, title, text]) => (
                    <div className="workflow-step" key={n}>
                      <span>{n}</span>
                      <div>
                        <h3>{title}</h3>
                        <p>{text}</p>
                      </div>
                    </div>
                  ))}
                  <Button
                    icon={FolderOpen}
                    onClick={() => setTab("instructions")}
                  >
                    Edit installation instructions <ChevronRight size={14} />
                  </Button>
                  <div className="hint">
                    Existing OIVs retain their full assembly.xml and binary
                    payloads. Studio projects include those files for portable
                    reopening.
                  </div>
                </section>
              </div>
            </>
          )}
          {tab === "instructions" && (
            <div className="split card">
              <section className="tree-panel">
                <div className="section-title">
                  <h2>Package structure</h2>
                  <span className="pill">XML tree</span>
                </div>
                <p className="muted">Select any element to edit it.</p>
                <Tree
                  node={doc.documentElement}
                  selected={selected}
                  onSelect={(path) => navigate(() => setSelected(path))}
                />
              </section>
              <NodeEditor
                key={`${JSON.stringify(selected)}:${project.xml}`}
                node={node}
                path={selected}
                run={run}
                onPending={setPendingElement}
                onUpdate={(data) =>
                  xmlChange(core.updateNode(project.xml, selected, data))
                }
                onAdd={addInstruction}
                onRemove={() =>
                  run(() => {
                    if (
                      window.confirm(
                        `Remove <${node.tagName}> and its children?`,
                      )
                    ) {
                      xmlChange(core.removeNode(project.xml, selected));
                      setSelected(selected.slice(0, -1));
                    }
                  })
                }
                onMove={(direction) =>
                  run(() => {
                    const moved = core.moveNode(
                      project.xml,
                      selected,
                      direction,
                    );
                    if (
                      core.elements(node.parentNode)[
                        selected.at(-1) + direction
                      ]
                    ) {
                      xmlChange(moved);
                      setSelected([
                        ...selected.slice(0, -1),
                        selected.at(-1) + direction,
                      ]);
                    }
                  })
                }
              />
            </div>
          )}
          {tab === "files" && (
            <>
              <div className="file-toolbar">
                <label className="search">
                  <Search size={16} />
                  <input
                    placeholder="Search package files…"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                  />
                </label>
                <select
                  aria-label="Where to add files"
                  value={importMode}
                  onChange={(e) => setImportMode(e.target.value)}
                >
                  <option value="content">Add to content/</option>
                  <option value="root">Add to package root</option>
                </select>
                <Button
                  icon={Upload}
                  onClick={() => {
                    setReplaceTarget(null);
                    addRef.current.click();
                  }}
                >
                  Add files
                </Button>
              </div>
              <div className="split card">
                <section className="file-list">
                  <div className="list-heading">
                    <span>FILE / PATH</span>
                    <span>SIZE</span>
                  </div>
                  {paths
                    .filter((p) =>
                      p.toLowerCase().includes(query.toLowerCase()),
                    )
                    .sort()
                    .map((p) => (
                      <button
                        key={p}
                        className={`file-row ${file === p ? "active" : ""}`}
                        onClick={() => selectFile(p)}
                      >
                        <FileCode2 size={16} />
                        <span className="ellipsis">{p}</span>
                        <small>{size(project.files[p].byteLength)}</small>
                      </button>
                    ))}
                  {!paths.length && (
                    <div className="empty">
                      <Files size={32} />
                      <h3>Your package starts here.</h3>
                      <p>
                        Add payload files, then reference them in install
                        instructions.
                      </p>
                    </div>
                  )}
                </section>
                <section className="inspector">
                  {file && project.files[file] ? (
                    <>
                      <span className="eyebrow">FILE INSPECTOR</span>
                      <h2 className="break">{file}</h2>
                      <p className="muted">
                        {size(project.files[file].byteLength)}
                      </p>
                      <div className="button-row wrap">
                        <Button
                          icon={Upload}
                          onClick={() => {
                            setReplaceTarget(file);
                            addRef.current.click();
                          }}
                        >
                          Replace binary
                        </Button>
                        <Button
                          icon={Download}
                          onClick={() =>
                            run(() =>
                              download(
                                project.files[file],
                                file.split("/").at(-1),
                                undefined,
                              ),
                            )
                          }
                        >
                          Export file
                        </Button>
                        <Button
                          icon={Trash2}
                          className="danger"
                          onClick={() => {
                            if (
                              window.confirm(
                                "Remove this payload? Instructions referencing it will need updating.",
                              )
                            ) {
                              const files = { ...project.files };
                              delete files[file];
                              change({ ...project, files });
                              setFile("");
                              setTextDraft(null);
                            }
                          }}
                        >
                          Remove
                        </Button>
                      </div>
                      <form
                        className="rename-form"
                        onSubmit={(e) => {
                          e.preventDefault();
                          run(() => {
                            if (pendingText)
                              throw new Error(
                                "Apply or discard file text edits before renaming.",
                              );
                            const to = e.target.elements.path.value;
                            change(core.renameFile(project, file, to));
                            setFile(to);
                            setTextDraft(null);
                          });
                        }}
                      >
                        <Field
                          key={file}
                          name="path"
                          label="Package path (renaming updates source references)"
                          defaultValue={file}
                        />
                        <Button type="submit">Rename file</Button>
                      </form>
                      {textFile !== null ? (
                        <>
                          <label className="field">
                            <span>UTF-8 file contents</span>
                            <textarea
                              className="code"
                              rows={18}
                              spellCheck="false"
                              value={textDraft ?? textFile}
                              onChange={(e) => setTextDraft(e.target.value)}
                            />
                          </label>
                          <div className="button-row">
                            <Button
                              className="primary"
                              disabled={textDraft === null}
                              onClick={() => {
                                change({
                                  ...project,
                                  files: {
                                    ...project.files,
                                    [file]: core.encodeText(textDraft),
                                  },
                                });
                                setTextDraft(null);
                              }}
                            >
                              Apply text changes
                            </Button>
                            <Button
                              disabled={textDraft === null}
                              onClick={() => setTextDraft(null)}
                            >
                              Discard edits
                            </Button>
                          </div>
                        </>
                      ) : (
                        <p className="hint">
                          Binary files are preserved byte for byte. Use Replace
                          binary to swap an RPF, texture, or image.
                        </p>
                      )}
                    </>
                  ) : (
                    <div className="empty">
                      <FileCode2 size={32} />
                      <p>
                        Select a file to rename, replace, export, or edit its
                        text.
                      </p>
                    </div>
                  )}
                </section>
              </div>
            </>
          )}
          {tab === "xml" && (
            <section className="card">
              <div className="section-title">
                <div>
                  <span className="eyebrow">FULL CONTROL</span>
                  <h2>assembly.xml</h2>
                </div>
                <div className="button-row">
                  <Button
                    icon={RotateCcw}
                    onClick={() => setXmlDraft(null)}
                    disabled={!pendingXml}
                  >
                    Discard edits
                  </Button>
                  <Button
                    className="primary"
                    icon={CheckCircle2}
                    disabled={!pendingXml}
                    onClick={() =>
                      run(() => {
                        core.parseXml(xmlDraft);
                        xmlChange(xmlDraft);
                        setXmlDraft(null);
                        setSelected([]);
                      })
                    }
                  >
                    Apply XML
                  </Button>
                </div>
              </div>
              <p className="muted">
                Edit every element, attribute, XML patch, installer color, and
                target. Changes are validated before applying.
              </p>
              <textarea
                aria-label="assembly.xml editor"
                className="code xml-editor"
                value={xmlDraft ?? project.xml}
                onChange={(e) => setXmlDraft(e.target.value)}
                spellCheck="false"
              />
              {pendingXml && (
                <p className="hint">
                  Unapplied edits — apply or discard before saving or building.
                </p>
              )}
            </section>
          )}
          {tab === "checks" && (
            <section className="card">
              <div className="section-title">
                <div>
                  <span className="eyebrow">PRE-FLIGHT</span>
                  <h2>
                    {checks.errors.length
                      ? "Resolve these issues before building"
                      : "Package checks passed"}
                  </h2>
                </div>
                <CheckCircle2 size={28} />
              </div>
              <p className="muted">
                Checks cover XML syntax, package paths, and referenced payloads.
                OpenIV installation still needs testing in a separate game copy.
              </p>
              {checks.errors.map((e, i) => (
                <div className="check error" key={i}>
                  <AlertTriangle size={18} />
                  <span>{e}</span>
                </div>
              ))}
              {checks.warnings.map((w, i) => (
                <div className="check warning" key={i}>
                  <AlertTriangle size={18} />
                  <span>{w}</span>
                </div>
              ))}
              {!checks.errors.length && (
                <div className="check success">
                  <CheckCircle2 size={18} />
                  XML parses and all referenced payloads are present.
                </div>
              )}
              <Button
                className="primary"
                icon={Hammer}
                disabled={busy || checks.errors.length > 0}
                onClick={() => save(false)}
              >
                Build OIV package
              </Button>
            </section>
          )}
        </div>
        <footer>
          <span>
            <span className="status-dot" />{" "}
            {busy ? "Processing package…" : "Workspace ready"}
          </span>
          <span>
            {paths.length} files · {size(bytes)} ·{" "}
            {doc.documentElement.getAttribute("target") || "Unspecified target"}
          </span>
        </footer>
      </main>
    </div>
  );
}
createRoot(document.getElementById("root")).render(<App />);
