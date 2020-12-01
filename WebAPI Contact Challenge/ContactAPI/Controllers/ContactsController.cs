using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactsAPI.Models;

namespace ContactsAPI.Controllers
{
    [Route("contactsapi/contact")]
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
            return await _context.Contacts.ToListAsync();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetContact(long id)
        {
            var contacts = await _context.Contacts.FirstOrDefaultAsync(i => i.ID == id);

            if (contacts == null)
            {
                return NotFound();
            }

            return contacts;
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutContact(long id, Contact contact)
        {
            if (id != contact.ID)
            {
                return BadRequest();
            }

            Contact contactInDB = _context.Contacts.Where(i => i.ID == id).Single();

            _context.Entry(contactInDB).CurrentValues.SetValues(contact);

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
