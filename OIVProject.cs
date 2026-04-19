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
        public string ModName { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public string VersionTag { get; set; } = "Stable";
        public string OIVSpec { get; set; } = "Stable";
        public string Description { get; set; } = string.Empty;
        public int NextId { get; set; } = 1;
        public List<OIVFileEntry> Files { get; set; } = new List<OIVFileEntry>();
    }
}
