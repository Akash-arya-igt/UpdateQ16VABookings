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
    public class QueueAction
    {
        private string _readPNR = "RetrieveExistingPNR.xml";
        private string _moveToQRequest = "PNRMoveToQueue.xml";
        private string _removeFromQ = "QueueRemoveSignOut.xml";
        private string _queueProcessRequest = "QueueProcessing.xml";


        public int QueueCount(GetHAPDetail _pHAP, int _pQNum)
        {
            int intQKnt = 0;
            bool responseFound = false;
            string strQAction = "QCT";
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_queueProcessRequest);
            reqTemplate.SetNodeTextIfExist("//Action", strQAction);
            reqTemplate.SetNodeTextIfExist("//QNum", _pQNum.ToString());

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXml(reqTemplate.DocumentElement);

            string xPath = "//QueueCount/HeaderCount/TotPNRBFCnt";
            XmlNode nodePnrCnt = response.SelectSingleNode(xPath);
            if (nodePnrCnt != null)
            {
                responseFound = int.TryParse(nodePnrCnt.InnerText, out intQKnt);
            }

            return intQKnt;
        }

        public int QueueCount(GetHAPDetail _pHAP, int _pQNum, string _pXmlTemplatePath)
        {
            int intQKnt = 0;
            bool responseFound = false;
            string strQAction = "QCT";
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_pXmlTemplatePath, _queueProcessRequest);
            reqTemplate.SetNodeTextIfExist("//Action", strQAction);
            reqTemplate.SetNodeTextIfExist("//QNum", _pQNum.ToString());

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXml(reqTemplate.DocumentElement);

            string xPath = "//QueueCount/HeaderCount/TotPNRBFCnt";
            XmlNode nodePnrCnt = response.SelectSingleNode(xPath);
            if (nodePnrCnt != null)
            {
                responseFound = int.TryParse(nodePnrCnt.InnerText, out intQKnt);
            }

            return intQKnt;
        }

        public XmlElement ReadQueue(GetHAPDetail _pHAP, int _pQNum, string _pSession)
        {
            string strQAction = "Q";
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_queueProcessRequest);
            reqTemplate.SetNodeTextIfExist("//Action", strQAction);
            reqTemplate.SetNodeTextIfExist("//QNum", _pQNum.ToString());

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return response;
        }

        public XmlElement ReadPNR(GetHAPDetail _pHAP, string _pRecloc, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_readPNR);
            reqTemplate.SetNodeTextIfExist("//RecLoc", _pRecloc);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return response;
        }

        public string CreateSession(GetHAPDetail _pHAP)
        {
            GWSConn objGwsConn = new GWSConn(_pHAP);
            return objGwsConn.CreateSession();
        }

        public void CloseSession(GetHAPDetail _pHAP, string _pSession)
        {
            GWSConn objGwsConn = new GWSConn(_pHAP);
            objGwsConn.EndSession(_pSession);
        }

        public XmlElement MoveTOQueue(GetHAPDetail _pSourceHAP, int _pQNum, string _pRemark, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_moveToQRequest);
            reqTemplate.SetNodeTextIfExist("//QNum", _pQNum.ToString());

            string xPath = "//Item[DataBlkInd='G']";
            XmlNode newNode = reqTemplate.SelectSingleNode(xPath);
            _pRemark = _pRemark.Replace(";", Environment.NewLine);
            string[] remarkList = _pRemark.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < remarkList.Length; i++)
            {
                string remark = remarkList[i];
                if (String.IsNullOrEmpty(remark))
                    continue;
                if (i != 0)
                {
                    newNode = newNode.CloneSiblingAfterExistingNode();
                }
                XmlNode rmkNode = newNode.SelectSingleNode(".//Rmk");
                remark = remark.Length > 85 ? remark.Substring(0, 85) : remark;
                rmkNode.InnerText = remark;
            }

            GWSConn objGwsConn = new GWSConn(_pSourceHAP);
            XmlElement response = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);


            string strErrorText = response.GetStringChildNode("//EndTransaction/EndTransactMessage/Text");
            string strErrorCode = response.GetStringChildNode("//EndTransaction/ErrorCode");

            if (!string.IsNullOrEmpty(strErrorText) || !string.IsNullOrEmpty(strErrorCode))
            {
                reqTemplate.RemoveChildIfExist("//RcvdFrom");
                reqTemplate.RemoveChildIfExist("//PNRBFSecondaryBldChgMods");
                response = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);
            }

            return response;
        }

        public XmlElement RemoveFromQ(GetHAPDetail _pHAP, string _pSession)
        {
            XmlDocument reqTemplate = XMLUtil.ReadTemplate(_removeFromQ);

            GWSConn objGwsConn = new GWSConn(_pHAP);
            XmlElement response = objGwsConn.SubmitXmlOnSession(_pSession, reqTemplate.DocumentElement);

            return response;
        }
    }
}
