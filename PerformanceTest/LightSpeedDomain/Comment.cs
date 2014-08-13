using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;

namespace LightSpeed.Domain
{
  [Serializable]
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  [System.ComponentModel.DataObject]
  [Table(IdColumnName="CommentId")]
  public partial class Comment : Entity<int>
  {
    #region Fields
  
    [ValidateLength(0, 255)]
    private string _content;
    private System.Nullable<int> _postId;
    private System.Nullable<int> _userId;
    private System.DateTime _commentDate;

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

    [System.Diagnostics.DebuggerNonUserCode]
    public Post Post
    {
      get { return Get(_post); }
      set { Set(_post, value); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public User User
    {
      get { return Get(_user); }
      set { Set(_user, value); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public EntityCollection<Like> Likes
    {
      get { return Get(_likes); }
    }


    [System.Diagnostics.DebuggerNonUserCode]
    public string Content
    {
      get { return Get(ref _content, "Content"); }
      set { Set(ref _content, value, "Content"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> PostId
    {
      get { return Get(ref _postId, "PostId"); }
      set { Set(ref _postId, value, "PostId"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> UserId
    {
      get { return Get(ref _userId, "UserId"); }
      set { Set(ref _userId, value, "UserId"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.DateTime CommentDate
    {
      get { return Get(ref _commentDate, "CommentDate"); }
      set { Set(ref _commentDate, value, "CommentDate"); }
    }

    #endregion
  }





}
