using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace VotingPC
{
    public class Scale
    {
        [NotNull, PrimaryKey, Unique]
        [Column("Name")]
        public string Name { get; set; }

        [NotNull]
        [Column("Votes")]
        public int Votes { get; set; }

        public string Gender { get; set; }
    }
    public class Info
    {
        [NotNull, PrimaryKey, Unique]
        [Column("Scale")]
        public string Scale { get; set; }

        [NotNull]
        [Column("Max")]
        public int Max { get; set; }

        [NotNull]
        [Column("Color")]
        public string Color { get; set; }

        [Column("Title")]
        public string Title { get; set; }
        
        [Column("Year")]
        public string Year { get; set; }

        public int TotalVoted { get; set; } = 0;
    }
}
