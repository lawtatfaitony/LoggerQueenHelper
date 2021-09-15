using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static Common.CommonBase;

namespace Common
{
    public class Logger
    {
        public LoggerMode loggerMode { get; set; } = LoggerMode.INFO;
        public string LoggerLine { get; set; } = string.Empty;
    }
    public class LoggerQueueHelper
    {
        private LoggerQueueHelper()
        { 
        } 
        private static string _LogPath;
        public static string LogPath
        {
            get
            {
                if (MemoryCacheHelper.Contains("LogPath") == false)
                {
                    DateTimeOffset thisOffsetTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,23,59,59);
                    string logPath = Path.GetFullPath("./"); 
                    MemoryCacheHelper.Set("LogPath", logPath, thisOffsetTime);
                    string path1 = logPath;
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(path1);
                    }
                    _LogPath = logPath;
                }
                else
                {
                    _LogPath = MemoryCacheHelper.GetCacheItem<string>("LogPath");
                }
                return _LogPath;
            }
        }
        #region  
        private static object lockObj1 = new object();
        private static void Log(Logger logger)
        {
            lock (lockObj1)
            {
                string filePath = Path.Combine(LogPath, "log");
                string filename = CheckLogFile(logger.loggerMode,500); 
                string pathFilename = Path.Combine(filePath, filename);
                 
                ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim(); //ref:https://www.cnblogs.com/tianma3798/p/8252553.html
                try
                {
                    readerWriterLockSlim.EnterWriteLock();
                    using (StreamWriter w = new StreamWriter(pathFilename, true, Encoding.UTF8))
                    {
                        w.WriteLine(logger.LoggerLine);
                        w.Close();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();   //
                }
                
            }
        }
        #endregion
        private static object lockObj2 = new object();

        private static void ErrorLog(string msg)
        {
            lock (lockObj2)
            {
                string filePath = Path.Combine(LogPath, "log");  
                string filename = CheckLogFile(LoggerMode.ERROR,500);
                string errPathfileName = Path.Combine(filePath, filename);
                if (!File.Exists(errPathfileName))
                {
                    FileStream fs = File.OpenWrite(errPathfileName);
                    fs.Flush();
                    fs.Close();
                }

                ReaderWriterLockSlim readerWriterLockSlim = new ReaderWriterLockSlim(); //ref:https://www.cnblogs.com/tianma3798/p/8252553.html
                try
                { 
                    readerWriterLockSlim.EnterWriteLock();
                    using (StreamWriter w = new StreamWriter(errPathfileName, true, Encoding.UTF8))
                    {
                        w.WriteLine(msg); 
                        w.Close();
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    readerWriterLockSlim.ExitWriteLock();   //
                }
                
            }
        }

        public bool AnalysisStatus = false;

        public static LoggerQueueHelper instance = new LoggerQueueHelper();
        
        private ConcurrentQueue<Logger> t = new ConcurrentQueue<Logger>();
        /// <summary>
        /// add to queue
        /// </summary>
        /// <param name="tip"></param>
        public int AddData(Logger tip)
        {
            t.Enqueue(tip);
            return t.Count;
        }

        /// <summary>
        /// get the the first queue
        /// </summary>
        /// <returns></returns>
        public Logger Get()
        {
            Logger logger = new Logger();
            return t.TryPeek(out logger) ? logger : null;
        }

        /// <summary>
        ///get from queue and calc
        /// </summary>
        public void PostAnalysis()
        {
            while (t.Count > 0)
            {
                try
                {
                    AnalysisStatus = true;

                    Logger outresult = new Logger();

                    if (t.TryDequeue(out outresult))
                    {
                        Log(outresult);
                    }
                    else
                    {
                        ErrorLog(string.Format("[{0:yyyy-MM-dd HH:mm:ss}][ERROR (TryDequeue error)]", DateTime.Now));
                    }
                }
                catch (Exception ex)
                {
                    AnalysisStatus = false;
                    ErrorLog(string.Format("[{0:yyyy-MM-dd HH:mm:ss}][FUNC::LoggerQueueHelper.PostAnalysis][EXCEPTION] [{1}]", DateTime.Now, ex.Message));
                    throw;
                }
            }
            AnalysisStatus = false;
        }

        /// <summary>
        /// get the queue total count
        /// </summary>
        /// <returns></returns>
        public int Getcount()
        {
            return t.Count;
        }

        /// <summary>
        /// 检查log文件
        /// </summary>
        /// <param name="loggerMode"></param>
        /// <param name="fileMaxSize">建议500 单位Kb</param>
        /// <returns></returns>
        public static string CheckLogFile(LoggerMode loggerMode,long fileMaxSize)
        {
            int fileIndex = 0;
            fileMaxSize = fileMaxSize * 1000; 

            string filePath = Path.Combine(LogPath, "log");
            string logExtend = ".log";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            string name = string.Format("Logger{0}{1:yyyyMMdd}", LoggerMode.INFO, DateTime.Now);
            switch (loggerMode)
            {
                case LoggerMode.INFO:
                    name = string.Format("Logger{0}{1:yyyyMMdd}", LoggerMode.INFO, DateTime.Now); 
                    break;
                case LoggerMode.DEBUG:
                    name = string.Format("Logger{0}{1:yyyyMMdd}", LoggerMode.DEBUG, DateTime.Now); 
                    break;
                case LoggerMode.ERROR:
                    name = string.Format("Logger{0}{1:yyyyMMdd}", LoggerMode.ERROR, DateTime.Now); 
                    break;
                case LoggerMode.FATAL:
                    name = string.Format("Logger{0}{1:yyyyMMdd}", LoggerMode.FATAL, DateTime.Now); 
                    break;
                case LoggerMode.WARNNING:
                    name = string.Format("Logger{0}{1:yyyyMMdd}", LoggerMode.WARNNING, DateTime.Now); 
                    break;
                default:
                    break;
            }
            string filename = string.Format("{0}_{1}{2}", name, fileIndex, logExtend);
            string pathFileName = Path.Combine(filePath, filename);

            if (!File.Exists(pathFileName))
            {
                FileStream fs = File.OpenWrite(pathFileName);
                fs.Flush();
                fs.Close();
                return filename;
            }
            else
            {
                //处理文件过大问题,超过200K,则使用序数文件 >2000000 (>200K) 
                while(true){
                     
                    FileInfo fileInfo = new FileInfo(pathFileName); 
                    if(fileInfo.Length < fileMaxSize)
                    {
                        return fileInfo.Name; 
                    }
                    else
                    {
                        fileIndex++;

                        filename = string.Format("{0}_{1}{2}", name, fileIndex, logExtend);
                        pathFileName = Path.Combine(filePath, filename);

                        if (!File.Exists(pathFileName))
                        {
                            FileStream fs = File.OpenWrite(pathFileName);
                            fs.Flush();
                            fs.Close();

                            return filename;
                        } 
                    }
                    

                    if (fileIndex > 99)  //The max files 99
                        break;
                }
            } 
            return filename;
        }
    }
}