using System;
using System.Data.SqlClient;
using System.IO;

namespace FileExtractor
{
    class GetFileFromSqlServer: IGetFile
    {
        public bool Run()
        {
            bool bRet = false;

            DBProperty _dbProperty = SectionReader.GetDbProperty();
            string strConn = _dbProperty.conn;
            string strSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SqlServerFiles");

            string strSQL = _dbProperty.ExtractSQL;

            string ID = _dbProperty.ID;
            string fileName = _dbProperty.Name;
            string fileContent = _dbProperty.File;
            string fileGroup = _dbProperty.Group;

            if (!Directory.Exists(strSavePath))
            {
                Directory.CreateDirectory(strSavePath);
            }

            int nID0 = 0;
            string strFileName = string.Empty;
            byte[] bData = new byte[0];
            using (SqlConnection conn = new SqlConnection(strConn))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand(strSQL, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Console.WriteLine(">>>>>>>>>> 开始提取 <<<<<<<<<<");
                int nIndex = 0;
                while (reader.Read())
                {
                    try
                    {
                        nID0 = (int)reader[ID];
                        strFileName = reader[fileName].ToString();
                        bData = (byte[])reader[fileContent];
                        int nSize = bData.GetUpperBound(0);

                        string groupValue = reader[fileGroup].ToString();
                        string strCurrPath = Path.Combine(strSavePath, groupValue);
                        if (!Directory.Exists(strCurrPath))
                        {
                            Directory.CreateDirectory(strCurrPath);
                        }

                        using (FileStream fs = new FileStream(string.Format("{0}\\{1}", strCurrPath, strFileName),
                            FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fs.Write(bData, 0, nSize);
                            Console.WriteLine("{0} --> ID0 = {1}: {2} 提取成功", ++nIndex, nID0, strFileName);
                        }
                        bRet = true;
                    }
                    catch (Exception ex)
                    {
                        bRet = false;
                        Console.WriteLine(ex.ToString());
                        Console.WriteLine("跳过错误, 按回车继续...");
                        Console.ReadKey();
                    }
                }
                Console.WriteLine("<<<<<<<<<< 提取完成 >>>>>>>>>>");

                return bRet;
            }
        }
    }
}
