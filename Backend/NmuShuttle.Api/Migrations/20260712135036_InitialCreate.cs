using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NmuShuttle.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "CalendarExceptions",
                columns: table => new
                {
                    CalendarExceptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExceptionType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarExceptions", x => x.CalendarExceptionId);
                    table.CheckConstraint("CK_CalendarExceptions_ExceptionType", "ExceptionType IN ('PublicHoliday','UniversityRecess')");
                });

            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    CampusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GpsCoordinates = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.CampusId);
                });

            migrationBuilder.CreateTable(
                name: "DeviceRegistrations",
                columns: table => new
                {
                    DeviceRegistrationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    OwnerType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FcmToken = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Platform = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceRegistrations", x => x.DeviceRegistrationId);
                    table.CheckConstraint("CK_DeviceRegistrations_OwnerType", "OwnerType IN ('Student','Marshal')");
                    table.CheckConstraint("CK_DeviceRegistrations_Platform", "Platform IN ('android','ios')");
                });

            migrationBuilder.CreateTable(
                name: "Shuttles",
                columns: table => new
                {
                    ShuttleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    VendorCompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsAccessibleVehicle = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shuttles", x => x.ShuttleId);
                    table.CheckConstraint("CK_Shuttles_Capacity", "Capacity > 0");
                });

            migrationBuilder.CreateTable(
                name: "Stops",
                columns: table => new
                {
                    StopId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StopName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FullAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GpsCoordinates = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stops", x => x.StopId);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    StudentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UniversityEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsPriorityUser = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OtpHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                });

            migrationBuilder.CreateTable(
                name: "Marshals",
                columns: table => new
                {
                    MarshalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UniversityEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssignedCampusId = table.Column<int>(type: "int", nullable: true),
                    AssignedStopId = table.Column<int>(type: "int", nullable: true),
                    OtpHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marshals", x => x.MarshalId);
                    table.CheckConstraint("CK_Marshals_AssignedXOR", "(AssignedCampusId IS NOT NULL AND AssignedStopId IS NULL) OR (AssignedCampusId IS NULL AND AssignedStopId IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_Marshals_Campuses_AssignedCampusId",
                        column: x => x.AssignedCampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Marshals_Stops_AssignedStopId",
                        column: x => x.AssignedStopId,
                        principalTable: "Stops",
                        principalColumn: "StopId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleTimes",
                columns: table => new
                {
                    ScheduleTimeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartureTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Direction = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ScheduleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    StopId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTimes", x => x.ScheduleTimeId);
                    table.CheckConstraint("CK_ScheduleTimes_Direction", "Direction IN ('ToCampus','FromCampus')");
                    table.CheckConstraint("CK_ScheduleTimes_ScheduleType", "ScheduleType IN ('Weekday','Saturday')");
                    table.ForeignKey(
                        name: "FK_ScheduleTimes_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleTimes_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "StopId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrioritySchedules",
                columns: table => new
                {
                    PriorityScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    ScheduleTimeId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrioritySchedules", x => x.PriorityScheduleId);
                    table.CheckConstraint("CK_PrioritySchedules_DayOfWeek", "DayOfWeek IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday')");
                    table.ForeignKey(
                        name: "FK_PrioritySchedules_ScheduleTimes_ScheduleTimeId",
                        column: x => x.ScheduleTimeId,
                        principalTable: "ScheduleTimes",
                        principalColumn: "ScheduleTimeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrioritySchedules_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "StopId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrioritySchedules_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripInstances",
                columns: table => new
                {
                    TripInstanceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleTimeId = table.Column<int>(type: "int", nullable: false),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    TripDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Scheduling"),
                    LockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InTransitAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripInstances", x => x.TripInstanceId);
                    table.CheckConstraint("CK_TripInstances_Status", "Status IN ('Scheduling','Locked','InTransit','Completed','IncidentActive')");
                    table.ForeignKey(
                        name: "FK_TripInstances_ScheduleTimes_ScheduleTimeId",
                        column: x => x.ScheduleTimeId,
                        principalTable: "ScheduleTimes",
                        principalColumn: "ScheduleTimeId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripInstances_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "StopId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TripInstanceId = table.Column<int>(type: "int", nullable: false),
                    BookingStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Confirmed"),
                    BookingType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Standard"),
                    BoardedStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    QrCodeToken = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShuttleId = table.Column<int>(type: "int", nullable: true),
                    BoardedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.CheckConstraint("CK_Bookings_BoardedStatus", "BoardedStatus IN ('Pending','Boarded','NoShow')");
                    table.CheckConstraint("CK_Bookings_BookingStatus", "BookingStatus IN ('Confirmed','WaitingList','Cancelled')");
                    table.CheckConstraint("CK_Bookings_BookingType", "BookingType IN ('Standard','Priority')");
                    table.ForeignKey(
                        name: "FK_Bookings_Shuttles_ShuttleId",
                        column: x => x.ShuttleId,
                        principalTable: "Shuttles",
                        principalColumn: "ShuttleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_TripInstances_TripInstanceId",
                        column: x => x.TripInstanceId,
                        principalTable: "TripInstances",
                        principalColumn: "TripInstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BreakdownLogs",
                columns: table => new
                {
                    BreakdownLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripInstanceId = table.Column<int>(type: "int", nullable: false),
                    ShuttleId = table.Column<int>(type: "int", nullable: false),
                    VendorCompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FlaggedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FlaggedByMarshalId = table.Column<int>(type: "int", nullable: false),
                    ResolutionType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ReplacementShuttleId = table.Column<int>(type: "int", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedByAdminId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakdownLogs", x => x.BreakdownLogId);
                    table.CheckConstraint("CK_BreakdownLogs_ResolutionType", "ResolutionType IS NULL OR ResolutionType IN ('Replaced','Cancelled')");
                    table.ForeignKey(
                        name: "FK_BreakdownLogs_Admins_ResolvedByAdminId",
                        column: x => x.ResolvedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BreakdownLogs_Marshals_FlaggedByMarshalId",
                        column: x => x.FlaggedByMarshalId,
                        principalTable: "Marshals",
                        principalColumn: "MarshalId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BreakdownLogs_Shuttles_ReplacementShuttleId",
                        column: x => x.ReplacementShuttleId,
                        principalTable: "Shuttles",
                        principalColumn: "ShuttleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BreakdownLogs_Shuttles_ShuttleId",
                        column: x => x.ShuttleId,
                        principalTable: "Shuttles",
                        principalColumn: "ShuttleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BreakdownLogs_TripInstances_TripInstanceId",
                        column: x => x.TripInstanceId,
                        principalTable: "TripInstances",
                        principalColumn: "TripInstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    RelatedTripInstanceId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_TripInstances_RelatedTripInstanceId",
                        column: x => x.RelatedTripInstanceId,
                        principalTable: "TripInstances",
                        principalColumn: "TripInstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Submitted"),
                    TripInstanceId = table.Column<int>(type: "int", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.CheckConstraint("CK_Reports_Category", "Category IN ('DriverBehavior','TimetableIssue','VehicleQuality','Other')");
                    table.CheckConstraint("CK_Reports_Status", "Status IN ('Submitted','UnderReview','Resolved')");
                    table.ForeignKey(
                        name: "FK_Reports_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "StudentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reports_TripInstances_TripInstanceId",
                        column: x => x.TripInstanceId,
                        principalTable: "TripInstances",
                        principalColumn: "TripInstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripShuttleAllocations",
                columns: table => new
                {
                    AllocationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripInstanceId = table.Column<int>(type: "int", nullable: false),
                    ShuttleId = table.Column<int>(type: "int", nullable: false),
                    OrderPosition = table.Column<int>(type: "int", nullable: false),
                    IsOperational = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripShuttleAllocations", x => x.AllocationId);
                    table.ForeignKey(
                        name: "FK_TripShuttleAllocations_Shuttles_ShuttleId",
                        column: x => x.ShuttleId,
                        principalTable: "Shuttles",
                        principalColumn: "ShuttleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripShuttleAllocations_TripInstances_TripInstanceId",
                        column: x => x.TripInstanceId,
                        principalTable: "TripInstances",
                        principalColumn: "TripInstanceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "AdminId", "CreatedAt", "Email", "FirstName", "LastName", "PasswordHash" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "admin@mandela.ac.za", "Thabo", "Mokoena", "$2a$12$LJ3m4sMKfRzG8WkBVcXKxeQ9rF7bY1tZ5dN0pA2vH6wX4uC8yE3Oi" },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "transport@mandela.ac.za", "Nomsa", "Dlamini", "$2a$12$Qp7n5RRKgTzH9WmDVdYLxeA8sG6cZ2uW4jM1oB3vI7xY5wE9kF4Rj" }
                });

            migrationBuilder.InsertData(
                table: "CalendarExceptions",
                columns: new[] { "CalendarExceptionId", "Description", "ExceptionType", "TargetDate" },
                values: new object[,]
                {
                    { 1, "Human Rights Day", "PublicHoliday", new DateOnly(2025, 3, 21) },
                    { 2, "Good Friday", "PublicHoliday", new DateOnly(2025, 4, 18) },
                    { 3, "Family Day", "PublicHoliday", new DateOnly(2025, 4, 21) },
                    { 4, "Mid-year recess start", "UniversityRecess", new DateOnly(2025, 4, 28) },
                    { 5, "Mid-year recess", "UniversityRecess", new DateOnly(2025, 4, 29) },
                    { 6, "Mid-year recess", "UniversityRecess", new DateOnly(2025, 4, 30) },
                    { 7, "Workers Day", "PublicHoliday", new DateOnly(2025, 5, 1) },
                    { 8, "Youth Day", "PublicHoliday", new DateOnly(2025, 6, 16) },
                    { 9, "National Women's Day", "PublicHoliday", new DateOnly(2025, 8, 9) },
                    { 10, "Heritage Day", "PublicHoliday", new DateOnly(2025, 9, 24) }
                });

            migrationBuilder.InsertData(
                table: "Campuses",
                columns: new[] { "CampusId", "CampusName", "GpsCoordinates" },
                values: new object[,]
                {
                    { 1, "South Campus", "-33.9580,25.5920" },
                    { 2, "North Campus", "-33.9185,25.5870" },
                    { 3, "2nd Avenue Campus", "-33.9610,25.6100" }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName" },
                values: new object[] { 1, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 142-837", "Bay Transit Corp" });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "LicensePlate", "VendorCompanyName" },
                values: new object[,]
                {
                    { 2, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 256-491", "Bay Transit Corp" },
                    { 3, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 389-720", "Bay Transit Corp" }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "LicensePlate", "VendorCompanyName" },
                values: new object[] { 4, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 501-163", "Coastal Movers" });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "LicensePlate", "VendorCompanyName" },
                values: new object[,]
                {
                    { 5, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 628-945", "Coastal Movers" },
                    { 6, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 714-382", "Coastal Movers" },
                    { 7, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 837-256", "Metro Link SA" },
                    { 8, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "EC 948-617", "Metro Link SA" }
                });

            migrationBuilder.InsertData(
                table: "Stops",
                columns: new[] { "StopId", "FullAddress", "GpsCoordinates", "IsActive", "StopName" },
                values: new object[,]
                {
                    { 1, "12 Marine Drive, Summerstrand, Gqeberha, 6001", "-33.9680,25.6450", true, "Summerstrand Village" },
                    { 2, "45 Beach Road, Humewood, Gqeberha, 6001", "-33.9630,25.6280", true, "Humewood" },
                    { 3, "78 Cape Road, Newton Park, Gqeberha, 6045", "-33.9480,25.5750", true, "Newton Park" },
                    { 4, "23 Heugh Road, Walmer Heights, Gqeberha, 6070", "-33.9850,25.6100", true, "Walmer Heights" },
                    { 5, "91 Circular Drive, Lorraine, Gqeberha, 6070", "-33.9320,25.5620", true, "Lorraine" },
                    { 6, "5 Seaview Crescent, Bluewater Bay, Gqeberha, 6210", "-33.8710,25.5590", true, "Bluewater Bay" },
                    { 7, "14 Langenhoven Drive, Framesby, Gqeberha, 6045", "-33.9260,25.5810", true, "Framesby" },
                    { 8, "30 Ring Road, Greenacres, Gqeberha, 6045", "-33.9410,25.5870", true, "Greenacres" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Sarah", "Nkosi", null, null, "s312847563", "s312847563@mandela.ac.za" },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Liam", "van der Merwe", null, null, "s319284731", "s319284731@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Ayanda", true, "Mthembu", null, null, "s321056839", "s321056839@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Chloe", "Botha", null, null, "s317493208", "s317493208@mandela.ac.za" },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Sipho", "Zulu", null, null, "s329187465", "s329187465@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Jessica", true, "Adams", null, null, "s318274056", "s318274056@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Mandla", "Ndlovu", null, null, "s322941587", "s322941587@mandela.ac.za" },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Regan", "Pieterse", null, null, "s316785234", "s316785234@mandela.ac.za" },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Zanele", "Khumalo", null, null, "s325098471", "s325098471@mandela.ac.za" },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Devon", "Jacobs", null, null, "s311653928", "s311653928@mandela.ac.za" },
                    { 11, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thandi", "Sithole", null, null, "s328417509", "s328417509@mandela.ac.za" },
                    { 12, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Jason", "Williams", null, null, "s314928673", "s314928673@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 13, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Nokwanda", true, "Dube", null, null, "s327651084", "s327651084@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 14, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Michael", "van Niekerk", null, null, "s319073845", "s319073845@mandela.ac.za" },
                    { 15, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Lerato", "Molefe", null, null, "s323846190", "s323846190@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Marshals",
                columns: new[] { "MarshalId", "AssignedCampusId", "AssignedStopId", "CreatedAt", "FirstName", "LastName", "OtpExpiry", "OtpHash", "StaffNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Bongani", "Cele", null, null, "4827391", "b.cele@mandela.ac.za" },
                    { 2, 2, null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Karen", "Swart", null, null, "5914826", "k.swart@mandela.ac.za" },
                    { 3, 3, null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Vuyo", "Mabena", null, null, "3208574", "v.mabena@mandela.ac.za" },
                    { 4, null, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thulani", "Ngcobo", null, null, "6153042", "t.ngcobo@mandela.ac.za" },
                    { 5, null, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Lindiwe", "Shabalala", null, null, "7294185", "l.shabalala@mandela.ac.za" },
                    { 6, null, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Pieter", "Venter", null, null, "8401267", "p.venter@mandela.ac.za" },
                    { 7, null, 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Nombuso", "Mkhize", null, null, "9537148", "n.mkhize@mandela.ac.za" },
                    { 8, null, 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Craig", "Daniels", null, null, "2648309", "c.daniels@mandela.ac.za" },
                    { 9, null, 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Noluthando", "Mthethwa", null, null, "1785423", "n.mthethwa@mandela.ac.za" },
                    { 10, null, 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Willem", "Pretorius", null, null, "4062891", "w.pretorius@mandela.ac.za" },
                    { 11, null, 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Zandile", "Mdluli", null, null, "5379216", "z.mdluli@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "ScheduleTimeId", "CampusId", "DepartureTime", "Direction", "ScheduleType", "StopId" },
                values: new object[,]
                {
                    { 1, 1, new TimeOnly(6, 30, 0), "ToCampus", "Weekday", 1 },
                    { 2, 1, new TimeOnly(6, 30, 0), "ToCampus", "Weekday", 2 },
                    { 3, 1, new TimeOnly(7, 15, 0), "ToCampus", "Weekday", 3 },
                    { 4, 2, new TimeOnly(7, 15, 0), "ToCampus", "Weekday", 4 },
                    { 5, 2, new TimeOnly(7, 45, 0), "ToCampus", "Weekday", 5 },
                    { 6, 3, new TimeOnly(8, 30, 0), "ToCampus", "Weekday", 6 },
                    { 7, 1, new TimeOnly(12, 30, 0), "FromCampus", "Weekday", 1 },
                    { 8, 1, new TimeOnly(12, 30, 0), "FromCampus", "Weekday", 2 },
                    { 9, 1, new TimeOnly(14, 0, 0), "FromCampus", "Weekday", 3 },
                    { 10, 2, new TimeOnly(14, 0, 0), "FromCampus", "Weekday", 4 },
                    { 11, 1, new TimeOnly(16, 30, 0), "FromCampus", "Weekday", 1 },
                    { 12, 1, new TimeOnly(16, 30, 0), "FromCampus", "Weekday", 2 },
                    { 13, 2, new TimeOnly(16, 30, 0), "FromCampus", "Weekday", 5 },
                    { 14, 3, new TimeOnly(17, 15, 0), "FromCampus", "Weekday", 6 },
                    { 15, 1, new TimeOnly(8, 0, 0), "ToCampus", "Saturday", 1 },
                    { 16, 1, new TimeOnly(8, 0, 0), "ToCampus", "Saturday", 2 },
                    { 17, 1, new TimeOnly(13, 0, 0), "FromCampus", "Saturday", 1 },
                    { 18, 1, new TimeOnly(13, 0, 0), "FromCampus", "Saturday", 2 }
                });

            migrationBuilder.InsertData(
                table: "PrioritySchedules",
                columns: new[] { "PriorityScheduleId", "CreatedAt", "DayOfWeek", "IsActive", "ScheduleTimeId", "StopId", "StudentId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 1, 1, 3 },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 1, 1, 3 },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Friday", true, 1, 1, 3 },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 11, 1, 3 },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 11, 1, 3 },
                    { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Friday", true, 11, 1, 3 },
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Tuesday", true, 2, 2, 6 },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thursday", true, 2, 2, 6 },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Tuesday", true, 12, 2, 6 },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thursday", true, 12, 2, 6 },
                    { 11, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 4, 4, 13 },
                    { 12, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 4, 4, 13 },
                    { 13, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 10, 4, 13 },
                    { 14, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 10, 4, 13 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_Email",
                table: "Admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_QrCodeToken",
                table: "Bookings",
                column: "QrCodeToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ShuttleId",
                table: "Bookings",
                column: "ShuttleId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_StudentId",
                table: "Bookings",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TripInstanceId_Status",
                table: "Bookings",
                columns: new[] { "TripInstanceId", "BookingStatus" });

            migrationBuilder.CreateIndex(
                name: "UQ_Bookings_Student_TripInstance_Active",
                table: "Bookings",
                columns: new[] { "StudentId", "TripInstanceId" },
                unique: true,
                filter: "BookingStatus != 'Cancelled'");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_FlaggedAt",
                table: "BreakdownLogs",
                column: "FlaggedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_FlaggedByMarshalId",
                table: "BreakdownLogs",
                column: "FlaggedByMarshalId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_ReplacementShuttleId",
                table: "BreakdownLogs",
                column: "ReplacementShuttleId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_ResolvedByAdminId",
                table: "BreakdownLogs",
                column: "ResolvedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_ShuttleId",
                table: "BreakdownLogs",
                column: "ShuttleId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_TripInstanceId",
                table: "BreakdownLogs",
                column: "TripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakdownLogs_VendorCompanyName",
                table: "BreakdownLogs",
                column: "VendorCompanyName");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarExceptions_TargetDate",
                table: "CalendarExceptions",
                column: "TargetDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campuses_CampusName",
                table: "Campuses",
                column: "CampusName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceRegistrations_OwnerId_Type",
                table: "DeviceRegistrations",
                columns: new[] { "OwnerId", "OwnerType" });

            migrationBuilder.CreateIndex(
                name: "UQ_DeviceRegistrations_Owner_Platform",
                table: "DeviceRegistrations",
                columns: new[] { "OwnerId", "OwnerType", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marshals_AssignedCampusId",
                table: "Marshals",
                column: "AssignedCampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Marshals_AssignedStopId",
                table: "Marshals",
                column: "AssignedStopId");

            migrationBuilder.CreateIndex(
                name: "IX_Marshals_StaffNumber",
                table: "Marshals",
                column: "StaffNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marshals_UniversityEmail",
                table: "Marshals",
                column: "UniversityEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RelatedTripInstanceId",
                table: "Notifications",
                column: "RelatedTripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_StudentId_IsRead",
                table: "Notifications",
                columns: new[] { "StudentId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySchedules_DayOfWeek_Active",
                table: "PrioritySchedules",
                columns: new[] { "DayOfWeek", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySchedules_ScheduleTimeId",
                table: "PrioritySchedules",
                column: "ScheduleTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySchedules_StopId",
                table: "PrioritySchedules",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_PrioritySchedules_StudentId",
                table: "PrioritySchedules",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId",
                table: "Reports",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TripInstanceId",
                table: "Reports",
                column: "TripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimes_CampusId",
                table: "ScheduleTimes",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimes_StopId",
                table: "ScheduleTimes",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_Shuttles_LicensePlate",
                table: "Shuttles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stops_StopName",
                table: "Stops",
                column: "StopName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_UniversityEmail",
                table: "Students",
                column: "UniversityEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripInstances_StopId",
                table: "TripInstances",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_TripInstances_TripDate_Status",
                table: "TripInstances",
                columns: new[] { "TripDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "UQ_TripInstances_ScheduleTime_Stop_Date",
                table: "TripInstances",
                columns: new[] { "ScheduleTimeId", "StopId", "TripDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TripShuttleAllocations_ShuttleId",
                table: "TripShuttleAllocations",
                column: "ShuttleId");

            migrationBuilder.CreateIndex(
                name: "IX_TripShuttleAllocations_TripInstanceId",
                table: "TripShuttleAllocations",
                column: "TripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TripShuttleAllocations_TripInstanceId_OrderPosition",
                table: "TripShuttleAllocations",
                columns: new[] { "TripInstanceId", "OrderPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_TripShuttleAllocations_TripInstance_Shuttle",
                table: "TripShuttleAllocations",
                columns: new[] { "TripInstanceId", "ShuttleId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "BreakdownLogs");

            migrationBuilder.DropTable(
                name: "CalendarExceptions");

            migrationBuilder.DropTable(
                name: "DeviceRegistrations");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PrioritySchedules");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "TripShuttleAllocations");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Marshals");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Shuttles");

            migrationBuilder.DropTable(
                name: "TripInstances");

            migrationBuilder.DropTable(
                name: "ScheduleTimes");

            migrationBuilder.DropTable(
                name: "Campuses");

            migrationBuilder.DropTable(
                name: "Stops");
        }
    }
}
