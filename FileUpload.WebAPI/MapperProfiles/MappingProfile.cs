using AutoMapper;
using FileUpload.Core.Entities;
using FileUpload.WebAPI.Dtos;
using System.Collections.Generic;
using System.Linq;

namespace FileUpload.WebAPI.MapperProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            List<KeyValuePair<string, string>> statusPair = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("Approved", "A"),
                new KeyValuePair<string,string>("Failed", "R"),
                new KeyValuePair<string,string>("Rejected", "R"),
                new KeyValuePair<string,string>("Finished", "D"),
                new KeyValuePair<string,string>("Done", "D")
            };

            CreateMap<InvoiceTrans, InvTransDto>()
                .ForMember(dest => dest.id, act => act.MapFrom(src => src.TransId))
                .ForMember(dest => dest.payment, act => act.MapFrom(src => src.Amount.ToString("F") + ' ' + src.CurrCode))
                .ForMember(dest => dest.Status, act => act.MapFrom(src => statusPair.SingleOrDefault(k => k.Key == src.Status).Value));
        }
    }
}
