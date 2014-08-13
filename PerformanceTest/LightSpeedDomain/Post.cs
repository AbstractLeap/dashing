using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;

namespace LightSpeed.Domain
{
  [Serializable]
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  [System.ComponentModel.DataObject]
  [Table(IdColumnName="PostId")]
  public partial class Post : Entity<int>
  {
    #region Fields
  
    [ValidateLength(0, 255)]
    private string _title;
    [ValidateLength(0, 255)]
    private string _content;
    private decimal _rating;
    private System.Nullable<int> _authorId;
    private System.Nullable<int> _blogId;
    private bool _doNotMap;

    #endregion
    
    #region Field attribute and view names
    
    /// <summary>Identifies the Title entity attribute.</summary>
    public const string TitleField = "Title";
    /// <summary>Identifies the Content entity attribute.</summary>
    public const string ContentField = "Content";
    /// <summary>Identifies the Rating entity attribute.</summary>
    public const string RatingField = "Rating";
    /// <summary>Identifies the AuthorId entity attribute.</summary>
    public const string AuthorIdField = "AuthorId";
    /// <summary>Identifies the BlogId entity attribute.</summary>
    public const string BlogIdField = "BlogId";
    /// <summary>Identifies the DoNotMap entity attribute.</summary>
    public const string DoNotMapField = "DoNotMap";


    #endregion
    
    #region Relationships

    [ReverseAssociation("Post")]
    private readonly EntityCollection<Comment> _comments = new EntityCollection<Comment>();
    [ReverseAssociation("Posts")]
    private readonly EntityHolder<User> _author = new EntityHolder<User>();
    [ReverseAssociation("Posts")]
    private readonly EntityHolder<Blog> _blog = new EntityHolder<Blog>();
    [ReverseAssociation("Post")]
    private readonly EntityCollection<PostTag> _postTags = new EntityCollection<PostTag>();


    #endregion
    
    #region Properties

    [System.Diagnostics.DebuggerNonUserCode]
    public EntityCollection<Comment> Comments
    {
      get { return Get(_comments); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public User Author
    {
      get { return Get(_author); }
      set { Set(_author, value); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public Blog Blog
    {
      get { return Get(_blog); }
      set { Set(_blog, value); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public EntityCollection<PostTag> PostTags
    {
      get { return Get(_postTags); }
    }


    [System.Diagnostics.DebuggerNonUserCode]
    public string Title
    {
      get { return Get(ref _title, "Title"); }
      set { Set(ref _title, value, "Title"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public string Content
    {
      get { return Get(ref _content, "Content"); }
      set { Set(ref _content, value, "Content"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public decimal Rating
    {
      get { return Get(ref _rating, "Rating"); }
      set { Set(ref _rating, value, "Rating"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> AuthorId
    {
      get { return Get(ref _authorId, "AuthorId"); }
      set { Set(ref _authorId, value, "AuthorId"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.Nullable<int> BlogId
    {
      get { return Get(ref _blogId, "BlogId"); }
      set { Set(ref _blogId, value, "BlogId"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public bool DoNotMap
    {
      get { return Get(ref _doNotMap, "DoNotMap"); }
      set { Set(ref _doNotMap, value, "DoNotMap"); }
    }

    #endregion
  }





}
