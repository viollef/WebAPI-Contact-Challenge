using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContactsAPI.Models;

namespace ContactsAPI.Controllers
{
    [Route("contactsapi/skill")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly ContactContext _context;

        public SkillController(ContactContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Skill>>> GetSkills()
        {
            return await _context.Skills.Include(p => p.Contacts).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> GetSkill(long id)
        {
            Skill skill = await _context.Skills.Include(p => p.Contacts).FirstOrDefaultAsync(i => i.ID == id);

            if (skill == null)
            {
                return NotFound();
            }

            return skill;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSkill(long id, Skill skill)
        {
            if (id != skill.ID)
            {
                return BadRequest();
            }

            Skill skillInDB = _context.Skills.Include(i => i.Contacts).Where(i => i.ID == id).Single();

            _context.Entry(skillInDB).CurrentValues.SetValues(skill);

            foreach (Contact contact in skillInDB.Contacts)
            {
                if (skill.Contacts == null || !skill.Contacts.Any(i => i.ID == skill.ID))
                {
                    skillInDB.Contacts.Remove(contact);
                }
            }

            if (skill.Contacts != null)
            {
                foreach (Contact newContact in skill.Contacts)
                {
                    Contact contact = skillInDB.Contacts.SingleOrDefault(i => i.ID == newContact.ID);

                    if (contact != null)
                    {
                        _context.Entry(contact).CurrentValues.SetValues(newContact);
                    }
                    else
                    {
                        skillInDB.Contacts.Add(newContact);
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SkillExists(id))
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
        public async Task<ActionResult<Skill>> PostSkill(Skill skill)
        {
            _context.Entry(skill).State = EntityState.Added;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSkill), new { id = skill.ID }, skill);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(long id)
        {
            Skill skill = await _context.Skills.FindAsync(id);

            if (skill == null)
            {
                return NotFound();
            }

            _context.Entry(skill).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SkillExists(long id)
        {
            return _context.Skills.Any(e => e.ID == id);
        }
    }
}
