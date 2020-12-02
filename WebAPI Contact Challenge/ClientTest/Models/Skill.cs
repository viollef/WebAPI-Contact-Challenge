using System.Collections.Generic;

namespace ClientTest.Models
{
    public enum Level
    {
        Novice,
        AdvancedBeginner,
        Competent,
        Proficient,
        Expert
    }

    public class Skill
    {
        public long ID { get; set; }

        public long CreatorUserId { get; set; }

        public string Name { get; set; }

        public Level Level { get; set; }

        public ICollection<Contact> Contacts { get; set; }
    }
}
