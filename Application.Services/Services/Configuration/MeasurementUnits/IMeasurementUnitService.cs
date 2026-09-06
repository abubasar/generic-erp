
using Application.Core.Entities;
using Application.Services.Dtos.Configuration.MeasurementUnit;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.MeasurementUnits
{
    public interface IMeasurementUnitService : IBaseService<MeasurementUnit, MeasurementUnitCreationDto, MeasurementUnitUpdateDto, MeasurementUnitRequestModel, MeasurementUnitViewModel>
    {


    }
}
