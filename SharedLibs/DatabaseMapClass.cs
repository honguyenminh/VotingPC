using SQLite;
using System;
using System.Text.RegularExpressions;

// TODO: finish making database error logs
// TODO: throw after all error found.

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
        private string name;
        private string gender;

        [NotNull, PrimaryKey, Unique]
        [Column("Name")]
        public string Name
        {
            get => name;
            // Cut the string if longer than StringMaxLength
            set
            {
                if (value.Length > GlobalVariable.StringMaxLength)
                {
                    name = value.Substring(0, GlobalVariable.StringMaxLength);
                }
                else name = value;
            }
        }

        [NotNull]
        [Column("Votes")]
        public long? Votes { get; set; }

        [NotNull]
        [Column("Gender")]
        public string Gender
        {
            get => gender;
            // Cut the string if longer than StringMaxLength
            set
            {
                if (value.Length > GlobalVariable.SmallStringMaxLength)
                {
                    gender = value.Substring(0, GlobalVariable.SmallStringMaxLength);
                }
                else gender = value;
            }
        }

        // Return true if all properties are not null
        public bool IsValid => Name != null && Gender != null && Votes != null;
    }
    [Table("Info")]
    public class Info : IEquatable<Info>
    {
        // Compare method
        public bool Equals(Info other)
        {
            return (other.Section == Section) &&
                (other.Color == Color) &&
                (other.Max == Max) &&
                (other.Title == Title) &&
                (other.Year == Year);
        }

        // Private fields
        private string section;
        private string color;
        private string title;
        private string year;

        public string Error { get; private set; } = "";
        public string Warning { get; private set; } = "";

        [NotNull, PrimaryKey, Unique]
        [Column("Section")]
        public string Section
        {
            get => section;
            set
            {
                if (value.Length > GlobalVariable.SmallStringMaxLength)
                {
                    Error += $"Tên Section quá dài (hơn {GlobalVariable.SmallStringMaxLength} ký tự).\n";
                }
                else section = value;
            }
        }

        [NotNull]
        [Column("Max")]
        public int? Max { get; set; }

        [NotNull]
        [Column("Color")]
        public string Color
        {
            get => color;
            set
            {
                value = value.Trim();
                // Is #RGB
                if (value.Length == 7)
                {
                    if (new Regex("^#(?:[0-9a-fA-F]{3}){1,2}$").Match(value).Success)
                    {
                        color = value;
                    }
                    else
                    {
                        Error += $"Màu nền RGB không hợp lệ tại Section {Section}.\n";
                    }
                }
                // Is #ARGB
                else if (value.Length == 9)
                {
                    if (new Regex("^#(?:[0-9a-fA-F]{3,4}){1,2}$").Match(value).Success)
                    {
                        color = value;
                    }
                    else
                    {
                        Error += $"Màu nền ARGB không hợp lệ tại Section {Section}.\n";
                    }
                }
                else
                {
                    Error += $"Màu nền không hợp lệ tại Section {Section}.\nVui lòng kiểm tra lại độ dài mã màu.\n";
                }
            }
        }

        [NotNull]
        [Column("Title")]
        public string Title
        {
            get => title;
            set
            {
                if (value.Length > GlobalVariable.StringMaxLength)
                {
                    Warning += $"Tiêu đề Section {Section} quá dài (hơn {GlobalVariable.StringMaxLength} ký tự). Đã tự động cắt.\n";
                    title = value.Substring(0, GlobalVariable.StringMaxLength);
                }
                else title = value;
            }
        }

        [NotNull]
        [Column("Year")]
        public string Year
        {
            get => year;
            set
            {
                if (value.Length > GlobalVariable.SmallStringMaxLength)
                {
                    Warning += $"Phụ đề niên khóa của Section {Section} quá dài (hơn {GlobalVariable.SmallStringMaxLength} ký tự). Đã tự động cắt.\n";
                    year = value.Substring(0, GlobalVariable.SmallStringMaxLength);
                }
                else year = value;
            }
        }

        public int TotalVoted { get; set; }

        // Return true if all properties are not null
        public bool IsValid => Section != null && Color != null && Title != null && Year != null && Max != null;
    }
}
