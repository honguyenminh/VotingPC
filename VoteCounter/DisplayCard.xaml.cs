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
using VotingPC;

namespace VoteCounter
{
    /// <summary>
    /// Interaction logic for DisplayCard.xaml
    /// </summary>
    public partial class DisplayCard : UserControl
    {
        // Public properties
        public Info SectionInfo { get; }
        public List<Candidate> Candidates { get; }

        // Constructors
        public DisplayCard()
        {
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
            InitializeComponent();
        }
        public DisplayCard(Info info, List<Candidate> candidates)
        {
            SectionInfo = info;
            Candidates = candidates;
            PreviewMouseLeftButtonUp += (sender, args) => OnClick();
            InitializeComponent();

            // Set details to parameters
            SectionTextBlock.Text = info.Title ?? info.Sector;

            // Show winning candidate
            NameTextBlock.Text = candidates[0].Name;
            TotalVoteTextBlock.Text = candidates[0].Votes.ToString();

            // Show total winning places count
            TotalWinningPlacesTextBlock.Text = candidates[0].TotalWinningPlaces.ToString();
        }

        // Click event setup
        #region Click event setup
        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DisplayCard));
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }
        private void RaiseClickEvent()
        {
            RoutedEventArgs newEventArgs = new(ClickEvent);
            RaiseEvent(newEventArgs);
        }
        private void OnClick()
        {
            RaiseClickEvent();
        } 
        #endregion
    }
}
