using Application.Core.Common;
using Application.Services.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportFromExcelController : ControllerBase
    {
        private readonly IImportManager _importManager;
        public ImportFromExcelController(IImportManager importManager)
        {
            _importManager = importManager;
        }

        [HttpPost]
        [Route("rm-upload")]
        public virtual async Task<Result> ImportRMFromXlsx(IFormFile importexcelfile)
        {

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportRawMaterialsFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    throw new Exception("Upload error");
                }
                return await Result<string>.SuccessAsync("", "Raw Materials including Packaging Materials Uploaded Successfully");

            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("fg-upload")]
        public virtual async Task<Result> ImportFGFromXlsx(IFormFile importexcelfile)
        {

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportFinishedGoodsFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    throw new Exception("Upload error");
                }
                return await Result<string>.SuccessAsync("", "FinishedGoods Uploaded Successfully");

            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("bank-balance-upload")]
        public virtual async Task<Result> ImportBankBalanceFromXlsx(IFormFile importexcelfile)
        {

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportCashAndBankOpeningBalanceFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    throw new Exception("Upload error");
                }
                return await Result<string>.SuccessAsync("", "Bank Balance Uploaded Successfully");

            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("customer-balance-upload")]
        public virtual async Task<Result> ImportCustomerBalanceFromXlsx(IFormFile importexcelfile)
        {

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportCustomerOpeningBalanceFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    throw new Exception("Upload error");
                }
                return await Result<string>.SuccessAsync("", "Customer Balance Uploaded Successfully");

            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpPost]
        [Route("supplier-balance-upload")]
        public virtual async Task<Result> ImportSupplierBalanceFromXlsx(IFormFile importexcelfile)
        {

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    _importManager.ImportSupplierOpeningBalanceFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    throw new Exception("Upload error");
                }
                return await Result<string>.SuccessAsync("", "Supplier Balance Uploaded Successfully");

            }
            catch (Exception exception)
            {
                return await Result<string>.FailAsync(exception.ToString(), exception.Message);
            }
        }
        [HttpGet]
        [Route("/api/ImportFromExcel/download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "excel-format", Path.GetFileName(fileName + ".xlsx"));
            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(filePath, FileMode.Open))
            {
                fileStream.CopyTo(memoryStream);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);
            return File(memoryStream, MimeTypes.ApplicationOctetStream, fileName);
        }
    }
}
