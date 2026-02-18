using Microsoft.EntityFrameworkCore;
using weatherapp.Entities;

namespace weatherapp.Utilities;

public static class AuditHelper
{
	public static void ApplyAuditInfo(DbContext context)
	{
		foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.CreatedAt = DateTime.UtcNow;
				entry.Entity.UpdatedAt = DateTime.UtcNow;
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Entity.UpdatedAt = DateTime.UtcNow;
			}
		}
	}
}
