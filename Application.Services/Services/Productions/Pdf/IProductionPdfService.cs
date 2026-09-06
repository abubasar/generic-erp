using Application.Services.SearchRequestModels.Production;
using Application.Services.ViewModels.Production;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Productions.Pdf
{
    public interface IProductionPdfService
    {
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductionDetailsReportToPdf(MemoryStream stream, List<ProductionViewModel> productionViewModels, string reportTitle, ProductionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintProductionReportToPdf(MemoryStream stream, List<ProductionViewModel> productionViewModels, string reportTitle, ProductionRequestModel request);
    }
}
