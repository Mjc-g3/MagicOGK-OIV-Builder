const { app, BrowserWindow, dialog, ipcMain } = require("electron");
const { readFile, writeFile, rename, unlink } = require("node:fs/promises");
const path = require("node:path");
let window;
app.whenReady().then(() => {
  window = new BrowserWindow({
    width: 1440,
    height: 960,
    minWidth: 1000,
    minHeight: 700,
    backgroundColor: "#0e1118",
    title: "MagicOGK OIV Studio",
    webPreferences: {
      preload: path.join(__dirname, "preload.cjs"),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: true,
    },
  });
  window.setMenuBarVisibility(false);
  window.webContents.setWindowOpenHandler(() => ({ action: "deny" }));
  window.webContents.on("will-navigate", (event) => event.preventDefault());
  window.loadFile(path.join(__dirname, "../dist/index.html"));
});
ipcMain.handle("studio:open", async () => {
  const result = await dialog.showOpenDialog(window, {
    filters: [
      { name: "OpenIV / Studio project", extensions: ["oiv", "mogk2"] },
    ],
    properties: ["openFile"],
  });
  if (result.canceled) return null;
  const filename = result.filePaths[0];
  return {
    name: path.basename(filename),
    bytes: new Uint8Array(await readFile(filename)),
  };
});
ipcMain.handle("studio:save", async (_event, { name, bytes, project }) => {
  const result = await dialog.showSaveDialog(window, {
    defaultPath: path.basename(name),
    filters: [
      {
        name: project ? "OIV Studio project" : "OpenIV package",
        extensions: [
          project === undefined
            ? path.extname(name).slice(1) || "bin"
            : project
              ? "mogk2"
              : "oiv",
        ],
      },
    ],
  });
  if (result.canceled) return false;
  const temp = `${result.filePath}.${Date.now()}.tmp`;
  try {
    await writeFile(temp, Buffer.from(bytes));
    await rename(temp, result.filePath);
  } finally {
    await unlink(temp).catch(() => {});
  }
  return true;
});
app.on("window-all-closed", () => app.quit());
