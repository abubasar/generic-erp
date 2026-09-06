using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Department;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Departments
{
    public class DepartmentService : BaseService<Department, DepartmentCreationDto, DepartmentUpdateDto, DepartmentRequestModel, DepartmentViewModel>, IDepartmentService
    {

        public DepartmentService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }



    }
}
