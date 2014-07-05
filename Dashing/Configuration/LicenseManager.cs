namespace Dashing.Configuration {
    using System;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Security.Cryptography;
    using System.Text;

    internal class LicenseManager : ILicenseManager {
        public bool IsLicensed(NameValueCollection settings, Version version) {
            var companyName = settings.Get("DashingCompanyName");
            var licenseKey = settings.Get("DashingLicenseKey");
            if (companyName == null || licenseKey == null || companyName.Trim().Length == 0 || licenseKey.Trim().Length == 0) {
                return false;
            }

            var actualLicense = this.DoGenerateLicense(companyName, version);
            return licenseKey == actualLicense;
        }

        public bool IsLicensed(NameValueCollection settings) {
            return this.IsLicensed(settings, typeof(LicenseManager).Assembly.GetName().Version);
        }

        public string GenerateLicense(string companyName, Version version) {
            return this.DoGenerateLicense(companyName, version);
        }

        private string DoGenerateLicense(string companyName, Version version) {
            string hash = @"
.___  _____                        .__  .__ __              __  .__                                     .___             __           .__                                ___.                           .__  .__                                 ._.
|   |/ ____\  ___.__. ____  __ __  |  | |__|  | __ ____   _/  |_|  |__   ____   _____________  ____   __| _/_ __   _____/  |_  ______ |  |   ____ _____    ______ ____   \_ |__  __ __ ___.__. _____    |  | |__| ____  ____   ____   ______ ____| |
|   \   __\  <   |  |/  _ \|  |  \ |  | |  |  |/ // __ \  \   __\  |  \_/ __ \  \____ \_  __ \/  _ \ / __ |  |  \_/ ___\   __\ \____ \|  | _/ __ \\__  \  /  ___// __ \   | __ \|  |  <   |  | \__  \   |  | |  |/ ___\/ __ \ /    \ /  ___// __ \ |
|   ||  |     \___  (  <_> )  |  / |  |_|  |    <\  ___/   |  | |   Y  \  ___/  |  |_> >  | \(  <_> ) /_/ |  |  /\  \___|  |   |  |_> >  |_\  ___/ / __ \_\___ \\  ___/   | \_\ \  |  /\___  |  / __ \_ |  |_|  \  \__\  ___/|   |  \\___ \\  ___/\|
|___||__|     / ____|\____/|____/  |____/__|__|_ \\___  >  |__| |___|  /\___  > |   __/|__|   \____/\____ |____/  \___  >__|   |   __/|____/\___  >____  /____  >\___  >  |___  /____/ / ____| (____  / |____/__|\___  >___  >___|  /____  >\___  >_
              \/                                \/    \/             \/     \/  |__|                     \/           \/       |__|             \/     \/     \/     \/       \/       \/           \/               \/    \/     \/     \/     \/\/
___________.__                   __                                                                                                                                                                                                                 
\__    ___/|  |__ _____    ____ |  | __  ______                                                                                                                                                                                                     
  |    |   |  |  \\__  \  /    \|  |/ / /  ___/                                                                                                                                                                                                     
  |    |   |   Y  \/ __ \|   |  \    <  \___ \                                                                                                                                                                                                      
  |____|   |___|  (____  /___|  /__|_ \/____  > /\                                                                                                                                                                                                  
                \/     \/     \/     \/     \/  \/                                                                                                                                                                                                  
";

            // generate the license generator
            Func<string, Version, string, string> generator = this.CreateGenerator();
            return generator(companyName, version, hash);
        }

        private Func<string, Version, string, string> CreateGenerator() {
            var parameters = new[] { Expression.Parameter(typeof(string)), Expression.Parameter(typeof(Version)), Expression.Parameter(typeof(string)) };
            var expr = Expression.Call(
                null,
                typeof(Convert).GetMethods().First(p => p.Name == "ToBase64String" && p.GetParameters().Count() == 1),
                new Expression[] {
                                     Expression.Call(
                                         Expression.New(typeof(SHA384CryptoServiceProvider)),
                                         typeof(SHA384CryptoServiceProvider).GetMethods()
                                                                            .First(
                                                                                m =>
                                                                                m.Name == "ComputeHash" && m.GetParameters().Count() == 1 && m.GetParameters()[0].ParameterType == typeof(byte[])),
                                         new[] {
                                                   Expression.Call(
                                                       Expression.MakeMemberAccess(null, typeof(Encoding).GetProperty("UTF32")),
                                                       typeof(Encoding).GetMethods().First(m => m.Name == "GetBytes" && m.GetParameters().Count(p => p.ParameterType == typeof(string)) == 1),
                                                       new[] {
                                                                 Expression.Call(
                                                                     typeof(String).GetMethods().First(m => m.Name == "Concat" && m.GetParameters().Count(p => p.ParameterType == typeof(object)) == 3),
                                                                     new Expression[] {
                                                                                          parameters[0],
                                                                                          Expression.Call(
                                                                                              Expression.MakeMemberAccess(parameters[1], typeof(Version).GetProperty("Major")),
                                                                                              typeof(int).GetMethods().First(m => m.Name == "ToString")),
                                                                                          parameters[2]
                                                                                      })
                                                             })
                                               })
                                 });

            return Expression.Lambda<Func<string, Version, string, string>>(expr, parameters).Compile();
        }
    }
}