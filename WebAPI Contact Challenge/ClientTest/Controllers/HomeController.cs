using ClientTest.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ClientTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> JwtToken()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");

            ViewBag.token = accessToken;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> GenerateAndPostContact()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            string contact = JsonConvert.SerializeObject(GenerateContact());
            HttpContent httpContent = new StringContent(contact, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            client.SetBearerToken(accessToken);
            HttpResponseMessage response = await client.PostAsync("https://localhost:44386/contactsapi/contact", httpContent);
            string rawResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Json = JObject.Parse(rawResponse).ToString();
            }
            else
            {
                ViewBag.Json = "Error: " + response.ReasonPhrase + ". " + rawResponse;
            }
            ViewBag.token = accessToken;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> GenerateAndPostSkill()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            string skill = JsonConvert.SerializeObject(GenerateSkill());
            HttpContent httpContent = new StringContent(skill, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            client.SetBearerToken(accessToken);
            HttpResponseMessage response = await client.PostAsync("https://localhost:44386/contactsapi/skill", httpContent);
            string rawResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Json = JObject.Parse(rawResponse).ToString();
            }
            else
            {
                ViewBag.Json = "Error: " + response.ReasonPhrase + ". " + rawResponse;
            }
            ViewBag.token = accessToken;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> LinkSkillToContact(int contactId, int skillId)
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            string contactJson = await GetContact(contactId, HttpContext);
            string skillJson = await GetSkill(skillId, HttpContext);
            Contact contact;
            Skill skill;

            if (contactId == 0 && skillId == 0)
            {
                ViewBag.Json = "Error: Empty parameter, contactId and skillId can't be null";
            }

            try
            {
                contact = JsonConvert.DeserializeObject<Contact>(contactJson);
            }
            catch
            {
                ViewBag.token = accessToken;
                ViewBag.Json = "Error: " + contactJson;

                return View();
            }

            try
            {
                skill = JsonConvert.DeserializeObject<Skill>(skillJson);
            }
            catch
            {
                ViewBag.token = accessToken;
                ViewBag.Json = "Error: " + skillJson;

                return View();
            }

            skill.Contacts = new List<Contact>();
            contact.Skills ??= new List<Skill>();
            contact.Skills.Add(skill);

            string contactSkillJson = JsonConvert.SerializeObject(contact);
            HttpContent httpContent = new StringContent(contactSkillJson, Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient();

            client.SetBearerToken(accessToken);
            HttpResponseMessage response = await client.PutAsync("https://localhost:44386/contactsapi/contact/" + contactId, httpContent);
            string rawResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Json = "Success";
            }
            else
            {
                ViewBag.Json = "Error: " + response.ReasonPhrase + ". " + rawResponse;
            }
            ViewBag.token = accessToken;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetContacts()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            HttpClient client = new HttpClient();
            client.SetBearerToken(accessToken);

            string content = await client.GetStringAsync("https://localhost:44386/contactsapi/contact");

            try
            {
                ViewBag.Json = JArray.Parse(content).ToString();
            }
            catch
            {
                ViewBag.Json = "Error: " + content;
            }
            ViewBag.token = accessToken;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> GetSkills()
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            HttpClient client = new HttpClient();

            client.SetBearerToken(accessToken);
            string content = await client.GetStringAsync("https://localhost:44386/contactsapi/skill");

            try
            {
                ViewBag.Json = JArray.Parse(content).ToString();
            }
            catch
            {
                ViewBag.Json = "Error: " + content;
            }
            ViewBag.token = accessToken;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> DeleteContact(string contactId)
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            HttpClient client = new HttpClient();
            client.SetBearerToken(accessToken);

            HttpResponseMessage response = await client.DeleteAsync("https://localhost:44386/contactsapi/contact/" + contactId);
            string rawResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Json = "Success";
            }
            else
            {
                ViewBag.Json = "Error: " + response.ReasonPhrase + ". " + rawResponse;
            }
            ViewBag.token = accessToken;

            return View();
        }

        [Authorize]
        public async Task<IActionResult> DeleteSkill(string skillId)
        {
            string accessToken = await HttpContext.GetTokenAsync("access_token");
            HttpClient client = new HttpClient();
            client.SetBearerToken(accessToken);

            HttpResponseMessage response = await client.DeleteAsync("https://localhost:44386/contactsapi/skill/" + skillId);
            string rawResponse = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Json = "Success";
            }
            else
            {
                ViewBag.Json = "Error: " + response.ReasonPhrase + ". " + rawResponse;
            }
            ViewBag.token = accessToken;

            return View();
        }

        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync("oidc");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static Contact GenerateContact()
        {
            var rand = new Random();

            Contact contact = new Contact()
            {
                ID = 0,
                FirstName = "FirstName" + rand.Next(0, 314159),
                LastName = "LastName" + rand.Next(0, 314159),
                FullName = "",
                Address = @"{ 'street_address': '" + rand.Next(0, 35) + " Avenue François Collignon', 'locality': 'Toulouse', 'postal_code': 31200, 'country': 'France' }",
                Email = "",
                MobilePhoneNumber = "+336582311" + rand.Next(0, 10) + rand.Next(0, 10)
            };
            contact.FullName = contact.FirstName + " MiddleName " + rand.Next(0, 314159) + contact.LastName;
            contact.Email = contact.FirstName + "Du31@gmail.com";

            return contact;
        }

        private static Skill GenerateSkill()
        {
            var rand = new Random();

            return new Skill()
            {
                ID = 0,
                Name = "Skill" + rand.Next(0, 314159),
                Level = (Level)rand.Next(0, 5)
            };
        }

        private static async Task<string> GetContact(int contactId, HttpContext httpContext)
        {
            string accessToken = await httpContext.GetTokenAsync("access_token");
            HttpClient client = new HttpClient();
            client.SetBearerToken(accessToken);

            try
            {
                return await client.GetStringAsync("https://localhost:44386/contactsapi/contact/" + contactId);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private static async Task<string> GetSkill(int skillId, HttpContext httpContext)
        {
            string accessToken = await httpContext.GetTokenAsync("access_token");
            HttpClient client = new HttpClient();
            client.SetBearerToken(accessToken);

            try
            {
                return await client.GetStringAsync("https://localhost:44386/contactsapi/skill/" + skillId);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
