using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;

namespace LightSpeed.Domain
{
  [Serializable]
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  [System.ComponentModel.DataObject]
  [Table(IdColumnName="BlogId")]
  public partial class Blog : Entity<int>
  {
    #region Fields
  
    [ValidateLength(0, 255)]
    private string _title;
    private System.DateTime _createDate;
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

    [System.Diagnostics.DebuggerNonUserCode]
    public EntityCollection<Post> Posts
    {
      get { return Get(_posts); }
    }


    [System.Diagnostics.DebuggerNonUserCode]
    public string Title
    {
      get { return Get(ref _title, "Title"); }
      set { Set(ref _title, value, "Title"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public System.DateTime CreateDate
    {
      get { return Get(ref _createDate, "CreateDate"); }
      set { Set(ref _createDate, value, "CreateDate"); }
    }

    [System.Diagnostics.DebuggerNonUserCode]
    public string Description
    {
      get { return Get(ref _description, "Description"); }
      set { Set(ref _description, value, "Description"); }
    }

    #endregion
  }





}
