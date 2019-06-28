using DataFilter.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DataFilter.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        private static readonly MethodInfo _dataFiltersMethodInfo = typeof(ApplicationDbContext).GetMethod(nameof(DataFilters), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IHttpContextAccessor _httpContextAccessor;
        public DbSet<Book> Books { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override int SaveChanges()
        {
            FillDataFilterInfo();
            return base.SaveChanges();
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Model
                .GetEntityTypes()
                .Where(w => typeof(IDataFilter).IsAssignableFrom(w.ClrType))
                .ToList().ForEach(entityType =>
                {
                    _dataFiltersMethodInfo
                      .MakeGenericMethod(entityType.ClrType)
                      .Invoke(this, new object[] { builder });
                });
            base.OnModelCreating(builder);
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            FillDataFilterInfo();
            return base.SaveChangesAsync(cancellationToken);
        }
        protected void FillDataFilterInfo() =>
            ChangeTracker
            .Entries()
            .Where(w => w.Entity is IDataFilter && w.State == EntityState.Added)
            .ToList()
            .ForEach(entry => ((IDataFilter)entry.Entity).UserName = CurrentUserName);

        private string CurrentUserName => _httpContextAccessor.HttpContext.User?.Identity?.Name;

        private void DataFilters<T>(ModelBuilder builder)
             where T : class
        {
            builder.Entity<T>().HasQueryFilter(s => ((IDataFilter)s).UserName == CurrentUserName);
        }
    }
}
