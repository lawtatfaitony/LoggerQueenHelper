using System; 
using System.Collections.Generic; 
using System.Drawing; 
using System.IO;
using System.Linq;
using System.Net;
using System.Text;   
using System.Configuration;

using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public partial class CommonBase
    { 

        public enum LoggerMode
        {
            DEBUG=-1,
            INFO = 0,
            WARNNING = 1,
            ERROR = 2,
            FATAL = 3
        }
        public static bool SaveImage(string Path1, string FileName, string PhotoBase64)
        {
            string PathFileName = Path.Combine(Path1, FileName);
            byte[] bt = Convert.FromBase64String(PhotoBase64);

            if (System.IO.Directory.Exists(Path1) == false)
            {
                System.IO.Directory.CreateDirectory(Path1);
            }
            try
            {
                File.WriteAllBytes(PathFileName, bt);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static string ReadConfigJson(string  PathFileName)
        {
            if(File.Exists(PathFileName))
            {
                string content = File.ReadAllText(PathFileName, Encoding.UTF8);
                return content;
            }else
            {
                return string.Empty;
            }
           
        }
        public static bool SaveDataJson(string FileContent,string PathFile)
        {
            StreamWriter writer = null;
            try
            { 
                writer = new StreamWriter(PathFile, false, System.Text.Encoding.GetEncoding("UTF-8"));
                writer.Write(FileContent);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return true;
        }
        public static bool SaveDataJson(string fileContent, string path, string targetFileName)
        {
            StreamWriter writer = null;
            try
            {
                if (System.IO.Directory.Exists(path) == false)
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                string PathFile = Path.Combine(path, targetFileName);
                writer = new StreamWriter(PathFile, false, System.Text.Encoding.GetEncoding("UTF-8"));
                writer.Write(fileContent);
            }
            catch
            {
                return false;
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

            return true;
        }

        public static string ReadDataFromJson( string path, string targetFileName)
        {
            string PathFileName = Path.Combine(path, targetFileName);
            if (File.Exists(PathFileName))
            {
                string content = File.ReadAllText(PathFileName, Encoding.UTF8);
                return content;
            }
            else
            {
                return string.Empty;
            }  
        }
        public static string ReadDataFromJson(string PathFileName)
        { 
            if (File.Exists(PathFileName))
            {
                string content = File.ReadAllText(PathFileName, Encoding.UTF8);
                return content;
            }
            else
            {
                return string.Empty;
            }
        }
        public static bool CopyToFolder(string TargetPath, string TargetFileName, string SourcePathFileName)
        {
            string TargetPathFileName = Path.Combine(TargetPath, TargetFileName);
             
            if (!System.IO.Directory.Exists(TargetPath) )
            {
                System.IO.Directory.CreateDirectory(TargetPath);
            }
            try
            {
                File.Copy(SourcePathFileName, TargetPathFileName, true);
                return true;
            }
            catch
            {
                return false;
            } 
        }

        #region 
        public static bool WriteLogNtimes(string LineContent, LoggerMode loggerMode)
        {
            string dateTimeLineLog = string.Format("[{0:yyyy-MM-dd HH:mm:ss fff}] {1}", DateTime.Now, LineContent);
            Logger logger = new Logger { LoggerLine = dateTimeLineLog, loggerMode = loggerMode }; 
            LoggerQueueHelper.instance.AddData(logger); 
            bool AnalysisStatus = LoggerQueueHelper.instance.AnalysisStatus; 
            if (!AnalysisStatus)
            { 
                if (LoggerQueueHelper.instance.Getcount() > 0)
                { 
                    LoggerQueueHelper.instance.PostAnalysis();
                }
            }
            return true;
        }
        public static bool OperateLoger(string LineContent)
        {
            bool result = WriteLogNtimes(LineContent, LoggerMode.INFO); 
            return result;
        } 
        public static void OperateDateLoger(string LoggerLineContent, object objData)
        {
            string ObjectData = JsonCompress(JsonConvert.SerializeObject(objData));
            string ItemContent = string.Format("[{0}] [DATA]:[{1}]", LoggerLineContent, ObjectData);
            OperateDateLoger(ItemContent);
            return ;
        }
        public static void OperateDateLoger(string LoggerLineContent, object objData,LoggerMode loggerMode)
        {
            string ObjectData = JsonCompress(JsonConvert.SerializeObject(objData));
            string ItemContent = string.Format("[{0}] [{1}]  [OBJECT DATA]:[{2}]", loggerMode.ToString(), LoggerLineContent, ObjectData);
            WriteLogNtimes(ItemContent, loggerMode); 
            return;
        }
        #endregion

        #region  Date Loger
        public static bool OperateDateLoger(string LineContent)
        {
            bool result = WriteLogNtimes(LineContent, LoggerMode.INFO);  
            return result;
        }
        public static bool OperateDateLoger(string LineContent,LoggerMode loggerMode)
        {
            bool result = WriteLogNtimes(LineContent, loggerMode); 
            return result;
        }
        public static bool EntryImageLoger(string LineEntryImage)
        {
            bool result = WriteLogNtimes(LineEntryImage, LoggerMode.INFO);
            return result;
        }
        #endregion
        public static string GetSystemDriveLogPath()
        {
            string path;
            string Systemdrive = Environment.GetEnvironmentVariable("systemdrive");
            Systemdrive = string.Format("{0}\\", Systemdrive);
            path = Path.Combine(Systemdrive, "log");
             
            if (System.IO.Directory.Exists(path) == false)
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }

        public static bool IsConsoleOrWinformPath()
        {
            string EnvPath = System.Environment.CurrentDirectory.TrimEnd('\\');
            string BaseDirPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'); 
            return EnvPath == BaseDirPath;
        }
    }
    

}
