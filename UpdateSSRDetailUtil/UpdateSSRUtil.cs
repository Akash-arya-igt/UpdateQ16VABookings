using IGT.Webjet.BusinessEntities;
using IGT.Webjet.CommonUtil;
using IGT.Webjet.GALEngine.GALAction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace UpdateSSRDetailUtil
{
    public partial class UpdateSSRUtil : Form
    {
        GetHAPDetail _objHAPDetail;
        BackgroundWorker _objWorker;
        int _qNo;
        int _totalQCount = 0;
        int _scanPNRCount = 0;
        int _processedPNRCount = 0;
        int _exceptions = 0;
        List<string> _lstProcesedPNR = new List<string>();
        List<string> _lstExceptionPNR = new List<string>();

        public UpdateSSRUtil()
        {
            InitializeComponent();
            _objHAPDetail = new GetHAPDetail()
            {
                PCC = ConfigUtil.GetConfigValue("WebJetPCC", "5KL6"),
                Profile = ConfigUtil.GetConfigValue("WebJetHAP", "DynGalileoCopy_5KL6"),
                UserID = ConfigUtil.GetConfigValue("WebJetUID", "PCC5KL6"),
                Password = ConfigUtil.GetConfigValue("WebJetPWD", "Webj5kl6")
            };
            _qNo = ConfigUtil.GetIntConfigValue("QNo", 16);
            lblHAP.Text = _objHAPDetail.Profile;

            lblStatus.Text = string.Empty;
            lblProcessedCount.Text = string.Empty;

            _objWorker = new BackgroundWorker();
            _objWorker.DoWork += new DoWorkEventHandler(m_oWorker_DoWork);
            _objWorker.ProgressChanged += new ProgressChangedEventHandler
                    (m_oWorker_ProgressChanged);
            _objWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler
                    (m_oWorker_RunWorkerCompleted);
            _objWorker.WorkerReportsProgress = true;
            _objWorker.WorkerSupportsCancellation = true;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartProcessing();
        }

        private void StartProcessing()
        {
            _lstProcesedPNR.Clear();
            _lstExceptionPNR.Clear();
            txtProcessedPNR.Text = string.Empty;
            txtExceptionPNR.Text = string.Empty;
            lblStatus.Text = string.Empty;
            lblProcessedCount.Text = string.Empty;
            btnStart.Enabled = false;
            btnCancel.Enabled = true;

            // Kickoff the worker thread to begin it's DoWork function.
            _objWorker.RunWorkerAsync();
        }

        void m_oWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // The background process is complete. We need to inspect
            // our response to see if an error occurred, a cancel was
            // requested or if we completed successfully.  
            if (e.Cancelled)
            {
                lblStatus.Text = "Task Cancelled.";
            }

            // Check to see if an error occurred in the background process.

            else if (e.Error != null)
            {
                lblStatus.Text = "Scan aborted due to: " + e.Error;
            }
            else
            {
                // Everything completed normally.
                lblStatus.Text = "Task Completed...";
                Application.Exit();
            }

            //Change the status of the buttons on the UI accordingly
            btnStart.Enabled = true;
            btnCancel.Enabled = false;
        }


        void m_oWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblStatus.Text = "Scanning (" + _scanPNRCount + "/" + _totalQCount + ")......." + progressBar1.Value.ToString() + "%";
            lblProcessedCount.Text = "Processed: " + _processedPNRCount + ";        Exception Count: " + _exceptions;

            if (_lstProcesedPNR != null && _lstProcesedPNR.Count > 0)
                txtProcessedPNR.Text = string.Join(",", _lstProcesedPNR.Where(x => !string.IsNullOrEmpty(x)));

            if (_lstExceptionPNR != null && _lstExceptionPNR.Count > 0)
                txtExceptionPNR.Text = string.Join(",", _lstExceptionPNR.Where(x => !string.IsNullOrEmpty(x)));
        }


        void m_oWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string strOutResult = string.Empty;
            QueueAction objQAction = new QueueAction();
            FareAction objFareAction = new FareAction();
            PNRParseAction objPNRParse = new PNRParseAction();

            _totalQCount = 0;
            _scanPNRCount = 0;
            _processedPNRCount = 0;
            _exceptions = 0;
            _lstProcesedPNR.Clear();
            _lstExceptionPNR.Clear();

            try
            {
                int qKnt = objQAction.QueueCount(_objHAPDetail, _qNo);
                if (qKnt > 0)
                {
                    _totalQCount = qKnt;
                    string strSession = objQAction.CreateSession(_objHAPDetail);
                    string strRecloc = string.Empty;
                    XmlElement _pnrDoc = objQAction.ReadQueue(_objHAPDetail, _qNo, strSession);
                    strRecloc = objPNRParse.GetReclocFromPNRXml(_pnrDoc);

                    while (qKnt > 0)
                    {
                        try
                        {
                            if (_objWorker.CancellationPending)
                            {
                                e.Cancel = true;
                                _objWorker.ReportProgress(0);
                                return;
                            }

                            _scanPNRCount = _scanPNRCount + 1;
                            if (!string.IsNullOrEmpty(strRecloc))
                            {
                                qKnt = qKnt - 1;

                                UpdateSSRDetail objUpdateSSR = new UpdateSSRDetail()
                                {
                                    HAPDetail = _objHAPDetail,
                                    Session = strSession,
                                    CurrentRecloc = strRecloc,
                                    PnrDoc = _pnrDoc
                                };

                                _pnrDoc = objUpdateSSR.StartProcessing(out strOutResult);
                                strRecloc = objPNRParse.GetReclocFromPNRXml(_pnrDoc);

                                if (!string.IsNullOrEmpty(strRecloc) || qKnt == 0)
                                {
                                    if (strOutResult == ProcessResult.RemovedFromQ.ToString())
                                    {
                                        _processedPNRCount = _processedPNRCount + 1;
                                        _lstProcesedPNR.Add(strRecloc);
                                    }

                                }

                                if(string.IsNullOrEmpty(strRecloc) && qKnt > 0)
                                {
                                    try { objQAction.CloseSession(_objHAPDetail, strSession); } catch { }

                                    strSession = objQAction.CreateSession(_objHAPDetail);
                                    _pnrDoc = objQAction.ReadQueue(_objHAPDetail, _qNo, strSession);
                                    strRecloc = objPNRParse.GetReclocFromPNRXml(_pnrDoc);
                                }

                            }
                            else
                                break;
                        }
                        catch(Exception ex)
                        {
                            _exceptions = _exceptions + 1;
                            
                            if(!string.IsNullOrEmpty(strRecloc))
                            {
                                _lstExceptionPNR.Add(strRecloc);
                            }

                            try
                            {
                                _pnrDoc = objFareAction.Ignore(_objHAPDetail, strSession);
                            }
                            catch
                            {
                                strSession = objQAction.CreateSession(_objHAPDetail);
                                _pnrDoc = objQAction.ReadQueue(_objHAPDetail, _qNo, strSession);
                                strRecloc = objPNRParse.GetReclocFromPNRXml(_pnrDoc);
                            }
                            NLogManager._instance.LogMsg(NLogLevel.Warn, ex.Message);
                        }
                        _objWorker.ReportProgress((_scanPNRCount * 100) / _totalQCount);
                    }
                }
            }
            catch(Exception ex)
            {
                NLogManager._instance.LogMsg(NLogLevel.Error, ex.Message);
                throw ex;
            }

            //Report 100% completion on operation completed
            _objWorker.ReportProgress(100);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (_objWorker.IsBusy)
            {
                _objWorker.CancelAsync();
            }
        }

        private void UpdateSSRUtil_Load(object sender, EventArgs e)
        {
            StartProcessing();
        }

        private void UpdateSSRUtil_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_objWorker.IsBusy)
            {
                _objWorker.CancelAsync();
            }
        }
    }
}
