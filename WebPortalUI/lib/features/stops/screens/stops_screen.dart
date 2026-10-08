import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/stops/providers/stops_providers.dart';
import 'package:nmu_shuttle_admin/features/stops/widgets/map_location_picker.dart';

/// Full CRUD screen for managing shuttle stops.
class StopsScreen extends ConsumerStatefulWidget {
  const StopsScreen({super.key});

  @override
  ConsumerState<StopsScreen> createState() => _StopsScreenState();
}

class _StopsScreenState extends ConsumerState<StopsScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  void initState() {
    super.initState();
    // Fetch stops on first load.
    Future.microtask(() => ref.read(stopsProvider.notifier).fetchStops());
  }

  @override
  Widget build(BuildContext context) {
    final stopsState = ref.watch(stopsProvider);

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
                    'Shuttle Stops',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 28,
                      fontWeight: FontWeight.bold,
                      color: _navy,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Manage pickup and drop-off locations',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 14,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
              FilledButton.icon(
                onPressed: () => _showStopDialog(context),
                icon: const Icon(Icons.add),
                label: Text(
                  'Add Stop',
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
          if (stopsState.errorMessage != null)
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
                      stopsState.errorMessage!,
                      style: GoogleFonts.hankenGrotesk(color: Colors.red.shade700),
                    ),
                  ],
                ),
              ),
            ),

          // Data table
          Expanded(
            child: stopsState.isLoading
                ? const Center(child: CircularProgressIndicator())
                : Card(
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                      side: BorderSide(color: Colors.grey.shade200),
                    ),
                    child: stopsState.stops.isEmpty
                        ? Center(
                            child: Text(
                              'No stops found. Click "Add Stop" to create one.',
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
                                      'Stop Name',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'Full Address',
                                      style: GoogleFonts.hankenGrotesk(
                                        fontWeight: FontWeight.w700,
                                        color: _navy,
                                        fontSize: 13,
                                      ),
                                    ),
                                  ),
                                  DataColumn(
                                    label: Text(
                                      'GPS Coordinates',
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
                                rows: stopsState.stops.map((stop) {
                                  return DataRow(
                                    cells: [
                                      DataCell(Text(
                                        stop.stopName,
                                        style: GoogleFonts.hankenGrotesk(fontSize: 13),
                                      )),
                                      DataCell(Text(
                                        stop.fullAddress,
                                        style: GoogleFonts.hankenGrotesk(fontSize: 13),
                                      )),
                                      DataCell(Text(
                                        stop.gpsCoordinates,
                                        style: GoogleFonts.hankenGrotesk(fontSize: 13),
                                      )),
                                      DataCell(Row(
                                        mainAxisSize: MainAxisSize.min,
                                        children: [
                                          IconButton(
                                            icon: Icon(Icons.visibility_outlined, color: _navy, size: 20),
                                            tooltip: 'View Details',
                                            onPressed: () => context.go('/stops/${stop.id}'),
                                          ),
                                          IconButton(
                                            icon: Icon(Icons.edit_outlined, color: _navy, size: 20),
                                            tooltip: 'Edit',
                                            onPressed: () => _showStopDialog(context, stop: stop),
                                          ),
                                          IconButton(
                                            icon: Icon(Icons.delete_outline, color: Colors.red.shade600, size: 20),
                                            tooltip: 'Delete',
                                            onPressed: () => _confirmDelete(context, stop),
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

  /// Shows the Add/Edit stop dialog.
  void _showStopDialog(BuildContext context, {Stop? stop}) {
    final isEditing = stop != null;
    final nameController = TextEditingController(text: stop?.stopName ?? '');
    final streetController = TextEditingController(text: stop?.street ?? '');
    final suburbController = TextEditingController(text: stop?.suburb ?? '');
    final cityController = TextEditingController(text: stop?.city ?? '');
    final postalCodeController = TextEditingController(text: stop?.postalCode ?? '');
    // Split existing GPS coordinates into lat/lng
    final gpsParts = (stop?.gpsCoordinates ?? '').split(',');
    final latController = TextEditingController(
      text: gpsParts.isNotEmpty ? gpsParts[0].trim() : '',
    );
    final lngController = TextEditingController(
      text: gpsParts.length > 1 ? gpsParts[1].trim() : '',
    );
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
                isEditing ? 'Edit Stop' : 'Add Stop',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.bold,
                  color: _navy,
                ),
              ),
              content: SizedBox(
                width: 480,
                child: SingleChildScrollView(
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
                          controller: nameController,
                          decoration: InputDecoration(
                            labelText: 'Stop Name',
                            labelStyle: GoogleFonts.hankenGrotesk(),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          style: GoogleFonts.hankenGrotesk(),
                          validator: (v) => v == null || v.trim().isEmpty ? 'Stop name is required' : null,
                        ),
                        const SizedBox(height: 16),
                        // Street
                        TextFormField(
                          controller: streetController,
                          decoration: InputDecoration(
                            labelText: 'Street',
                            hintText: 'e.g. 12 Marine Drive',
                            labelStyle: GoogleFonts.hankenGrotesk(),
                            hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          style: GoogleFonts.hankenGrotesk(),
                          validator: (v) => v == null || v.trim().isEmpty ? 'Street is required' : null,
                        ),
                        const SizedBox(height: 16),
                        // Suburb
                        TextFormField(
                          controller: suburbController,
                          decoration: InputDecoration(
                            labelText: 'Suburb',
                            hintText: 'e.g. Summerstrand',
                            labelStyle: GoogleFonts.hankenGrotesk(),
                            hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          style: GoogleFonts.hankenGrotesk(),
                          validator: (v) => v == null || v.trim().isEmpty ? 'Suburb is required' : null,
                        ),
                        const SizedBox(height: 16),
                        // City
                        TextFormField(
                          controller: cityController,
                          decoration: InputDecoration(
                            labelText: 'City',
                            hintText: 'e.g. Gqeberha',
                            labelStyle: GoogleFonts.hankenGrotesk(),
                            hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          style: GoogleFonts.hankenGrotesk(),
                          validator: (v) => v == null || v.trim().isEmpty ? 'City is required' : null,
                        ),
                        const SizedBox(height: 16),
                        // Postal Code
                        TextFormField(
                          controller: postalCodeController,
                          decoration: InputDecoration(
                            labelText: 'Postal Code',
                            hintText: 'e.g. 6001',
                            helperText: 'Numbers only',
                            labelStyle: GoogleFonts.hankenGrotesk(),
                            hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                            helperStyle: GoogleFonts.hankenGrotesk(fontSize: 11, color: Colors.grey.shade500),
                            border: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(8),
                            ),
                          ),
                          style: GoogleFonts.hankenGrotesk(),
                          keyboardType: TextInputType.number,
                          inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                          validator: (v) => v == null || v.trim().isEmpty ? 'Postal code is required' : null,
                        ),
                        const SizedBox(height: 16),
                        // Latitude + Longitude
                        Row(
                          children: [
                            Expanded(
                              child: TextFormField(
                                controller: latController,
                                decoration: InputDecoration(
                                  labelText: 'Latitude',
                                  hintText: 'e.g. -33.9580',
                                  labelStyle: GoogleFonts.hankenGrotesk(),
                                  hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                                  border: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                ),
                                style: GoogleFonts.hankenGrotesk(),
                                keyboardType: const TextInputType.numberWithOptions(decimal: true, signed: true),
                                validator: (v) {
                                  if (v == null || v.trim().isEmpty) return 'Required';
                                  final n = double.tryParse(v.trim());
                                  if (n == null || n < -90 || n > 90) return 'Invalid (-90 to 90)';
                                  return null;
                                },
                              ),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: TextFormField(
                                controller: lngController,
                                decoration: InputDecoration(
                                  labelText: 'Longitude',
                                  hintText: 'e.g. 25.5940',
                                  labelStyle: GoogleFonts.hankenGrotesk(),
                                  hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                                  border: OutlineInputBorder(
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                ),
                                style: GoogleFonts.hankenGrotesk(),
                                keyboardType: const TextInputType.numberWithOptions(decimal: true, signed: true),
                                validator: (v) {
                                  if (v == null || v.trim().isEmpty) return 'Required';
                                  final n = double.tryParse(v.trim());
                                  if (n == null || n < -180 || n > 180) return 'Invalid (-180 to 180)';
                                  return null;
                                },
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        // Interactive map picker — writes the tapped point
                        // back into latController / lngController.
                        MapLocationPicker(
                          latController: latController,
                          lngController: lngController,
                        ),
                      ],
                    ),
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
                    final gpsCoordinates = '${latController.text.trim()},${lngController.text.trim()}';
                    if (isEditing) {
                      error = await ref.read(stopsProvider.notifier).updateStop(
                            id: stop.id,
                            stopName: nameController.text.trim(),
                            street: streetController.text.trim(),
                            suburb: suburbController.text.trim(),
                            city: cityController.text.trim(),
                            postalCode: postalCodeController.text.trim(),
                            gpsCoordinates: gpsCoordinates,
                          );
                    } else {
                      error = await ref.read(stopsProvider.notifier).createStop(
                            stopName: nameController.text.trim(),
                            street: streetController.text.trim(),
                            suburb: suburbController.text.trim(),
                            city: cityController.text.trim(),
                            postalCode: postalCodeController.text.trim(),
                            gpsCoordinates: gpsCoordinates,
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

  /// Shows a delete confirmation dialog.
  void _confirmDelete(BuildContext context, Stop stop) {
    showDialog(
      context: context,
      builder: (dialogContext) {
        return AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
          title: Text(
            'Delete Stop',
            style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.bold, color: _navy),
          ),
          content: Text(
            'Are you sure you want to delete "${stop.stopName}"? This action cannot be undone.',
            style: GoogleFonts.hankenGrotesk(),
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
                Navigator.of(dialogContext).pop();
                final error = await ref.read(stopsProvider.notifier).deleteStop(stop.id);
                if (error != null && mounted) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(
                      content: Text(error),
                      backgroundColor: Colors.red.shade600,
                    ),
                  );
                }
              },
              style: FilledButton.styleFrom(
                backgroundColor: Colors.red.shade600,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
              ),
              child: Text(
                'Delete',
                style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
              ),
            ),
          ],
        );
      },
    );
  }
}
