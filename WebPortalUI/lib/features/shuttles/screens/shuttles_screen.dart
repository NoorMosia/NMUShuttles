import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/core/validators.dart';
import 'package:nmu_shuttle_admin/features/shuttles/providers/shuttles_providers.dart';
import 'package:nmu_shuttle_admin/features/vendors/providers/vendors_providers.dart';

/// Full CRUD screen for managing shuttle vehicles.
class ShuttlesScreen extends ConsumerStatefulWidget {
  const ShuttlesScreen({super.key});

  @override
  ConsumerState<ShuttlesScreen> createState() => _ShuttlesScreenState();
}

class _ShuttlesScreenState extends ConsumerState<ShuttlesScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(shuttlesProvider.notifier).fetchShuttles();
      ref.read(vendorsProvider.notifier).fetchVendors();
    });
  }

  @override
  Widget build(BuildContext context) {
    final shuttlesState = ref.watch(shuttlesProvider);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header row
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Shuttles',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 28,
                      fontWeight: FontWeight.bold,
                      color: _navy,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Manage shuttle vehicles and vendors',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 14,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
              FilledButton.icon(
                onPressed: () => _showShuttleDialog(context),
                icon: const Icon(Icons.add),
                label: Text(
                  'Add Shuttle',
                  style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
                ),
                style: FilledButton.styleFrom(
                  backgroundColor: _navy,
                  foregroundColor: Colors.white,
                  padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 24),

          // Error message
          if (shuttlesState.errorMessage != null)
            Padding(
              padding: const EdgeInsets.only(bottom: 16),
              child: Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: Colors.red.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.red.shade200),
                ),
                child: Row(
                  children: [
                    Icon(Icons.error_outline, color: Colors.red.shade600, size: 20),
                    const SizedBox(width: 8),
                    Text(
                      shuttlesState.errorMessage!,
                      style: GoogleFonts.hankenGrotesk(color: Colors.red.shade700),
                    ),
                  ],
                ),
              ),
            ),

          // Data table
          Expanded(
            child: shuttlesState.isLoading
                ? const Center(child: CircularProgressIndicator())
                : Card(
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                      side: BorderSide(color: Colors.grey.shade200),
                    ),
                    child: shuttlesState.shuttles.isEmpty
                        ? Center(
                            child: Text(
                              'No shuttles found. Click "Add Shuttle" to create one.',
                              style: GoogleFonts.hankenGrotesk(
                                color: Colors.grey.shade500,
                                fontSize: 14,
                              ),
                            ),
                          )
                        : SizedBox(
                            width: double.infinity,
                            child: SingleChildScrollView(
                              child: DataTable(
                                headingRowColor: WidgetStateProperty.all(
                                  _navy.withValues(alpha: 0.05),
                                ),
                                dataRowColor: WidgetStateProperty.resolveWith<Color?>(
                                  (states) => Colors.transparent,
                                ),
                                headingRowHeight: 52,
                                dataRowMinHeight: 48,
                                dataRowMaxHeight: 56,
                                columnSpacing: 32,
                                horizontalMargin: 20,
                                columns: [
                                  DataColumn(
                                    label: Text(
                                      'License Plate',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'Capacity',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                    numeric: true,
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'Vendor',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'Accessible',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'Status',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'Actions',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                ],
                                rows: shuttlesState.shuttles.map((shuttle) {
                                  return DataRow(
                                    cells: [
                                      DataCell(Text(
                                        shuttle.licensePlate,
                                        style: GoogleFonts.hankenGrotesk(
                                          fontSize: 13,
                                          fontWeight: FontWeight.w500,
                                        ),
                                      )),
                                      DataCell(Text(
                                        shuttle.capacity.toString(),
                                        style: GoogleFonts.hankenGrotesk(fontSize: 13),
                                      )),
                                      DataCell(Text(
                                        shuttle.vendorCompanyName,
                                        style: GoogleFonts.hankenGrotesk(fontSize: 13),
                                      )),
                                      DataCell(
                                        Icon(
                                          shuttle.isAccessibleVehicle
                                              ? Icons.check_circle
                                              : Icons.cancel_outlined,
                                          color: shuttle.isAccessibleVehicle
                                              ? Colors.green.shade600
                                              : Colors.grey.shade400,
                                          size: 20,
                                        ),
                                      ),
                                      DataCell(
                                        Container(
                                          padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                                          decoration: BoxDecoration(
                                            color: shuttle.isActive
                                                ? Colors.green.shade50
                                                : Colors.red.shade50,
                                            borderRadius: BorderRadius.circular(12),
                                            border: Border.all(
                                              color: shuttle.isActive
                                                  ? Colors.green.shade300
                                                  : Colors.red.shade300,
                                            ),
                                          ),
                                          child: Text(
                                            shuttle.isActive ? 'Active' : 'Inactive',
                                            style: GoogleFonts.hankenGrotesk(
                                              fontSize: 11,
                                              fontWeight: FontWeight.w600,
                                              color: shuttle.isActive
                                                  ? Colors.green.shade700
                                                  : Colors.red.shade700,
                                            ),
                                          ),
                                        ),
                                      ),
                                      DataCell(Row(
                                        mainAxisSize: MainAxisSize.min,
                                        children: [
                                          IconButton(
                                            icon: Icon(Icons.edit_outlined, color: _navy, size: 20),
                                            tooltip: 'Edit',
                                            onPressed: () => _showShuttleDialog(context, shuttle: shuttle),
                                          ),
                                          if (shuttle.isActive)
                                            IconButton(
                                              icon: Icon(Icons.block, color: Colors.orange.shade700, size: 20),
                                              tooltip: 'Deactivate',
                                              onPressed: () => _confirmDeactivate(context, shuttle),
                                            )
                                          else
                                            IconButton(
                                              icon: Icon(Icons.play_circle_outline, color: Colors.green.shade700, size: 20),
                                              tooltip: 'Reactivate',
                                              onPressed: () => _reactivateShuttle(shuttle),
                                            ),
                                        ],
                                      )),
                                    ],
                                  );
                                }).toList(),
                              ),
                            ),
                          ),
                  ),
          ),
        ],
      ),
    );
  }

  /// Confirms deactivation of a shuttle.
  void _confirmDeactivate(BuildContext context, Shuttle shuttle) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        title: Text(
          'Deactivate Shuttle',
          style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.bold, color: _navy),
        ),
        content: Text(
          'This will remove ${shuttle.licensePlate} from all default convoys. '
          'Existing trip allocations will not be affected.\n\nProceed?',
          style: GoogleFonts.hankenGrotesk(),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () async {
              Navigator.of(ctx).pop();
              final error = await ref.read(shuttlesProvider.notifier).deactivateShuttle(shuttle.id);
              if (error != null && context.mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(content: Text(error), backgroundColor: Colors.red.shade600),
                );
              }
            },
            style: FilledButton.styleFrom(backgroundColor: Colors.orange.shade700),
            child: const Text('Deactivate'),
          ),
        ],
      ),
    );
  }

  /// Reactivates a shuttle.
  Future<void> _reactivateShuttle(Shuttle shuttle) async {
    final error = await ref.read(shuttlesProvider.notifier).reactivateShuttle(shuttle.id);
    if (error != null && mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error), backgroundColor: Colors.red.shade600),
      );
    }
  }

  /// Shows the Add/Edit shuttle dialog.
  void _showShuttleDialog(BuildContext context, {Shuttle? shuttle}) {
    final isEditing = shuttle != null;
    final plateController = TextEditingController(text: shuttle?.licensePlate ?? '');
    final capacityController = TextEditingController(
      text: shuttle != null ? shuttle.capacity.toString() : '',
    );
    int? selectedVendorId = shuttle?.vendorId;
    bool isAccessible = shuttle?.isAccessibleVehicle ?? false;
    final formKey = GlobalKey<FormState>();
    String? dialogError;

    showDialog(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            final vendorsState = ref.watch(vendorsProvider);

            return AlertDialog(
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              title: Text(
                isEditing ? 'Edit Shuttle' : 'Add Shuttle',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.bold,
                  color: _navy,
                ),
              ),
              content: SizedBox(
                width: 420,
                child: Form(
                  key: formKey,
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      if (dialogError != null)
                        Padding(
                          padding: const EdgeInsets.only(bottom: 12),
                          child: Container(
                            width: double.infinity,
                            padding: const EdgeInsets.all(10),
                            decoration: BoxDecoration(
                              color: Colors.red.shade50,
                              borderRadius: BorderRadius.circular(8),
                              border: Border.all(color: Colors.red.shade200),
                            ),
                            child: Text(
                              dialogError!,
                              style: GoogleFonts.hankenGrotesk(
                                color: Colors.red.shade700,
                                fontSize: 13,
                              ),
                            ),
                          ),
                        ),
                      TextFormField(
                        controller: plateController,
                        decoration: InputDecoration(
                          labelText: 'License Plate',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        style: GoogleFonts.hankenGrotesk(),
                        textCapitalization: TextCapitalization.characters,
                        inputFormatters: const [
                          MaxLengthFormatter(Validators.maxLicensePlateLength),
                        ],
                        validator: Validators.licensePlate,
                      ),
                      const SizedBox(height: 16),
                      // Capacity: slider + editable text field
                      Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Capacity',
                            style: GoogleFonts.hankenGrotesk(
                              fontSize: 14,
                              fontWeight: FontWeight.w500,
                              color: _navy,
                            ),
                          ),
                          const SizedBox(height: 8),
                          Row(
                            children: [
                              Expanded(
                                child: Slider(
                                  value: (int.tryParse(capacityController.text) ?? 0)
                                      .clamp(0, 80)
                                      .toDouble(),
                                  min: 0,
                                  max: 80,
                                  divisions: 80,
                                  activeColor: _navy,
                                  label: capacityController.text.isEmpty
                                      ? '0'
                                      : capacityController.text,
                                  onChanged: (val) {
                                    setDialogState(() {
                                      capacityController.text =
                                          val.round().toString();
                                    });
                                  },
                                ),
                              ),
                              const SizedBox(width: 12),
                              SizedBox(
                                width: 72,
                                child: TextFormField(
                                  controller: capacityController,
                                  style: GoogleFonts.hankenGrotesk(fontSize: 14),
                                  textAlign: TextAlign.center,
                                  keyboardType: TextInputType.number,
                                  maxLength: 2,
                                  inputFormatters: [
                                    FilteringTextInputFormatter.digitsOnly,
                                    const IntRangeFormatter(
                                      min: 0,
                                      max: Validators.maxShuttleCapacity,
                                    ),
                                  ],
                                  decoration: InputDecoration(
                                    border: OutlineInputBorder(
                                      borderRadius: BorderRadius.circular(8),
                                    ),
                                    contentPadding: const EdgeInsets.symmetric(
                                        horizontal: 12, vertical: 10),
                                    counterText: '',
                                  ),
                                  onChanged: (v) {
                                    setDialogState(() {});
                                  },
                                  validator: Validators.shuttleCapacity,
                                ),
                              ),
                            ],
                          ),
                        ],
                      ),
                      const SizedBox(height: 16),
                      // Vendor dropdown
                      DropdownButtonFormField<int>(
                        value: selectedVendorId,
                        decoration: InputDecoration(
                          labelText: 'Vendor',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                          prefixIcon: const Icon(Icons.business_outlined),
                        ),
                        style: GoogleFonts.hankenGrotesk(color: _navy, fontSize: 14),
                        items: vendorsState.vendors.map((v) {
                          return DropdownMenuItem<int>(
                            value: v.id,
                            child: Text(v.companyName),
                          );
                        }).toList(),
                        onChanged: (val) {
                          setDialogState(() => selectedVendorId = val);
                        },
                        validator: (v) => v == null ? 'Please select a vendor' : null,
                      ),
                      const SizedBox(height: 16),
                      // Accessible vehicle toggle
                      Row(
                        children: [
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text(
                                  'Accessible Vehicle',
                                  style: GoogleFonts.hankenGrotesk(fontSize: 14),
                                ),
                                const SizedBox(height: 2),
                                Text(
                                  'Wheelchair-accessible shuttle',
                                  style: GoogleFonts.hankenGrotesk(
                                    fontSize: 12,
                                    color: Colors.grey.shade600,
                                  ),
                                ),
                              ],
                            ),
                          ),
                          Switch(
                            value: isAccessible,
                            activeTrackColor: _navy,
                            onChanged: (val) {
                              setDialogState(() => isAccessible = val);
                            },
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.of(dialogContext).pop(),
                  child: Text(
                    'Cancel',
                    style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade600),
                  ),
                ),
                FilledButton(
                  onPressed: () async {
                    if (!formKey.currentState!.validate()) return;

                    String? error;
                    if (isEditing) {
                      error = await ref.read(shuttlesProvider.notifier).updateShuttle(
                            id: shuttle.id,
                            licensePlate: plateController.text.trim(),
                            capacity: int.parse(capacityController.text.trim()),
                            vendorId: selectedVendorId!,
                            isAccessibleVehicle: isAccessible,
                          );
                    } else {
                      error = await ref.read(shuttlesProvider.notifier).createShuttle(
                            licensePlate: plateController.text.trim(),
                            capacity: int.parse(capacityController.text.trim()),
                            vendorId: selectedVendorId!,
                            isAccessibleVehicle: isAccessible,
                          );
                    }

                    if (error != null) {
                      setDialogState(() => dialogError = error);
                    } else {
                      if (dialogContext.mounted) Navigator.of(dialogContext).pop();
                    }
                  },
                  child: Text(
                    isEditing ? 'Update' : 'Create',
                    style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
                  ),
                ),
              ],
            );
          },
        );
      },
    );
  }
}
