using System.Collections.Generic;
using System.Windows;
using VotingPC;

namespace VoteCounter;

/// <summary>
/// Interaction logic for DisplayCard.xaml
/// </summary>
public partial class DisplayCard
{
    // Public properties
    public Info SectionInfo { get; }
    public List<Candidate> Candidates { get; }

    // Constructors
    public DisplayCard()
    {
        PreviewMouseLeftButtonUp += (_, _) => OnClick();
        InitializeComponent();
    }

    public DisplayCard(Info info, List<Candidate> candidates)
    {
        SectionInfo = info;
        Candidates = candidates;
        PreviewMouseLeftButtonUp += (_, _) => OnClick();
        InitializeComponent();

        // Set details to parameters
        sectionTextBlock.Text = info.Title ?? info.Sector;

        // Show winning candidate
        nameTextBlock.Text = candidates[0].Name;
        totalVoteTextBlock.Text = candidates[0].Votes.ToString();

        // Show total winning places count
        totalWinningPlacesTextBlock.Text = candidates[0].TotalWinningPlaces.ToString();
    }

    // Click event setup
    #region Click event setup

    public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent("Click", RoutingStrategy.Bubble,
        typeof(RoutedEventHandler), typeof(DisplayCard));

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