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
    [Table(IdColumnName = "UserId")]
    public class User : Entity<int> {
        #region Fields

        [ValidateLength(0, 255)]
        private string _username;

        [ValidateEmailAddress]
        [ValidateLength(0, 255)]
        private string _emailAddress;

        [ValidateLength(0, 255)]
        private string _password;

        private bool _isEnabled;

        private decimal _heightInMeters;

        #endregion

        #region Field attribute and view names

        /// <summary>Identifies the Username entity attribute.</summary>
        public const string UsernameField = "Username";

        /// <summary>Identifies the EmailAddress entity attribute.</summary>
        public const string EmailAddressField = "EmailAddress";

        /// <summary>Identifies the Password entity attribute.</summary>
        public const string PasswordField = "Password";

        /// <summary>Identifies the IsEnabled entity attribute.</summary>
        public const string IsEnabledField = "IsEnabled";

        /// <summary>Identifies the HeightInMeters entity attribute.</summary>
        public const string HeightInMetersField = "HeightInMeters";

        #endregion

        #region Relationships

        [ReverseAssociation("User")]
        private readonly EntityCollection<Comment> _comments = new EntityCollection<Comment>();

        [ReverseAssociation("User")]
        private readonly EntityCollection<Like> _likes = new EntityCollection<Like>();

        [ReverseAssociation("Author")]
        private readonly EntityCollection<Post> _posts = new EntityCollection<Post>();

        #endregion

        #region Properties

        [DebuggerNonUserCode]
        public EntityCollection<Comment> Comments
        {
            get
            {
                return Get(_comments);
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
        public EntityCollection<Post> Posts
        {
            get
            {
                return Get(_posts);
            }
        }

        [DebuggerNonUserCode]
        public string Username
        {
            get
            {
                return Get(ref _username, "Username");
            }
            set
            {
                Set(ref _username, value, "Username");
            }
        }

        [DebuggerNonUserCode]
        public string EmailAddress
        {
            get
            {
                return Get(ref _emailAddress, "EmailAddress");
            }
            set
            {
                Set(ref _emailAddress, value, "EmailAddress");
            }
        }

        [DebuggerNonUserCode]
        public string Password
        {
            get
            {
                return Get(ref _password, "Password");
            }
            set
            {
                Set(ref _password, value, "Password");
            }
        }

        [DebuggerNonUserCode]
        public bool IsEnabled
        {
            get
            {
                return Get(ref _isEnabled, "IsEnabled");
            }
            set
            {
                Set(ref _isEnabled, value, "IsEnabled");
            }
        }

        [DebuggerNonUserCode]
        public decimal HeightInMeters
        {
            get
            {
                return Get(ref _heightInMeters, "HeightInMeters");
            }
            set
            {
                Set(ref _heightInMeters, value, "HeightInMeters");
            }
        }

        #endregion
    }
}