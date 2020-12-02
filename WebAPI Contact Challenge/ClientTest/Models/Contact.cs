﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClientTest.Models
{
    public class Contact
    {
        public long ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string Email { get; set; }

        public string MobilePhoneNumber { get; set; }

        public ICollection<Skill> Skills { get; set; }
    }
}
