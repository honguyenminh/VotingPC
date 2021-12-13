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
            textBlock.Text = config.text ?? "";
            if (config.size != -1) textBlock.FontSize = config.size;
            textBlock.FontStyle = config.style;
            textBlock.FontWeight = config.weight;
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
