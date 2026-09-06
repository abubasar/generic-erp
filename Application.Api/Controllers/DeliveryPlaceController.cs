using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.DeliveryPlace;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.DeliveryPlaces;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeliveryPlaceController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeliveryPlaceService _deliveryPlaceService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public DeliveryPlaceController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IDeliveryPlaceService deliveryPlaceService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _deliveryPlaceService = deliveryPlaceService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.DeliveryPlaces.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<DeliveryPlaceViewModel>, int>>> Search(DeliveryPlaceRequestModel request)
        {
            return await Result<Tuple<List<DeliveryPlaceViewModel>, int>>.SuccessAsync(await _deliveryPlaceService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.DeliveryPlaces.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] DeliveryPlaceCreationDto deliveryPlaceCreationDto)
        {
            var deliveryPlaceId = await _deliveryPlaceService.AddAsync(deliveryPlaceCreationDto);
            return await Result<Guid>.SuccessAsync(deliveryPlaceId, "Delivery Place Added Successfully");
        }
        [Authorize(Permissions.DeliveryPlaces.Edit)]
        [HttpPost("/api/deliveryPlace/update")]
        public virtual async Task<Result> Put([FromBody] DeliveryPlaceUpdateDto deliveryPlaceUpdateDto)
        {
            var deliveryPlaceId = await _deliveryPlaceService.UpdateAsync(deliveryPlaceUpdateDto);
            return await Result<Guid>.SuccessAsync(deliveryPlaceId, "Delivery Place Updated Successfully");
        }
        [Authorize(Permissions.DeliveryPlaces.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var deliveryPlaceId = await _deliveryPlaceService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(deliveryPlaceId, "Delivery Place Deleted Successfully");
        }
        [HttpPost]
        [Route("print")]
        public virtual async Task<IActionResult> Print(DeliveryPlaceRequestModel request)
        {
            try
            {
                request.Page = -1;
                var list = await _deliveryPlaceService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Name" };
                List<float> columnWidths = new List<float> { 10f, 90f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var deliveryPlace in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        deliveryPlace?.Name,
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _configurationPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, "Delivery Place List");
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
