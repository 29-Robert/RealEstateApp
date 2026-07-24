using AutoMapper;
using RealEstateApp.Application.DTOs.Improvements;
using RealEstateApp.Application.DTOs.Property;
using RealEstateApp.Application.DTOs.PropertyTypes;
using RealEstateApp.Application.DTOs.SaleTypes;
using RealEstateApp.Application.DTOs.Message;
using RealEstateApp.Application.DTOs.Offer;
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

            
            // Mapeos de Property
            

            CreateMap<Property, PropertyDto>()
          .ForMember(dest => dest.PropertyTypeName,
              opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : string.Empty))
          .ForMember(dest => dest.SaleTypeName,
              opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : string.Empty))
          .ForMember(dest => dest.Size,
              opt => opt.MapFrom(src => (double)src.LandSize))
          .ForMember(dest => dest.MainImagePath,
              opt => opt.MapFrom(src =>
                  src.PropertyImages != null && src.PropertyImages.Any(pi => pi.IsMain)
                      ? src.PropertyImages.First(pi => pi.IsMain).ImagePath
                      : (src.PropertyImages != null && src.PropertyImages.Any()
                          ? src.PropertyImages.First().ImagePath
                          : string.Empty)))
          .ForMember(dest => dest.Status,
              opt => opt.MapFrom(src => src.PropertyStatus.ToString()));

            CreateMap<Property, PropertyDetailDto>()
    .ForMember(dest => dest.PropertyTypeName,
        opt => opt.MapFrom(src => src.PropertyType != null ? src.PropertyType.Name : string.Empty))
    .ForMember(dest => dest.SaleTypeName,
        opt => opt.MapFrom(src => src.SaleType != null ? src.SaleType.Name : string.Empty))
    .ForMember(dest => dest.Size,
        opt => opt.MapFrom(src => (double)src.LandSize))
    .ForMember(dest => dest.MainImagePath,
        opt => opt.MapFrom(src =>
            src.PropertyImages != null && src.PropertyImages.Any(pi => pi.IsMain)
                ? src.PropertyImages.First(pi => pi.IsMain).ImagePath
                : (src.PropertyImages != null && src.PropertyImages.Any()
                    ? src.PropertyImages.First().ImagePath
                    : string.Empty)))
    .ForMember(dest => dest.Status,
        opt => opt.MapFrom(src => src.PropertyStatus.ToString()))
    .ForMember(dest => dest.ImagePaths,
        opt => opt.MapFrom(src =>
            src.PropertyImages != null
                ?
            src.PropertyImages.Select(pi => pi.ImagePath).ToList()
                            : new List<string>()))
                .ForMember(dest => dest.Improvements,
                    opt => opt.MapFrom(src =>
                        src.Improvements != null
                            ? src.Improvements.Select(i => i.Name).ToList()
                            : new List<string>()))
                .ForMember(dest => dest.AgentFullName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentPhone, opt => opt.Ignore())
                .ForMember(dest => dest.AgentPhotoPath, opt => opt.Ignore());

            CreateMap<SavePropertyDto, Property>()
                .ForMember(dest => dest.LandSize, opt => opt.MapFrom(src => (int)src.Size))
                .ForMember(dest => dest.Code, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyImages, opt => opt.Ignore())
                .ForMember(dest => dest.Improvements, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyType, opt => opt.Ignore())
                .ForMember(dest => dest.SaleType, opt => opt.Ignore())
                .ForMember(dest => dest.PropertyStatus, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<Message, MessageDto>()
                .ForMember(dest => dest.ClientName, opt => opt.Ignore())
                .ForMember(dest => dest.AgentName, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Offer, OfferDto>()
                .ForMember(dest => dest.ClientFullName, opt => opt.Ignore())
                .ReverseMap();
        }
    }

}
    

