using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Application.Core.Common
{
    public class PdfHelper
    {
        public static byte[] AddPageNumbers(byte[] pdf, string userName)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

            // step 1: creation of a document-object
            Document document = new Document(psize, 50, 50, 50, 50);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int p = 0;
            // Console.WriteLine("There are " + n + " pages in the document.");
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                p++;

                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                cb.BeginText();
                cb.SetFontAndSize(bf, 8);

                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, (string.IsNullOrEmpty(userName) ? "Printed " : "Printed By: " + userName + ", ") + "Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt") + ", Software By: BUTS, www.butsbd.com", 300, 13, 0);
                if (!string.IsNullOrEmpty(userName))
                {
                    cb.ShowTextAligned(PdfContentByte.ALIGN_RIGHT, "Page " + p + " of " + n, 578, 13, 0);
                }

                cb.EndText();

                //cb.BeginText();
                //cb.SetFontAndSize(bf, 8);


                //cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Software By: Bangladesh Unique Technology Services, www.butsbd.com", 300, 8, 0);
                //cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }
        public static byte[] AddFooter(byte[] pdf, string userName)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

            // step 1: creation of a document-object
            Document document = new Document(psize, 50, 50, 50, 50);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int p = 0;
            // Console.WriteLine("There are " + n + " pages in the document.");
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                p++;

                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                cb.BeginText();
                cb.SetFontAndSize(bf, 8);

                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Printed By: " + userName + ", Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), 300, 16, 0);
                cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 8);


                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Software By: Bangladesh Unique Technology Services, www.butsbd.com", 300, 6, 0);
                cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }
        public static byte[] AddA5Footer(byte[] pdf, string userName)
        {
            MemoryStream ms = new MemoryStream();
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(pdf);
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            // we retrieve the size of the first page
            Rectangle psize = reader.GetPageSize(1);

            // step 1: creation of a document-object
            Document document = new Document(psize, 50, 50, 50, 50);
            // step 2: we create a writer that listens to the document
            PdfWriter writer = PdfWriter.GetInstance(document, ms);
            // step 3: we open the document

            document.Open();
            // step 4: we add content
            PdfContentByte cb = writer.DirectContent;

            int p = 0;
            // Console.WriteLine("There are " + n + " pages in the document.");
            for (int page = 1; page <= reader.NumberOfPages; page++)
            {
                document.NewPage();
                p++;

                PdfImportedPage importedPage = writer.GetImportedPage(reader, page);
                cb.AddTemplate(importedPage, 0, 0);

                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

                cb.BeginText();
                cb.SetFontAndSize(bf, 7);

                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Printed By: " + userName + ", Date: " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToString("hh:mm:ss tt"), 210, 16, 0);
                cb.EndText();

                cb.BeginText();
                cb.SetFontAndSize(bf, 7);


                cb.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "Software By: Bangladesh Unique Technology Services, www.butsbd.com", 210, 8, 0);
                cb.EndText();
            }
            // step 5: we close the document
            document.Close();
            return ms.ToArray();
        }
    }
}
