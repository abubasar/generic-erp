using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase;
using Application.Services.Dtos.Accounts.JournalEntry;
using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using Application.Services.Dtos.Purchase.LcAdjustment;
using Application.Services.Dtos.Purchase.LCCostEntry;
using Application.Services.Dtos.Purchase.PoPriceAdjustmentAfterGrn;
using Application.Services.Dtos.Purchase.PurchaseInvoice;
using Application.Services.Dtos.Purchase.PurchaseOrder;
using Application.Services.Dtos.Purchase.PurchaseRequisition;
using Application.Services.Dtos.Purchase.PurchaseReturn;
using Application.Services.Dtos.Purchase.StockAdjustment;
using Application.Services.Dtos.Purchase.SupplierPayment;
using Application.Services.Dtos.Purchase.VendorQuotation;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.AccountsPayable;
using Application.Services.ViewModels.Purchase;
using AutoMapper;

namespace Application.Services.Services.Purchase
{
    public class PurchaseMappingProfile : Profile
    {
        public PurchaseMappingProfile()
        {

            //Purchase Requisition
            CreateMap<PurchaseRequisitionCreationDto, PurchaseRequisition>();//Add
            CreateMap<PurchaseRequisitionUpdateDto, PurchaseRequisition>();//Update
            CreateMap<PurchaseRequisition, PurchaseRequisitionViewModel>()//Search Response
                .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(s => Enum.GetName(typeof(Priority), s.Priority)))
                .ForMember(dest => dest.RequisitionStatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(RequisitionStatus), s.RequisitionStatus)))
                .ForMember(dest => dest.RequestByName, opt => opt.MapFrom(s => s.RequestBy))
                .ForMember(dest => dest.ExpectedDeliveryDate, opt => opt.MapFrom(s => s.ExpectedDeliveryDate.HasValue ? DateTimeHelper.UtcToLocal(s.ExpectedDeliveryDate.Value) : s.ExpectedDeliveryDate))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Purchase Requisition Detail
            CreateMap<PurchaseRequisitionDetailCreationDto, PurchaseRequisitionDetail>();//Add
            CreateMap<PurchaseRequisitionDetailUpdateDto, PurchaseRequisitionDetail>();//Update
            CreateMap<PurchaseRequisitionDetail, PurchaseRequisitionDetailViewModel>();//Search Response

            //Vendor Quotation
            CreateMap<VendorQuotationCreationDto, VendorQuotation>();//Add
            CreateMap<VendorQuotationUpdateDto, VendorQuotation>();//Update
            CreateMap<VendorQuotation, VendorQuotationViewModel>()
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn))); ;//Search Response
            //Vendor Quotation Detail
            CreateMap<VendorQuotationDetailCreationDto, VendorQuotationDetail>();//Add
            CreateMap<VendorQuotationDetailUpdateDto, VendorQuotationDetail>();//Update
            CreateMap<VendorQuotationDetail, VendorQuotationDetailViewModel>();//Search Response

            //Purchase Order
            CreateMap<PurchaseOrderCreationDto, PurchaseOrder>();//Add
            CreateMap<PurchaseOrderUpdateDto, PurchaseOrder>();//Update
            CreateMap<PurchaseOrder, PurchaseOrderViewModel>()//Search Response
                 .ForMember(dest => dest.TransportName, opt => opt.MapFrom(s => Enum.GetName(typeof(Transport), s.Transport)))
                .ForMember(dest => dest.PaymentModeName, opt => opt.MapFrom(s => Enum.GetName(typeof(Core.Enums.PaymentMode), s.PaymentMode)))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(PurchaseOrderStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Purchase Order Detail
            CreateMap<PurchaseOrderDetailCreationDto, PurchaseOrderDetail>();//Add
            CreateMap<PurchaseOrderDetailUpdateDto, PurchaseOrderDetail>();//Update
            CreateMap<PurchaseOrderDetail, PurchaseOrderDetailViewModel>();//Search Response
            //Goods Receive Note
            CreateMap<GoodsReceiveNoteCreationDto, GoodsReceiveNote>();//Add
            CreateMap<GoodsReceiveNoteUpdateDto, GoodsReceiveNote>();//Update
            CreateMap<GoodsReceiveNote, GoodsReceiveNoteViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(GRNStatus), s.Status)))
                .ForMember(dest => dest.ChallanDate, opt => opt.MapFrom(s => s.ChallanDate.HasValue ? DateTimeHelper.UtcToLocal(s.ChallanDate.Value) : s.ChallanDate))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Goods Receive Note Detail
            CreateMap<GoodsReceiveNoteDetailCreationDto, GoodsReceiveNoteDetail>();//Add
            CreateMap<GoodsReceiveNoteDetailUpdateDto, GoodsReceiveNoteDetail>();//Update
            CreateMap<GoodsReceiveNoteDetail, GoodsReceiveNoteDetailViewModel>();//Search Response
            //Purchase Return
            CreateMap<PurchaseReturnCreationDto, PurchaseReturn>();//Add
            CreateMap<PurchaseReturnUpdateDto, PurchaseReturn>();//Update
            CreateMap<PurchaseReturn, PurchaseReturnViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(PurchaseReturnStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Purchase Return Detail
            CreateMap<PurchaseReturnDetailCreationDto, PurchaseReturnDetail>();//Add
            CreateMap<PurchaseReturnDetailUpdateDto, PurchaseReturnDetail>();//Update
            CreateMap<PurchaseReturnDetail, PurchaseReturnDetailViewModel>();//Search Response

            //Po Price Adjustment After Grn
            CreateMap<PoPriceAdjustmentAfterGrnCreationDto, PoPriceAdjustmentAfterGrn>();//Add
            CreateMap<PoPriceAdjustmentAfterGrnUpdateDto, PoPriceAdjustmentAfterGrn>();//Update
            CreateMap<PoPriceAdjustmentAfterGrn, PoPriceAdjustmentAfterGrnViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(PoPriceAdjustmentAfterGrnStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Po Price Adjustment After Grn Detail
            CreateMap<PoPriceAdjustmentAfterGrnDetailCreationDto, PoPriceAdjustmentAfterGrnDetail>();//Add
            CreateMap<PoPriceAdjustmentAfterGrnDetailUpdateDto, PoPriceAdjustmentAfterGrnDetail>();//Update
            CreateMap<PoPriceAdjustmentAfterGrnDetail, PoPriceAdjustmentAfterGrnDetailViewModel>();//Search Response

            //Purchase Invoice
            CreateMap<PurchaseInvoiceCreationDto, PurchaseInvoice>();//Add
            CreateMap<PurchaseInvoiceUpdateDto, PurchaseInvoice>();//Update
            CreateMap<PurchaseInvoice, PurchaseInvoiceViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(PurchaseInvoiceStatus), s.Status)))
                .ForMember(dest => dest.SupplierInvoiceDate, opt => opt.MapFrom(s => s.SupplierInvoiceDate.HasValue ? DateTimeHelper.UtcToLocal(s.SupplierInvoiceDate.Value) : s.SupplierInvoiceDate))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Purchase Invoice Detail
            CreateMap<PurchaseInvoiceDetailCreationDto, PurchaseInvoiceDetail>();//Add
            CreateMap<PurchaseInvoiceDetailUpdateDto, PurchaseInvoiceDetail>();//Update
            CreateMap<PurchaseInvoiceDetail, PurchaseInvoiceDetailViewModel>();//Search Response

            //Stock Adjustment
            CreateMap<StockAdjustmentCreationDto, StockAdjustment>();//Add
            CreateMap<StockAdjustmentUpdateDto, StockAdjustment>();//Update
            CreateMap<StockAdjustment, StockAdjustmentViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(StockAdjustmentStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Stock Adjustment Detail
            CreateMap<StockAdjustmentDetailCreationDto, StockAdjustmentDetail>();//Add
            CreateMap<StockAdjustmentDetailUpdateDto, StockAdjustmentDetail>();//Update
            CreateMap<StockAdjustmentDetail, StockAdjustmentDetailViewModel>();//Search Response

            //SupplierPaymentAgainstPurchase
            CreateMap<SupplierPaymentAgainstPurchaseCreationDto, SupplierPaymentAgainstPurchase>();//Add
            CreateMap<SupplierPaymentAgainstPurchaseUpdateDto, SupplierPaymentAgainstPurchase>();//Update
            CreateMap<SupplierPaymentAgainstPurchase, SupplierPaymentAgainstPurchaseViewModel>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SupplierPaymentStatus), s.Status)))
                .ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.PaymentDate)));//Search Response
                                                                                                                        //Supplier Payment
            CreateMap<SupplierPaymentCreationDto, SupplierPayment>();//Add
            CreateMap<SupplierPaymentUpdateDto, SupplierPayment>();//Update
            CreateMap<SupplierPayment, SupplierPaymentViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(SupplierPaymentStatus), s.Status)))
                .ForMember(dest => dest.SupplierPaymentTypeName, opt => opt.MapFrom(s => Enum.GetName(typeof(SupplierPaymentType), s.SupplierPaymentType)))
                //.ForMember(dest => dest.PaymentDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.PaymentDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Supplier Payment Detail
            CreateMap<SupplierPaymentDetailCreationDto, SupplierPaymentDetail>();//Add
            CreateMap<SupplierPaymentDetailUpdateDto, SupplierPaymentDetail>();//Update
            CreateMap<SupplierPaymentDetail, SupplierPaymentDetailViewModel>();//Search Response

            //LC Cost Entry
            CreateMap<LCCostEntryCreationDto, LccostEntry>();//Add
            CreateMap<LCCostEntryUpdateDto, LccostEntry>();//Update
            CreateMap<LccostEntry, LCCostEntryViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(LCCostEntryStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //LC Cost Entry Detail
            CreateMap<LCCostEntryDetailCreationDto, LccostEntryDetail>();//Add
            CreateMap<LCCostEntryDetailUpdateDto, LccostEntryDetail>();//Update
            CreateMap<LccostEntryDetail, LCCostEntryDetailViewModel>();//Search Response

            //LC Adjustment
            CreateMap<LcAdjustmentCreationDto, LcAdjustment>();//Add
            CreateMap<LcAdjustmentUpdateDto, LcAdjustment>();//Update
            CreateMap<LcAdjustment, LcAdjustmentViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(LcAdjustmentStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //LC Adjustment Detail
            CreateMap<LcAdjustmentDetailCreationDto, LcAdjustmentDetail>();//Add
            CreateMap<LcAdjustmentDetailUpdateDto, LcAdjustmentDetail>();//Update
            CreateMap<LcAdjustmentDetail, LcAdjustmentDetailViewModel>();//Search Response
        }
    }
}
