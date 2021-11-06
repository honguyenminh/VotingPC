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

namespace VotingPCNew
{
    /// <summary>
    /// Interaction logic for Slide1_Welcome.xaml
    /// </summary>
    public partial class Slide1 : UserControl
    {
        #region Binding
        public string IconPath { get; set; } = "/assets/Emblem_of_Vietnam.png";
        public string TopTitle { get; set; } = "CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM";
        public string TopSubtitle { get; set; } = "Độc lập - Tự do - Hạnh phúc";
        #endregion
        public Slide1()
        {
            InitializeComponent();
        }
    }
}
