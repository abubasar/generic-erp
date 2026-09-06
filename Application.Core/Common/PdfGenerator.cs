using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Dynamic;
using System.Reflection;

namespace Application.Core.Common
{

    public class PdfGenerator
    {
        private readonly Document document;
        private readonly PdfWriter writer;

        public PdfGenerator(MemoryStream stream, bool isLandscape)
        {
            document = new Document(isLandscape ? new(PageSize.A4.Height, PageSize.A4.Width) : PageSize.A4);
            document.SetMargins(20f, 20f, 20f, 20f);
            writer = PdfWriter.GetInstance(document, stream);
        }
        public void AddHeader(string reportTitleName, string? companyName, string? address, string? mobile, string? email)
        {
            document.Open();
            PdfPTable headerPage = new(1);
            Font fontArial10 = FontFactory.GetFont("Arial", 10);
            Font fontArial13Bold = FontFactory.GetFont("Arial", 13, Font.BOLD);
            Font fontArial14Bold = FontFactory.GetFont("Arial", 14, Font.BOLD);
            float[] widthsCellsHeaderPage = new float[] { 100f };
            headerPage.SetWidths(widthsCellsHeaderPage);
            headerPage.WidthPercentage = 100;

            headerPage.AddCell(new PdfPCell(new Phrase(companyName, fontArial13Bold)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(address, fontArial10)) { Border = 0, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(mobile + ", Email : " + email, fontArial10)) { Border = 0, PaddingTop = 5f, HorizontalAlignment = Element.ALIGN_CENTER });
            headerPage.AddCell(new PdfPCell(new Phrase(reportTitleName, fontArial14Bold)) { Border = 0, PaddingTop = 5f, PaddingBottom = 5f, HorizontalAlignment = Element.ALIGN_CENTER });

            document.Add(headerPage);
        }

        public PdfPTable AddTable<T>(List<T> data, List<string> headers, List<float> columnWidths, Font headerFont, Font tableDataFont, List<string>? numericColumnsForSum = null)
        {
            PdfPTable table = new PdfPTable(headers.Count)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };
            table.SetWidths(columnWidths.ToArray());

            foreach (var header in headers)
            {
                table.AddCell(GetHeaderCell(header, headerFont, Element.ALIGN_CENTER));
            }
            table.HeaderRows = 1;
            foreach (var item in data)
            {
                var properties = item!.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    bool isInteger = IsInteger(value!);
                    bool isDecimal = IsDecimal(value!);
                    if (isInteger)
                        table.AddCell(GetTableCell(Convert.ToInt32(value!).ToString("#,##0"), tableDataFont, Element.ALIGN_RIGHT));
                    else if (isDecimal)
                        table.AddCell(GetTableCell(Convert.ToDecimal(value!).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                    else
                        table.AddCell(GetTableCell(value!.ToString(), tableDataFont, Element.ALIGN_LEFT));

                }
            }
            if (numericColumnsForSum != null && numericColumnsForSum.Any())
            {
                Dictionary<string, decimal> sums = CalculateNumericColumnSums(data, numericColumnsForSum);
                var totalColumnPosition = (headers.Count - numericColumnsForSum.Count) - 1;
                for (int i = 0; i < headers.Count; i++)
                {
                    PdfPCell sumCell = new PdfPCell();
                    if (numericColumnsForSum.Contains(headers[i]) && sums.TryGetValue(headers[i], out decimal sum))
                    {
                        sumCell = GetTableCell(sum.ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT);
                    }
                    else
                    {
                        if (totalColumnPosition == i)
                            sumCell = GetTableCell("Total", tableDataFont, Element.ALIGN_RIGHT);
                        else
                            sumCell = GetTableCell("", tableDataFont, Element.ALIGN_RIGHT);
                    }

                    table.AddCell(sumCell);
                }
            }
            document.Add(table);

            return table;
        }
        public void AddTable<T>(List<T> data, Dictionary<string, bool> headers, List<float> columnWidths, Font headerFont, Font tableDataFont)
        {
            PdfPTable table = new PdfPTable(headers.Count)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };
            table.SetWidths(columnWidths.ToArray());

