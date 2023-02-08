using System.Xml.Linq;

namespace ExcelToResxConverter.Model
{
    public class ResourceUnit
    {
        #region Properties
        public string Keyword { get; }
        public string Korean { get; }
        public string English { get; }
        #endregion

        #region Constructor
        public ResourceUnit(string keyword, string korean, string english)
        {
            Keyword = keyword;
            Korean = korean;
            English = english;
        }
        #endregion

        #region Functions
        public XElement ToKoreanResx() => ToResx(Keyword, Korean);

        public XElement ToEnglishResx() => ToResx(Keyword, English);

        private static XElement ToResx(string keyword, string value)
        {
            var element = new XElement("data");

            element.Add(new XAttribute("name", keyword));
            element.Add(ToValueElement(value));

            return element;
        }

        private static XElement ToValueElement(string value)
        {
            var element = new XElement("value");
            element.Value = value;

            return element;
        }
        #endregion
    }
}
