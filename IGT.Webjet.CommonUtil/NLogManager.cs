using IGT.Webjet.BusinessEntities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGT.Webjet.CommonUtil
{
    public class NLogManager
    {
        public static readonly NLogManager _instance = new NLogManager();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public NLogManager()
        {
        }

        public void LogMsg(NLogLevel _pLogLevel, string _pMsg)
        {
            LogLevel LoggingLevel = LogLevel.Debug;

            switch (_pLogLevel)
            {
                case NLogLevel.Debug:
                    LoggingLevel = LogLevel.Debug;
                    break;
                case NLogLevel.Error:
                    LoggingLevel = LogLevel.Error;
                    break;
                case NLogLevel.Fatal:
                    LoggingLevel = LogLevel.Fatal;
                    break;
                case NLogLevel.Info:
                    LoggingLevel = LogLevel.Info;
                    break;
                case NLogLevel.Off:
                    LoggingLevel = LogLevel.Off;
                    break;
                case NLogLevel.Trace:
                    LoggingLevel = LogLevel.Trace;
                    break;
                case NLogLevel.Warn:
                    LoggingLevel = LogLevel.Warn;
                    break;
            }

            logger.Log(LoggingLevel, _pMsg);
        }
    }
}
