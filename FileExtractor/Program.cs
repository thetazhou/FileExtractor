using System;
using System.Configuration;

namespace FileExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            string dbType = ConfigurationManager.AppSettings["dbType"].ToString().ToLower();
            IGetFile getFile = null;

            SectionReader sction = new SectionReader();

            switch (dbType)
            {
                case "sqlserver":
                    getFile = new GetFileFromSqlServer();
                    getFile.Run();
                    break;
                case "oracle":
                    getFile = new GetFileFromOracle();
                    getFile.Run();
                    break;
            }
            Console.ReadKey();
        }
    }
}
