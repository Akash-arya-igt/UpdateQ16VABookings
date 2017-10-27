using System;
using System.Xml;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using IGT.Webjet.CommonUtil;
using System.ComponentModel;
using System.Collections.Generic;
using IGT.Webjet.BusinessEntities;
using IGT.Webjet.GALEngine.GALAction;

namespace UpdateSSRDetailUtil
{
    public partial class UpdateSSRUtil : Form
    {
        int _qNo;
        int _exceptions = 0;
        int _totalQCount = 0;
        int _scanPNRCount = 0;
        int _processedPNRCount = 0;
        int _NoActiveSegRemoveCount = 0;

        GetHAPDetail _objHAPDetail;
        BackgroundWorker _objWorker;
        List<string> _lstProcesedPNR = new List<string>();
        List<string> _lstExceptionPNR = new List<string>();
        List<string> _lstNoActiveSegPNR = new List<string>();

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
            try
            {
                _lstProcesedPNR.Clear();
                _lstExceptionPNR.Clear();
                _lstNoActiveSegPNR.Clear();
                txtProcessedPNR.Text = string.Empty;
                txtExceptionPNR.Text = string.Empty;
                lblStatus.Text = string.Empty;
                lblProcessedCount.Text = string.Empty;
                btnStart.Enabled = false;
                btnCancel.Enabled = true;
                // Kickoff the worker thread to begin it's DoWork function.
                _objWorker.RunWorkerAsync();
            }
            catch(Exception ex)
            {
                NLogManager._instance.LogMsg(NLogLevel.Error, ex.Message);
            }
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
            try
            {
                progressBar1.Value = e.ProgressPercentage;
                lblStatus.Text = "Scanning (" + _scanPNRCount + "/" + _totalQCount + ")......." + progressBar1.Value.ToString() + "%";
                lblProcessedCount.Text = "Processed: " + _processedPNRCount + ";        No Active Seg Remove PNR Count: " + _NoActiveSegRemoveCount + ";        Exception Count: " + _exceptions;

                if (_lstProcesedPNR != null && _lstProcesedPNR.Count > 0)
                    txtProcessedPNR.Text = string.Join(",", _lstProcesedPNR.Where(x => !string.IsNullOrEmpty(x)));

                if (_lstExceptionPNR != null && _lstExceptionPNR.Count > 0)
                    txtExceptionPNR.Text = string.Join(",", _lstExceptionPNR.Where(x => !string.IsNullOrEmpty(x)));

                if (_lstNoActiveSegPNR != null && _lstNoActiveSegPNR.Count > 0)
                    txtNoActiveSegPNR.Text = string.Join(",", _lstNoActiveSegPNR.Where(x => !string.IsNullOrEmpty(x)));
            }
            catch(Exception ex)
            {
                NLogManager._instance.LogMsg(NLogLevel.Error, ex.Message);
            }
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
            _NoActiveSegRemoveCount = 0;
            _exceptions = 0;
            _lstProcesedPNR.Clear();
            _lstExceptionPNR.Clear();
            _lstNoActiveSegPNR.Clear();

            try
            {
                int qKnt = objQAction.QueueCount(_objHAPDetail, _qNo);
                if (qKnt > 0)
                {
                    _totalQCount = qKnt;
                    string strSession = objQAction.CreateSession(_objHAPDetail);
                    string strRecloc = string.Empty;

                    XmlElement _pnrDoc = objQAction.ReadQueue(_objHAPDetail, _qNo, strSession);
                    //XmlElement _pnrDoc = objQAction.ReadPNR (_objHAPDetail, "Q1F2BI", strSession);
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

                                try { _pnrDoc = objUpdateSSR.StartProcessing(out strOutResult); }
                                catch(Exception ex) { NLogManager._instance.LogMsg(NLogLevel.Warn, strRecloc + ": " + ex.Message); }

                                if (!string.IsNullOrEmpty(strRecloc) || qKnt == 0)
                                {
                                    if (strOutResult == ProcessResult.RemovedFromQ.ToString())
                                    {
                                        _processedPNRCount = _processedPNRCount + 1;
                                        _lstProcesedPNR.Add(strRecloc);
                                    }
                                    else if (strOutResult == ProcessResult.NoActiveSegments.ToString())
                                    {
                                        _NoActiveSegRemoveCount = _NoActiveSegRemoveCount + 1;
                                        _lstNoActiveSegPNR.Add(strRecloc);
                                    }
                                }

                                string tempPNR = strRecloc;
                                strRecloc = objPNRParse.GetReclocFromPNRXml(_pnrDoc);

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
                            string tempRecloc = string.Empty;
                            tempRecloc = strRecloc;
                            
                            if (!string.IsNullOrEmpty(strRecloc))
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
                            NLogManager._instance.LogMsg(NLogLevel.Warn, tempRecloc + ": " + ex.Message);
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
