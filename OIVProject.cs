using System.Collections.Generic;

namespace MagicOGK_OIV_Builder
{
    public class OIVFileEntry
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string SourcePath { get; set; } = string.Empty;
        public string TargetPath { get; set; } = string.Empty;
        public string Type { get; set; } = "content";
    }

    public class OIVProject
    {
        public string ModName    { get; set; } = string.Empty;
        public string Author     { get; set; } = string.Empty;
        public string Version    { get; set; } = "1.0";
        public string VersionTag { get; set; } = "Stable";
        public string Description { get; set; } = string.Empty;
        // Hex color string e.g. "640000" for the OIV installer banner
        public string BannerColor { get; set; } = "640000";
        // Absolute path to the preview photo (not bundled in the .oiv by default)
        public string PhotoPath  { get; set; } = string.Empty;
        public int    NextId     { get; set; } = 1;
        public List<OIVFileEntry> Files { get; set; } = new List<OIVFileEntry>();
    }
}
