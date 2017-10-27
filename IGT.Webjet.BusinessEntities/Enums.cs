using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IGT.Webjet.BusinessEntities
{
    public enum NLogLevel
    {
        Debug,
        Error,
        Fatal,
        Info,
        Off,
        Trace,
        Warn
    }
    public enum PNRProcessingStatus
    {
        Recorded,
        InProgress,
        Completed
    }
    public enum TypeOfFare
    {
        Unspecified,
        Private,
        Published
    }

    public enum ProcessResult
    {
        CarrierNotFound,
        EmailOrPhoneNotFound,
        RemovedFromQ
    }
}
