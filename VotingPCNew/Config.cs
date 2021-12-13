using System.Windows;
using System.Text.Json.Serialization;
// ReSharper disable InconsistentNaming

namespace VotingPCNew
{
#pragma warning disable IDE1006 // JavaScript style name for json
    public class Config
    {
        //[JsonConverter(typeof(JsonConverter.ImageSource))]
        public string iconPath { get; set; }
        public TextConfig header { get; set; }
        public TextConfig subheader { get; set; }
        public TextConfig title { get; set; }
    }

    public class TextConfig
    {
        public string text { get; set; }
        [JsonConverter(typeof(JsonConverter.FontSizeJsonConverter))]
        public double size { get; set; }
        [JsonConverter(typeof(JsonConverter.FontStyleJsonConverter))]
        public FontStyle style { get; set; }
        [JsonConverter(typeof(JsonConverter.FontWeightJsonConverter))]
        public FontWeight weight { get; set; }
    }
#pragma warning restore IDE1006 // JavaScript style name for json
}