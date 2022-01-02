using System.Text.Json.Serialization;
using System.Windows;
using VotingPCNew.JsonConverter;

namespace VotingPCNew;

public struct Config
{
    public string IconPath { get; set; }
    public TextConfig Header { get; set; }
    public TextConfig Subheader { get; set; }
    public TextConfig Title { get; set; }
}

public struct TextConfig
{
    public string Text { get; set; }

    [JsonConverter(typeof(FontSizeJsonConverter))]
    public double Size { get; set; }

    [JsonConverter(typeof(FontStyleJsonConverter))]
    public FontStyle Style { get; set; }

    [JsonConverter(typeof(FontWeightJsonConverter))]
    public FontWeight Weight { get; set; }
}