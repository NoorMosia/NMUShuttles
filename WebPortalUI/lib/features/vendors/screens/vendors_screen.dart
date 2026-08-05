import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/vendors/providers/vendors_providers.dart';

/// Admin screen for managing vendor companies.
class VendorsScreen extends ConsumerStatefulWidget {
  const VendorsScreen({super.key});

  @override
  ConsumerState<VendorsScreen> createState() => _VendorsScreenState();
}

class _VendorsScreenState extends ConsumerState<VendorsScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(vendorsProvider.notifier).fetchVendors());
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(vendorsProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FB),
      body: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Vendors',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 22,
                        fontWeight: FontWeight.w700,
                        color: _navy,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Manage shuttle vendor companies and contact details',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 14,
                        color: const Color(0xFF5A6B7B),
                      ),
                    ),
                  ],
                ),
                FilledButton.icon(
                  onPressed: () => _showVendorDialog(context),
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Add Vendor'),
                ),
              ],
            ),
            const SizedBox(height: 24),

            // Error
            if (state.errorMessage != null)
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
                      Icon(Icons.error_outline,
                          color: Colors.red.shade600, size: 20),
                      const SizedBox(width: 8),
                      Expanded(
                        child: Text(
                          state.errorMessage!,
                          style: GoogleFonts.hankenGrotesk(
                              color: Colors.red.shade700),
                        ),
                      ),
                    ],
                  ),
                ),
              ),

            // Table
            Expanded(
              child: state.isLoading
                  ? const Center(
                      child: CircularProgressIndicator(strokeWidth: 3))
                  : state.vendors.isEmpty
                      ? Center(
                          child: Text(
                            'No vendors found. Click "Add Vendor" to create one.',
                            style: GoogleFonts.hankenGrotesk(
                              color: const Color(0xFF5A6B7B),
                              fontSize: 14,
                            ),
                          ),
                        )
                      : _buildTable(context, state.vendors),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildTable(BuildContext context, List<Vendor> vendors) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFE8ECF0)),
      ),
      child: SizedBox(
        width: double.infinity,
        child: SingleChildScrollView(
          child: DataTable(
            columnSpacing: 32,
            columns: const [
              DataColumn(label: Text('COMPANY NAME')),
              DataColumn(label: Text('EMAIL')),
              DataColumn(label: Text('PHONE')),
              DataColumn(label: Text('SHUTTLES'), numeric: true),
              DataColumn(label: Text('ACTIONS')),
            ],
            rows: vendors.map((vendor) {
              return DataRow(cells: [
                DataCell(Text(
                  vendor.companyName,
                  style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w500),
                )),
                DataCell(Text(vendor.contactEmail ?? '-')),
                DataCell(Text(vendor.contactPhone ?? '-')),
                DataCell(Text('${vendor.shuttleCount}')),
                DataCell(Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      icon: Icon(Icons.edit_outlined,
                          size: 18, color: Colors.blue.shade700),
                      tooltip: 'Edit',
                      onPressed: () =>
                          _showVendorDialog(context, vendor: vendor),
                    ),
                    IconButton(
                      icon: Icon(Icons.delete_outlined,
                          size: 18, color: Colors.red.shade700),
                      tooltip: 'Delete',
                      onPressed: () => _confirmDelete(context, vendor),
                    ),
                  ],
                )),
              ]);
            }).toList(),
          ),
        ),
      ),
    );
  }

  void _showVendorDialog(BuildContext context, {Vendor? vendor}) {
    final isEditing = vendor != null;
    final nameController =
        TextEditingController(text: vendor?.companyName ?? '');
    final emailController =
        TextEditingController(text: vendor?.contactEmail ?? '');
    final phoneController =
        TextEditingController(text: vendor?.contactPhone ?? '');
    final formKey = GlobalKey<FormState>();
    String? dialogError;

    showDialog(
      context: context,
      builder: (dialogContext) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: Text(
                isEditing ? 'Edit Vendor' : 'Add Vendor',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
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
                        controller: nameController,
                        decoration: InputDecoration(
                          labelText: 'Company Name',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                          prefixIcon: const Icon(Icons.business_outlined),
                        ),
                        style: GoogleFonts.hankenGrotesk(),
                        validator: (v) => v == null || v.trim().isEmpty
                            ? 'Company name is required'
                            : null,
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: emailController,
                        decoration: InputDecoration(
                          labelText: 'Contact Email (optional)',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                          prefixIcon: const Icon(Icons.email_outlined),
                        ),
                        style: GoogleFonts.hankenGrotesk(),
                        keyboardType: TextInputType.emailAddress,
                      ),
                      const SizedBox(height: 16),
                      TextFormField(
                        controller: phoneController,
                        decoration: InputDecoration(
                          labelText: 'Contact Phone (optional)',
                          hintText: '041-555-0101',
                          labelStyle: GoogleFonts.hankenGrotesk(),
                          hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
                          border: OutlineInputBorder(
                            borderRadius: BorderRadius.circular(10),
                          ),
                          prefixIcon: const Icon(Icons.phone_outlined),
                        ),
                        style: GoogleFonts.hankenGrotesk(),
                        keyboardType: TextInputType.phone,
                        inputFormatters: [
                          FilteringTextInputFormatter.digitsOnly,
                          _PhoneNumberFormatter(),
                        ],
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.of(dialogContext).pop(),
                  child: Text('Cancel', style: GoogleFonts.hankenGrotesk()),
                ),
                FilledButton(
                  onPressed: () async {
                    if (!formKey.currentState!.validate()) return;

                    String? error;
                    if (isEditing) {
                      error =
                          await ref.read(vendorsProvider.notifier).updateVendor(
                                id: vendor.id,
                                companyName: nameController.text.trim(),
                                contactEmail: emailController.text.trim().isEmpty
                                    ? null
                                    : emailController.text.trim(),
                                contactPhone: phoneController.text.trim().isEmpty
                                    ? null
                                    : phoneController.text.trim(),
                              );
                    } else {
                      error =
                          await ref.read(vendorsProvider.notifier).createVendor(
                                companyName: nameController.text.trim(),
                                contactEmail: emailController.text.trim().isEmpty
                                    ? null
                                    : emailController.text.trim(),
                                contactPhone: phoneController.text.trim().isEmpty
                                    ? null
                                    : phoneController.text.trim(),
                              );
                    }

                    if (error != null) {
                      setDialogState(() => dialogError = error);
                    } else {
                      if (dialogContext.mounted) {
                        Navigator.of(dialogContext).pop();
                      }
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

  void _confirmDelete(BuildContext context, Vendor vendor) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(
          'Delete Vendor',
          style: GoogleFonts.hankenGrotesk(
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
        content: Text(
          'Are you sure you want to delete "${vendor.companyName}"?\n\n'
          'This will fail if any shuttles are still assigned to this vendor.',
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
              final error =
                  await ref.read(vendorsProvider.notifier).deleteVendor(vendor.id);
              if (error != null && mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  SnackBar(
                    content: Text(error),
                    backgroundColor: Colors.red.shade700,
                  ),
                );
              }
            },
            style: FilledButton.styleFrom(
              backgroundColor: Colors.red.shade700,
            ),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
  }
}


// ─── Phone Number Formatter (3-3-4) ─────────────────────────────────────────

/// Automatically inserts dashes in a 3-3-4 pattern (e.g. 041-555-0101).
class _PhoneNumberFormatter extends TextInputFormatter {
  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    final digits = newValue.text.replaceAll(RegExp(r'[^0-9]'), '');

    // Limit to 10 digits
    final trimmed = digits.length > 10 ? digits.substring(0, 10) : digits;

    final buffer = StringBuffer();
    for (int i = 0; i < trimmed.length; i++) {
      if (i == 3 || i == 6) buffer.write('-');
      buffer.write(trimmed[i]);
    }

    final formatted = buffer.toString();
    return TextEditingValue(
      text: formatted,
      selection: TextSelection.collapsed(offset: formatted.length),
    );
  }
}
