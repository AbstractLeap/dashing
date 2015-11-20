namespace LightSpeed.Domain {
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;

    using Mindscape.LightSpeed;

    [Serializable]
    [GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
    [DataObject]
    [Table(IdColumnName = "LikeId")]
    public class Like : Entity<int> {
        #region Fields

        private int? _userId;

        private int? _commentId;

        #endregion

        #region Field attribute and view names

        /// <summary>Identifies the UserId entity attribute.</summary>
        public const string UserIdField = "UserId";

        /// <summary>Identifies the CommentId entity attribute.</summary>
        public const string CommentIdField = "CommentId";

        #endregion

        #region Relationships

        [ReverseAssociation("Likes")]
        private readonly EntityHolder<User> _user = new EntityHolder<User>();

        [ReverseAssociation("Likes")]
        private readonly EntityHolder<Comment> _comment = new EntityHolder<Comment>();

        #endregion

        #region Properties

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
        public Comment Comment
        {
            get
            {
                return Get(_comment);
            }
            set
            {
                Set(_comment, value);
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
        public int? CommentId
        {
            get
            {
                return Get(ref _commentId, "CommentId");
            }
            set
            {
                Set(ref _commentId, value, "CommentId");
            }
        }

        #endregion
    }
}