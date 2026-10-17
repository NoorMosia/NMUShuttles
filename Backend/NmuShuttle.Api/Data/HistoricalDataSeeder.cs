using Microsoft.EntityFrameworkCore;
using NmuShuttle.Api.Infrastructure;
using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Data;

/// <summary>
/// Lean demo seeder for historical (past) runtime data.
///
/// Instead of thousands of trips, this builds a small, hand-shaped dataset over
/// the last few operational days so a demo can show:
///   • Past bookings per student (Boarded / No-Show / Cancelled)
///   • Vendor/shuttle reliability analytics (one breakdown-replaced + one
///     breakdown-cancelled, tallied against shuttle and vendor)
///   • Student reports in various states (Submitted / Under Review / Resolved)
///   • A notification history (read + unread)
///
/// Dates are computed relative to "today" (SAST) so the data always looks recent
/// on demo day. All IDs are explicit and inserted with IDENTITY_INSERT.
///
/// Only runs in Development, and only if the TripInstances table is empty.
/// Uses a fixed Random seed (42) for reproducible output.
/// </summary>
public class HistoricalDataSeeder
{
    private readonly ShuttleDbContext _context;
    private readonly Random _rng = new(42);

    // ── Lean fleet (mirrors SeedData) ────────────────────────────────────────
    // ShuttleId == StopId for 1-8; shuttle 2 (Humewood) is the 2-seater; 9 is the spare.
    private static readonly Dictionary<int, int> ShuttleCapacity = new()
    {
        { 1, 22 }, { 2, 2 }, { 3, 22 }, { 4, 28 }, { 5, 22 },
        { 6, 35 }, { 7, 28 }, { 8, 35 }, { 9, 28 }
    };

    private static readonly Dictionary<int, string> ShuttlePlate = new()
    {
        { 1, "EC 101-001" }, { 2, "EC 102-002" }, { 3, "EC 103-003" }, { 4, "EC 104-004" },
        { 5, "EC 105-005" }, { 6, "EC 106-006" }, { 7, "EC 107-007" }, { 8, "EC 108-008" },
        { 9, "EC 109-009" }
    };

    private static readonly Dictionary<int, string> ShuttleVendor = new()
    {
        { 1, "Bay Transit Corp" }, { 2, "Bay Transit Corp" }, { 3, "Coastal Movers" },
        { 4, "Metro Link SA" }, { 5, "Bay Transit Corp" }, { 6, "Coastal Movers" },
        { 7, "Metro Link SA" }, { 8, "Bay Transit Corp" }, { 9, "Coastal Movers" }
    };

    // Each stop runs its single default shuttle (ShuttleId == StopId).
    private static int StopShuttle(int stopId) => stopId;

    // A few active students per stop, used to populate past bookings believably.
    // All IDs exist in SeedData (documented 1-15 + extended 16-60).
    private static readonly Dictionary<int, int[]> StopStudents = new()
    {
        { 1, new[] { 1, 2, 3, 16, 51 } },
        { 2, new[] { 4, 5, 6, 19, 53 } },
        { 3, new[] { 7, 8, 9, 22, 55 } },
        { 4, new[] { 10, 11, 13, 25, 57 } },
        { 5, new[] { 12, 14, 35, 28, 58 } },
        { 6, new[] { 15, 31, 32, 59 } },
        { 7, new[] { 34, 36, 37, 60 } },
        { 8, new[] { 39, 40, 41, 42 } },
    };

    // ── Running ID counters (IDENTITY_INSERT) ────────────────────────────────
    private int _tripId = 1;
    private int _allocId = 1;
    private int _bookingId = 1;
    private int _notifId = 1;

    public HistoricalDataSeeder(ShuttleDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.TripInstances.AnyAsync())
            return; // Already seeded

        var trips = new List<TripInstance>();
        var allocations = new List<TripShuttleAllocation>();
        var bookings = new List<Booking>();
        var breakdownLogs = new List<BreakdownLog>();
        var tripNotes = new List<TripNote>();
        var notifications = new List<Notification>();
        var reports = new List<Report>();

