using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MaterialDesignExtensions.Controls;
using MaterialDesignExtensions.Model;
using MaterialDesignThemes.Wpf;
using VotingPC;
using VotingPCNew.Controls;

namespace VotingPCNew.Slides;

/// <summary>
/// Interaction logic for Slide2_Vote.xaml
/// </summary>
public partial class Slide2
{
    public Slide2()
    {
        InitializeComponent();
    }

    public string TopTitle
    {
        get => topTitle.Text;
        set => topTitle.Text = value;
    }

    // TODO: add the title thing on top of this divider to config
    private static readonly DividerNavigationItem s_divider = new();
    private Dictionary<string, (Sector, VirtualizingStackPanel)> _sectorVotePanels;

    // TODO: write xmldoc for this
    public void SetItemsSource(List<Sector> sectorInfos)
    {
        _sectorVotePanels = new(sectorInfos.Count);
        List<BaseNavigationItem> navigationItems = new(sectorInfos.Count + 2) {s_divider};
        foreach (var sector in sectorInfos)
        {
            // Add sector to nav list
            FirstLevelNavigationItem navItem = new()
            {
                Label = sector.Name,
                Icon = PackIconKind.CameraControl // This icon is god-level generic
            };
            navigationItems.Add(navItem);

            // Populate vote panels
            VirtualizingStackPanel votePanel = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(32)
            };
            foreach (var candidate in sector.Candidates)
            {
                RadioTextBox textBox = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 12, 0, 12),
                    Text = $"{candidate.Gender}   {candidate.Name}",
                    IsChecked = true
                };
                votePanel.Children.Add(textBox);
            }
            // Add the sector to dictionary for easier lookup later
            _sectorVotePanels.Add(sector.Name, (sector, votePanel));
        }
        navigationItems.Add(s_divider);
        sideNav.Items = navigationItems;
        if (sectorInfos.Count != 0)
        {
            sideNav.SelectedItem = navigationItems[1];
        }
    }

    /// <summary>
    /// Occurs when the item selected in sidenav bar is changed
    /// </summary>
    /// <remarks>
    /// Apply selected sector's style and candidate list
    /// </remarks>
    private void NavigationItem_SelectionChanged(object sender, NavigationItemSelectedEventArgs args)
    {
        // Get selected sector's name
        string selectedItem = ((NavigationItem)args.NavigationItem)?.Label;
        if (selectedItem is null) return; // In case nav item is reset from code, and nothing is selected
        // Get the selected sector's info and apply styling
        var (sector, panel) = _sectorVotePanels[selectedItem];
        title.Text = sector.Title;
        subtitle.Text = sector.Subtitle;
        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(sector.Color)!);
        // TODO: move text below to config
        maxVote.Text = $"Chọn tối đa {sector.Max} người";
        candidateCard.Content = panel;
    }
    
    private void Button_Click(object sender, RoutedEventArgs e)
    {
    }
}
