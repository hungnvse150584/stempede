using AutoMapper;
using DataAccess.Entities;
using BusinessLogic.DTOs.Order;

namespace BusinessLogic.Configurations.MappingProfiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.CustomerUsername, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.Deliveries, opt => opt.MapFrom(src => src.Deliveries))
                .ForMember(dest => dest.OrderDetails, opt => opt.MapFrom(src => src.OrderDetails));

            CreateMap<Delivery, DeliveryDto>();
            CreateMap<OrderDetail, OrderDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductDescription, opt => opt.MapFrom(src => src.Product.Description));
        }
    }
}
