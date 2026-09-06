using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.VendorQuotation;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Purchase.VendorQuotations
{
    public class VendorQuotationService : BaseService<VendorQuotation, VendorQuotationCreationDto, VendorQuotationUpdateDto, VendorQuotationRequestModel, VendorQuotationViewModel>, IVendorQuotationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        public VendorQuotationService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }
        public async Task<VendorQuotationViewModel> GetByIdAsync(Guid id)
        {
            var vendorQuotation = await _unitOfWork.Repository<VendorQuotation>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Supplier)
                .Include(x => x.VendorQuotationDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (vendorQuotation == null) throw new NotFoundResultException("Vendor Quotation Not Found With this Id");
            return _mapper.Map<VendorQuotationViewModel>(vendorQuotation);
        }
        public new async Task<Guid> AddAsync(VendorQuotationCreationDto vendorQuotationCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            vendorQuotationCreationDto.DeliveryDate = vendorQuotationCreationDto.DeliveryDate.ToLocal();
            var vendorQuotation = _mapper.Map<VendorQuotation>(vendorQuotationCreationDto);
            var count = _unitOfWork.Repository<VendorQuotation>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            vendorQuotation.Id = Guid.NewGuid();
            vendorQuotation.QuotationNo = "PQ" + financialYear.Code + "-" + count.ToString().PadLeft(6, '0');
            vendorQuotation.FinancialYearId = financialYear.Id;
            vendorQuotation.Status = (int)VendorQuotationStatus.Pending;

            foreach (var item in vendorQuotation.VendorQuotationDetails)
            {
                item.Id = Guid.NewGuid();
                item.VendorQuotationId = vendorQuotation.Id;
                await _unitOfWork.Repository<VendorQuotationDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<VendorQuotation>().AddAsync(vendorQuotation);
            await _unitOfWork.SaveChangesAsync();
            return vendorQuotation.Id;
        }
        public new async Task<Guid> UpdateAsync(VendorQuotationUpdateDto vendorQuotationUpdateDto)
        {
            var dbVendorQuotation = await _unitOfWork.Repository<VendorQuotation>().FindAsync(vendorQuotationUpdateDto.Id);
            //when date updated,convert it to local date
            if (vendorQuotationUpdateDto.DeliveryDate.Equals(dbVendorQuotation.DeliveryDate) == false)
            {
                vendorQuotationUpdateDto.DeliveryDate = vendorQuotationUpdateDto.DeliveryDate.ToLocal();
            }

            var vendorQuotation = _mapper.Map(vendorQuotationUpdateDto, dbVendorQuotation);
            await _unitOfWork.Repository<VendorQuotation>().UpdateAsync(vendorQuotation);
            foreach (var item in vendorQuotation.VendorQuotationDetails)
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                    item.VendorQuotationId = vendorQuotation.Id;
                    await _unitOfWork.Repository<VendorQuotationDetail>().AddAsync(item);
                }
                else await _unitOfWork.Repository<VendorQuotationDetail>().UpdateAsync(item);
            }
            //Delete for VendorQuotationDetails
            if (!string.IsNullOrEmpty(vendorQuotationUpdateDto.DeletedVendorQuotationDetailIds))
            {
                foreach (var id in vendorQuotationUpdateDto.DeletedVendorQuotationDetailIds.Split(',').Where(x => x != ""))
                {
                    var vendorQuotationdetail = await _unitOfWork.Repository<VendorQuotationDetail>().FindAsync(new Guid(id));
                    vendorQuotationdetail.Deleted = true;
                    await _unitOfWork.Repository<VendorQuotationDetail>().UpdateAsync(vendorQuotationdetail);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return vendorQuotation.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var vendorQuotation = await _unitOfWork.Repository<VendorQuotation>().FindAsync(x => x.Id == id, x => x.Include(x => x.VendorQuotationDetails));
            if (vendorQuotation == null) throw new NotFoundResultException("VendorQuotation Not Found With this Id");
            foreach (var item in vendorQuotation.VendorQuotationDetails.ToList())
            {
                item.Deleted = true;
                await _unitOfWork.Repository<VendorQuotationDetail>().UpdateAsync(item);
            }
            vendorQuotation.Deleted = true;
            await _unitOfWork.Repository<VendorQuotation>().UpdateAsync(vendorQuotation);
            await _unitOfWork.SaveChangesAsync();
            return vendorQuotation.Id;
        }
        public virtual async Task<bool> ApproveQuotationAsync(Guid vendorQuotationId, string requisitionNo)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var quotations = _unitOfWork.Repository<VendorQuotation>().TableNoTracking().Where(x => x.RequisitionNo == requisitionNo);
            foreach (var item in quotations)
            {
                if (item.Id == vendorQuotationId)
                {
                    if (financialYear.Id != item.FinancialYearId) throw new BadRequestException("Approval Denied: This Vendor Quotation does not belong to the current financial year.");
                    item.Status = (int)VendorQuotationStatus.Approved;
                    item.ApprovedBy = _workContext.GetUserName();
                    //update purchase Requisition
                    var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().TableNoTracking().SingleOrDefaultAsync(x => x.RequisitionNo == requisitionNo);
                    if (dbPurchaseRequisition is null) throw new NotFoundResultException("Purchase Requisitioin Not Found");
                    dbPurchaseRequisition.RequisitionStatus = (int)RequisitionStatus.Vendor_Selected;
                    await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);
                }
                //others automatically cancelled
                else item.Status = (int)VendorQuotationStatus.Canceled;
                await _unitOfWork.Repository<VendorQuotation>().UpdateAsync(item);
            }

            bool isApproved = await _unitOfWork.SaveChangesAsync();
            return isApproved;

        }

        public async Task<bool> UnpostQuotationAsync(string requisitionNo)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var quotations = _unitOfWork.Repository<VendorQuotation>().TableNoTracking().Where(x => x.RequisitionNo == requisitionNo);
            foreach (var item in quotations)
            {
                if (financialYear.Id != item.FinancialYearId) throw new BadRequestException("Unpost Denied: This Vendor Quotation does not belong to the current financial year.");
                item.Status = (int)VendorQuotationStatus.Pending;
                await _unitOfWork.Repository<VendorQuotation>().UpdateAsync(item);
            }
            //update purchase Requisition
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().TableNoTracking().SingleOrDefaultAsync(x => x.RequisitionNo == requisitionNo);
            if (dbPurchaseRequisition is null) throw new NotFoundResultException("Purchase Requisitioin Not Found");
            if (financialYear.Id != dbPurchaseRequisition.FinancialYearId) throw new BadRequestException("Unpost Denied: This Purchase Requisition does not belong to the current financial year.");
            dbPurchaseRequisition.RequisitionStatus = (int)RequisitionStatus.RFQSent;
            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);

            bool isSuccess = await _unitOfWork.SaveChangesAsync();
            return isSuccess;
        }
    }
}
