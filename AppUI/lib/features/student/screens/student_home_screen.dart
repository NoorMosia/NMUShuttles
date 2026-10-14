import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/core/ui/error_display.dart';
import 'package:nmu_shuttle_app/features/student/providers/booking_providers.dart';
import 'package:nmu_shuttle_app/features/student/utils/booking_date_helpers.dart';
import 'package:nmu_shuttle_app/features/student/widgets/booking_card.dart';
import 'package:nmu_shuttle_app/features/student/widgets/date_separator.dart';
import 'package:nmu_shuttle_app/shared/widgets/student_shell.dart';

/// Student home screen with Upcoming/Past tabs and bottom navigation.
class StudentHomeScreen extends ConsumerStatefulWidget {
  const StudentHomeScreen({super.key});

  @override
  ConsumerState<StudentHomeScreen> createState() => _StudentHomeScreenState();
}

class _StudentHomeScreenState extends ConsumerState<StudentHomeScreen>
    with SingleTickerProviderStateMixin {
  late final TabController _tabController;
  bool _isFabExtended = true;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  bool _handleScrollNotification(ScrollNotification notification) {
    // Only react to vertical scrolls (ignore TabBarView horizontal swipes)
    if (notification.metrics.axis != Axis.vertical) return false;

    final offset = notification.metrics.pixels;
    final shouldExtend = offset <= 20;

    if (shouldExtend != _isFabExtended) {
      setState(() => _isFabExtended = shouldExtend);
    }
    return false;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          Column(
            children: [
              SizedBox(height: MediaQuery.of(context).padding.top + 60),
              // Tab bar
              Container(
                margin: const EdgeInsets.symmetric(horizontal: 16),
                decoration: BoxDecoration(
                  color: Colors.grey.shade100,
                  borderRadius: BorderRadius.circular(12),
                ),
                child: TabBar(
                  controller: _tabController,
                  indicator: BoxDecoration(
                    color: AppTheme.primaryGold,
                    borderRadius: BorderRadius.circular(12),
                  ),
                  indicatorSize: TabBarIndicatorSize.tab,
                  labelColor: AppTheme.navy,
                  unselectedLabelColor: Colors.grey.shade600,
                  labelStyle: const TextStyle(fontWeight: FontWeight.w600),
                  dividerColor: Colors.transparent,
                  tabs: const [
                    Tab(text: 'Upcoming'),
                    Tab(text: 'Past'),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              // Tab views
              Expanded(
                child: NotificationListener<ScrollNotification>(
                  onNotification: _handleScrollNotification,
                  child: TabBarView(
                    controller: _tabController,
                    children: const [
                      _UpcomingBookingsTab(),
                      _PastBookingsTab(),
                    ],
                  ),
                ),
              ),
            ],
          ),
          // Floating hamburger menu + Home button
          Positioned(
            top: MediaQuery.of(context).padding.top + 12,
            left: 16,
            right: 16,
            child: Row(
              children: [
                // Hamburger menu button
                Material(
                  color: Colors.white,
                  elevation: 0,
                  borderRadius: BorderRadius.circular(8),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(8),
                    onTap: () {
                      if (studentShellScaffoldKey.currentState?.hasDrawer ?? false) {
                        studentShellScaffoldKey.currentState!.openDrawer();
                      } else {
                        Scaffold.of(context).openDrawer();
                      }
                    },
                    child: const Padding(
                      padding: EdgeInsets.all(10),
                      child: Icon(
                        Icons.menu,
                        color: Color(0xFF132E51),
                        size: 24,
                      ),
                    ),
                  ),
                ),
                const SizedBox(width: 6),
                // Home button
                Expanded(
                  child: Material(
                    color: Colors.white,
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    child: InkWell(
                      borderRadius: BorderRadius.circular(8),
                      onTap: () => context.go('/student/home'),
                      child: const Padding(
                        padding: EdgeInsets.symmetric(horizontal: 10, vertical: 10),
                        child: Text(
                          'Home',
                          style: TextStyle(
                            color: Color(0xFF132E51),
                            fontWeight: FontWeight.w600,
                            fontSize: 18,
                          ),
                        ),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
      floatingActionButton: _AnimatedFab(
        isExtended: _isFabExtended,
        onPressed: () => context.push('/student/booking/new'),
      ),
    );
  }

}

/// Upcoming bookings tab content.
/// Shows bookings where tripDate >= today AND status != Cancelled.
class _UpcomingBookingsTab extends ConsumerWidget {
  const _UpcomingBookingsTab();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final upcomingAsync = ref.watch(upcomingBookingsProvider);

    return upcomingAsync.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, _) => ErrorRetry(
        error: error,
        title: 'Failed to load bookings',
        onRetry: () => ref.invalidate(upcomingBookingsProvider),
      ),
      data: (bookings) {
        if (bookings.isEmpty) {
          return RefreshIndicator(
            onRefresh: () async {
              ref.invalidate(upcomingBookingsProvider);
              await ref.read(upcomingBookingsProvider.future);
            },
            child: ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              children: [
                SizedBox(height: MediaQuery.of(context).size.height * 0.25),
                Center(
                  child: Column(
                    children: [
                      Icon(Icons.directions_bus_outlined,
                          size: 64, color: Colors.grey.shade400),
                      const SizedBox(height: 16),
                      Text(
                        'No upcoming bookings',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              color: Colors.grey.shade600,
                            ),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        'Tap the + button to book a shuttle',
                        style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                              color: Colors.grey.shade500,
                            ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        }

        final grouped = groupBookingsByDate(bookings);
        final dateKeys = grouped.keys.toList();

        return RefreshIndicator(
          onRefresh: () async {
            ref.invalidate(upcomingBookingsProvider);
            await ref.read(upcomingBookingsProvider.future);
          },
          child: ListView.builder(
            physics: const AlwaysScrollableScrollPhysics(),
            padding: const EdgeInsets.symmetric(horizontal: 16),
            itemCount: dateKeys.length,
            itemBuilder: (context, dateIndex) {
              final date = dateKeys[dateIndex];
              final dateBookings = grouped[date]!;
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  DateSeparator(label: formatDateLabel(date)),
                  ...dateBookings.map((booking) =>
                      BookingCard(booking: booking)),
                ],
              );
            },
          ),
        );
      },
    );
  }
}

/// Past bookings tab content.
/// Shows bookings where tripDate < today OR status == Cancelled.
class _PastBookingsTab extends ConsumerWidget {
  const _PastBookingsTab();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final pastAsync = ref.watch(pastBookingsProvider);

    return pastAsync.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, _) => ErrorRetry(
        error: error,
        title: 'Failed to load bookings',
        onRetry: () => ref.invalidate(pastBookingsProvider),
      ),
      data: (bookings) {
        if (bookings.isEmpty) {
          return RefreshIndicator(
            onRefresh: () async {
              ref.invalidate(pastBookingsProvider);
              await ref.read(pastBookingsProvider.future);
            },
            child: ListView(
              physics: const AlwaysScrollableScrollPhysics(),
              children: [
                SizedBox(height: MediaQuery.of(context).size.height * 0.25),
                Center(
                  child: Column(
                    children: [
                      Icon(Icons.history, size: 64, color: Colors.grey.shade400),
                      const SizedBox(height: 16),
                      Text(
                        'No past bookings',
                        style: Theme.of(context).textTheme.titleMedium?.copyWith(
                              color: Colors.grey.shade600,
                            ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        }

        final grouped = groupBookingsByDate(bookings);
        final dateKeys = grouped.keys.toList();

        return RefreshIndicator(
          onRefresh: () async {
            ref.invalidate(pastBookingsProvider);
            await ref.read(pastBookingsProvider.future);
          },
          child: ListView.builder(
            physics: const AlwaysScrollableScrollPhysics(),
            padding: const EdgeInsets.symmetric(horizontal: 16),
            itemCount: dateKeys.length,
            itemBuilder: (context, dateIndex) {
              final date = dateKeys[dateIndex];
              final dateBookings = grouped[date]!;
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  DateSeparator(label: formatDateLabel(date)),
                  ...dateBookings.map((booking) =>
                      BookingCard(booking: booking)),
                ],
              );
            },
          ),
        );
      },
    );
  }
}

/// A custom FAB that smoothly animates between extended (pill with label)
/// and collapsed (circle with just "+") states with a controlled duration.
class _AnimatedFab extends StatelessWidget {
  const _AnimatedFab({
    required this.isExtended,
    required this.onPressed,
  });

  final bool isExtended;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final fabColor = theme.colorScheme.primaryContainer;
    final iconColor = theme.colorScheme.onPrimaryContainer;

    return GestureDetector(
      onTap: onPressed,
      child: AnimatedContainer(
        duration: const Duration(milliseconds: 400),
        curve: Curves.easeInOut,
        height: 56,
        padding: EdgeInsets.symmetric(horizontal: isExtended ? 20 : 16),
        decoration: BoxDecoration(
          color: fabColor,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.add, color: iconColor),
            AnimatedSize(
              duration: const Duration(milliseconds: 400),
              curve: Curves.easeInOut,
              child: isExtended
                  ? Padding(
                      padding: const EdgeInsets.only(left: 8),
                      child: Text(
                        'New Booking',
                        style: TextStyle(
                          color: iconColor,
                          fontWeight: FontWeight.w500,
                          fontSize: 14,
                        ),
                        overflow: TextOverflow.clip,
                        maxLines: 1,
                      ),
                    )
                  : const SizedBox.shrink(),
            ),
          ],
        ),
      ),
    );
  }
}
