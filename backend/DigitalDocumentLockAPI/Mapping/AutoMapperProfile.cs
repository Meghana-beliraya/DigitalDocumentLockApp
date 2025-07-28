using AutoMapper;
using DigitalDocumentLockCommon.Models;
using DigitalDocumentLockCommom.DTOs;

namespace DigitalDocumentLockAPI.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserSummaryDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TotalDocumentsUploaded, opt => opt.Ignore());
                //.ForMember(dest => dest.LastLogin, opt => opt.Ignore()); 
        }
    }
}
