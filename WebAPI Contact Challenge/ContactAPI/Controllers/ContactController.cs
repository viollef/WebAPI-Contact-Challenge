using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using Newtonsoft.Json;

namespace ContactsAPI.Controllers
{
    [Route("contactsapi/contact")]
    [Authorize]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly ContactContext _context;

        public ContactController(ContactContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            List<Contact> contacts = await _context.Contacts.Include(c => c.Skills).ToListAsync();

            foreach (Contact contact in contacts)
            {
                foreach (Skill skill in contact.Skills)
                {
                    skill.Contacts ??= new List<Contact>();
                    skill.Contacts.Clear();
                }
            }

            return contacts;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(long id)
        {
            Contact contact = await _context.Contacts.Include(c => c.Skills).FirstOrDefaultAsync(c => c.ID == id);

            if (contact == null)
            {
                return NotFound();
            }

            foreach (Skill skill in contact.Skills)
            {
                skill.Contacts ??= new List<Contact>();
                skill.Contacts.Clear();
            }

            return contact;
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(long id, Contact contact)
        {
            if (id != contact.ID)
            {
                return BadRequest();
            }

            string subjectId = User.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            Contact contactInDB = _context.Contacts.Include(c => c.Skills).Where(c => c.ID == id).Single();

            if (contactInDB.CreatorUserId != int.Parse(subjectId))
            {
                return Unauthorized();
            }

            _context.Entry(contactInDB).CurrentValues.SetValues(contact);

            foreach (Skill skill in contactInDB.Skills)
            {
                if (contact.Skills == null || !contact.Skills.Any(s => s.ID == skill.ID))
                {
                    contactInDB.Skills.Remove(skill);
                }
            }

            if (contact.Skills != null)
            {
                foreach (Skill newSkill in contact.Skills)
                {
                    Skill skill = await _context.Skills.AsNoTracking().FirstOrDefaultAsync(c => c.ID == newSkill.ID);

                    if (skill != null)
                    {
                        skill.Contacts = null;
                        newSkill.Contacts = null;

                        if (JsonConvert.SerializeObject(skill) != JsonConvert.SerializeObject(newSkill) &&
                        skill.CreatorUserId != int.Parse(subjectId))
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        newSkill.CreatorUserId = int.Parse(subjectId);
                    }
                    skill = contactInDB.Skills.SingleOrDefault(s => s.ID == newSkill.ID);

                    if (skill != null)
                    {
                        _context.Entry(skill).CurrentValues.SetValues(newSkill);
                    }
                    else
                    {
                        contactInDB.Skills.Add(newSkill);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        [HttpPost]
        public async Task<ActionResult<Contact>> PostContact(Contact contact)
        {
            string subjectId = User.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;

            contact.CreatorUserId = int.Parse(subjectId);
            _context.Entry(contact).State = EntityState.Added;

            if (contact.Skills != null)
            {
                foreach (Skill newSkill in contact.Skills)
                {
                    Skill skill = await _context.Skills.AsNoTracking().FirstOrDefaultAsync(c => c.ID == newSkill.ID);

                    if (skill != null)
                    {
                        skill.Contacts = null;
                        newSkill.Contacts = null;

                        if (JsonConvert.SerializeObject(skill) != JsonConvert.SerializeObject(newSkill) &&
                        skill.CreatorUserId != int.Parse(subjectId))
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        newSkill.CreatorUserId = int.Parse(subjectId);
                    }
                }
            }
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContact), new { id = contact.ID }, contact);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(long id)
        {
            string subjectId = User.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            Contact contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }

            if (contact.CreatorUserId != int.Parse(subjectId))
            {
                return Unauthorized();
            }

            _context.Entry(contact).State = EntityState.Deleted;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactExists(long id)
        {
            return _context.Contacts.Any(c => c.ID == id);
        }
    }
}