            foreach (var kvp in headers)
            {
                string columnName = kvp.Key;
                bool isSummable = kvp.Value;

                table.AddCell(GetHeaderCell(columnName, headerFont, Element.ALIGN_CENTER));
            }
            table.HeaderRows = 1;

            // Prepare dictionary to store column sums
            Dictionary<string, decimal> columnSums = headers
                .Where(kvp => kvp.Value)
                .ToDictionary(kvp => kvp.Key, _ => 0m);

            foreach (var item in data)
            {
                var properties = item!.GetType().GetProperties();
                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var value = property.GetValue(item);
                    string columnName = property.Name;

                    bool isSummable = headers.TryGetValue(columnName, out bool shouldSum);
                    bool isNumeric = isSummable && (IsInteger(value!) || IsDecimal(value!));

                    if (isSummable && isNumeric)
                    {
                        columnSums[columnName] += Convert.ToDecimal(value);
                    }

                    table.AddCell(GetTableCell(value!.ToString() ?? "", tableDataFont, isNumeric ? Element.ALIGN_RIGHT : Element.ALIGN_LEFT));
                }
            }

            foreach (var kvp in headers)
            {
                string columnName = kvp.Key;
                bool shouldSum = kvp.Value;

                if (shouldSum && columnSums.ContainsKey(columnName))
                {
                    table.AddCell(GetTableCell(columnSums[columnName].ToString(), tableDataFont, Element.ALIGN_RIGHT));
                }
                else
                {
                    table.AddCell(GetTableCell("", tableDataFont, Element.ALIGN_RIGHT));
                }
            }

