
using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Auth;
using Application.Services.SearchRequestModels.Auth;
using Application.Services.Services.Auth.Common;
using Application.Services.Services.Auth.Users;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfigurationPdfService _configurationPdfService;
        public UserController(IUserService userService,
           IUnitOfWork unitOfWork,
            IHttpContextAccessor contextAccessor, IAuthService authService, IConfigurationPdfService configurationPdfService)
        {
            _authService = authService;
            _userService = userService;
            _contextAccessor = contextAccessor;
            _unitOfWork = unitOfWork;
            _configurationPdfService = configurationPdfService;
        }

        [HttpPost]
        [Authorize(Permissions.Users.View)]
        [Route("search")]
        public virtual async Task<Result<Tuple<List<UserViewModel>, int>>> Search(UserRequestModel request)
        {
            Tuple<List<UserViewModel>, int> content = await _userService.SearchAsync(request);
            return await Result<Tuple<List<UserViewModel>, int>>.SuccessAsync(content, "Users Found");
        }


        [HttpPost]
        public virtual async Task<Result> Register(RegisterDto model)
        {
            if (model.EmployeeId.HasValue)
            {
                if (await _userService.UserExistsForEmployeeId(model.EmployeeId.Value)) return await Result<string>.FailAsync("", "User already exists for this Employee");
            }
            else throw new BadRequestException("Please, Select Employee First");
            if (await _authService.UserExists(model.Username)) return await Result<string>.FailAsync("", "User already exists.");

            CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
            try
            {
                User user = new User();
                user.Id = Guid.NewGuid();
                user.Username = model.Username;
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.RoleId = model.RoleId;
                user.EmployeeId = model.EmployeeId;
                await _unitOfWork.Repository<User>().AddAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return await Result<Guid>.SuccessAsync(user.Id, "Registration Completed Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost("/api/user/update")]
        [Authorize(Permissions.Users.Edit)]
        public virtual async Task<Result> Put([FromBody] RegisterUpdateDto model)
        {
            try
            {
                User user = await _unitOfWork.Repository<User>().FindAsync(model.Id);
                if (user == null) return await Result<User>.FailAsync("", "User Not Found");
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    CreatePasswordHash(model.Password, out byte[] passwordHash, out byte[] passwordSalt);
                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }
                user.Username = model.Username;
                user.RoleId = model.RoleId;
                user.EmployeeId = model.EmployeeId;
                await _unitOfWork.Repository<User>().UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();
                return await Result<Guid>.SuccessAsync(user.Id, "User Updated Successfully");
            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(UserRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _userService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "User Name", "Role" };
                List<float> columnWidths = new List<float> { 10f, 45f, 45f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var user in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        user?.Username,
                        Role = user?.RoleName
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "User List");
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

    }
}
