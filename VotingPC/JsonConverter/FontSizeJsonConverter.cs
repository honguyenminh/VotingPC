using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace VotingPC.JsonConverter;

public class FontSizeJsonConverter : JsonConverter<double>
{
    private static readonly FontSizeConverter s_sizeConverter = new();

    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number) return reader.GetDouble();
        if (reader.TokenType != JsonTokenType.String)
            throw new NotSupportedException(@"Cỡ chữ trong cài đặt 'fontWeight' không hợp lệ. "
                                            + @"Chấp nhận số hoặc string chứa giá trị có đơn vị pixel (px)");
        string styleStr = reader.GetString();
        if (string.IsNullOrWhiteSpace(styleStr)) return -1;
        try
        {
            return (double) s_sizeConverter.ConvertFromString(styleStr)!;
        }
        catch (NotSupportedException)
        {
            throw new JsonException(@"Cỡ chữ trong cài đặt 'fontSize' không hợp lệ. "
                                    + @"Chấp nhận số hoặc string chứa giá trị có đơn vị pixel (px)");
        }
    }

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}