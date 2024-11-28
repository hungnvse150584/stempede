using BusinessLogic.DTOs.Product;
using AutoMapper;
using BusinessLogic.DTOs.Auth;
using BusinessLogic.DTOs.Lab;
using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs.Subcategory;
using BusinessLogic.DTOs.Cart;
using BusinessLogic.DTOs.Order;
using DataAccess.Entities;

namespace BusinessLogic.Configurations.MappingProfiles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, ReadUserDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status ? "Active" : "Banned"))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role.RoleName).ToList()));

            CreateMap<UserPermission, UserPermissionDto>()
            .ForMember(dest => dest.AssignedBy, opt => opt.MapFrom(src => src.AssignedByNavigation.FullName));

            //Product mappings
            CreateMap<Product, ReadProductDto>()
                .ForMember(dest => dest.LabName, opt => opt.MapFrom(src => src.Lab.LabName))
                .ForMember(dest => dest.SubcategoryName, opt => opt.MapFrom(src => src.Subcategory.SubcategoryName));

            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            // MappingsAuthentication
            CreateMap<UserRegistrationDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => true)) // Default status
                .ForMember(dest => dest.IsExternal, opt => opt.MapFrom(src => src.IsExternal))
                .ForMember(dest => dest.ExternalProvider, opt => opt.MapFrom(src => src.ExternalProvider ?? null));

            // Lab Mappings
            CreateMap<Lab, ReadLabDto>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products));

            CreateMap<CreateLabDto, Lab>();
            CreateMap<UpdateLabDto, Lab>();
            CreateMap<Lab, ReadLabSimpleDto>();

            // Subcategory mappings
            CreateMap<Subcategory, ReadSubcategoryDto>();

            // Cart mappings
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.Price * src.Quantity));

            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.CartItems.Sum(ci => ci.Price * ci.Quantity)));
        }
    }
}
