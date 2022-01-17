using System.IO;
using NPOI.XWPF.UserModel;

namespace AcceptanceTests.Helpers.FileGenerators
{
    public static class DocxGenerator
    {
        public static (long, byte[]) GenerateDocxMaterial(string fileName, string content)
        {
            XWPFDocument doc = new XWPFDocument();
            var p = doc.CreateParagraph();
            var run = p.CreateRun();
            run.SetText(content);
            using var ms = new MemoryStream();

            using var fs1 = new FileStream(fileName + ".docx", FileMode.Create);

            doc.Write(fs1);
            doc.Close();
            using var fs = new FileStream(fileName + ".docx", FileMode.Open);

            fs.Position = 0;
            var size = fs.Length;
            fs.CopyTo(ms);

            ms.Position = 0;
            var bytes = ms.ToArray();
            return (size, bytes);
        }
    }
}
