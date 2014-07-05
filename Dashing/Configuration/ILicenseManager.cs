namespace Dashing.Configuration {
    using System;
    using System.Collections.Specialized;

    internal interface ILicenseManager {
        bool IsLicensed(NameValueCollection settings, Version version);

        bool IsLicensed(NameValueCollection settings);

        string GenerateLicense(string companyName, Version version);
    }
}