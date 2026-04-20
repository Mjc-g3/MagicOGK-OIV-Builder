using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace MagicOGK_OIV_Builder
{
    public partial class main : Form
    {
        private bool isDragging = false;
        private Point dragStart;
        private string currentProjectPath = string.Empty;
        private OIVProject currentProject = new OIVProject();
        private bool sidebarExpanded = false;
        private bool editorExpanded = false;
        private string? selectedPhotoPath = null;
        private bool webViewReady = false;

        public main()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        // ─────────────────── INIT ───────────────────

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Drag
            panelDrag.MouseDown += PanelDrag_MouseDown;
            panelDrag.MouseMove += PanelDrag_MouseMove;
            panelDrag.MouseUp   += PanelDrag_MouseUp;

            // Drop zone
            panelDropZone.DragEnter += PanelDropZone_DragEnter;
            panelDropZone.DragDrop  += PanelDropZone_DragDrop;
            panelDropZone.AllowDrop  = true;

            // Button events
            btnAddFiles.Click        += btnAddFiles_Click;
            btnAddPhoto.Click        += btnAddPhoto_Click;
            btnOpenEditor.Click      += btnOpenEditor_Click;
            btnBuildOIV.Click        += btnBuildOIV_Click;
            panelColorPicker.Click   += panelColorPicker_Click;

            btnSidebarOpenProject.Click   += btnSidebarOpenProject_Click;
            btnSidebarSaveProjectAs.Click += btnSidebarSaveProjectAs_Click;
            btnSidebarOpenOIV.Click       += btnSidebarOpenOIV_Click;
            btnSidebarBuildOIV.Click      += btnBuildOIV_Click;
            btnSidebarFeedback.Click      += btnSidebarFeedback_Click;

            // Timers
            sidebarTimer.Interval = 12;
            sidebarTimer.Tick    += SidebarTimer_Tick;
            editorTimer.Interval  = 12;
            editorTimer.Tick     += EditorTimer_Tick;

            // Button hover effects
            AddButtonHover(btnOpenEditor);
            AddButtonHover(btnBuildOIV);
            AddButtonHover(btnAddFiles);
            AddButtonHover(btnAddPhoto);

            // Photo preview paint
            panelPhotoPreview.Paint += PanelPhotoPreview_Paint;

            // WebView
            await webViewFileList.EnsureCoreWebView2Async();
            webViewFileList.CoreWebView2.Settings.IsZoomControlEnabled        = false;
            webViewFileList.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webViewFileList.CoreWebView2.Settings.AreDevToolsEnabled           = false;
            webViewFileList.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            webViewReady = true;
            RenderFileList();
        }

        // ─────────────────── WINDOW CONTROLS ───────────────────

        private void button5_Click(object sender, EventArgs e) => Application.Exit();
        private void button6_Click(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized
                ? FormWindowState.Normal
                : FormWindowState.Maximized;
        }
        private void button7_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void PanelDrag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; dragStart = e.Location; }
        }
        private void PanelDrag_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
                Location = new Point(Left + e.X - dragStart.X, Top + e.Y - dragStart.Y);
        }
        private void PanelDrag_MouseUp(object sender, MouseEventArgs e) => isDragging = false;

        // ─────────────────── SIDEBAR ───────────────────

        private void btnHamburger_Click(object sender, EventArgs e)
        {
            sidebarExpanded = !sidebarExpanded;
            sidebarTimer.Start();
        }

        private int sidebarTarget => sidebarExpanded ? 200 : 0;
        private void SidebarTimer_Tick(object sender, EventArgs e)
        {
            int diff = sidebarTarget - panelSidebar.Width;
            if (Math.Abs(diff) <= 3)
            {
                panelSidebar.Width = sidebarTarget;
                sidebarTimer.Stop();
            }
            else
            {
                panelSidebar.Width += diff / 3;
            }
        }

        private void btnSidebarOpenProject_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title  = "Open MagicOGK Project",
                Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                var proj = System.Text.Json.JsonSerializer.Deserialize<OIVProject>(File.ReadAllText(dlg.FileName));
                if (proj != null)
                {
                    currentProject     = proj;
                    currentProjectPath = dlg.FileName;
                    LoadProjectIntoUI();
                    RenderFileList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to open project: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSidebarSaveProjectAs_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title    = "Save MagicOGK Project",
                Filter   = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*",
                FileName = string.IsNullOrWhiteSpace(currentProject.ModName) ? "MyMod" : currentProject.ModName
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                SyncUIToProject();
                File.WriteAllText(dlg.FileName,
                    System.Text.Json.JsonSerializer.Serialize(currentProject,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                currentProjectPath = dlg.FileName;
                MessageBox.Show("Project saved.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSidebarOpenOIV_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "OIV Package (*.oiv)|*.oiv|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    { FileName = dlg.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSidebarFeedback_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Feedback form coming soon!", "Feedback", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ─────────────────── RIGHT EDITOR PANEL ───────────────────

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            editorExpanded = !editorExpanded;
            if (editorExpanded)
                BuildEditorPanel();
            editorTimer.Start();
        }

        private int editorTarget => editorExpanded ? 380 : 0;
        private void EditorTimer_Tick(object sender, EventArgs e)
        {
            int diff = editorTarget - panelEditorRight.Width;
            if (Math.Abs(diff) <= 3)
            {
                panelEditorRight.Width = editorTarget;
                editorTimer.Stop();
            }
            else
            {
                panelEditorRight.Width += diff / 3;
            }
        }

        private void BuildEditorPanel()
        {
            panelEditorRight.Controls.Clear();

            // Header bar
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.FromArgb(28, 28, 28)
            };

            var lblTitle = new Label
            {
                Text      = "FILE EDITOR",
                ForeColor = Color.FromArgb(188, 143, 143),
                Font      = new Font("Syne", 10F, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(16, 14)
            };
            header.Controls.Add(lblTitle);

            var btnClose = new Button
            {
                Text      = "\u2715",
                ForeColor = Color.FromArgb(180, 80, 80),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10F),
                Size      = new Size(32, 32),
                Location  = new Point(336, 8),
                TabStop   = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, ev) =>
            {
                editorExpanded = false;
                editorTimer.Start();
            };
            header.Controls.Add(btnClose);
            panelEditorRight.Controls.Add(header);

            // Quick path presets
            var presetPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 70,
                BackColor = Color.FromArgb(22, 22, 22),
                Padding   = new Padding(12, 8, 12, 4)
            };
            var presetLabel = new Label
            {
                Text      = "QUICK PATHS",
                ForeColor = Color.FromArgb(120, 80, 80),
                Font      = new Font("Syne", 7F, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(12, 6)
            };
            presetPanel.Controls.Add(presetLabel);

            string[] presets = { "x64/models/cdimages/", "scripts/", "plugins/", "update/update.rpf/", "mods/update/x64/dlcpacks/" };
            int px = 12, py = 24;
            foreach (var preset in presets)
            {
                var p = preset;
                var chip = new Label
                {
                    Text      = p.TrimEnd('/').Split('/')[^1],
                    ForeColor = Color.FromArgb(150, 100, 100),
                    BackColor = Color.FromArgb(40, 15, 15),
                    Font      = new Font("Syne", 7F),
                    AutoSize  = true,
                    Padding   = new Padding(5, 2, 5, 2),
                    Location  = new Point(px, py),
                    Cursor    = Cursors.Hand,
                    Tag       = p
                };
                chip.MouseEnter += (s, ev) => ((Label)s!).BackColor = Color.FromArgb(80, 20, 20);
                chip.MouseLeave += (s, ev) => ((Label)s!).BackColor = Color.FromArgb(40, 15, 15);
                chip.Click += (s, ev) => ApplyPresetToLastFocused(p);
                presetPanel.Controls.Add(chip);
                px += chip.PreferredWidth + 12;
                if (px > 330) { px = 12; py += 22; }
            }
            panelEditorRight.Controls.Add(presetPanel);

            // Scroll container for file cards
            var scrollPanel = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.FromArgb(18, 18, 18),
                Padding   = new Padding(12)
            };

            int yPos = 8;
            if (currentProject.Files.Count == 0)
            {
                scrollPanel.Controls.Add(new Label
                {
                    Text      = "No files added yet.\nAdd files using the + Add Files button.",
                    ForeColor = Color.FromArgb(90, 90, 90),
                    Font      = new Font("Syne", 9F),
                    Size      = new Size(340, 60),
                    Location  = new Point(12, 20),
                    TextAlign = ContentAlignment.MiddleCenter
                });
            }

            foreach (var file in currentProject.Files)
            {
                var card = new Panel
                {
                    BackColor   = Color.FromArgb(28, 28, 28),
                    BorderStyle = BorderStyle.FixedSingle,
                    Size        = new Size(340, 115),
                    Location    = new Point(8, yPos)
                };

                // file name
                card.Controls.Add(new Label
                {
                    Text      = file.FileName,
                    ForeColor = Color.FromArgb(220, 220, 220),
                    Font      = new Font("Syne", 9F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 10)
                });

                // source path hint
                card.Controls.Add(new Label
                {
                    Text      = TruncatePath(file.SourcePath, 45),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Font      = new Font("Consolas", 7F),
                    AutoSize  = true,
                    Location  = new Point(10, 30)
                });

                // target label
                card.Controls.Add(new Label
                {
                    Text      = "TARGET PATH",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 50)
                });

                var txtPath = new TextBox
                {
                    BackColor       = Color.Black,
                    ForeColor       = Color.FromArgb(200, 200, 200),
                    BorderStyle     = BorderStyle.FixedSingle,
                    Text            = file.TargetPath,
                    PlaceholderText = "e.g. x64/models/cdimages/",
                    Size            = new Size(320, 22),
                    Location        = new Point(10, 65),
                    Font            = new Font("Consolas", 8F),
                    Tag             = file.Id
                };
                txtPath.TextChanged += (s, ev) =>
                {
                    file.TargetPath = ((TextBox)s!).Text;
                    RenderFileList();
                };
                txtPath.GotFocus += (s, ev) => { lastFocusedPathBox = (TextBox)s!; };
                card.Controls.Add(txtPath);

                // type dropdown
                var typeBox = new ComboBox
                {
                    BackColor     = Color.Black,
                    ForeColor     = Color.FromArgb(200, 200, 200),
                    FlatStyle     = FlatStyle.Flat,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Size          = new Size(100, 22),
                    Location      = new Point(10, 89),
                    Font          = new Font("Syne", 7F)
                };
                typeBox.Items.AddRange(new object[] { "Content", "Replace", "XML Edit" });
                typeBox.SelectedIndex = file.Type switch { "replace" => 1, "xmledit" => 2, _ => 0 };
                typeBox.SelectedIndexChanged += (s, ev) =>
                {
                    file.Type = typeBox.SelectedIndex switch { 1 => "replace", 2 => "xmledit", _ => "content" };
                };
                card.Controls.Add(typeBox);

                // remove button
                var btnRem = new Button
                {
                    Text      = "Remove",
                    BackColor = Color.FromArgb(60, 0, 0),
                    ForeColor = Color.FromArgb(220, 130, 130),
                    FlatStyle = FlatStyle.Flat,
                    Size      = new Size(70, 22),
                    Location  = new Point(268, 89),
                    Font      = new Font("Syne", 7F),
                    TabStop   = false
                };
                btnRem.FlatAppearance.BorderSize = 0;
                int fid = file.Id;
                btnRem.Click += (s, ev) =>
                {
                    currentProject.Files.RemoveAll(f => f.Id == fid);
                    BuildEditorPanel();
                    RenderFileList();
                };
                card.Controls.Add(btnRem);

                scrollPanel.Controls.Add(card);
                yPos += 123;
            }

            panelEditorRight.Controls.Add(scrollPanel);
        }

        private TextBox? lastFocusedPathBox = null;
        private void ApplyPresetToLastFocused(string path)
        {
            if (lastFocusedPathBox != null)
                lastFocusedPathBox.Text = path;
        }

        // ─────────────────── FILE LIST (WebView) ───────────────────

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string? rawMsg = e.TryGetWebMessageAsString();
            if (rawMsg == null) return;

            if (rawMsg.StartsWith("remove:") && int.TryParse(rawMsg.Substring(7), out int removeId))
            {
                currentProject.Files.RemoveAll(f => f.Id == removeId);
                if (editorExpanded) BuildEditorPanel();
                RenderFileList();
            }
            else if (rawMsg.StartsWith("path:"))
            {
                try
                {
                    var d = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(rawMsg.Substring(5));
                    if (d != null && d.ContainsKey("id") && d.ContainsKey("val")
                        && int.TryParse(d["id"], out int pid))
                    {
                        var f = currentProject.Files.Find(x => x.Id == pid);
                        if (f != null) f.TargetPath = d["val"];
                    }
                }
                catch { }
            }
        }

        private void RenderFileList()
        {
            if (!webViewReady) return;

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html><html><head><meta charset='utf-8'>
<style>
*{box-sizing:border-box;margin:0;padding:0}
body{background:#0d0d0d;color:#ccc;font-family:'Segoe UI',Arial,sans-serif;font-size:12px;padding:0;height:100vh;display:flex;flex-direction:column}
.toolbar{display:flex;align-items:center;gap:10px;padding:10px 16px;background:#111;border-bottom:1px solid #1e1e1e}
.file-count{color:#666;font-size:11px}
.table-wrap{flex:1;overflow-y:auto}
table{width:100%;border-collapse:collapse}
thead th{background:#141414;color:#9e6060;font-size:10px;letter-spacing:1px;text-transform:uppercase;padding:9px 12px;text-align:left;border-bottom:1px solid #1e1e1e;position:sticky;top:0}
tbody tr{border-bottom:1px solid #161616;transition:background .1s}
tbody tr:hover{background:#141414}
td{padding:7px 12px;vertical-align:middle}
.fn{color:#e0e0e0;font-weight:600;font-size:12px;max-width:180px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
.tp input{background:#0a0a0a;border:1px solid #222;color:#bbb;padding:4px 7px;width:100%;font-family:Consolas,monospace;font-size:11px;border-radius:2px}
.tp input:focus{outline:none;border-color:#7a2a2a;color:#eee}
.rm button{background:#3a0000;border:1px solid #5a1a1a;color:#c08080;padding:3px 9px;cursor:pointer;font-size:10px;border-radius:2px}
.rm button:hover{background:#5a0000;color:#f0c0c0}
.empty{display:flex;flex-direction:column;align-items:center;justify-content:center;height:100%;color:#444;gap:10px;padding:40px}
.empty-icon{font-size:40px}
::-webkit-scrollbar{width:5px}
::-webkit-scrollbar-track{background:#0d0d0d}
::-webkit-scrollbar-thumb{background:#2a2a2a;border-radius:3px}
</style></head><body>");

            if (currentProject.Files.Count == 0)
            {
                sb.Append(@"<div class='empty'>
  <div class='empty-icon'>&#128230;</div>
  <div>No files added yet</div>
  <div style='font-size:11px;color:#333'>Add mod files using the button above or drag &amp; drop</div>
</div>");
            }
            else
            {
                sb.Append($"<div class='toolbar'><span class='file-count'>{currentProject.Files.Count} file(s) added</span></div>");
                sb.Append("<div class='table-wrap'><table><thead><tr><th>File Name</th><th>Target Path in GTA 5</th><th style='width:70px'>Remove</th></tr></thead><tbody>");

                foreach (var file in currentProject.Files)
                {
                    string name  = System.Net.WebUtility.HtmlEncode(file.FileName);
                    string src   = System.Net.WebUtility.HtmlEncode(file.SourcePath);
                    string path  = System.Net.WebUtility.HtmlEncode(file.TargetPath).Replace("'", "&#39;");
                    sb.Append($@"<tr>
<td class='fn' title='{System.Net.WebUtility.HtmlEncode(file.SourcePath)}'>{name}</td>
<td class='tp'><input type='text' value='{path}' placeholder='e.g. x64/models/cdimages/' onchange='sendPath({file.Id},this.value)'/></td>
<td class='rm'><button onclick='sendRemove({file.Id})'>Remove</button></td>
</tr>");
                }

                sb.Append("</tbody></table></div>");
            }

            sb.Append(@"<script>
function sendRemove(id){window.chrome.webview.postMessage('remove:'+id);}
function sendPath(id,val){window.chrome.webview.postMessage('path:'+JSON.stringify({id:String(id),val:val}));}
</script></body></html>");

            webViewFileList.CoreWebView2.NavigateToString(sb.ToString());
        }

        // ─────────────────── FILE ADDING ───────────────────

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title      = "Select mod file(s)",
                Multiselect = true,
                Filter     = "All Files (*.*)|*.*|YFT (*.yft)|*.yft|YTD (*.ytd)|*.ytd|META (*.meta)|*.meta|XML (*.xml)|*.xml|ASI (*.asi)|*.asi"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            foreach (string path in dlg.FileNames)
                AddFile(path);
            RenderFileList();
            if (editorExpanded) BuildEditorPanel();
        }

        private void PanelDropZone_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            panelDropZone.BackColor = Color.FromArgb(30, 10, 10);
        }

        private void PanelDropZone_DragDrop(object sender, DragEventArgs e)
        {
            panelDropZone.BackColor = Color.FromArgb(17, 17, 17);
            if (e.Data == null) return;
            string[]? paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null) return;
            foreach (string path in paths)
                if (File.Exists(path)) AddFile(path);
            RenderFileList();
            if (editorExpanded) BuildEditorPanel();
        }

        private void AddFile(string path)
        {
            currentProject.Files.Add(new OIVFileEntry
            {
                Id         = currentProject.NextId++,
                SourcePath = path,
                FileName   = Path.GetFileName(path),
                TargetPath = "",
                Type       = "content"
            });
        }

        // ─────────────────── PHOTO PREVIEW ───────────────────

        private void btnAddPhoto_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title  = "Select preview image",
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            try
            {
                selectedPhotoPath = dlg.FileName;
                btnAddPhoto.Text  = "CHANGE";
                panelPhotoPreview.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PanelPhotoPreview_Paint(object sender, PaintEventArgs e)
        {
            if (selectedPhotoPath != null)
            {
                try
                {
                    using var img = Image.FromFile(selectedPhotoPath);
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.DrawImage(img, 0, 0, panelPhotoPreview.Width, panelPhotoPreview.Height);
                    return;
                }
                catch { }
            }
            // placeholder
            using var font  = new Font("Syne", 8F);
            using var brush = new SolidBrush(Color.FromArgb(70, 70, 70));
            var rect = panelPhotoPreview.ClientRectangle;
            e.Graphics.DrawString("No photo", font, brush, rect,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        // ─────────────────── COLOR PICKER ───────────────────

        private void panelColorPicker_Click(object sender, EventArgs e)
        {
            using var dlg = new ColorDialog { Color = panelColorPicker.BackColor };
            if (dlg.ShowDialog() == DialogResult.OK)
                panelColorPicker.BackColor = dlg.Color;
        }

        // ─────────────────── BUILD OIV ───────────────────

        private void btnBuildOIV_Click(object sender, EventArgs e)
        {
            SyncUIToProject();

            if (string.IsNullOrWhiteSpace(currentProject.ModName))
            {
                MessageBox.Show("Please enter a Mod Name before building.", "Missing Metadata",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (currentProject.Files.Count == 0)
            {
                MessageBox.Show("No files added. Add at least one file.", "No Files",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new SaveFileDialog
            {
                Title    = "Save OIV Package",
                Filter   = "OIV Package (*.oiv)|*.oiv",
                FileName = currentProject.ModName.Replace(" ", "_") + ".oiv"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            try
            {
                OIVBuilder.Build(currentProject, dlg.FileName);
                MessageBox.Show($"OIV package built successfully!\n\n{dlg.FileName}",
                    "Build Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Build failed: " + ex.Message, "Build Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─────────────────── PROJECT SYNC ───────────────────

        private void SyncUIToProject()
        {
            currentProject.ModName     = txtModName.Text.Trim();
            currentProject.Author      = txtAuthor.Text.Trim();
            currentProject.Version     = txtVersion.Text.Trim();
            currentProject.Description = txtDescription.Text.Trim();
            if (dropdownVersionTag.SelectedItem != null)
                currentProject.VersionTag = dropdownVersionTag.SelectedItem.ToString() ?? "Stable";

            // Store banner color as hex string (RRGGBB)
            var c = panelColorPicker.BackColor;
            currentProject.BannerColor = $"{c.R:X2}{c.G:X2}{c.B:X2}";

            if (selectedPhotoPath != null)
                currentProject.PhotoPath = selectedPhotoPath;
        }

        private void LoadProjectIntoUI()
        {
            txtModName.Text     = currentProject.ModName;
            txtAuthor.Text      = currentProject.Author;
            txtVersion.Text     = currentProject.Version;
            txtDescription.Text = currentProject.Description;

            for (int i = 0; i < dropdownVersionTag.Items.Count; i++)
            {
                if (dropdownVersionTag.Items[i]?.ToString() == currentProject.VersionTag)
                {
                    dropdownVersionTag.SelectedIndex = i;
                    break;
                }
            }

            // Restore banner color
            if (!string.IsNullOrWhiteSpace(currentProject.BannerColor))
            {
                try
                {
                    string hex = currentProject.BannerColor.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                        int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                        int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                        panelColorPicker.BackColor = Color.FromArgb(r, g, b);
                    }
                }
                catch { }
            }

            // Restore photo
            if (!string.IsNullOrWhiteSpace(currentProject.PhotoPath) && File.Exists(currentProject.PhotoPath))
            {
                selectedPhotoPath = currentProject.PhotoPath;
                btnAddPhoto.Text  = "CHANGE";
                panelPhotoPreview.Invalidate();
            }
        }

        // ─────────────────── HOVER HELPERS ───────────────────

        private void AddButtonHover(Button btn)
        {
            var normal = btn.BackColor;
            var bright = ControlPaint.Light(normal, 0.15f);
            btn.MouseEnter += (s, e) => btn.BackColor = bright;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private static string TruncatePath(string path, int maxLen)
        {
            if (path.Length <= maxLen) return path;
            return "..." + path.Substring(path.Length - maxLen + 3);
        }
    }
}
