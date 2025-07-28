using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlDotNET.Common;

namespace EFCoreTest
{
    /// <summary>
    /// Represents a test entity for Entity Framework testing, mapped to the test_entity_framework table.
    /// </summary>
    [Table("test_entity_framework", Schema = "public")]
    public class TestEntityFramework
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity. Maximum length is 100 characters.
        /// </summary>
        [Column("name", TypeName = "varchar(100)")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Gets or sets the category of the entity. Maximum length is 100 characters.
        /// </summary>
        [Column("category", TypeName = "varchar(100)")]
        public string Category { get; set; } = null!;

        /// <summary>
        /// Gets or sets the price of the entity stored as money type in PostgreSQL.
        /// </summary>
        [Column("price", TypeName = "money")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the creation timestamp of the entity without timezone information.
        /// </summary>
        [Column("created_at", TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active.
        /// </summary>
        [Column("is_active")]
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Represents a test category entity mapped to the test_categories table.
    /// </summary>
    [Table("test_categories", Schema = "public")]
    public class TestCategory
    {
        /// <summary>
        /// Gets or sets the category name, which serves as the primary key.
        /// </summary>
        [Key]
        [Column("category")]
        public string Category { get; set; } = null!;

        /// <summary>
        /// Gets or sets the sort order for the category.
        /// </summary>
        [Column("sort_order")]
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Represents the Entity Framework database context for test entities and categories.
    /// Provides access to the test_entity_framework and test_categories tables.
    /// </summary>
    public class TestEntitiesContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet for TestEntityFramework entities.
        /// </summary>
        public DbSet<TestEntityFramework> TestEntities { get; set; } = null!;

        /// <summary>
        /// Gets or sets the DbSet for TestCategory entities.
        /// </summary>
        public DbSet<TestCategory> Categories { get; set; } = null!;

        /// <summary>
        /// Configures the database context to use PostgreSQL with an empty connection string.
        /// </summary>
        /// <param name="options">The options builder for configuring the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseNpgsql(string.Empty);
    }
}
