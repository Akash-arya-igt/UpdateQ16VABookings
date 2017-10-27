using IGT.Webjet.BusinessEntities;
using IGT.Webjet.CommonUtil;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IGT.Webjet.GALEngine.GALAction
{
    public class PNRParseAction
    {
        private string[] _sPassiveSegmentStatuses = new[] { "BK", "AK" };

        public string GetReclocFromPNRXml(XmlElement PNRXml)
        {
            string strRecloc = string.Empty;

            if (PNRXml != null)
                strRecloc = PNRXml.GetStringChildNode("//GenPNRInfo/RecLoc");

            return strRecloc;
        }

        public List<string> GetGeneralRemarks(XmlElement pnr)
        {
            List<string> genRemarks = new List<string>();
            XmlNodeList nodes = pnr.SelectNodes("//GenRmkInfo");
            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    string remark = XMLUtil.GetStringChildNode(nodes[i], "GenRmk").Trim();
                    genRemarks.Add(remark);
                }
            }

            nodes = null;

            return genRemarks;
        }

        public List<FlightInfo> GetFlightDetails(XmlElement pnr)
        {
            List<FlightInfo> flights = new List<FlightInfo>();

            XmlNodeList airSegList = pnr.SelectNodes("//PNRBFRetrieve/AirSeg");

            int index = 0;
            string status = string.Empty;
            string airV = string.Empty;
            string startAirport = string.Empty;
            string departureDate = string.Empty;
            string startTm = string.Empty;
            string endAirport = string.Empty;
            string endTm = string.Empty;
            string fltNum = string.Empty;
            string bic = string.Empty;
            string fltFlownInd = string.Empty;
            string prevStatus = string.Empty;
            string segNum = string.Empty;
            string dayChg = string.Empty;
            DateTime arrivalDate;

            if (airSegList != null && airSegList.Count > 0)
            {
                foreach (XmlNode airSegNode in airSegList)
                {
                    status = XMLUtil.GetStringChildNode(airSegNode, "Status");
                    if (IsPassiveSegmentStatusCode(status))
                    {
                        //Debug.Assert(false, "TO DEBUG PassiveSegments");
                        continue;//skip PassiveSegment
                    }
                    airV = XMLUtil.GetStringChildNode(airSegNode, "AirV");
                    startAirport = XMLUtil.GetStringChildNode(airSegNode, "StartAirp");
                    departureDate = XMLUtil.GetStringChildNode(airSegNode, "Dt");
                    startTm = XMLUtil.GetStringChildNode(airSegNode, "StartTm");
                    endAirport = XMLUtil.GetStringChildNode(airSegNode, "EndAirp");
                    endTm = XMLUtil.GetStringChildNode(airSegNode, "EndTm");
                    fltNum = XMLUtil.GetStringChildNode(airSegNode, "FltNum");
                    bic = XMLUtil.GetStringChildNode(airSegNode, "BIC");
                    fltFlownInd = XMLUtil.GetStringChildNode(airSegNode, "FltFlownInd");
                    prevStatus = XMLUtil.GetStringChildNode(airSegNode, "PrevStatusCode");
                    segNum = XMLUtil.GetStringChildNode(airSegNode, "SegNum");
                    dayChg = XMLUtil.GetStringChildNode(airSegNode, "DayChg");
                    arrivalDate = DateTime.ParseExact(String.Format("{0} {1}", departureDate, GetCompleteTime(endTm)), "yyyyMMdd HHmm", CultureInfo.InvariantCulture);

                    if (dayChg == "-1")   //previous day arrival 
                    {
                        arrivalDate = arrivalDate.AddDays(-1);
                    }
                    else if (dayChg == "01")   //next day arrival 
                    {
                        arrivalDate = arrivalDate.AddDays(1);
                    }
                    else if (dayChg == "02")     //second day arrival
                    {
                        arrivalDate = arrivalDate.AddDays(2);
                    }
                    /* arrival date will be set as departure date if DayChg node is not present in PNR response or it contains any one of the following code 
                        * 03 - Rail vendor only
                        * 04 - Rail vendor only
                    */
                    FlightInfo newAirSeg = new FlightInfo
                    {
                        DepartureTime = DateTime.ParseExact(String.Format("{0} {1}", departureDate, GetCompleteTime(startTm)), "yyyyMMdd HHmm", CultureInfo.InvariantCulture),
                        ArrivalTime = arrivalDate,
                        Source = startAirport,
                        Destination = endAirport,
                        Status = status, 
                        CarrierCode = airV,
                        FlightNumber = fltNum,
                        Class = bic,
                        FlightFlownInd = (fltFlownInd == "Y") ? true : false,
                        PreviousStatus = prevStatus,
                        SegNum = segNum,
                        Index = index
                    };
                    flights.Add(newAirSeg);
                    index++;
                }
            }

            airSegList = null;

            return flights;
        }
        public bool IsPassiveSegmentStatusCode(string statusCode)
        {
            statusCode = statusCode.ToUpper();
            bool bRet = false;
            if (_sPassiveSegmentStatuses.Contains(statusCode))
                bRet = true;
            return bRet;
        }

        public string GetCompleteTime(string time)
        {
            string completeTime;

            switch (time.Length)
            {
                case 1:
                    completeTime = "000" + time;
                    break;
                case 2:
                    completeTime = "00" + time;
                    break;
                case 3:
                    completeTime = "0" + time;
                    break;
                default:
                    completeTime = time;
                    break;
            }

            return completeTime;
        }

        public string GetEmail(XmlElement pnrXML)
        {
            string strEmail = string.Empty;

            if(XMLUtil.GetStringChildNode(pnrXML, "//Email") != null && XMLUtil.GetStringChildNode(pnrXML, "//Email/Data") != null)
                strEmail = XMLUtil.GetStringChildNode(pnrXML, "//Email/Data");

            return strEmail;
        }

        public string GetPhoneNo(XmlElement pnrXML)
        {
            string strPhoneNo = string.Empty;
            var phoneNodeList = pnrXML.SelectNodes("//PhoneInfo");

            foreach(XmlNode phoneNode in phoneNodeList)
            {
                if(phoneNode != null && phoneNode.GetStringChildNode("Type")!=null 
                    && phoneNode.GetStringChildNode("Type") != "A"
                    && phoneNode.GetStringChildNode("Phone") != null)
                {
                    strPhoneNo = phoneNode.GetStringChildNode("Phone");
                    break;
                }
            }

            return strPhoneNo.Replace(" ", "").Replace("-", "");
        }

    }
}
