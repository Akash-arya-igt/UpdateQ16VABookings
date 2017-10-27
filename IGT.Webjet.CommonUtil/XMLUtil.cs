using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IGT.Webjet.CommonUtil
{
    public static class XMLUtil
    {
        private static string _rootPath = "GALRequestTemplate";

        public static XmlDocument ReadTemplate(string _pTemplateName)
        {
            var xmlStr = File.ReadAllText(Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), _rootPath, _pTemplateName));

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlStr);

            return xmlDoc;
        }

        public static XmlDocument ReadTemplate(string _pPath, string _pTemplateName)
        {
            var xmlStr = File.ReadAllText(Path.Combine(_pPath, _rootPath, _pTemplateName));

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlStr);

            return xmlDoc;
        }

        public static int GetIntXmlNode(XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                try
                {
                    Double tempValue;
                    if (Double.TryParse(xmlNode.InnerText, NumberStyles.Any, CultureInfo.InvariantCulture, out tempValue))
                    {
                        return Convert.ToInt32(tempValue);
                    }
                    else
                    {
                        return 0;
                    }
                }
                catch (Exception)
                {

                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        public static string GetStringXmlNode(this XmlNode xmlNode)
        {
            if (xmlNode != null)
            {
                return xmlNode.InnerText;
            }
            else
            {
                return "";
            }
        }

        public static string GetStringXmlNode(string xmlString, string xPath)
        {
            string xmlNodeString = string.Empty;

            if (!string.IsNullOrEmpty(xmlString))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);

                XmlNodeList xmlStringNodeList = xmlDoc.SelectNodes(xPath);

                foreach (XmlNode xmlStringNode in xmlStringNodeList)
                {
                    xmlNodeString = string.Concat(xmlNodeString, xmlStringNode.InnerText, "-");
                }
            }

            return xmlNodeString;
        }

        public static string GetStringChildNode(this XmlNode xmlParent, string xpathChild)
        {
            if (xmlParent == null) { throw new ArgumentNullException("xmlParent"); }
            XmlNode xmlNode = xmlParent.SelectSingleNode(xpathChild);
            return GetStringXmlNode(xmlNode);
        }

        public static int GetIntChildNode(this XmlNode xmlParent, string xpathChild)
        {
            if (xmlParent == null) { throw new ArgumentNullException("xmlParent"); }
            XmlNode xmlNode = xmlParent.SelectSingleNode(xpathChild);
            return GetIntXmlNode(xmlNode);
        }

        public static bool RemoveChildIfExist(this XmlNode nodeCurrent, string xPath)
        {
            bool bRet = false;
            XmlNode changeNode = nodeCurrent.SelectSingleNode(xPath);
            if (changeNode != null)
            {
                RemoveChild(nodeCurrent, xPath);
                bRet = true;
            }
            return bRet;
        }

        public static XmlNode RemoveChild(this XmlNode nodeCurrent, string xPath)
        {
            if (nodeCurrent == null) { throw new ArgumentNullException("nodeCurrent"); }
            XmlNode childNode = nodeCurrent.SelectSingleNode(xPath);
            if (childNode == null)
            { throw new NullReferenceException("xPath " + xPath + " not found in node  " + nodeCurrent.OuterXml); }
            XmlNode directParent = childNode.ParentNode;
            directParent.RemoveChild(childNode);
            return nodeCurrent;
        }

        public static void AppendNewNodeFromOtherDoc(XmlDocument originDoc, XmlNode nodeParent, XmlNode newNode)
        {
            if (newNode != null && originDoc != null && nodeParent != null)
            {
                XmlNode importedNode = originDoc.ImportNode(newNode, true);
                nodeParent.AppendChild(importedNode);
            }
        }

        public static void InsertNewNodeFromOtherDocAfterRef(XmlDocument originDoc, XmlNode nodeParent, XmlNode newNode, XmlNode refNode)
        {
            if (newNode != null && originDoc != null && nodeParent != null && refNode != null)
            {
                XmlNode importedNode = originDoc.ImportNode(newNode, true);
                nodeParent.InsertAfter(importedNode, refNode);
            }
        }

        public static void InsertFragmentFromOtherDoc(XmlDocument argHostDoc, string argXPathToRefNode, XmlDocument argFragmentDoc)
        {
            if (argHostDoc != null)
            {
                XmlNode importedNode = argHostDoc.ImportNode(argFragmentDoc.DocumentElement, true);
                XmlNode referenceNodeInOriginalDoc = argHostDoc.SelectSingleNode(argXPathToRefNode);
                if (referenceNodeInOriginalDoc != null && importedNode != null)
                {
                    argHostDoc.DocumentElement.InsertAfter(importedNode, referenceNodeInOriginalDoc);
                }
            }
        }

        public static XmlNode SetSingleNodeText(this XmlNode nodeCurrent, string xPath, string innerText)
        {
            if (nodeCurrent == null) { throw new ArgumentNullException("nodeCurrent"); }
            XmlNode changeNode = nodeCurrent.SelectSingleNode(xPath);
            if (changeNode == null)
            { throw new NullReferenceException("xPath " + xPath + " not found in node  " + nodeCurrent.OuterXml); }
            changeNode.InnerText = innerText;
            return changeNode;
        }

        public static XmlNode SetNodeTextIfExist(this XmlNode nodeCurrent, string xPath, string innerText)
        {
            if (nodeCurrent == null) { throw new ArgumentNullException("nodeCurrent"); }
            XmlNode changeNode = nodeCurrent.SelectSingleNode(xPath);
            if (changeNode != null)
            {
                changeNode.InnerText = innerText;
            }
            return changeNode;
        }

        public static XmlNode SetChildNodeCDataStringIfExist(XmlNode xmlParent, string xPathChild, string innerText)
        {
            XmlNode acctNode = xmlParent.SelectSingleNode(xPathChild);
            XmlNode childNode = acctNode.ChildNodes[0];
            if (childNode is XmlCDataSection)
            {
                XmlCDataSection cdataSection = childNode as XmlCDataSection;
                cdataSection.Value = innerText;
            }

            return childNode;
        }

        public static XmlNode CloneSiblingAfterExistingNode(this XmlNode existingNode)
        {
            XmlNode parent = existingNode.ParentNode;
            XmlNode newNode = existingNode.Clone();
            parent.InsertAfter(newNode, existingNode);
            return newNode;
        }
    }
}
