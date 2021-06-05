using System.Windows.Controls;

namespace VotingPC
{
    /// <summary>
    /// Interaction logic for Slide1_Welcome.xaml
    /// </summary>
    public partial class Slide1_Welcome : UserControl
    {
        public Slide1_Welcome()
        {
            InitializeComponent();
        }
        // After slide load, wait for fingerprint, delete from database,
        // then move to voting slide
    }
}
