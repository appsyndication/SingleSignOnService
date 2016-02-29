﻿using System.Web.Configuration;

namespace AppSyndication.SingleSignOnService.Web
{
    public abstract class EnvironmentConfiguration
    {
        protected EnvironmentConfiguration()
        {
            this.Environment = WebConfigurationManager.AppSettings.Get("Environment") ?? "Dev";
        }

        public string Environment { get; }

        protected string GetConnectionString(string key, string defaultValue = null)
        {
            var environmentKey = key + "." + this.Environment;
            return WebConfigurationManager.ConnectionStrings[environmentKey]?.ConnectionString ?? defaultValue;
        }

        protected string GetSetting(string key, string defaultValue = null)
        {
            var environmentKey = key + "." + this.Environment;
            return WebConfigurationManager.AppSettings[environmentKey] ?? defaultValue;
        }
    }

    public class SsoServiceEnvironmentConfiguration : EnvironmentConfiguration
    {
        public string TableStorageConnectionString => base.GetConnectionString("Storage");

        public string CertificateThumprint => base.GetSetting("CertificateThumbprint");
    }
}