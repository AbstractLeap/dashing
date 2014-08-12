using System;

using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Validation;
using Mindscape.LightSpeed.Linq;

namespace LightSpeed.Domain
{


  /// <summary>
  /// Provides a strong-typed unit of work for working with the Test model.
  /// </summary>
  [System.CodeDom.Compiler.GeneratedCode("LightSpeedModelGenerator", "1.0.0.0")]
  public partial class TestUnitOfWork : Mindscape.LightSpeed.UnitOfWork
  {

    public System.Linq.IQueryable<User> Users
    {
      get { return this.Query<User>(); }
    }
    
    public System.Linq.IQueryable<Tag> Tags
    {
      get { return this.Query<Tag>(); }
    }
    
    public System.Linq.IQueryable<PostTag> PostTags
    {
      get { return this.Query<PostTag>(); }
    }
    
    public System.Linq.IQueryable<Blog> Blogs
    {
      get { return this.Query<Blog>(); }
    }
    
    public System.Linq.IQueryable<Like> Likes
    {
      get { return this.Query<Like>(); }
    }
    
    public System.Linq.IQueryable<Post> Posts
    {
      get { return this.Query<Post>(); }
    }
    
    public System.Linq.IQueryable<Comment> Comments
    {
      get { return this.Query<Comment>(); }
    }
    
  }

}
