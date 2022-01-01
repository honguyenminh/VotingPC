namespace VotingPCNew.Slides;

/// <summary>
/// Interaction logic for Slide1_Welcome.xaml
/// </summary>
public partial class IntroSlide
{
    public TextConfig TitleConfig { set => title.SetConfig(value); }
    public string IconPath { set => image.SetSource(value); }
    public TextConfig TopHeaderConfig { set => topHeader.SetConfig(value); }
    public TextConfig TopSubheaderConfig { set => topSubheader.SetConfig(value); }

    public IntroSlide()
    {
        InitializeComponent();
    }
}
