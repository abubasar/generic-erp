using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.Manufacturer;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Configuration.Manufacturers
{
    public class ManufacturerService : BaseService<Manufacturer, ManufacturerCreationDto, ManufacturerUpdateDto, ManufacturerRequestModel, ManufacturerViewModel>, IManufacturerService
    {
        public ManufacturerService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext) : base(unitOfWork, mapper, workContext)
        {
        }
    }
}
