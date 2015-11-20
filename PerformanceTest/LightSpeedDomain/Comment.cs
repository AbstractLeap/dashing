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
    [Table(IdColumnName = "CommentId")]
    public class Comment : Entity<int> {
        #region Fields

        [ValidateLength(0, 255)]
        private string _content;

        private int? _postId;

        private int? _userId;

        private DateTime _commentDate;

        #endregion

        #region Field attribute and view names

        /// <summary>Identifies the Content entity attribute.</summary>
        public const string ContentField = "Content";

        /// <summary>Identifies the PostId entity attribute.</summary>
        public const string PostIdField = "PostId";

        /// <summary>Identifies the UserId entity attribute.</summary>
        public const string UserIdField = "UserId";

        /// <summary>Identifies the CommentDate entity attribute.</summary>
        public const string CommentDateField = "CommentDate";

        #endregion

        #region Relationships

        [ReverseAssociation("Comments")]
        private readonly EntityHolder<Post> _post = new EntityHolder<Post>();

        [ReverseAssociation("Comments")]
        private readonly EntityHolder<User> _user = new EntityHolder<User>();

        [ReverseAssociation("Comment")]
        private readonly EntityCollection<Like> _likes = new EntityCollection<Like>();

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
        public User User
        {
            get
            {
                return Get(_user);
            }
            set
            {
                Set(_user, value);
            }
        }

        [DebuggerNonUserCode]
        public EntityCollection<Like> Likes
        {
            get
            {
                return Get(_likes);
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
        public int? UserId
        {
            get
            {
                return Get(ref _userId, "UserId");
            }
            set
            {
                Set(ref _userId, value, "UserId");
            }
        }

        [DebuggerNonUserCode]
        public DateTime CommentDate
        {
            get
            {
                return Get(ref _commentDate, "CommentDate");
            }
            set
            {
                Set(ref _commentDate, value, "CommentDate");
            }
        }

        #endregion
    }
}