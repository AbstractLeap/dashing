namespace Dashing {
    using System.Collections.Generic;

    /// <summary>
    /// Represents a page of results of a query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T> {
        /// <summary>
        /// The total number of results.
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// The number of results skipped over.
        /// </summary>
        public int Skipped { get; set; }

        /// <summary>
        /// The number of results taken.
        /// </summary>
        public int Taken { get; set; }

        /// <summary>
        /// The enumerable of items in this page.
        /// </summary>
        public IEnumerable<T> Items { get; set; }
    }
}