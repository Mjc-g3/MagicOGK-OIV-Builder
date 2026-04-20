using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
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

        // ── Tree view state ───────────────────────────────────────────────────
        private TreeView?  editorTree      = null;
        private Panel?     editorPropPanel = null;

        private void BuildEditorPanel()
        {
            panelEditorRight.Controls.Clear();
            editorTree      = null;
            editorPropPanel = null;

            // ── Header ────────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.FromArgb(24, 24, 24)
            };
            header.Controls.Add(new Label
            {
                Text      = "FILE EDITOR",
                ForeColor = Color.FromArgb(188, 143, 143),
                Font      = new Font("Syne", 10F, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(14, 13)
            });
            var btnClose = new Button
            {
                Text      = "\u2715",
                ForeColor = Color.FromArgb(180, 80, 80),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10F),
                Size      = new Size(32, 32),
                Location  = new Point(344, 6),
                TabStop   = false
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, ev) => { editorExpanded = false; editorTimer.Start(); };
            header.Controls.Add(btnClose);
            panelEditorRight.Controls.Add(header);

            // ── Tree toolbar ──────────────────────────────────────────────────
            var toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            // Toolbar icon buttons: new folder | new RPF | add file | rename | delete
            (string text, string tip, Action act)[] toolActions = {
                ("\uD83D\uDCC1", "New Folder",     () => TreeCmd_NewFolder()),
                ("\uD83D\uDDC4",  "New RPF Archive",() => TreeCmd_NewRpf()),
                ("\uD83D\uDCC4", "Add File Here",  () => TreeCmd_AddFile()),
                ("\u270F",       "Rename",          () => TreeCmd_Rename()),
                ("\uD83D\uDDD1", "Delete",          () => TreeCmd_Delete()),
            };
            int tbx = 6;
            foreach (var (text, tip, act) in toolActions)
            {
                var a = act;
                var btn = new Button
                {
                    Text      = text,
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(170, 120, 120),
                    FlatStyle = FlatStyle.Flat,
                    Size      = new Size(32, 28),
                    Location  = new Point(tbx, 4),
                    Font      = new Font("Segoe UI Emoji", 11F),
                    TabStop   = false
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 30, 30);
                var tooltip = new ToolTip();
                tooltip.SetToolTip(btn, tip);
                btn.Click += (s, ev) => a();
                toolbar.Controls.Add(btn);
                tbx += 34;
            }
            panelEditorRight.Controls.Add(toolbar);

            // ── Properties panel (docked bottom, shown when node selected) ────
            editorPropPanel = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 0,
                BackColor = Color.FromArgb(20, 10, 10)
            };
            panelEditorRight.Controls.Add(editorPropPanel);

            // ── TreeView ──────────────────────────────────────────────────────
            editorTree = new TreeView
            {
                Dock            = DockStyle.Fill,
                BackColor       = Color.FromArgb(15, 15, 15),
                ForeColor       = Color.FromArgb(210, 180, 180),
                BorderStyle     = BorderStyle.None,
                Font            = new Font("Segoe UI", 9.5F),
                ItemHeight      = 22,
                ShowLines       = true,
                ShowPlusMinus   = true,
                ShowRootLines   = true,
                FullRowSelect   = true,
                HideSelection   = false,
                LineColor       = Color.FromArgb(70, 50, 50),
                DrawMode        = TreeViewDrawMode.OwnerDrawAll,
                Indent          = 18
            };
            editorTree.DrawNode          += EditorTree_DrawNode;
            editorTree.AfterSelect       += EditorTree_AfterSelect;
            editorTree.KeyDown           += EditorTree_KeyDown;
            editorTree.MouseDoubleClick  += EditorTree_MouseDoubleClick;
            editorTree.DragEnter         += (s, ev) => ev.Effect = DragDropEffects.Copy;
            editorTree.DragDrop          += EditorTree_DragDrop;
            editorTree.AllowDrop          = true;
            panelEditorRight.Controls.Add(editorTree);

            RebuildTree();
        }

        // ─── Tree building ────────────────────────────────────────────────────

        private void RebuildTree()
        {
            if (editorTree == null) return;
            editorTree.BeginUpdate();
            editorTree.Nodes.Clear();

            var root = new TreeNode("Root") { Tag = "root" };
            root.ForeColor = Color.FromArgb(220, 170, 170);

            // Recursively add folder nodes and their files
            AddFolderNodes(root, parentId: null);

            // Unassigned files go under root directly
            foreach (var file in currentProject.Files.Where(f => !f.FolderId.HasValue))
                root.Nodes.Add(MakeFileNode(file));

            editorTree.Nodes.Add(root);
            root.Expand();
            editorTree.EndUpdate();
        }

        private void AddFolderNodes(TreeNode parent, int? parentId)
        {
            foreach (var folder in currentProject.Folders.Where(f => f.ParentId == parentId))
            {
                var node = MakeFolderNode(folder);
                // Recurse into children
                AddFolderNodes(node, folder.Id);
                // Files assigned to this folder
                foreach (var file in currentProject.Files.Where(f => f.FolderId == folder.Id))
                    node.Nodes.Add(MakeFileNode(file));
                parent.Nodes.Add(node);
            }
        }

        private TreeNode MakeFolderNode(OIVFolder folder)
        {
            string icon = folder.IsRpf ? "\uD83D\uDDC4 " : "\uD83D\uDCC1 ";
            var node = new TreeNode(icon + folder.Name)
            {
                Tag       = folder,
                ForeColor = folder.IsRpf
                    ? Color.FromArgb(150, 200, 255)
                    : Color.FromArgb(220, 160, 160)
            };
            return node;
        }

        private TreeNode MakeFileNode(OIVFileEntry file)
        {
            return new TreeNode("\uD83D\uDCC4 " + file.FileName)
            {
                Tag       = file,
                ForeColor = Color.FromArgb(180, 180, 180)
            };
        }

        // ─── Tree drawing ─────────────────────────────────────────────────────

        private void EditorTree_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            bool selected = (e.State & TreeNodeStates.Selected) != 0;
            var bounds    = e.Bounds;
            if (bounds.Width == 0 && bounds.Height == 0) { e.DrawDefault = true; return; }

            // Background
            e.Graphics.FillRectangle(
                new SolidBrush(selected ? Color.FromArgb(70, 30, 30) : Color.FromArgb(15, 15, 15)),
                new Rectangle(0, bounds.Y, editorTree!.Width, bounds.Height));

            // Indent lines — drawn by WinForms when ShowLines=true, we just handle colours
            // Text
            var textBrush = new SolidBrush(e.Node?.ForeColor ?? Color.FromArgb(200, 200, 200));
            if (e.Node != null)
                e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont ?? editorTree.Font,
                    textBrush, bounds.X + 2, bounds.Y + 2);
        }

        // ─── Tree selection → properties panel ───────────────────────────────

        private void EditorTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (editorPropPanel == null || e.Node?.Tag == null) return;
            ShowPropertiesFor(e.Node.Tag);
        }

        private void ShowPropertiesFor(object tag)
        {
            if (editorPropPanel == null) return;
            editorPropPanel.Controls.Clear();

            if (tag is OIVFolder folder)
            {
                editorPropPanel.Height = 95;

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = folder.IsRpf ? "RPF ARCHIVE" : "FOLDER",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 8)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "NAME:",
                    ForeColor = Color.FromArgb(120, 80, 80),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 26)
                });
                var txtName = new TextBox
                {
                    BackColor   = Color.Black,
                    ForeColor   = Color.FromArgb(210, 210, 210),
                    BorderStyle = BorderStyle.FixedSingle,
                    Text        = folder.Name,
                    Size        = new Size(220, 20),
                    Location    = new Point(56, 23),
                    Font        = new Font("Consolas", 8F)
                };
                txtName.TextChanged += (s, ev) =>
                {
                    folder.Name = txtName.Text;
                    RebuildTree();
                    RenderFileList();
                };
                editorPropPanel.Controls.Add(txtName);

                var chkDlc = new CheckBox
                {
                    Text      = "Add to dlclist.xml on install",
                    ForeColor = Color.FromArgb(170, 130, 130),
                    BackColor = Color.Transparent,
                    Font      = new Font("Syne", 7F),
                    Checked   = folder.AddToDlcList,
                    AutoSize  = true,
                    Location  = new Point(10, 50)
                };
                chkDlc.CheckedChanged += (s, ev) => folder.AddToDlcList = chkDlc.Checked;
                editorPropPanel.Controls.Add(chkDlc);

                var chkRpf = new CheckBox
                {
                    Text      = "This is an RPF archive",
                    ForeColor = Color.FromArgb(130, 160, 200),
                    BackColor = Color.Transparent,
                    Font      = new Font("Syne", 7F),
                    Checked   = folder.IsRpf,
                    AutoSize  = true,
                    Location  = new Point(10, 70)
                };
                chkRpf.CheckedChanged += (s, ev) =>
                {
                    folder.IsRpf = chkRpf.Checked;
                    RebuildTree();
                };
                editorPropPanel.Controls.Add(chkRpf);
            }
            else if (tag is OIVFileEntry file)
            {
                editorPropPanel.Height = 95;

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "FILE",
                    ForeColor = Color.FromArgb(140, 90, 90),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 8)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = TruncatePath(file.SourcePath, 48),
                    ForeColor = Color.FromArgb(80, 80, 80),
                    Font      = new Font("Consolas", 7F),
                    AutoSize  = true,
                    Location  = new Point(10, 26)
                });

                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "TYPE:",
                    ForeColor = Color.FromArgb(120, 80, 80),
                    Font      = new Font("Syne", 7F, FontStyle.Bold),
                    AutoSize  = true,
                    Location  = new Point(10, 50)
                });

                var typeBox = new ComboBox
                {
                    BackColor     = Color.Black,
                    ForeColor     = Color.FromArgb(200, 200, 200),
                    FlatStyle     = FlatStyle.Flat,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Size          = new Size(110, 20),
                    Location      = new Point(48, 48),
                    Font          = new Font("Syne", 7F)
                };
                typeBox.Items.AddRange(new object[] { "Content", "Replace", "XML Edit" });
                typeBox.SelectedIndex = file.Type switch { "replace" => 1, "xmledit" => 2, _ => 0 };
                typeBox.SelectedIndexChanged += (s, ev) =>
                    file.Type = typeBox.SelectedIndex switch { 1 => "replace", 2 => "xmledit", _ => "content" };
                editorPropPanel.Controls.Add(typeBox);

                // Full resolved path preview
                string resolved = ResolveFilePath(file);
                editorPropPanel.Controls.Add(new Label
                {
                    Text      = "→ " + TruncatePath(resolved, 50),
                    ForeColor = Color.FromArgb(100, 130, 100),
                    Font      = new Font("Consolas", 7F),
                    AutoSize  = true,
                    Location  = new Point(10, 72)
                });
            }
            else
            {
                editorPropPanel.Height = 0;
            }
        }

        // Resolve the full install path for a file by walking up the folder tree
        private string ResolveFilePath(OIVFileEntry file)
        {
            var parts = new List<string> { file.FileName };
            int? cur = file.FolderId;
            while (cur.HasValue)
            {
                var folder = currentProject.Folders.Find(f => f.Id == cur.Value);
                if (folder == null) break;
                parts.Insert(0, folder.Name);
                cur = folder.ParentId;
            }
            return string.Join("\\", parts);
        }

        // ─── Toolbar commands ─────────────────────────────────────────────────

        private int? SelectedFolderId()
        {
            var tag = editorTree?.SelectedNode?.Tag;
            if (tag is OIVFolder f) return f.Id;
            if (tag is OIVFileEntry file) return file.FolderId;
            return null; // root or nothing selected
        }

        private void TreeCmd_NewFolder() => TreeCmd_NewNode(isRpf: false);
        private void TreeCmd_NewRpf()    => TreeCmd_NewNode(isRpf: true);

        private void TreeCmd_NewNode(bool isRpf)
        {
            string kind = isRpf ? "RPF archive" : "folder";
            string name = PromptName($"New {kind} name:", isRpf ? "myarchive.rpf" : "MyFolder");
            if (string.IsNullOrWhiteSpace(name)) return;

            currentProject.Folders.Add(new OIVFolder
            {
                Id       = currentProject.NextId++,
                Name     = name,
                ParentId = SelectedFolderId(),
                IsRpf    = isRpf
            });
            RebuildTree();
            RenderFileList();
        }

        private void TreeCmd_AddFile()
        {
            int? parentFolderId = SelectedFolderId();

            using var dlg = new OpenFileDialog
            {
                Title       = "Add file to selected folder",
                Multiselect = true,
                Filter      = "All Files (*.*)|*.*"
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;

            foreach (string path in dlg.FileNames)
            {
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id         = currentProject.NextId++,
                    SourcePath = path,
                    FileName   = Path.GetFileName(path),
                    SubPath    = string.Empty,
                    Type       = "content",
                    FolderId   = parentFolderId
                });
            }
            RebuildTree();
            RenderFileList();
        }

        private void TreeCmd_Rename()
        {
            var node = editorTree?.SelectedNode;
            if (node == null) return;

            if (node.Tag is OIVFolder folder)
            {
                string name = PromptName("Rename folder:", folder.Name);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    folder.Name = name;
                    RebuildTree();
                    RenderFileList();
                }
            }
            else if (node.Tag is OIVFileEntry)
            {
                MessageBox.Show("File names are determined by the source file.\nRe-add the file to change it.",
                    "Cannot Rename", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TreeCmd_Delete()
        {
            var node = editorTree?.SelectedNode;
            if (node == null) return;

            if (node.Tag is OIVFolder folder)
            {
                // Collect all descendant folder IDs
                var toRemove = new HashSet<int>();
                CollectDescendants(folder.Id, toRemove);
                toRemove.Add(folder.Id);

                // Unassign / remove files in those folders
                foreach (var file in currentProject.Files.ToList())
                {
                    if (file.FolderId.HasValue && toRemove.Contains(file.FolderId.Value))
                        currentProject.Files.Remove(file);
                }
                currentProject.Folders.RemoveAll(f => toRemove.Contains(f.Id));
            }
            else if (node.Tag is OIVFileEntry file)
            {
                int fid = file.Id;
                currentProject.Files.RemoveAll(f => f.Id == fid);
            }

            RebuildTree();
            RenderFileList();
        }

        private void CollectDescendants(int folderId, HashSet<int> set)
        {
            foreach (var child in currentProject.Folders.Where(f => f.ParentId == folderId))
            {
                set.Add(child.Id);
                CollectDescendants(child.Id, set);
            }
        }

        private void EditorTree_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete) TreeCmd_Delete();
            if (e.KeyCode == Keys.F2)     TreeCmd_Rename();
        }

        private void EditorTree_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            // Double-clicking a file node opens its source in Explorer
            if (editorTree?.SelectedNode?.Tag is OIVFileEntry file && File.Exists(file.SourcePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{file.SourcePath}\"");
            }
        }

        private void EditorTree_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            string[]? paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null) return;

            int? parentFolderId = SelectedFolderId();
            foreach (string path in paths)
            {
                if (!File.Exists(path)) continue;
                currentProject.Files.Add(new OIVFileEntry
                {
                    Id         = currentProject.NextId++,
                    SourcePath = path,
                    FileName   = Path.GetFileName(path),
                    SubPath    = string.Empty,
                    Type       = "content",
                    FolderId   = parentFolderId
                });
            }
            RebuildTree();
            RenderFileList();
        }

        // ─── Prompt helper ────────────────────────────────────────────────────

        private string PromptName(string label, string defaultVal)
        {
            using var dlg = new Form
            {
                Text            = label,
                BackColor       = Color.FromArgb(20, 20, 20),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition   = FormStartPosition.CenterParent,
                ClientSize      = new Size(320, 86),
                MaximizeBox     = false,
                MinimizeBox     = false,
                ControlBox      = false
            };
            dlg.Controls.Add(new Label
            {
                Text = label, ForeColor = Color.FromArgb(160, 110, 110),
                Font = new Font("Syne", 8F, FontStyle.Bold),
                AutoSize = true, Location = new Point(12, 10)
            });
            var txt = new TextBox
            {
                BackColor = Color.Black, ForeColor = Color.FromArgb(210, 210, 210),
                BorderStyle = BorderStyle.FixedSingle, Text = defaultVal,
                Size = new Size(294, 22), Location = new Point(12, 28),
                Font = new Font("Consolas", 9F)
            };
            dlg.Controls.Add(txt);
            var ok = new Button
            {
                Text = "OK", DialogResult = DialogResult.OK,
                BackColor = Color.FromArgb(70, 0, 0), ForeColor = Color.FromArgb(220, 160, 160),
                FlatStyle = FlatStyle.Flat, Size = new Size(70, 26), Location = new Point(12, 54)
            };
            ok.FlatAppearance.BorderColor = Color.FromArgb(110, 40, 40);
            dlg.Controls.Add(ok);
            dlg.Controls.Add(new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.FromArgb(140, 100, 100),
                FlatStyle = FlatStyle.Flat, Size = new Size(70, 26), Location = new Point(90, 54)
            });
            dlg.AcceptButton = ok;
            txt.SelectAll();

            return dlg.ShowDialog(this) == DialogResult.OK ? txt.Text.Trim() : string.Empty;
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
