using ExcelDataReader;
using ExcelToResxConverter.Model;
using System.Collections.Generic;
using System.IO;

namespace ExcelToResxConverter.Service
{
    public static class ExcelReader
    {
        public static IEnumerable<ResourceUnit> Read(string path)
        {
            var result = new List<ResourceUnit>();

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            var keyword = reader.GetString(0);
                            var korean = reader.GetString(1);
                            var english = reader.GetString(2);

                            result.Add(new ResourceUnit(keyword, korean, english));
                        }
                    } while (reader.NextResult());
                }
            }

            return result;
        }
    }
}
