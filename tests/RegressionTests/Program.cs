using ExcelToResxConverter.Model;
using ExcelToResxConverter.Service;
using System.Xml.Linq;
var unit = new ResourceUnit("hello", "안녕", "Hello <&>");
Check.That(unit.ToKoreanResx().Element("value")!.Value == "안녕", "Korean value");
Check.That(unit.ToEnglishResx().Element("value")!.Value == "Hello <&>", "XML escaping");
Check.That(new ResourceUnit("empty", null!, "").ToKoreanResx().Element("value")!.Value == "", "Blank spreadsheet cell");
Check.That(new ResxBuilder().BuildEnglish() is null, "Missing template");
var directory = Path.Combine(Path.GetTempPath(), "ResxRegression", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
try {
    var path = Path.Combine(directory, "template.xml");
    File.WriteAllText(path, "<root><header /></root>");
    var builder = new ResxBuilder { TemplatePath = path };
    Check.That(builder.BuildEnglish().Elements("data").Count() == 0, "Empty resources");
    builder.Units.Add(unit);
    Check.That(builder.BuildKorean().Elements("data").Count() == 1, "Korean export");
    Check.That(builder.BuildEnglish().Elements("data").Count() == 1, "Independent builds");
    File.WriteAllText(path, "not xml");
    await Check.ThrowsAsync<System.Xml.XmlException>(() => Task.Run(() => builder.BuildEnglish()), "Invalid template");
} finally { Directory.Delete(directory, true); }
Console.WriteLine($"PASS {Check.Count} resource regression checks");
