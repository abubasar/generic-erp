using Application.Api.Attributes;
using Application.Core.Constants;
using Application.Services.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;
        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }
        [Authorize(Permissions.Maintenances.DownlaodBackup)]
        [HttpGet]
        [Route("download-backup")]
        public async Task<IActionResult> DownloadBackup()
        {
            var backupFilePath = "";
            try
            {
                backupFilePath = await _maintenanceService.BackupDatabaseAsync();

                if (!System.IO.File.Exists(backupFilePath))
                {
                    return NotFound(new { Message = "Backup file not found." });
                }

                var stream = new FileStream(backupFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
                var fileName = Path.GetFileName(backupFilePath);
                var contentType = "application/octet-stream";

                // Ensure the file stream is disposed correctly, which will also close the stream
                return new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = fileName,
                    EnableRangeProcessing = true, // Optional: Allows for range processing if the client supports it
                };
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
           
        }

    }
}
