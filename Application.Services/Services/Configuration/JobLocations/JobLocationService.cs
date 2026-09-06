using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.JobLocation;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.JobLocations
{
    public class JobLocationService : BaseService<JobLocation, JobLocationCreationDto, JobLocationUpdateDto, JobLocationRequestModel, JobLocationViewModel>, IJobLocationService
    {
        public JobLocationService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext) { }
    }
}
