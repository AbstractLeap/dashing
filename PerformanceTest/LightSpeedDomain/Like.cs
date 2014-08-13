using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;

namespace LightSpeed.Domain
{
  [Serializable]
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  [System.ComponentModel.DataObject]
  [Table(IdColumnName="LikeId")]
  public partial class Like : Entity<int>
  {
    #region Fields
  
    private System.Nullable<int> _userId;
    private System.Nullable<int> _commentId;

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

    [System.Diagnostics.DebuggerNonUserCode]
    public User User
    {
      get { return Get(_user); }
      set { Set(_user, value); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public Comment Comment
    {
      get { return Get(_comment); }
      set { Set(_comment, value); }
    }


    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> UserId
    {
      get { return Get(ref _userId, "UserId"); }
      set { Set(ref _userId, value, "UserId"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> CommentId
    {
      get { return Get(ref _commentId, "CommentId"); }
      set { Set(ref _commentId, value, "CommentId"); }
    }

    #endregion
  }





}
