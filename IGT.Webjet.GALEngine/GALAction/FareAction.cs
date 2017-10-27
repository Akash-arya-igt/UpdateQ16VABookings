using IGT.Webjet.BusinessEntities;
using IGT.Webjet.CommonUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IGT.Webjet.GALEngine.GALAction
{
    public class FareAction
    {
        private string _fareInfoRequest = "DisplayFareInfo.xml";
        private string _fareDetailRequest = "DisplayFareDetails.xml";
        private string _ticketIssuanceCash = "IssueTicketForCash.xml";
        private string _retrievePNRRequest = "RetrieveExistingPNR.xml";
        private string _addRemarkWithNoMove = "AddGenRmkNoMove.xml";
        private string _endTransact = "EndTransact.xml";
        private string _ignore = "IgnoreBooking.xml";
        private string _ignoreAndDisplay = "IgnoreAndRedisplay.xml";
        private string _updateSSR = "UpdateEmailPhoneSSR.xml";
        //private string _endTransactNRetrieve = "EndTransactRetrieve";

        public enum RequestAction
        {
            I,
            D
        }

        public XmlElement RetrievePNR(GetHAPDetail _pHAP, string _pRecloc, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_retrievePNRRequest);
            reqTemplate.SetNodeTextIfExist("//RecLoc", _pRecloc);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);
            return response;
        }

        public XmlElement GetFareInfo(GetHAPDetail _pHAP, string _pRecloc, out string _oSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_fareInfoRequest);
            reqTemplate.SetNodeTextIfExist("//RecLoc", _pRecloc);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXmlOnSession(reqTemplate.DocumentElement);
            _oSession = objGwsConn.Session;
            return response;
        }

        public XmlElement GetFareInfoInSession(GetHAPDetail _pHAP, string _pRecloc, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_fareInfoRequest);
            reqTemplate.SetNodeTextIfExist("//RecLoc", _pRecloc);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            objGwsConn.Session = _pSession;
            XmlElement response = objGwsConn.SubmitXmlOnSession(reqTemplate.DocumentElement);

            return response;
        }

        public XmlElement GetFareDetail(GetHAPDetail _pHAP, string _pRecloc, string _pFareNum)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_fareDetailRequest);
            reqTemplate.SetNodeTextIfExist("//RecLoc", _pRecloc);
            reqTemplate.SetNodeTextIfExist("//FareNum", _pFareNum);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            return objGwsConn.SubmitXml(reqTemplate.DocumentElement);
        }

        public string CheckMCTError(string _pSessionID, GetHAPDetail _pHAP)
        {
            string strMCTError = string.Empty;
            string s = ConfigUtil.GetConfigValue("MCTErrorPattern", "CHECK MINIMUM CONNECT TIME");
            string[] patterns = s.Split(',');

            GWSConn objGwsConn = new GWSConn(_pHAP);
            string misConectionResp = objGwsConn.SubmitTermainalCmd(_pSessionID, "@MT");
            foreach (string pattern in patterns)
            {
                if (misConectionResp.Contains(pattern))
                {
                    strMCTError = misConectionResp.Replace(">>", "");
                    break;
                }
            }

            return strMCTError;
        }

        public bool IsFOPTypeSupported(XmlElement _pPNRXml, string _pSupportedFOP)
        {
            if (!string.IsNullOrEmpty(_pSupportedFOP))
            {
                XmlNode otherFOPNode = _pPNRXml.SelectSingleNode("//OtherFOP");
                XmlNode creditCardNode = _pPNRXml.SelectSingleNode("//CreditCardFOP");
                XmlNode checkFOPNode = _pPNRXml.SelectSingleNode("//CheckFOP");

                try
                {
                    if (otherFOPNode != null)
                    {
                        string FOPId = otherFOPNode.SelectSingleNode("ID").InnerText;
                        if (!string.IsNullOrEmpty(FOPId))
                        {
                            if (_pSupportedFOP.Contains(FOPId))
                                return true;
                        }
                    }
                    else if (creditCardNode != null)
                    {
                        string creditCardFOPId = creditCardNode.SelectSingleNode("ID").InnerText;
                        if (!string.IsNullOrEmpty(creditCardFOPId))
                        {
                            if (_pSupportedFOP.Contains(creditCardFOPId))
                                return true;
                        }
                    }
                    else if (checkFOPNode != null)
                    {
                        string checkFOPId = checkFOPNode.SelectSingleNode("ID").InnerText;
                        if (!string.IsNullOrEmpty(checkFOPId))
                        {
                            if (_pSupportedFOP.Contains(checkFOPId))
                                return true;
                        }
                    }
                }
                finally
                {
                    otherFOPNode = null;
                    creditCardNode = null;
                    checkFOPNode = null;
                }
            }
            return false;
        }

        public TypeOfFare GetFareType(GetHAPDetail _pHAP, string _pRecloc, string _pSession)
        {
            string strSession = string.Empty;
            XmlElement fareInfo = GetFareInfo(_pHAP, _pRecloc, out strSession);
            TypeOfFare fareType = TypeOfFare.Unspecified;

            if (fareInfo != null)
            {
                try
                {
                    XmlNodeList extendedQuoteList = fareInfo.SelectNodes("//DocProdDisplayStoredQuote/ExtendedQuoteInformation");

                    if (extendedQuoteList != null)
                    {
                        string fareInd = string.Empty;
                        foreach (XmlNode netFareIndicator in extendedQuoteList)
                        {
                            if (netFareIndicator != null)
                            {
                                if (string.IsNullOrEmpty(fareInd))
                                    fareInd = netFareIndicator.SelectSingleNode("NetFareInd").InnerText;

                                if (fareInd != netFareIndicator.SelectSingleNode("NetFareInd").InnerText)
                                {
                                    fareType = TypeOfFare.Unspecified;
                                    break;
                                }
                                else if (fareInd == "Y")
                                    fareType = TypeOfFare.Private;
                                else
                                    fareType = TypeOfFare.Published;
                            }
                            else
                            {
                                fareType = TypeOfFare.Published;
                            }
                        }
                    }

                    extendedQuoteList = null;
                }
                catch
                {
                    fareType = TypeOfFare.Unspecified;
                }
            }
            return fareType;
        }

        public void IssueTicket(GetHAPDetail _pHAP, string _pRecloc, string _pTraceId, string _pSuccessRemark, out string _oSession)
        {
            string strSession;
            int intNoOfFares = 0;
            List<string> lstErrors;
            _oSession = string.Empty;
            //PNRProcessingAction objProcessTrace = new PNRProcessingAction();

            //try
            //{
            //    if (objProcessTrace.GetProcessingStatus(_pTraceId) == BusinessEntities.PNRProcessingStatus.Recorded.ToString())
            //    {
            //        XmlElement response = GetFareInfo(_pHAP, _pRecloc, out strSession);
            //        _oSession = strSession;

            //        if (response != null)
            //            intNoOfFares = response.SelectNodes("//GenQuoteDetails").Count;

            //        if (intNoOfFares == 0)
            //        {
            //            objProcessTrace.UpdateProcessingStatus(_pTraceId, BusinessEntities.PNRProcessingStatus.Completed, "NO FARES", 0, 0, true);
            //            throw new Exception("NO FARES");
            //        }
            //        else
            //            objProcessTrace.UpdateProcessingStatus(_pTraceId, BusinessEntities.PNRProcessingStatus.InProgress, string.Empty, intNoOfFares, 0, false);

            //        string strTransType = "TK"; // "MR" of multiple FOP

            //        for (int i = 1; i <= intNoOfFares; i++)
            //        {
            //            if (i != 1)
            //            {
            //                RetrievePNR(_pHAP, _pRecloc, strSession);
            //            }

            //            IssueTicketToFare(_pHAP, i.ToString(), strTransType, strSession, out lstErrors);


            //            if (lstErrors.Count > 0)
            //            {
            //                objProcessTrace.UpdateProcessingStatus(_pTraceId, PNRProcessingStatus.Completed, lstErrors[0], intNoOfFares, i - 1, true);
            //                throw new Exception(lstErrors[0]);
            //            }

            //            if (i == intNoOfFares)
            //            {
            //                objProcessTrace.UpdateProcessingStatus(_pTraceId, PNRProcessingStatus.Completed, string.Empty, intNoOfFares, i, true);
            //                AddRemarkNoMove(_pHAP, _pSuccessRemark, strSession);
            //            }
            //            else
            //                objProcessTrace.UpdateProcessingStatus(_pTraceId, PNRProcessingStatus.InProgress, string.Empty, intNoOfFares, i, false);
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    objProcessTrace.UpdateProcessingStatus(_pTraceId, PNRProcessingStatus.Completed, ex.Message, true);
            //    throw ex;
            //}
        }

        public XmlElement IssueTicketToFare(GetHAPDetail _pHAP, string _pFareNum, string _pTransType, string _pSession, out List<string> lstErrorMsg)
        {
            string strErrorMsg = string.Empty;
            lstErrorMsg = new List<string>();
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_ticketIssuanceCash);
            reqTemplate.SetNodeTextIfExist("//FareNum", _pFareNum);
            reqTemplate.SetNodeTextIfExist("//TransType", _pTransType);
            reqTemplate.RemoveChildIfExist("//CommissionMod");
            reqTemplate.RemoveChildIfExist("//NettFare");
            reqTemplate.RemoveChildIfExist("//CheckFOP");

            GWSConn objGwsConn = new GWSConn(_pHAP);

            XmlElement resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);
            lstErrorMsg = GetError(resp);

            return resp;
        }

        public XmlElement AddRemarkNoMove(GetHAPDetail _pHAP, string _pRemark, string _pSession)
        {
            XmlElement resp = null;
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_addRemarkWithNoMove);

            reqTemplate.SetNodeTextIfExist("//ItemAry/Item/GenRmkQual/AddQual/Rmk", _pRemark);


            GWSConn objGwsConn = new GWSConn(_pHAP);
            resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            //XmlElement xmlResp = EndTransact(objGwsConn, _pSession);
            List<string> lstError = GetError(resp);

            if (lstError != null && lstError.Count > 0)
            {
                ///Ignore retrieve and resend the cmd
                IgnoreAndReDisplay(objGwsConn, _pSession);
                resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

                //xmlResp = EndTransact(objGwsConn, _pSession);
            }

            return resp;
        }

        public XmlElement UpdatePhoneEmailInSSR(GetHAPDetail _pHAP, string _pEmail, string _pPhone, string _pSession)
        {
            XmlElement resp = null;
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_updateSSR);
            bool isEmailUpdated = false, isPhoneUpdated = false;

            XmlNodeList SSRItemList = reqTemplate.SelectNodes("//PNRBFSecondaryBldChgMods/ItemAry/Item");

            foreach(XmlNode SSRItem in SSRItemList)
            {
                if(SSRItem.SelectSingleNode("//SSRQual/AddQual/SSRCode") != null)
                {
                    if (SSRItem.GetStringChildNode("SSRQual/AddQual/SSRCode") == "CTCM")
                    {
                        SSRItem.SetSingleNodeText("SSRQual/AddQual/Text", _pPhone);
                        isPhoneUpdated = true;
                    }

                    else if (SSRItem.GetStringChildNode("SSRQual/AddQual/SSRCode") == "CTCE")
                    {
                        SSRItem.SetSingleNodeText("SSRQual/AddQual/Text", _pEmail);
                        isEmailUpdated = true;
                    }

                    if (isPhoneUpdated && isEmailUpdated)
                        break;
                }
            }

            GWSConn objGwsConn = new GWSConn(_pHAP);
            resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            //XmlElement xmlResp = EndTransact(objGwsConn, _pSession);
            List<string> lstError = GetError(resp);

            if (lstError != null && lstError.Count > 0)
            {
                /////Ignore retrieve and resend the cmd
                //IgnoreAndReDisplay(objGwsConn, _pSession);
                //resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

                ////xmlResp = EndTransact(objGwsConn, _pSession);

                throw new Exception(lstError[0]);
            }

            return resp;
        }

        public XmlElement IgnoreAndReDisplay(GWSConn _pGwsConn, string _pSession)
        {
            XmlElement resp = null;
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_ignoreAndDisplay);

            resp = _pGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return resp;
        }

        public XmlElement Ignore(GWSConn _pGwsConn, string _pSession)
        {
            XmlElement resp = null;
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_ignore);

            resp = _pGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return resp;
        }

        public XmlElement IgnoreAndReDisplay(GetHAPDetail _pHAP, string _pSession)
        {
            XmlElement resp = null;

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_ignoreAndDisplay);

            resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return resp;
        }

        public XmlElement Ignore(GetHAPDetail _pHAP, string _pSession)
        {
            XmlElement resp = null;

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_ignore);

            resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return resp;
        }

        public XmlElement EndTransact(GWSConn _pGwsConn, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_endTransact);
            XmlElement resp = _pGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            List<string> lstError = GetError(resp);
            if (lstError != null && lstError.Count > 0)
            {
                reqTemplate.RemoveChildIfExist("//RcvdFrom");
                resp = _pGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);
            }

            return resp;
        }

        public XmlElement EndTransactRetrieveNextPNR(GetHAPDetail _pHAP, string _pSession)
        {
            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_endTransact);
            XmlElement resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            List<string> lstError = GetError(resp);

            if ((lstError != null && lstError.Count > 0))
            {
                lstError.Clear();
                reqTemplate.RemoveChildIfExist("//RcvdFrom");
                resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);
            }

            lstError = GetError(resp);
            if (lstError != null && lstError.Count > 0)
            {
                throw new Exception(lstError[0]);
            }

            return resp;
        }

        public XmlElement EndTransactNRetrieve(GetHAPDetail _pHAP, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_endTransact);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            List<string> lstError = GetError(resp);


            if (lstError != null && lstError.Count > 0)
            {
                reqTemplate.RemoveChildIfExist("//RcvdFrom");
                resp = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

                IgnoreAndReDisplay(_pHAP, _pSession);
            }

            return resp;
        }

        public List<string> GetError(XmlElement responseDoc)
        {
            string strErrorMsg = string.Empty;
            List<string> lstErrorMsg = new List<string>();
            strErrorMsg = GetErrorsFromNodesWithErrorCode(responseDoc);
            if (!string.IsNullOrEmpty(strErrorMsg))
                lstErrorMsg.Add(strErrorMsg);

            strErrorMsg = string.Empty;
            strErrorMsg = GetErrorsFromNodesWithErrText(responseDoc);
            if (!string.IsNullOrEmpty(strErrorMsg))
                lstErrorMsg.Add(strErrorMsg);

            strErrorMsg = string.Empty;
            strErrorMsg = GetErrorsFromNodesWithErrorFault(responseDoc);
            if (!string.IsNullOrEmpty(strErrorMsg))
                lstErrorMsg.Add(strErrorMsg);

            strErrorMsg = string.Empty;
            strErrorMsg = GetErrorsFromNodesWithTrancestionError(responseDoc);
            if (!string.IsNullOrEmpty(strErrorMsg))
                lstErrorMsg.Add(strErrorMsg);

            return lstErrorMsg;
        }

        public string GetErrorsFromNodesWithErrorCode(XmlElement responseDoc)
        {
            string sErrorNodesPath = "//ErrorCode";
            return GetErrorsFromErrorNodes(responseDoc, sErrorNodesPath);
        }

        public string GetErrorsFromNodesWithErrText(XmlElement responseDoc)
        {
            string sErrorNodesPath = "//ErrText";
            return GetErrorsFromErrorNodes(responseDoc, sErrorNodesPath);
        }

        public string GetErrorsFromNodesWithErrorFault(XmlElement responseDoc)
        {
            string sErrorNodesPath = "//ErrorFault";
            return GetErrorsFromErrorNodes(responseDoc, sErrorNodesPath);
        }

        public string GetErrorsFromNodesWithTrancestionError(XmlElement responseDoc)
        {
            string sErrorNodesPath = string.Empty;

            if (responseDoc.SelectSingleNode("//TransactionErrorCode") != null
                && responseDoc.SelectSingleNode("//PNRBFSecondaryBldChg") != null
                && responseDoc.SelectSingleNode("//PNRBFSecondaryBldChg/Text") != null)
                sErrorNodesPath = responseDoc.GetStringChildNode("//PNRBFSecondaryBldChg/Text");
            else if(responseDoc.SelectSingleNode("//TransactionErrorCode") != null
                    && responseDoc.SelectSingleNode("//EndTransaction") != null
                    && responseDoc.SelectSingleNode("//EndTransaction/EndTransactMessage") != null
                    && responseDoc.SelectSingleNode("//EndTransaction/EndTransactMessage/Text") != null)
                sErrorNodesPath = responseDoc.GetStringChildNode("//EndTransaction/EndTransactMessage/Text");
            return sErrorNodesPath;
        }

        private string GetErrorsFromErrorNodes(XmlElement responseDoc, string sErrorNodesPath)
        {
            return GetErrorsFromErrorNodes(responseDoc, sErrorNodesPath, null, null);
        }

        public string GetErrorsFromErrorNodes(XmlElement responseDoc, string sErrorNodesPath, List<string> excludeParentNodesNames, List<string> skipMessagesPatterns)
        {
            string sRet = "";

            /*TODO: recognize error like the following 
             <PNRBFSecondaryBldChg>
              <Len>53</Len> 
              <RecID>EROR</RecID> 
              <AryCnt>0</AryCnt> 
              <DelimiterCharacter>F</DelimiterCharacter> 
              <LevelNum>0</LevelNum> 
              <VersionNum>1</VersionNum> 
              <Err /> 
            - <DataBlkInd>
            - <![CDATA[   O
              ]]> 
              </DataBlkInd>
              <InsertedTextAry /> 
              <Text>INVALID CHARACTER IN TEXT</Text> 
              </PNRBFSecondaryBldChg>

             */
            XmlNodeList nodeList = responseDoc.SelectNodes(sErrorNodesPath);//eg "Ticketing/ErrText");
            foreach (XmlNode errNode1 in nodeList)
            {
                if (errNode1 != null)
                {
                    string sParentName = "";
                    if (errNode1.ParentNode != null)
                    {
                        sParentName = errNode1.ParentNode.Name;
                        if (excludeParentNodesNames != null && excludeParentNodesNames.Where(x => !string.IsNullOrEmpty(x)).Count() > 0)
                        {
                            if (excludeParentNodesNames.Contains(sParentName))
                            {
                                continue;
                            }
                        }
                    }
                    if (skipMessagesPatterns != null && skipMessagesPatterns.Where(x => !string.IsNullOrEmpty(x)).Count() > 0)
                    {
                        string sMsg = errNode1.InnerText;
                        string sPattern = skipMessagesPatterns.Find(sMsg.Contains);
                        if (!String.IsNullOrEmpty(sPattern))
                        {
                            //sMsg = sPattern;//return pattern instead of message???
                            continue;
                        }
                    }
                    //example of ErrText <ErrText><Err>D0002308</Err><KlrInErr>0000</KlrInErr><InsertedTextAry></InsertedTextAry><Text>NO FARES</Text></ErrText>
                    string shortText = XMLUtil.GetStringChildNode(errNode1, "Text");
                    sRet += shortText;// +String.Format(" Error in node:{0}  {1}\n", sParentName, errNode1.OuterXml);
                    //sRet += shortText + String.Format(" Error in node:{0}  {1}\n", sParentName, errNode1.OuterXml);
                }
            }
            return sRet;
        }
    }
}
