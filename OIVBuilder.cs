using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;

namespace MagicOGK_OIV_Builder
{
    // ── Internal model ────────────────────────────────────────────────────────

    // Represents one RPF archive block in the assembly.xml <content> section.
    // Mirrors the OIVArchive model from the reference source.
    internal class ArchiveBlock
    {
        // Full path of the RPF as seen by OpenIV, e.g. "mods\update\x64\dlcpacks\mod1\dlc.rpf"
        public string  RpfPath         { get; set; } = string.Empty;
        // The short name used for the rpf/ subfolder inside the .oiv content folder
        public string  ContentFolderName { get; set; } = string.Empty;
        public bool    CreateIfNotExist  { get; set; } = true;
        // Always RPF7 for GTA5
        public string  ArchiveType       { get; set; } = "RPF7";
        // Files inside this archive
        public List<ArchiveFile> Files   { get; set; } = new List<ArchiveFile>();
    }

    internal class ArchiveFile
    {
        public string SourcePath   { get; set; } = string.Empty; // absolute path on disk
        public string NameInRpf    { get; set; } = string.Empty; // path+name inside the RPF
        public string Type         { get; set; } = "content";    // content | xmledit
    }

    internal class LooseFile
    {
        public string SourcePath { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty; // path as seen by OpenIV
        public string Type        { get; set; } = "content";
    }

    // ── Builder ───────────────────────────────────────────────────────────────

