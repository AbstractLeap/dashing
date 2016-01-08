namespace Dashing.Tests.Engine.DML.TestDomains.MultipleFetchManyWithThenFetchAndOneToOne {
    using System.Collections.Generic;

    //patient.Applications = await session.Query<Application>().Fetch(a => a.Patient).Fetch(a => a.Provider.Organisation)
    // .FetchMany(a => a.Plans).ThenFetch(p => p.Product.Product)
    // .FetchMany(a => a.ApplicationReferences).ThenFetch(r => r.ClientReference).Where(p => p.Patient == patient).ToArrayAsync();

    public class Application {
        public virtual int ApplicationId { get; set; }

        public virtual Provider Provider { get; set; }

        public virtual Person Person { get; set; }

        public virtual IList<Plan> Plans { get; set; }

        public virtual IList<Reference> ApplicationReferences { get; set; }
    }

    public class Reference {
        public virtual int ReferenceId { get; set; }

        public virtual ParentReference ParentReference { get; set; }

        public virtual Application Application { get; set; }
    }

    public class ParentReference {
        public virtual int ParentReferenceId { get; set; }

        public string Question { get; set; }
    }

    public class Person {
        public virtual int PersonId { get; set; }

        public string Name { get; set; }
    }

    public class Plan {
        public virtual int PlanId { get; set; }

        public virtual ProductInstance ProductInstance { get; set; }

        public virtual Application Application { get; set; }
    }

    public class ProductInstance {
        public virtual int ProductInstanceId { get; set; }

        public virtual Product Product { get; set; }
    }

    public class Product {
        public virtual int ProductId { get; set; }

        public virtual string Name { get; set; }
    }

    public class Provider {
        public virtual int ProviderId { get; set; }

        public virtual Organisation Organisation { get; set; }
    }

    public class Organisation {
        public virtual int OrganisationId { get; set; }

        public virtual string Foo { get; set; }

        public virtual Provider Provider { get; set; }
    }
}