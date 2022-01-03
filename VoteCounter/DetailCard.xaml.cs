namespace VoteCounter;

/// <summary>
/// Interaction logic for DetailCard.xaml
/// </summary>
public partial class DetailCard
{
    public DetailCard()
    {
        InitializeComponent();
    }
    public DetailCard(long position, string name, long totalVotes)
    {
        InitializeComponent();
        positionTextBlock.Text = position.ToString();
        nameTextBlock.Text = name;
        totalVotesTextBlock.Text = totalVotes.ToString();
    }
}