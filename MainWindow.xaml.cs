using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage.Pickers;
using Windows.Storage;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CKPE_Config
{
    public sealed partial class MainWindow : Window
    {
        private readonly Dictionary<(string SectionName, string EntryName), FrameworkElement> _widgets = new();
        private List<ConfigSection> _sections = new();
        private List<string> _originalLines = new();
        private string? _currentFile;

        public MainWindow()
        {
            InitializeComponent();
            Title = "CreationKit Platform Extended INI Editor";

            // Create the main grid
            var rootGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };

            // Create branding grid
            brandingGrid = CreateBrandingGrid();

            // Create navigation view
            navigationView = new NavigationView
            {
                IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed,
                IsSettingsVisible = false,
                PaneDisplayMode = NavigationViewPaneDisplayMode.Top
            };

            // Create content frame
            contentFrame = new Frame();
            navigationView.Content = contentFrame;

            // Create button panel
            buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Spacing = 10,
                Margin = new Thickness(10)
            };

            var loadButton = new Button { Content = "Load INI" };
            var saveButton = new Button { Content = "Save INI" };

            loadButton.Click += LoadIni_Click;
            saveButton.Click += SaveIni_Click;

            buttonPanel.Children.Add(loadButton);
            buttonPanel.Children.Add(saveButton);

            // Add controls to root grid
            Grid.SetRow(navigationView, 0);
            Grid.SetRow(buttonPanel, 1);
            rootGrid.Children.Add(navigationView);
            rootGrid.Children.Add(buttonPanel);

            // Initially show branding
            contentFrame.Content = brandingGrid;

            Content = rootGrid;
        }

        private Grid CreateBrandingGrid()
        {
            var grid = new Grid();
            var panel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 10
            };

            var heading = new TextBlock
            {
                Text = "Creation Kit Platform Extended",
                FontSize = 32,
                FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };

            var subheading = new TextBlock
            {
                Text = "INI Configuration Editor",
                FontSize = 24,
                TextAlignment = TextAlignment.Center
            };

            var version = new TextBlock
            {
                Text = "v0.1.0",
                FontSize = 16,
                TextAlignment = TextAlignment.Center
            };

            panel.Children.Add(heading);
            panel.Children.Add(subheading);
            panel.Children.Add(version);
            grid.Children.Add(panel);

            return grid;
        }

        private static string ParseComments(List<string> lines, int startIdx)
        {
            var comments = new List<string>();
            var idx = startIdx - 1;

            while (idx >= 0 && (lines[idx].Trim().StartsWith(";") || string.IsNullOrWhiteSpace(lines[idx])))
            {
                if (lines[idx].Trim().StartsWith(";"))
                {
                    comments.Insert(0, lines[idx].Trim()[1..].Trim());
                }
                idx--;
            }

            return string.Join("\n", comments);
        }

        private (List<ConfigSection> Sections, List<string> Lines) ParseIniWithComments(string filePath)
        {
            var lines = File.ReadAllLines(filePath).ToList();
            var sections = new List<ConfigSection>();
            ConfigSection? currentSection = null;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith(";"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    var sectionName = line[1..^1];
                    var tooltip = ParseComments(lines, i);
                    currentSection = new ConfigSection
                    {
                        Name = sectionName,
                        Tooltip = tooltip,
                        LineNumber = i
                    };
                    sections.Add(currentSection);
                }
                else if (line.Contains('=') && currentSection != null)
                {
                    var parts = line.Split(new[] { '=' }, 2);
                    var name = parts[0].Trim();
                    var value = parts[1].Trim();

                    var tooltip = ParseComments(lines, i);
                    var inlineComment = string.Empty;

                    if (value.Contains(';'))
                    {
                        var commentParts = value.Split(new[] { ';' }, 2);
                        value = commentParts[0].Trim();
                        inlineComment = commentParts[1].Trim();
                        tooltip = string.IsNullOrEmpty(tooltip) ? inlineComment : $"{tooltip}\n{inlineComment}";
                    }

                    currentSection.Entries.Add(new ConfigEntry
                    {
                        Name = name,
                        Value = value,
                        Tooltip = tooltip,
                        LineNumber = i,
                        InlineComment = inlineComment
                    });
                }
            }

            return (sections, lines);
        }

        public FrameworkElement CreateWidgetForValue(string value, string entryName, string sectionName)
        {
            if (sectionName == "Hotkeys" || entryName == "uTintMaskResolution" || sectionName == "Log")
            {
                return new TextBox { Text = value };
            }

            if (entryName == "nCharset")
            {
                var comboBox = new ComboBox();
                var charsets = new Dictionary<string, int>
                {
                    { "ANSI_CHARSET", 0 },
                    { "DEFAULT_CHARSET", 1 },
                    // Add other charsets...
                };

                foreach (var charset in charsets)
                {
                    comboBox.Items.Add(new ComboBoxItem { Content = charset.Key, Tag = charset.Value });
                }

                comboBox.SelectedIndex = comboBox.Items.Cast<ComboBoxItem>()
                    .ToList()
                    .FindIndex(item => (int)item.Tag == int.Parse(value));

                return comboBox;
            }

            if (entryName == "uUIDarkThemeId")
            {
                var comboBox = new ComboBox();
                var themes = new Dictionary<string, int>
                {
                    { "Lighter", 0 },
                    { "Darker", 1 },
                    { "Custom", 2 }
                };

                foreach (var theme in themes)
                {
                    comboBox.Items.Add(new ComboBoxItem { Content = theme.Key, Tag = theme.Value });
                }

                comboBox.SelectedIndex = comboBox.Items.Cast<ComboBoxItem>()
                    .ToList()
                    .FindIndex(item => (int)item.Tag == int.Parse(value));

                return comboBox;
            }

            if (bool.TryParse(value, out var boolValue))
            {
                return new CheckBox { IsChecked = boolValue };
            }

            if (int.TryParse(value, out _))
            {
                return new NumberBox { Value = double.Parse(value), SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact };
            }

            return new TextBox { Text = value };
        }

        private ScrollViewer CreateSectionControl(ConfigSection section)
        {
            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            var panel = new StackPanel
            {
                Spacing = 10,
                Padding = new Thickness(20)
            };

            foreach (var entry in section.Entries)
            {
                var entryPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 10
                };

                var label = new TextBlock
                {
                    Text = entry.Name,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 200
                };

                if (!string.IsNullOrEmpty(entry.Tooltip))
                {
                    ToolTipService.SetToolTip(label, entry.Tooltip);
                }

                var widget = CreateWidgetForValue(entry.Value, entry.Name, section.Name);
                _widgets[(section.Name, entry.Name)] = widget;

                entryPanel.Children.Add(label);
                entryPanel.Children.Add(widget);
                panel.Children.Add(entryPanel);
            }

            scrollViewer.Content = panel;
            return scrollViewer;
        }

        private async void LoadIni_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            picker.FileTypeFilter.Add(".ini");

            // WinUI 3 requires a window handle for the picker
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file == null) return;

            if (!await VerifyFilename(file.Name, "selected")) return;

            var (sections, lines) = ParseIniWithComments(file.Path);
            _sections = sections;
            _originalLines = lines;
            _currentFile = file.Path;

            RefreshUI();
        }


        private async void SaveIni_Click(object sender, RoutedEventArgs e)
        {
            if (_currentFile == null)
            {
                var picker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = "CreationKitPlatformExtended.ini"
                };
                picker.FileTypeChoices.Add("INI files", new List<string> { ".ini" });

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSaveFileAsync();
                if (file == null) return;

                if (!await VerifyFilename(file.Name, "save")) return;

                _currentFile = file.Path;
            }

            var newLines = new List<string>(_originalLines);

            foreach (var section in _sections)
            {
                foreach (var entry in section.Entries)
                {
                    var widget = _widgets[(section.Name, entry.Name)];
                    string value = widget switch
                    {
                        CheckBox cb => cb.IsChecked?.ToString().ToLower() ?? "false",
                        NumberBox nb => nb.Value.ToString(),
                        ComboBox cb => ((ComboBoxItem)cb.SelectedItem).Tag.ToString() ?? "0",
                        TextBox tb => tb.Text,
                        _ => string.Empty
                    };

                    var newLine = !string.IsNullOrEmpty(entry.InlineComment)
                        ? $"{entry.Name}={value}\t\t\t; {entry.InlineComment}"
                        : $"{entry.Name}={value}";

                    if (entry.LineNumber.HasValue)
                    {
                        var leadingSpace = newLines[entry.LineNumber.Value].Length - newLines[entry.LineNumber.Value].TrimStart().Length;
                        newLines[entry.LineNumber.Value] = new string(' ', leadingSpace) + newLine + Environment.NewLine;
                    }
                    else
                    {
                        newLines.Add(newLine + Environment.NewLine);
                    }
                }
            }

            try
            {
                await File.WriteAllLinesAsync(_currentFile, newLines);
                var dialog = new ContentDialog
                {
                    Title = "Success",
                    Content = "File saved successfully!",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error saving file: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }


        private void RefreshUI()
        {
            navigationView.MenuItems.Clear();
            _widgets.Clear();

            foreach (var section in _sections)
            {
                var navigationViewItem = new NavigationViewItem
                {
                    Content = section.Name,
                    Tag = section
                };

                if (!string.IsNullOrEmpty(section.Tooltip))
                {
                    ToolTipService.SetToolTip(navigationViewItem, section.Tooltip);
                }

                navigationView.MenuItems.Add(navigationViewItem);
            }

            // Remove any existing event handler to prevent duplicates
            navigationView.SelectionChanged -= NavigationView_SelectionChanged;
            navigationView.SelectionChanged += NavigationView_SelectionChanged;

            // Switch visibility
            brandingGrid.Visibility = Visibility.Collapsed;
            navigationView.Visibility = Visibility.Visible;

            // Select the first item if there are any sections
            if (navigationView.MenuItems.Count > 0)
            {
                var firstItem = navigationView.MenuItems[0] as NavigationViewItem;
                if (firstItem != null)
                {
                    firstItem.IsSelected = true;
                    // Manually trigger the content update for the first section
                    if (firstItem.Tag is ConfigSection section)
                    {
                        contentFrame.Content = CreateSectionControl(section);
                    }
                }
            }
        }

        // Separate event handler for better organization
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item && item.Tag is ConfigSection section)
            {
                contentFrame.Content = CreateSectionControl(section);
            }
        }

        private async Task<bool> VerifyFilename(string filepath, string operation)
        {
            const string expectedName = "CreationKitPlatformExtended.ini";
            var actualName = Path.GetFileName(filepath);

            if (actualName != expectedName)
            {
                var dialog = new ContentDialog
                {
                    Title = "Invalid Filename",
                    Content = $"The {operation} filename must be '{expectedName}'\nSelected file: '{actualName}'",
                    CloseButtonText = "OK",
                    XamlRoot = Content.XamlRoot
                };
                await dialog.ShowAsync();
                return false;
            }
            return true;
        }
    }
}
