const { contextBridge, ipcRenderer } = require("electron");
contextBridge.exposeInMainWorld("studio", {
  open: () => ipcRenderer.invoke("studio:open"),
  save: (data) => ipcRenderer.invoke("studio:save", data),
});
