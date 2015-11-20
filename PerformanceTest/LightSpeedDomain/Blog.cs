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
    [Table(IdColumnName = "BlogId")]
    public class Blog : Entity<int> {
        #region Fields

        [ValidateLength(0, 255)]
        private string _title;

        private DateTime _createDate;

        [ValidateLength(0, 255)]
        private string _description;

        #endregion

        #region Field attribute and view names

        /// <summary>Identifies the Title entity attribute.</summary>
        public const string TitleField = "Title";

        /// <summary>Identifies the CreateDate entity attribute.</summary>
        public const string CreateDateField = "CreateDate";

        /// <summary>Identifies the Description entity attribute.</summary>
        public const string DescriptionField = "Description";

        #endregion

        #region Relationships

        [ReverseAssociation("Blog")]
        private readonly EntityCollection<Post> _posts = new EntityCollection<Post>();

        #endregion

        #region Properties

        [DebuggerNonUserCode]
        public EntityCollection<Post> Posts
        {
            get
            {
                return Get(_posts);
            }
        }

        [DebuggerNonUserCode]
        public string Title
        {
            get
            {
                return Get(ref _title, "Title");
            }
            set
            {
                Set(ref _title, value, "Title");
            }
        }

        [DebuggerNonUserCode]
        public DateTime CreateDate
        {
            get
            {
                return Get(ref _createDate, "CreateDate");
            }
            set
            {
                Set(ref _createDate, value, "CreateDate");
            }
        }

        [DebuggerNonUserCode]
        public string Description
        {
            get
            {
                return Get(ref _description, "Description");
            }
            set
            {
                Set(ref _description, value, "Description");
            }
        }

        #endregion
    }
}