using AutoMapper;
using ContactApi.DTO;

namespace ContactApi.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // --- Address mappings ---
            CreateMap<Address, AddressDto>().ReverseMap();
            CreateMap<CreateAddressDto, Address>();
            CreateMap<UpdateAddressDto, Address>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            // --- Contact mappings ---
            CreateMap<CreateContactDto, Contact>()
                .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses));

            CreateMap<Contact, ContactDto>()
                .ForMember(dest => dest.Addresses, opt => opt.MapFrom(src => src.Addresses));

            CreateMap<UpdateContactDto, Contact>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<Contact, UpdateContactDto>();
        }
    }
}
