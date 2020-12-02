using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactsAPI.Models;
using Microsoft.AspNetCore.Authorization;

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
            return await _context.Contacts.Include(p => p.Skills).ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(long id)
        {
            Contact contact = await _context.Contacts.Include(p => p.Skills).FirstOrDefaultAsync(i => i.ID == id);

            if (contact == null)
            {
                return NotFound();
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

            Contact contactInDB = _context.Contacts.Include(i => i.Skills).Where(i => i.ID == id).Single();

            _context.Entry(contactInDB).CurrentValues.SetValues(contact);

            foreach (Skill skill in contactInDB.Skills)
            {
                if (contact.Skills == null || !contact.Skills.Any(i => i.ID == skill.ID))
                {
                    contactInDB.Skills.Remove(skill);
                }
            }

            if (contact.Skills != null)
            {
                foreach (Skill newSkill in contact.Skills)
                {
                    Skill skill = contactInDB.Skills.SingleOrDefault(i => i.ID == newSkill.ID);

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
            _context.Entry(contact).State = EntityState.Added;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetContact), new { id = contact.ID }, contact);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(long id)
        {
            var contact = await _context.Contacts.FindAsync(id);

            if (contact == null)
            {
                return NotFound();
            }
            _context.Entry(contact).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactExists(long id)
        {
            return _context.Contacts.Any(e => e.ID == id);
        }
    }
}
