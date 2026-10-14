using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Data;

/// <summary>
/// Programmatic seeder for historical runtime data as defined in seed.md.
/// Generates TripInstances, TripShuttleAllocations, Bookings, BreakdownLogs,
/// TripNotes, Notifications, and Reports.
/// 
/// Only runs in Development if the TripInstances table is empty.
/// Uses a fixed Random seed (42) for reproducible output.
/// </summary>
public class HistoricalDataSeeder
{
    private readonly ShuttleDbContext _context;
    private readonly Random _rng = new(42);

    // Operational days from seed.md (Jul 28 – Aug 12, 2026)
    private static readonly (DateOnly Date, bool IsSaturday)[] OperationalDays =
    {
        (new DateOnly(2026, 7, 28), false),  // Day 1 Tue
        (new DateOnly(2026, 7, 29), false),  // Day 2 Wed
        (new DateOnly(2026, 7, 30), false),  // Day 3 Thu
        (new DateOnly(2026, 7, 31), false),  // Day 4 Fri
        (new DateOnly(2026, 8, 1), true),    // Day 5 Sat
        (new DateOnly(2026, 8, 3), false),   // Day 6 Mon
        (new DateOnly(2026, 8, 4), false),   // Day 7 Tue
        (new DateOnly(2026, 8, 5), false),   // Day 8 Wed
        (new DateOnly(2026, 8, 6), false),   // Day 9 Thu
        (new DateOnly(2026, 8, 7), false),   // Day 10 Fri
        (new DateOnly(2026, 8, 8), true),    // Day 11 Sat
        (new DateOnly(2026, 8, 11), false),  // Day 12 Tue
        (new DateOnly(2026, 8, 12), false),  // Day 13 Wed
    };

    // Weekday timeslots (DepartureTime SAST, offset within stop's ScheduleTimeId range: 0-based)
    private static readonly TimeOnly[] WeekdayTimes =
    {
        new(6, 45), new(7, 45), new(8, 45), new(9, 45),        // ToCampus 0-3
        new(12, 30), new(14, 30), new(16, 0), new(17, 30),     // FromCampus 4-7
        new(18, 30), new(20, 30), new(22, 0)                    // FromCampus 8-10
    };

    // Saturday timeslots (offset 11-18 within stop's range, but we index 0-based here)
    private static readonly TimeOnly[] SaturdayTimes =
    {
        new(6, 45), new(7, 45), new(8, 45), new(9, 45),        // ToCampus 0-3
        new(12, 30), new(14, 30), new(16, 0), new(17, 30)      // FromCampus 4-7
    };

    // Load percentages by timeslot index (weekday). Index matches WeekdayTimes.
    private static readonly double[] WeekdayLoadPercent =
        { 0.90, 0.75, 0.55, 0.35, 0.45, 0.60, 0.85, 0.75, 0.50, 0.30, 0.17 };

    // Stop convoy configuration: StopId -> (ShuttleIds in order, TotalCapacity)
    private static readonly Dictionary<int, (int[] ShuttleIds, int Capacity)> ConvoyConfig = new()
    {
        { 1, (new[] { 1, 2, 3 }, 79) },
        { 2, (new[] { 4, 5, 6 }, 85) },
        { 3, (new[] { 9, 7, 8 }, 85) },
        { 4, (new[] { 19, 10 }, 56) },
        { 5, (new[] { 11, 12 }, 57) },
        { 6, (new[] { 13, 14 }, 63) },
        { 7, (new[] { 15, 16 }, 50) },
        { 8, (new[] { 17, 18 }, 57) },
    };

    // Shuttle capacities
    private static readonly Dictionary<int, int> ShuttleCapacity = new()
    {
        {1,22},{2,35},{3,22},{4,28},{5,35},{6,22},{7,28},{8,35},{9,22},
        {10,28},{11,35},{12,22},{13,28},{14,35},{15,22},{16,28},{17,35},{18,22},{19,28}
    };

    // Student-to-Stop affinity (active students only, StudentIds 47-50 excluded)
    private static readonly Dictionary<int, int[]> StopStudents = new()
    {
        { 1, new[] { 1, 2, 3, 16, 17, 18, 51, 52 } },
        { 2, new[] { 4, 5, 6, 19, 20, 21, 53, 54 } },
        { 3, new[] { 7, 8, 9, 22, 23, 24, 55, 56 } },
        { 4, new[] { 10, 11, 13, 25, 26, 27, 57 } },
        { 5, new[] { 12, 14, 35, 28, 29, 30, 58 } },
        { 6, new[] { 15, 31, 32, 33, 59 } },
        { 7, new[] { 34, 36, 37, 38, 60 } },
        { 8, new[] { 39, 40, 41, 42, 43, 44, 45, 46 } },
    };

