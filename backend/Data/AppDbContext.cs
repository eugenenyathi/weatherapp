using Microsoft.EntityFrameworkCore;
using weatherapp.Entities;
using weatherapp.Enums;

namespace weatherapp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Location> Locations { get; set; }
	public DbSet<TrackLocation> TrackLocations { get; set; }
	public DbSet<UserPreference> UserPreferences { get; set; }
	public DbSet<DayWeather> DailyWeathers { get; set; }
	public DbSet<HourWeather> HourlyWeathers { get; set; }
	public DbSet<User> Users { get; set; }
	public DbSet<LocationJob> LocationJobs { get; set; }
	public DbSet<LocationSyncSchedule> LocationSyncSchedules { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.Email)
				.IsRequired()
				.HasMaxLength(255);

			entity.HasIndex(e => e.Email)
				.IsUnique();

			entity.Property(e => e.PasswordHash)
				.IsRequired();

			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});

		modelBuilder.Entity<Location>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.Name)
				.IsRequired()
				.HasMaxLength(200);

			entity.Property(e => e.Latitude)
				.HasColumnType("decimal(9,6)");

			entity.Property(e => e.Longitude)
				.HasColumnType("decimal(9,6)");

			entity.Property(e => e.Country)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});

		modelBuilder.Entity<TrackLocation>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.UserId)
				.IsRequired();

			entity.HasOne(tl => tl.User)
				.WithMany()
				.HasForeignKey(tl => tl.UserId)
				.OnDelete(DeleteBehavior.NoAction); // Changed to NoAction or set to Restrict to avoid multiple cascade paths

			entity.Property(e => e.isFavorite)
				.HasColumnName("IsFavorite");

			entity.Property(e => e.DisplayName)
				.HasMaxLength(200);

			entity.HasOne(tl => tl.Location)
				.WithMany()
				.HasForeignKey(tl => tl.LocationId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});

		modelBuilder.Entity<UserPreference>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.UserId)
				.IsRequired();

			entity.HasOne(up => up.User)
				.WithMany()
				.HasForeignKey(up => up.UserId)
				.OnDelete(DeleteBehavior.NoAction); // Changed to NoAction or set to Restrict to avoid multiple cascade paths

			entity.Property(e => e.PreferredUnit)
				.HasConversion<string>()
				.IsRequired();

			entity.Property(e => e.RefreshInterval)
				.IsRequired();

			// Configure timestamps
			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});

		modelBuilder.Entity<DayWeather>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.MinTempMetric)
				.HasColumnType("decimal(5,2)")
				.IsRequired();

			entity.Property(e => e.MaxTempMetric)
				.HasColumnType("decimal(5,2)")
				.IsRequired();
			
			entity.Property(e => e.MinTempImperial)
				.HasColumnType("decimal(5,2)")
				.IsRequired();
			
			entity.Property(e => e.MaxTempImperial)
				.HasColumnType("decimal(5,2)")
				.IsRequired();

			entity.Property(e => e.Humidity)
				.HasColumnType("decimal(5,2)")
				.IsRequired();

			entity.Property(e => e.Rain)
				.HasColumnType("decimal(6,2)");

			entity.Property(e => e.Summary)
				.HasMaxLength(200)
				.IsRequired();;

			// Configure timestamps
			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();

			entity.HasOne(e => e.Location)
				.WithMany(e => e.DailyWeathers)
				.HasForeignKey(e => e.LocationId);
		});

		modelBuilder.Entity<HourWeather>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.Property(e => e.DateTime)
				.IsRequired();

			entity.Property(e => e.TempMetric)
				.HasColumnType("decimal(5,2)")
				.IsRequired();

			entity.Property(e => e.TempImperial)
				.HasColumnType("decimal(5,2)")
				.IsRequired();

			entity.Property(e => e.Humidity)
				.HasColumnType("decimal(5,2)")
				.IsRequired();

			// Configure timestamps
			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});

		modelBuilder.Entity<LocationJob>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.HasOne(lj => lj.Location)
				.WithMany(l => l.LocationJobs)
				.HasForeignKey(lj => lj.LocationId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.Property(e => e.JobId)
				.IsRequired()
				.HasMaxLength(100);

			entity.Property(e => e.JobCreatedAt)
				.IsRequired();

			entity.Property(e => e.Status)
				.IsRequired()
				.HasMaxLength(50);

			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();
		});

		modelBuilder.Entity<LocationSyncSchedule>(entity =>
		{
			entity.HasKey(e => e.Id);

			entity.HasOne(lss => lss.User)
				.WithMany()
				.HasForeignKey(lss => lss.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasOne(lss => lss.Location)
				.WithMany()
				.HasForeignKey(lss => lss.LocationId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.Property(e => e.RecurringJobId)
				.IsRequired()
				.HasMaxLength(200);

			entity.Property(e => e.LastSyncAt)
				.IsRequired();

			entity.Property(e => e.NextSyncAt)
				.IsRequired();

			entity.Property(e => e.CreatedAt)
				.IsRequired();

			entity.Property(e => e.UpdatedAt)
				.IsRequired();

			// Unique constraint for user-location pair
			entity.HasIndex(e => new { e.UserId, e.LocationId })
				.IsUnique();
		});
	}
}