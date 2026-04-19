using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace MagicOGK_OIV_Builder
{
    public partial class main : Form
    {
        private bool sidebarOpen = false;
        private int sidebarTargetX = -160;
        private Point dragStart;
        private bool isDragging = false;
        private string currentProjectPath = string.Empty;
        private OIVProject currentProject = new OIVProject();
        private string activeTab = "package";

        public main()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            panelDrag.MouseDown += PanelDrag_MouseDown;
            panelDrag.MouseMove += PanelDrag_MouseMove;
            panelDrag.MouseUp += PanelDrag_MouseUp;

            btnOpenP.Click += btnOpenP_Click;
            btnSave.Click += btnSave_Click;
            btnSaveAs.Click += btnSaveAs_Click;
            btnBuildOIV.Click += btnBuildOIV_Click;
            btnOpenOIV.Click += btnOpenOIV_Click;

            dropdownVersionTag.SelectedIndex = 3;
            dropdownOIVSpec.SelectedIndex = 3;

            await webViewBackground.EnsureCoreWebView2Async();
            webViewBackground.CoreWebView2.Settings.IsZoomControlEnabled = false;
            webViewBackground.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webViewBackground.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webViewBackground.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            LoadPackageTab();
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string? rawMsg = e.TryGetWebMessageAsString();
            if (rawMsg == null) return;
            string msg = rawMsg;
            if (msg.StartsWith("addFile:"))
            {
                string json = msg.Substring(8);
                HandleAddFileMessage(json);
            }
            else if (msg.StartsWith("removeFile:"))
            {
                string idStr = msg.Substring(11);
                if (int.TryParse(idStr, out int id))
                    currentProject.Files.RemoveAll(f => f.Id == id);
                RefreshCurrentTab();
            }
            else if (msg.StartsWith("updatePath:"))
            {
                string json = msg.Substring(11);
                HandleUpdatePathMessage(json);
            }
            else if (msg == "openFilePicker")
            {
                OpenFilePicker();
            }
        }

        private void HandleAddFileMessage(string json)
        {
            try
            {
                var parts = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(json);
                if (parts == null) return;
                var entry = new OIVFileEntry
                {
                    Id = currentProject.NextId++,
                    SourcePath = parts.ContainsKey("source") ? parts["source"] : "",
                    TargetPath = parts.ContainsKey("target") ? parts["target"] : "",
                    FileName = parts.ContainsKey("name") ? parts["name"] : "",
                    Type = parts.ContainsKey("type") ? parts["type"] : "content"
                };
                currentProject.Files.Add(entry);
                RefreshCurrentTab();
            }
            catch { }
        }

        private void HandleUpdatePathMessage(string json)
        {
            try
            {
                var parts = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(json);
                if (parts == null) return;
                if (parts.ContainsKey("id") && int.TryParse(parts["id"], out int id))
                {
                    var entry = currentProject.Files.Find(f => f.Id == id);
                    if (entry != null && parts.ContainsKey("target"))
                        entry.TargetPath = parts["target"];
                    if (entry != null && parts.ContainsKey("type"))
                        entry.Type = parts["type"];
                }
            }
            catch { }
        }

        private void OpenFilePicker()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(OpenFilePicker));
                return;
            }
            using var dlg = new OpenFileDialog
            {
                Title = "Select mod file(s)",
                Multiselect = true,
                Filter = "All Files (*.*)|*.*|YFT Files (*.yft)|*.yft|YTD Files (*.ytd)|*.ytd|META Files (*.meta)|*.meta|XML Files (*.xml)|*.xml|ASI Files (*.asi)|*.asi"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (string path in dlg.FileNames)
                {
                    string name = System.IO.Path.GetFileName(path);
                    var entry = new OIVFileEntry
                    {
                        Id = currentProject.NextId++,
                        SourcePath = path,
                        TargetPath = "",
                        FileName = name,
                        Type = "content"
                    };
                    currentProject.Files.Add(entry);
                }
                RefreshCurrentTab();
            }
        }

        private void LoadPackageTab()
        {
            activeTab = "package";
            string html = PackageTabHtml();
            webViewBackground.CoreWebView2.NavigateToString(html);
        }

        private void RefreshCurrentTab()
        {
            if (activeTab == "package")
                LoadPackageTab();
        }

        private string PackageTabHtml()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'>