        // Build 5 past operational weekdays (skipping Sundays), ending yesterday.
        var pastDays = BuildPastWeekdays(5);

        // Scenario markers filled in while generating trips.
        int? breakdownReplacedTripId = null;
        int? breakdownCancelledTripId = null;

        for (var dayIndex = 0; dayIndex < pastDays.Count; dayIndex++)
        {
            var date = pastDays[dayIndex];

            // Keep it lean: 2 timeslots per stop per day (a morning ToCampus and an
            // afternoon FromCampus), across all 8 stops.
            for (var stopId = 1; stopId <= 8; stopId++)
            {
                foreach (var slot in new[] { TripSlot.MorningToCampus, TripSlot.AfternoonFromCampus })
                {
                    var scheduleTimeId = ResolveScheduleTimeId(stopId, slot);
                    var departure = slot == TripSlot.MorningToCampus
                        ? new TimeOnly(6, 45)
                        : new TimeOnly(16, 0);

                    var trip = BuildTrip(stopId, scheduleTimeId, date, departure);
                    trips.Add(trip);

                    // Decide if this trip is a breakdown scenario.
                    // Replaced: Walmer Heights (Stop 4) morning on the 2nd-oldest day.
                    // Cancelled: Framesby (Stop 7) morning on the 3rd-oldest day.
                    var isReplaced = breakdownReplacedTripId == null &&
                                     stopId == 4 && slot == TripSlot.MorningToCampus && dayIndex == 1;
                    var isCancelled = breakdownCancelledTripId == null &&
                                      stopId == 7 && slot == TripSlot.MorningToCampus && dayIndex == 2;

                    if (isReplaced) breakdownReplacedTripId = trip.TripInstanceId;
                    if (isCancelled) breakdownCancelledTripId = trip.TripInstanceId;

                    allocations.AddRange(BuildAllocations(trip, isReplaced, isCancelled));
                    bookings.AddRange(BuildBookings(trip, slot, isReplaced, isCancelled));
                }
            }
        }

        // Breakdown logs, trip notes and passenger notifications for the two scenarios.
        if (breakdownReplacedTripId is int replacedId)
        {
            var trip = trips.First(t => t.TripInstanceId == replacedId);
            breakdownLogs.Add(BuildReplacedBreakdownLog(trip));
            tripNotes.AddRange(BuildReplacedNotes(trip));
            notifications.AddRange(BuildBreakdownNotifications(trip, bookings, replaced: true));
        }

        if (breakdownCancelledTripId is int cancelledId)
        {
            var trip = trips.First(t => t.TripInstanceId == cancelledId);
            breakdownLogs.Add(BuildCancelledBreakdownLog(trip));
            tripNotes.AddRange(BuildCancelledNotes(trip));
            notifications.AddRange(BuildBreakdownNotifications(trip, bookings, replaced: false));
        }

        // A handful of generic "Trip Completed" notifications (read) for students 1 & 2
        // (they have DeviceRegistrations seeded) so the notification list isn't empty.
        notifications.AddRange(BuildHistoryNotifications(trips));

        reports.AddRange(BuildReports(trips));

