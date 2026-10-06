using API.Models.DTOs;
using API.Models.Entities;
using AutoMapper;

namespace API.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category
            //CreateMap<Category, CategoryDto>().ReverseMap();
            //CreateMap<CategoryCreateRequest, Category>();
            //CreateMap<CategoryUpdateRequest, Category>();

            // Product
            CreateMap<Product, ProductUpsertDto>().ReverseMap();
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));

            // User
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, UserUpsertDto>().ReverseMap();
        }
    }
}
