using Microsoft.EntityFrameworkCore;
using OcrApi.Models;

namespace OcrApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // เพิ่ม DbSet properties สำหรับ models ที่ต้องการเก็บใน database
        public DbSet<CustomsReceipt> CustomsReceipts { get; set; }
        public DbSet<OcrResult> OcrResults { get; set; }
        public DbSet<FinancialDocumentData> FinancialDocuments { get; set; }
        public DbSet<ExpenseItem> ExpenseItems { get; set; }
        public DbSet<ReferenceInfo> ReferenceInfos { get; set; }
        public DbSet<ItemsInfo> ItemsInfos { get; set; }
        public DbSet<TotalInfo> TotalInfos { get; set; }
        public DbSet<SignInfo> SignInfos { get; set; }
        public DbSet<DocumentData> DocumentData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // กำหนดความสัมพันธ์ระหว่าง CustomsReceipt และ ReferenceInfo
            modelBuilder.Entity<CustomsReceipt>()
                .HasOne(c => c.Reference)
                .WithOne()
                .HasForeignKey<ReferenceInfo>(r => r.CustomsReceiptId);

            // กำหนดความสัมพันธ์ระหว่าง CustomsReceipt และ ItemsInfo
            modelBuilder.Entity<CustomsReceipt>()
                .HasOne(c => c.Items)
                .WithOne()
                .HasForeignKey<ItemsInfo>(i => i.CustomsReceiptId);

            // กำหนดความสัมพันธ์ระหว่าง CustomsReceipt และ TotalInfo
            modelBuilder.Entity<CustomsReceipt>()
                .HasOne(c => c.Total)
                .WithOne()
                .HasForeignKey<TotalInfo>(t => t.CustomsReceiptId);

            // กำหนดความสัมพันธ์ระหว่าง CustomsReceipt และ SignInfo
            modelBuilder.Entity<CustomsReceipt>()
                .HasOne(c => c.Sign)
                .WithOne()
                .HasForeignKey<SignInfo>(s => s.CustomsReceiptId);

            // กำหนดความสัมพันธ์ระหว่าง FinancialDocumentData และ ExpenseItem
            modelBuilder.Entity<FinancialDocumentData>()
                .HasMany<ExpenseItem>()
                .WithOne()
                .HasForeignKey(e => e.FinancialDocumentDataId);
        }
    }
}
