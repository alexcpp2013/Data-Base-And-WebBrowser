using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DBQ
{
    class ReadXMLData
    {
        string fileheader = "/RequestsForRegistration";
        string file = "templateUrl";

        public void GetParameters(string file, ref string url)
        {
            try
            {
                XmlDocument rdr = new XmlDocument();
                rdr.LoadXml(@file); // Загрузка XML

                XmlNode xmlData = rdr.SelectSingleNode(fileheader);

                url = xmlData[this.file].InnerText;
            }
            catch (Exception exp)
            {
                throw;
            }
        }
    }
}
