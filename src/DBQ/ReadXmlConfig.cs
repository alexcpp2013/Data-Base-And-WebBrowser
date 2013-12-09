using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DBQ
{
    class ReadXmlConfig
    {
        string fileheader = "/config";
        string server = "server";
        string bd = "bd";
        string table = "table";
        string user = "user";
        string password = "password";

        public void GetParameters(string file, 
                           ref string server,
                           ref string bd,
                           ref string table,
                           ref string user,
                           ref string password)
        {
            try
            {
                XmlDocument rdr = new XmlDocument();
                rdr.Load(@file); // Загрузка XML

                XmlNode xmlData = rdr.SelectSingleNode(fileheader);

                server = xmlData[this.server].InnerText;
                bd = xmlData[this.bd].InnerText;
                table = xmlData[this.table].InnerText;
                user = xmlData[this.user].InnerText;
                password = xmlData[this.password].InnerText;
            }
            catch (Exception exp)
            {
                throw;
            }
        }
    }
}
