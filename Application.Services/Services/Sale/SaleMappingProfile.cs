using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.Dtos.Inventory.StockTransfer;
using Application.Services.Dtos.Sale.DeliveryNote;
using Application.Services.Dtos.Sale.SaleOrder;
using Application.Services.Dtos.Sale.SaleQuotation;
using Application.Services.Dtos.Sale.SaleReturn;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Inventory;
using Application.Services.ViewModels.Sale;
using AutoMapper;

namespace Application.Services.Services.Sale
{
    public class SaleMappingProfile : Profile
    {
        public SaleMappingProfile()
        {
            //Sale Quotation
            CreateMap<SaleQuotationCreationDto, SaleQuotation>();//Add
            CreateMap<SaleQuotationUpdateDto, SaleQuotation>();//Update
            CreateMap<SaleQuotation, SaleQuotationViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SaleQuotationStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Sale Quotation Detail
            CreateMap<SaleQuotationDetailCreationDto, SaleQuotationDetail>();//Add
            CreateMap<SaleQuotationDetailUpdateDto, SaleQuotationDetail>();//Update
            CreateMap<SaleQuotationDetail, SaleQuotationDetailViewModel>();//Search Response
            //Sale Order
            CreateMap<SaleOrderCreationDto, SaleOrder>();//Add
            CreateMap<SaleOrderUpdateDto, SaleOrder>();//Update
            CreateMap<SaleOrder, SaleOrderViewModel>()//Search Response
                .ForMember(dest => dest.TransportName, opt => opt.MapFrom(s => Enum.GetName(typeof(Transport), s.Transport)))
                .ForMember(dest => dest.PaymentTermName, opt => opt.MapFrom(s => Enum.GetName(typeof(PaymentTerm), s.PaymentTerm)))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SaleOrderStatus), s.Status)))
                //.ForMember(dest => dest.OrderDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.OrderDate)))
                // .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.DeliveryDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Sale Order Detail
            CreateMap<SaleOrderDetailCreationDto, SaleOrderDetail>();//Add
            CreateMap<SaleOrderDetailUpdateDto, SaleOrderDetail>();//Update
            CreateMap<SaleOrderDetail, SaleOrderDetailViewModel>();//Search Response
            //Delivery Note
            CreateMap<DeliveryNoteCreationDto, DeliveryNote>();//Add
            CreateMap<DeliveryNoteUpdateDto, DeliveryNote>();//Update
            CreateMap<DeliveryNote, DeliveryNoteViewModel>()//Search Response
                .ForMember(dest => dest.TransportName, opt => opt.MapFrom(s => Enum.GetName(typeof(Transport), s.Transport)))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SaleOrderStatus), s.Status)))
                // .ForMember(dest => dest.DeliveryDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.DeliveryDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Delivery Note Detail
            CreateMap<DeliveryNoteDetailCreationDto, DeliveryNoteDetail>();//Add
            CreateMap<DeliveryNoteDetailUpdateDto, DeliveryNoteDetail>();//Update
            CreateMap<DeliveryNoteDetail, DeliveryNoteDetailViewModel>();//Search Response
            //ReceivePaymentAgainstSale
            CreateMap<ReceivePaymentAgainstSaleCreationDto, ReceivePaymentAgainstSale>();//Add
            CreateMap<ReceivePaymentAgainstSaleUpdateDto, ReceivePaymentAgainstSale>();//Update
            CreateMap<ReceivePaymentAgainstSale, ReceivePaymentAgainstSaleViewModel>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(ReceivePaymentStatus), s.Status)))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.PaymentDate)));//Search Response
            //Sale Invoice
            CreateMap<SaleInvoiceCreationDto, SaleInvoice>();//Add
            CreateMap<SaleInvoiceUpdateDto, SaleInvoice>();//Update
            CreateMap<SaleInvoice, SaleInvoiceViewModel>()//Search Response
                 .ForMember(dest => dest.TransportName, opt => opt.MapFrom(s => Enum.GetName(typeof(Transport), s.Transport)))
                 .ForMember(dest => dest.PaymentTermName, opt => opt.MapFrom(s => Enum.GetName(typeof(PaymentTerm), s.PaymentTerm)))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SaleInvoiceStatus), s.Status)))
                // .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.InvoiceDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Sale Invoice Detail
            CreateMap<SaleInvoiceDetailCreationDto, SaleInvoiceDetail>();//Add
            CreateMap<SaleInvoiceDetailUpdateDto, SaleInvoiceDetail>();//Update
            CreateMap<SaleInvoiceDetail, SaleInvoiceDetailViewModel>();//Search Response
            //Sale Return
            CreateMap<SaleReturnCreationDto, SaleReturn>();//Add
            CreateMap<SaleReturnUpdateDto, SaleReturn>();//Update
            CreateMap<SaleReturn, SaleReturnViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SaleReturnStatus), s.Status)))
                // .ForMember(dest => dest.SaleReturnDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.SaleReturnDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Sale Return Detail
            CreateMap<SaleReturnDetailCreationDto, SaleReturnDetail>();//Add
            CreateMap<SaleReturnDetailUpdateDto, SaleReturnDetail>();//Update
            CreateMap<SaleReturnDetail, SaleReturnDetailViewModel>();//Search Response
            //Receive Payment
            CreateMap<ReceivePaymentCreationDto, ReceivePayment>();//Add
            CreateMap<ReceivePaymentUpdateDto, ReceivePayment>();//Update
            CreateMap<ReceivePayment, ReceivePaymentViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(ReceivePaymentStatus), s.Status)))
                //.ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.PaymentDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Receive Payment Detail
            CreateMap<ReceivePaymentDetailCreationDto, ReceivePaymentDetail>();//Add
            CreateMap<ReceivePaymentDetailUpdateDto, ReceivePaymentDetail>();//Update
            CreateMap<ReceivePaymentDetail, ReceivePaymentDetailViewModel>();//Search Response
            //Stock Transfer
            CreateMap<StockTransferCreationDto, StockTransfer>();//Add
            CreateMap<StockTransferUpdateDto, StockTransfer>();//Update
            CreateMap<StockTransfer, StockTransferViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(StockTransferStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Stock Transfer Detail
            CreateMap<StockTransferDetailCreationDto, StockTransferDetail>();//Add
            CreateMap<StockTransferDetailUpdateDto, StockTransferDetail>();//Update
            CreateMap<StockTransferDetail, StockTransferDetailViewModel>();//Search Response

        }
    }
}