<style>
  html, body { margin:0; padding:0; background:#0d0d0d; color:#e8e8e8; font-family:'Segoe UI',Arial,sans-serif; font-size:14px; height:100%; overflow:hidden; }
  .container { padding:24px 32px; height:calc(100vh - 48px); display:flex; flex-direction:column; gap:16px; }
  .section-title { font-size:13px; font-weight:700; letter-spacing:1.4px; text-transform:uppercase; color:#bc8f8f; margin-bottom:8px; }
  .toolbar { display:flex; gap:10px; align-items:center; }
  .btn { background:#1a1a1a; border:1px solid #333; color:#e0e0e0; padding:8px 18px; border-radius:4px; cursor:pointer; font-size:13px; transition:background 0.15s,border-color 0.15s; }
  .btn:hover { background:#400000; border-color:#bc8f8f; color:#fff; }
  .btn-primary { background:#400000; border-color:#8b3a3a; color:#f0c0c0; }
  .btn-primary:hover { background:#600000; border-color:#bc8f8f; }
  .drop-zone { border:2px dashed #333; border-radius:8px; padding:24px; text-align:center; color:#666; cursor:pointer; transition:border-color 0.2s,background 0.2s; background:#111; }
  .drop-zone:hover,.drop-zone.drag-over { border-color:#8b3a3a; background:#1a0a0a; color:#bc8f8f; }
  .drop-zone-icon { font-size:28px; margin-bottom:4px; }
  .drop-zone-hint { font-size:12px; color:#555; margin-top:4px; }
  .files-table { flex:1; overflow-y:auto; background:#111; border:1px solid #222; border-radius:6px; }
  table { width:100%; border-collapse:collapse; }
  thead th { background:#181818; color:#bc8f8f; font-size:11px; font-weight:700; letter-spacing:1px; text-transform:uppercase; padding:10px 14px; text-align:left; border-bottom:1px solid #222; position:sticky; top:0; }
  tbody tr { border-bottom:1px solid #1a1a1a; transition:background 0.1s; }
  tbody tr:hover { background:#161616; }
  tbody td { padding:8px 14px; vertical-align:middle; color:#ccc; font-size:13px; }
  .td-name { color:#e8e8e8; font-weight:500; max-width:180px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
  .td-path input { background:#0d0d0d; border:1px solid #2a2a2a; color:#ccc; padding:5px 9px; border-radius:3px; width:100%; font-size:12px; font-family:'Consolas',monospace; box-sizing:border-box; transition:border-color 0.15s; }
  .td-path input:focus { outline:none; border-color:#8b3a3a; color:#fff; }
  .td-type select { background:#0d0d0d; border:1px solid #2a2a2a; color:#ccc; padding:5px 8px; border-radius:3px; font-size:12px; }
  .td-type select:focus { outline:none; border-color:#8b3a3a; }
  .td-remove button { background:transparent; border:1px solid #3a1a1a; color:#8b3a3a; padding:4px 10px; border-radius:3px; cursor:pointer; font-size:11px; transition:all 0.15s; }
  .td-remove button:hover { background:#3a0000; color:#ff8080; border-color:#ff5555; }
  .empty-state { display:flex; flex-direction:column; align-items:center; justify-content:center; padding:60px 20px; color:#444; }
  .empty-state-icon { font-size:48px; margin-bottom:12px; }
  ::-webkit-scrollbar { width:6px; }
  ::-webkit-scrollbar-track { background:#111; }
  ::-webkit-scrollbar-thumb { background:#333; border-radius:3px; }
  ::-webkit-scrollbar-thumb:hover { background:#555; }
  .path-presets { display:flex; flex-wrap:wrap; gap:6px; padding:10px 14px 4px; border-bottom:1px solid #1a1a1a; }
  .preset-btn { background:#181818; border:1px solid #2a2a2a; color:#888; padding:3px 10px; border-radius:12px; cursor:pointer; font-size:11px; transition:all 0.15s; }
  .preset-btn:hover { background:#400000; border-color:#8b3a3a; color:#f0c0c0; }
  .preset-label { color:#555; font-size:11px; align-self:center; margin-right:4px; }
</style>
</head>
<body>
<div class='container'>
  <div>
    <div class='section-title'>Package Files</div>
    <div class='toolbar'>
      <button class='btn btn-primary' onclick='window.chrome.webview.postMessage(""openFilePicker"")'>+ Add Files</button>
      <span style='color:#555;font-size:12px;margin-left:4px;'>or drag &amp; drop files below</span>
    </div>
  </div>
  <div class='drop-zone' id='dropZone'
    ondragover='event.preventDefault(); this.classList.add(""drag-over"");'
    ondragleave='this.classList.remove(""drag-over"");'
    ondrop='handleDrop(event)'>
    <div class='drop-zone-icon'>&#128194;</div>
    <div>Drop mod files here</div>
    <div class='drop-zone-hint'>Supports .yft .ytd .meta .xml .asi .dlc and more</div>
  </div>
  <div class='files-table'>");

            if (currentProject.Files.Count == 0)
            {
                sb.Append(@"<div class='empty-state'>
    <div class='empty-state-icon'>&#128230;</div>
    <div>No files added yet</div>
    <div style='font-size:12px;margin-top:6px;'>Add mod files to include in your OIV package</div>
  </div>");
            }
            else
            {
                sb.Append(@"<div class='path-presets'>
    <span class='preset-label'>Quick paths:</span>
    <span class='preset-btn' onclick='setPresetPath(""x64/models/cdimages/"")'>cdimages/</span>
    <span class='preset-btn' onclick='setPresetPath(""x64/levels/gta5/vehicles/"")'>vehicles/</span>
    <span class='preset-btn' onclick='setPresetPath(""x64/textures/"")'>textures/</span>
    <span class='preset-btn' onclick='setPresetPath(""mods/update/x64/dlcpacks/"")'>dlcpacks/</span>
    <span class='preset-btn' onclick='setPresetPath(""scripts/"")'>scripts/</span>
    <span class='preset-btn' onclick='setPresetPath(""plugins/"")'>plugins/</span>
    <span class='preset-btn' onclick='setPresetPath(""update/update.rpf/common/data/"")'>common/data/</span>
  </div>
  <table>
    <thead>
      <tr>
        <th>File Name</th>
        <th>Target Path in GTA 5</th>
        <th>Type</th>
        <th style='width:80px;'>Remove</th>
      </tr>
    </thead>
    <tbody>");

                foreach (var f in currentProject.Files)
                {
                    string safeName = f.FileName.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                    string safeSource = f.SourcePath.Replace("\"", "&quot;").Replace("'", "&#39;");
                    string safeTarget = f.TargetPath.Replace("\"", "&quot;").Replace("'", "&#39;");
                    string selContent = f.Type == "content" ? " selected" : "";
                    string selReplace = f.Type == "replace" ? " selected" : "";
                    string selXml = f.Type == "xmledit" ? " selected" : "";

                    sb.Append($@"<tr>
      <td class='td-name' title='{safeSource}'>{safeName}</td>
      <td class='td-path'>
        <input type='text' id='path_{f.Id}' value='{safeTarget}' placeholder='e.g. x64/models/cdimages/'
          onfocus='lastFocused={f.Id}'
          onchange='updatePath({f.Id}, this.value)' />
      </td>
      <td class='td-type'>
        <select onchange='updateType({f.Id}, this.value)'>
          <option value='content'{selContent}>Content</option>
          <option value='replace'{selReplace}>Replace</option>
          <option value='xmledit'{selXml}>XML Edit</option>
        </select>
      </td>
      <td class='td-remove'><button onclick='removeFile({f.Id})'>Remove</button></td>
    </tr>");
                }

                sb.Append("</tbody></table>");
            }

            sb.Append(@"</div>
</div>
<script>
var lastFocused = -1;
function handleDrop(e) {
  e.preventDefault();
  document.getElementById('dropZone').classList.remove('drag-over');
  window.chrome.webview.postMessage('openFilePicker');
}
function removeFile(id) {
  window.chrome.webview.postMessage('removeFile:' + id);
}
function updatePath(id, val) {
  window.chrome.webview.postMessage('updatePath:' + JSON.stringify({id: String(id), target: val}));
}
function updateType(id, val) {
  var pathEl = document.getElementById('path_' + id);
  var pathVal = pathEl ? pathEl.value : '';
  window.chrome.webview.postMessage('updatePath:' + JSON.stringify({id: String(id), target: pathVal, type: val}));
}
function setPresetPath(path) {
  if (lastFocused >= 0) {
    var el = document.getElementById('path_' + lastFocused);
    if (el) { el.value = path; updatePath(lastFocused, path); }
  }
}
</script>
</body>
</html>");

            return sb.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadPackageTab();
        }

        private void MenuButton_MouseEnter(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = Color.FromArgb(100, 0, 0);
                btn.ForeColor = Color.White;
            }
        }

        private void MenuButton_MouseLeave(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                btn.BackColor = Color.FromArgb(64, 0, 0);
                btn.ForeColor = Color.RosyBrown;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dropdownVersionTag.SelectedItem != null)
                currentProject.VersionTag = dropdownVersionTag.SelectedItem.ToString() ?? "Stable";
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (dropdownOIVSpec.SelectedItem != null)
                currentProject.OIVSpec = dropdownOIVSpec.SelectedItem.ToString() ?? "Stable";
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Maximized)
                this.WindowState = FormWindowState.Normal;
            else
                this.WindowState = FormWindowState.Maximized;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnShowSidebar_Click(object sender, EventArgs e)
        {
            OpenSidebar();
        }

        private void SidebarBtn1_Click(object sender, EventArgs e)
        {
            CloseSidebar();
        }

        private void OpenSidebar()
        {
            if (sidebarOpen) return;
            sidebarOpen = true;
            panelSidebar.Visible = true;
            sidebarTargetX = 0;
            timerSidebar.Start();
        }

        private void CloseSidebar()
        {
            if (!sidebarOpen) return;
            sidebarOpen = false;
            sidebarTargetX = -160;
            timerSidebar.Start();
        }

        private void timerSidebar_Tick(object sender, EventArgs e)
        {
            int current = panelSidebar.Left;
            int diff = sidebarTargetX - current;

            if (Math.Abs(diff) <= 2)
            {
                panelSidebar.Left = sidebarTargetX;
                timerSidebar.Stop();
                if (!sidebarOpen)
                    panelSidebar.Visible = false;
            }
            else
            {
                panelSidebar.Left = current + diff / 4;
            }
        }

        private void PanelDrag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = true;
                dragStart = e.Location;
            }
        }

        private void PanelDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point delta = new Point(e.X - dragStart.X, e.Y - dragStart.Y);
                this.Location = new Point(this.Left + delta.X, this.Top + delta.Y);
            }
        }

        private void PanelDrag_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void btnOpenP_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Open MagicOGK Project",
                Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string json = System.IO.File.ReadAllText(dlg.FileName);
                    var proj = System.Text.Json.JsonSerializer.Deserialize<OIVProject>(json);
                    if (proj != null)
                    {
                        currentProject = proj;
                        currentProjectPath = dlg.FileName;
                        txtboxModName.Text = proj.ModName;
                        txtboxAuthor.Text = proj.Author;
                        txtboxVersion.Text = proj.Version;
                        SetDropdownByValue(dropdownVersionTag, proj.VersionTag);
                        SetDropdownByValue(dropdownOIVSpec, proj.OIVSpec);
                        CloseSidebar();
                        RefreshCurrentTab();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to open project: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentProjectPath))
                SaveProjectAs();
            else
                SaveProject(currentProjectPath);
        }

        private void btnSaveAs_Click(object sender, EventArgs e)
        {
            SaveProjectAs();
        }

        private void SaveProjectAs()
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Save MagicOGK Project",
                Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*",
                FileName = string.IsNullOrWhiteSpace(txtboxModName.Text) ? "MyMod" : txtboxModName.Text
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                currentProjectPath = dlg.FileName;
                SaveProject(currentProjectPath);
            }
        }

        private void SaveProject(string path)
        {
            try
            {
                SyncMetadataToProject();
                string json = System.Text.Json.JsonSerializer.Serialize(currentProject, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(path, json);
                CloseSidebar();
                MessageBox.Show("Project saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBuildOIV_Click(object sender, EventArgs e)
        {
            SyncMetadataToProject();

            if (string.IsNullOrWhiteSpace(currentProject.ModName))
            {
                MessageBox.Show("Please enter a Mod Name before building.", "Missing Metadata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (currentProject.Files.Count == 0)
            {
                MessageBox.Show("No files added. Please add at least one file to package.", "No Files", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool missingPaths = currentProject.Files.Exists(f => string.IsNullOrWhiteSpace(f.TargetPath));
            if (missingPaths)
            {
                var result = MessageBox.Show(
                    "Some files have no target path set. They will be placed in the root of the package.\n\nContinue building?",
                    "Missing Paths",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (result == DialogResult.No) return;
            }

            using var dlg = new SaveFileDialog
            {
                Title = "Save OIV Package",
                Filter = "OIV Package (*.oiv)|*.oiv",
                FileName = currentProject.ModName.Replace(" ", "_") + ".oiv"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    OIVBuilder.Build(currentProject, dlg.FileName);
                    CloseSidebar();
                    MessageBox.Show($"OIV package built successfully!\n\n{dlg.FileName}", "Build Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Build failed: " + ex.Message, "Build Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnOpenOIV_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Open OIV Package",
                Filter = "OIV Package (*.oiv)|*.oiv|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dlg.FileName,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not open file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SyncMetadataToProject()
        {
            currentProject.ModName = txtboxModName.Text.Trim();
            currentProject.Author = txtboxAuthor.Text.Trim();
            currentProject.Version = txtboxVersion.Text.Trim();
            if (dropdownVersionTag.SelectedItem != null)
                currentProject.VersionTag = dropdownVersionTag.SelectedItem.ToString() ?? "Stable";
            if (dropdownOIVSpec.SelectedItem != null)
                currentProject.OIVSpec = dropdownOIVSpec.SelectedItem.ToString() ?? "Stable";
        }

        private void SetDropdownByValue(ComboBox cb, string value)
        {
            for (int i = 0; i < cb.Items.Count; i++)
            {
                if (cb.Items[i]?.ToString() == value)
                {
                    cb.SelectedIndex = i;
                    return;
                }
            }
        }
    }
}
