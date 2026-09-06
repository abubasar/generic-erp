using Application.Core.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnumValueController : ControllerBase
    {
        [HttpGet]
        [Route("Priorities")]
        public IActionResult GetPriorities()
        {
            return Ok(EnumExtensions.GetValues<Priority>());
        }

        [HttpGet]
        [Route("RequisitionStatuses")]
        public IActionResult GetRequisitionStatuses()
        {
            return Ok(EnumExtensions.GetValues<RequisitionStatus>());
        }
        [HttpGet]
        [Route("PurchaseOrderStatuses")]
        public IActionResult GetPurchaseOrderStatuses()
        {
            return Ok(EnumExtensions.GetValues<PurchaseOrderStatus>());
        }
        [HttpGet]
        [Route("GRNStatuses")]
        public IActionResult GetGRNStatuses()
        {
            return Ok(EnumExtensions.GetValues<GRNStatus>());
        }
        [HttpGet]
        [Route("VendorQuotationStatuses")]
        public IActionResult GetVendorQuotationStatuses()
        {
            return Ok(EnumExtensions.GetValues<VendorQuotationStatus>());
        }
        [HttpGet]
        [Route("PurchaseInvoiceStatuses")]
        public IActionResult GetPurchaseInvoiceStatuses()
        {
            return Ok(EnumExtensions.GetValues<PurchaseInvoiceStatus>());
        }
        [HttpGet]
        [Route("MaritalStatues")]
        public IActionResult GetMaritalStatues()
        {
            return Ok(EnumExtensions.GetValues<MaritalStatus>());
        }
        [HttpGet]
        [Route("Genders")]
        public IActionResult GetGenders()
        {
            return Ok(EnumExtensions.GetValues<Gender>());
        }
        [HttpGet]
        [Route("BloodGroups")]
        public IActionResult GetBloodGroups()
        {
            return Ok(EnumExtensions.GetValues<BloodGroup>());
        }
        [HttpGet]
        [Route("Transports")]
        public IActionResult GetTransports()
        {
            return Ok(EnumExtensions.GetValues<Transport>());
        }
        [HttpGet]
        [Route("PaymentModes")]
        public IActionResult GetPaymentModes()
        {
            return Ok(EnumExtensions.GetValues<Core.Enums.PaymentMode>());
        }
        [HttpGet]
        [Route("ImportPurchaseIncoTerms")]
        public IActionResult GetImportPurchaseIncoTerms()
        {
            return Ok(EnumExtensions.GetValues<ImportPurchaseIncoTerm>());
        }
        [HttpGet]
        [Route("ImportPurchasePaymentTerms")]
        public IActionResult GetImportPurchasePaymentTerms()
        {
            return Ok(EnumExtensions.GetValues<ImportPurchasePaymentTerm>());
        }
        [HttpGet]
        [Route("BOMStatuses")]
        public IActionResult GetBOMStatuses()
        {
            return Ok(EnumExtensions.GetValues<BOMStatus>());
        }
        [HttpGet]
        [Route("ManufacturingOrderStatuses")]
        public IActionResult GetManufacturingOrderStatuses()
        {
            return Ok(EnumExtensions.GetValues<ManufacturingOrderStatus>());
        }
        [HttpGet]
        [Route("ProductionStatuses")]
        public IActionResult GetProductionStatuses()
        {
            return Ok(EnumExtensions.GetValues<ProductionStatus>());
        }
        [HttpGet]
        [Route("SaleQuotationStatuses")]
        public IActionResult GetSaleQuotationStatuses()
        {
            return Ok(EnumExtensions.GetValues<SaleQuotationStatus>());
        }
        [HttpGet]
        [Route("SaleOrderStatuses")]
        public IActionResult GetSaleOrderStatuses()
        {
            return Ok(EnumExtensions.GetValues<SaleOrderStatus>());
        }
        [HttpGet]
        [Route("SaleReturnStatuses")]
        public IActionResult GetSaleReturnStatuses()
        {
            return Ok(EnumExtensions.GetValues<SaleReturnStatus>());
        }
        [HttpGet]
        [Route("VoucherEntryStatuses")]
        public IActionResult GetVoucherEntryStatuses()
        {
            return Ok(EnumExtensions.GetValues<VoucherEntryStatus>());
        }
        [HttpGet]
        [Route("VoucherTypes")]
        public IActionResult GetVoucherTypes()
        {
            return Ok(EnumExtensions.GetValues<VoucherType>());
        }
        [HttpGet]
        [Route("JournalEntryStatuses")]
        public IActionResult GetJournalEntryStatuses()
        {
            return Ok(EnumExtensions.GetValues<JournalEntryStatus>());
        }
        [HttpGet]
        [Route("FundTransferStatuses")]
        public IActionResult GetFundTransferStatuses()
        {
            return Ok(EnumExtensions.GetValues<FundTransferStatus>());
        }
        [HttpGet]
        [Route("PostTypes")]
        public IActionResult GetPostTypes()
        {
            return Ok(EnumExtensions.GetValues<PostType>());
        }
        [HttpGet]
        [Route("DeliveryNoteStatuses")]
        public IActionResult GetDeliveryNoteStatuses()
        {
            return Ok(EnumExtensions.GetValues<DeliveryNoteStatus>());
        }
        [HttpGet]
        [Route("SaleInvoiceStatuses")]
        public IActionResult GetSaleInvoiceStatuses()
        {
            return Ok(EnumExtensions.GetValues<SaleInvoiceStatus>());
        }
        [HttpGet]
        [Route("EmployeeTypes")]
        public IActionResult GetEmployeeTypes()
        {
            return Ok(EnumExtensions.GetValues<EmployeeType>());
        }
        [HttpGet]
        [Route("ReceivePaymentStatuses")]
        public IActionResult GetReceivePaymentStatuses()
        {
            return Ok(EnumExtensions.GetValues<ReceivePaymentStatus>());
        }
        [HttpGet]
        [Route("StockTransferStatuses")]
        public IActionResult GetStockTransferStatuses()
        {
            return Ok(EnumExtensions.GetValues<StockTransferStatus>());
        }
        [HttpGet]
        [Route("PurchaseReturnStatuses")]
        public IActionResult GetPurchaseReturnStatuses()
        {
            return Ok(EnumExtensions.GetValues<PurchaseReturnStatus>());
        }
        [HttpGet]
        [Route("PoPriceAdjustmentAfterGrnStatuses")]
        public IActionResult GetPoPriceAdjustmentAfterGrnStatuses()
        {
            return Ok(EnumExtensions.GetValues<PoPriceAdjustmentAfterGrnStatus>());
        }
        [HttpGet]
        [Route("SupplierPaymentStatuses")]
        public IActionResult GetSupplierPaymentStatuses()
        {
            return Ok(EnumExtensions.GetValues<SupplierPaymentStatus>());
        }
        [HttpGet]
        [Route("StockAdjustmentStatuses")]
        public IActionResult GetStockAdjustmentStatuses()
        {
            return Ok(EnumExtensions.GetValues<StockAdjustmentStatus>());
        }

        [HttpGet]
        [Route("PaymentVoucherStatuses")]
        public IActionResult GetPaymentVoucherStatuses()
        {
            return Ok(EnumExtensions.GetValues<PaymentVoucherStatus>());
        }

        [HttpGet]
        [Route("ReceiveVoucherStatuses")]
        public IActionResult GetReceiveVoucherStatuses()
        {
            return Ok(EnumExtensions.GetValues<ReceiveVoucherStatus>());
        }
        [HttpGet]
        [Route("SupplierPaymentTypes")]
        public IActionResult GetSupplierPaymentTypes()
        {
            return Ok(EnumExtensions.GetValues<SupplierPaymentType>());
        }

        [HttpGet]
        [Route("LCCostEntryStatuses")]
        public IActionResult GetLCCostEntryStatuses()
        {
            return Ok(EnumExtensions.GetValues<LCCostEntryStatus>());
        }

        [HttpGet]
        [Route("LcAdjustmentStatuses")]
        public IActionResult GetLcAdjustmentStatuses()
        {
            return Ok(EnumExtensions.GetValues<LcAdjustmentStatus>());
        }
        [HttpGet]
        [Route("CustomerWiseProductDiscountStatuses")]
        public IActionResult GetCustomerWiseProductDiscountStatuses()
        {
            return Ok(EnumExtensions.GetValues<CustomerWiseProductDiscountStatus>());
        }

        [HttpGet]
        [Route("PaymentTerms")]
        public IActionResult GetPaymentTerms()
        {
            return Ok(EnumExtensions.GetValues<PaymentTerm>());
        }
    }
}
