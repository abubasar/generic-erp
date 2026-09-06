using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace Application.Core.ExcelHelper
{
    public static class ExcelGenerator
    {
        public static byte[] GenerateExcelByteArrayFromItextTable(PdfPTable table, string sheetName, string? companyName, string? companyAddress, string reportTitle)
        {
            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
                //header
                int numColumns = table.Rows.Count > 0 ? table.Rows[0].GetCells().Length : 1;
                worksheet.InsertRow(1, 3);

                worksheet.Cells["A1"].Value = companyName;
                worksheet.Cells["A1"].Style.Font.Size = 16f;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A1:{GetExcelColumnName(numColumns)}1"].Merge = true;

                worksheet.Cells["A2"].Value = companyAddress;
                worksheet.Cells["A2"].Style.Font.Bold = false;
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A2:{GetExcelColumnName(numColumns)}2"].Merge = true;

                worksheet.Cells["A3"].Value = reportTitle;
                worksheet.Cells["A3"].Style.Font.Size = 14f;
                worksheet.Cells["A3"].Style.Font.Bold = true;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A3:{GetExcelColumnName(numColumns)}3"].Merge = true;
                //data table
                for (int row = 0; row < table.Rows.Count; row++)
                {
                    for (int col = 0; col < table.Rows[row].GetCells().Length; col++)
                    {
                        PdfPCell cell = table.Rows[row].GetCells()[col];
                        if (cell != null)
                        {
                            string cellValue = cell.Phrase.Content;
                            bool isBold = cell.Phrase.Font.IsBold();
                            float fontSize = cell.Phrase.Font.Size;
                            //BaseColor fontColor = cell.Phrase.Font.Color?? BaseColor.Black;
                            //BaseColor backgroundColor = cell.BackgroundColor??BaseColor.White;
                            if (cell.Colspan > 1)
                            {
                                worksheet.Cells[row + 4, col + 1, row + 4, col + cell.Colspan].Merge = true;
                                worksheet.Cells[row + 4, col + 1].Value = IsNumber(cellValue) ? Convert.ToDecimal(cellValue) : (object)cellValue;
                                worksheet.Cells[row + 4, col + 1, row + 4, col + cell.Colspan].Style.Font.Bold = isBold;
                                worksheet.Cells[row + 4, col + 1, row + 4, col + cell.Colspan].Style.Font.Size = fontSize;
                                //worksheet.Cells[row + 4, col + 1, row + 4, col + cell.Colspan].Style.Font.Color.SetColor(ConvertBaseColorToRgb(fontColor));
                                //worksheet.Cells[row + 4, col + 1, row + 4, col + cell.Colspan].Style.Fill.BackgroundColor.SetColor(ConvertBaseColorToRgb(backgroundColor));
                                col += cell.Colspan - 1;
                            }
                            else
                            {
                                worksheet.Cells[row + 4, col + 1].Value = IsNumber(cellValue) ? Convert.ToDecimal(cellValue) : (object)cellValue;
                                worksheet.Cells[row + 4, col + 1].Style.Font.Bold = isBold;
                                worksheet.Cells[row + 4, col + 1].Style.Font.Size = fontSize;
                                //worksheet.Cells[row + 4, col + 1].Style.Font.Color.SetColor(ConvertBaseColorToRgb(fontColor));
                                //worksheet.Cells[row + 4, col + 1].Style.Fill.BackgroundColor.SetColor(ConvertBaseColorToRgb(backgroundColor));
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                worksheet.Cells.AutoFitColumns();
                return excelPackage.GetAsByteArray();
            }
        }
        public static byte[] GenerateExcelByteArrayFromItextTable(PdfPTable filterTable, PdfPTable dataTable, string sheetName, string? companyName, string? companyAddress, string reportTitle)
        {
            var filterTableRowCount = filterTable.Rows.Count;
            using (var excelPackage = new ExcelPackage())
            {
                var worksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
                //header
                int numColumns = dataTable.Rows.Count > 0 ? dataTable.Rows[0].GetCells().Length : 1;
                worksheet.InsertRow(1, 3 + filterTableRowCount);

                worksheet.Cells["A1"].Value = companyName;
                worksheet.Cells["A1"].Style.Font.Size = 16f;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A1:{GetExcelColumnName(numColumns)}1"].Merge = true;

                worksheet.Cells["A2"].Value = companyAddress;
                worksheet.Cells["A2"].Style.Font.Bold = false;
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A2:{GetExcelColumnName(numColumns)}2"].Merge = true;

                worksheet.Cells["A3"].Value = reportTitle;
                worksheet.Cells["A3"].Style.Font.Size = 14f;
                worksheet.Cells["A3"].Style.Font.Bold = true;
                worksheet.Cells["A3"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[$"A3:{GetExcelColumnName(numColumns)}3"].Merge = true;

                //filter table
                for (int row = 1; row <= filterTableRowCount; row++)
                {
                    PdfPCell cell = filterTable.Rows[row - 1].GetCells()[0];
                    if (cell != null)
                    {
                        string cellValue = cell.Phrase.Content;
                        bool isBold = cell.Phrase.Font.IsBold();
                        float fontSize = cell.Phrase.Font.Size;
                        worksheet.Cells[$"A{3 + row}"].Value = cellValue;
                        worksheet.Cells[$"A{3 + row}"].Style.Font.Size = fontSize;
                        worksheet.Cells[$"A{3 + row}"].Style.Font.Bold = isBold;
                        worksheet.Cells[$"A{3 + row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        worksheet.Cells[$"A{3 + row}:{GetExcelColumnName(numColumns)}{3 + row}"].Merge = true;
                    }
                    else
                    {
                        continue;
                    }

                }

                //data table
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    for (int col = 0; col < dataTable.Rows[row].GetCells().Length; col++)
                    {
                        PdfPCell cell = dataTable.Rows[row].GetCells()[col];
                        if (cell != null)
                        {
                            string cellValue = cell.Phrase.Content;
                            bool isBold = cell.Phrase.Font.IsBold();
                            float fontSize = cell.Phrase.Font.Size;
                            if (cell.Colspan > 1)
                            {
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1, row + 4 + filterTableRowCount, col + cell.Colspan].Merge = true;
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1].Value = IsNumber(cellValue) ? Convert.ToDecimal(cellValue) : (object)cellValue;
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1, row + 4 + filterTableRowCount, col + cell.Colspan].Style.Font.Bold = isBold;
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1, row + 4 + filterTableRowCount, col + cell.Colspan].Style.Font.Size = fontSize;
                                col += cell.Colspan - 1;
                            }
                            else
                            {
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1].Value = IsNumber(cellValue) ? Convert.ToDecimal(cellValue) : (object)cellValue;
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1].Style.Font.Bold = isBold;
                                worksheet.Cells[row + 4 + filterTableRowCount, col + 1].Style.Font.Size = fontSize;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                worksheet.Cells.AutoFitColumns();
                return excelPackage.GetAsByteArray();
            }
        }
        private static string GetExcelColumnName(int columnIndex)
        {
            int dividend = columnIndex;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        private static bool IsNumber(string value)
        {
            return decimal.TryParse(value, out _);
        }
     
    }
}