    // Priority schedules: (StudentId, StopId, ScheduleTimeId, DaysOfWeek[])
    private static readonly (int StudentId, int StopId, int ScheduleTimeId, DayOfWeek[] Days)[] PriorityBookingRules =
    {
        (3, 1, 1, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
        (3, 1, 7, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
        (6, 2, 20, new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }),
        (6, 2, 26, new[] { DayOfWeek.Tuesday, DayOfWeek.Thursday }),
        (13, 4, 58, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }),
        (13, 4, 63, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday }),
        (20, 3, 39, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }),
        (20, 3, 45, new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday }),
        (35, 5, 77, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
        (35, 5, 83, new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday }),
    };

    // Scenario trip IDs
    private const int BreakdownReplacedTripId = 505;
    private const int WaitlistOverflowTripId = 593;
    private const int BreakdownCancelledTripId = 769;

    public HistoricalDataSeeder(ShuttleDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.TripInstances.AnyAsync())
            return; // Already seeded

        var trips = GenerateTrips();
        var allocations = GenerateAllocations(trips);
        var bookings = GenerateBookings(trips);
        var (breakdownLogs, tripNotes) = GenerateBreakdownsAndNotes();
        var notifications = GenerateNotifications(trips, bookings);
        var reports = GenerateReports();

        // Insert each entity set with IDENTITY_INSERT toggled.
        // No wrapping transaction — execution strategy doesn't support user transactions.
        // Order matters for FK constraints.
        await InsertWithIdentity("TripInstances", trips);
        await InsertWithIdentity("TripShuttleAllocations", allocations);
        await InsertWithIdentity("Bookings", bookings);
        await InsertWithIdentity("BreakdownLogs", breakdownLogs);
        await InsertWithIdentity("TripNotes", tripNotes);
        await InsertWithIdentity("Notifications", notifications);
        await InsertWithIdentity("Reports", reports);
    }

    private async Task InsertWithIdentity<T>(string tableName, List<T> entities) where T : class
    {
        if (entities.Count == 0) return;

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

#pragma warning disable EF1002 // Table names are hardcoded constants, not user input
            await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] ON");
