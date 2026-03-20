using ContactApi.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json.Linq;

namespace ContactApi.Services
{
    public interface IContactService
    {
        Task<IEnumerable<ContactDto>> GetAllContactsAsync();
        Task<ContactDto?> GetContactByIdAsync(int id);
        Task<ContactDto> CreateContactAsync(CreateContactDto createContactDto);
        Task<ContactDto?> UpdateContactAsync(int id, UpdateContactDto updateContactDto);
        Task<bool> DeleteContactAsync(int id);
        Task<ContactDto?> PatchContactAsync(int id, JsonPatchDocument<Models.Contact> patchDoc);

        // New: plain JSON partial update
        Task<ContactDto?> PatchContactPlainAsync(int id, JObject patch);
    }
}
