using System;
using System.Data.OracleClient;
using System.IO;

namespace FileExtractor
{
    class GetFileFromOracle : IGetFile
    {
        public bool Run()
        {
            bool bRet = false;
            DBProperty _dbProperty = SectionReader.GetDbProperty();
            string strConn = _dbProperty.conn;

            string strSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OracleFiles");

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
            using (OracleConnection conn = new OracleConnection(strConn))
            {
                conn.Open();

                OracleCommand cmd = new OracleCommand(strSQL, conn);
                OracleDataReader reader = cmd.ExecuteReader();
              
                Console.WriteLine(">>>>>>>>>> 开始提取 <<<<<<<<<<");

                int nIndex = 0;
                while (reader.Read())
                {
                    try
                    {
                        nID0 = Convert.ToInt32(reader[ID]);
                        strFileName = reader[fileName].ToString();

                        Byte[] bData = null;
                        bData = new Byte[(reader.GetBytes(1, 0, null, 0, int.MaxValue))];
                        reader.GetBytes(1, 0, bData, 0, bData.Length);

                        string groupValue = reader[fileGroup].ToString();
                        string strCurrPath = Path.Combine(strSavePath, groupValue);
                        if (!Directory.Exists(strCurrPath))
                        {
                            Directory.CreateDirectory(strCurrPath);
                        }

                        using (FileStream fs = new FileStream(string.Format("{0}\\{1}", strCurrPath, strFileName),
                            FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            fs.Write(bData, 0, bData.Length);
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

              
            }
            return bRet;
        }

        private void SaveTempFile(Byte[] blob, string filePath)
        {
            FileStream fs = null;
            //如果存在临时文件 先删除
            if (System.IO.File.Exists(filePath))  //打开文件
            {
                System.IO.File.Delete(filePath);
            }
            fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            fs.Write(blob, 0, blob.Length);
            fs.Close();
        }

        private Byte[] GetblobByFilePath(string FilePath)
        {
            System.IO.FileStream fs = new System.IO.FileStream(FilePath, FileMode.Open, FileAccess.Read);
            Byte[] blob = new Byte[fs.Length];
            fs.Read(blob, 0, blob.Length);
            fs.Close();
            return blob;
        }
    }
}
