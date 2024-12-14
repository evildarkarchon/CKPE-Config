using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

namespace CKPE_Config
{
    public sealed partial class SectionPage : Page
    {
        private ConfigSection? _section;

        public SectionPage()
        {
            this.InitializeComponent();
        }

        public void Initialize(ConfigSection section)
        {
            _section = section;
            SectionTitle.Text = section.Name;

            var entries = section.Entries.Select(entry => new
            {
                entry.Name,
                entry.Tooltip,
                ValueControl = ((MainWindow)Window.Current).CreateWidgetForValue(entry.Value, entry.Name, section.Name)
            });

            EntriesRepeater.ItemsSource = entries;
        }
    }
}
