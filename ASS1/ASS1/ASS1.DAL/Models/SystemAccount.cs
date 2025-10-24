using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ASS1.DAL.Models;

[Table("SystemAccount")]
public partial class SystemAccount
{
    [Key]
    [Column("AccountID")]
    public short AccountId { get; set; }

    [StringLength(100)]
    public string? AccountName { get; set; }

    [StringLength(70)]
    public string? AccountEmail { get; set; }

    public int? AccountRole { get; set; }

    [StringLength(70)]
    public string? AccountPassword { get; set; }

    [InverseProperty("CreatedBy")]
    public virtual ICollection<NewsArticle> NewsArticles { get; set; } = new List<NewsArticle>();
}
