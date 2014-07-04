using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Design.PluralizationServices;

namespace Dashing.Tools.ModelGeneration
{
    public class DefaultConvention : IConvention
    {
        PluralizationService pluralizationService;

        public DefaultConvention()
        {
            this.pluralizationService = PluralizationService.CreateService(new System.Globalization.CultureInfo("en-gb"));
        }

        public string ClassNameForTable(string tableName)
        {
            var className = this.pluralizationService.Singularize(tableName);
            
            // capitalise - helps with case-insensitivity of MySql on Windows
            return className[0].ToString().ToUpper() + className.Substring(1);
        }
    }
}
