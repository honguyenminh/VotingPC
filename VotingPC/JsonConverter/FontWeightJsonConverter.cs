using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using VotingPC.Domain.Extensions;

namespace VotingPC.JsonConverter;

public class FontWeightJsonConverter : JsonConverter<FontWeight>
{
    private static readonly FontWeightConverter s_weightConverter = new();

    public override FontWeight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new NotSupportedException(@"Cài đặt in đậm/nhạt chữ 'fontWeight' không hợp lệ. "
                                            + @"Chỉ chấp nhận string, chi tiết vui lòng đọc README.");

        string weightStr = reader.GetString().ToTitleCase();
        if (string.IsNullOrEmpty(weightStr)) return FontWeights.Normal;
        try
        {
            return (FontWeight) s_weightConverter.ConvertFromString(weightStr)!;
        }
        catch (NotSupportedException)
        {
            throw new JsonException(@"In đậm/nhạt chữ trong cài đặt 'fontWeight' không hợp lệ. "
                                    + @"Chi tiết về các giá trị hợp lệ vui lòng đọc README.");
        }
    }

    public override void Write(Utf8JsonWriter writer, FontWeight value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}