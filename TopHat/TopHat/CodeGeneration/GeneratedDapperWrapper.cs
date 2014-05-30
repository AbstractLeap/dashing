namespace TopHat.CodeGeneration {
    using System;
    using System.Collections.Generic;
    using System.Data;

    using Dapper;

    using TopHat.Engine;

    public class GeneratedDapperWrapper {
        private static readonly IDictionary<int, Delegate> PostDelegates = new Dictionary<int, Delegate> { { 1, new DelegateQuery<BlogPost>(PostAuthor) } };

        private static readonly IDictionary<int, Delegate> AuthorDelegates = new Dictionary<int, Delegate> { { 1, new DelegateQuery<Author>(AuthorCountry) } };

        private static readonly IDictionary<Type, IDictionary<int, Delegate>> TypeDelegates = new Dictionary<Type, IDictionary<int, Delegate>> {
                                                                                                                                                   { typeof(BlogPost), PostDelegates },
                                                                                                                                                   { typeof(Author), AuthorDelegates }
                                                                                                                                               };

        public static IEnumerable<T> Query<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn) {
            var meth = (DelegateQuery<T>)TypeDelegates[typeof(T)][1];
            return meth(result, query, conn);
        }

        public delegate IEnumerable<T> DelegateQuery<T>(SqlWriterResult result, SelectQuery<T> query, IDbConnection conn);

        public static IEnumerable<BlogPost> PostAuthor(SqlWriterResult result, SelectQuery<BlogPost> query, IDbConnection conn) {
            if (query.IsTracked) {
                Func<BlogPostTracked, Author, BlogPostTracked> mapper = PostAuthorMappingTracked;
                return conn.Query(result.Sql, mapper, result.Parameters);
            }
            else {
                Func<BlogPostFk, Author, BlogPostFk> mapper = PostAuthorMappingFk;
                return conn.Query(result.Sql, mapper, result.Parameters);
            }
        }

        public static IEnumerable<Author> AuthorCountry(SqlWriterResult result, SelectQuery<Author> query, IDbConnection conn) {
            Func<Author, Country, Author> mapper = AuthorCountryMapping;
            return conn.Query(result.Sql, mapper, result.Parameters);
        }

        public static BlogPostFk PostAuthorMappingFk(BlogPostFk post, Author author) {
            post.Author = author;
            return post;
        }

        public static BlogPostTracked PostAuthorMappingTracked(BlogPostTracked post, Author author)
        {
            post.Author = author;
            return post;
        }

        public static Author AuthorCountryMapping(Author author, Country country) {
            author.Country = country;
            return author;
        }
    }

    public class BlogPostFk : BlogPost {}

    public class BlogPostTracked : BlogPostFk { }

    public class BlogPost {
        public Author Author { get; set; }
    }

    public class Author {
        public Country Country { get; set; }
    }

    public class Country {}
}