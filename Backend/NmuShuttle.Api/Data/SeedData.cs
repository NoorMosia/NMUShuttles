using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Data;

/// <summary>
/// Seeds configuration-level data for development and testing.
/// All data matches the canonical seed.md specification.
/// 
/// Runtime data (TripInstances, Bookings, etc.) is seeded programmatically
/// by HistoricalDataSeeder — not via HasData().
/// </summary>
public static class SeedData
{
    private static readonly DateTime SeedDate = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime SeedDate2 = new DateTime(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime DeviceRegDate = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc);

    // Pre-computed BCrypt hashes (cost factor 12)
    private const string AdminPasswordHash =
        "$2a$12$UnzkP2CPSCL04uIvla3UKOpZAVQZFEPRngF12SJyE2.gdVhHlZBk6"; // Admin@2025!
    private const string TransportPasswordHash =
        "$2a$12$YqZo4jPNiOcMGYhDRJ4CleeMqptACgPleJfWR/n.UJKTol9pZfE0i"; // Transport@2025!

    public static void Apply(ModelBuilder modelBuilder)
    {
        SeedAdmins(modelBuilder);
        SeedCampuses(modelBuilder);
        SeedStops(modelBuilder);
        SeedVendors(modelBuilder);
        SeedShuttles(modelBuilder);
        SeedStopShuttleDefaults(modelBuilder);
        SeedMarshals(modelBuilder);
        SeedStudents(modelBuilder);
        SeedScheduleTimes(modelBuilder);
        SeedScheduleTimeCampuses(modelBuilder);
        SeedCalendarExceptions(modelBuilder);
        SeedPrioritySchedules(modelBuilder);
        SeedDeviceRegistrations(modelBuilder);
    }

