using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopHat.Configuration.Mapper;

namespace TopHat.Configuration
{
    public abstract class Configuration : IConfiguration
    {
        protected Mapper.Mapper mapper;

        public Configuration()
        {
            this.mapper = new Mapper.Mapper(this);
            this.Maps = new Dictionary<Type, IMap>();
        }

        public bool AlwaysTrackEntities { get; set; }

        public bool PrimaryKeysDatabaseGeneratedByDefault { get; set; }

        public bool GenerateIndexesOnForeignKeysByDefault { get; set; }

        public bool PluraliseNamesByDefault { get; set; }

        public int DefaultDecimalPrecision { get; set; }

        public int DefaultDecimalScale { get; set; }

        public int DefaultStringLength { get; set; }

        public IDictionary<Type, IMap> Maps { get; private set; }

        public abstract Mapper.Mapper Configure();

        public string DefaultSchema { get; set; }
    }
}