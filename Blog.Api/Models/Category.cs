using System;
using System.Collections.Generic;

public class Category: IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<BlogPost> BlogPosts { get; set; }
}