    private static void SeedAdmins(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>().HasData(
            new Admin { AdminId = 1, Email = "admin@mandela.ac.za", StaffNumber = "7001001", PasswordHash = AdminPasswordHash, FirstName = "Thabo", LastName = "Mokoena", IsActive = true, CreatedAt = SeedDate },
            new Admin { AdminId = 2, Email = "transport@mandela.ac.za", StaffNumber = "7001002", PasswordHash = TransportPasswordHash, FirstName = "Nomsa", LastName = "Dlamini", IsActive = true, CreatedAt = SeedDate }
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

    private static void SeedVendors(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vendor>().HasData(
            new Vendor { VendorId = 1, CompanyName = "Bay Transit Corp", ContactEmail = "info@baytransit.co.za", ContactPhone = "041-555-0101", CreatedAt = SeedDate },
            new Vendor { VendorId = 2, CompanyName = "Coastal Movers", ContactEmail = "info@coastalmovers.co.za", ContactPhone = "041-555-0202", CreatedAt = SeedDate },
            new Vendor { VendorId = 3, CompanyName = "Metro Link SA", ContactEmail = "info@metrolinksa.co.za", ContactPhone = "041-555-0303", CreatedAt = SeedDate }
        );
    }

    private static void SeedShuttles(ModelBuilder modelBuilder)
    {
        // Lean demo fleet: one shuttle per stop (ShuttleId == StopId for 1-8),
        // plus one spare (ShuttleId 9) used as the breakdown-replacement vehicle.
        //
        // Stops 1-5 carry priority (disability-cleared) students, so their single
        // shuttle must be accessible — priority bookings are pre-assigned to the
        // accessible front shuttle. Humewood (Stop 2) is intentionally a 2-seater
        // to demonstrate the waiting-list flow (fills after 2 bookings).
        modelBuilder.Entity<Shuttle>().HasData(
            new Shuttle { ShuttleId = 1, LicensePlate = "EC 101-001", Capacity = 22, VendorId = 1, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 2, LicensePlate = "EC 102-002", Capacity = 2, VendorId = 1, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 3, LicensePlate = "EC 103-003", Capacity = 22, VendorId = 2, VendorCompanyName = "Coastal Movers", IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 4, LicensePlate = "EC 104-004", Capacity = 28, VendorId = 3, VendorCompanyName = "Metro Link SA", IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 5, LicensePlate = "EC 105-005", Capacity = 22, VendorId = 1, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = true, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 6, LicensePlate = "EC 106-006", Capacity = 35, VendorId = 2, VendorCompanyName = "Coastal Movers", IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 7, LicensePlate = "EC 107-007", Capacity = 28, VendorId = 3, VendorCompanyName = "Metro Link SA", IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            new Shuttle { ShuttleId = 8, LicensePlate = "EC 108-008", Capacity = 35, VendorId = 1, VendorCompanyName = "Bay Transit Corp", IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate },
            // Spare — not a stop default; dispatched as the replacement in the breakdown demo.
            new Shuttle { ShuttleId = 9, LicensePlate = "EC 109-009", Capacity = 28, VendorId = 2, VendorCompanyName = "Coastal Movers", IsAccessibleVehicle = false, IsActive = true, CreatedAt = SeedDate }
        );
    }

    private static void SeedStopShuttleDefaults(ModelBuilder modelBuilder)
    {
        // Each stop has exactly one default shuttle at the front (OrderPosition 1).
        // ShuttleId mirrors StopId for 1-8. Humewood (Stop 2) is the 2-seater.
        modelBuilder.Entity<StopShuttleDefault>().HasData(
            new StopShuttleDefault { StopShuttleDefaultId = 1, StopId = 1, ShuttleId = 1, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 2, StopId = 2, ShuttleId = 2, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 3, StopId = 3, ShuttleId = 3, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 4, StopId = 4, ShuttleId = 4, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 5, StopId = 5, ShuttleId = 5, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 6, StopId = 6, ShuttleId = 6, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 7, StopId = 7, ShuttleId = 7, OrderPosition = 1, CreatedAt = SeedDate },
            new StopShuttleDefault { StopShuttleDefaultId = 8, StopId = 8, ShuttleId = 8, OrderPosition = 1, CreatedAt = SeedDate }
        );
    }

    private static void SeedMarshals(ModelBuilder modelBuilder)
    {
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

    private static void SeedStudents(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>().HasData(
            // Original 15 students
            new Student { StudentId = 1, StudentNumber = "s312847563", FirstName = "Sarah", LastName = "Nkosi", UniversityEmail = "s312847563@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 2, StudentNumber = "s319284731", FirstName = "Liam", LastName = "van der Merwe", UniversityEmail = "s319284731@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 3, StudentNumber = "s321056839", FirstName = "Ayanda", LastName = "Mthembu", UniversityEmail = "s321056839@mandela.ac.za", IsPriorityUser = true, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 4, StudentNumber = "s317493208", FirstName = "Chloe", LastName = "Botha", UniversityEmail = "s317493208@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 5, StudentNumber = "s329187465", FirstName = "Sipho", LastName = "Zulu", UniversityEmail = "s329187465@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 6, StudentNumber = "s318274056", FirstName = "Jessica", LastName = "Adams", UniversityEmail = "s318274056@mandela.ac.za", IsPriorityUser = true, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 7, StudentNumber = "s322941587", FirstName = "Mandla", LastName = "Ndlovu", UniversityEmail = "s322941587@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 8, StudentNumber = "s316785234", FirstName = "Regan", LastName = "Pieterse", UniversityEmail = "s316785234@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 9, StudentNumber = "s325098471", FirstName = "Zanele", LastName = "Khumalo", UniversityEmail = "s325098471@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 10, StudentNumber = "s311653928", FirstName = "Devon", LastName = "Jacobs", UniversityEmail = "s311653928@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 11, StudentNumber = "s328417509", FirstName = "Thandi", LastName = "Sithole", UniversityEmail = "s328417509@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 12, StudentNumber = "s314928673", FirstName = "Jason", LastName = "Williams", UniversityEmail = "s314928673@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 13, StudentNumber = "s327651084", FirstName = "Nokwanda", LastName = "Dube", UniversityEmail = "s327651084@mandela.ac.za", IsPriorityUser = true, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 14, StudentNumber = "s319073845", FirstName = "Michael", LastName = "van Niekerk", UniversityEmail = "s319073845@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            new Student { StudentId = 15, StudentNumber = "s323846190", FirstName = "Lerato", LastName = "Molefe", UniversityEmail = "s323846190@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate },
            // New students 16-60
            new Student { StudentId = 16, StudentNumber = "s314527896", FirstName = "Thabo", LastName = "Maseko", UniversityEmail = "s314527896@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 17, StudentNumber = "s328193045", FirstName = "Amahle", LastName = "Nxumalo", UniversityEmail = "s328193045@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 18, StudentNumber = "s311847290", FirstName = "Ethan", LastName = "du Plessis", UniversityEmail = "s311847290@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 19, StudentNumber = "s325604183", FirstName = "Naledi", LastName = "Tshabalala", UniversityEmail = "s325604183@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 20, StudentNumber = "s319438267", FirstName = "Kwame", LastName = "Osei", UniversityEmail = "s319438267@mandela.ac.za", IsPriorityUser = true, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 21, StudentNumber = "s322785410", FirstName = "Bianca", LastName = "Ferreira", UniversityEmail = "s322785410@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 22, StudentNumber = "s316094358", FirstName = "Siyabonga", LastName = "Mhlongo", UniversityEmail = "s316094358@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 23, StudentNumber = "s329471023", FirstName = "Megan", LastName = "Swanepoel", UniversityEmail = "s329471023@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 24, StudentNumber = "s313268947", FirstName = "Thandolwethu", LastName = "Gumede", UniversityEmail = "s313268947@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 25, StudentNumber = "s327815604", FirstName = "Ryan", LastName = "Cloete", UniversityEmail = "s327815604@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 26, StudentNumber = "s311542789", FirstName = "Nompumelelo", LastName = "Zwane", UniversityEmail = "s311542789@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 27, StudentNumber = "s324896031", FirstName = "Dylan", LastName = "Viljoen", UniversityEmail = "s324896031@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 28, StudentNumber = "s318623475", FirstName = "Palesa", LastName = "Mokoena", UniversityEmail = "s318623475@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 29, StudentNumber = "s326057184", FirstName = "Luca", LastName = "Naude", UniversityEmail = "s326057184@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 30, StudentNumber = "s312384956", FirstName = "Zinhle", LastName = "Buthelezi", UniversityEmail = "s312384956@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 31, StudentNumber = "s329710438", FirstName = "Joshua", LastName = "Erasmus", UniversityEmail = "s329710438@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 32, StudentNumber = "s315847291", FirstName = "Busisiwe", LastName = "Mabaso", UniversityEmail = "s315847291@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 33, StudentNumber = "s323196804", FirstName = "Luke", LastName = "Barnard", UniversityEmail = "s323196804@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 34, StudentNumber = "s317429058", FirstName = "Nonhlanhla", LastName = "Nkabinde", UniversityEmail = "s317429058@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 35, StudentNumber = "s321863742", FirstName = "Thato", LastName = "Letsoko", UniversityEmail = "s321863742@mandela.ac.za", IsPriorityUser = true, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 36, StudentNumber = "s314085267", FirstName = "Hannah", LastName = "Muller", UniversityEmail = "s314085267@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 37, StudentNumber = "s328574190", FirstName = "Lungelo", LastName = "Shabangu", UniversityEmail = "s328574190@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 38, StudentNumber = "s316238904", FirstName = "Amber", LastName = "Joubert", UniversityEmail = "s316238904@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 39, StudentNumber = "s322491637", FirstName = "Sbusiso", LastName = "Radebe", UniversityEmail = "s322491637@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 40, StudentNumber = "s319764082", FirstName = "Tamsin", LastName = "Olivier", UniversityEmail = "s319764082@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 41, StudentNumber = "s325138476", FirstName = "Buhle", LastName = "Mahlangu", UniversityEmail = "s325138476@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 42, StudentNumber = "s313692815", FirstName = "Connor", LastName = "le Roux", UniversityEmail = "s313692815@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 43, StudentNumber = "s327045963", FirstName = "Sinethemba", LastName = "Majola", UniversityEmail = "s327045963@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 44, StudentNumber = "s311928347", FirstName = "Kayla", LastName = "Steyn", UniversityEmail = "s311928347@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 45, StudentNumber = "s324581076", FirstName = "Andile", LastName = "Dladla", UniversityEmail = "s324581076@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 46, StudentNumber = "s318305492", FirstName = "Emma", LastName = "van Wyk", UniversityEmail = "s318305492@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            // Inactive (struck out) students
            new Student { StudentId = 47, StudentNumber = "s326742815", FirstName = "Themba", LastName = "Vilakazi", UniversityEmail = "s326742815@mandela.ac.za", IsPriorityUser = false, IsActive = false, CreatedAt = SeedDate2 },
            new Student { StudentId = 48, StudentNumber = "s312059638", FirstName = "Danielle", LastName = "Potgieter", UniversityEmail = "s312059638@mandela.ac.za", IsPriorityUser = false, IsActive = false, CreatedAt = SeedDate2 },
            new Student { StudentId = 49, StudentNumber = "s329384071", FirstName = "Mthunzi", LastName = "Cele", UniversityEmail = "s329384071@mandela.ac.za", IsPriorityUser = false, IsActive = false, CreatedAt = SeedDate2 },
            new Student { StudentId = 50, StudentNumber = "s315670284", FirstName = "Robyn", LastName = "Prinsloo", UniversityEmail = "s315670284@mandela.ac.za", IsPriorityUser = false, IsActive = false, CreatedAt = SeedDate2 },
            // Remaining active students
            new Student { StudentId = 51, StudentNumber = "s323947158", FirstName = "Anathi", LastName = "Magwaza", UniversityEmail = "s323947158@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 52, StudentNumber = "s317283046", FirstName = "Jaden", LastName = "Booysen", UniversityEmail = "s317283046@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 53, StudentNumber = "s325816490", FirstName = "Lindiwe", LastName = "Ngubane", UniversityEmail = "s325816490@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 54, StudentNumber = "s311475823", FirstName = "Aiden", LastName = "Marais", UniversityEmail = "s311475823@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 55, StudentNumber = "s328640175", FirstName = "Nandi", LastName = "Mthimkhulu", UniversityEmail = "s328640175@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 56, StudentNumber = "s314903268", FirstName = "Caleb", LastName = "Bezuidenhout", UniversityEmail = "s314903268@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 57, StudentNumber = "s322167549", FirstName = "Yonela", LastName = "Sifuba", UniversityEmail = "s322167549@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 58, StudentNumber = "s316538291", FirstName = "Mia", LastName = "Hendriks", UniversityEmail = "s316538291@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 59, StudentNumber = "s329081467", FirstName = "Khaya", LastName = "Mkhwanazi", UniversityEmail = "s329081467@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 },
            new Student { StudentId = 60, StudentNumber = "s313724850", FirstName = "Jade", LastName = "van Rensburg", UniversityEmail = "s313724850@mandela.ac.za", IsPriorityUser = false, IsActive = true, CreatedAt = SeedDate2 }
        );
    }

    private static void SeedScheduleTimes(ModelBuilder modelBuilder)
    {
        var id = 1;
        var schedules = new List<ScheduleTime>();

        for (var stopId = 1; stopId <= 8; stopId++)
        {
            // Weekday ToCampus: 06:45, 07:45, 08:45, 09:45
            foreach (var time in new[] { new TimeOnly(6, 45), new TimeOnly(7, 45), new TimeOnly(8, 45), new TimeOnly(9, 45) })
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "ToCampus", ScheduleType = "Weekday", StopId = stopId, RequiresScanning = true });

            // Weekday FromCampus: 12:30, 14:30, 16:00, 17:30, 18:30, 20:30, 22:00
            foreach (var time in new[] { new TimeOnly(12, 30), new TimeOnly(14, 30), new TimeOnly(16, 0), new TimeOnly(17, 30), new TimeOnly(18, 30), new TimeOnly(20, 30), new TimeOnly(22, 0) })
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "FromCampus", ScheduleType = "Weekday", StopId = stopId, RequiresScanning = true });

            // Saturday ToCampus: 06:45, 07:45, 08:45, 09:45
            foreach (var time in new[] { new TimeOnly(6, 45), new TimeOnly(7, 45), new TimeOnly(8, 45), new TimeOnly(9, 45) })
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "ToCampus", ScheduleType = "Saturday", StopId = stopId, RequiresScanning = true });

            // Saturday FromCampus: 12:30, 14:30, 16:00, 17:30
            foreach (var time in new[] { new TimeOnly(12, 30), new TimeOnly(14, 30), new TimeOnly(16, 0), new TimeOnly(17, 30) })
                schedules.Add(new ScheduleTime { ScheduleTimeId = id++, DepartureTime = time, Direction = "FromCampus", ScheduleType = "Saturday", StopId = stopId, RequiresScanning = true });
        }

        modelBuilder.Entity<ScheduleTime>().HasData(schedules.ToArray());
    }

