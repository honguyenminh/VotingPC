namespace VotingPC.Slides;

/// <summary>
///     Interaction logic for IntroSlide
/// </summary>
public partial class IntroSlide
{
    public IntroSlide()
    {
        InitializeComponent();
    }

    public TextConfig TitleConfig
    {
        set => title.SetConfig(value);
    }

    public string IconPath
    {
        set => image.SetSource(value);
    }

    public TextConfig TopHeaderConfig
    {
        set => topHeader.SetConfig(value);
    }

    public TextConfig TopSubheaderConfig
    {
        set => topSubheader.SetConfig(value);
    }
}