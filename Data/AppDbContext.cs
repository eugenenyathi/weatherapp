using Microsoft.EntityFrameworkCore;
using weatherapp.Entities;

namespace weatherapp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Location> Locations { get; set; }
	public DbSet<TrackLocation> TrackLocations { get; set; }
	public DbSet<UserPreference> UserPreferences { get; set; }
	public DbSet<DayWeather> DailyWeathers { get; set; }
	public DbSet<HourWeather> HourlyWeathers { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

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
				.IsRequired()
				.HasMaxLength(255);

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
				.IsRequired()
				.HasMaxLength(255);

			entity.Property(e => e.PreferredUnit)
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
				.IsRequired();
			
			entity.Property(e => e.MaxTempImperial)
				.IsRequired();

			entity.Property(e => e.Humidity)
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
	}
}