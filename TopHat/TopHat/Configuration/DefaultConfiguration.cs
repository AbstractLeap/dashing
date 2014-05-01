using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration
{
    public class DefaultConfiguration : Configuration
    {
        public override IConfiguration Configure()
        {
            this.AlwaysTrackEntities = false;
            this.PrimaryKeysDatabaseGeneratedByDefault = true;
            this.GenerateIndexesOnForeignKeysByDefault = true;
            this.PluraliseNamesByDefault = true;
            this.DefaultStringLength = 255;
            this.DefaultDecimalPrecision = 18;
            this.DefaultDecimalScale = 10;

            return this;
        }
    }
}