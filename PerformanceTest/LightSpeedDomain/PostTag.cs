using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;

namespace LightSpeed.Domain
{
  [Serializable]
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  [System.ComponentModel.DataObject]
  [Table(IdColumnName="PostTagId")]
  public partial class PostTag : Entity<int>
  {
    #region Fields
  
    private System.Nullable<int> _postId;
    private System.Nullable<int> _tagId;

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

    [System.Diagnostics.DebuggerNonUserCode]
    public Post Post
    {
      get { return Get(_post); }
      set { Set(_post, value); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public Tag Tag
    {
      get { return Get(_tag); }
      set { Set(_tag, value); }
    }


    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> PostId
    {
      get { return Get(ref _postId, "PostId"); }
      set { Set(ref _postId, value, "PostId"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> TagId
    {
      get { return Get(ref _tagId, "TagId"); }
      set { Set(ref _tagId, value, "TagId"); }
    }

    #endregion
  }





}