#pragma warning restore EF1002

            // Batch inserts to avoid timeout on large sets
            const int batchSize = 5000;
            for (var i = 0; i < entities.Count; i += batchSize)
            {
                var batch = entities.Skip(i).Take(batchSize).ToList();
                _context.Set<T>().AddRange(batch);
                await _context.SaveChangesAsync();

                // Detach to free memory and avoid duplicate tracking
                foreach (var entry in _context.ChangeTracker.Entries().ToList())
                    entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

#pragma warning disable EF1002
            await _context.Database.ExecuteSqlRawAsync($"SET IDENTITY_INSERT [{tableName}] OFF");
#pragma warning restore EF1002

            await transaction.CommitAsync();
        });
    }

    #region Trip Generation

    private List<TripInstance> GenerateTrips()
    {
        var trips = new List<TripInstance>(1096);
        var tripId = 1;

        foreach (var (date, isSaturday) in OperationalDays)
        {
            var timeslotCount = isSaturday ? 8 : 11;

            for (var stopId = 1; stopId <= 8; stopId++)
            {
                for (var slotIdx = 0; slotIdx < timeslotCount; slotIdx++)
                {
                    var scheduleTimeId = GetScheduleTimeId(stopId, slotIdx, isSaturday);
                    var departureTime = isSaturday ? SaturdayTimes[slotIdx] : WeekdayTimes[slotIdx];
                    var (lockedAt, inTransitAt, completedAt) = ComputeTimestamps(date, departureTime);

                    trips.Add(new TripInstance
                    {
                        TripInstanceId = tripId,
                        ScheduleTimeId = scheduleTimeId,
                        StopId = stopId,
                        TripDate = date,
                        Status = "Completed",
                        LockedAt = lockedAt,
                        InTransitAt = inTransitAt,
                        CompletedAt = completedAt
                    });

                    tripId++;
                }
            }
        }

        return trips;
    }

    private static int GetScheduleTimeId(int stopId, int slotIndex, bool isSaturday)
    {
        // Each stop has 19 ScheduleTimeIds: offsets 0-10 weekday, 11-18 saturday
        var baseId = (stopId - 1) * 19 + 1;
        return isSaturday ? baseId + 11 + slotIndex : baseId + slotIndex;
    }

    private static (DateTime LockedAt, DateTime InTransitAt, DateTime CompletedAt) ComputeTimestamps(
        DateOnly date, TimeOnly departureTimeSast)
    {
        // Convert SAST to UTC: subtract 2 hours
        var departureUtc = date.ToDateTime(departureTimeSast, DateTimeKind.Utc).AddHours(-2);
        var lockedAt = departureUtc.AddHours(-1); // T-60min
        var completedAt = departureUtc.AddMinutes(45);

        return (lockedAt, departureUtc, completedAt);
    }

    #endregion

    #region Allocation Generation

    private List<TripShuttleAllocation> GenerateAllocations(List<TripInstance> trips)
    {
        var allocations = new List<TripShuttleAllocation>(2700);
        var allocId = 1;

        foreach (var trip in trips)
        {
            var convoy = ConvoyConfig[trip.StopId];

            for (var i = 0; i < convoy.ShuttleIds.Length; i++)
            {
                var isOperational = true;

                // Breakdown scenarios
                if (trip.TripInstanceId == BreakdownReplacedTripId && convoy.ShuttleIds[i] == 2)
                    isOperational = false;
                if (trip.TripInstanceId == BreakdownCancelledTripId && convoy.ShuttleIds[i] == 3)
                    isOperational = false;

                allocations.Add(new TripShuttleAllocation
                {
                    AllocationId = allocId++,
                    TripInstanceId = trip.TripInstanceId,
                    ShuttleId = convoy.ShuttleIds[i],
                    OrderPosition = i + 1,
                    IsOperational = isOperational,
                    AssignedAt = trip.LockedAt!.Value
                });
            }

            // Trip 505: add replacement shuttle 11 at position 4
            if (trip.TripInstanceId == BreakdownReplacedTripId)
            {
                allocations.Add(new TripShuttleAllocation
                {
                    AllocationId = allocId++,
                    TripInstanceId = trip.TripInstanceId,
                    ShuttleId = 11,
                    OrderPosition = 4,
                    IsOperational = true,
                    AssignedAt = trip.LockedAt!.Value.AddMinutes(65) // ~20min after flag
                });
            }
        }

        return allocations;
    }

    #endregion

    #region Booking Generation

    private List<Booking> GenerateBookings(List<TripInstance> trips)
    {
        var bookings = new List<Booking>(40000);
        var bookingId = 1;

        foreach (var trip in trips)
        {
            var isScenarioTrip = trip.TripInstanceId == WaitlistOverflowTripId;
            var isBreakdownReplaced = trip.TripInstanceId == BreakdownReplacedTripId;
            var isBreakdownCancelled = trip.TripInstanceId == BreakdownCancelledTripId;

            if (isScenarioTrip)
            {
                bookingId = GenerateWaitlistScenarioBookings(bookings, trip, bookingId);
                continue;
            }

            var convoy = ConvoyConfig[trip.StopId];
            var isSaturday = trip.TripDate.DayOfWeek == DayOfWeek.Saturday;
            var slotIdx = GetSlotIndex(trip, isSaturday);
            var loadPercent = isSaturday
                ? WeekdayLoadPercent[Math.Min(slotIdx, WeekdayLoadPercent.Length - 1)] * 0.6
                : WeekdayLoadPercent[slotIdx];

            var effectiveCapacity = convoy.Capacity;
            if (isBreakdownCancelled)
                effectiveCapacity = 57; // S1(22) + S2(35), S3 down

            var targetBookings = (int)(effectiveCapacity * loadPercent);
            targetBookings = Math.Max(targetBookings, 3); // minimum 3 bookings

            // Determine which students book this trip
            var dayOfWeek = trip.TripDate.DayOfWeek;
            var scheduleTimeId = trip.ScheduleTimeId;

            // Check if this trip has a priority booking
            int? priorityStudentId = null;
            foreach (var rule in PriorityBookingRules)
            {
                if (rule.ScheduleTimeId == scheduleTimeId && rule.Days.Contains(dayOfWeek))
                {
                    priorityStudentId = rule.StudentId;
                    break;
                }
            }

            // Pick students for this trip from primary stop + some overflow from neighbors
            var students = PickStudentsForTrip(trip.StopId, targetBookings, priorityStudentId);
            var noShowCount = Math.Max(1, (int)(students.Count * 0.05));
            var cancelledCount = _rng.Next(0, 2); // 0 or 1 cancellation

            // Determine operational shuttles for boarding assignment
            int[] operationalShuttles;
            if (isBreakdownReplaced)
                operationalShuttles = new[] { 1, 3, 11 }; // S2 down, S11 replacement
            else if (isBreakdownCancelled)
                operationalShuttles = new[] { convoy.ShuttleIds[0], convoy.ShuttleIds[1] }; // S3 down
            else
                operationalShuttles = convoy.ShuttleIds;

            var boardedCount = 0;
            var maxBoardable = isBreakdownCancelled ? 57 : effectiveCapacity;

            for (var i = 0; i < students.Count; i++)
            {
                var studentId = students[i];
                var isPriority = studentId == priorityStudentId && i == 0;
                var isCancelled = i >= students.Count - cancelledCount;
                var isNoShow = !isCancelled && i >= students.Count - cancelledCount - noShowCount;

                string bookingStatus;
                string boardedStatus;
                int? shuttleId = null;
                DateTime? boardedAt = null;
                DateTime createdAt;

                if (isCancelled)
                {
                    bookingStatus = "Cancelled";
                    boardedStatus = "Pending";
                    createdAt = ComputeBookingCreatedAt(trip.TripDate, isPriority);
                }
                else if (isNoShow)
                {
                    bookingStatus = "Confirmed";
                    boardedStatus = "NoShow";
                    createdAt = ComputeBookingCreatedAt(trip.TripDate, isPriority);
                }
                else if (boardedCount >= maxBoardable)
                {
                    // Breakdown scenario: couldn't board
                    bookingStatus = "Confirmed";
                    boardedStatus = "Pending";
                    createdAt = ComputeBookingCreatedAt(trip.TripDate, isPriority);
                }
                else
                {
                    bookingStatus = "Confirmed";
                    boardedStatus = "Boarded";
                    shuttleId = AssignShuttle(operationalShuttles, boardedCount);
                    boardedAt = ComputeBoardedAt(trip);
                    boardedCount++;
                    createdAt = ComputeBookingCreatedAt(trip.TripDate, isPriority);
                }

                var suffix = isPriority ? "pri" : isCancelled ? "cx" : "";
                var qrToken = $"qr_{trip.TripInstanceId}_s{studentId}_{bookingId}";

                bookings.Add(new Booking
                {
                    BookingId = bookingId,
                    StudentId = studentId,
                    TripInstanceId = trip.TripInstanceId,
                    BookingStatus = bookingStatus,
                    BookingType = isPriority ? "Priority" : "Standard",
                    BoardedStatus = boardedStatus,
                    QrCodeToken = qrToken,
                    ShuttleId = shuttleId,
                    BoardedAt = boardedAt,
                    CreatedAt = createdAt
                });

                bookingId++;
            }
        }

        return bookings;
    }

    private int GenerateWaitlistScenarioBookings(List<Booking> bookings, TripInstance trip, int bookingId)
    {
        // Trip 593: 79 Confirmed + 2 WaitingList + 2 Cancelled = 83 total
        var operationalShuttles = new[] { 1, 2, 3 };

        // Priority booking (Student 3, Wed)
        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 3,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "Confirmed",
            BookingType = "Priority",
            BoardedStatus = "Boarded",
            QrCodeToken = $"qr_593_s3_pri",
            ShuttleId = 1,
            BoardedAt = trip.InTransitAt!.Value.AddMinutes(-10),
            CreatedAt = new DateTime(2026, 8, 1, 22, 0, 0, DateTimeKind.Utc)
        });

        // 76 regular confirmed boarded + 2 NoShow = 78 standard confirmed
        var scenarioStudents = new HashSet<int> { 3, 45, 46, 42, 43, 39, 40 };
        var allStudents = StopStudents.Values.SelectMany(s => s).Where(s => !scenarioStudents.Contains(s)).ToList();
        Shuffle(allStudents);
        var boardedCount = 1; // priority already boarded

        for (var i = 0; i < 76 && i < allStudents.Count; i++)
        {
            var shuttleId = AssignShuttle(operationalShuttles, boardedCount);
            bookings.Add(new Booking
            {
                BookingId = bookingId++,
                StudentId = allStudents[i],
                TripInstanceId = trip.TripInstanceId,
                BookingStatus = "Confirmed",
                BookingType = "Standard",
                BoardedStatus = "Boarded",
                QrCodeToken = $"qr_593_s{allStudents[i]}_{bookingId}",
                ShuttleId = shuttleId,
                BoardedAt = ComputeBoardedAt(trip),
                CreatedAt = new DateTime(2026, 8, 3, 4 + _rng.Next(0, 12), _rng.Next(0, 60), 0, DateTimeKind.Utc)
            });
            boardedCount++;
        }

        // 2 NoShow
        for (var i = 76; i < 78 && i < allStudents.Count; i++)
        {
            bookings.Add(new Booking
            {
                BookingId = bookingId++,
                StudentId = allStudents[i],
                TripInstanceId = trip.TripInstanceId,
                BookingStatus = "Confirmed",
                BookingType = "Standard",
                BoardedStatus = "NoShow",
                QrCodeToken = $"qr_593_s{allStudents[i]}_ns_{bookingId}",
                ShuttleId = null,
                BoardedAt = null,
                CreatedAt = new DateTime(2026, 8, 3, 5 + _rng.Next(0, 8), _rng.Next(0, 60), 0, DateTimeKind.Utc)
            });
        }

        // 2 promoted students (45, 46) — now Confirmed/Boarded
        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 45,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "Confirmed",
            BookingType = "Standard",
            BoardedStatus = "Boarded",
            QrCodeToken = $"qr_593_s45_promoted",
            ShuttleId = AssignShuttle(operationalShuttles, boardedCount++),
            BoardedAt = ComputeBoardedAt(trip),
            CreatedAt = new DateTime(2026, 8, 3, 12, 30, 0, DateTimeKind.Utc)
        });

        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 46,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "Confirmed",
            BookingType = "Standard",
            BoardedStatus = "Boarded",
            QrCodeToken = $"qr_593_s46_promoted",
            ShuttleId = AssignShuttle(operationalShuttles, boardedCount++),
            BoardedAt = ComputeBoardedAt(trip),
            CreatedAt = new DateTime(2026, 8, 3, 12, 45, 0, DateTimeKind.Utc)
        });

        // 2 WaitingList (Students 42, 43)
        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 43,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "WaitingList",
            BookingType = "Standard",
            BoardedStatus = "Pending",
            QrCodeToken = $"qr_593_s43_wl",
            ShuttleId = null,
            BoardedAt = null,
            CreatedAt = new DateTime(2026, 8, 3, 13, 0, 0, DateTimeKind.Utc)
        });

        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 42,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "WaitingList",
            BookingType = "Standard",
            BoardedStatus = "Pending",
            QrCodeToken = $"qr_593_s42_wl",
            ShuttleId = null,
            BoardedAt = null,
            CreatedAt = new DateTime(2026, 8, 3, 13, 15, 0, DateTimeKind.Utc)
        });

        // 2 Cancelled (Students 39, 40)
        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 39,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "Cancelled",
            BookingType = "Standard",
            BoardedStatus = "Pending",
            QrCodeToken = $"qr_593_s39_cx",
            ShuttleId = null,
            BoardedAt = null,
            CreatedAt = new DateTime(2026, 8, 3, 6, 0, 0, DateTimeKind.Utc)
        });

        bookings.Add(new Booking
        {
            BookingId = bookingId++,
            StudentId = 40,
            TripInstanceId = trip.TripInstanceId,
            BookingStatus = "Cancelled",
            BookingType = "Standard",
            BoardedStatus = "Pending",
            QrCodeToken = $"qr_593_s40_cx",
            ShuttleId = null,
            BoardedAt = null,
            CreatedAt = new DateTime(2026, 8, 3, 7, 0, 0, DateTimeKind.Utc)
        });

        return bookingId;
    }

    private List<int> PickStudentsForTrip(int stopId, int count, int? priorityStudentId)
    {
        var result = new List<int>(count);

        // Priority student first
        if (priorityStudentId.HasValue)
            result.Add(priorityStudentId.Value);

        // Primary stop students
        var primary = StopStudents[stopId].Where(s => s != priorityStudentId).ToList();
        Shuffle(primary);
        result.AddRange(primary);

        // If we need more, pull from neighboring stops
        if (result.Count < count)
        {
            var overflow = StopStudents
                .Where(kv => kv.Key != stopId)
                .SelectMany(kv => kv.Value)
                .Where(s => s != priorityStudentId && !result.Contains(s))
                .ToList();
            Shuffle(overflow);
            result.AddRange(overflow.Take(count - result.Count));
        }

        return result.Take(count).ToList();
    }

    private int AssignShuttle(int[] operationalShuttles, int boardedSoFar)
    {
        var filled = 0;
        foreach (var shuttleId in operationalShuttles)
        {
            var cap = ShuttleCapacity[shuttleId];
            if (boardedSoFar < filled + cap)
                return shuttleId;
            filled += cap;
        }
        // Overflow: assign to last shuttle
        return operationalShuttles[^1];
    }

    private DateTime ComputeBookingCreatedAt(DateOnly tripDate, bool isPriority)
    {
        if (isPriority)
        {
            // Midnight SAST on (TripDate - 3) = (TripDate - 4 days) at 22:00 UTC
            return tripDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                .AddDays(-4).AddHours(22);
        }

        // Standard: 1-3 days before, random time 04:00-20:00 UTC (06:00-22:00 SAST)
        var daysBefore = _rng.Next(1, 4);
        var hour = _rng.Next(4, 20);
        var minute = _rng.Next(0, 60);
        return tripDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            .AddDays(-daysBefore).AddHours(hour).AddMinutes(minute);
    }

    private DateTime ComputeBoardedAt(TripInstance trip)
    {
        // BoardedAt: between T-35min and T-2min (spread within scanning window)
        var departureUtc = trip.InTransitAt!.Value;
        var windowStart = departureUtc.AddMinutes(-35);
        var offsetMinutes = _rng.Next(0, 34); // 0 to 33 minutes into window
        return windowStart.AddMinutes(offsetMinutes);
    }

    private int GetSlotIndex(TripInstance trip, bool isSaturday)
    {
        // Reverse-compute slot index from ScheduleTimeId
        var baseId = (trip.StopId - 1) * 19 + 1;
        var offset = trip.ScheduleTimeId - baseId;
        return isSaturday ? offset - 11 : offset;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    #endregion

    #region Breakdowns and Notes

    private (List<BreakdownLog>, List<TripNote>) GenerateBreakdownsAndNotes()
    {
        var breakdowns = new List<BreakdownLog>
        {
            new()
            {
                BreakdownLogId = 1,
                TripInstanceId = BreakdownReplacedTripId,
                ShuttleId = 2,
                VendorCompanyName = "Bay Transit Corp",
                FlaggedAt = new DateTime(2026, 8, 4, 4, 30, 0, DateTimeKind.Utc),
                FlaggedByMarshalId = 4,
                ResolutionType = "Replaced",
                ReplacementShuttleId = 11,
                ResolvedAt = new DateTime(2026, 8, 4, 4, 50, 0, DateTimeKind.Utc),
                ResolvedByAdminId = 1
            },
            new()
            {
                BreakdownLogId = 2,
                TripInstanceId = BreakdownCancelledTripId,
                ShuttleId = 3,
                VendorCompanyName = "Bay Transit Corp",
                FlaggedAt = new DateTime(2026, 8, 7, 4, 28, 0, DateTimeKind.Utc),
                FlaggedByMarshalId = 4,
                ResolutionType = "Cancelled",
                ReplacementShuttleId = null,
                ResolvedAt = new DateTime(2026, 8, 7, 4, 55, 0, DateTimeKind.Utc),
                ResolvedByAdminId = 2
            }
        };

        var notes = new List<TripNote>
        {
            new() { TripNoteId = 1, TripInstanceId = BreakdownReplacedTripId, NoteType = "Incident", Body = "Marshal reported Shuttle EC 256-491 failed to arrive at Summerstrand Village stop. Driver reported engine failure en route.", CreatedByAdminId = 1, CreatedAt = new DateTime(2026, 8, 4, 4, 35, 0, DateTimeKind.Utc), IsResolutionAction = false },
            new() { TripNoteId = 2, TripInstanceId = BreakdownReplacedTripId, NoteType = "Replacement", Body = "Replacement shuttle EC 331-058 dispatched from depot. ETA 15 minutes. Vendor confirmed availability.", CreatedByAdminId = 1, CreatedAt = new DateTime(2026, 8, 4, 4, 50, 0, DateTimeKind.Utc), IsResolutionAction = true },
            new() { TripNoteId = 3, TripInstanceId = BreakdownCancelledTripId, NoteType = "Incident", Body = "Marshal reported Shuttle EC 389-720 mechanical failure at stop. Tyre blowout.", CreatedByAdminId = 2, CreatedAt = new DateTime(2026, 8, 7, 4, 30, 0, DateTimeKind.Utc), IsResolutionAction = false },
            new() { TripNoteId = 4, TripInstanceId = BreakdownCancelledTripId, NoteType = "General", Body = "No replacement available from Bay Transit Corp. Remaining shuttles (S1, S2) absorbed passengers. Capacity reduced but manageable.", CreatedByAdminId = 2, CreatedAt = new DateTime(2026, 8, 7, 4, 55, 0, DateTimeKind.Utc), IsResolutionAction = true },
            new() { TripNoteId = 5, TripInstanceId = 100, NoteType = "General", Body = "High passenger volume noted. All shuttles at near capacity. Consider adding fourth shuttle for this route on Wednesdays.", CreatedByAdminId = 1, CreatedAt = new DateTime(2026, 7, 29, 5, 30, 0, DateTimeKind.Utc), IsResolutionAction = false },
            new() { TripNoteId = 6, TripInstanceId = 417, NoteType = "NoShow", Body = "Unusually high no-show rate (8 students). May correlate with reported campus event schedule change.", CreatedByAdminId = 2, CreatedAt = new DateTime(2026, 8, 3, 5, 45, 0, DateTimeKind.Utc), IsResolutionAction = false },
        };

        return (breakdowns, notes);
    }

    #endregion

    #region Notification Generation

    private List<Notification> GenerateNotifications(List<TripInstance> trips, List<Booking> bookings)
    {
        var notifications = new List<Notification>(3000);
        var notifId = 1;

        // Per seed.md recommendation: full notifications for scenario trips + ~50 sample trips
        var scenarioTripIds = new HashSet<int> { BreakdownReplacedTripId, WaitlistOverflowTripId, BreakdownCancelledTripId };

        // Pick 50 sample trips spread across different days
        var sampleTripIds = new HashSet<int>();
        for (var i = 0; i < 50; i++)
        {
            var idx = _rng.Next(trips.Count);
            sampleTripIds.Add(trips[idx].TripInstanceId);
        }

        var notifiableTripIds = scenarioTripIds.Union(sampleTripIds).ToHashSet();

        // Trip Locked notifications
        foreach (var booking in bookings.Where(b =>
            b.BookingStatus == "Confirmed" && notifiableTripIds.Contains(b.TripInstanceId)))
        {
            var trip = trips.First(t => t.TripInstanceId == booking.TripInstanceId);
            notifications.Add(new Notification
            {
                NotificationId = notifId++,
                OwnerId = booking.StudentId,
                OwnerType = "Student",
                Title = "Trip Locked",
                Message = $"Your trip is now locked. No changes can be made. Please be at the stop 10 minutes early.",
                IsRead = true,
                RelatedTripInstanceId = trip.TripInstanceId,
                CreatedAt = trip.LockedAt!.Value
            });
        }

        // Breakdown notifications for Trip 505
        var trip505Bookings = bookings.Where(b =>
            b.TripInstanceId == BreakdownReplacedTripId && b.BookingStatus == "Confirmed").ToList();
        foreach (var booking in trip505Bookings)
        {
            notifications.Add(new Notification
            {
                NotificationId = notifId++,
                OwnerId = booking.StudentId,
                OwnerType = "Student",
                Title = "Shuttle Update",
                Message = "Shuttle EC 256-491 on your trip experienced a breakdown. A replacement shuttle EC 331-058 has been dispatched. Your trip will proceed.",
                IsRead = true,
                RelatedTripInstanceId = BreakdownReplacedTripId,
                CreatedAt = new DateTime(2026, 8, 4, 4, 50, 0, DateTimeKind.Utc)
            });
        }

        // Breakdown notifications for Trip 769
        var trip769Bookings = bookings.Where(b =>
            b.TripInstanceId == BreakdownCancelledTripId && b.BookingStatus == "Confirmed").ToList();
        foreach (var booking in trip769Bookings)
        {
            notifications.Add(new Notification
            {
                NotificationId = notifId++,
                OwnerId = booking.StudentId,
                OwnerType = "Student",
                Title = "Shuttle Cancelled",
                Message = "Shuttle EC 389-720 on your trip broke down and no replacement is available. Remaining shuttles will serve all passengers. Expect possible delays.",
                IsRead = true,
                RelatedTripInstanceId = BreakdownCancelledTripId,
                CreatedAt = new DateTime(2026, 8, 7, 4, 55, 0, DateTimeKind.Utc)
            });
        }

        // Waitlist promoted notifications (Students 45, 46 on Trip 593)
        notifications.Add(new Notification
        {
            NotificationId = notifId++,
            OwnerId = 45,
            OwnerType = "Student",
            Title = "Waitlist Promoted",
            Message = "Great news! A spot opened up for your trip from Summerstrand Village (06:45, 05 Aug). Your booking is now confirmed.",
            IsRead = true,
            RelatedTripInstanceId = WaitlistOverflowTripId,
            CreatedAt = new DateTime(2026, 8, 5, 3, 45, 0, DateTimeKind.Utc)
        });

        notifications.Add(new Notification
        {
            NotificationId = notifId++,
            OwnerId = 46,
            OwnerType = "Student",
            Title = "Waitlist Promoted",
            Message = "Great news! A spot opened up for your trip from Summerstrand Village (06:45, 05 Aug). Your booking is now confirmed.",
            IsRead = false,
            RelatedTripInstanceId = WaitlistOverflowTripId,
            CreatedAt = new DateTime(2026, 8, 5, 3, 45, 0, DateTimeKind.Utc)
        });

        return notifications;
    }

    #endregion

    #region Report Generation

    private List<Report> GenerateReports()
    {
        return new List<Report>
        {
            new() { ReportId = 1, StudentId = 5, Category = "DriverBehavior", Description = "The driver of shuttle EC 628-945 was driving recklessly on Marine Drive, exceeding speed limit and overtaking dangerously. Several passengers felt unsafe.", Status = "Resolved", TripInstanceId = 12, AdminNotes = "Incident reported to Coastal Movers management. Driver has been given a written warning. Vendor confirmed corrective action.", CreatedAt = new DateTime(2026, 7, 28, 10, 30, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 7, 30, 14, 0, 0, DateTimeKind.Utc) },
            new() { ReportId = 2, StudentId = 21, Category = "VehicleQuality", Description = "Shuttle EC 714-382 had broken air conditioning. The vehicle was extremely hot and uncomfortable during the afternoon trip. Windows were also difficult to open.", Status = "Resolved", TripInstanceId = 107, AdminNotes = "Reported to Coastal Movers. Vehicle pulled from service for maintenance on 2026-08-01. Returned to service 2026-08-03 after AC repair confirmed.", CreatedAt = new DateTime(2026, 7, 30, 12, 45, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc) },
            new() { ReportId = 3, StudentId = 14, Category = "TimetableIssue", Description = "The 14:30 shuttle from South Campus consistently arrives 10-15 minutes late. This has happened three days in a row. Students are missing connections.", Status = "UnderReview", TripInstanceId = 314, AdminNotes = "Investigating with Bay Transit Corp dispatch. Requested GPS logs for the past week.", CreatedAt = new DateTime(2026, 7, 31, 13, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 8, 2, 11, 0, 0, DateTimeKind.Utc) },
            new() { ReportId = 4, StudentId = 22, Category = "Other", Description = "The shelter at Newton Park stop has a broken bench. Students have to stand while waiting for the shuttle in the rain.", Status = "Submitted", TripInstanceId = null, AdminNotes = null, CreatedAt = new DateTime(2026, 8, 3, 7, 30, 0, DateTimeKind.Utc), UpdatedAt = null },
            new() { ReportId = 5, StudentId = 7, Category = "DriverBehavior", Description = "Driver of shuttle EC 837-256 was on his phone while driving. Very dangerous and irresponsible.", Status = "UnderReview", TripInstanceId = 439, AdminNotes = "Report forwarded to Metro Link SA on 2026-08-05. Awaiting their investigation response.", CreatedAt = new DateTime(2026, 8, 4, 9, 15, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 8, 5, 10, 0, 0, DateTimeKind.Utc) },
            new() { ReportId = 6, StudentId = 16, Category = "VehicleQuality", Description = "Shuttle EC 256-491 has a strong diesel smell inside. Multiple passengers complained of nausea during the morning trip.", Status = "Resolved", TripInstanceId = 593, AdminNotes = "Bay Transit Corp inspected the vehicle. Exhaust seal replaced. Vehicle cleared for service after inspection on 2026-08-08.", CreatedAt = new DateTime(2026, 8, 5, 6, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 8, 8, 15, 30, 0, DateTimeKind.Utc) },
            new() { ReportId = 7, StudentId = 41, Category = "TimetableIssue", Description = "Saturday 08:45 shuttle from Greenacres never arrived. Waited 30 minutes then had to arrange alternative transport.", Status = "Submitted", TripInstanceId = 915, AdminNotes = null, CreatedAt = new DateTime(2026, 8, 8, 8, 0, 0, DateTimeKind.Utc), UpdatedAt = null },
            new() { ReportId = 8, StudentId = 55, Category = "Other", Description = "Marshal at Newton Park was rude to a student who arrived 2 minutes after departure time. The student was told to find your own way in front of other passengers.", Status = "UnderReview", TripInstanceId = 703, AdminNotes = "HR department notified. Reviewing CCTV footage from stop. Marshal will be counselled.", CreatedAt = new DateTime(2026, 8, 6, 7, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2026, 8, 9, 10, 0, 0, DateTimeKind.Utc) },
        };
    }

    #endregion
}
