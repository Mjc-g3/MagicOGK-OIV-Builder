using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace MagicOGK_OIV_Builder
{
    public static class OIVBuilder
    {
        // ─── Entry point ─────────────────────────────────────────────────────

        public static void Build(OIVProject project, string outputPath)
        {
            // Validate source files exist before touching disk
            foreach (var file in project.Files)
            {
                if (!File.Exists(file.SourcePath))
                    throw new FileNotFoundException($"Source file not found: {file.SourcePath}");
            }

            string tempDir = Path.Combine(Path.GetTempPath(), "MagicOGK_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            try
            {
                string contentDir = Path.Combine(tempDir, "content");
                Directory.CreateDirectory(contentDir);

                // Copy each mod file flat into content/, using a safe unique name
                // so we never have collisions between same-named files in different paths.
                var entries = new List<PackEntry>();
                var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var file in project.Files)
                {
                    string safeName = MakeSafeContentName(file.FileName, usedNames);
                    usedNames.Add(safeName);

                    File.Copy(file.SourcePath, Path.Combine(contentDir, safeName), overwrite: true);

                    entries.Add(new PackEntry
                    {
                        ContentName = safeName,   // name inside content/ folder in the ZIP
                        TargetPath  = file.TargetPath.Trim().Replace('/', '\\').TrimStart('\\'),
                        FileName    = file.FileName,
                        Type        = file.Type
                    });
                }

                // Write assembly.xml
                string xml = BuildAssemblyXml(project, entries);
                File.WriteAllText(Path.Combine(tempDir, "assembly.xml"), xml, new UTF8Encoding(false));

                // Package into ZIP renamed to .oiv
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                ZipFile.CreateFromDirectory(tempDir, outputPath,
                    CompressionLevel.Optimal, includeBaseDirectory: false);
            }
            finally
            {
                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        // ─── XML generation ──────────────────────────────────────────────────

        private static string BuildAssemblyXml(OIVProject project, List<PackEntry> entries)
        {
            var ms       = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Indent           = true,
                IndentChars      = "\t",
                Encoding         = new UTF8Encoding(false),
                OmitXmlDeclaration = false
            };

            using (var w = XmlWriter.Create(ms, settings))
            {
                w.WriteStartDocument();

                // <package version="2.1" id="{GUID}" target="Five">
                w.WriteStartElement("package");
                w.WriteAttributeString("version", "2.1");
                w.WriteAttributeString("id",      "{" + Guid.NewGuid().ToString().ToUpper() + "}");
                w.WriteAttributeString("target",  "Five");

                WriteMetadata(w, project);
                WriteColors(w, project);
                WriteContent(w, entries);

                w.WriteEndElement(); // </package>
                w.WriteEndDocument();
            }

            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // ─── <metadata> ──────────────────────────────────────────────────────

        private static void WriteMetadata(XmlWriter w, OIVProject project)
        {
            w.WriteStartElement("metadata");

            w.WriteElementString("name", project.ModName);

            // <version>
            w.WriteStartElement("version");
            w.WriteElementString("major", GetVersionPart(project.Version, 0));
            w.WriteElementString("minor", GetVersionPart(project.Version, 1));
            if (!string.IsNullOrWhiteSpace(project.VersionTag))
                w.WriteElementString("tag", project.VersionTag.ToUpper());
            w.WriteEndElement();

            // <author>
            w.WriteStartElement("author");
            w.WriteElementString("displayName", project.Author);
            w.WriteEndElement();

            // <description>
            w.WriteStartElement("description");
            w.WriteCData(!string.IsNullOrWhiteSpace(project.Description)
                ? project.Description
                : $"{project.ModName} by {project.Author}");
            w.WriteEndElement();

            w.WriteEndElement(); // </metadata>
        }

        // ─── <colors> ────────────────────────────────────────────────────────
        // OIV uses ARGB hex with a $ prefix: $AARRGGBB

        private static void WriteColors(XmlWriter w, OIVProject project)
        {
            // Default to a dark red if no banner color stored
            string headerColor = ArgbToOivHex(project.BannerColor.Length > 0
                ? ParseHexColor(project.BannerColor)
                : Color.FromArgb(255, 100, 0, 0));

            string iconColor = ArgbToOivHex(Color.FromArgb(255, 40, 0, 0));

            w.WriteStartElement("colors");

            w.WriteStartElement("headerBackground");
            w.WriteAttributeString("useBlackTextColor", "False");
            w.WriteString(headerColor);
            w.WriteEndElement();

            w.WriteElementString("iconBackground", iconColor);

            w.WriteEndElement(); // </colors>
        }

        // ─── <content> ───────────────────────────────────────────────────────
        //
        // OIV content model:
        //   <archive path="update\update.rpf" createIfNotExist="True" type="RPF8">
        //     <add source="content/myfile.yft">path\inside\rpf\myfile.yft</add>
        //   </archive>
        //
        // Files with no target path go into the root of the archive.
        // Files whose target path ends in a folder (no .rpf) are treated as a
        // path inside the nearest parent RPF.
        //
        // We group entries by their RPF archive path so files sharing the same
        // archive end up in one <archive> block.

        private static void WriteContent(XmlWriter w, List<PackEntry> entries)
        {
            w.WriteStartElement("content");

            // Separate entries that target a known RPF vs. loose root files
            var rpfGroups = new Dictionary<string, List<PackEntry>>(StringComparer.OrdinalIgnoreCase);
            var rootFiles = new List<PackEntry>();

            foreach (var entry in entries)
            {
                if (string.IsNullOrWhiteSpace(entry.TargetPath))
                {
                    // No path set — drop in root (no archive wrapper)
                    rootFiles.Add(entry);
                    continue;
                }

                // Find the deepest .rpf segment in the target path
                string rpfPath   = ExtractRpfPath(entry.TargetPath);
                string innerPath = rpfPath.Length > 0
                    ? entry.TargetPath.Substring(rpfPath.Length).TrimStart('\\')
                    : entry.TargetPath;

                if (rpfPath.Length == 0)
                {
                    // Target path has no .rpf — treat whole path as the archive
                    // e.g. "scripts\" just means a loose folder; wrap in a generic archive
                    rootFiles.Add(entry);
                    continue;
                }

                if (!rpfGroups.ContainsKey(rpfPath))
                    rpfGroups[rpfPath] = new List<PackEntry>();

                rpfGroups[rpfPath].Add(new PackEntry
                {
                    ContentName = entry.ContentName,
                    FileName    = entry.FileName,
                    Type        = entry.Type,
                    TargetPath  = innerPath  // path inside the RPF
                });
            }

            // Write each RPF archive block
            foreach (var kv in rpfGroups)
            {
                w.WriteStartElement("archive");
                w.WriteAttributeString("path", kv.Key);
                w.WriteAttributeString("createIfNotExist", "True");
                w.WriteAttributeString("type", "RPF8");

                foreach (var entry in kv.Value)
                    WriteAddElement(w, entry);

                w.WriteEndElement(); // </archive>
            }

            // Root/loose files (scripts, plugins, no-archive files)
            foreach (var entry in rootFiles)
                WriteAddElement(w, entry);

            w.WriteEndElement(); // </content>
        }

        private static void WriteAddElement(XmlWriter w, PackEntry entry)
        {
            if (entry.Type == "xmledit")
            {
                // <xml source="content/file.xml">target\path\file.xml</xml>
                w.WriteStartElement("xml");
                w.WriteAttributeString("source", "content/" + entry.ContentName);
                string target = string.IsNullOrWhiteSpace(entry.TargetPath)
                    ? entry.FileName
                    : (entry.TargetPath.TrimEnd('\\') + "\\" + entry.FileName).TrimStart('\\');
                w.WriteString(target);
                w.WriteEndElement();
            }
            else
            {
                // <add source="content/file.yft">target\path\file.yft</add>
                w.WriteStartElement("add");
                w.WriteAttributeString("source", "content/" + entry.ContentName);
                string target = string.IsNullOrWhiteSpace(entry.TargetPath)
                    ? entry.FileName
                    : (entry.TargetPath.TrimEnd('\\') + "\\" + entry.FileName).TrimStart('\\');
                w.WriteString(target);
                w.WriteEndElement();
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        // Returns the deepest path segment ending with .rpf, using backslashes.
        // e.g. "update\update.rpf\common\data" → "update\update.rpf"
        //      "x64e.rpf\levels\gta5\vehicles.rpf\pedestrians.rpf" → full deepest .rpf
        private static string ExtractRpfPath(string targetPath)
        {
            string[] parts = targetPath.Split('\\');
            var sb = new StringBuilder();
            string last = string.Empty;
            foreach (string part in parts)
            {
                if (sb.Length > 0) sb.Append('\\');
                sb.Append(part);
                if (part.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                    last = sb.ToString();
            }
            return last;
        }

        // Makes a unique safe filename for the content/ folder.
        // Keeps the original name but adds a numeric suffix if there's a collision.
        private static string MakeSafeContentName(string fileName, HashSet<string> used)
        {
            if (!used.Contains(fileName))
                return fileName;

            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext  = Path.GetExtension(fileName);
            int    n    = 2;
            string candidate;
            do { candidate = $"{name}_{n++}{ext}"; }
            while (used.Contains(candidate));
            return candidate;
        }

        private static string GetVersionPart(string version, int index)
        {
            if (string.IsNullOrWhiteSpace(version)) return "0";
            string[] parts = version.Split('.');
            if (index < parts.Length && int.TryParse(parts[index].Trim(), out int val))
                return val.ToString();
            return "0";
        }

        private static string ArgbToOivHex(Color c)
            => $"$FF{c.R:X2}{c.G:X2}{c.B:X2}";

        private static Color ParseHexColor(string hex)
        {
            try
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    int r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    int g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    int b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return Color.FromArgb(255, r, g, b);
                }
            }
            catch { }
            return Color.FromArgb(255, 100, 0, 0);
        }

        // ─── PackEntry ───────────────────────────────────────────────────────

        private class PackEntry
        {
            public string ContentName { get; set; } = string.Empty; // filename in content/ folder
            public string FileName    { get; set; } = string.Empty; // original filename
            public string TargetPath  { get; set; } = string.Empty; // path inside RPF (after .rpf/)
            public string Type        { get; set; } = "content";
        }
    }
}
