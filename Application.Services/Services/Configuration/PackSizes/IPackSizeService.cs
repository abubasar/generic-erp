using Application.Core.Entities;
using Application.Services.Dtos.Configuration.PackSize;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Configuration.PackSizes
{
    public interface IPackSizeService : IBaseService<PackSize, PackSizeCreationDto, PackSizeUpdateDto, PackSizeRequestModel, PackSizeViewModel>
    {
    }
}
