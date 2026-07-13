using AutoMapper;
using RealEstateApp.Application.DTOs.Improvements;
using RealEstateApp.Application.DTOs.PropertyTypes;
using RealEstateApp.Application.DTOs.SaleTypes;
using RealEstateApp.Application.ViewModels.Improvements;
using RealEstateApp.Application.ViewModels.PropertyTypes;
using RealEstateApp.Application.ViewModels.SaleTypes;
using RealEstateApp.Domain.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RealEstateApp.Application.Mappings
{
    public class GeneralProfile : Profile
    {
        public GeneralProfile()
        {
            CreateMap<PropertyType, PropertyTypeViewModel>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0))
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<PropertyType, SavePropertyTypeViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<PropertyType, PropertyTypeDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<PropertyType, SavePropertyTypeDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<SaleType, SaleTypeViewModel>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0))
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<SaleType, SaveSaleTypeViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<SaleType, SaleTypeDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<SaleType, SaveSaleTypeDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<Improvement, ImprovementViewModel>()
                .ForMember(dest => dest.PropertiesCount, opt => opt.MapFrom(src => src.Properties != null ? src.Properties.Count : 0))
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<Improvement, SaveImprovementViewModel>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<Improvement, ImprovementDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());

            CreateMap<Improvement, SaveImprovementDto>()
                .ReverseMap()
                .ForMember(dest => dest.Properties, opt => opt.Ignore());
        }
    }
}