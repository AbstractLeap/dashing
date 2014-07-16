namespace Dashing.Tests.Configuration {
    using System;
    using System.Collections.Specialized;

    using Dashing.Configuration;

    using Xunit;

    public class LicenseManagerTests {
        [Fact]
        public void GeneratesLicenseKey() {
            var licenseManager = new LicenseManager();
            var license = licenseManager.GenerateLicense("Polylytics", new Version(1, 0));
            Assert.Equal("g2wgSAWNqaxwmtvlAXG8On8JXd6XICBDjDR2ksU4V8E/8b3zUrNOU533pBHW5Iur", license);
        }

        [Fact]
        public void NoKeyWithAutoVersionIsNotLicensed() {
            var licenseManager = new LicenseManager();
            Assert.False(licenseManager.IsLicensed(new NameValueCollection()));
        }

        [Fact]
        public void NoKeyWithVersionIsNotLicensed() {
            var licenseManager = new LicenseManager();
            Assert.False(licenseManager.IsLicensed(new NameValueCollection(), new Version(1, 0)));
        }

        [Fact]
        public void IncorrectLicenseDoesNotWork() {
            var licenseManager = new LicenseManager();
            var settings = new NameValueCollection();
            settings.Add("DashingCompanyName", "Polylytics");
            settings.Add("DashingLicenseKey", "g2wgSAWNqaxwmtvlAXG8On8JXd6XICBDjDsdfksU4V8E/8b3zUrNOU533pBHW5Iur");
            Assert.False(licenseManager.IsLicensed(settings, new Version(1, 0)));
        }

        [Fact]
        public void CorrectLicense() {
            var licenseManager = new LicenseManager();
            var settings = new NameValueCollection();
            settings.Add("DashingCompanyName", "Polylytics");
            settings.Add("DashingLicenseKey", "g2wgSAWNqaxwmtvlAXG8On8JXd6XICBDjDR2ksU4V8E/8b3zUrNOU533pBHW5Iur");
            Assert.True(licenseManager.IsLicensed(settings, new Version(1, 0)));
        }

        [Fact]
        public void IncorrectVersionFails() {
            var licenseManager = new LicenseManager();
            var settings = new NameValueCollection();
            settings.Add("DashingCompanyName", "Polylytics");
            settings.Add("DashingLicenseKey", "g2wgSAWNqaxwmtvlAXG8On8JXd6XICBDjDR2ksU4V8E/8b3zUrNOU533pBHW5Iur");
            Assert.False(licenseManager.IsLicensed(settings, new Version(2, 0)));
        }
    }
}