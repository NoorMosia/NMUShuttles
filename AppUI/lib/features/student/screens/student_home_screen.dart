import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/providers/booking_providers.dart';
import 'package:nmu_shuttle_app/features/student/widgets/booking_card.dart';
import 'package:nmu_shuttle_app/shared/widgets/hamburger_menu_button.dart';

/// Student home screen with Upcoming/Past tabs and bottom navigation.
class StudentHomeScreen extends ConsumerStatefulWidget {
  const StudentHomeScreen({super.key});

  @override
  ConsumerState<StudentHomeScreen> createState() => _StudentHomeScreenState();
}

class _StudentHomeScreenState extends ConsumerState<StudentHomeScreen>
    with SingleTickerProviderStateMixin {
  late final TabController _tabController;
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
                child: TabBarView(
                  controller: _tabController,
                  children: const [
                    _UpcomingBookingsTab(),
                    _PastBookingsTab(),
                  ],
                ),
              ),
            ],
          ),
          // Floating hamburger menu
          const HamburgerMenuButton(),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => context.push('/student/booking/new'),
        icon: const Icon(Icons.add),
        label: const Text('New Booking'),
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
      error: (error, _) => Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 48, color: Colors.red),
            const SizedBox(height: 12),
            Text(
              'Failed to load bookings',
              style: Theme.of(context).textTheme.bodyLarge,
            ),
            const SizedBox(height: 8),
            TextButton(
              onPressed: () => ref.invalidate(bookingsProvider),
              child: const Text('Retry'),
            ),
          ],
        ),
      ),
      data: (bookings) {
        if (bookings.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
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
          );
        }
        return RefreshIndicator(
          onRefresh: () async => ref.invalidate(bookingsProvider),
          child: ListView.builder(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            itemCount: bookings.length,
            itemBuilder: (context, index) =>
                BookingCard(booking: bookings[index]),
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
      error: (error, _) => Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            const Icon(Icons.error_outline, size: 48, color: Colors.red),
            const SizedBox(height: 12),
            Text(
              'Failed to load bookings',
              style: Theme.of(context).textTheme.bodyLarge,
            ),
            const SizedBox(height: 8),
            TextButton(
              onPressed: () => ref.invalidate(bookingsProvider),
              child: const Text('Retry'),
            ),
          ],
        ),
      ),
      data: (bookings) {
        if (bookings.isEmpty) {
          return Center(
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
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
          );
        }
        return RefreshIndicator(
          onRefresh: () async => ref.invalidate(bookingsProvider),
          child: ListView.builder(
            padding: const EdgeInsets.symmetric(horizontal: 16),
            itemCount: bookings.length,
            itemBuilder: (context, index) =>
                BookingCard(booking: bookings[index]),
          ),
        );
      },
    );
  }
}
