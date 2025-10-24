using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ASS1.DAL.Models;

[Table("Tag")]
public partial class Tag
{
    [Key]
    [Column("TagID")]
    public int TagId { get; set; }

    [StringLength(50)]
    public string? TagName { get; set; }

    [StringLength(400)]
    public string? Note { get; set; }

    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
