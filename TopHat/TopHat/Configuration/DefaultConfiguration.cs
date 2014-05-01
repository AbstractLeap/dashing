using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopHat.Configuration
{
    public class DefaultConfiguration : Configuration
    {
        public DefaultConfiguration()
            : base()
        {
            this.Conventions.AlwaysTrackEntities = false;
            this.Conventions.PrimaryKeysDatabaseGeneratedByDefault = true;
            this.Conventions.GenerateIndexesOnForeignKeysByDefault = true;
            this.Conventions.PluraliseNamesByDefault = true;
            this.Conventions.DefaultStringLength = 255;
            this.Conventions.DefaultDecimalPrecision = 18;
            this.Conventions.DefaultDecimalScale = 10;

            this.Conventions.PrimaryKeyIdentifier = p => p.Name == p.DeclaringType.Name + "Id";
        }

        public override IConfiguration Configure()
        {
            return this;
        }
    }
}