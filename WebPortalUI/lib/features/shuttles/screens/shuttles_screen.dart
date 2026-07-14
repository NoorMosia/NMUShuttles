import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/shuttles/providers/shuttles_providers.dart';

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
    Future.microtask(() => ref.read(shuttlesProvider.notifier).fetchShuttles());
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
                                  (states) {
                                    if (states.contains(WidgetState.hovered)) {
                                      return Colors.grey.shade200;
                                    }
                                    return null;
                                  },
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
                                      DataCell(Row(
                                        mainAxisSize: MainAxisSize.min,
                                        children: [
                                          IconButton(
                                            icon: Icon(Icons.edit_outlined, color: _navy, size: 20),
                                            tooltip: 'Edit',
                                            onPressed: () => _showShuttleDialog(context, shuttle: shuttle),
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

  /// Shows the Add/Edit shuttle dialog.
  void _showShuttleDialog(BuildContext context, {Shuttle? shuttle}) {
    final isEditing = shuttle != null;
    final plateController = TextEditingController(text: shuttle?.licensePlate ?? '');
    final capacityController = TextEditingController(
      text: shuttle != null ? shuttle.capacity.toString() : '',
    );
    final vendorController = TextEditingController(text: shuttle?.vendorCompanyName ?? '');
    bool isAccessible = shuttle?.isAccessibleVehicle ?? false;
    final formKey = GlobalKey<FormState>();
    String? dialogError;

    showDialog(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
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
                        validator: (v) =>
                            v == null || v.trim().isEmpty ? 'License plate is required' : null,
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: capacityController,
                        decoration: InputDecoration(
                          labelText: 'Capacity',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        style: GoogleFonts.hankenGrotesk(),
                        keyboardType: TextInputType.number,
                        inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                        validator: (v) {
                          if (v == null || v.trim().isEmpty) return 'Capacity is required';
                          final n = int.tryParse(v.trim());
                          if (n == null || n <= 0) return 'Capacity must be greater than 0';
                          return null;
                        },
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: vendorController,
                        decoration: InputDecoration(
                          labelText: 'Vendor Company Name',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(8),
                          ),
                        ),
                        style: GoogleFonts.hankenGrotesk(),
                        validator: (v) =>
                            v == null || v.trim().isEmpty ? 'Vendor name is required' : null,
                      ),
                      const SizedBox(height: 16),
                      SwitchListTile(
                        contentPadding: EdgeInsets.zero,
                        title: Text(
                          'Accessible Vehicle',
                          style: GoogleFonts.hankenGrotesk(fontSize: 14),
                        ),
                        subtitle: Text(
                          'Wheelchair-accessible shuttle',
                          style: GoogleFonts.hankenGrotesk(
                            fontSize: 12,
                            color: Colors.grey.shade600,
                          ),
                        ),
                        value: isAccessible,
                        activeTrackColor: _navy,
                        onChanged: (val) {
                          setDialogState(() => isAccessible = val);
                        },
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
                            vendorCompanyName: vendorController.text.trim(),
                            isAccessibleVehicle: isAccessible,
                          );
                    } else {
                      error = await ref.read(shuttlesProvider.notifier).createShuttle(
                            licensePlate: plateController.text.trim(),
                            capacity: int.parse(capacityController.text.trim()),
                            vendorCompanyName: vendorController.text.trim(),
                            isAccessibleVehicle: isAccessible,
                          );
                    }

                    if (error != null) {
                      setDialogState(() => dialogError = error);
                    } else {
                      if (dialogContext.mounted) Navigator.of(dialogContext).pop();
                    }
                  },
                  style: FilledButton.styleFrom(
                    backgroundColor: _navy,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                  ),
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
