using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace VotingDatabaseMaker;

public class NotEmptyValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        return string.IsNullOrWhiteSpace((value ?? "").ToString())
            ? new ValidationResult(false, "Không để trống")
            : ValidationResult.ValidResult;
    }
}
public class HexColorNoHashValidationRule : ValidationRule
{
    // If flexible hash sign is needed, use ^(#{0,1})([0-9A-F]{8}|[0-9A-F]{6})$
    private static readonly Regex s_hexColorRegex = new("^([0-9A-F]{8}|[0-9A-F]{6})$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        if (value is null) return new ValidationResult(false, "Không hợp lệ");
        string newValue = value.ToString();
        if (newValue!.Length is not 6 and not 8)
            return new ValidationResult(false, "Không hợp lệ");

        return s_hexColorRegex.IsMatch(newValue) ? ValidationResult.ValidResult : new ValidationResult(false, "Không hợp lệ");
    }
}