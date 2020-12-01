using System.ComponentModel.DataAnnotations;

namespace ContactsAPI.Models
{
    public class Contact
    {
        public long ID { get; set; }

        [MaxLength(35)]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(35)]
        [Required]
        public string LastName { get; set; }

        [MaxLength(105)]
        [Required]
        public string FullName { get; set; }

        [MaxLength(255)]
        [Required]
        public string Address { get; set; }

        [MaxLength(255)]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Phone]
        [Required]
        public string MobilePhoneNumber { get; set; }
    }
}
