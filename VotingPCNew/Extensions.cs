using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VotingPCNew
{
    internal static class Extensions
    {
        private static readonly ImageSourceConverter imageConverter = new();
        public static void SetConfig(this TextBlock textBlock, TextConfig config)
        {
            textBlock.Text = config.Text ?? "";
            if (config.Size != -1) textBlock.FontSize = config.Size;
            textBlock.FontStyle = config.Style;
            textBlock.FontWeight = config.Weight;
        }
        public static void SetSource(this Image image, string path)
        {
            image.Source = (ImageSource)imageConverter.ConvertFromString(path);
        }
        public static string ToTitleCase(this string str)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);
        }
    }
}
