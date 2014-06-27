using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity.Design.PluralizationServices;

namespace TopHat.Tools.ModelGeneration
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
            return this.pluralizationService.Singularize(tableName);
        }
    }
}
