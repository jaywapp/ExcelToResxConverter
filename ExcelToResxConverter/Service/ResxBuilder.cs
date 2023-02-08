using ExcelToResxConverter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace ExcelToResxConverter.Service
{
    public class ResxBuilder
    {
        #region Properties
        public string TemplatePath { get; set; }
        public List<ResourceUnit> Units { get; } = new List<ResourceUnit>();
        #endregion

        #region Functions
        private XElement Build(Func<ResourceUnit, XElement> selector)
        {
            if (string.IsNullOrEmpty(TemplatePath) || !File.Exists(TemplatePath))
                return null;

            var element = XDocument.Load(TemplatePath).Root;
            foreach (var unit in Units)
                element.Add(selector(unit));

            return element;
        }

        public XElement BuildKorean() => Build(u => u.ToKoreanResx());

        public XElement BuildEnglish() => Build(u => u.ToEnglishResx());
        #endregion
    }
}
