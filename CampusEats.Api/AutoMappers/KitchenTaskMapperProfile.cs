using AutoMapper;
using CampusEats.Api.Domain.Entities;
using CampusEats.Api.Features.KitchenTask.DTOs;

namespace CampusEats.Api.AutoMappers;

public class KitchenTaskMapperProfile : Profile
{
    public KitchenTaskMapperProfile()
    {
        CreateMap<KitchenTask, KitchenTaskResponse>()
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.AssignedStaff, 
                opt => opt.MapFrom(src => src.Staff == null ? null : 
                    new SimpleStaffResponse(src.Staff.Id, src.Staff.FullName))) 
            .ForMember(dest => dest.OrderItems, 
                opt => opt.MapFrom(src => src.Order.OrderItems.Select(oi => 
                    new SimpleOrderItemResponse(oi.MenuItem.Name, oi.Quantity)))); 
    }
}