        // Insert in FK-safe order with IDENTITY_INSERT.
        await InsertWithIdentity("TripInstances", trips);
        await InsertWithIdentity("TripShuttleAllocations", allocations);
        await InsertWithIdentity("Bookings", bookings);
        await InsertWithIdentity("BreakdownLogs", breakdownLogs);
        await InsertWithIdentity("TripNotes", tripNotes);
        await InsertWithIdentity("Notifications", notifications);
        await InsertWithIdentity("Reports", reports);
    }

    private enum TripSlot
    {
        MorningToCampus,
        AfternoonFromCampus
    }

    // ── Date helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the <paramref name="count"/> most recent operational weekdays before
    /// today (SAST), oldest first. Skips Sundays (keeps it simple — matches the
    /// generator's primary skip rule for a lean demo).
    /// </summary>
    private static List<DateOnly> BuildPastWeekdays(int count)
    {
        var days = new List<DateOnly>();
        var cursor = SastClock.Today.AddDays(-1);

        while (days.Count < count)
        {
            if (cursor.DayOfWeek != DayOfWeek.Sunday)
                days.Add(cursor);
            cursor = cursor.AddDays(-1);
        }

        days.Reverse(); // oldest first
        return days;
    }

    /// <summary>
    /// Resolves the seeded ScheduleTimeId for a stop + slot. ScheduleTimes are seeded
    /// in blocks of 19 per stop: weekday ToCampus 06:45 is offset 0, weekday FromCampus
    /// 16:00 is offset 6 (ToCampus 06:45/07:45/08:45/09:45 then FromCampus
    /// 12:30/14:30/16:00/...).
    /// </summary>
    private static int ResolveScheduleTimeId(int stopId, TripSlot slot)
    {
        var baseId = (stopId - 1) * 19 + 1;
        return slot == TripSlot.MorningToCampus ? baseId + 0 : baseId + 6;
    }

    // ── Trip / allocation / booking builders ───────────────────────────────────

    private TripInstance BuildTrip(int stopId, int scheduleTimeId, DateOnly date, TimeOnly departure)
    {
        var departureUtc = DateTime.SpecifyKind(SastClock.ToUtc(date, departure), DateTimeKind.Utc);
        return new TripInstance
        {
            TripInstanceId = _tripId++,
            ScheduleTimeId = scheduleTimeId,
            StopId = stopId,
            TripDate = date,
            Status = "Completed",
            LockedAt = departureUtc.AddHours(-1),
            InTransitAt = departureUtc,
            CompletedAt = departureUtc.AddMinutes(45)
        };
    }

    private IEnumerable<TripShuttleAllocation> BuildAllocations(TripInstance trip, bool isReplaced, bool isCancelled)
    {
        var frontShuttle = StopShuttle(trip.StopId);
        var result = new List<TripShuttleAllocation>
        {
            new()
            {
                AllocationId = _allocId++,
                TripInstanceId = trip.TripInstanceId,
                ShuttleId = frontShuttle,
                OrderPosition = 1,
                // Replaced/Cancelled: the stop's own shuttle broke down → not operational.
                IsOperational = !(isReplaced || isCancelled),
                AssignedAt = trip.LockedAt!.Value
            }
        };

        if (isReplaced)
        {
            // Spare shuttle 9 is dispatched as the replacement, ~20 min after the flag.
            result.Add(new TripShuttleAllocation
            {
                AllocationId = _allocId++,
                TripInstanceId = trip.TripInstanceId,
                ShuttleId = 9,
                OrderPosition = 2,
                IsOperational = true,
                AssignedAt = trip.LockedAt!.Value.AddMinutes(40)
            });
        }

        return result;
    }

    private IEnumerable<Booking> BuildBookings(TripInstance trip, TripSlot slot, bool isReplaced, bool isCancelled)
    {
        var frontShuttle = StopShuttle(trip.StopId);
        var capacity = ShuttleCapacity[frontShuttle] + (isReplaced ? ShuttleCapacity[9] : 0);

        // For the 2-seat Humewood stop, cap demand so it stays believable.
        var pool = StopStudents[trip.StopId];
        var demand = Math.Min(pool.Length, Math.Max(2, capacity));
        // Morning trips run a bit fuller than afternoon.
        if (slot == TripSlot.AfternoonFromCampus) demand = Math.Max(2, demand - 1);

        var chosen = pool.Take(demand).ToList();
        var result = new List<Booking>();

        for (var i = 0; i < chosen.Count; i++)
        {
            var studentId = chosen[i];
            string bookingStatus = "Confirmed";
            string boardedStatus;
            int? shuttleId;
            DateTime? boardedAt;

            if (isCancelled && i >= capacity)
            {
                // No replacement — passengers beyond remaining capacity could not board.
                boardedStatus = "Pending";
                shuttleId = null;
                boardedAt = null;
            }
            else if (i == chosen.Count - 1 && chosen.Count > 2)
            {
                // Last passenger: a no-show (nice for analytics), unless it's the tiny Humewood trip.
                boardedStatus = "NoShow";
                shuttleId = null;
                boardedAt = null;
            }
            else
            {
                boardedStatus = "Boarded";
                shuttleId = isReplaced && i >= ShuttleCapacity[frontShuttle] ? 9 : frontShuttle;
                boardedAt = trip.InTransitAt!.Value.AddMinutes(-20 + _rng.Next(0, 18));
            }

            result.Add(new Booking
            {
                BookingId = _bookingId++,
                StudentId = studentId,
                TripInstanceId = trip.TripInstanceId,
                BookingStatus = bookingStatus,
                BookingType = "Standard",
                BoardedStatus = boardedStatus,
                QrCodeToken = $"hist_t{trip.TripInstanceId}_s{studentId}_{_bookingId}",
                ShuttleId = shuttleId,
                BoardedAt = boardedAt,
                CreatedAt = trip.TripDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                    .AddDays(-1).AddHours(14 + _rng.Next(0, 6))
            });
        }

        // Add one explicitly cancelled booking per morning trip for realism.
        if (slot == TripSlot.MorningToCampus)
        {
            var extra = StopStudents[trip.StopId].LastOrDefault();
            // Only if that student isn't already booked above.
            if (!chosen.Contains(extra) && extra != 0)
            {
                result.Add(new Booking
                {
                    BookingId = _bookingId++,
                    StudentId = extra,
                    TripInstanceId = trip.TripInstanceId,
                    BookingStatus = "Cancelled",
                    BookingType = "Standard",
                    BoardedStatus = "Pending",
                    QrCodeToken = $"hist_t{trip.TripInstanceId}_s{extra}_cx_{_bookingId}",
                    ShuttleId = null,
                    BoardedAt = null,
                    CreatedAt = trip.TripDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
                        .AddDays(-2).AddHours(10)
                });
            }
        }

        return result;
    }

    // ── Breakdown scenarios ─────────────────────────────────────────────────────

    private BreakdownLog BuildReplacedBreakdownLog(TripInstance trip)
    {
        var brokenShuttle = StopShuttle(trip.StopId);
        var flaggedAt = trip.LockedAt!.Value.AddMinutes(20);
        return new BreakdownLog
        {
            BreakdownLogId = 1,
            TripInstanceId = trip.TripInstanceId,
            ShuttleId = brokenShuttle,
            VendorCompanyName = ShuttleVendor[brokenShuttle],
            FlaggedAt = flaggedAt,
            FlaggedByMarshalId = MarshalForStop(trip.StopId),
            ResolutionType = "Replaced",
            ReplacementShuttleId = 9,
            ResolvedAt = flaggedAt.AddMinutes(20),
            ResolvedByAdminId = 1
        };
    }

    private BreakdownLog BuildCancelledBreakdownLog(TripInstance trip)
    {
        var brokenShuttle = StopShuttle(trip.StopId);
        var flaggedAt = trip.LockedAt!.Value.AddMinutes(15);
        return new BreakdownLog
        {
            BreakdownLogId = 2,
            TripInstanceId = trip.TripInstanceId,
            ShuttleId = brokenShuttle,
            VendorCompanyName = ShuttleVendor[brokenShuttle],
            FlaggedAt = flaggedAt,
            FlaggedByMarshalId = MarshalForStop(trip.StopId),
            ResolutionType = "Cancelled",
            ReplacementShuttleId = null,
            ResolvedAt = flaggedAt.AddMinutes(25),
            ResolvedByAdminId = 2
        };
    }

    private IEnumerable<TripNote> BuildReplacedNotes(TripInstance trip)
    {
        var brokenShuttle = StopShuttle(trip.StopId);
        var flaggedAt = trip.LockedAt!.Value.AddMinutes(20);
        return new[]
        {
            new TripNote
            {
                TripNoteId = 1,
                TripInstanceId = trip.TripInstanceId,
                NoteType = "Incident",
                Body = $"Marshal reported shuttle {ShuttlePlate[brokenShuttle]} failed to arrive. Driver reported engine trouble en route.",
                CreatedByAdminId = 1,
                CreatedAt = flaggedAt.AddMinutes(5),
                IsResolutionAction = false
            },
            new TripNote
            {
                TripNoteId = 2,
                TripInstanceId = trip.TripInstanceId,
                NoteType = "Replacement",
                Body = $"Replacement shuttle {ShuttlePlate[9]} dispatched from depot. Vendor confirmed availability.",
                CreatedByAdminId = 1,
                CreatedAt = flaggedAt.AddMinutes(20),
                IsResolutionAction = true
            }
        };
    }

    private IEnumerable<TripNote> BuildCancelledNotes(TripInstance trip)
    {
        var brokenShuttle = StopShuttle(trip.StopId);
        var flaggedAt = trip.LockedAt!.Value.AddMinutes(15);
        return new[]
        {
            new TripNote
            {
                TripNoteId = 3,
                TripInstanceId = trip.TripInstanceId,
                NoteType = "Incident",
                Body = $"Marshal reported shuttle {ShuttlePlate[brokenShuttle]} mechanical failure at the stop (tyre blowout).",
                CreatedByAdminId = 2,
                CreatedAt = flaggedAt.AddMinutes(3),
                IsResolutionAction = false
            },
            new TripNote
            {
                TripNoteId = 4,
                TripInstanceId = trip.TripInstanceId,
                NoteType = "General",
                Body = $"No replacement available from {ShuttleVendor[brokenShuttle]}. Trip marked cancelled; affected students notified.",
                CreatedByAdminId = 2,
                CreatedAt = flaggedAt.AddMinutes(25),
                IsResolutionAction = true
            }
        };
    }

    private IEnumerable<Notification> BuildBreakdownNotifications(
        TripInstance trip, List<Booking> bookings, bool replaced)
    {
        var brokenShuttle = StopShuttle(trip.StopId);
        var resolvedAt = replaced
            ? trip.LockedAt!.Value.AddMinutes(40)
            : trip.LockedAt!.Value.AddMinutes(40);

        var title = replaced ? "Shuttle Update" : "Shuttle Cancelled";
        var message = replaced
            ? $"Shuttle {ShuttlePlate[brokenShuttle]} on your trip broke down. Replacement {ShuttlePlate[9]} has been dispatched — your trip will proceed."
            : $"Shuttle {ShuttlePlate[brokenShuttle]} on your trip broke down and no replacement was available. We're sorry for the disruption.";

        var affected = bookings
            .Where(b => b.TripInstanceId == trip.TripInstanceId && b.BookingStatus == "Confirmed")
            .Select(b => b.StudentId)
            .Distinct();

        var result = new List<Notification>();
        foreach (var studentId in affected)
        {
            result.Add(new Notification
            {
                NotificationId = _notifId++,
                OwnerId = studentId,
                OwnerType = "Student",
                Title = title,
                Message = message,
                IsRead = true,
                RelatedTripInstanceId = trip.TripInstanceId,
                CreatedAt = resolvedAt
            });
        }

        return result;
    }

    private IEnumerable<Notification> BuildHistoryNotifications(List<TripInstance> trips)
    {
        // A couple of friendly "trip completed" notices for students 1 & 2 so the
        // in-app notification history has read items alongside the unread demo one.
        var result = new List<Notification>();
        var recent = trips
            .Where(t => StopStudents[t.StopId].Contains(1) || StopStudents[t.StopId].Contains(2))
            .OrderByDescending(t => t.TripDate)
            .Take(2)
            .ToList();

        foreach (var trip in recent)
        {
            var studentId = StopStudents[trip.StopId].Contains(1) ? 1 : 2;
            result.Add(new Notification
            {
                NotificationId = _notifId++,
                OwnerId = studentId,
                OwnerType = "Student",
                Title = "Trip Completed",
                Message = "Thanks for riding with NMU Shuttle. Your trip is complete.",
                IsRead = true,
                RelatedTripInstanceId = trip.TripInstanceId,
                CreatedAt = trip.CompletedAt!.Value
            });
        }

        return result;
    }

    // ── Reports ──────────────────────────────────────────────────────────────

    private List<Report> BuildReports(List<TripInstance> trips)
    {
        // Attach reports to real trips where possible for believable deep-links.
        int? TripAt(int stopId) =>
            trips.Where(t => t.StopId == stopId)
                 .OrderByDescending(t => t.TripDate)
                 .Select(t => (int?)t.TripInstanceId)
                 .FirstOrDefault();

        var now = SastClock.UtcNow;

        return new List<Report>
        {
            new()
            {
                ReportId = 1, StudentId = 5, Category = "DriverBehavior",
                Description = "Driver was overtaking dangerously on Marine Drive. Several passengers felt unsafe.",
                Status = "Resolved", TripInstanceId = TripAt(2),
                AdminNotes = "Raised with the vendor. Driver given a written warning; vendor confirmed corrective action.",
                CreatedAt = now.AddDays(-4), UpdatedAt = now.AddDays(-2)
            },
            new()
            {
                ReportId = 2, StudentId = 22, Category = "VehicleQuality",
                Description = "Shuttle had broken air conditioning. Very hot and uncomfortable on the afternoon trip.",
                Status = "Resolved", TripInstanceId = TripAt(3),
                AdminNotes = "Vehicle pulled for maintenance and returned to service after AC repair confirmed.",
                CreatedAt = now.AddDays(-3), UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                ReportId = 3, StudentId = 14, Category = "TimetableIssue",
                Description = "The afternoon shuttle has arrived 10-15 minutes late three days in a row.",
                Status = "UnderReview", TripInstanceId = TripAt(5),
                AdminNotes = "Investigating with vendor dispatch. Requested GPS logs for the week.",
                CreatedAt = now.AddDays(-2), UpdatedAt = now.AddDays(-1)
            },
            new()
            {
                ReportId = 4, StudentId = 7, Category = "Other",
                Description = "The shelter at the stop has a broken bench. Students stand in the rain while waiting.",
                Status = "Submitted", TripInstanceId = null,
                AdminNotes = null, CreatedAt = now.AddDays(-1), UpdatedAt = null
            },
            new()
            {
                ReportId = 5, StudentId = 16, Category = "VehicleQuality",
                Description = "Strong diesel smell inside the vehicle. A few passengers felt nauseous.",
                Status = "Submitted", TripInstanceId = TripAt(1),
                AdminNotes = null, CreatedAt = now.AddHours(-18), UpdatedAt = null
            },
        };
    }

    // ── Lookups ────────────────────────────────────────────────────────────────

    /// <summary>Marshal assigned to a stop in SeedData (MarshalIds 4-11 map to Stops 1-8).</summary>
    private static int MarshalForStop(int stopId) => stopId + 3;

    // ── Insert plumbing ──────────────────────────────────────────────────────

    private async Task InsertWithIdentity<T>(string tableName, List<T> entities) where T : class
    {
        if (entities.Count == 0) return;

        // PostgreSQL identity columns are created as GENERATED BY DEFAULT AS IDENTITY,
        // so explicit PK values can be inserted directly — no SET IDENTITY_INSERT needed
        // (that is SQL Server syntax). After inserting explicit IDs we must advance the
        // backing identity sequence so the next auto-generated value doesn't collide.
        var entityType = _context.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException($"No EF entity type for {typeof(T).Name}.");
        var keyProperty = entityType.FindPrimaryKey()?.Properties.Single()
            ?? throw new InvalidOperationException($"No single-column primary key for {typeof(T).Name}.");
        var keyColumn = keyProperty.GetColumnName();

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            _context.Set<T>().AddRange(entities);
            await _context.SaveChangesAsync();

            foreach (var entry in _context.ChangeTracker.Entries().ToList())
                entry.State = EntityState.Detached;

            // Advance the identity sequence to MAX(id) so future inserts don't clash.
            var resetSequenceSql =
                $"SELECT setval(" +
                $"pg_get_serial_sequence('\"{tableName}\"', '{keyColumn}'), " +
                $"(SELECT MAX(\"{keyColumn}\") FROM \"{tableName}\"))";
#pragma warning disable EF1002 // Table and column names come from EF model metadata, not user input
            await _context.Database.ExecuteSqlRawAsync(resetSequenceSql);
#pragma warning restore EF1002

            await transaction.CommitAsync();
        });
    }
}
