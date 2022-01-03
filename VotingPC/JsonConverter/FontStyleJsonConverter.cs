using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using VotingPC.Domain;

namespace VotingPC.JsonConverter;

public class FontStyleJsonConverter : JsonConverter<FontStyle>
{
    private static readonly FontStyleConverter s_styleConverter = new();

    public override FontStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new NotSupportedException(@"Cài đặt kiểu chữ 'fontStyle' không hợp lệ. "
                                            + @"Chỉ chấp nhận string, chi tiết vui lòng đọc README.");

        string styleStr = reader.GetString().ToTitleCase();
        if (string.IsNullOrEmpty(styleStr)) return FontStyles.Normal;
        try
        {
            return (FontStyle) s_styleConverter.ConvertFromString(styleStr)!;
        }
        catch (NotSupportedException)
        {
            throw new JsonException(@"Kiểu chữ trong cài đặt 'fontStyle' không hợp lệ. "
                                    + @"Chi tiết về các giá trị hợp lệ vui lòng đọc README.");
        }
    }

    public override void Write(Utf8JsonWriter writer, FontStyle value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}