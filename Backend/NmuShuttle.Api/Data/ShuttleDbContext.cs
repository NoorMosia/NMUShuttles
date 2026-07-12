using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Data;

/// <summary>
/// The primary EF Core DbContext for the NMU Shuttle Management system.
/// Configures all entity relationships, constraints, indexes, and default values.
/// </summary>
public class ShuttleDbContext : DbContext
{
    public ShuttleDbContext(DbContextOptions<ShuttleDbContext> options)
        : base(options)
    {
    }

    // DbSet properties for all 16 entities
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Marshal> Marshals => Set<Marshal>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Campus> Campuses => Set<Campus>();
    public DbSet<Stop> Stops => Set<Stop>();
    public DbSet<ScheduleTime> ScheduleTimes => Set<ScheduleTime>();
    public DbSet<CalendarException> CalendarExceptions => Set<CalendarException>();
    public DbSet<Shuttle> Shuttles => Set<Shuttle>();
    public DbSet<TripInstance> TripInstances => Set<TripInstance>();
    public DbSet<TripShuttleAllocation> TripShuttleAllocations => Set<TripShuttleAllocation>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<PrioritySchedule> PrioritySchedules => Set<PrioritySchedule>();
    public DbSet<DeviceRegistration> DeviceRegistrations => Set<DeviceRegistration>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<BreakdownLog> BreakdownLogs => Set<BreakdownLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureStudent(modelBuilder);
        ConfigureMarshal(modelBuilder);
        ConfigureAdmin(modelBuilder);
        ConfigureCampus(modelBuilder);
        ConfigureStop(modelBuilder);
        ConfigureScheduleTime(modelBuilder);
        ConfigureCalendarException(modelBuilder);
        ConfigureShuttle(modelBuilder);
        ConfigureTripInstance(modelBuilder);
        ConfigureTripShuttleAllocation(modelBuilder);
        ConfigureBooking(modelBuilder);
        ConfigurePrioritySchedule(modelBuilder);
        ConfigureDeviceRegistration(modelBuilder);
        ConfigureNotification(modelBuilder);
        ConfigureReport(modelBuilder);
        ConfigureBreakdownLog(modelBuilder);

