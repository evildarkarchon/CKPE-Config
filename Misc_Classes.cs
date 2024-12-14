using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;

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