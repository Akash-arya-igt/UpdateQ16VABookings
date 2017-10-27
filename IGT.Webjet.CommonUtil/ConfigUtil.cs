using System;
using System.Configuration;

namespace IGT.Webjet.CommonUtil
{
    public static class ConfigUtil
    {
        public static string GetConfigValue(string key, string defaultValue = null)
        {
            string strConfigValue = string.Empty;
            strConfigValue = ConfigurationManager.AppSettings[key];
            return !string.IsNullOrEmpty(strConfigValue) ? strConfigValue : (!string.IsNullOrEmpty(defaultValue) ? defaultValue : string.Empty);
        }

        public static int GetIntConfigValue(string key, int defaultValue)
        {
            int intConfigValue = 0;
            return Int32.TryParse(ConfigurationManager.AppSettings[key], out intConfigValue) ? intConfigValue : defaultValue;
        }
    }
}
