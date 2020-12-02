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
    [Route("contactsapi/skill")]
    [Authorize]
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
            List<Skill> skills = await _context.Skills.Include(p => p.Contacts).ToListAsync();

            foreach (Skill skill in skills)
            {
                foreach (Contact contact in skill.Contacts)
                {
                    contact.Skills ??= new List<Skill>();
                    contact.Skills.Clear();
                }
            }

            return skills;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Skill>> GetSkill(long id)
        {
            Skill skill = await _context.Skills.Include(p => p.Contacts).FirstOrDefaultAsync(i => i.ID == id);

            if (skill == null)
            {
                return NotFound();
            }

            foreach (Contact contact in skill.Contacts)
            {
                contact.Skills ??= new List<Skill>();
                contact.Skills.Clear();
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

            string subjectId = User.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            Skill skillInDB = _context.Skills.Include(i => i.Contacts).Where(i => i.ID == id).Single();
            
            if (skillInDB.CreatorUserId != int.Parse(subjectId))
            {
                return Unauthorized();
            }
            _context.Entry(skillInDB).CurrentValues.SetValues(skill);

            foreach (Contact contact in skillInDB.Contacts)
            {
                if (skill.Contacts == null || !skill.Contacts.Any(i => i.ID == contact.ID))
                {
                    skillInDB.Contacts.Remove(contact);
                }
            }

            if (skill.Contacts != null)
            {
                foreach (Contact newContact in skill.Contacts)
                {
                    Contact contact = await _context.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.ID == newContact.ID);

                    if (contact != null)
                    {
                        contact.Skills = null;
                        newContact.Skills = null;

                        if (JsonConvert.SerializeObject(contact) != JsonConvert.SerializeObject(newContact) &&
                         contact.CreatorUserId != int.Parse(subjectId))
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        newContact.CreatorUserId = int.Parse(subjectId);
                    }
                    contact = skillInDB.Contacts.SingleOrDefault(c => c.ID == newContact.ID);

                    if (contact == null)
                    {
                        skillInDB.Contacts.Add(newContact);
                    }
                    else
                    {
                        _context.Entry(contact).CurrentValues.SetValues(newContact);
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
            string subjectId = User.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;

            skill.CreatorUserId = int.Parse(subjectId);

            if (skill.Contacts != null)
            {
                foreach (Contact newContact in skill.Contacts)
                {
                    Contact contact = await _context.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.ID == newContact.ID);

                    if (contact != null)
                    {
                        contact.Skills = null;
                        newContact.Skills = null;

                        if (JsonConvert.SerializeObject(contact) != JsonConvert.SerializeObject(newContact) &&
                         contact.CreatorUserId != int.Parse(subjectId))
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        newContact.CreatorUserId = int.Parse(subjectId);
                    }
                }
            }
            _context.Entry(skill).State = EntityState.Added;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSkill), new { id = skill.ID }, skill);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSkill(long id)
        {
            string subjectId = User.Claims?.FirstOrDefault(c => c.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase))?.Value;
            Skill skill = await _context.Skills.FindAsync(id);

            if (skill == null)
            {
                return NotFound();
            }

            if (skill.CreatorUserId != int.Parse(subjectId))
            {
                return Unauthorized();
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
