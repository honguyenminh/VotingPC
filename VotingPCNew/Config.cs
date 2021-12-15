using System.Windows;
using System.Text.Json.Serialization;

namespace VotingPCNew
{
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
        [JsonConverter(typeof(JsonConverter.FontSizeJsonConverter))]
        public double Size { get; set; }
        [JsonConverter(typeof(JsonConverter.FontStyleJsonConverter))]
        public FontStyle Style { get; set; }
        [JsonConverter(typeof(JsonConverter.FontWeightJsonConverter))]
        public FontWeight Weight { get; set; }
    }
}