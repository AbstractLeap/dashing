namespace Dashing.Versioning {
    using System;

    public interface IVersionedEntity<TUserId> {
        TUserId SessionUser { get; set; }

        TUserId CreatedBy { get; set; }

        DateTime SysStartTime { get; set; }

        DateTime SysEndTime { get; set; }
    }
}