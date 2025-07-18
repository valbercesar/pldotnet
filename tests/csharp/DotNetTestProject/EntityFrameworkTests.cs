using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlDotNET.Common;

namespace EFCoreTest
{
    [Table("test_entity_framework", Schema = "public")]
    public class TestEntityFramework
    {
        [Key, Column("id")]
        public int Id { get; set; }

        [Column("name", TypeName = "varchar(100)")]
        public string Name { get; set; } = null!;

        [Column("category", TypeName = "varchar(100)")]
        public string Category { get; set; } = null!;

        [Column("price", TypeName = "money")]
        public decimal Price { get; set; }

        [Column("created_at", TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }

    [Table("test_categories", Schema = "public")]
    public class TestCategory
    {
        [Key, Column("category")]
        public string Category { get; set; } = null!;

        [Column("sort_order")]
        public int SortOrder { get; set; }
    }

    public class TestEntitiesContext : DbContext
    {
        public DbSet<TestEntityFramework> TestEntities { get; set; } = null!;
        public DbSet<TestCategory> Categories { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseNpgsql("");
    }

    public static class TestEntityFunctions
    {
        static readonly TestEntitiesContext _context = new();

        private static TResult Do<TResult>(Func<IQueryable<TestEntityFramework>, TResult> query)
        {
            return query(_context.TestEntities);
        }

        // 1. total count
        public static int GetTotalCount() =>
            Do(c => c.Count());

        // 2. sum of price
        public static decimal GetSumPrice() =>
            Do(c => c.Sum(e => e.Price));

        // 3. min price
        public static decimal GetMinPrice() =>
            Do(c => c.Min(e => e.Price));

        // 4. max price
        public static decimal GetMaxPrice() =>
            Do(c => c.Max(e => e.Price));

        // 5. avg id
        public static double GetAverageID() =>
            Do(c => c.Average(e => e.Id));

        // 6. any inactive?
        public static bool GetAnyInactive() =>
            Do(c => c.Any(e => !e.IsActive));

        // 7. all active?
        public static bool GetAllActive() =>
            Do(c => c.All(e => e.IsActive));

        // 8. first name by ID asc
        public static string GetFirstName() =>
            Do(c => c.OrderBy(e => e.Id).Select(e => e.Name).First());

        // 9. first‑or‑default name by ID asc
        public static string? GetFirstOrDefaultName() =>
            Do(c => c.OrderBy(e => e.Id).Select(e => e.Name).FirstOrDefault());

        // 10. single ID by name
        public static int GetSingleByName(string name) =>
            Do(c => c.Single(e => e.Name == name).Id);

        // 11. single‑or‑default ID by name
        public static int? GetSingleOrDefaultByName(string name) =>
            Do(c => c.Where(e => e.Name == name).Select(e => e.Id).SingleOrDefault());

        // 12. distinct category count
        public static int GetDistinctCategoriesCount() =>
            Do(c => c.Select(e => e.Category).Distinct().Count());

        // 13. category grouping: "Category 1:2,Category 2:2,…"
        public static string GetCategoryCounts() =>
            Do(c =>
                string.Join(
                    ",",
                    c.GroupBy(e => e.Category)
                     .OrderBy(g => g.Key)
                     .Select(g => $"{g.Key}:{g.Count()}")
                )
            );

        // 14. skip N → remaining count
        public static int GetSkipCount(int skip) =>
            Do(c => c.OrderBy(e => e.Id).Skip(skip).Count());

        // 15. take N → count
        public static int GetTakeCount(int take) =>
            Do(c => c.OrderBy(e => e.Id).Take(take).Count());

        // 16. join count
        public static int GetJoinedCount() =>
            Do(c => c.Join(
                        _context.Categories,
                        e => e.Category,
                        c => c.Category,
                        (e, c) => e
                    ).Count()
            );

        public static void DeleteFirstEntity()
        {
            using var ctx = new TestEntitiesContext();
            ctx.Database.AutoTransactionsEnabled = false;
            var first = ctx.TestEntities.OrderBy(e => e.Id).FirstOrDefault();
            if (first != null)
            {
                ctx.TestEntities.Remove(first);
                ctx.SaveChanges();
            }
        }

        public static void DeleteEntitiesByIds(Array idsArray)
        {
            var ids = idsArray.Cast<int>().ToArray();
            using var ctx = new TestEntitiesContext();
            ctx.Database.AutoTransactionsEnabled = false;
            var toDelete = ctx.TestEntities.Where(e => ids.Contains(e.Id)).ToList();
            if (toDelete.Count > 0)
            {
                ctx.TestEntities.RemoveRange(toDelete);
                ctx.SaveChanges();
            }
        }

        public static void UpdateEntityName(int? id, string newName)
        {
            using var ctx = new TestEntitiesContext();
            ctx.Database.AutoTransactionsEnabled = false;
            var entity = ctx.TestEntities.SingleOrDefault(e => e.Id == id);
            if (entity == null) return;
            entity.Name = newName;
            ctx.SaveChanges();
        }

        public static void DoublePriceByCategories(Array categoriesArray)
        {
            var categories = categoriesArray.Cast<string>().ToArray();
            using var ctx = new TestEntitiesContext();
            ctx.Database.AutoTransactionsEnabled = false;
            var toUpdate = ctx.TestEntities.Where(e => categories.Contains(e.Category)).ToList();
            foreach (var e in toUpdate)
                e.Price *= 2;
            if (toUpdate.Count > 0)
                ctx.SaveChanges();
        }

        public static void InsertEntity(
           string name, string category, decimal? price, DateTime? createdAt, bool? isActive
        )
        {
            using var ctx = new TestEntitiesContext();
            ctx.Database.AutoTransactionsEnabled = false;
            int maxId = ctx.TestEntities.Any() ? ctx.TestEntities.Max(e => e.Id) : 0;
            var ent = new TestEntityFramework
            {
                Name = name,
                Category = category,
                Price = price ?? 0m,
                CreatedAt = createdAt ?? DateTime.Now,
                IsActive = isActive ?? false
            };
            ctx.TestEntities.Add(ent);
            ctx.SaveChanges();
        }

        public static void InsertEntitiesRange(
            Array namesArray, Array categoriesArray, Array pricesArray, Array createdAtArray, Array isActiveArray
        )
        {
            var names      = namesArray     .Cast<string>()   .ToArray();
            var categories = categoriesArray.Cast<string>()   .ToArray();
            var prices     = pricesArray    .Cast<decimal?>()
                                            .Select(p => p ?? 0m)
                                            .ToArray();
            var createdAts = createdAtArray .Cast<DateTime?>()
                                            .Select(d => d ?? DateTime.Now)
                                            .ToArray();
            var isActives  = isActiveArray  .Cast<bool?>()
                                            .Select(b => b ?? false)
                                            .ToArray();
            var list = new List<TestEntityFramework>();
            for (int i = 0; i < names.Length; i++)
            {
                list.Add(new TestEntityFramework {
                    Name      = names[i],
                    Category  = categories[i],
                    Price     = prices[i],
                    CreatedAt = createdAts[i],
                    IsActive  = isActives[i]
                });
            }
            using var ctx = new TestEntitiesContext();
            ctx.Database.AutoTransactionsEnabled = false;
            ctx.TestEntities.AddRange(list);
            ctx.SaveChanges();
        }
    }
}
