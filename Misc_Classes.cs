using System.Collections.Generic;

namespace CKPE_Config
{
    public class ConfigEntry
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
        public string InlineComment { get; set; } = string.Empty;
    }

    public class ConfigSection
    {
        public string Name { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
        public List<ConfigEntry> Entries { get; set; } = new();
    }
}