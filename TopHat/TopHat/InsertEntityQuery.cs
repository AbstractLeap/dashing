namespace TopHat {
  using System.Collections.Generic;

  /// <summary>
  ///   The insert entity query.
  /// </summary>
  /// <typeparam name="T">
  /// </typeparam>
  public class InsertEntityQuery<T> : EntityQueryBase<T> {
    /// <summary>
    ///   Initializes a new instance of the <see cref="InsertEntityQuery{T}" /> class.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    public InsertEntityQuery(params T[] entities)
      : base(entities) {}

    /// <summary>
    ///   Initializes a new instance of the <see cref="InsertEntityQuery{T}" /> class.
    /// </summary>
    /// <param name="entities">
    ///   The entities.
    /// </param>
    public InsertEntityQuery(IEnumerable<T> entities)
      : base(entities) {}
  }
}