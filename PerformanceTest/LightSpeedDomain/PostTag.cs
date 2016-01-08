namespace LightSpeed.Domain {
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;

    using Mindscape.LightSpeed;

    [Serializable]
    [GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
    [DataObject]
    [Table(IdColumnName = "PostTagId")]
    public class PostTag : Entity<int> {
        #region Fields

        private int? _postId;

        private int? _tagId;

        #endregion

        #region Field attribute and view names

        /// <summary>Identifies the PostId entity attribute.</summary>
        public const string PostIdField = "PostId";

        /// <summary>Identifies the TagId entity attribute.</summary>
        public const string TagIdField = "TagId";

        #endregion

        #region Relationships

        [ReverseAssociation("PostTags")]
        private readonly EntityHolder<Post> _post = new EntityHolder<Post>();

        [ReverseAssociation("PostTags")]
        private readonly EntityHolder<Tag> _tag = new EntityHolder<Tag>();

        #endregion

        #region Properties

        [DebuggerNonUserCode]
        public Post Post
        {
            get
            {
                return Get(_post);
            }
            set
            {
                Set(_post, value);
            }
        }

        [DebuggerNonUserCode]
        public Tag Tag
        {
            get
            {
                return Get(_tag);
            }
            set
            {
                Set(_tag, value);
            }
        }

        [DebuggerNonUserCode]
        public int? PostId
        {
            get
            {
                return Get(ref _postId, "PostId");
            }
            set
            {
                Set(ref _postId, value, "PostId");
            }
        }

        [DebuggerNonUserCode]
        public int? TagId
        {
            get
            {
                return Get(ref _tagId, "TagId");
            }
            set
            {
                Set(ref _tagId, value, "TagId");
            }
        }

        #endregion
    }
}