using AutoMapper;
using AutoMapper.QueryableExtensions;
using ContactApi.Data;
using ContactApi.DTO;
using ContactApi.Models;
using ContactApi.Utilities;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ContactApi.Services
{
    public class ContactService : IContactService
    {
        private readonly ContactDbContext _context;
        private readonly IMapper _mapper;

        public ContactService(ContactDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ContactDto>> GetAllContactsAsync()
        {
            return await _context.Contacts
                .Include(c => c.Addresses) // ✅ load addresses
                .ProjectTo<ContactDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<ContactDto?> GetContactByIdAsync(int id)
        {
            var contact = await _context.Contacts
                .Include(c => c.Addresses) // ✅ load addresses
                .FirstOrDefaultAsync(c => c.Id == id);

            return contact == null ? null : _mapper.Map<ContactDto>(contact);
        }

        public async Task<ContactDto> CreateContactAsync(CreateContactDto createContactDto)
        {
            var contact = _mapper.Map<Contact>(createContactDto);
            contact.CreatedAt = DateTime.UtcNow;

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            return _mapper.Map<ContactDto>(contact);
        }

        public async Task<ContactDto?> UpdateContactAsync(int id, UpdateContactDto updateContactDto)
        {
            var contact = await _context.Contacts
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null) return null;

            // Update simple properties
            _mapper.Map(updateContactDto, contact);

            if (updateContactDto.Addresses != null)
            {
                // Track incoming address IDs
                var incomingIds = updateContactDto.Addresses
                    .Where(a => a.Id.HasValue && a.Id.Value > 0)
                    .Select(a => a.Id!.Value)
                    .ToList();

                // 1. Remove addresses not in payload
                var toRemove = contact.Addresses
                    .Where(a => !incomingIds.Contains(a.Id))
                    .ToList();
                _context.Addresses.RemoveRange(toRemove);

                // 2. Update existing addresses
                foreach (var updateDto in updateContactDto.Addresses.Where(a => a.Id.HasValue && a.Id.Value > 0))
                {
                    var existing = contact.Addresses.FirstOrDefault(a => a.Id == updateDto.Id);
                    if (existing != null)
                    {
                        // Map non-null fields
                        if (!string.IsNullOrWhiteSpace(updateDto.Street))
                            existing.Street = updateDto.Street;

                        if (!string.IsNullOrWhiteSpace(updateDto.City))
                            existing.City = updateDto.City;

                        if (updateDto.Country.HasValue)
                            existing.Country = updateDto.Country.Value;
                    }
                }

                // 3. Add new addresses
                var newAddresses = updateContactDto.Addresses
                    .Where(a => !a.Id.HasValue || a.Id == 0)
                    .Select(a => new Address
                    {
                        Street = a.Street ?? string.Empty,
                        City = a.City ?? string.Empty,
                        Country = a.Country ?? Country.NoCountry,
                        ContactId = contact.Id
                    })
                    .ToList();

                if (newAddresses.Any())
                {
                    foreach (var newAddress in newAddresses)
                    {
                        contact.Addresses.Add(newAddress);
                    }


                }
            }

            contact.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return _mapper.Map<ContactDto>(contact);
        }


        public async Task<bool> DeleteContactAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return false;

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ContactDto?> PatchContactAsync(int id, JsonPatchDocument<Contact> patchDoc)
        {
            var contact = await _context.Contacts
                .Include(c => c.Addresses) // ✅ patch might target nested props
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null) return null;

            patchDoc.ApplyTo(contact);
            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<ContactDto>(contact);
        }

        public async Task<ContactDto?> PatchContactPlainAsync(int id, JObject patch)
        {
            var contact = await _context.Contacts
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null) return null;

            DynamicPatch.Apply(
                contact,
                patch,
                new PatchOptions
                {
                    Blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                        { "Id", "CreatedAt", "UpdatedAt" },
                    RejectUnknown = false,
                    RespectJsonPropertyAttribute = true,
                    CaseInsensitive = true
                }
            );

            contact.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return _mapper.Map<ContactDto>(contact);
        }
    }
}
