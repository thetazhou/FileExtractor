using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;

namespace FileExtractor
{
    class GetFileFromOracle : IGetFile
    {
        public bool Run()
        {
            bool bRet = false;

            Thread _thread = null;

            string strErrFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            string strInfoFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "info.log");

            DBProperty _dbProperty = SectionReader.GetDbProperty();
            string strConn = _dbProperty.conn;

            string strSavePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OracleFiles");

            string ID = _dbProperty.ID;
            string fileName = _dbProperty.Name;
            string fileContent = _dbProperty.File;
            string fileGroup = _dbProperty.Group;
            string strExt = _dbProperty.Ext;

            string strSQLAllID = Regex.Replace(_dbProperty.ExtractSQL.ToLower(), @"select [\s\S]*? from", "select " + ID + " from");
            string strSQLOneRecordBase = Regex.Replace(_dbProperty.ExtractSQL.ToLower(), fileContent + @"\s*,", "");
            strSQLOneRecordBase = Regex.Replace(strSQLOneRecordBase, "select ", "select " + fileContent + ",");

            Dictionary<string, string> mapFileExt = FileType.getFileType();

            if (!Directory.Exists(strSavePath))
            {
                Directory.CreateDirectory(strSavePath);
            }

            int nID0 = 0;
            string strFileName = string.Empty;
            using (OracleConnection conn = new OracleConnection(strConn))
            {
                conn.Open();

                OracleCommand cmdID = new OracleCommand(strSQLAllID, conn);
                OracleDataReader readerID = cmdID.ExecuteReader();
                List<string> listID = new List<string>();
                while(readerID.Read())
                {
                    listID.Add(readerID[0].ToString());
                }
                readerID.Close();
              
                Console.WriteLine(">>>>>>>>>> 开始提取 <<<<<<<<<<");

                int nIndex = 0;
                string strSQLOneRecord = string.Empty;
                foreach(string id in listID)
                {
                    try
                    {
                        string strOrderByPre = string.Empty;
                        string strOrderByAfter = string.Empty;
                         
                        //ORDER BY 语句处理
                        if(Regex.IsMatch(strSQLOneRecordBase, @"order[\s]+by"))
                        {
                            Match _matchOrderBy = Regex.Match(strSQLOneRecordBase, @"([\s\S]*?)order[\s]+by([\s\S]*?)");
                            strOrderByPre = _matchOrderBy.Groups[1].Value;
                            strOrderByAfter = _matchOrderBy.Groups[2].Value;
                        }
                        else
                        {
                            strOrderByPre = strSQLOneRecordBase;
                        }

                        //WHERE 语句处理
                        if(strSQLOneRecordBase.Contains("where"))
                        {
                            strSQLOneRecord = strOrderByPre + " and " + ID + " = '" + id + "' " + strOrderByAfter;
                        }
                        else
                        {
                            strSQLOneRecord = strOrderByPre + " where " + ID + " = '" + id + "' " + strOrderByAfter;
                        }

                        OracleCommand cmdFile = new OracleCommand(strSQLOneRecord, conn);
                        OracleDataReader reader = cmdFile.ExecuteReader();
                        reader.Read();

                        nID0 = Convert.ToInt32(reader[ID]);
                        strFileName = reader[fileName].ToString();

                        Byte[] bData = null;
                        bData = new Byte[(reader.GetBytes(0, 0, null, 0, int.MaxValue))];
                        reader.GetBytes(0, 0, bData, 0, bData.Length);

                        string groupValue = reader[fileGroup].ToString();
                        reader.Close();

                        string strCurrPath = Path.Combine(strSavePath, groupValue);
                        if (!Directory.Exists(strCurrPath))
                        {
                            Directory.CreateDirectory(strCurrPath);
                        }
                        
                        if(!strFileName.Contains("."))
                        {
                            strFileName += strExt;
                        }

                        _thread = new Thread(new ThreadStart(delegate
                        {
                            using (FileStream fs = new FileStream(string.Format("{0}\\{1}", strCurrPath, strFileName),
                          FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                fs.Write(bData, 0, bData.Length);

                                string strMessage = string.Format("{0} --> ID = {1}, GroupID = {2}: {3} 提取成功. [size:{4}K]", 
                                    ++nIndex, nID0, groupValue, strFileName, bData.Length / 1024);

                                Console.WriteLine(strMessage);
                                Loger.Debug(strInfoFile, strMessage);
                            }
                        }));
                        _thread.IsBackground = true;
                        _thread.Start();
                      
                        bRet = true;
                    }
                    catch (Exception ex)
                    {
                        bRet = false;
                        Console.WriteLine("ID = {0} 记录出错, 已记录到error.log文件.");
                        Loger.Debug(strErrFile, "ERROR ID = "+ id +". >>>>>>> "+ ex.ToString());
                    }
                }
                _thread.Join();
                Console.WriteLine("<<<<<<<<<< 提取完成 >>>>>>>>>>");
                Console.ReadKey();
            }
            return bRet;
        }
    }
}
