using Application.Core.Entities;
using Application.Services.Dtos.Configuration.JobLocation;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.JobLocations
{
    public interface IJobLocationService : IBaseService<JobLocation, JobLocationCreationDto, JobLocationUpdateDto, JobLocationRequestModel, JobLocationViewModel>
    {
    }
}
