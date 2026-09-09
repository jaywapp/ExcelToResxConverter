using ExcelToResxConverter;
using ExcelToResxConverter.Service;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

internal static class Program
{
    private static int count;

    [STAThread]
    private static int Main()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ExcelToResxIntegration", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            TestExcel(directory);
            var application = new Application();
            var window = new MainWindow();
            That(window.Content != null && window.DataContext is MainWindowViewModel, "WPF window and bindings initialize");
            var content = (FrameworkElement)window.Content;
            content.Measure(new Size(800, 600));
            content.Arrange(new Rect(0, 0, 800, 600));
            That(content.ActualWidth > 0, "WPF content layout executes");
            window.Close();
            application.Shutdown();
            Console.WriteLine("PASS " + count + " original assembly integration checks");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static void TestExcel(string directory)
    {
        var path = Path.Combine(directory, "input.xlsx");
        WriteWorkbook(path, "<row r='1'>" + Cell("A1", "hello") + Cell("B1", "안녕") + Cell("C1", "Hello <&>") + "</row>",
            "<row r='1'>" + Cell("A1", "empty") + "<c r='B1'/><c r='C1'/></row>");
        var units = ExcelReader.Read(path).ToList();
        That(units.Count == 2, "Multiple worksheets are read");
        That(units[0].ToKoreanResx().Element("value").Value == "안녕", "Unicode spreadsheet value");
        That(units[0].ToEnglishResx().Element("value").Value == "Hello <&>", "Spreadsheet XML characters");
        That(units[1].ToKoreanResx().Element("value").Value == "", "Blank Korean cell");
        That(units[1].ToEnglishResx().Element("value").Value == "", "Blank English cell");
        using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            count++;
            Throws<IOException>(() => ExcelReader.Read(path).ToList(), "Locked input fails");
        }

        var template = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "template.xml");
        That(File.Exists(template), "Production template copied");
        var builder = new ResxBuilder { TemplatePath = template };
        builder.Units.AddRange(units);
        var koreanPath = Path.Combine(directory, MainWindowViewModel.KOREAN_FILE_NAME);
        var englishPath = Path.Combine(directory, MainWindowViewModel.ENGLISH_FILE_NAME);
        builder.BuildKorean().Save(koreanPath);
        builder.BuildEnglish().Save(englishPath);
        That(XDocument.Load(koreanPath).Root.Elements("data").Count() == 2, "Korean file persisted");
        That(XDocument.Load(englishPath).Root.Elements("data").First().Element("value").Value == "Hello <&>", "English output round trip");
        using (File.Open(englishPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            Throws<IOException>(() => builder.BuildEnglish().Save(englishPath), "Locked output fails");
        File.SetAttributes(englishPath, FileAttributes.ReadOnly);
        try
        {
            Throws<UnauthorizedAccessException>(() => builder.BuildEnglish().Save(englishPath), "Read-only output denies writes");
        }
        finally
        {
            File.SetAttributes(englishPath, FileAttributes.Normal);
        }
        Throws<DirectoryNotFoundException>(() => builder.BuildEnglish().Save(Path.Combine(directory, "missing", "out.resx")), "Missing output directory fails");
        Throws<FileNotFoundException>(() => ExcelReader.Read(Path.Combine(directory, "missing.xlsx")).ToList(), "Missing input fails");

        var broken = Path.Combine(directory, "broken.xlsx");
        File.WriteAllText(broken, "not a workbook");
        Throws<Exception>(() => ExcelReader.Read(broken).ToList(), "Invalid workbook fails", "ExcelDataReader.Exceptions.HeaderException");
        using (File.Open(broken, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            count++;

        var numeric = Path.Combine(directory, "numeric.xlsx");
        WriteWorkbook(numeric, "<row r='1'><c r='A1'><v>42</v></c>" + Cell("B1", "value") + Cell("C1", "value") + "</row>");
        Throws<InvalidCastException>(() => ExcelReader.Read(numeric).ToList(), "Numeric key preserves rejection contract");
        using (File.Open(numeric, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            count++;

        var empty = Path.Combine(directory, "empty.xlsx");
        WriteWorkbook(empty, "");
        That(!ExcelReader.Read(empty).Any(), "Empty worksheet");
    }

    private static string Cell(string address, string value)
    {
        return "<c r='" + address + "' t='inlineStr'><is><t>" + System.Security.SecurityElement.Escape(value) + "</t></is></c>";
    }

    private static void WriteWorkbook(string path, params string[] sheets)
    {
        using (var stream = File.Create(path))
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create))
        {
            Add(zip, "[Content_Types].xml", "<Types xmlns='http://schemas.openxmlformats.org/package/2006/content-types'><Default Extension='rels' ContentType='application/vnd.openxmlformats-package.relationships+xml'/><Default Extension='xml' ContentType='application/xml'/><Override PartName='/xl/workbook.xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml'/>" + string.Concat(sheets.Select((s, i) => "<Override PartName='/xl/worksheets/sheet" + (i + 1) + ".xml' ContentType='application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml'/>")) + "</Types>");
            Add(zip, "_rels/.rels", "<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'><Relationship Id='rId1' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument' Target='xl/workbook.xml'/></Relationships>");
            Add(zip, "xl/workbook.xml", "<workbook xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><sheets>" + string.Concat(sheets.Select((s, i) => "<sheet name='Sheet" + (i + 1) + "' sheetId='" + (i + 1) + "' r:id='rId" + (i + 1) + "'/>")) + "</sheets></workbook>");
            Add(zip, "xl/_rels/workbook.xml.rels", "<Relationships xmlns='http://schemas.openxmlformats.org/package/2006/relationships'>" + string.Concat(sheets.Select((s, i) => "<Relationship Id='rId" + (i + 1) + "' Type='http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet' Target='worksheets/sheet" + (i + 1) + ".xml'/>")) + "</Relationships>");
            for (var index = 0; index < sheets.Length; index++)
                Add(zip, "xl/worksheets/sheet" + (index + 1) + ".xml", "<worksheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main'><sheetData>" + sheets[index] + "</sheetData></worksheet>");
        }
    }

    private static void Add(ZipArchive zip, string name, string content)
    {
        using (var writer = new StreamWriter(zip.CreateEntry(name).Open()))
            writer.Write(content);
    }

    private static void That(bool value, string name)
    {
        if (!value) throw new Exception(name);
        count++;
    }

    private static void Throws<T>(Action action, string name, string exactType = null) where T : Exception
    {
        try { action(); }
        catch (T exception)
        {
            if (exactType != null && exception.GetType().FullName != exactType) throw;
            count++;
            return;
        }
        throw new Exception(name);
    }
}
