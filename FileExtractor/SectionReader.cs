using System.Collections.Specialized;
using System.Configuration;

namespace FileExtractor
{
    class SectionReader
    {
      public static DBProperty GetDbProperty()
        {
            string dbType = ConfigurationManager.AppSettings["dbType"].ToString();

            DBProperty _dbProperty = new DBProperty();

            NameValueCollection settings = ConfigurationManager.GetSection(dbType + "/appSettings") as NameValueCollection;
            if (settings != null)
            {
                foreach (string key in settings)
                {
                    switch(key)
                    {
                        case "conn":
                            _dbProperty.conn = settings[key];
                            break;
                        case "ExtractSQL":
                            _dbProperty.ExtractSQL = settings[key];
                            break;
                        case "ID":
                            _dbProperty.ID = settings[key];
                            break;
                        case "Name":
                            _dbProperty.Name = settings[key];
                            break;
                        case "File":
                            _dbProperty.File = settings[key];
                            break;
                        case "Group":
                            _dbProperty.Group = settings[key];
                            break;
                        case "Ext":
                            _dbProperty.Ext = settings[key];
                            break;
                    }
                }
            }

            return _dbProperty;
        } 
    }
}
