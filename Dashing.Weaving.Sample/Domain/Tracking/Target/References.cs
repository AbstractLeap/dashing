using System;

namespace Dashing.Weaving.Sample.Domain.Tracking.Target {

    public class References {
        protected bool __isSetLogging;

        protected bool __GuidPk_IsSet;

        public Guid? GuidPkId;

        protected bool __isTracking;

        protected bool __GuidPk_IsDirty;

        protected GuidPk __GuidPk_OldValue;

        public int Id { get; set; }

        private GuidPk _guidPk;

        public GuidPk GuidPk
        {
            get
            {
                if (_guidPk == null && GuidPkId.HasValue) {

                    _guidPk = new GuidPk {
                                             Id = GuidPkId.Value
                                         };
                }

                return _guidPk;
            }

            set
            {
                if (__isTracking && !__GuidPk_IsDirty && (_guidPk == null && value != null && (!GuidPkId.HasValue || !GuidPkId.Value.Equals(value.Id)) || (value == null && GuidPkId.HasValue) || (_guidPk != null && !_guidPk.Equals(value)))) {
                    if (_guidPk == null && GuidPkId.HasValue) {
                        __GuidPk_OldValue = new GuidPk {
                                                           Id = GuidPkId.Value
                                                       };
                    }
                    else {
                        __GuidPk_OldValue = GuidPk;
                    }

                    __GuidPk_IsDirty = true;
                }

                if (__isSetLogging) {
                    __GuidPk_IsSet = true;
                }

                GuidPkId = null;
                _guidPk = value;
            }
        }
    }
}