using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.MeasurementUnit;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.MeasurementUnits
{
    public class MeasurementUnitService : BaseService<MeasurementUnit, MeasurementUnitCreationDto, MeasurementUnitUpdateDto, MeasurementUnitRequestModel, MeasurementUnitViewModel>, IMeasurementUnitService
    {

        public MeasurementUnitService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }


    }
}
