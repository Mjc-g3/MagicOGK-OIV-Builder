# MagicOGK OIV Studio — React rewrite

A separate, working alpha of the OIV Builder UI, built with React, HTML/CSS, and Electron. The original C# application stays intact in the repository root. This directory is independent of its solution and release pipeline.

## Run on Windows

Install Node.js 22 or newer, then run from this directory:

```powershell
npm ci
npm run desktop
```

For the browser preview, use `npm run dev` and open the local URL. The browser uses file selection and downloads; Electron uses native Windows open/save dialogs. No local web server is required for the packaged desktop application.

Build a portable Windows executable on Windows with `npm run pack:win`. The **OIV Studio React** GitHub Actions workflow also builds a downloadable `oiv-studio-windows` artifact from the rewrite branch. This is an alpha artifact, not a published release or installer.

## Working features

- Open existing `.oiv` ZIP packages with `assembly.xml` at the root.
- Edit package metadata with a form.
- Inspect, edit, add, remove, and reorder XML elements, including nested RPF archive instructions, add/delete actions, and inline XML patches.
- Edit every part of `assembly.xml` directly, including installer colors, unknown instructions, comments, CDATA, and mixed content.
- Add, replace, rename, export, and remove payload files. Renaming files in `content/` updates matching `source` attributes.
- Edit UTF-8 text payloads up to 2 MB. RPFs, textures, and other binary files can be replaced without interpreting their internals.
- Save and reopen self-contained `.mogk2` projects with their payloads included. Unlike legacy `.mogk` files, these do not depend on external source paths.
- Build `.oiv` packages. Missing payloads, unsafe paths, malformed XML, and Windows filename collisions block the build. Incomplete projects with missing payloads can still be saved.
- Protect unsaved changes and unapplied editor drafts. Ctrl+S saves a project; native saves use a temporary file before replacing the destination.

## Compatibility and scope

This is the start of a migration, not a replacement for the mature C# tool. Its full instruction editor intentionally uses `assembly.xml` as the source of truth: importing an OIV does not flatten, regenerate, or drop unknown XML commands. XML formatting can change after visual edits; untouched XML and all payload bytes survive archive round trips.

Legacy `.mogk` JSON projects are not imported directly yet. Open them in the original C# application, build an OIV, then open that OIV here. Studio saves use a new `.mogk2` extension so they cannot accidentally overwrite legacy projects. The original app cannot read `.mogk2` projects.

The existing C# installer, game-folder detection, automatic content.xml/dlclist generation, remote assets, and legacy project migration have **not** been ported. Imported instructions created by those tools are retained and editable. Creating new XML patches currently requires the instruction tree or full XML editor. Editing the inside of an RPF archive still requires OpenIV or another RPF editor.

The alpha handles archives in memory and applies a 1 GB compressed/uncompressed size limit. Large mods should continue using the original builder. Build checks verify package structure and payload references; they do not certify arbitrary instruction semantics. No in-game or OpenIV installation validation has been performed in this Linux development environment.

## Layout

- `src/core.js`: archive/project serialization, XML operations, and validation; no UI dependency.
- `src/main.jsx` / `src/styles.css`: React workspace and presentation.
- `electron/`: isolated desktop shell and native file dialogs. The renderer has no direct Node or filesystem access.
- `tests/core.test.js`: archive round trips, binary preservation, XML editing, missing payloads, and path checks.

## Validation

```powershell
npm test
npm run build
```

Keep the original C# release pipeline separate until this version reaches feature parity and has been tested with real packages in OpenIV.
