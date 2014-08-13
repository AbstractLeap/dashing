using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;

namespace LightSpeed.Domain
{
  [Serializable]
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  [System.ComponentModel.DataObject]
  [Table(IdColumnName="TagId")]
  public partial class Tag : Entity<int>
  {
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

    [System.Diagnostics.DebuggerNonUserCode]
    public EntityCollection<PostTag> PostTags
    {
      get { return Get(_postTags); }
    }


    [System.Diagnostics.DebuggerNonUserCode]
    public string Content
    {
      get { return Get(ref _content, "Content"); }
      set { Set(ref _content, value, "Content"); }
    }

    #endregion
  }





}
