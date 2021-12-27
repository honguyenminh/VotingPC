using System;
using System.Collections.Generic;
using VotingPC;

namespace VoteCounter;

/// <summary>
/// Interaction logic for Slide3_Detail.xaml
/// </summary>
public partial class Slide3_Detail
{
    public Slide3_Detail()
    {
        InitializeComponent();
    }
    public void ChangeData(Sector info, List<Candidate> candidates)
    {

        if ((info == null) || (candidates == null) || (candidates.Count == 0))
            throw new ArgumentException("Null argument or empty candidate list");

        titleTextBox.Text = info.Title;

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

            // Add custom margin for the card based on the rank of user
            detailCard.Margin = position switch
            {
                1 => new(0, 16, 0, 16),
                2 => new(32, 16, 32, 16),
                3 => new(64, 16, 64, 16),
                _ => new(80, 16, 80, 16)
            };

            _ = detailStackPanel.Children.Add(detailCard);
        }
    }
}