using Application.Core.Entities;
using Application.Services.Dtos.Configuration.Employee;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.Services.Configuration.Employees
{
    public interface IEmployeeService : IBaseService<Employee, EmployeeCreationDto, EmployeeUpdateDto, EmployeeRequestModel, EmployeeViewModel>
    {

    }
}
