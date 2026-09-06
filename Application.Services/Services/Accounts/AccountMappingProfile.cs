using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Services.Dtos.Accounts.FundTransfer;
using Application.Services.Dtos.Accounts.JournalEntry;
using Application.Services.Dtos.Accounts.PaymentVoucher;
using Application.Services.Dtos.Accounts.ReceiveVoucher;
using Application.Services.Dtos.Accounts.VoucherEntry;
using Application.Services.ViewModels.Accounts;
using AutoMapper;

namespace Application.Services.Services.Accounts
{
    public class AccountMappingProfile : Profile
    {
        public AccountMappingProfile()
        {
            //Voucher Entry
            CreateMap<VoucherEntryCreationDto, VoucherEntry>();//Add
            CreateMap<VoucherEntryUpdateDto, VoucherEntry>();//Update
            CreateMap<VoucherEntry, VoucherEntryViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(VoucherEntryStatus), s.Status)))
                // .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.VoucherDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Voucher Entry Detail
            CreateMap<VoucherEntryDetailCreationDto, VoucherEntryDetail>();//Add
            CreateMap<VoucherEntryDetailUpdateDto, VoucherEntryDetail>();//Update
            CreateMap<VoucherEntryDetail, VoucherEntryDetailViewModel>();//Search Response

            //Journal Entry
            CreateMap<JournalEntryCreationDto, JournalEntry>();//Add
            CreateMap<JournalEntryUpdateDto, JournalEntry>();//Update
            CreateMap<JournalEntry, JournalEntryViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(JournalEntryStatus), s.Status)))
                //.ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.VoucherDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Journal Entry Detail
            CreateMap<JournalEntryDetailCreationDto, JournalEntryDetail>();//Add
            CreateMap<JournalEntryDetailUpdateDto, JournalEntryDetail>();//Update
            CreateMap<JournalEntryDetail, JournalEntryDetailViewModel>();//Search Response

            //Fund Transfer
            CreateMap<FundTransferCreationDto, FundTransfer>();//Add
            CreateMap<FundTransferUpdateDto, FundTransfer>();//Update
            CreateMap<FundTransfer, FundTransferViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(FundTransferStatus), s.Status)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));

            //Payment Voucher
            CreateMap<PaymentVoucherCreationDto, PaymentVoucher>();//Add
            CreateMap<PaymentVoucherUpdateDto, PaymentVoucher>();//Update
            CreateMap<PaymentVoucher, PaymentVoucherViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(PaymentVoucherStatus), s.Status)))
                // .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.VoucherDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Payment Voucher Detail
            CreateMap<PaymentVoucherDetailCreationDto, PaymentVoucherDetail>();//Add
            CreateMap<PaymentVoucherDetailUpdateDto, PaymentVoucherDetail>();//Update
            CreateMap<PaymentVoucherDetail, PaymentVoucherDetailViewModel>();//Search Response

            //Receive Voucher
            CreateMap<ReceiveVoucherCreationDto, ReceiveVoucher>();//Add
            CreateMap<ReceiveVoucherUpdateDto, ReceiveVoucher>();//Update
            CreateMap<ReceiveVoucher, ReceiveVoucherViewModel>()//Search Response
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(ReceiveVoucherStatus), s.Status)))
                // .ForMember(dest => dest.VoucherDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.VoucherDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Receive Voucher Detail
            CreateMap<ReceiveVoucherDetailCreationDto, ReceiveVoucherDetail>();//Add
            CreateMap<ReceiveVoucherDetailUpdateDto, ReceiveVoucherDetail>();//Update
            CreateMap<ReceiveVoucherDetail, ReceiveVoucherDetailViewModel>();//Search Response
        }
    }
}
