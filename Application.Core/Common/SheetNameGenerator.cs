namespace Application.Core.Common
{
    public static class SheetNameGenerator
    {
        public static string Generate(string baseName, DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate.HasValue && toDate.HasValue)
                return $"{baseName}_{fromDate.Value.ToString("dd_MM_yy")}_to_{toDate.Value.ToString("dd_MM_yy")}";
            else return baseName;
        }
    }
}
