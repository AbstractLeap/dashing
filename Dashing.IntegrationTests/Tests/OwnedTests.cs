using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dashing.IntegrationTests.Tests
{
    using System.Data.SqlClient;

    using Dashing.Configuration;
    using Dashing.Engine.Dialects;

    using Xunit;

    public class OwnedTests : IClassFixture<OwnedConfig>
    {
        private readonly OwnedConfig ownedConfig;

        private SqlDatabase database;

        public OwnedTests(OwnedConfig ownedConfig) {
            this.ownedConfig = ownedConfig;
            this.database = new SqlDatabase(ownedConfig, SqlClientFactory.Instance, "Server=localhost;Trusted_Connection=True;MultipleActiveResultSets=True", new SqlServer2012Dialect());
        }

        [Fact]
        public async Task RoundTripWorks() {
            using (var session = this.database.BeginSession()) {
                var owner = new Owner {
                                          Name = "Foo",
                                          Owned = new Owned {
                                                                X = 3,
                                                                Y = 5
                                                            }
                                      };
                await session.InsertAsync(owner);
                var ownerSelected = await session.Query<Owner>()
                                                 .SingleAsync(o => o.Id == owner.Id);
                Assert.Equal(3, ownerSelected.Owned.X);
            }
        }
    }

       public class OwnedConfig : BaseConfiguration {
            public OwnedConfig() {
                this.Add<Owner>();
                this.Add<Owned>();
            }
        }

        class Owner {
            public int Id { get; set; }

            public string Name { get; set; }

            public Owned Owned { get; set; }
        }

        class Owned {
            public int X { get; set; }

            public int Y { get; set; }
        }
}
