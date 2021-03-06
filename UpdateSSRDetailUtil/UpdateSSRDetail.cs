﻿using IGT.Webjet.BusinessEntities;
using IGT.Webjet.CommonUtil;
using IGT.Webjet.GALEngine.GALAction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace UpdateSSRDetailUtil
{
    public class UpdateSSRDetail
    {
        public string Session { get; set; }
        public GetHAPDetail HAPDetail { get; set; }
        public XmlElement PnrDoc { get; set; }
        public string CurrentRecloc { get; set; }

        public XmlElement StartProcessing(out string EndStatus)
        {
            XmlElement _pnrDoc = PnrDoc;
            FareAction objFareAction = new FareAction();
            QueueAction objQAction = new QueueAction();
            PNRParseAction objPNRParse = new PNRParseAction();

            // Read PNR

            var flghts = objPNRParse.GetFlightDetails(_pnrDoc);
            List<string> carrier = ConfigUtil.GetConfigValue("ValidCarrier", "VA").Split(',').Where(x => !string.IsNullOrEmpty(x)).ToList();

            if (flghts.Any(x => carrier.Contains(x.CarrierCode) && x.DepartureTime.Date > DateTime.Now))
            {
                // GET email and Phone no.
                string strEmail = string.Empty, strPhone = string.Empty;
                strEmail = objPNRParse.GetEmail(_pnrDoc);
                strPhone = objPNRParse.GetPhoneNo(_pnrDoc);
                int intPhNo = 0;

                if (int.TryParse(strPhone, out intPhNo))
                {
                    if (!string.IsNullOrEmpty(strEmail) && !string.IsNullOrEmpty(strPhone))
                    {
                        //Format email and phone no.
                        strEmail = strEmail.Replace("@", @"//").Replace("_", "..").Replace("-", "./");
                        strPhone = string.Concat("61", strPhone.StartsWith("0") ? strPhone.TrimStart('0') : strPhone);

                        //Update PNR detail
                        objFareAction.UpdatePhoneEmailInSSR(HAPDetail, strEmail, strPhone, Session);
                        _pnrDoc = objFareAction.EndTransactRetrieveNextPNR(HAPDetail, Session);
                        //_pnrDoc = objFareAction.Ignore(HAPDetail, Session);
                        EndStatus = ProcessResult.RemovedFromQ.ToString();

                        NLogManager._instance.LogMsg(NLogLevel.Info, CurrentRecloc + ": " + EndStatus);
                    }
                    else
                    {
                        _pnrDoc = objFareAction.Ignore(HAPDetail, Session);
                        EndStatus = ProcessResult.EmailOrPhoneNotFound.ToString();
                        NLogManager._instance.LogMsg(NLogLevel.Info, CurrentRecloc + ": " + EndStatus);
                    }
                }
                else
                {
                    _pnrDoc = objFareAction.Ignore(HAPDetail, Session);
                    EndStatus = ProcessResult.PhoneNoInCorrectFormat.ToString();
                    NLogManager._instance.LogMsg(NLogLevel.Info, CurrentRecloc + ": " + EndStatus);
                }
            }
            else
            {
                if (flghts.Any(x => carrier.Contains(x.CarrierCode)))
                {
                    _pnrDoc = objQAction.RemoveFromQ(HAPDetail, Session);
                    EndStatus = ProcessResult.NoActiveSegments.ToString();
                }
                else
                {
                    _pnrDoc = objFareAction.Ignore(HAPDetail, Session);
                    EndStatus = ProcessResult.CarrierNotFound.ToString();
                }
                
                NLogManager._instance.LogMsg(NLogLevel.Info, CurrentRecloc + ": " + EndStatus);
            }

            return _pnrDoc;
        }
             
    }
}
