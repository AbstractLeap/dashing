namespace LightSpeed.Domain {
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;

    using Mindscape.LightSpeed;
    using Mindscape.LightSpeed.Validation;

    [Serializable]
    [GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
    [DataObject]
    [Table(IdColumnName = "TagId")]
    public class Tag : Entity<int> {
        #region Fields

        [ValidateLength(0, 255)]
        private string _content;

        #endregion

        #region Field attribute and view names

        /// <summary>Identifies the Content entity attribute.</summary>
        public const string ContentField = "Content";

        #endregion

        #region Relationships

        [ReverseAssociation("Tag")]
        private readonly EntityCollection<PostTag> _postTags = new EntityCollection<PostTag>();

        #endregion

        #region Properties

        [DebuggerNonUserCode]
        public EntityCollection<PostTag> PostTags
        {
            get
            {
                return Get(_postTags);
            }
        }

        [DebuggerNonUserCode]
        public string Content
        {
            get
            {
                return Get(ref _content, "Content");
            }
            set
            {
                Set(ref _content, value, "Content");
            }
        }

        #endregion
    }
}