        // Apply seed data from firstSeed.md specification
        SeedData.Apply(modelBuilder);
    }

    private static void ConfigureStudent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.StudentId);

            entity.Property(e => e.StudentNumber).HasColumnType("nvarchar(20)").IsRequired();
            entity.Property(e => e.FirstName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.LastName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.UniversityEmail).HasColumnType("nvarchar(200)").IsRequired();
            entity.Property(e => e.IsPriorityUser).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.OtpHash).HasColumnType("nvarchar(256)");
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime2");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.StudentNumber).IsUnique();
            entity.HasIndex(e => e.UniversityEmail).IsUnique();
        });
    }

    private static void ConfigureMarshal(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Marshal>(entity =>
        {
            entity.ToTable("Marshals");
            entity.HasKey(e => e.MarshalId);

            entity.Property(e => e.StaffNumber).HasColumnType("nvarchar(20)").IsRequired();
            entity.Property(e => e.FirstName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.LastName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.UniversityEmail).HasColumnType("nvarchar(200)").IsRequired();
            entity.Property(e => e.OtpHash).HasColumnType("nvarchar(256)");
            entity.Property(e => e.OtpExpiry).HasColumnType("datetime2");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.StaffNumber).IsUnique();
            entity.HasIndex(e => e.UniversityEmail).IsUnique();

            // XOR CHECK constraint: exactly one of AssignedCampusId or AssignedStopId must be non-null
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Marshals_AssignedXOR",
                "(AssignedCampusId IS NOT NULL AND AssignedStopId IS NULL) OR (AssignedCampusId IS NULL AND AssignedStopId IS NOT NULL)"));

            // Relationships
            entity.HasOne(e => e.AssignedCampus)
                .WithMany(c => c.Marshals)
                .HasForeignKey(e => e.AssignedCampusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedStop)
                .WithMany(s => s.Marshals)
                .HasForeignKey(e => e.AssignedStopId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAdmin(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.ToTable("Admins");
            entity.HasKey(e => e.AdminId);

            entity.Property(e => e.Email).HasColumnType("nvarchar(200)").IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnType("nvarchar(256)").IsRequired();
            entity.Property(e => e.FirstName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.LastName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    private static void ConfigureCampus(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campus>(entity =>
        {
            entity.ToTable("Campuses");
            entity.HasKey(e => e.CampusId);

            entity.Property(e => e.CampusName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.GpsCoordinates).HasColumnType("nvarchar(50)").IsRequired();

            entity.HasIndex(e => e.CampusName).IsUnique();
        });
    }

    private static void ConfigureStop(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stop>(entity =>
        {
            entity.ToTable("Stops");
            entity.HasKey(e => e.StopId);

            entity.Property(e => e.StopName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.FullAddress).HasColumnType("nvarchar(300)").IsRequired();
            entity.Property(e => e.GpsCoordinates).HasColumnType("nvarchar(50)").IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

            entity.HasIndex(e => e.StopName).IsUnique();
        });
    }

    private static void ConfigureScheduleTime(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduleTime>(entity =>
        {
            entity.ToTable("ScheduleTimes");
            entity.HasKey(e => e.ScheduleTimeId);

            entity.Property(e => e.DepartureTime).IsRequired();
            entity.Property(e => e.Direction).HasColumnType("nvarchar(20)").IsRequired();
            entity.Property(e => e.ScheduleType).HasColumnType("nvarchar(20)").IsRequired();

            // CHECK constraints for enum-like columns
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_ScheduleTimes_Direction",
                "Direction IN ('ToCampus','FromCampus')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_ScheduleTimes_ScheduleType",
                "ScheduleType IN ('Weekday','Saturday')"));

            // Relationships
            entity.HasOne(e => e.Campus)
                .WithMany(c => c.ScheduleTimes)
                .HasForeignKey(e => e.CampusId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Stop)
                .WithMany(s => s.ScheduleTimes)
                .HasForeignKey(e => e.StopId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureCalendarException(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarException>(entity =>
        {
            entity.ToTable("CalendarExceptions");
            entity.HasKey(e => e.CalendarExceptionId);

            entity.Property(e => e.TargetDate).IsRequired();
            entity.Property(e => e.ExceptionType).HasColumnType("nvarchar(30)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("nvarchar(500)");

            entity.HasIndex(e => e.TargetDate).IsUnique();

            // CHECK constraint for ExceptionType
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_CalendarExceptions_ExceptionType",
                "ExceptionType IN ('PublicHoliday','UniversityRecess')"));
        });
    }

    private static void ConfigureShuttle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shuttle>(entity =>
        {
            entity.ToTable("Shuttles");
            entity.HasKey(e => e.ShuttleId);

            entity.Property(e => e.LicensePlate).HasColumnType("nvarchar(20)").IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.VendorCompanyName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.IsAccessibleVehicle).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.LicensePlate).IsUnique();

            // CHECK constraint: Capacity > 0
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Shuttles_Capacity",
                "Capacity > 0"));
        });
    }

    private static void ConfigureTripInstance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TripInstance>(entity =>
        {
            entity.ToTable("TripInstances");
            entity.HasKey(e => e.TripInstanceId);

            entity.Property(e => e.TripDate).IsRequired();
            entity.Property(e => e.Status).HasColumnType("nvarchar(20)").IsRequired()
                .HasDefaultValue("Scheduling");
            entity.Property(e => e.LockedAt).HasColumnType("datetime2");
            entity.Property(e => e.InTransitAt).HasColumnType("datetime2");
            entity.Property(e => e.CompletedAt).HasColumnType("datetime2");

            // CHECK constraint for Status
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_TripInstances_Status",
                "Status IN ('Scheduling','Locked','InTransit','Completed','IncidentActive')"));

            // Composite unique constraint: prevents duplicate trip injection
            entity.HasIndex(e => new { e.ScheduleTimeId, e.StopId, e.TripDate }).IsUnique()
                .HasDatabaseName("UQ_TripInstances_ScheduleTime_Stop_Date");

            // Performance index
            entity.HasIndex(e => new { e.TripDate, e.Status })
                .HasDatabaseName("IX_TripInstances_TripDate_Status");

            // Relationships
            entity.HasOne(e => e.ScheduleTime)
                .WithMany(st => st.TripInstances)
                .HasForeignKey(e => e.ScheduleTimeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Stop)
                .WithMany(s => s.TripInstances)
                .HasForeignKey(e => e.StopId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTripShuttleAllocation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TripShuttleAllocation>(entity =>
        {
            entity.ToTable("TripShuttleAllocations");
            entity.HasKey(e => e.AllocationId);

            entity.Property(e => e.OrderPosition).IsRequired();
            entity.Property(e => e.IsOperational).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.AssignedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Composite unique constraints
            entity.HasIndex(e => new { e.TripInstanceId, e.ShuttleId }).IsUnique()
                .HasDatabaseName("UQ_TripShuttleAllocations_TripInstance_Shuttle");

            entity.HasIndex(e => new { e.TripInstanceId, e.OrderPosition }).IsUnique()
                .HasDatabaseName("IX_TripShuttleAllocations_TripInstanceId_OrderPosition");

            // Performance index
            entity.HasIndex(e => e.TripInstanceId)
                .HasDatabaseName("IX_TripShuttleAllocations_TripInstanceId");

            // Relationships
            entity.HasOne(e => e.TripInstance)
                .WithMany(ti => ti.TripShuttleAllocations)
                .HasForeignKey(e => e.TripInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Shuttle)
                .WithMany(s => s.TripShuttleAllocations)
                .HasForeignKey(e => e.ShuttleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBooking(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(e => e.BookingId);

            entity.Property(e => e.BookingStatus).HasColumnType("nvarchar(20)").IsRequired()
                .HasDefaultValue("Confirmed");
            entity.Property(e => e.BookingType).HasColumnType("nvarchar(20)").IsRequired()
                .HasDefaultValue("Standard");
            entity.Property(e => e.BoardedStatus).HasColumnType("nvarchar(20)").IsRequired()
                .HasDefaultValue("Pending");
            entity.Property(e => e.QrCodeToken).HasColumnType("nvarchar(128)").IsRequired();
            entity.Property(e => e.BoardedAt).HasColumnType("datetime2");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // CHECK constraints for enum-like columns
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Bookings_BookingStatus",
                "BookingStatus IN ('Confirmed','WaitingList','Cancelled')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Bookings_BookingType",
                "BookingType IN ('Standard','Priority')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Bookings_BoardedStatus",
                "BoardedStatus IN ('Pending','Boarded','NoShow')"));

            // Unique constraint on QrCodeToken
            entity.HasIndex(e => e.QrCodeToken).IsUnique();

            // Filtered unique index: no duplicate active bookings per student per trip
            entity.HasIndex(e => new { e.StudentId, e.TripInstanceId })
                .IsUnique()
                .HasDatabaseName("UQ_Bookings_Student_TripInstance_Active")
                .HasFilter("BookingStatus != 'Cancelled'");

            // Performance indexes
            entity.HasIndex(e => new { e.TripInstanceId, e.BookingStatus })
                .HasDatabaseName("IX_Bookings_TripInstanceId_Status");

            entity.HasIndex(e => e.StudentId)
                .HasDatabaseName("IX_Bookings_StudentId");

            entity.HasIndex(e => e.QrCodeToken)
                .HasDatabaseName("IX_Bookings_QrCodeToken");

            // Relationships
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TripInstance)
                .WithMany(ti => ti.Bookings)
                .HasForeignKey(e => e.TripInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Shuttle)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.ShuttleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePrioritySchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrioritySchedule>(entity =>
        {
            entity.ToTable("PrioritySchedules");
            entity.HasKey(e => e.PriorityScheduleId);

            entity.Property(e => e.DayOfWeek).HasColumnType("nvarchar(10)").IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // CHECK constraint for DayOfWeek
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_PrioritySchedules_DayOfWeek",
                "DayOfWeek IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday')"));

            // Performance index
            entity.HasIndex(e => new { e.DayOfWeek, e.IsActive })
                .HasDatabaseName("IX_PrioritySchedules_DayOfWeek_Active");

            // Relationships
            entity.HasOne(e => e.Student)
                .WithMany(s => s.PrioritySchedules)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Stop)
                .WithMany(s => s.PrioritySchedules)
                .HasForeignKey(e => e.StopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ScheduleTime)
                .WithMany(st => st.PrioritySchedules)
                .HasForeignKey(e => e.ScheduleTimeId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureDeviceRegistration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceRegistration>(entity =>
        {
            entity.ToTable("DeviceRegistrations");
            entity.HasKey(e => e.DeviceRegistrationId);

            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.OwnerType).HasColumnType("nvarchar(10)").IsRequired();
            entity.Property(e => e.FcmToken).HasColumnType("nvarchar(512)").IsRequired();
            entity.Property(e => e.Platform).HasColumnType("nvarchar(10)").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // CHECK constraints for enum-like columns
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_DeviceRegistrations_OwnerType",
                "OwnerType IN ('Student','Marshal')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_DeviceRegistrations_Platform",
                "Platform IN ('android','ios')"));

            // Composite unique constraint: one token per platform per user
            entity.HasIndex(e => new { e.OwnerId, e.OwnerType, e.Platform }).IsUnique()
                .HasDatabaseName("UQ_DeviceRegistrations_Owner_Platform");

            // Performance index
            entity.HasIndex(e => new { e.OwnerId, e.OwnerType })
                .HasDatabaseName("IX_DeviceRegistrations_OwnerId_Type");
        });
    }

    private static void ConfigureNotification(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(e => e.NotificationId);

            entity.Property(e => e.Title).HasColumnType("nvarchar(200)").IsRequired();
            entity.Property(e => e.Message).HasColumnType("nvarchar(1000)").IsRequired();
            entity.Property(e => e.IsRead).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Performance index
            entity.HasIndex(e => new { e.StudentId, e.IsRead })
                .HasDatabaseName("IX_Notifications_StudentId_IsRead");

            // Relationships
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Notifications)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RelatedTripInstance)
                .WithMany(ti => ti.Notifications)
                .HasForeignKey(e => e.RelatedTripInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReport(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Reports");
            entity.HasKey(e => e.ReportId);

            entity.Property(e => e.Category).HasColumnType("nvarchar(30)").IsRequired();
            entity.Property(e => e.Description).HasColumnType("nvarchar(2000)").IsRequired();
            entity.Property(e => e.Status).HasColumnType("nvarchar(20)").IsRequired()
                .HasDefaultValue("Submitted");
            entity.Property(e => e.AdminNotes).HasColumnType("nvarchar(2000)");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime2");

            // CHECK constraints for enum-like columns
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Reports_Category",
                "Category IN ('DriverBehavior','TimetableIssue','VehicleQuality','Other')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "CK_Reports_Status",
                "Status IN ('Submitted','UnderReview','Resolved')"));

            // Relationships
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Reports)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.TripInstance)
                .WithMany(ti => ti.Reports)
                .HasForeignKey(e => e.TripInstanceId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureBreakdownLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BreakdownLog>(entity =>
        {
            entity.ToTable("BreakdownLogs");
            entity.HasKey(e => e.BreakdownLogId);

            entity.Property(e => e.VendorCompanyName).HasColumnType("nvarchar(100)").IsRequired();
            entity.Property(e => e.FlaggedAt).HasColumnType("datetime2").IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.ResolutionType).HasColumnType("nvarchar(20)");
            entity.Property(e => e.ResolvedAt).HasColumnType("datetime2");

            // CHECK constraint for ResolutionType (nullable, so only checked when not null)
            entity.ToTable(t => t.HasCheckConstraint(
                "CK_BreakdownLogs_ResolutionType",
                "ResolutionType IS NULL OR ResolutionType IN ('Replaced','Cancelled')"));

            // Performance indexes
            entity.HasIndex(e => e.ShuttleId)
                .HasDatabaseName("IX_BreakdownLogs_ShuttleId");

            entity.HasIndex(e => e.VendorCompanyName)
                .HasDatabaseName("IX_BreakdownLogs_VendorCompanyName");

            entity.HasIndex(e => e.FlaggedAt)
                .HasDatabaseName("IX_BreakdownLogs_FlaggedAt");

            // Relationships
            entity.HasOne(e => e.TripInstance)
                .WithMany(ti => ti.BreakdownLogs)
                .HasForeignKey(e => e.TripInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Shuttle)
                .WithMany(s => s.BreakdownLogs)
                .HasForeignKey(e => e.ShuttleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FlaggedByMarshal)
                .WithMany(m => m.FlaggedBreakdownLogs)
                .HasForeignKey(e => e.FlaggedByMarshalId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReplacementShuttle)
                .WithMany(s => s.ReplacementBreakdownLogs)
                .HasForeignKey(e => e.ReplacementShuttleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ResolvedByAdmin)
                .WithMany(a => a.ResolvedBreakdownLogs)
                .HasForeignKey(e => e.ResolvedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
