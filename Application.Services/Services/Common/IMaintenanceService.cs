using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Services.Common
{
    public  interface IMaintenanceService
    {
        Task<string> BackupDatabaseAsync();
    }
}
