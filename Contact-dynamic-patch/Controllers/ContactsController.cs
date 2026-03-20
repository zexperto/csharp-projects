using ContactApi.DTO;
using ContactApi.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ContactApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContactDto>>> GetContacts()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ContactDto>> GetContact(int id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            return Ok(contact);
        }

        [HttpPost]
        public async Task<ActionResult<ContactDto>> CreateContact(CreateContactDto createContactDto)
        {
            var contact = await _contactService.CreateContactAsync(createContactDto);
            return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContact(int id, UpdateContactDto updateContactDto)
        {
            var updatedContact = await _contactService.UpdateContactAsync(id, updateContactDto);
            if (updatedContact == null)
            {
                return NotFound();
            }
            return Ok(updatedContact);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContact(int id)
        {
            var result = await _contactService.DeleteContactAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPatch("p1/{id}")]
        [Consumes("application/json-patch+json")]

        public async Task<IActionResult> PatchContact(int id, [FromBody] JsonPatchDocument<ContactApi.Models.Contact> patchDoc)
        {
            if (patchDoc is null) return BadRequest("Invalid or missing JSON Patch document.");

            var updatedContact = await _contactService.PatchContactAsync(id, patchDoc);

            if (updatedContact == null) return NotFound();

            return Ok(updatedContact);
        }


        [HttpPatch("p2/{id}")]
        [Consumes("application/json")] // plain JSON partial update
        public async Task<IActionResult> PatchContactPlain(int id, [FromBody] JObject patch)
        {
            if (patch is null || !patch.HasValues)
                return BadRequest("Provide at least one field to update.");

            var updated = await _contactService.PatchContactPlainAsync(id, patch);
            if (updated is null) return NotFound();

            return Ok(updated);
        }

    }
}
