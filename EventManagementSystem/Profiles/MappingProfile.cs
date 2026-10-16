using AutoMapper;
using EventManagementSystem.DTOs;
using EventManagementSystem.Models;

namespace EventManagementSystem.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Event Mappings
            CreateMap<Event, EventResponseDto>();
            CreateMap<CreateEventDto, Event>()
                .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Approved"))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Registration Mappings
            CreateMap<Registration, RegistrationResponseDto>();
            CreateMap<CreateRegistrationDto, Registration>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Confirmed"))
                .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => DateTime.UtcNow));

            // Ticket Mappings
            CreateMap<Ticket, TicketResponseDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Registration != null ? src.Registration.UserId : 0));
            // User Mappings
            CreateMap<UserRegisterDto, User>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            CreateMap<User, UserResponseDto>();
        }
    }
}