using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Employee;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration.Employees
{
    public class EmployeeService : BaseService<Employee, EmployeeCreationDto, EmployeeUpdateDto, EmployeeRequestModel, EmployeeViewModel>, IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        public EmployeeService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
        }

        public override async Task<Guid> AddAsync(EmployeeCreationDto employeeCreationDto)
        {
            var model = _mapper.Map<Employee>(employeeCreationDto);
            model.Id = Guid.NewGuid();
            model.FullName = employeeCreationDto.FirstName + " " + employeeCreationDto.LastName;
            await _unitOfWork.Repository<Employee>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }

        public override async Task<Guid> UpdateAsync(EmployeeUpdateDto employeeUpdateDto)
        {
            var dbEmployee = await _unitOfWork.Repository<Employee>().FindAsync(employeeUpdateDto.Id);
            var employee = _mapper.Map(employeeUpdateDto, dbEmployee);
            employee.FullName = employeeUpdateDto.FirstName + " " + employeeUpdateDto.LastName;
            await _unitOfWork.Repository<Employee>().UpdateAsync(employee);
            await _unitOfWork.SaveChangesAsync();
            return employee.Id;
        }

    }
}
