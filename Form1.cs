using System;
using System.Collections.Generic;
using System.Drawing;
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
        private Image? selectedPhoto = null;

        public main()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            panelDrag.MouseDown += PanelDrag_MouseDown;
            panelDrag.MouseMove += PanelDrag_MouseMove;
            panelDrag.MouseUp += PanelDrag_MouseUp;

            await webViewFileList.EnsureCoreWebView2Async();
            webViewFileList.CoreWebView2.Settings.IsZoomControlEnabled = false;
            webViewFileList.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            webViewFileList.CoreWebView2.Settings.AreDevToolsEnabled = false;
            webViewFileList.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            SetupSidebarHandlers();
            RefreshFilesList();
        }

        private void SetupSidebarHandlers()
        {
            btnSidebarToggle.Click += btnSidebarToggle_Click;
            btnSidebarOpenProject.Click += btnSidebarOpenProject_Click;
            btnSidebarOpenOIV.Click += btnSidebarOpenOIV_Click;
            btnSidebarSaveProjectAs.Click += btnSidebarSaveProjectAs_Click;
            btnSidebarBuildOIV.Click += btnSidebarBuildOIV_Click;
            btnSidebarFeedback.Click += btnSidebarFeedback_Click;

            sidebarTimer.Interval = 20;
            sidebarTimer.Tick += sidebarTimer_Tick;

            editorTimer.Interval = 20;
            editorTimer.Tick += editorTimer_Tick;
        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            if (sidebarExpanded)
            {
                if (panelSidebar.Width < 200)
                {
                    panelSidebar.Width += 15;
                    if (panelSidebar.Width > 200)
                        panelSidebar.Width = 200;
                }
                else
                {
                    sidebarTimer.Stop();
                }
            }
            else
            {
                if (panelSidebar.Width > 0)
                {
                    panelSidebar.Width -= 15;
                    if (panelSidebar.Width < 0)
                        panelSidebar.Width = 0;
                }
                else
                {
                    sidebarTimer.Stop();
                }
            }
        }

        private void editorTimer_Tick(object sender, EventArgs e)
        {
            if (editorExpanded)
            {
                if (panelEditorRight.Width < 350)
                {
                    panelEditorRight.Width += 15;
                    if (panelEditorRight.Width > 350)
                        panelEditorRight.Width = 350;
                }
                else
                {
                    editorTimer.Stop();
                }
            }
            else
            {
                if (panelEditorRight.Width > 0)
                {
                    panelEditorRight.Width -= 15;
                    if (panelEditorRight.Width < 0)
                        panelEditorRight.Width = 0;
                }
                else
                {
                    editorTimer.Stop();
                }
            }
        }

        private void btnSidebarToggle_Click(object sender, EventArgs e)
        {
            sidebarExpanded = !sidebarExpanded;
            sidebarTimer.Start();
        }

        private void btnSidebarOpenProject_Click(object sender, EventArgs e)
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
                    string json = File.ReadAllText(dlg.FileName);
                    var proj = System.Text.Json.JsonSerializer.Deserialize<OIVProject>(json);
                    if (proj != null)
                    {
                        currentProject = proj;
                        currentProjectPath = dlg.FileName;
                        LoadProjectIntoUI();
                        RefreshFilesList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to open project: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSidebarOpenOIV_Click(object sender, EventArgs e)
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

        private void btnSidebarSaveProjectAs_Click(object sender, EventArgs e)
        {
            using var dlg = new SaveFileDialog
            {
                Title = "Save MagicOGK Project As",
                Filter = "MagicOGK Project (*.mogk)|*.mogk|All Files (*.*)|*.*",
                FileName = string.IsNullOrWhiteSpace(currentProject.ModName)
                    ? "NewProject.mogk"
                    : currentProject.ModName.Replace(" ", "_") + ".mogk"
            };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SyncUIToProject();
                    string json = System.Text.Json.JsonSerializer.Serialize(
                        currentProject,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                    File.WriteAllText(dlg.FileName, json);
                    currentProjectPath = dlg.FileName;

                    MessageBox.Show("Project saved successfully.", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save project: " + ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSidebarBuildOIV_Click(object sender, EventArgs e)
        {
            btnBuildOIV_Click(sender, e);
        }

        private void btnSidebarFeedback_Click(object sender, EventArgs e)
        {
            string feedbackUrl = "https://forms.gle/your-placeholder-link";

            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = feedbackUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open feedback link: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string? rawMsg = e.TryGetWebMessageAsString();
            if (rawMsg == null) return;
            string msg = rawMsg;

            if (msg.StartsWith("removeFile:"))
            {
                if (int.TryParse(msg.Substring(11), out int id))
                {
                    currentProject.Files.RemoveAll(f => f.Id == id);
                    RefreshFilesList();
                }
            }
            else if (msg.StartsWith("updatePath:"))
            {
                try
                {
                    var data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(msg.Substring(11));
                    if (data != null && int.TryParse(data.Get("id"), out int id))
                    {
                        var file = currentProject.Files.Find(f => f.Id == id);
                        if (file != null && data.ContainsKey("target"))
                            file.TargetPath = data["target"];
                    }
                }
                catch { }
            }
        }

        private void btnAddFiles_Click(object sender, EventArgs e)
        {
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
                    string name = Path.GetFileName(path);
                    currentProject.Files.Add(new OIVFileEntry
                    {
                        Id = currentProject.NextId++,
                        SourcePath = path,
                        TargetPath = "",
                        FileName = name,
                        Type = "content"
                    });
                }
                RefreshFilesList();
            }
        }

        private void PanelDropZone_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            panelDropZone.BorderStyle = BorderStyle.FixedSingle;
        }

        private void PanelDropZone_DragDrop(object sender, DragEventArgs e)
        {
            panelDropZone.BorderStyle = BorderStyle.FixedSingle;
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null)
                {
                    foreach (string path in files)
                    {
                        if (File.Exists(path))
                        {
                            string name = Path.GetFileName(path);
                            currentProject.Files.Add(new OIVFileEntry
                            {
                                Id = currentProject.NextId++,
                                SourcePath = path,
                                TargetPath = "",
                                FileName = name,
                                Type = "content"
                            });
                        }
                    }
                    RefreshFilesList();
                }
            }
        }

        private void btnOpenEditor_Click(object sender, EventArgs e)
        {
            editorExpanded = !editorExpanded;
            if (editorExpanded)
                BuildEditorPanel();
            editorTimer.Start();
        }

        private void BuildEditorPanel()
        {
            panelEditorRight.Controls.Clear();

            Label lblTitle = new Label
            {
                Text = "FILE EDITOR",
                ForeColor = Color.RosyBrown,
                Font = new Font("Syne", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20)
            };
            panelEditorRight.Controls.Add(lblTitle);

            int yPos = 60;
            foreach (var file in currentProject.Files)
            {
                var panel = new Panel
                {
                    BackColor = Color.FromArgb(30, 30, 30),
                    Size = new Size(310, 100),
                    Location = new Point(20, yPos),
                    BorderStyle = BorderStyle.FixedSingle
                };

                var lblName = new Label
                {
                    Text = file.FileName,
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(10, 10),
                    Font = new Font("Syne", 9F, FontStyle.Bold)
                };
                panel.Controls.Add(lblName);

                var lblPath = new Label
                {
                    Text = "Path:",
                    ForeColor = Color.RosyBrown,
                    AutoSize = true,
                    Location = new Point(10, 35),
                    Font = new Font("Syne", 8F)
                };
                panel.Controls.Add(lblPath);

                var txtPath = new TextBox
                {
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Text = file.TargetPath,
                    Size = new Size(290, 23),
                    Location = new Point(10, 53),
                    Font = new Font("Syne", 8F)
                };
                txtPath.TextChanged += (s, e) =>
                {
                    file.TargetPath = txtPath.Text;
                };
                panel.Controls.Add(txtPath);

                var btnRemove = new Button
                {
                    BackColor = Color.FromArgb(139, 30, 30),
                    ForeColor = Color.FromArgb(240, 192, 192),
                    FlatStyle = FlatStyle.Flat,
                    Text = "Remove",
                    Size = new Size(80, 22),
                    Location = new Point(230, 73),
                    Font = new Font("Syne", 7F)
                };
                int fileId = file.Id;
                btnRemove.Click += (s, e) =>
                {
                    currentProject.Files.RemoveAll(f => f.Id == fileId);
                    BuildEditorPanel();
                    RefreshFilesList();
                };
                panel.Controls.Add(btnRemove);

                panelEditorRight.Controls.Add(panel);
                yPos += 110;
            }
        }

        private void RefreshFilesList()
        {
            SyncUIToProject();

            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html><head><meta charset='utf-8'>
<style>
  body { margin:0; padding:15px; background:#0d0d0d; color:#ccc; font-family:Arial,sans-serif; font-size:12px; }
  table { width:100%; border-collapse:collapse; }
  thead th { background:#1a1a1a; color:#bc8f8f; padding:8px; text-align:left; border-bottom:1px solid #222; }
  tbody tr { border-bottom:1px solid #1a1a1a; }
  tbody tr:hover { background:#161616; }
  td { padding:6px 8px; }
  input { background:#0d0d0d; border:1px solid #2a2a2a; color:#ccc; padding:4px; width:100%; box-sizing:border-box; }
  input:focus { outline:none; border-color:#8b3a3a; }
  button { background:#400000; border:1px solid #8b3a3a; color:#f0c0c0; padding:3px 8px; cursor:pointer; font-size:11px; }
  button:hover { background:#600000; }
  .empty { text-align:center; padding:40px; color:#555; }
</style>
</head><body>");

            if (currentProject.Files.Count == 0)
            {
                sb.Append("<div class='empty'>No files added yet. Click Add Files to get started.</div>");
            }
            else
            {
                sb.Append("<table><thead><tr><th>File</th><th>Target Path</th><th>Action</th></tr></thead><tbody>");
                foreach (var file in currentProject.Files)
                {
                    string safeName = System.Net.WebUtility.HtmlEncode(file.FileName);
                    string safePath = System.Net.WebUtility.HtmlEncode(file.TargetPath);
                    sb.Append($@"<tr>
      <td>{safeName}</td>
      <td><input type='text' value='{safePath}' onchange='updatePath({file.Id}, this.value)' /></td>
      <td><button onclick='removeFile({file.Id})'>Remove</button></td>
    </tr>");
                }
                sb.Append("</tbody></table>");
            }

            sb.Append(@"</body></html>");

            webViewFileList.CoreWebView2.NavigateToString(sb.ToString());
        }

        private void btnAddPhoto_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select photo",
                Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    selectedPhoto = Image.FromFile(dlg.FileName);
                    btnAddPhoto.Text = "CHANGE PHOTO";
                    btnAddPhoto.BackColor = Color.FromArgb(70, 50, 50);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void panelColorPicker_Click(object sender, EventArgs e)
        {
            using var dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                panelColorPicker.BackColor = dlg.Color;
            }
        }

        private void btnBuildOIV_Click(object sender, EventArgs e)
        {
            SyncUIToProject();

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
                    MessageBox.Show($"OIV package built successfully!\n\n{dlg.FileName}", "Build Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Build failed: " + ex.Message, "Build Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

        private void SyncUIToProject()
        {
            currentProject.ModName = txtModName.Text.Trim();
            currentProject.Author = txtAuthor.Text.Trim();
            currentProject.Version = txtVersion.Text.Trim();
            currentProject.Description = txtDescription.Text.Trim();
            if (dropdownVersionTag.SelectedItem != null)
                currentProject.VersionTag = dropdownVersionTag.SelectedItem.ToString() ?? "Stable";
        }

        private void LoadProjectIntoUI()
        {
            txtModName.Text = currentProject.ModName;
            txtAuthor.Text = currentProject.Author;
            txtVersion.Text = currentProject.Version;
            txtDescription.Text = currentProject.Description;

            for (int i = 0; i < dropdownVersionTag.Items.Count; i++)
            {
                if (dropdownVersionTag.Items[i]?.ToString() == currentProject.VersionTag)
                {
                    dropdownVersionTag.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    public static class DictExtensions
    {
        public static string Get(this Dictionary<string, string> dict, string key)
        {
            return dict.ContainsKey(key) ? dict[key] : "";
        }
    }
}