    public static class OIVBuilder
    {
        public static void Build(OIVProject project, string outputPath)
        {
            foreach (var file in project.Files)
                if (!File.Exists(file.SourcePath))
                    throw new FileNotFoundException($"Source file not found: {file.SourcePath}");

            string tempDir = Path.Combine(Path.GetTempPath(), "MagicOGK_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                // ── Classify every file into RPF archives vs loose files ──────
                var archives   = new Dictionary<string, ArchiveBlock>(StringComparer.OrdinalIgnoreCase);
                var looseFiles = new List<LooseFile>();

                foreach (var file in project.Files)
                {
                    string fullTarget = ResolveTargetPath(file, project).Replace('/', '\\').TrimStart('\\');

                    // Find the deepest .rpf segment — everything before it is the
                    // archive path, everything after is the path inside the RPF.
                    string rpfPath = ExtractDeepestRpfPath(fullTarget);

                    if (string.IsNullOrEmpty(rpfPath))
                    {
                        // Loose file — no RPF in path
                        looseFiles.Add(new LooseFile
                        {
                            SourcePath  = file.SourcePath,
                            Destination = fullTarget,
                            Type        = file.Type
                        });
                    }
                    else
                    {
                        // File goes inside an RPF archive
                        string nameInRpf = fullTarget.Length > rpfPath.Length
                            ? fullTarget.Substring(rpfPath.Length).TrimStart('\\')
                            : file.FileName;

                        if (!archives.ContainsKey(rpfPath))
                        {
                            // Content folder name = last segment of the rpf path without extension
                            string rpfShortName = Path.GetFileNameWithoutExtension(rpfPath);
                            // Make unique if two RPFs share a short name
                            string contentFolderName = MakeUnique(rpfShortName,
                                new HashSet<string>(archives.Values.Select(a => a.ContentFolderName),
                                    StringComparer.OrdinalIgnoreCase));

                            archives[rpfPath] = new ArchiveBlock
                            {
                                RpfPath          = rpfPath,
                                ContentFolderName = contentFolderName,
                                CreateIfNotExist  = true,
                                ArchiveType       = "RPF7"
                            };
                        }

                        archives[rpfPath].Files.Add(new ArchiveFile
                        {
                            SourcePath = file.SourcePath,
                            NameInRpf  = string.IsNullOrWhiteSpace(nameInRpf) ? file.FileName : nameInRpf,
                            Type       = file.Type
                        });
                    }
                }

                // ── Write content/ directory structure ────────────────────────
                // RPF files:   content/rpf/<ContentFolderName>/<NameInRpf>
                // Loose files: content/file/<Destination>
                // DLC patches: content/file/__dlclist_<id>.xml

                // RPF content
                foreach (var arc in archives.Values)
                {
                    string arcDir = Path.Combine(tempDir, "content", "rpf", arc.ContentFolderName);
                    Directory.CreateDirectory(arcDir);
                    foreach (var f in arc.Files)
                    {
                        string destPath = Path.Combine(arcDir, f.NameInRpf.Replace('\\', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        File.Copy(f.SourcePath, destPath, overwrite: true);
                    }
                }

                // Loose file content
                if (looseFiles.Count > 0)
                {
                    Directory.CreateDirectory(Path.Combine(tempDir, "content", "file"));
                    foreach (var f in looseFiles)
                    {
                        // Preserve the destination path structure inside file/
                        string rel      = f.Destination.TrimStart('\\');
                        string destPath = Path.Combine(tempDir, "content", "file", rel.Replace('\\', Path.DirectorySeparatorChar));
                        Directory.CreateDirectory(Path.GetDirectoryName(destPath)!);
                        File.Copy(f.SourcePath, destPath, overwrite: true);
                    }
                }

                // DLC list patch XMLs
                WriteDlcListPatches(project, Path.Combine(tempDir, "content", "file"));

                // ── icon.png ──────────────────────────────────────────────────
                // Must be exactly 128×128 per the reference source
                string iconDest = Path.Combine(tempDir, "icon.png");
                if (!string.IsNullOrWhiteSpace(project.PhotoPath) && File.Exists(project.PhotoPath))
                    CreateIcon128(project.PhotoPath, iconDest);
                else
                    CreateDefaultIcon(iconDest);

                // ── assembly.xml ──────────────────────────────────────────────
                string xml = BuildAssemblyXml(project, archives.Values.ToList(), looseFiles);
                File.WriteAllText(Path.Combine(tempDir, "assembly.xml"), xml, new UTF8Encoding(false));

                // ── Zip into .oiv ─────────────────────────────────────────────
                if (File.Exists(outputPath)) File.Delete(outputPath);
                ZipFile.CreateFromDirectory(tempDir, outputPath, CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        // ─── Target path resolution ───────────────────────────────────────────
        // Walks up the parent folder chain to build the full install path.
        // Root → update → x64 → dlcpacks → mod1 → dlc.rpf → "" (file sits in RPF root)
        // produces: update\x64\dlcpacks\mod1\dlc.rpf

        private static string ResolveTargetPath(OIVFileEntry file, OIVProject project)
        {
            var parts = new List<string> { file.FileName };
            int? cur = file.FolderId;
            while (cur.HasValue)
            {
                var folder = project.Folders.Find(f => f.Id == cur.Value);
                if (folder == null) break;
                parts.Insert(0, folder.Name);
                cur = folder.ParentId;
            }
            return string.Join("\\", parts);
        }

        // ─── assembly.xml ─────────────────────────────────────────────────────

        private static string BuildAssemblyXml(
            OIVProject project,
            List<ArchiveBlock> archives,
            List<LooseFile> looseFiles)
        {
            var doc = new XmlDocument();
            doc.InsertBefore(doc.CreateXmlDeclaration("1.0", "UTF-8", null), doc.DocumentElement);

            var pkg = doc.CreateElement("package");
            SetAttr(doc, pkg, "version", "2.1");
            SetAttr(doc, pkg, "id",      "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            SetAttr(doc, pkg, "target",  "Five");
            doc.AppendChild(pkg);

            // ── metadata ──────────────────────────────────────────────────────
            var meta = Elem(doc, pkg, "metadata");
            Elem(doc, meta, "name").InnerText = project.ModName;

            var ver = Elem(doc, meta, "version");
            Elem(doc, ver, "major").InnerText = GetVersionPart(project.Version, 0);
            Elem(doc, ver, "minor").InnerText = GetVersionPart(project.Version, 1);
            if (!string.IsNullOrWhiteSpace(project.VersionTag))
                Elem(doc, ver, "tag").InnerText = project.VersionTag.ToUpper();

            var auth = Elem(doc, meta, "author");
            Elem(doc, auth, "displayName").InnerText = project.Author;

            var desc = Elem(doc, meta, "description");
            desc.AppendChild(doc.CreateCDataSection(
                string.IsNullOrWhiteSpace(project.Description)
                    ? $"{project.ModName} by {project.Author}"
                    : project.Description));

            // ── colors ────────────────────────────────────────────────────────
            Color headerColor = string.IsNullOrWhiteSpace(project.BannerColor)
                ? Color.FromArgb(255, 35, 54, 106)
                : ParseHexColor(project.BannerColor);

            var colors = Elem(doc, pkg, "colors");

            var hdrBg = Elem(doc, colors, "headerBackground");
            SetAttr(doc, hdrBg, "useBlackTextColor", "False");
            hdrBg.InnerText = ToOivColor(headerColor);

            Elem(doc, colors, "iconBackground").InnerText =
                ToOivColor(Color.FromArgb(255, 59, 89, 152));

            // ── content ───────────────────────────────────────────────────────
            var content = Elem(doc, pkg, "content");

            // Generic / loose files
            foreach (var f in looseFiles)
            {
                var add = Elem(doc, content, "add");
                SetAttr(doc, add, "source", @"file\" + f.Destination.TrimEnd('\\'));
                add.InnerText = f.Destination.TrimEnd('\\');
            }

            // DLC list patches (written as generic files)
            foreach (var folder in project.Folders.Where(f => f.AddToDlcList))
            {
                var add = Elem(doc, content, "add");
                SetAttr(doc, add, "source", $@"file\__dlclist_{folder.Id}.xml");
                add.InnerText = $@"__dlclist_{folder.Id}.xml";
            }

            // RPF archives
            foreach (var arc in archives)
            {
                var arcElem = Elem(doc, content, "archive");
                SetAttr(doc, arcElem, "path",            arc.RpfPath);
                SetAttr(doc, arcElem, "createIfNotExist", arc.CreateIfNotExist.ToString());
                SetAttr(doc, arcElem, "type",            arc.ArchiveType);

                foreach (var f in arc.Files)
                {
                    string tag = f.Type == "xmledit" ? "xml" : "add";
                    var fileElem = Elem(doc, arcElem, tag);
                    // source path inside the .oiv content folder
                    SetAttr(doc, fileElem, "source", $@"rpf\{arc.ContentFolderName}\{f.NameInRpf.TrimStart('\\')}");
                    fileElem.InnerText = f.NameInRpf.TrimStart('\\');
                }
            }

            // dlclist.xml edits as archive references in update.rpf
            var dlcFolders = project.Folders.Where(f => f.AddToDlcList).ToList();
            if (dlcFolders.Count > 0)
            {
                var updateRpf = Elem(doc, content, "archive");
                SetAttr(doc, updateRpf, "path",            @"update\update.rpf");
                SetAttr(doc, updateRpf, "createIfNotExist", "False");
                SetAttr(doc, updateRpf, "type",            "RPF7");

                var commonData = Elem(doc, updateRpf, "archive");
                SetAttr(doc, commonData, "path",            @"common\data");
                SetAttr(doc, commonData, "createIfNotExist", "False");
                SetAttr(doc, commonData, "type",            "RPF7");

                foreach (var folder in dlcFolders)
                {
                    var xmlElem = Elem(doc, commonData, "xml");
                    SetAttr(doc, xmlElem, "source", $@"file\__dlclist_{folder.Id}.xml");
                    xmlElem.InnerText = "dlclist.xml";
                }
            }

            // Serialize
            var ms = new MemoryStream();
            using (var writer = new XmlTextWriter(ms, new UTF8Encoding(false)))
            {
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
            }
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // ─── DLC list patch XMLs ──────────────────────────────────────────────

        private static void WriteDlcListPatches(OIVProject project, string fileContentDir)
        {
            var dlcFolders = project.Folders.Where(f => f.AddToDlcList).ToList();
            if (dlcFolders.Count == 0) return;

            Directory.CreateDirectory(fileContentDir);

            foreach (var folder in dlcFolders)
            {
                string dlcEntry = $"dlcpacks:/{folder.Name}/";
                string patchXml =
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
<SMandatoryPacksData>
	<Paths>
		<Item>{dlcEntry}</Item>
	</Paths>
</SMandatoryPacksData>";
                File.WriteAllText(
                    Path.Combine(fileContentDir, $"__dlclist_{folder.Id}.xml"),
                    patchXml,
                    new UTF8Encoding(false));
            }
        }

        // ─── Icon helpers ─────────────────────────────────────────────────────
        // Reference source: normalise to 128×128 with high quality resampling.

        private static void CreateIcon128(string sourcePath, string destPng)
        {
            try
            {
                using var source = new Bitmap(sourcePath);
                using var dest   = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
                using var g      = Graphics.FromImage(dest);
                g.CompositingMode    = CompositingMode.SourceCopy;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode  = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode      = SmoothingMode.AntiAlias;
                g.PixelOffsetMode    = PixelOffsetMode.HighQuality;
                g.DrawImage(source, 0, 0, 128, 128);
                dest.Save(destPng, ImageFormat.Png);
            }
            catch
            {
                File.Copy(sourcePath, destPng, overwrite: true);
            }
        }

        private static void CreateDefaultIcon(string destPng)
        {
            try
            {
                using var bmp = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
                using var g   = Graphics.FromImage(bmp);
                g.Clear(Color.FromArgb(255, 35, 54, 106));
                bmp.Save(destPng, ImageFormat.Png);
            }
            catch { }
        }

        // ─── XML helpers ──────────────────────────────────────────────────────

        private static XmlElement Elem(XmlDocument doc, XmlNode parent, string name)
        {
            var e = doc.CreateElement(name);
            parent.AppendChild(e);
            return e;
        }

        private static void SetAttr(XmlDocument doc, XmlElement elem, string name, string value)
        {
            var attr = doc.CreateAttribute(name);
            attr.Value = value;
            elem.Attributes.Append(attr);
        }

        // ─── Path helpers ─────────────────────────────────────────────────────

        // Returns the deepest path prefix that ends at a .rpf segment.
        // "update\x64\dlcpacks\mod1\dlc.rpf" → "update\x64\dlcpacks\mod1\dlc.rpf"
        // "update\x64\dlcpacks\mod1\dlc.rpf\somefile.txt" → "update\x64\dlcpacks\mod1\dlc.rpf"
        // "scripts\myscript.asi" → ""
        private static string ExtractDeepestRpfPath(string target)
        {
            string[] parts = target.Split('\\');
            var sb   = new StringBuilder();
            string last = string.Empty;
            foreach (string p in parts)
            {
                if (sb.Length > 0) sb.Append('\\');
                sb.Append(p);
                if (p.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    last = sb.ToString();
            }
            return last;
        }

        private static string MakeUnique(string name, HashSet<string> used)
        {
            if (!used.Contains(name)) return name;
            int n = 2;
            string candidate;
            do { candidate = $"{name}_{n++}"; } while (used.Contains(candidate));
            return candidate;
        }

        private static string GetVersionPart(string version, int index)
        {
            if (string.IsNullOrWhiteSpace(version)) return "0";
            string[] parts = version.Split('.');
            return (index < parts.Length && int.TryParse(parts[index].Trim(), out int v))
                ? v.ToString() : "0";
        }

        // OIV color format: $AARRGGBB
        private static string ToOivColor(Color c)
            => $"${c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color ParseHexColor(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                    return Color.FromArgb(255,
                        Convert.ToInt32(hex.Substring(0, 2), 16),
                        Convert.ToInt32(hex.Substring(2, 2), 16),
                        Convert.ToInt32(hex.Substring(4, 2), 16));
            }
            catch { }
            return Color.FromArgb(255, 35, 54, 106);
        }
    }
}
