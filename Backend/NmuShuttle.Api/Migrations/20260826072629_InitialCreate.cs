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
                    StaffNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OtpHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    OtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
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
                name: "Stops",
                columns: table => new
                {
                    StopId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StopName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Street = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Suburb = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OtpHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.StudentId);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                columns: table => new
                {
                    VendorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.VendorId);
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
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marshals", x => x.MarshalId);
                    table.CheckConstraint("CK_Marshals_AssignedXOR", "1=1");
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
                    StopId = table.Column<int>(type: "int", nullable: false),
                    RequiresScanning = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTimes", x => x.ScheduleTimeId);
                    table.CheckConstraint("CK_ScheduleTimes_Direction", "Direction IN ('ToCampus','FromCampus')");
                    table.CheckConstraint("CK_ScheduleTimes_ScheduleType", "ScheduleType IN ('Weekday','Saturday')");
                    table.ForeignKey(
                        name: "FK_ScheduleTimes_Stops_StopId",
                        column: x => x.StopId,
                        principalTable: "Stops",
                        principalColumn: "StopId",
                        onDelete: ReferentialAction.Restrict);
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
                    VendorId = table.Column<int>(type: "int", nullable: false),
                    IsAccessibleVehicle = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shuttles", x => x.ShuttleId);
                    table.CheckConstraint("CK_Shuttles_Capacity", "Capacity > 0");
                    table.ForeignKey(
                        name: "FK_Shuttles_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "VendorId",
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
                name: "ScheduleTimeCampuses",
                columns: table => new
                {
                    ScheduleTimeCampusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ScheduleTimeId = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTimeCampuses", x => x.ScheduleTimeCampusId);
                    table.ForeignKey(
                        name: "FK_ScheduleTimeCampuses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScheduleTimeCampuses_ScheduleTimes_ScheduleTimeId",
                        column: x => x.ScheduleTimeId,
                        principalTable: "ScheduleTimes",
                        principalColumn: "ScheduleTimeId",
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
                name: "StopShuttleDefaults",
                columns: table => new
                {
                    StopShuttleDefaultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    ShuttleId = table.Column<int>(type: "int", nullable: false),
                    OrderPosition = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StopShuttleDefaults", x => x.StopShuttleDefaultId);
                    table.ForeignKey(
                        name: "FK_StopShuttleDefaults_Shuttles_ShuttleId",
                        column: x => x.ShuttleId,
                        principalTable: "Shuttles",
                        principalColumn: "ShuttleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StopShuttleDefaults_Stops_StopId",
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
                name: "TripNotes",
                columns: table => new
                {
                    TripNoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TripInstanceId = table.Column<int>(type: "int", nullable: false),
                    NoteType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedByAdminId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    IsResolutionAction = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripNotes", x => x.TripNoteId);
                    table.CheckConstraint("CK_TripNotes_NoteType", "NoteType IN ('Incident','Replacement','General','NoShow')");
                    table.ForeignKey(
                        name: "FK_TripNotes_Admins_CreatedByAdminId",
                        column: x => x.CreatedByAdminId,
                        principalTable: "Admins",
                        principalColumn: "AdminId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripNotes_TripInstances_TripInstanceId",
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
                    BookingId = table.Column<int>(type: "int", nullable: true),
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
                        name: "FK_Reports_Bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "Bookings",
                        principalColumn: "BookingId");
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

            migrationBuilder.InsertData(
                table: "Admins",
                columns: new[] { "AdminId", "CreatedAt", "Email", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "PasswordHash", "StaffNumber" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "admin@mandela.ac.za", "Thabo", true, "Mokoena", null, null, "$2a$12$UnzkP2CPSCL04uIvla3UKOpZAVQZFEPRngF12SJyE2.gdVhHlZBk6", "7001001" },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "transport@mandela.ac.za", "Nomsa", true, "Dlamini", null, null, "$2a$12$YqZo4jPNiOcMGYhDRJ4CleeMqptACgPleJfWR/n.UJKTol9pZfE0i", "7001002" }
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
                    { 10, "Heritage Day", "PublicHoliday", new DateOnly(2025, 9, 24) },
                    { 11, "Human Rights Day", "PublicHoliday", new DateOnly(2026, 3, 21) },
                    { 12, "Good Friday", "PublicHoliday", new DateOnly(2026, 4, 3) },
                    { 13, "Family Day", "PublicHoliday", new DateOnly(2026, 4, 6) },
                    { 14, "Freedom Day", "PublicHoliday", new DateOnly(2026, 4, 27) },
                    { 15, "Workers Day", "PublicHoliday", new DateOnly(2026, 5, 1) },
                    { 16, "Youth Day", "PublicHoliday", new DateOnly(2026, 6, 16) },
                    { 17, "National Women's Day (observed)", "PublicHoliday", new DateOnly(2026, 8, 10) },
                    { 18, "Heritage Day", "PublicHoliday", new DateOnly(2026, 9, 24) }
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
                table: "DeviceRegistrations",
                columns: new[] { "DeviceRegistrationId", "FcmToken", "OwnerId", "OwnerType", "Platform", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "fcm_token_student_1_a1b2c3d4e5", 1, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "fcm_token_student_2_f6g7h8i9j0", 2, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "fcm_token_student_3_k1l2m3n4o5", 3, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, "fcm_token_student_4_p6q7r8s9t0", 4, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, "fcm_token_student_5_u1v2w3x4y5", 5, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, "fcm_token_student_6_z6a7b8c9d0", 6, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, "fcm_token_student_7_e1f2g3h4i5", 7, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, "fcm_token_student_8_j6k7l8m9n0", 8, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, "fcm_token_student_9_o1p2q3r4s5", 9, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 10, "fcm_token_student_10_t6u7v8w9x0", 10, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 11, "fcm_token_student_13_y1z2a3b4c5", 13, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, "fcm_token_student_16_d6e7f8g9h0", 16, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, "fcm_token_student_20_i1j2k3l4m5", 20, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, "fcm_token_student_25_n6o7p8q9r0", 25, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, "fcm_token_student_30_s1t2u3v4w5", 30, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, "fcm_token_student_35_x6y7z8a9b0", 35, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, "fcm_token_student_40_c1d2e3f4g5", 40, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, "fcm_token_student_45_h6i7j8k9l0", 45, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, "fcm_token_student_50_m1n2o3p4q5", 50, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, "fcm_token_student_55_r6s7t8u9v0", 55, "Student", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 21, "fcm_token_marshal_1_w1x2y3z4a5", 1, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 22, "fcm_token_marshal_2_b6c7d8e9f0", 2, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 23, "fcm_token_marshal_3_g1h2i3j4k5", 3, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 24, "fcm_token_marshal_4_l6m7n8o9p0", 4, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 25, "fcm_token_marshal_5_q1r2s3t4u5", 5, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 26, "fcm_token_marshal_6_v6w7x8y9z0", 6, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 27, "fcm_token_marshal_7_a1b2c3d4e5", 7, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 28, "fcm_token_marshal_8_f6g7h8i9j0", 8, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 29, "fcm_token_marshal_9_k1l2m3n4o5", 9, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 30, "fcm_token_marshal_10_p6q7r8s9t0", 10, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) },
                    { 31, "fcm_token_marshal_11_u1v2w3x4y5", 11, "Marshal", "android", new DateTime(2026, 7, 20, 8, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Stops",
                columns: new[] { "StopId", "City", "GpsCoordinates", "IsActive", "PostalCode", "StopName", "Street", "Suburb" },
                values: new object[,]
                {
                    { 1, "Gqeberha", "-33.9680,25.6450", true, "6001", "Summerstrand Village", "12 Marine Drive", "Summerstrand" },
                    { 2, "Gqeberha", "-33.9630,25.6280", true, "6001", "Humewood", "45 Beach Road", "Humewood" },
                    { 3, "Gqeberha", "-33.9480,25.5750", true, "6045", "Newton Park", "78 Cape Road", "Newton Park" },
                    { 4, "Gqeberha", "-33.9850,25.6100", true, "6070", "Walmer Heights", "23 Heugh Road", "Walmer Heights" },
                    { 5, "Gqeberha", "-33.9320,25.5620", true, "6070", "Lorraine", "91 Circular Drive", "Lorraine" },
                    { 6, "Gqeberha", "-33.8710,25.5590", true, "6210", "Bluewater Bay", "5 Seaview Crescent", "Bluewater Bay" },
                    { 7, "Gqeberha", "-33.9260,25.5810", true, "6045", "Framesby", "14 Langenhoven Drive", "Framesby" },
                    { 8, "Gqeberha", "-33.9410,25.5870", true, "6045", "Greenacres", "30 Ring Road", "Greenacres" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Sarah", true, "Nkosi", null, null, "s312847563", "s312847563@mandela.ac.za" },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Liam", true, "van der Merwe", null, null, "s319284731", "s319284731@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Ayanda", true, true, "Mthembu", null, null, "s321056839", "s321056839@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Chloe", true, "Botha", null, null, "s317493208", "s317493208@mandela.ac.za" },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Sipho", true, "Zulu", null, null, "s329187465", "s329187465@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Jessica", true, true, "Adams", null, null, "s318274056", "s318274056@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Mandla", true, "Ndlovu", null, null, "s322941587", "s322941587@mandela.ac.za" },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Regan", true, "Pieterse", null, null, "s316785234", "s316785234@mandela.ac.za" },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Zanele", true, "Khumalo", null, null, "s325098471", "s325098471@mandela.ac.za" },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Devon", true, "Jacobs", null, null, "s311653928", "s311653928@mandela.ac.za" },
                    { 11, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thandi", true, "Sithole", null, null, "s328417509", "s328417509@mandela.ac.za" },
                    { 12, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Jason", true, "Williams", null, null, "s314928673", "s314928673@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 13, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Nokwanda", true, true, "Dube", null, null, "s327651084", "s327651084@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 14, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Michael", true, "van Niekerk", null, null, "s319073845", "s319073845@mandela.ac.za" },
                    { 15, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Lerato", true, "Molefe", null, null, "s323846190", "s323846190@mandela.ac.za" },
                    { 16, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thabo", true, "Maseko", null, null, "s314527896", "s314527896@mandela.ac.za" },
                    { 17, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Amahle", true, "Nxumalo", null, null, "s328193045", "s328193045@mandela.ac.za" },
                    { 18, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ethan", true, "du Plessis", null, null, "s311847290", "s311847290@mandela.ac.za" },
                    { 19, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Naledi", true, "Tshabalala", null, null, "s325604183", "s325604183@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 20, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kwame", true, true, "Osei", null, null, "s319438267", "s319438267@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 21, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Bianca", true, "Ferreira", null, null, "s322785410", "s322785410@mandela.ac.za" },
                    { 22, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Siyabonga", true, "Mhlongo", null, null, "s316094358", "s316094358@mandela.ac.za" },
                    { 23, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Megan", true, "Swanepoel", null, null, "s329471023", "s329471023@mandela.ac.za" },
                    { 24, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thandolwethu", true, "Gumede", null, null, "s313268947", "s313268947@mandela.ac.za" },
                    { 25, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ryan", true, "Cloete", null, null, "s327815604", "s327815604@mandela.ac.za" },
                    { 26, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nompumelelo", true, "Zwane", null, null, "s311542789", "s311542789@mandela.ac.za" },
                    { 27, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dylan", true, "Viljoen", null, null, "s324896031", "s324896031@mandela.ac.za" },
                    { 28, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Palesa", true, "Mokoena", null, null, "s318623475", "s318623475@mandela.ac.za" },
                    { 29, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Luca", true, "Naude", null, null, "s326057184", "s326057184@mandela.ac.za" },
                    { 30, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Zinhle", true, "Buthelezi", null, null, "s312384956", "s312384956@mandela.ac.za" },
                    { 31, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Joshua", true, "Erasmus", null, null, "s329710438", "s329710438@mandela.ac.za" },
                    { 32, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Busisiwe", true, "Mabaso", null, null, "s315847291", "s315847291@mandela.ac.za" },
                    { 33, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Luke", true, "Barnard", null, null, "s323196804", "s323196804@mandela.ac.za" },
                    { 34, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nonhlanhla", true, "Nkabinde", null, null, "s317429058", "s317429058@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "IsPriorityUser", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[] { 35, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thato", true, true, "Letsoko", null, null, "s321863742", "s321863742@mandela.ac.za" });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 36, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Hannah", true, "Muller", null, null, "s314085267", "s314085267@mandela.ac.za" },
                    { 37, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lungelo", true, "Shabangu", null, null, "s328574190", "s328574190@mandela.ac.za" },
                    { 38, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Amber", true, "Joubert", null, null, "s316238904", "s316238904@mandela.ac.za" },
                    { 39, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sbusiso", true, "Radebe", null, null, "s322491637", "s322491637@mandela.ac.za" },
                    { 40, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tamsin", true, "Olivier", null, null, "s319764082", "s319764082@mandela.ac.za" },
                    { 41, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Buhle", true, "Mahlangu", null, null, "s325138476", "s325138476@mandela.ac.za" },
                    { 42, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Connor", true, "le Roux", null, null, "s313692815", "s313692815@mandela.ac.za" },
                    { 43, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sinethemba", true, "Majola", null, null, "s327045963", "s327045963@mandela.ac.za" },
                    { 44, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kayla", true, "Steyn", null, null, "s311928347", "s311928347@mandela.ac.za" },
                    { 45, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Andile", true, "Dladla", null, null, "s324581076", "s324581076@mandela.ac.za" },
                    { 46, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Emma", true, "van Wyk", null, null, "s318305492", "s318305492@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 47, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Themba", "Vilakazi", null, null, "s326742815", "s326742815@mandela.ac.za" },
                    { 48, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Danielle", "Potgieter", null, null, "s312059638", "s312059638@mandela.ac.za" },
                    { 49, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mthunzi", "Cele", null, null, "s329384071", "s329384071@mandela.ac.za" },
                    { 50, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Robyn", "Prinsloo", null, null, "s315670284", "s315670284@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Students",
                columns: new[] { "StudentId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StudentNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 51, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Anathi", true, "Magwaza", null, null, "s323947158", "s323947158@mandela.ac.za" },
                    { 52, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jaden", true, "Booysen", null, null, "s317283046", "s317283046@mandela.ac.za" },
                    { 53, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Lindiwe", true, "Ngubane", null, null, "s325816490", "s325816490@mandela.ac.za" },
                    { 54, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Aiden", true, "Marais", null, null, "s311475823", "s311475823@mandela.ac.za" },
                    { 55, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nandi", true, "Mthimkhulu", null, null, "s328640175", "s328640175@mandela.ac.za" },
                    { 56, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Caleb", true, "Bezuidenhout", null, null, "s314903268", "s314903268@mandela.ac.za" },
                    { 57, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Yonela", true, "Sifuba", null, null, "s322167549", "s322167549@mandela.ac.za" },
                    { 58, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Mia", true, "Hendriks", null, null, "s316538291", "s316538291@mandela.ac.za" },
                    { 59, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Khaya", true, "Mkhwanazi", null, null, "s329081467", "s329081467@mandela.ac.za" },
                    { 60, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Jade", true, "van Rensburg", null, null, "s313724850", "s313724850@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "Vendors",
                columns: new[] { "VendorId", "CompanyName", "ContactEmail", "ContactPhone", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "Bay Transit Corp", "info@baytransit.co.za", "041-555-0101", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, "Coastal Movers", "info@coastalmovers.co.za", "041-555-0202", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, "Metro Link SA", "info@metrolinksa.co.za", "041-555-0303", new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "Marshals",
                columns: new[] { "MarshalId", "AssignedCampusId", "AssignedStopId", "CreatedAt", "FirstName", "IsActive", "LastName", "OtpExpiry", "OtpHash", "StaffNumber", "UniversityEmail" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Bongani", true, "Cele", null, null, "4827391", "b.cele@mandela.ac.za" },
                    { 2, 2, null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Karen", true, "Swart", null, null, "5914826", "k.swart@mandela.ac.za" },
                    { 3, 3, null, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Vuyo", true, "Mabena", null, null, "3208574", "v.mabena@mandela.ac.za" },
                    { 4, null, 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thulani", true, "Ngcobo", null, null, "6153042", "t.ngcobo@mandela.ac.za" },
                    { 5, null, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Lindiwe", true, "Shabalala", null, null, "7294185", "l.shabalala@mandela.ac.za" },
                    { 6, null, 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Pieter", true, "Venter", null, null, "8401267", "p.venter@mandela.ac.za" },
                    { 7, null, 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Nombuso", true, "Mkhize", null, null, "9537148", "n.mkhize@mandela.ac.za" },
                    { 8, null, 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Craig", true, "Daniels", null, null, "2648309", "c.daniels@mandela.ac.za" },
                    { 9, null, 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Noluthando", true, "Mthethwa", null, null, "1785423", "n.mthethwa@mandela.ac.za" },
                    { 10, null, 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Willem", true, "Pretorius", null, null, "4062891", "w.pretorius@mandela.ac.za" },
                    { 11, null, 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Zandile", true, "Mdluli", null, null, "5379216", "z.mdluli@mandela.ac.za" }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimes",
                columns: new[] { "ScheduleTimeId", "DepartureTime", "Direction", "RequiresScanning", "ScheduleType", "StopId" },
                values: new object[,]
                {
                    { 1, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 1 },
                    { 2, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 1 },
                    { 3, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 1 },
                    { 4, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 1 },
                    { 5, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 1 },
                    { 6, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 1 },
                    { 7, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 1 },
                    { 8, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 1 },
                    { 9, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 1 },
                    { 10, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 1 },
                    { 11, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 1 },
                    { 12, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 1 },
                    { 13, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 1 },
                    { 14, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 1 },
                    { 15, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 1 },
                    { 16, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 1 },
                    { 17, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 1 },
                    { 18, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 1 },
                    { 19, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 1 },
                    { 20, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 2 },
                    { 21, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 2 },
                    { 22, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 2 },
                    { 23, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 2 },
                    { 24, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 2 },
                    { 25, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 2 },
                    { 26, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 2 },
                    { 27, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 2 },
                    { 28, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 2 },
                    { 29, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 2 },
                    { 30, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 2 },
                    { 31, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 2 },
                    { 32, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 2 },
                    { 33, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 2 },
                    { 34, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 2 },
                    { 35, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 2 },
                    { 36, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 2 },
                    { 37, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 2 },
                    { 38, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 2 },
                    { 39, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 3 },
                    { 40, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 3 },
                    { 41, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 3 },
                    { 42, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 3 },
                    { 43, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 3 },
                    { 44, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 3 },
                    { 45, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 3 },
                    { 46, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 3 },
                    { 47, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 3 },
                    { 48, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 3 },
                    { 49, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 3 },
                    { 50, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 3 },
                    { 51, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 3 },
                    { 52, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 3 },
                    { 53, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 3 },
                    { 54, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 3 },
                    { 55, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 3 },
                    { 56, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 3 },
                    { 57, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 3 },
                    { 58, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 4 },
                    { 59, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 4 },
                    { 60, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 4 },
                    { 61, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 4 },
                    { 62, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 4 },
                    { 63, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 4 },
                    { 64, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 4 },
                    { 65, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 4 },
                    { 66, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 4 },
                    { 67, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 4 },
                    { 68, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 4 },
                    { 69, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 4 },
                    { 70, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 4 },
                    { 71, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 4 },
                    { 72, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 4 },
                    { 73, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 4 },
                    { 74, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 4 },
                    { 75, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 4 },
                    { 76, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 4 },
                    { 77, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 5 },
                    { 78, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 5 },
                    { 79, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 5 },
                    { 80, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 5 },
                    { 81, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 5 },
                    { 82, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 5 },
                    { 83, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 5 },
                    { 84, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 5 },
                    { 85, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 5 },
                    { 86, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 5 },
                    { 87, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 5 },
                    { 88, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 5 },
                    { 89, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 5 },
                    { 90, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 5 },
                    { 91, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 5 },
                    { 92, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 5 },
                    { 93, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 5 },
                    { 94, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 5 },
                    { 95, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 5 },
                    { 96, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 6 },
                    { 97, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 6 },
                    { 98, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 6 },
                    { 99, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 6 },
                    { 100, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 6 },
                    { 101, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 6 },
                    { 102, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 6 },
                    { 103, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 6 },
                    { 104, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 6 },
                    { 105, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 6 },
                    { 106, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 6 },
                    { 107, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 6 },
                    { 108, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 6 },
                    { 109, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 6 },
                    { 110, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 6 },
                    { 111, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 6 },
                    { 112, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 6 },
                    { 113, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 6 },
                    { 114, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 6 },
                    { 115, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 7 },
                    { 116, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 7 },
                    { 117, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 7 },
                    { 118, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 7 },
                    { 119, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 7 },
                    { 120, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 7 },
                    { 121, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 7 },
                    { 122, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 7 },
                    { 123, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 7 },
                    { 124, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 7 },
                    { 125, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 7 },
                    { 126, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 7 },
                    { 127, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 7 },
                    { 128, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 7 },
                    { 129, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 7 },
                    { 130, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 7 },
                    { 131, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 7 },
                    { 132, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 7 },
                    { 133, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 7 },
                    { 134, new TimeOnly(6, 45, 0), "ToCampus", true, "Weekday", 8 },
                    { 135, new TimeOnly(7, 45, 0), "ToCampus", true, "Weekday", 8 },
                    { 136, new TimeOnly(8, 45, 0), "ToCampus", true, "Weekday", 8 },
                    { 137, new TimeOnly(9, 45, 0), "ToCampus", true, "Weekday", 8 },
                    { 138, new TimeOnly(12, 30, 0), "FromCampus", true, "Weekday", 8 },
                    { 139, new TimeOnly(14, 30, 0), "FromCampus", true, "Weekday", 8 },
                    { 140, new TimeOnly(16, 0, 0), "FromCampus", true, "Weekday", 8 },
                    { 141, new TimeOnly(17, 30, 0), "FromCampus", true, "Weekday", 8 },
                    { 142, new TimeOnly(18, 30, 0), "FromCampus", true, "Weekday", 8 },
                    { 143, new TimeOnly(20, 30, 0), "FromCampus", true, "Weekday", 8 },
                    { 144, new TimeOnly(22, 0, 0), "FromCampus", true, "Weekday", 8 },
                    { 145, new TimeOnly(6, 45, 0), "ToCampus", true, "Saturday", 8 },
                    { 146, new TimeOnly(7, 45, 0), "ToCampus", true, "Saturday", 8 },
                    { 147, new TimeOnly(8, 45, 0), "ToCampus", true, "Saturday", 8 },
                    { 148, new TimeOnly(9, 45, 0), "ToCampus", true, "Saturday", 8 },
                    { 149, new TimeOnly(12, 30, 0), "FromCampus", true, "Saturday", 8 },
                    { 150, new TimeOnly(14, 30, 0), "FromCampus", true, "Saturday", 8 },
                    { 151, new TimeOnly(16, 0, 0), "FromCampus", true, "Saturday", 8 },
                    { 152, new TimeOnly(17, 30, 0), "FromCampus", true, "Saturday", 8 }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 1, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "EC 142-837", "Bay Transit Corp", 1 });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[,]
                {
                    { 2, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 256-491", "Bay Transit Corp", 1 },
                    { 3, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 389-720", "Bay Transit Corp", 1 }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 4, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "EC 501-163", "Coastal Movers", 2 });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[,]
                {
                    { 5, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 628-945", "Coastal Movers", 2 },
                    { 6, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 714-382", "Coastal Movers", 2 },
                    { 7, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 837-256", "Metro Link SA", 3 },
                    { 8, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 948-617", "Metro Link SA", 3 }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 9, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "EC 103-475", "Metro Link SA", 3 });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[,]
                {
                    { 10, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 217-864", "Bay Transit Corp", 1 },
                    { 11, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 331-058", "Bay Transit Corp", 1 },
                    { 12, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 445-912", "Coastal Movers", 2 },
                    { 13, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 552-376", "Coastal Movers", 2 },
                    { 14, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 668-741", "Coastal Movers", 2 },
                    { 15, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 774-285", "Metro Link SA", 3 },
                    { 16, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 889-639", "Metro Link SA", 3 },
                    { 17, 35, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 993-104", "Metro Link SA", 3 },
                    { 18, 22, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, "EC 107-528", "Bay Transit Corp", 1 }
                });

            migrationBuilder.InsertData(
                table: "Shuttles",
                columns: new[] { "ShuttleId", "Capacity", "CreatedAt", "IsAccessibleVehicle", "IsActive", "LicensePlate", "VendorCompanyName", "VendorId" },
                values: new object[] { 19, 28, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "EC 214-963", "Bay Transit Corp", 1 });

            migrationBuilder.InsertData(
                table: "PrioritySchedules",
                columns: new[] { "PriorityScheduleId", "CreatedAt", "DayOfWeek", "IsActive", "ScheduleTimeId", "StopId", "StudentId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 1, 1, 3 },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 1, 1, 3 },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Friday", true, 1, 1, 3 },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 7, 1, 3 },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 7, 1, 3 },
                    { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Friday", true, 7, 1, 3 },
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Tuesday", true, 20, 2, 6 },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thursday", true, 20, 2, 6 },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Tuesday", true, 26, 2, 6 },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Thursday", true, 26, 2, 6 },
                    { 11, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 58, 4, 13 },
                    { 12, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 58, 4, 13 },
                    { 13, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 63, 4, 13 },
                    { 14, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 63, 4, 13 },
                    { 15, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 39, 3, 20 },
                    { 16, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tuesday", true, 39, 3, 20 },
                    { 17, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 39, 3, 20 },
                    { 18, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thursday", true, 39, 3, 20 },
                    { 19, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 45, 3, 20 },
                    { 20, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tuesday", true, 45, 3, 20 },
                    { 21, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 45, 3, 20 },
                    { 22, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Thursday", true, 45, 3, 20 },
                    { 23, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 77, 5, 35 },
                    { 24, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 77, 5, 35 },
                    { 25, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Friday", true, 77, 5, 35 },
                    { 26, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Monday", true, 83, 5, 35 },
                    { 27, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Wednesday", true, 83, 5, 35 },
                    { 28, new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Friday", true, 83, 5, 35 }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTimeCampuses",
                columns: new[] { "ScheduleTimeCampusId", "CampusId", "ScheduleTimeId", "StopOrder" },
                values: new object[,]
                {
                    { 1, 1, 1, 1 },
                    { 2, 1, 2, 1 },
                    { 3, 1, 3, 1 },
                    { 4, 1, 4, 1 },
                    { 5, 1, 5, 1 },
                    { 6, 1, 6, 1 },
                    { 7, 1, 7, 1 },
                    { 8, 1, 8, 1 },
                    { 9, 1, 9, 1 },
                    { 10, 1, 10, 1 },
                    { 11, 1, 11, 1 },
                    { 12, 1, 12, 1 },
                    { 13, 1, 13, 1 },
                    { 14, 1, 14, 1 },
                    { 15, 1, 15, 1 },
                    { 16, 1, 16, 1 },
                    { 17, 1, 17, 1 },
                    { 18, 1, 18, 1 },
                    { 19, 1, 19, 1 },
                    { 20, 1, 20, 1 },
                    { 21, 1, 21, 1 },
                    { 22, 1, 22, 1 },
                    { 23, 1, 23, 1 },
                    { 24, 1, 24, 1 },
                    { 25, 1, 25, 1 },
                    { 26, 1, 26, 1 },
                    { 27, 1, 27, 1 },
                    { 28, 1, 28, 1 },
                    { 29, 1, 29, 1 },
                    { 30, 1, 30, 1 },
                    { 31, 1, 31, 1 },
                    { 32, 1, 32, 1 },
                    { 33, 1, 33, 1 },
                    { 34, 1, 34, 1 },
                    { 35, 1, 35, 1 },
                    { 36, 1, 36, 1 },
                    { 37, 1, 37, 1 },
                    { 38, 1, 38, 1 },
                    { 39, 1, 39, 1 },
                    { 40, 1, 40, 1 },
                    { 41, 1, 41, 1 },
                    { 42, 1, 42, 1 },
                    { 43, 1, 43, 1 },
                    { 44, 1, 44, 1 },
                    { 45, 1, 45, 1 },
                    { 46, 1, 46, 1 },
                    { 47, 1, 47, 1 },
                    { 48, 1, 48, 1 },
                    { 49, 1, 49, 1 },
                    { 50, 1, 50, 1 },
                    { 51, 1, 51, 1 },
                    { 52, 1, 52, 1 },
                    { 53, 1, 53, 1 },
                    { 54, 1, 54, 1 },
                    { 55, 1, 55, 1 },
                    { 56, 1, 56, 1 },
                    { 57, 1, 57, 1 },
                    { 58, 1, 58, 1 },
                    { 59, 1, 59, 1 },
                    { 60, 1, 60, 1 },
                    { 61, 1, 61, 1 },
                    { 62, 1, 62, 1 },
                    { 63, 1, 63, 1 },
                    { 64, 1, 64, 1 },
                    { 65, 1, 65, 1 },
                    { 66, 1, 66, 1 },
                    { 67, 1, 67, 1 },
                    { 68, 1, 68, 1 },
                    { 69, 1, 69, 1 },
                    { 70, 1, 70, 1 },
                    { 71, 1, 71, 1 },
                    { 72, 1, 72, 1 },
                    { 73, 1, 73, 1 },
                    { 74, 1, 74, 1 },
                    { 75, 1, 75, 1 },
                    { 76, 1, 76, 1 },
                    { 77, 1, 77, 1 },
                    { 78, 1, 78, 1 },
                    { 79, 1, 79, 1 },
                    { 80, 1, 80, 1 },
                    { 81, 1, 81, 1 },
                    { 82, 1, 82, 1 },
                    { 83, 1, 83, 1 },
                    { 84, 1, 84, 1 },
                    { 85, 1, 85, 1 },
                    { 86, 1, 86, 1 },
                    { 87, 1, 87, 1 },
                    { 88, 1, 88, 1 },
                    { 89, 1, 89, 1 },
                    { 90, 1, 90, 1 },
                    { 91, 1, 91, 1 },
                    { 92, 1, 92, 1 },
                    { 93, 1, 93, 1 },
                    { 94, 1, 94, 1 },
                    { 95, 1, 95, 1 },
                    { 96, 1, 96, 1 },
                    { 97, 1, 97, 1 },
                    { 98, 1, 98, 1 },
                    { 99, 1, 99, 1 },
                    { 100, 1, 100, 1 },
                    { 101, 1, 101, 1 },
                    { 102, 1, 102, 1 },
                    { 103, 1, 103, 1 },
                    { 104, 1, 104, 1 },
                    { 105, 1, 105, 1 },
                    { 106, 1, 106, 1 },
                    { 107, 1, 107, 1 },
                    { 108, 1, 108, 1 },
                    { 109, 1, 109, 1 },
                    { 110, 1, 110, 1 },
                    { 111, 1, 111, 1 },
                    { 112, 1, 112, 1 },
                    { 113, 1, 113, 1 },
                    { 114, 1, 114, 1 },
                    { 115, 1, 115, 1 },
                    { 116, 1, 116, 1 },
                    { 117, 1, 117, 1 },
                    { 118, 1, 118, 1 },
                    { 119, 1, 119, 1 },
                    { 120, 1, 120, 1 },
                    { 121, 1, 121, 1 },
                    { 122, 1, 122, 1 },
                    { 123, 1, 123, 1 },
                    { 124, 1, 124, 1 },
                    { 125, 1, 125, 1 },
                    { 126, 1, 126, 1 },
                    { 127, 1, 127, 1 },
                    { 128, 1, 128, 1 },
                    { 129, 1, 129, 1 },
                    { 130, 1, 130, 1 },
                    { 131, 1, 131, 1 },
                    { 132, 1, 132, 1 },
                    { 133, 1, 133, 1 },
                    { 134, 1, 134, 1 },
                    { 135, 1, 135, 1 },
                    { 136, 1, 136, 1 },
                    { 137, 1, 137, 1 },
                    { 138, 1, 138, 1 },
                    { 139, 1, 139, 1 },
                    { 140, 1, 140, 1 },
                    { 141, 1, 141, 1 },
                    { 142, 1, 142, 1 },
                    { 143, 1, 143, 1 },
                    { 144, 1, 144, 1 },
                    { 145, 1, 145, 1 },
                    { 146, 1, 146, 1 },
                    { 147, 1, 147, 1 },
                    { 148, 1, 148, 1 },
                    { 149, 1, 149, 1 },
                    { 150, 1, 150, 1 },
                    { 151, 1, 151, 1 },
                    { 152, 1, 152, 1 }
                });

            migrationBuilder.InsertData(
                table: "StopShuttleDefaults",
                columns: new[] { "StopShuttleDefaultId", "CreatedAt", "OrderPosition", "ShuttleId", "StopId" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1, 1 },
                    { 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 2, 1 },
                    { 3, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 3, 1 },
                    { 4, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 4, 2 },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 5, 2 },
                    { 6, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 6, 2 },
                    { 7, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 9, 3 },
                    { 8, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 7, 3 },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 3, 8, 3 },
                    { 10, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 19, 4 },
                    { 11, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 10, 4 },
                    { 12, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 11, 5 },
                    { 13, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 12, 5 },
                    { 14, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 13, 6 },
                    { 15, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 14, 6 },
                    { 16, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 15, 7 },
                    { 17, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 16, 7 },
                    { 18, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 1, 17, 8 },
                    { 19, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Utc), 2, 18, 8 }
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
                name: "IX_Reports_BookingId",
                table: "Reports",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_StudentId",
                table: "Reports",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_TripInstanceId",
                table: "Reports",
                column: "TripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimeCampuses_CampusId",
                table: "ScheduleTimeCampuses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimeCampuses_ScheduleTimeId",
                table: "ScheduleTimeCampuses",
                column: "ScheduleTimeId");

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleTimeCampuses_ScheduleTime_Campus",
                table: "ScheduleTimeCampuses",
                columns: new[] { "ScheduleTimeId", "CampusId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_ScheduleTimeCampuses_ScheduleTime_StopOrder",
                table: "ScheduleTimeCampuses",
                columns: new[] { "ScheduleTimeId", "StopOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleTimes_StopId",
                table: "ScheduleTimes",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_Shuttles_IsActive",
                table: "Shuttles",
                column: "IsActive",
                filter: "IsActive = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Shuttles_LicensePlate",
                table: "Shuttles",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shuttles_VendorId",
                table: "Shuttles",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Stops_StopName",
                table: "Stops",
                column: "StopName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StopShuttleDefaults_ShuttleId",
                table: "StopShuttleDefaults",
                column: "ShuttleId");

            migrationBuilder.CreateIndex(
                name: "IX_StopShuttleDefaults_StopId",
                table: "StopShuttleDefaults",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "UQ_StopShuttleDefaults_Stop_OrderPosition",
                table: "StopShuttleDefaults",
                columns: new[] { "StopId", "OrderPosition" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_StopShuttleDefaults_Stop_Shuttle",
                table: "StopShuttleDefaults",
                columns: new[] { "StopId", "ShuttleId" },
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
                name: "IX_TripNotes_CreatedByAdminId",
                table: "TripNotes",
                column: "CreatedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_TripNotes_TripInstanceId",
                table: "TripNotes",
                column: "TripInstanceId");

            migrationBuilder.CreateIndex(
                name: "IX_TripNotes_TripInstanceId_NoteType",
                table: "TripNotes",
                columns: new[] { "TripInstanceId", "NoteType" });

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

            migrationBuilder.CreateIndex(
                name: "UQ_Vendors_CompanyName",
                table: "Vendors",
                column: "CompanyName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "ScheduleTimeCampuses");

            migrationBuilder.DropTable(
                name: "StopShuttleDefaults");

            migrationBuilder.DropTable(
                name: "TripNotes");

            migrationBuilder.DropTable(
                name: "TripShuttleAllocations");

            migrationBuilder.DropTable(
                name: "Marshals");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Admins");

            migrationBuilder.DropTable(
                name: "Campuses");

            migrationBuilder.DropTable(
                name: "Shuttles");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "TripInstances");

            migrationBuilder.DropTable(
                name: "Vendors");

            migrationBuilder.DropTable(
                name: "ScheduleTimes");

            migrationBuilder.DropTable(
                name: "Stops");
        }
    }
}
