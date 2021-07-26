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
    /// Interaction logic for Slide3_Detail.xaml
    /// </summary>
    public partial class Slide3_Detail : UserControl
    {
        public Slide3_Detail()
        {
            InitializeComponent();
        }
        public void ChangeData(Info info, List<Candidate> candidates)
        {

            if ((info == null) || (candidates == null) || (candidates.Count == 0))
                throw new ArgumentException("Null argument or empty candidate list");

            titleTextBox.Text = "Chi tiết - " + info.Title;

            detailStackPanel.Children.Clear();

            long position = 1, previousVotes = candidates[0].Votes;
            foreach (Candidate candidate in candidates)
            {
                if (candidate.Votes < previousVotes)
                {
                    position++;
                    previousVotes = candidate.Votes;
                }

                DetailCard detailCard = new(position, candidate.Name, candidate.Votes);

                detailCard.Margin = position switch
                {
                    1 => new(56, 16, 56, 16),
                    2 => new(64, 16, 64, 16),
                    3 => new(72, 16, 72, 16),
                    _ => new(80, 16, 80, 16)
                };

                _ = detailStackPanel.Children.Add(detailCard);
            }
        }
    }
}
