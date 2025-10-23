using Microsoft.EntityFrameworkCore;
using BusinessObjects;  // chứa các lớp entity

namespace DataAccessObjects
{
    public class FUNewsContext : DbContext
    {
        // Constructor nhận tùy chọn để tích hợp DI
        public FUNewsContext(DbContextOptions<FUNewsContext> options) : base(options) { }

        // Nếu muốn dùng new FUNewsContext() trực tiếp (không DI), có thể override OnConfiguring:
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Sử dụng chuỗi kết nối mặc định (phòng trường hợp không dùng DI)
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:FUNewsManagementDB");
            }
        }

        // DbSet cho từng bảng
        public DbSet<SystemAccount> SystemAccounts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NewsTag> NewsTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Khóa chính phức hợp cho NewsTag (NewsArticleId + TagId)
            modelBuilder.Entity<NewsTag>().HasKey(nt => new { nt.NewsArticleId, nt.TagId });

            // Quan hệ 1-n giữa Category và NewsArticle
            modelBuilder.Entity<NewsArticle>()
                .HasOne(c => c.Category)
                .WithMany(n => n.NewsArticles)
                .HasForeignKey(n => n.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Category nếu có News

            // Quan hệ 1-n giữa SystemAccount và NewsArticle (CreatedBy)
            modelBuilder.Entity<NewsArticle>()
                .HasOne(a => a.CreatedBy)
                .WithMany(u => u.CreatedNews)   // CreatedNews là ICollection<NewsArticle> trong SystemAccount
                .HasForeignKey(n => n.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ 1-n giữa SystemAccount và NewsArticle (UpdatedBy)
            modelBuilder.Entity<NewsArticle>()
                .HasOne(a => a.UpdatedBy)
                .WithMany(u => u.UpdatedNews)   // UpdatedNews là ICollection<NewsArticle> trong SystemAccount
                .HasForeignKey(n => n.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Quan hệ tự tham chiếu Category (Parent-Child)
            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
