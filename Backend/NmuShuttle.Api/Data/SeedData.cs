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
                PasswordHash = AdminPasswordHash,
                FirstName = "Thabo",
                LastName = "Mokoena",
                CreatedAt = SeedDate
            },
            new Admin
            {
                AdminId = 2,
                Email = "transport@mandela.ac.za",
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
            new Stop { StopId = 1, StopName = "Summerstrand Village", FullAddress = "12 Marine Drive, Summerstrand, Gqeberha, 6001", GpsCoordinates = "-33.9680,25.6450", IsActive = true },
            new Stop { StopId = 2, StopName = "Humewood", FullAddress = "45 Beach Road, Humewood, Gqeberha, 6001", GpsCoordinates = "-33.9630,25.6280", IsActive = true },
            new Stop { StopId = 3, StopName = "Newton Park", FullAddress = "78 Cape Road, Newton Park, Gqeberha, 6045", GpsCoordinates = "-33.9480,25.5750", IsActive = true },
            new Stop { StopId = 4, StopName = "Walmer Heights", FullAddress = "23 Heugh Road, Walmer Heights, Gqeberha, 6070", GpsCoordinates = "-33.9850,25.6100", IsActive = true },
            new Stop { StopId = 5, StopName = "Lorraine", FullAddress = "91 Circular Drive, Lorraine, Gqeberha, 6070", GpsCoordinates = "-33.9320,25.5620", IsActive = true },
            new Stop { StopId = 6, StopName = "Bluewater Bay", FullAddress = "5 Seaview Crescent, Bluewater Bay, Gqeberha, 6210", GpsCoordinates = "-33.8710,25.5590", IsActive = true },
            new Stop { StopId = 7, StopName = "Framesby", FullAddress = "14 Langenhoven Drive, Framesby, Gqeberha, 6045", GpsCoordinates = "-33.9260,25.5810", IsActive = true },
            new Stop { StopId = 8, StopName = "Greenacres", FullAddress = "30 Ring Road, Greenacres, Gqeberha, 6045", GpsCoordinates = "-33.9410,25.5870", IsActive = true }
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
            new Marshal { MarshalId = 1, StaffNumber = "4827391", FirstName = "Bongani", LastName = "Cele", UniversityEmail = "b.cele@mandela.ac.za", AssignedCampusId = 1, AssignedStopId = null, CreatedAt = SeedDate },
            new Marshal { MarshalId = 2, StaffNumber = "5914826", FirstName = "Karen", LastName = "Swart", UniversityEmail = "k.swart@mandela.ac.za", AssignedCampusId = 2, AssignedStopId = null, CreatedAt = SeedDate },
            new Marshal { MarshalId = 3, StaffNumber = "3208574", FirstName = "Vuyo", LastName = "Mabena", UniversityEmail = "v.mabena@mandela.ac.za", AssignedCampusId = 3, AssignedStopId = null, CreatedAt = SeedDate },
            new Marshal { MarshalId = 4, StaffNumber = "6153042", FirstName = "Thulani", LastName = "Ngcobo", UniversityEmail = "t.ngcobo@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 1, CreatedAt = SeedDate },
            new Marshal { MarshalId = 5, StaffNumber = "7294185", FirstName = "Lindiwe", LastName = "Shabalala", UniversityEmail = "l.shabalala@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 2, CreatedAt = SeedDate },
            new Marshal { MarshalId = 6, StaffNumber = "8401267", FirstName = "Pieter", LastName = "Venter", UniversityEmail = "p.venter@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 3, CreatedAt = SeedDate },
            new Marshal { MarshalId = 7, StaffNumber = "9537148", FirstName = "Nombuso", LastName = "Mkhize", UniversityEmail = "n.mkhize@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 4, CreatedAt = SeedDate },
            new Marshal { MarshalId = 8, StaffNumber = "2648309", FirstName = "Craig", LastName = "Daniels", UniversityEmail = "c.daniels@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 5, CreatedAt = SeedDate },
            new Marshal { MarshalId = 9, StaffNumber = "1785423", FirstName = "Noluthando", LastName = "Mthethwa", UniversityEmail = "n.mthethwa@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 6, CreatedAt = SeedDate },
            new Marshal { MarshalId = 10, StaffNumber = "4062891", FirstName = "Willem", LastName = "Pretorius", UniversityEmail = "w.pretorius@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 7, CreatedAt = SeedDate },
            new Marshal { MarshalId = 11, StaffNumber = "5379216", FirstName = "Zandile", LastName = "Mdluli", UniversityEmail = "z.mdluli@mandela.ac.za", AssignedCampusId = null, AssignedStopId = 8, CreatedAt = SeedDate }
        );
    }

    private static void SeedShuttles(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Shuttle>().HasData(
            new Shuttle { ShuttleId = 1, LicensePlate = "EC 142-837", Capacity = 22, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 2, LicensePlate = "EC 256-491", Capacity = 35, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = false, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 3, LicensePlate = "EC 389-720", Capacity = 22, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = false, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 4, LicensePlate = "EC 501-163", Capacity = 28, VendorCompanyName = "Coastal Movers", IsAccessibleVehicle = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 5, LicensePlate = "EC 628-945", Capacity = 35, VendorCompanyName = "Coastal Movers", IsAccessibleVehicle = false, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 6, LicensePlate = "EC 714-382", Capacity = 22, VendorCompanyName = "Coastal Movers", IsAccessibleVehicle = false, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 7, LicensePlate = "EC 837-256", Capacity = 28, VendorCompanyName = "Metro Link SA", IsAccessibleVehicle = false, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 8, LicensePlate = "EC 948-617", Capacity = 35, VendorCompanyName = "Metro Link SA", IsAccessibleVehicle = false, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 9, LicensePlate = "EC 103-475", Capacity = 22, VendorCompanyName = "Metro Link SA", IsAccessibleVehicle = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 10, LicensePlate = "EC 217-864", Capacity = 28, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = false, CreatedAt = SeedDate }
        );
    }

    private static void SeedScheduleTimes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduleTime>().HasData(
            // ToCampus Weekday (6 entries)
            new ScheduleTime { ScheduleTimeId = 1, DepartureTime = new TimeOnly(6, 30), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 1 },
            new ScheduleTime { ScheduleTimeId = 2, DepartureTime = new TimeOnly(6, 30), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 2 },
            new ScheduleTime { ScheduleTimeId = 3, DepartureTime = new TimeOnly(7, 15), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 3 },
            new ScheduleTime { ScheduleTimeId = 4, DepartureTime = new TimeOnly(7, 15), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 2, StopId = 4 },
            new ScheduleTime { ScheduleTimeId = 5, DepartureTime = new TimeOnly(7, 45), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 2, StopId = 5 },
            new ScheduleTime { ScheduleTimeId = 6, DepartureTime = new TimeOnly(8, 30), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 3, StopId = 6 },
            // FromCampus Weekday (8 entries)
            new ScheduleTime { ScheduleTimeId = 7, DepartureTime = new TimeOnly(12, 30), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 1 },
            new ScheduleTime { ScheduleTimeId = 8, DepartureTime = new TimeOnly(12, 30), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 2 },
            new ScheduleTime { ScheduleTimeId = 9, DepartureTime = new TimeOnly(14, 0), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 3 },
            new ScheduleTime { ScheduleTimeId = 10, DepartureTime = new TimeOnly(14, 0), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 2, StopId = 4 },
            new ScheduleTime { ScheduleTimeId = 11, DepartureTime = new TimeOnly(16, 30), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 1 },
            new ScheduleTime { ScheduleTimeId = 12, DepartureTime = new TimeOnly(16, 30), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 2 },
            new ScheduleTime { ScheduleTimeId = 13, DepartureTime = new TimeOnly(16, 30), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 2, StopId = 5 },
            new ScheduleTime { ScheduleTimeId = 14, DepartureTime = new TimeOnly(17, 15), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 3, StopId = 6 },
            // Saturday (4 entries)
            new ScheduleTime { ScheduleTimeId = 15, DepartureTime = new TimeOnly(8, 0), Direction = "ToCampus", ScheduleType = "Saturday", CampusId = 1, StopId = 1 },
            new ScheduleTime { ScheduleTimeId = 16, DepartureTime = new TimeOnly(8, 0), Direction = "ToCampus", ScheduleType = "Saturday", CampusId = 1, StopId = 2 },
            new ScheduleTime { ScheduleTimeId = 17, DepartureTime = new TimeOnly(13, 0), Direction = "FromCampus", ScheduleType = "Saturday", CampusId = 1, StopId = 1 },
            new ScheduleTime { ScheduleTimeId = 18, DepartureTime = new TimeOnly(13, 0), Direction = "FromCampus", ScheduleType = "Saturday", CampusId = 1, StopId = 2 },
            // Stops 7 & 8 Weekday (4 entries)
            new ScheduleTime { ScheduleTimeId = 19, DepartureTime = new TimeOnly(7, 45), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 7 },
            new ScheduleTime { ScheduleTimeId = 20, DepartureTime = new TimeOnly(8, 0), Direction = "ToCampus", ScheduleType = "Weekday", CampusId = 2, StopId = 8 },
            new ScheduleTime { ScheduleTimeId = 21, DepartureTime = new TimeOnly(16, 30), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 1, StopId = 7 },
            new ScheduleTime { ScheduleTimeId = 22, DepartureTime = new TimeOnly(17, 00), Direction = "FromCampus", ScheduleType = "Weekday", CampusId = 2, StopId = 8 }
        );
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
        modelBuilder.Entity<PrioritySchedule>().HasData(
            // Student 3 (Ayanda Mthembu) - Stop 1 (Summerstrand Village)
            new PrioritySchedule { PriorityScheduleId = 1, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 2, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 3, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 4, StudentId = 3, StopId = 1, ScheduleTimeId = 11, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 5, StudentId = 3, StopId = 1, ScheduleTimeId = 11, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 6, StudentId = 3, StopId = 1, ScheduleTimeId = 11, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate },
            // Student 6 (Jessica Adams) - Stop 2 (Humewood)
            new PrioritySchedule { PriorityScheduleId = 7, StudentId = 6, StopId = 2, ScheduleTimeId = 2, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 8, StudentId = 6, StopId = 2, ScheduleTimeId = 2, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 9, StudentId = 6, StopId = 2, ScheduleTimeId = 12, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 10, StudentId = 6, StopId = 2, ScheduleTimeId = 12, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate },
            // Student 13 (Nokwanda Dube) - Stop 4 (Walmer Heights)
            new PrioritySchedule { PriorityScheduleId = 11, StudentId = 13, StopId = 4, ScheduleTimeId = 4, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 12, StudentId = 13, StopId = 4, ScheduleTimeId = 4, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 13, StudentId = 13, StopId = 4, ScheduleTimeId = 10, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 14, StudentId = 13, StopId = 4, ScheduleTimeId = 10, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate }
        );
    }
}
