using SQLite;
using System.Text.RegularExpressions;

namespace VotingPC
{
    internal static class GlobalVariable
    {
        public const int StringMaxLength = 128;
        public const int SmallStringMaxLength = 64;
    }

    public class Candidate
    {
        [Ignore]
        public ulong TotalWinningPlaces { get; set; }

        [NotNull, PrimaryKey, Unique]
        [Column("Name")]
        public string Name { get; set; }

        [NotNull]
        [Column("Votes")]
        public long Votes { get; set; }

        [NotNull]
        [Column("Gender")]
        public string Gender { get; set; }

        // Return true if required properties are not null, also reset votes to be safe
        [Ignore]
        public bool IsValid => Name != null && Gender != null;
    }

    [Table("master")]
    public class Sector
    {
        // Hex color regex
        private static readonly Regex s_hexColorRegex = new("^#([0-9A-F]{8}|[0-9A-F]{6})$", RegexOptions.Compiled);
        // Private fields
        private string _sector;
        private string _color;
        [Ignore]
        public string Error { get; private set; } = "";

        [NotNull, PrimaryKey, Unique]
        [Column("Sector")]
        public string Name
        {
            get => _sector;
            set
            {
                if (value.Length > GlobalVariable.SmallStringMaxLength)
                {
                    Error += $"Tên Sector quá dài (hơn {GlobalVariable.SmallStringMaxLength} ký tự).\n";
                }
                else _sector = value;
            }
        }

        [NotNull]
        [Column("Max")]
        public int? Max { get; set; }

        [NotNull]
        [Column("Color")]
        public string Color
        {
            get => _color;
            set
            {
                if (value is null)
                {
                    _color = null;
                    return;
                }

                value = value.Trim().ToUpper();
                if (!s_hexColorRegex.IsMatch(value))
                {
                    Error += $"Màu nền RGB không hợp lệ tại Sector {Name}.\n";
                    _color = null;
                    return;
                }
                _color = value;
            }
        }

        [NotNull]
        [Column("Title")]
        public string Title { get; set; }

        [NotNull]
        [Column("Subtitle")]
        public string Subtitle { get; set; }

        [Ignore]
        public int TotalVoted { get; set; }
        [Ignore]
        public string ColorNoHash
        {
            get => Color?[1..];
            set => Color = '#' + value;
        }
        [Ignore]
        public bool IsValid
        {
            get
            {
                // Db is valid if all members are not null
                bool isInvalid = 
                    string.IsNullOrWhiteSpace(Name) ||
                    Title == null ||
                    Color == null ||
                    Subtitle == null ||
                    Max == null;
                return !isInvalid;
            }
        }
    }
}