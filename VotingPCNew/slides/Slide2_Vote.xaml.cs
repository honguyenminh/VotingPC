using MaterialDesignExtensions.Model;
using MaterialDesignExtensions.Controls;
using MaterialDesignThemes.Wpf;
using VotingPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VotingPCNew;
/// <summary>
/// Interaction logic for Slide2_Vote.xaml
/// </summary>
public partial class Slide2
{
    public Slide2()
    {
        InitializeComponent();
    }

    private string _selectedSector;
    public string SelectedSector { get => _selectedSector; }

    public string TopTitle
    {
        get => topTitle.Text;
        set => topTitle.Text = value;
    }

    // TODO: Put this in the config file
    private readonly DividerNavigationItem _divider = new();
    private Dictionary<string, Sector> sectors;
    public List<Sector> SectorList
    {
        set
        {
            sectors = new(value.Count);
            List<BaseNavigationItem> navigationItems = new(value.Count + 2);
            navigationItems.Add(_divider);
            foreach (var sector in value)
            {
                sectors.Add(sector.Name, sector);
                FirstLevelNavigationItem navItem = new()
                {
                    Label = sector.Name,
                    Icon = PackIconKind.CameraControl // This icon is god-level generic
                };
                navigationItems.Add(navItem);
            }
            navigationItems.Add(_divider);
            sideNav.Items = navigationItems;
            if (value.Count != 0)
            {
                sideNav.SelectedItem = navigationItems[1];
            }
        }
    }

    private void NavigationItemSelected(object sender, NavigationItemSelectedEventArgs args)
    {
        string selectedItem = ((NavigationItem)args.NavigationItem)?.Label;
        if (_selectedSector == selectedItem) return;
        _selectedSector = selectedItem;
        if (_selectedSector is null) return;
        Sector newSector = sectors[_selectedSector];
        title.Text = newSector.Title;
        subtitle.Text = newSector.Subtitle;
        // TODO: move this to config
        maxVote.Text = $"Chọn tối đa {newSector.Max} người";
    }
}
