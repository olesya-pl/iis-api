using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Iis.FlightRadar.DataModel
{
    public partial class FlightsContext : DbContext
    {
        public FlightsContext()
        {
        }

        public FlightsContext(DbContextOptions<FlightsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Aircraft> Aircraft { get; set; }
        public virtual DbSet<Airports> Airports { get; set; }
        public virtual DbSet<Flights> Flights { get; set; }
        public virtual DbSet<Operators> Operators { get; set; }
        public virtual DbSet<Routes> Routes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum(null, "enum_Aircraft_purposeType", new[] { "civil", "millitary" });

            modelBuilder.Entity<Aircraft>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DetailedModel)
                    .HasColumnName("detailedModel")
                    .HasMaxLength(255);

                entity.Property(e => e.Icao)
                    .HasColumnName("icao")
                    .HasMaxLength(255);

                entity.Property(e => e.Model)
                    .HasColumnName("model")
                    .HasMaxLength(255);

                entity.Property(e => e.OwnerId).HasColumnName("ownerId");

                entity.Property(e => e.Photo)
                    .HasColumnName("photo")
                    .HasMaxLength(255);

                entity.Property(e => e.RegistrationNumber)
                    .HasColumnName("registration_number")
                    .HasMaxLength(255);

                entity.Property(e => e.Type)
                    .HasColumnName("type")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp with time zone");

                entity.HasOne(d => d.Owner)
                    .WithMany(p => p.Aircraft)
                    .HasForeignKey(d => d.OwnerId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("Aircraft_ownerId_fkey");
            });

            modelBuilder.Entity<Airports>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Altitude)
                    .HasColumnName("altitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.City)
                    .HasColumnName("city")
                    .HasMaxLength(255);

                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasMaxLength(255);

                entity.Property(e => e.CountryCode)
                    .HasColumnName("countryCode")
                    .HasMaxLength(255);

                entity.Property(e => e.CountryCodeLong)
                    .HasColumnName("countryCodeLong")
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Iata)
                    .HasColumnName("iata")
                    .HasMaxLength(255);

                entity.Property(e => e.Icao)
                    .HasColumnName("icao")
                    .HasMaxLength(255);

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Website)
                    .HasColumnName("website")
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<Flights>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.ArrivalAirportId).HasColumnName("arrivalAirportId");

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.DepartureAirportId).HasColumnName("departureAirportId");

                entity.Property(e => e.ExternalId)
                    .HasColumnName("externalId")
                    .HasMaxLength(100);

                entity.Property(e => e.FlightNo)
                    .HasColumnName("flightNo")
                    .HasMaxLength(100)
                    .IsFixedLength();

                entity.Property(e => e.Meta)
                    .HasColumnName("meta")
                    .HasColumnType("json");

                entity.Property(e => e.PlaneId).HasColumnName("planeId");

                entity.Property(e => e.RealArrivalAt)
                    .HasColumnName("realArrivalAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.RealDepartureAt)
                    .HasColumnName("realDepartureAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.ScheduledArrivalAt)
                    .HasColumnName("scheduledArrivalAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.ScheduledDepartureAt)
                    .HasColumnName("scheduledDepartureAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp with time zone");

                entity.HasOne(d => d.ArrivalAirport)
                    .WithMany(p => p.FlightsArrivalAirport)
                    .HasForeignKey(d => d.ArrivalAirportId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("Flights_arrivalAirportId_fkey");

                entity.HasOne(d => d.DepartureAirport)
                    .WithMany(p => p.FlightsDepartureAirport)
                    .HasForeignKey(d => d.DepartureAirportId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("Flights_departureAirportId_fkey");

                entity.HasOne(d => d.Plane)
                    .WithMany(p => p.Flights)
                    .HasForeignKey(d => d.PlaneId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("Flights_planeId_fkey");
            });

            modelBuilder.Entity<Operators>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.About)
                    .HasColumnName("about")
                    .IsRequired(false)
                    .HasColumnType("json");

                entity.Property(e => e.Country)
                    .HasColumnName("country")
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("createdAt")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Iata)
                    .HasColumnName("iata")
                    .HasMaxLength(255);

                entity.Property(e => e.Icao)
                    .HasColumnName("icao")
                    .HasMaxLength(255);

                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);

                entity.Property(e => e.ShortName)
                    .HasColumnName("shortName")
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updatedAt")
                    .HasColumnType("timestamp with time zone");
            });

            modelBuilder.Entity<Routes>(entity =>
            {
                entity.HasIndex(e => new { e.FlightId, e.Latitude, e.Longitude, e.Altitude, e.TimeNow })
                    .HasName("routes_flight_id_latitude_longitude_altitude_time_now")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Altitude)
                    .HasColumnName("altitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Callsign)
                    .HasColumnName("callsign")
                    .HasMaxLength(255);

                entity.Property(e => e.FlightId).HasColumnName("flightId");

                entity.Property(e => e.Latitude)
                    .HasColumnName("latitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Longitude)
                    .HasColumnName("longitude")
                    .HasColumnType("numeric");

                entity.Property(e => e.Speed).HasColumnName("speed");

                entity.Property(e => e.SquawkCode)
                    .HasColumnName("squawk_code")
                    .HasMaxLength(100)
                    .IsFixedLength();

                entity.Property(e => e.TimeNow)
                    .HasColumnName("timeNow")
                    .HasColumnType("timestamp with time zone");

                entity.Property(e => e.Track).HasColumnName("track");

                entity.HasOne(d => d.Flight)
                    .WithMany(p => p.Routes)
                    .HasForeignKey(d => d.FlightId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("Routes_flightId_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

    public class FlightsContextInterceptor : DbCommandInterceptor
    {
        public override Task<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            if (command.CommandText.StartsWith("INSERT INTO", StringComparison.OrdinalIgnoreCase))
            {
                command.CommandText = command.CommandText.Replace(";", $" ON CONFLICT (\"id\") DO UPDATE SET \"id\"=EXCLUDED.\"id\";");
            }                
            
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