            document.Add(table);
        }


        public PdfPTable AddTable(PdfPTable pdfTable)
        {
            document.Add(pdfTable);
            return pdfTable;
        }

        public PdfPCell GetHeaderCell(string? text, Font font, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(AddSpaceBeforeUppercase(text ?? ""), font));
            cell.PaddingTop = 3f;
            cell.PaddingBottom = 3f;
            cell.HorizontalAlignment = alignment;
            return cell;
        }
        public PdfPCell GetTableCell(string? text, Font font, int alignment, int colspan = 1)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font));
            cell.PaddingTop = 3f;
            cell.PaddingBottom = 3f;
            cell.PaddingLeft = 3f;
            cell.PaddingRight = 3f;
            cell.HorizontalAlignment = alignment;
            cell.Colspan = colspan;
            return cell;
        }
        private bool IsInteger(object value)
        {
            return value is sbyte || value is byte ||
                   value is short || value is ushort ||
                   value is int || value is uint ||
                   value is long || value is ulong;
        }
        private bool IsDecimal(object value)
        {
            return value is float || value is double || value is decimal;
        }
        private string AddSpaceBeforeUppercase(string input)
        {
            bool isUpperCase = input.All(char.IsUpper);
            if (isUpperCase) return input;
            string output = string.Empty;

            foreach (char c in input)
            {
                if (char.IsUpper(c))
                {
                    output += " " + c;
                }
                else
                {
                    output += c;
                }
            }

            return output;
        }

        public Dictionary<string, decimal> CalculateNumericColumnSums<T>(List<T> data, List<string> columnNames)
        {
            Dictionary<string, decimal> sums = new Dictionary<string, decimal>();
            foreach (string columnName in columnNames)
            {
                decimal sum = 0;
                foreach (var item in data)
                {
                    PropertyInfo property = item.GetType().GetProperty(columnName);
                    if (property == null)
                        throw new ArgumentException($"Column '{columnName}' not found in the data type.");
                    var value = property.GetValue(item);
                    if (IsInteger(value!) || IsDecimal(value!))
                    {
                        sum += Convert.ToDecimal(value);
                    }
                }

                sums[columnName] = sum;
            }

            return sums;
        }

        public void Close()
        {
            document.Close();
        }
        public PdfPTable AddTableForExpando<T>(List<T> data, Font headerFont, Font tableDataFont)
        {
            if (!data.Any()) return new PdfPTable(10);
            var obj = data[0] as ExpandoObject;
            IDictionary<string, object> expandoDict = obj!;
            List<string> headers = expandoDict!.Keys.ToList();
            // Prepare dictionary to store column sums
            Dictionary<string, decimal> columnSums = new Dictionary<string, decimal>();
            foreach (var kvp in expandoDict)
            {
                if (IsInteger(kvp.Value) || IsDecimal(kvp.Value))
                {
                    columnSums[kvp.Key] = 0m;
                }
            }
            int numberOfColumns = headers.Count;
            PdfPTable table = new PdfPTable(numberOfColumns)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };

            // Set equal column widths
            List<float> columnWidths = new List<float>();
            for (int i = 0; i < numberOfColumns; i++)
            {
                columnWidths.Add(100f);
            }
            table.SetWidths(columnWidths.ToArray());
            foreach (var header in headers)
            {
                table.AddCell(GetHeaderCell(header, headerFont, Element.ALIGN_CENTER));
            }


            table.HeaderRows = 1;
            foreach (var item in data)
            {
                IDictionary<string, object> expandoDictionary = item as ExpandoObject;
                foreach (var kvp in expandoDictionary)
                {
                    var value = kvp.Value;
                    string columnName = kvp.Key;
                    bool isInteger = IsInteger(value!);
                    bool isDecimal = IsDecimal(value!);
                    if (isInteger)
                    {
                        table.AddCell(GetTableCell(Convert.ToInt32(value!).ToString("#,##0"), tableDataFont, Element.ALIGN_RIGHT));
                        columnSums[columnName] += Convert.ToDecimal(value);
                    }

                    else if (isDecimal)
                    {
                        table.AddCell(GetTableCell(Convert.ToDecimal(value!).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                        columnSums[columnName] += Convert.ToDecimal(value);
                    }

                    else
                        table.AddCell(GetTableCell(value!.ToString(), tableDataFont, Element.ALIGN_LEFT));
                }

            }
            foreach (var columnName in headers)
            {

                if (columnSums.ContainsKey(columnName))
                {
                    table.AddCell(GetTableCell(columnSums[columnName].ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                }
                else
                {
                    table.AddCell(GetTableCell("Grand Total : ", tableDataFont, Element.ALIGN_RIGHT));
                }
            }
            document.Add(table);
            return table;
        }
        public PdfPTable AddTableForExpandoForAverage<T>(List<T> data, Font headerFont, Font tableDataFont)
        {
            if (!data.Any()) return new PdfPTable(10);
            var obj = data[0] as ExpandoObject;
            IDictionary<string, object> expandoDict = obj!;
            List<string> headers = expandoDict!.Keys.ToList();
            // Prepare dictionary to store column sums
            Dictionary<string, decimal> columnSums = new Dictionary<string, decimal>();
            Dictionary<string, int> columnCounts = new Dictionary<string, int>();
            foreach (var kvp in expandoDict)
            {
                if (IsInteger(kvp.Value) || IsDecimal(kvp.Value))
                {
                    columnSums[kvp.Key] = 0m;
                    columnCounts[kvp.Key] = 0;
                }
            }
            int numberOfColumns = headers.Count;
            PdfPTable table = new PdfPTable(numberOfColumns)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };

            // Set equal column widths
            List<float> columnWidths = new List<float>();
            for (int i = 0; i < numberOfColumns; i++)
            {
                columnWidths.Add(100f);
            }
            table.SetWidths(columnWidths.ToArray());
            foreach (var header in headers)
            {
                table.AddCell(GetHeaderCell(header, headerFont, Element.ALIGN_CENTER));
            }


            table.HeaderRows = 1;
            foreach (var item in data)
            {
                IDictionary<string, object> expandoDictionary = item as ExpandoObject;
                foreach (var kvp in expandoDictionary!)
                {
                    var value = kvp.Value;
                    string columnName = kvp.Key;
                    bool isInteger = IsInteger(value!);
                    bool isDecimal = IsDecimal(value!);
                    if (isInteger)
                    {
                        table.AddCell(GetTableCell(Convert.ToInt32(value!).ToString("#,##0"), tableDataFont, Element.ALIGN_RIGHT));
                        columnSums[columnName] += Convert.ToDecimal(value);
                        if (Convert.ToInt32(value!) > 0) columnCounts[columnName]++;
                    }

                    else if (isDecimal)
                    {
                        table.AddCell(GetTableCell(Convert.ToDecimal(value!).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                        columnSums[columnName] += Convert.ToDecimal(value);
                        if (Convert.ToDecimal(value!) > 0m) columnCounts[columnName]++;
                    }

                    else
                        table.AddCell(GetTableCell(value!.ToString(), tableDataFont, Element.ALIGN_LEFT));
                }

            }
            foreach (var columnName in headers)
            {

                if (columnSums.ContainsKey(columnName))
                {
                    if (columnCounts[columnName] == 0)
                    {
                        table.AddCell(GetTableCell("0.00", tableDataFont, Element.ALIGN_RIGHT));
                    }
                    else
                        table.AddCell(GetTableCell((columnSums[columnName] / columnCounts[columnName]).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                }
                else
                {
                    table.AddCell(GetTableCell("Average : ", tableDataFont, Element.ALIGN_RIGHT));
                }
            }
            document.Add(table);
            return table;
        }

        public PdfPTable AddTableForExpando2(Dictionary<string, List<ExpandoObject>> keyValuePairs, Font headerFont, Font tableDataFont)
        {
            if (!keyValuePairs.Values.Any()) return new PdfPTable(10);
            var obj = keyValuePairs.Values.First().First();
            IDictionary<string, object> expandoDict = obj!;
            List<string> headers = expandoDict!.Keys.ToList();
            int numberOfColumns = headers.Count;
            PdfPTable table = new PdfPTable(numberOfColumns)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };
            // Set equal column widths
            List<float> columnWidths = new List<float>();
            columnWidths.Add(300f);
            for (int i = 1; i < numberOfColumns; i++)
            {
                columnWidths.Add(100f);
            }
            table.SetWidths(columnWidths.ToArray());
            foreach (var header in headers)
            {
                table.AddCell(GetHeaderCell(header, headerFont, Element.ALIGN_CENTER));
            }
            // Prepare dictionary to store grand total
            Dictionary<string, decimal> columnTotalOfSums = new Dictionary<string, decimal>();
            foreach (var kvp in expandoDict)
            {
                if (IsInteger(kvp.Value) || IsDecimal(kvp.Value))
                {
                    columnTotalOfSums[kvp.Key] = 0m;
                }
            }
            table.HeaderRows = 1;
            foreach (var keyValuePair in keyValuePairs)
            {
                var data = keyValuePair.Value;
                // Prepare dictionary to store column sums/total
                Dictionary<string, decimal> columnSums = new Dictionary<string, decimal>();
                foreach (var kvp in expandoDict)
                {
                    if (IsInteger(kvp.Value) || IsDecimal(kvp.Value))
                    {
                        columnSums[kvp.Key] = 0m;
                    }
                }
                table.AddCell(new PdfPCell(new Phrase(keyValuePair.Key, headerFont)) { Colspan = numberOfColumns, HorizontalAlignment = 0, PaddingTop = 3f, PaddingBottom = 3f, PaddingLeft = 5f, PaddingRight = 5f });
                foreach (var item in data!)
                {
                    IDictionary<string, object> expandoDictionary = item;
                    foreach (var kvp in expandoDictionary)
                    {
                        var value = kvp.Value;
                        string columnName = kvp.Key;
                        bool isInteger = IsInteger(value!);
                        bool isDecimal = IsDecimal(value!);
                        if (isInteger)
                        {
                            table.AddCell(GetTableCell(Convert.ToInt32(value!).ToString("#,##0"), tableDataFont, Element.ALIGN_RIGHT));
                            columnSums[columnName] += Convert.ToDecimal(value);
                        }
                        else if (isDecimal)
                        {
                            table.AddCell(GetTableCell(Convert.ToDecimal(value!).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                            columnSums[columnName] += Convert.ToDecimal(value);
                        }
                        else
                            table.AddCell(GetTableCell(value!.ToString(), tableDataFont, Element.ALIGN_LEFT));
                    }

                }
                //total
                foreach (var columnName in headers)
                {
                    if (columnSums.ContainsKey(columnName))
                    {
                        table.AddCell(GetTableCell(columnSums[columnName].ToString("#,##0.00"), headerFont, Element.ALIGN_RIGHT));
                        columnTotalOfSums[columnName] += Convert.ToDecimal(columnSums[columnName]);
                    }
                    else
                        table.AddCell(GetTableCell(keyValuePair.Key + " Total : ", headerFont, Element.ALIGN_RIGHT));
                }

            }
            //grand total
            foreach (var columnName in headers)
            {
                if (columnTotalOfSums.ContainsKey(columnName))
                {
                    table.AddCell(GetTableCell(columnTotalOfSums[columnName].ToString("#,##0.00"), headerFont, Element.ALIGN_RIGHT));
                }
                else
                    table.AddCell(GetTableCell("Grand Total : ", headerFont, Element.ALIGN_RIGHT));
            }
            document.Add(table);
            return table;
        }

        //newly added
        public PdfPTable AddTable<T>(int numberOfColumns, List<T> data, List<TableHeader> headers, List<float> columnWidths, Font headerFont, Font tableDataFont, List<Func<T, decimal>>? sumProperties = null)
        {
            PdfPTable table = new PdfPTable(numberOfColumns)
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };
            table.SetWidths(columnWidths.ToArray());
            int maxValue = headers[0].RowSpan;
            foreach (var header in headers)
            {
                if (header.RowSpan > maxValue)
                    maxValue = header.RowSpan;
                PdfPCell cell = new PdfPCell(new Phrase(header.Text, headerFont));
                cell.Colspan = header.ColSpan;
                cell.Rowspan = header.RowSpan;
                cell.PaddingTop = 3;
                cell.PaddingBottom = 3;
                cell.HorizontalAlignment = Element.ALIGN_CENTER; // Default alignment
                cell.VerticalAlignment = Element.ALIGN_MIDDLE; // Default alignment
                table.AddCell(cell);
            }
            table.HeaderRows = maxValue;
            int sl = 0;
            foreach (var item in data)
            {
                sl++;
                table.AddCell(GetTableCell(sl.ToString(), tableDataFont, Element.ALIGN_CENTER));
                var properties = item!.GetType().GetProperties();
                foreach (var property in properties)
                {
                    var value = property.GetValue(item);
                    bool isInteger = IsInteger(value!);
                    bool isDecimal = IsDecimal(value!);
                    if (isInteger)
                        table.AddCell(GetTableCell(Convert.ToInt32(value!).ToString("#,##0"), tableDataFont, Element.ALIGN_RIGHT));
                    else if (isDecimal)
                        table.AddCell(GetTableCell(Convert.ToDecimal(value!).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                    else
                        table.AddCell(GetTableCell(value!.ToString(), tableDataFont, Element.ALIGN_LEFT));

                }
            }
            //total row
            if (sumProperties is not null && sumProperties.Any())
            {
                var totalTextColspan = numberOfColumns - sumProperties.Count;
                table.AddCell(GetTableCell("Total", tableDataFont, Element.ALIGN_RIGHT, totalTextColspan));
                foreach (var propertySum in sumProperties)
                {
                    decimal sum = data.Sum(propertySum);
                    table.AddCell(GetTableCell(Convert.ToDecimal(sum!).ToString("#,##0.00"), tableDataFont, Element.ALIGN_RIGHT));
                }
            }

            document.Add(table);
            return table;
        }
    }

}

public class TableHeader
{
    public string Text { get; set; } = string.Empty;
    public int ColSpan { get; set; }
    public int RowSpan { get; set; }
}

public class TableCell
{
    public string Text { get; set; } = string.Empty;
    public int ColSpan { get; set; }
    public int RowSpan { get; set; }
    public int HorizontalAlignment { get; set; }
}