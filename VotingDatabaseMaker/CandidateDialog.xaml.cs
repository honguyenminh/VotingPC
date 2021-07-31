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

namespace VotingDatabaseMaker
{
    /// <summary>
    /// Interaction logic for CandidateDialog.xaml
    /// </summary>
    public partial class CandidateDialog : UserControl
    {
        // Public properties
        public string NameInput { get; set; }
        public string GenderInput { get; set; }

        public CandidateDialog()
        {
            InitializeComponent();
        }
    }
}