    private static void SeedScheduleTimeCampuses(ModelBuilder modelBuilder)
    {
        var campuses = new List<ScheduleTimeCampus>();
        for (var i = 1; i <= 152; i++)
            campuses.Add(new ScheduleTimeCampus { ScheduleTimeCampusId = i, ScheduleTimeId = i, CampusId = 1, StopOrder = 1 });

        modelBuilder.Entity<ScheduleTimeCampus>().HasData(campuses.ToArray());
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
            new CalendarException { CalendarExceptionId = 10, TargetDate = new DateOnly(2025, 9, 24), ExceptionType = "PublicHoliday", Description = "Heritage Day" },
            new CalendarException { CalendarExceptionId = 11, TargetDate = new DateOnly(2026, 3, 21), ExceptionType = "PublicHoliday", Description = "Human Rights Day" },
            new CalendarException { CalendarExceptionId = 12, TargetDate = new DateOnly(2026, 4, 3), ExceptionType = "PublicHoliday", Description = "Good Friday" },
            new CalendarException { CalendarExceptionId = 13, TargetDate = new DateOnly(2026, 4, 6), ExceptionType = "PublicHoliday", Description = "Family Day" },
            new CalendarException { CalendarExceptionId = 14, TargetDate = new DateOnly(2026, 4, 27), ExceptionType = "PublicHoliday", Description = "Freedom Day" },
            new CalendarException { CalendarExceptionId = 15, TargetDate = new DateOnly(2026, 5, 1), ExceptionType = "PublicHoliday", Description = "Workers Day" },
            new CalendarException { CalendarExceptionId = 16, TargetDate = new DateOnly(2026, 6, 16), ExceptionType = "PublicHoliday", Description = "Youth Day" },
            new CalendarException { CalendarExceptionId = 17, TargetDate = new DateOnly(2026, 8, 10), ExceptionType = "PublicHoliday", Description = "National Women's Day (observed)" },
            new CalendarException { CalendarExceptionId = 18, TargetDate = new DateOnly(2026, 9, 24), ExceptionType = "PublicHoliday", Description = "Heritage Day" }
        );
    }

    private static void SeedPrioritySchedules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrioritySchedule>().HasData(
            // Student 3 (Ayanda) — Stop 1, ToCampus 06:45 (ID=1) + FromCampus 16:00 (ID=7), Mon/Wed/Fri
            new PrioritySchedule { PriorityScheduleId = 1, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 2, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 3, StudentId = 3, StopId = 1, ScheduleTimeId = 1, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 4, StudentId = 3, StopId = 1, ScheduleTimeId = 7, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 5, StudentId = 3, StopId = 1, ScheduleTimeId = 7, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 6, StudentId = 3, StopId = 1, ScheduleTimeId = 7, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate },
            // Student 6 (Jessica) — Stop 2, ToCampus 06:45 (ID=20) + FromCampus 16:00 (ID=26), Tue/Thu
            new PrioritySchedule { PriorityScheduleId = 7, StudentId = 6, StopId = 2, ScheduleTimeId = 20, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 8, StudentId = 6, StopId = 2, ScheduleTimeId = 20, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 9, StudentId = 6, StopId = 2, ScheduleTimeId = 26, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 10, StudentId = 6, StopId = 2, ScheduleTimeId = 26, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate },
            // Student 13 (Nokwanda) — Stop 4, ToCampus 06:45 (ID=58) + FromCampus 14:30 (ID=63), Mon/Wed
            new PrioritySchedule { PriorityScheduleId = 11, StudentId = 13, StopId = 4, ScheduleTimeId = 58, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 12, StudentId = 13, StopId = 4, ScheduleTimeId = 58, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 13, StudentId = 13, StopId = 4, ScheduleTimeId = 63, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate },
            new PrioritySchedule { PriorityScheduleId = 14, StudentId = 13, StopId = 4, ScheduleTimeId = 63, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate },
            // Student 20 (Kwame) — Stop 3, ToCampus 06:45 (ID=39) + FromCampus 16:00 (ID=45), Mon/Tue/Wed/Thu
            new PrioritySchedule { PriorityScheduleId = 15, StudentId = 20, StopId = 3, ScheduleTimeId = 39, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 16, StudentId = 20, StopId = 3, ScheduleTimeId = 39, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 17, StudentId = 20, StopId = 3, ScheduleTimeId = 39, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 18, StudentId = 20, StopId = 3, ScheduleTimeId = 39, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 19, StudentId = 20, StopId = 3, ScheduleTimeId = 45, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 20, StudentId = 20, StopId = 3, ScheduleTimeId = 45, DayOfWeek = "Tuesday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 21, StudentId = 20, StopId = 3, ScheduleTimeId = 45, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 22, StudentId = 20, StopId = 3, ScheduleTimeId = 45, DayOfWeek = "Thursday", IsActive = true, CreatedAt = SeedDate2 },
            // Student 35 (Thato) — Stop 5, ToCampus 06:45 (ID=77) + FromCampus 16:00 (ID=83), Mon/Wed/Fri
            new PrioritySchedule { PriorityScheduleId = 23, StudentId = 35, StopId = 5, ScheduleTimeId = 77, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 24, StudentId = 35, StopId = 5, ScheduleTimeId = 77, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 25, StudentId = 35, StopId = 5, ScheduleTimeId = 77, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 26, StudentId = 35, StopId = 5, ScheduleTimeId = 83, DayOfWeek = "Monday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 27, StudentId = 35, StopId = 5, ScheduleTimeId = 83, DayOfWeek = "Wednesday", IsActive = true, CreatedAt = SeedDate2 },
            new PrioritySchedule { PriorityScheduleId = 28, StudentId = 35, StopId = 5, ScheduleTimeId = 83, DayOfWeek = "Friday", IsActive = true, CreatedAt = SeedDate2 }
        );
    }

    private static void SeedDeviceRegistrations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceRegistration>().HasData(
            // Students
            new DeviceRegistration { DeviceRegistrationId = 1, OwnerId = 1, OwnerType = "Student", FcmToken = "fcm_token_student_1_a1b2c3d4e5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 2, OwnerId = 2, OwnerType = "Student", FcmToken = "fcm_token_student_2_f6g7h8i9j0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 3, OwnerId = 3, OwnerType = "Student", FcmToken = "fcm_token_student_3_k1l2m3n4o5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 4, OwnerId = 4, OwnerType = "Student", FcmToken = "fcm_token_student_4_p6q7r8s9t0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 5, OwnerId = 5, OwnerType = "Student", FcmToken = "fcm_token_student_5_u1v2w3x4y5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 6, OwnerId = 6, OwnerType = "Student", FcmToken = "fcm_token_student_6_z6a7b8c9d0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 7, OwnerId = 7, OwnerType = "Student", FcmToken = "fcm_token_student_7_e1f2g3h4i5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 8, OwnerId = 8, OwnerType = "Student", FcmToken = "fcm_token_student_8_j6k7l8m9n0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 9, OwnerId = 9, OwnerType = "Student", FcmToken = "fcm_token_student_9_o1p2q3r4s5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 10, OwnerId = 10, OwnerType = "Student", FcmToken = "fcm_token_student_10_t6u7v8w9x0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 11, OwnerId = 13, OwnerType = "Student", FcmToken = "fcm_token_student_13_y1z2a3b4c5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 12, OwnerId = 16, OwnerType = "Student", FcmToken = "fcm_token_student_16_d6e7f8g9h0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 13, OwnerId = 20, OwnerType = "Student", FcmToken = "fcm_token_student_20_i1j2k3l4m5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 14, OwnerId = 25, OwnerType = "Student", FcmToken = "fcm_token_student_25_n6o7p8q9r0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 15, OwnerId = 30, OwnerType = "Student", FcmToken = "fcm_token_student_30_s1t2u3v4w5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 16, OwnerId = 35, OwnerType = "Student", FcmToken = "fcm_token_student_35_x6y7z8a9b0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 17, OwnerId = 40, OwnerType = "Student", FcmToken = "fcm_token_student_40_c1d2e3f4g5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 18, OwnerId = 45, OwnerType = "Student", FcmToken = "fcm_token_student_45_h6i7j8k9l0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 19, OwnerId = 50, OwnerType = "Student", FcmToken = "fcm_token_student_50_m1n2o3p4q5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 20, OwnerId = 55, OwnerType = "Student", FcmToken = "fcm_token_student_55_r6s7t8u9v0", Platform = "android", UpdatedAt = DeviceRegDate },
            // Marshals
            new DeviceRegistration { DeviceRegistrationId = 21, OwnerId = 1, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_1_w1x2y3z4a5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 22, OwnerId = 2, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_2_b6c7d8e9f0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 23, OwnerId = 3, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_3_g1h2i3j4k5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 24, OwnerId = 4, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_4_l6m7n8o9p0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 25, OwnerId = 5, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_5_q1r2s3t4u5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 26, OwnerId = 6, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_6_v6w7x8y9z0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 27, OwnerId = 7, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_7_a1b2c3d4e5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 28, OwnerId = 8, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_8_f6g7h8i9j0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 29, OwnerId = 9, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_9_k1l2m3n4o5", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 30, OwnerId = 10, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_10_p6q7r8s9t0", Platform = "android", UpdatedAt = DeviceRegDate },
            new DeviceRegistration { DeviceRegistrationId = 31, OwnerId = 11, OwnerType = "Marshal", FcmToken = "fcm_token_marshal_11_u1v2w3x4y5", Platform = "android", UpdatedAt = DeviceRegDate }
        );
    }
}
