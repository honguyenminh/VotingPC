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
/// Interaction logic for Slide1_Welcome.xaml
/// </summary>
public partial class Slide1
{
    public TextConfig TitleConfig { set => title.SetConfig(value); }
    public string IconPath { set => image.SetSource(value); }
    public TextConfig TopHeaderConfig { set => topHeader.SetConfig(value); }
    public TextConfig TopSubheaderConfig { set => topSubheader.SetConfig(value); }

    public Slide1()
    {
        InitializeComponent();
    }
}
