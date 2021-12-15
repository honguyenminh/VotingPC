using SQLite;
using System;
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
        // Private fields
        private string _name;
        private string _gender;

        // Public field
        [Ignore]
        public ulong TotalWinningPlaces { get; set; }

        [NotNull, PrimaryKey, Unique]
        [Column("Name")]
        public string Name
        {
            get => _name;
            // Cut the string if longer than StringMaxLength
            set
            {
                if (value.Length > GlobalVariable.StringMaxLength)
                {
                    _name = value[..GlobalVariable.StringMaxLength];
                }
                else _name = value;
            }
        }

        [NotNull]
        [Column("Votes")]
        public long Votes { get; set; }

        [NotNull]
        [Column("Gender")]
        public string Gender
        {
            get => _gender;
            // Cut the string if longer than SmallStringMaxLength
            set
            {
                if (value.Length > GlobalVariable.SmallStringMaxLength)
                {
                    _gender = value[..GlobalVariable.SmallStringMaxLength];
                }
                else _gender = value;
            }
        }

        // Return true if required properties are not null, also reset votes to be safe
        [Ignore]
        public bool IsValid => Name != null && Gender != null;
    }

    [Table("Info")]
    public class Info
    {
        // Private fields
        private string _sector;
        private string _color;
        private string _title;
        private string _year;
        // Hex color regex
        private static readonly Regex s_hexColorRegex = new("^#([0-9A-F]{8}|[0-9A-F]{6})$", RegexOptions.Compiled);

        [Ignore]
        public string Error { get; private set; } = "";
        [Ignore]
        public string Warning { get; private set; } = "";

        [NotNull, PrimaryKey, Unique]
        [Column("Sector")]
        public string Sector
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
                if (value.Length is 7 or 9)
                {
                    if (s_hexColorRegex.IsMatch(value))
                    {
                        _color = value;
                    }
                    else
                    {
                        Error += $"Màu nền RGB không hợp lệ tại Sector {Sector}.\n";
                        _color = null;
                    }
                }
                else
                {
                    Error += $"Màu nền không hợp lệ tại Sector {Sector}.\nVui lòng kiểm tra lại độ dài mã màu.\n";
                    _color = null;
                }
            }
        }

        [NotNull]
        [Column("Title")]
        public string Title
        {
            get => _title;
            set
            {
                if (value.Length > GlobalVariable.StringMaxLength)
                {
                    Warning += $"Tiêu đề Sector {Sector} quá dài (hơn {GlobalVariable.StringMaxLength} ký tự). Đã tự động cắt.\n";
                    _title = value.Substring(0, GlobalVariable.StringMaxLength);
                }
                else _title = value;
            }
        }

        [NotNull]
        [Column("Year")]
        public string Year
        {
            get => _year;
            set
            {
                if (value.Length > GlobalVariable.SmallStringMaxLength)
                {
                    Warning += $"Phụ đề niên khóa của Sector {Sector} quá dài (hơn {GlobalVariable.SmallStringMaxLength} ký tự). Đã tự động cắt.\n";
                    _year = value[..GlobalVariable.SmallStringMaxLength];
                }
                else _year = value;
            }
        }

        [Ignore]
        public int TotalVoted { get; set; }
        [Ignore]
        public string ColorNoHash
        {
            get => Color?[1..];
            set => Color = '#' + value;
        }

        [Ignore]
        // Return true if all properties are not null and empty
        public bool IsValid => !string.IsNullOrWhiteSpace(Sector) &&
            !string.IsNullOrWhiteSpace(Color) &&
            !string.IsNullOrWhiteSpace(Title) &&
            Year != null && Max != null;
    }
}
