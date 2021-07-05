using SQLite;

namespace VotingPC
{
    public class Section
    {
        [NotNull, PrimaryKey, Unique]
        [Column("Name")]
        public string Name { get; set; }

        [NotNull]
        [Column("Votes")]
        public long? Votes { get; set; }

        [NotNull]
        [Column("Gender")]
        public string Gender { get; set; }

        // Return true if all properties are not null
        public bool IsValid => Name != null && Gender != null && Votes != null;
    }
    [Table("Info")]
    public class Info
    {
        [NotNull, PrimaryKey, Unique]
        [Column("Section")]
        public string Section { get; set; }

        [NotNull]
        [Column("Max")]
        public int? Max { get; set; }

        [NotNull]
        [Column("Color")]
        public string Color { get; set; }

        [NotNull]
        [Column("Title")]
        public string Title { get; set; }

        [NotNull]
        [Column("Year")]
        public string Year { get; set; }

        public int TotalVoted { get; set; }

        // Return true if all properties are not null
        public bool IsValid => Section != null && Color != null && Title != null && Year != null && Max != null;
    }
}
