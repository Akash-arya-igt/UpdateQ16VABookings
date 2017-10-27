using System;
using System.Net;
using System.Xml;
using IGT.Webjet.BusinessEntities;
using IGT.Webjet.GALEngine.XmlSelectService;

namespace IGT.Webjet.GALEngine
{
    public class GWSConn : XmlSelect
    {
        public string Session { get { return _token; } set { _token = value; } }
        private string _token = "";
        private XmlDocument _filter;
        private GetHAPDetail _hapDetail;

        public GWSConn(GetHAPDetail _pHAPDetail)
        {
            _hapDetail = _pHAPDetail;

            if (string.IsNullOrEmpty(this.Url))
            {
                this.Url = _hapDetail.GWSConnURL;
            }

            _filter = new XmlDocument();
            _filter.LoadXml("<_/>");

            CredentialCache cc = new CredentialCache();
            NetworkCredential netCredentials = new NetworkCredential(_hapDetail.UserID, _hapDetail.Password);

            Uri uri = new Uri(this.Url);
            ServicePoint servicePoint = ServicePointManager.FindServicePoint(uri);
            if (servicePoint.Expect100Continue == true)
            {
                servicePoint.Expect100Continue = false;
            }

            cc.Add(uri, "Basic", netCredentials);
            Credentials = cc;
            PreAuthenticate = true;

            //this.Timeout = ConfigUtil.GetIntConfigValue("GalileoTimeOut", 120);
        }

        public XmlElement SubmitXml(XmlElement request)
        {
            return this.SubmitXml(this._hapDetail.Profile, request, _filter.DocumentElement);
        }

        public XmlElement SubmitXmlOnSession(XmlElement request)
        {
            if (this._token == "") this._token = this.BeginSession(this._hapDetail.Profile);
            return this.SubmitXmlOnSession(this._token, request, _filter.DocumentElement);
        }

        public XmlElement SubmitXmlOnSession(string session, XmlElement request)
        {
            return this.SubmitXmlOnSession(session, request, _filter.DocumentElement);
        }

        public void CloseSession()
        {
            if (string.IsNullOrEmpty(_token))
            {
                this.EndSession(this._token);
                this._token = "";
            }
        }

        public void CloseSession(string session)
        {
            if (string.IsNullOrEmpty(session))
            {
                this.EndSession(session);
            }
        }

        public string CreateSession()
        {
            if (this._token == "") this._token = this.BeginSession(this._hapDetail.Profile);
            return this._token;
        }

        public string SubmitTermainalCmd(string session, string command)
        {
            return this.SubmitTerminalTransaction(this._hapDetail.Profile, session, command, "");
        }
    }
}
