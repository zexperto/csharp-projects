using ContactApi.Models;

namespace ContactApi.DTO
{
    // --- Address DTOs ---
    public class AddressDto
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public Country Country { get; set; }
    }

    public class CreateAddressDto
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public Country Country { get; set; }
    }

    public class UpdateAddressDto
    {
        public int? Id { get; set; } // ✅ allows update vs new
        public string? Street { get; set; }
        public string? City { get; set; }
        public Country? Country { get; set; }
    }


    // --- Contact DTOs ---
    public class ContactDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string Email { get; set; } = string.Empty;

        // ✅ Include addresses when returning a contact
        public List<AddressDto> Addresses { get; set; } = new();
    }

    public class CreateContactDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Telephone { get; set; } = string.Empty;
        public Gender Gender { get; set; }
        public string Email { get; set; } = string.Empty;

        // ✅ Optionally create with addresses
        public List<CreateAddressDto> Addresses { get; set; } = new();
    }

    public class UpdateContactDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Telephone { get; set; }
        public Gender Gender { get; set; }
        public string? Email { get; set; }

        // ✅ Allow updating addresses too
        public List<UpdateAddressDto>? Addresses { get; set; }
    }
}
