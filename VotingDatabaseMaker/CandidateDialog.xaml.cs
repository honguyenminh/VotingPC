namespace VotingDatabaseMaker;

/// <summary>
/// Interaction logic for CandidateDialog.xaml
/// </summary>
public partial class CandidateDialog
{
    // Public properties
    public string NameInput { get; set; } = "";
    public string GenderInput { get; set; } = "";

    public CandidateDialog()
    {
        InitializeComponent();
    }
}