using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Data;

/// <summary>
/// Seeds initial data for development and testing.
/// All data matches the canonical firstSeed.md specification.
/// 
/// To generate the EF Core migration after adding this seed data:
///   dotnet ef migrations add InitialCreate
///   dotnet ef database update
/// </summary>
public static class SeedData
{
    // Fixed seed date for all CreatedAt values
    private static readonly DateTime SeedDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);

    // Pre-computed BCrypt hashes (cost factor 12)
    // "Admin@2025!" hashed with BCrypt cost 12
    private const string AdminPasswordHash =
        "$2a$12$UnzkP2CPSCL04uIvla3UKOpZAVQZFEPRngF12SJyE2.gdVhHlZBk6";

    // "Transport@2025!" hashed with BCrypt cost 12
    private const string TransportPasswordHash =
        "$2a$12$YqZo4jPNiOcMGYhDRJ4CleeMqptACgPleJfWR/n.UJKTol9pZfE0i";

    /// <summary>
    /// Applies all seed data to the model builder.
    /// Call this method from ShuttleDbContext.OnModelCreating().
    /// </summary>
    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedAdmins(modelBuilder);
        SeedCampuses(modelBuilder);
        SeedStops(modelBuilder);
        SeedStudents(modelBuilder);
        SeedMarshals(modelBuilder);
        SeedShuttles(modelBuilder);
        SeedScheduleTimes(modelBuilder);
        SeedCalendarExceptions(modelBuilder);
        SeedPrioritySchedules(modelBuilder);
    }

    private static void SeedAdmins(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>().HasData(
            new Admin
            {
                AdminId = 1,
                Email = "admin@mandela.ac.za",
                StaffNumber = "7001001",
                PasswordHash = AdminPasswordHash,
                FirstName = "Thabo",
                LastName = "Mokoena",
                CreatedAt = SeedDate
            },
            new Admin
            {
                AdminId = 2,
                Email = "transport@mandela.ac.za",
                StaffNumber = "7001002",
                PasswordHash = TransportPasswordHash,
                FirstName = "Nomsa",
                LastName = "Dlamini",
                CreatedAt = SeedDate
            }
        );
    }

    private static void SeedCampuses(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Campus>().HasData(
            new Campus { CampusId = 1, CampusName = "South Campus", GpsCoordinates = "-33.9580,25.5920" },
            new Campus { CampusId = 2, CampusName = "North Campus", GpsCoordinates = "-33.9185,25.5870" },
            new Campus { CampusId = 3, CampusName = "2nd Avenue Campus", GpsCoordinates = "-33.9610,25.6100" }
        );
    }

    private static void SeedStops(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Stop>().HasData(
            new Stop { StopId = 1, StopName = "Summerstrand Village", Street = "12 Marine Drive", Suburb = "Summerstrand", City = "Gqeberha", PostalCode = "6001", GpsCoordinates = "-33.9680,25.6450", IsActive = true },
            new Stop { StopId = 2, StopName = "Humewood", Street = "45 Beach Road", Suburb = "Humewood", City = "Gqeberha", PostalCode = "6001", GpsCoordinates = "-33.9630,25.6280", IsActive = true },
            new Stop { StopId = 3, StopName = "Newton Park", Street = "78 Cape Road", Suburb = "Newton Park", City = "Gqeberha", PostalCode = "6045", GpsCoordinates = "-33.9480,25.5750", IsActive = true },
            new Stop { StopId = 4, StopName = "Walmer Heights", Street = "23 Heugh Road", Suburb = "Walmer Heights", City = "Gqeberha", PostalCode = "6070", GpsCoordinates = "-33.9850,25.6100", IsActive = true },
            new Stop { StopId = 5, StopName = "Lorraine", Street = "91 Circular Drive", Suburb = "Lorraine", City = "Gqeberha", PostalCode = "6070", GpsCoordinates = "-33.9320,25.5620", IsActive = true },
            new Stop { StopId = 6, StopName = "Bluewater Bay", Street = "5 Seaview Crescent", Suburb = "Bluewater Bay", City = "Gqeberha", PostalCode = "6210", GpsCoordinates = "-33.8710,25.5590", IsActive = true },
            new Stop { StopId = 7, StopName = "Framesby", Street = "14 Langenhoven Drive", Suburb = "Framesby", City = "Gqeberha", PostalCode = "6045", GpsCoordinates = "-33.9260,25.5810", IsActive = true },
            new Stop { StopId = 8, StopName = "Greenacres", Street = "30 Ring Road", Suburb = "Greenacres", City = "Gqeberha", PostalCode = "6045", GpsCoordinates = "-33.9410,25.5870", IsActive = true }
        );
    }

    private static void SeedStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().HasData(
            new Student { StudentId = 1, StudentNumber = "s312847563", FirstName = "Sarah", LastName = "Nkosi", UniversityEmail = "s312847563@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 2, StudentNumber = "s319284731", FirstName = "Liam", LastName = "van der Merwe", UniversityEmail = "s319284731@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 3, StudentNumber = "s321056839", FirstName = "Ayanda", LastName = "Mthembu", UniversityEmail = "s321056839@mandela.ac.za", IsPriorityUser = true, CreatedAt = SeedDate },
            new Student { StudentId = 4, StudentNumber = "s317493208", FirstName = "Chloe", LastName = "Botha", UniversityEmail = "s317493208@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 5, StudentNumber = "s329187465", FirstName = "Sipho", LastName = "Zulu", UniversityEmail = "s329187465@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 6, StudentNumber = "s318274056", FirstName = "Jessica", LastName = "Adams", UniversityEmail = "s318274056@mandela.ac.za", IsPriorityUser = true, CreatedAt = SeedDate },
            new Student { StudentId = 7, StudentNumber = "s322941587", FirstName = "Mandla", LastName = "Ndlovu", UniversityEmail = "s322941587@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 8, StudentNumber = "s316785234", FirstName = "Regan", LastName = "Pieterse", UniversityEmail = "s316785234@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 9, StudentNumber = "s325098471", FirstName = "Zanele", LastName = "Khumalo", UniversityEmail = "s325098471@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 10, StudentNumber = "s311653928", FirstName = "Devon", LastName = "Jacobs", UniversityEmail = "s311653928@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 11, StudentNumber = "s328417509", FirstName = "Thandi", LastName = "Sithole", UniversityEmail = "s328417509@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 12, StudentNumber = "s314928673", FirstName = "Jason", LastName = "Williams", UniversityEmail = "s314928673@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 13, StudentNumber = "s327651084", FirstName = "Nokwanda", LastName = "Dube", UniversityEmail = "s327651084@mandela.ac.za", IsPriorityUser = true, CreatedAt = SeedDate },
            new Student { StudentId = 14, StudentNumber = "s319073845", FirstName = "Michael", LastName = "van Niekerk", UniversityEmail = "s319073845@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate },
            new Student { StudentId = 15, StudentNumber = "s323846190", FirstName = "Lerato", LastName = "Molefe", UniversityEmail = "s323846190@mandela.ac.za", IsPriorityUser = false, CreatedAt = SeedDate }
        );
    }

    private static void SeedMarshals(ModelBuilder modelBuilder)
    {
        // Marshals 1-3: Campus-assigned (AssignedCampusId set, AssignedStopId = null)
        // Marshals 4-11: Stop-assigned (AssignedStopId set, AssignedCampusId = null)
        // This respects the XOR constraint: exactly one of AssignedCampusId or AssignedStopId must be non-null
        modelBuilder.Entity<Marshal>().HasData(
            new Marshal { MarshalId = 1, StaffNumber = "4827391", FirstName = "Bongani", LastName = "Cele", UniversityEmail = "b.cele@mandela.ac.za", AssignedCampusId = 1, AssignedStopId = null, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 2, StaffNumber = "5914826", FirstName = "Karen", LastName = "Swart", UniversityEmail = "k.swart@mandela.ac.za", AssignedCampusId = 2, AssignedStopId = null, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 3, StaffNumber = "3208574", FirstName = "Vuyo", LastName = "Mabena", UniversityEmail = "v.mabena@mandela.ac.za", AssignedCampusId = 3, AssignedStopId = null, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 4, StaffNumber = "6153042", FirstName = "Thulani", LastName = "Ngcobo", UniversityEmail = "t.ngcobo@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 1, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 5, StaffNumber = "7294185", FirstName = "Lindiwe", LastName = "Shabalala", UniversityEmail = "l.shabalala@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 2, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 6, StaffNumber = "8401267", FirstName = "Pieter", LastName = "Venter", UniversityEmail = "p.venter@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 3, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 7, StaffNumber = "9537148", FirstName = "Nombuso", LastName = "Mkhize", UniversityEmail = "n.mkhize@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 4, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 8, StaffNumber = "2648309", FirstName = "Craig", LastName = "Daniels", UniversityEmail = "c.daniels@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 5, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 9, StaffNumber = "1785423", FirstName = "Noluthando", LastName = "Mthethwa", UniversityEmail = "n.mthethwa@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 6, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 10, StaffNumber = "4062891", FirstName = "Willem", LastName = "Pretorius", UniversityEmail = "w.pretorius@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 7, IsActive = true, CreatedAt = SeedDate },
            new Marshal { MarshalId = 11, StaffNumber = "5379216", FirstName = "Zandile", LastName = "Mdluli", UniversityEmail = "z.mdluli@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 8, IsActive = true, CreatedAt = SeedDate }
        );
    }

    private static void SeedShuttles(ModelBuilder modelBuilder)
    {
        // Seed vendors first
        modelBuilder.Entity<Vendor>().HasData(
            new Vendor { VendorId = 1, CompanyName = "Bay Transit Corp", ContactEmail = "info@baytransit.co.za", ContactPhone = "041-555-0101", CreatedAt = SeedDate },
            new Vendor { VendorId = 2, CompanyName = "Coastal Movers", ContactEmail = "info@coastalmovers.co.za", ContactPhone = "041-555-0202", CreatedAt = SeedDate },
            new Vendor { VendorId = 3, CompanyName = "Metro Link SA", ContactEmail = "info@metrolinksa.co.za", ContactPhone = "041-555-0303", CreatedAt = SeedDate }
        );

        modelBuilder.Entity<Shuttle>().HasData(
            new Shuttle { ShuttleId = 1, LicensePlate = "EC 142-837", Capacity = 22, VendorCompanyName = "Bay Transit Corp", VendorId = 1, IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 2, LicensePlate = "EC 256-491", Capacity = 35, VendorCompanyName = "Bay Transit Corp", VendorId = 1, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 3, LicensePlate = "EC 389-720", Capacity = 22, VendorCompanyName = "Bay Transit Corp", VendorId = 1, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 4, LicensePlate = "EC 501-163", Capacity = 28, VendorCompanyName = "Coastal Movers", VendorId = 2, IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 5, LicensePlate = "EC 628-945", Capacity = 35, VendorCompanyName = "Coastal Movers", VendorId = 2, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 6, LicensePlate = "EC 714-382", Capacity = 22, VendorCompanyName = "Coastal Movers", VendorId = 2, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 7, LicensePlate = "EC 837-256", Capacity = 28, VendorCompanyName = "Metro Link SA", VendorId = 3, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 8, LicensePlate = "EC 948-617", Capacity = 35, VendorCompanyName = "Metro Link SA", VendorId = 3, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 9, LicensePlate = "EC 103-475", Capacity = 22, VendorCompanyName = "Metro Link SA", VendorId = 3, IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 10, LicensePlate = "EC 217-864", Capacity = 28, VendorCompanyName = "Bay Transit Corp", VendorId = 1, IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate }
        );
    }

    private static void SeedScheduleTimes(ModelBuilder modelBuilder)
    {
        var id = 1;
        var schedules = new List<ScheduleTime>();

        // All 8 stops share the same timetable, assigned to South Campus (CampusId = 1)
        for (var stopId = 1; stopId <= 8; stopId++)
        {
            // Weekday ToCampus: 06:45, 07:45, 08:45, 09:45
            foreach (var time in new[] { new TimeOnly(6, 45), new TimeOnly(7, 45), new TimeOnly(8, 45), new TimeOnly(9, 45) })
            {
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "ToCampus", ScheduleType = "Weekday", StopId = stopId, RequiresScanning = true });
            }

            // Weekday FromCampus: 12:30, 14:30, 16:00, 17:30, 18:30, 20:30, 22:00
            foreach (var time in new[] { new TimeOnly(12, 30), new TimeOnly(14, 30), new TimeOnly(16, 0), new TimeOnly(17, 30), new TimeOnly(18, 30), new TimeOnly(20, 30), new TimeOnly(22, 0) })
            {
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "FromCampus", ScheduleType = "Weekday", StopId = stopId, RequiresScanning = true });
            }

            // Saturday ToCampus: 06:45, 07:45, 08:45, 09:45
            foreach (var time in new[] { new TimeOnly(6, 45), new TimeOnly(7, 45), new TimeOnly(8, 45), new TimeOnly(9, 45) })
            {
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "ToCampus", ScheduleType = "Saturday", StopId = stopId, RequiresScanning = true });
            }

            // Saturday FromCampus: 12:30, 14:30, 16:00, 17:30
            foreach (var time in new[] { new TimeOnly(12, 30), new TimeOnly(14, 30), new TimeOnly(16, 0), new TimeOnly(17, 30) })
            {
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "FromCampus", ScheduleType = "Saturday", StopId = stopId, RequiresScanning = true });
            }
        }

        modelBuilder.Entity<ScheduleTime>().HasData(schedules.ToArray());
    }

    private static void SeedCalendarExceptions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CalendarException>().HasData(
            new CalendarException { CalendarExceptionId = 1, TargetDate = new DateOnly(2025, 3, 21), ExceptionType = "PublicHoliday", Description = "Human Rights Day" },
            new CalendarException { CalendarExceptionId = 2, TargetDate = new DateOnly(2025, 4, 18), ExceptionType = "PublicHoliday", Description = "Good Friday" },
            new CalendarException { CalendarExceptionId = 3, TargetDate = new DateOnly(2025, 4, 21), ExceptionType = "PublicHoliday", Description = "Family Day" },
            new CalendarException { CalendarExceptionId = 4, TargetDate = new DateOnly(2025, 4, 28), ExceptionType = "UniversityRecess", Description = "Mid-year recess start" },
            new CalendarException { CalendarExceptionId = 5, TargetDate = new DateOnly(2025, 4, 29), ExceptionType = "UniversityRecess", Description = "Mid-year recess" },
            new CalendarException { CalendarExceptionId = 6, TargetDate = new DateOnly(2025, 4, 30), ExceptionType = "UniversityRecess", Description = "Mid-year recess" },
            new CalendarException { CalendarExceptionId = 7, TargetDate = new DateOnly(2025, 5, 1), ExceptionType = "PublicHoliday", Description = "Workers Day" },
            new CalendarException { CalendarExceptionId = 8, TargetDate = new DateOnly(2025, 6, 16), ExceptionType = "PublicHoliday", Description = "Youth Day" },
            new CalendarException { CalendarExceptionId = 9, TargetDate = new DateOnly(2025, 8, 9), ExceptionType = "PublicHoliday", Description = "National Women's Day" },
            new CalendarException { CalendarExceptionId = 10, TargetDate = new DateOnly(2025, 9, 24), ExceptionType = "PublicHoliday", Description = "Heritage Day" }
        );
    }

    private static void SeedPrioritySchedules(ModelBuilder modelBuilder)
    {
        // New ScheduleTimeId mapping (19 entries per stop):
        // Stop 1: 1-19, Stop 2: 20-38, Stop 3: 39-57, Stop 4: 58-76, ...
        // Within each stop: [1-4] ToCampus Weekday (06:45,07:45,08:45,09:45)
        //                    [5-11] FromCampus Weekday (12:30,14:30,16:00,17:30,18:30,20:30,22:00)
        //                    [12-15] ToCampus Saturday, [16-19] FromCampus Saturday
        modelBuilder.Entity<PrioritySchedule>().HasData(
            // Student 3 (Ayanda Mthembu) - Stop 1: ToCampus 06:45 (ID=1), FromCampus 16:00 (ID=7)
            new PrioritySchedule { PriorityScheduleId = 1, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 2, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 3, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 4, StudentId = 3, StopId = 1, ScheduleTimeId = 7, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 5, StudentId = 3, StopId = 1, ScheduleTimeId = 7, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 6, StudentId = 3, StopId = 1, ScheduleTimeId = 7, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate },
            // Student 6 (Jessica Adams) - Stop 2: ToCampus 06:45 (ID=20), FromCampus 16:00 (ID=26)
            new PrioritySchedule { PriorityScheduleId = 7, StudentId = 6, StopId = 2, ScheduleTimeId = 20, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 8, StudentId = 6, StopId = 2, ScheduleTimeId = 20, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 9, StudentId = 6, StopId = 2, ScheduleTimeId = 26, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 10, StudentId = 6, StopId = 2, ScheduleTimeId = 26, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate },
            // Student 13 (Nokwanda Dube) - Stop 4: ToCampus 06:45 (ID=58), FromCampus 14:30 (ID=63)
            new PrioritySchedule { PriorityScheduleId = 11, StudentId = 13, StopId = 4, ScheduleTimeId = 58, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 12, StudentId = 13, StopId = 4, ScheduleTimeId = 58, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 13, StudentId = 13, StopId = 4, ScheduleTimeId = 63, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 14, StudentId = 13, StopId = 4, ScheduleTimeId = 63, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate }
        );
    }
}
