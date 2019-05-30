//namespace PerformanceTest.Tests.NHibernate {
//    using System;
//    using System.Globalization;

//    using FluentNHibernate;
//    using FluentNHibernate.Automapping;
//    using FluentNHibernate.Cfg;
//    using FluentNHibernate.Cfg.Db;
//    using FluentNHibernate.Conventions;
//    using FluentNHibernate.Conventions.Instances;

//    using global::Dashing.Extensions;

//    using global::NHibernate;
//    using global::NHibernate.Cfg;
//    using global::NHibernate.Tool.hbm2ddl;

//    using PerformanceTest.Domain;

//    public class Nh {
//        public static ISessionFactory SessionFactory { get; set; }

//        static Nh() {
//            SessionFactory = BuildSessionFactory();
//        }

//        private static AutoPersistenceModel CreateMappings() {
//            var mappings = AutoMap.AssemblyOf<Post>(new Config());
//            mappings.Conventions.Add(new TableNameConvention(), new FKConvention());
//            return mappings;
//        }

//        private static void BuildSchema(Configuration config) {
//            new SchemaExport(config).Create(false, false);
//        }

//        private static ISessionFactory BuildSessionFactory() {
//            AutoPersistenceModel model = CreateMappings();

//            return
//                Fluently.Configure()
//                        .Database(MsSqlConfiguration.MsSql2012.ConnectionString(Program.ConnectionString.ConnectionString))
//                        .Mappings(m => m.AutoMappings.Add(CreateMappings))
//                        .ExposeConfiguration(BuildSchema)
//                        .BuildSessionFactory();
//        }

//        private class Config : DefaultAutomappingConfiguration {
//            public override bool ShouldMap(Type type) {
//                return type.Namespace == "PerformanceTest.Domain";
//            }

//            public override bool IsId(Member member) {
//                return member.Name == member.DeclaringType.Name + "Id";
//            }
//        }

//        private class TableNameConvention : IClassConvention {
//            public void Apply(IClassInstance instance) {
//                string typeName = instance.EntityType.Name;
//                instance.Table(typeName.Pluralize());
//            }
//        }

//        private class FKConvention : ForeignKeyConvention {
//            protected override string GetKeyName(Member property, Type type) {
//                if (property == null) {
//                    return type.Name + "Id";
//                }

//                return property.Name + "Id";
//            }
//        }
//    }
//}