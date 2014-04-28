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
        }

        public bool AlwaysTrackEntities { get; set; }

        public IMapping Mapping { get; private set; }

        public abstract Mapper.Mapper Configure();

        public string DefaultSchema { get; set; }
    }
}