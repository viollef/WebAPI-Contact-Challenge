using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContactsAPI.Models
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

        [MaxLength(35)]
        [Required]
        public string Name { get; set; }

        [Range(0, 4)]
        [Required]
        public Level Level { get; set; }

        //Navigation Propertie
        public ICollection<Contact> Contacts { get; set; }
    }
}
