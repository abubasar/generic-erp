using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Services.Dtos.Production.BillOfMaterial;
using Application.Services.Dtos.Production.ManufacturingOrder;
using Application.Services.Dtos.Production.Production;
using Application.Services.ViewModels.Production;
using AutoMapper;

namespace Application.Services.Services.Productions
{
    public class ProductionMappingProfile : Profile
    {
        public ProductionMappingProfile()
        {
            //Bill of Material
            CreateMap<BillOfMaterialCreationDto, BillOfMaterial>();//Add
            CreateMap<BillOfMaterialUpdateDto, BillOfMaterial>();//Update
            CreateMap<BillOfMaterial, BillOfMaterialViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(BOMStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Bill of Material Detail
            CreateMap<BillOfMaterialDetailCreationDto, BillOfMaterialDetail>();//Add
            CreateMap<BillOfMaterialDetailUpdateDto, BillOfMaterialDetail>();//Update
            CreateMap<BillOfMaterialDetail, BillOfMaterialDetailViewModel>();//Search Response
            //Manufacturing Order
            CreateMap<ManufacturingOrderCreationDto, ManufacturingOrder>();//Add
            CreateMap<ManufacturingOrderUpdateDto, ManufacturingOrder>();//Update
            CreateMap<ManufacturingOrder, ManufacturingOrderViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(ManufacturingOrderStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Manufacturing Order Detail
            CreateMap<ManufacturingOrderDetailCreationDto, ManufacturingOrderDetail>();//Add
            CreateMap<ManufacturingOrderDetailUpdateDto, ManufacturingOrderDetail>();//Update
            CreateMap<ManufacturingOrderDetail, ManufacturingOrderDetailViewModel>();//Search Response
            
            //Production
            CreateMap<ProductionCreationDto, Production>();//Add
            CreateMap<ProductionUpdateDto, Production>();//Update
            CreateMap<Production, ProductionViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(ProductionStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Production Detail
            CreateMap<ProductionDetailCreationDto, ProductionDetail>();//Add
            CreateMap<ProductionDetailUpdateDto, ProductionDetail>();//Update
            CreateMap<ProductionDetail, ProductionDetailViewModel>();//Search Response
        }
    }
}
