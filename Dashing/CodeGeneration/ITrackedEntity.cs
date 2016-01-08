namespace Dashing.CodeGeneration {
    using System.Collections.Generic;

    public interface ITrackedEntity {
        void EnableTracking();

        void DisableTracking();

        bool IsTrackingEnabled();

        IEnumerable<string> GetDirtyProperties();

        object GetOldValue(string propertyName);
    }
}