import 'package:file_picker/file_picker.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/users/providers/users_providers.dart';

// ─── Constants ───────────────────────────────────────────────────────────────

const _navy = Color(0xFF132E51);
const _gold = Color(0xFFFFCC00);

// ─── Validation ──────────────────────────────────────────────────────────────

final _studentNumberRegex = RegExp(r'^s3[12]\d{7}$');
final _staffNumberRegex = RegExp(r'^\d{7}$');

String? _validateIdentifier(String? value, String role) {
  if (value == null || value.trim().isEmpty) {
    return 'Identifier is required';
  }
  final v = value.trim();
  if (role == 'Student') {
    if (!_studentNumberRegex.hasMatch(v)) {
      return 'Student number must match s3[1|2]XXXXXXX (e.g. s312345678)';
    }
  } else {
    if (!_staffNumberRegex.hasMatch(v)) {
      return 'Staff number must be 7 digits';
    }
  }
  return null;
}

// ─── Main Screen ─────────────────────────────────────────────────────────────

class UsersScreen extends ConsumerStatefulWidget {
  const UsersScreen({super.key});

  @override
  ConsumerState<UsersScreen> createState() => _UsersScreenState();
}

class _UsersScreenState extends ConsumerState<UsersScreen> {
  final _searchController = TextEditingController();
  String _selectedRole = 'All';
  String _searchQuery = '';

  @override
  void initState() {
    super.initState();
    // Fetch users on first load.
    Future.microtask(() {
      ref.read(usersProvider.notifier).fetchUsers();
    });
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  List<UserModel> _filteredUsers(List<UserModel> users) {
    var filtered = users;

    // Role filter
    if (_selectedRole != 'All') {
      final roleMap = {
        'Students': 'Student',
        'Marshals': 'Marshal',
        'Admins': 'Admin',
      };
      final targetRole = roleMap[_selectedRole] ?? _selectedRole;
      filtered = filtered.where((u) => u.role == targetRole).toList();
    }

    // Search filter
    if (_searchQuery.isNotEmpty) {
      final q = _searchQuery.toLowerCase();
      filtered = filtered.where((u) {
        return u.fullName.toLowerCase().contains(q) ||
            u.identifier.toLowerCase().contains(q) ||
            u.email.toLowerCase().contains(q);
      }).toList();
    }

    return filtered;
  }

  @override
  Widget build(BuildContext context) {
    final usersState = ref.watch(usersProvider);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          Text(
            'Users',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 28,
              fontWeight: FontWeight.bold,
              color: _navy,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            'Manage students, marshals, and admins',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 14,
              color: Colors.grey.shade600,
            ),
          ),
          const SizedBox(height: 24),

          // Action buttons row
          _buildActionBar(),
          const SizedBox(height: 16),

          // Role filter + search
          _buildFiltersRow(),
          const SizedBox(height: 16),

          // Content
          Expanded(
            child: _buildContent(usersState),
          ),
        ],
      ),
    );
  }

  Widget _buildActionBar() {
    return Row(
      children: [
        FilledButton.icon(
          onPressed: () => _showAddUserDialog(),
          icon: const Icon(Icons.person_add_outlined, size: 18),
          label: Text(
            'Add User',
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
        const SizedBox(width: 12),
        OutlinedButton.icon(
          onPressed: () => _handleCsvUpload(),
          icon: const Icon(Icons.upload_file_outlined, size: 18),
          label: Text(
            'Upload CSV',
            style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
          ),
          style: OutlinedButton.styleFrom(
            foregroundColor: _navy,
            side: const BorderSide(color: _navy),
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 14),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildFiltersRow() {
    final roles = ['All', 'Students', 'Marshals', 'Admins'];

    return Row(
      children: [
        // Role filter chips
        ...roles.map((role) {
          final isSelected = _selectedRole == role;
          return Padding(
            padding: const EdgeInsets.only(right: 8),
            child: FilterChip(
              label: Text(
                role,
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w500,
                  color: isSelected ? Colors.white : _navy,
                ),
              ),
              selected: isSelected,
              onSelected: (_) {
                setState(() => _selectedRole = role);
              },
              selectedColor: _navy,
              backgroundColor: Colors.grey.shade100,
              checkmarkColor: _gold,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
              side: BorderSide(
                color: isSelected ? _navy : Colors.grey.shade300,
              ),
            ),
          );
        }),
        const Spacer(),
        // Search field
        SizedBox(
          width: 300,
          child: TextField(
            controller: _searchController,
            onChanged: (value) {
              setState(() => _searchQuery = value);
            },
            style: GoogleFonts.hankenGrotesk(),
            decoration: InputDecoration(
              hintText: 'Search by name, identifier, or email...',
              hintStyle: GoogleFonts.hankenGrotesk(
                color: Colors.grey.shade500,
              ),
              prefixIcon: const Icon(Icons.search, color: _navy),
              suffixIcon: _searchQuery.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.clear, size: 18),
                      onPressed: () {
                        _searchController.clear();
                        setState(() => _searchQuery = '');
                      },
                    )
                  : null,
              filled: true,
              fillColor: Colors.grey.shade50,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(8),
                borderSide: BorderSide(color: Colors.grey.shade300),
              ),
              enabledBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(8),
                borderSide: BorderSide(color: Colors.grey.shade300),
              ),
              focusedBorder: OutlineInputBorder(
                borderRadius: BorderRadius.circular(8),
                borderSide: const BorderSide(color: _navy, width: 1.5),
              ),
              contentPadding: const EdgeInsets.symmetric(
                horizontal: 16,
                vertical: 12,
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildContent(UsersState usersState) {
    if (usersState.status == UsersStatus.loading) {
      return const Center(child: CircularProgressIndicator(color: _navy));
    }

    if (usersState.status == UsersStatus.error) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, size: 48, color: Colors.red.shade300),
            const SizedBox(height: 12),
            Text(
              usersState.errorMessage ?? 'An error occurred',
              style: GoogleFonts.hankenGrotesk(color: Colors.red.shade700),
            ),
            const SizedBox(height: 12),
            FilledButton(
              onPressed: () => ref.read(usersProvider.notifier).fetchUsers(),
              style: FilledButton.styleFrom(backgroundColor: _navy),
              child: const Text('Retry'),
            ),
          ],
        ),
      );
    }

    final filtered = _filteredUsers(usersState.users);

    if (filtered.isEmpty && usersState.users.isNotEmpty) {
      return Center(
        child: Text(
          'No users match your filter.',
          style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade600),
        ),
      );
    }

    if (filtered.isEmpty) {
      return Center(
        child: Text(
          'No users found. Add users or upload a CSV.',
          style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade600),
        ),
      );
    }

    return _buildUsersTable(filtered);
  }

  Widget _buildUsersTable(List<UserModel> users) {
    return LayoutBuilder(
      builder: (context, constraints) {
        return Container(
          width: constraints.maxWidth,
          decoration: BoxDecoration(
            color: Colors.white,
            borderRadius: BorderRadius.circular(12),
            border: Border.all(color: Colors.grey.shade200),
          ),
          child: ClipRRect(
            borderRadius: BorderRadius.circular(12),
            child: Column(
              children: [
                // Sticky header
                Container(
                  width: constraints.maxWidth,
                  color: Colors.grey.shade50,
                  child: DataTable(
                    headingRowColor: WidgetStateProperty.all(Colors.grey.shade50),
                    headingTextStyle: GoogleFonts.hankenGrotesk(
                      fontWeight: FontWeight.w700,
                      color: _navy,
                      fontSize: 13,
                    ),
                    dataTextStyle: GoogleFonts.hankenGrotesk(fontSize: 13),
                    columnSpacing: 32,
                    horizontalMargin: 20,
                    headingRowHeight: 48,
                    columns: const [
                      DataColumn(label: Text('Identifier')),
                      DataColumn(label: Text('Name')),
                      DataColumn(label: Text('Role')),
                      DataColumn(label: Text('Email')),
                      DataColumn(label: Text('Actions')),
                    ],
                    rows: const [],
                  ),
                ),
                // Scrollable body
                Expanded(
                  child: SingleChildScrollView(
                    child: SizedBox(
                      width: constraints.maxWidth,
                      child: DataTable(
                        headingRowHeight: 0,
                        headingTextStyle: GoogleFonts.hankenGrotesk(
                          fontWeight: FontWeight.w700,
                          color: _navy,
                          fontSize: 13,
                        ),
                        dataTextStyle: GoogleFonts.hankenGrotesk(fontSize: 13),
                        dataRowColor: WidgetStateProperty.resolveWith<Color?>(
                          (states) {
                            if (states.contains(WidgetState.hovered)) {
                              return Colors.grey.shade200;
                            }
                            return null;
                          },
                        ),
                        columnSpacing: 32,
                        horizontalMargin: 20,
                        columns: const [
                          DataColumn(label: SizedBox.shrink()),
                          DataColumn(label: SizedBox.shrink()),
                          DataColumn(label: SizedBox.shrink()),
                          DataColumn(label: SizedBox.shrink()),
                          DataColumn(label: SizedBox.shrink()),
                        ],
                        rows: users.map((user) {
                          return DataRow(
                            cells: [
                              DataCell(Text(user.identifier)),
                              DataCell(Text(user.fullName)),
                              DataCell(Text(user.role)),
                              DataCell(Text(user.email)),
                              DataCell(
                                IconButton(
                                  icon: const Icon(Icons.edit_outlined, size: 18),
                                  color: _navy,
                                  tooltip: 'Edit user',
                                  onPressed: () => _showEditUserDialog(user),
                                ),
                              ),
                            ],
                          );
                        }).toList(),
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  // ─── Add User Dialog ─────────────────────────────────────────────────────

  void _showAddUserDialog() {
    showDialog(
      context: context,
      builder: (ctx) => _UserFormDialog(
        title: 'Add User',
        onSubmit: (role, identifier, firstName, lastName, email) async {
          final success = await ref.read(usersProvider.notifier).addUser(
                role: role,
                identifier: identifier,
                firstName: firstName,
                lastName: lastName,
                email: email,
              );
          return success;
        },
      ),
    );
  }

  // ─── Edit User Dialog ────────────────────────────────────────────────────

  void _showEditUserDialog(UserModel user) {
    showDialog(
      context: context,
      builder: (ctx) => _UserFormDialog(
        title: 'Edit User',
        initialRole: user.role,
        initialIdentifier: user.identifier,
        initialFirstName: user.firstName,
        initialLastName: user.lastName,
        initialEmail: user.email,
        onSubmit: (role, identifier, firstName, lastName, email) async {
          final success = await ref.read(usersProvider.notifier).updateUser(
                id: user.id,
                role: role,
                identifier: identifier,
                firstName: firstName,
                lastName: lastName,
                email: email,
              );
          return success;
        },
      ),
    );
  }

  // ─── CSV Upload ──────────────────────────────────────────────────────────

  Future<void> _handleCsvUpload() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.custom,
      allowedExtensions: ['csv'],
      withData: true,
    );

    if (result == null || result.files.isEmpty) return;

    final file = result.files.first;
    if (file.bytes == null) return;

    if (!mounted) return;

    // Show loading dialog
    showDialog(
      context: context,
      barrierDismissible: false,
      builder: (_) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        content: Row(
          children: [
            const CircularProgressIndicator(color: _navy),
            const SizedBox(width: 20),
            Text(
              'Uploading ${file.name}...',
              style: GoogleFonts.hankenGrotesk(),
            ),
          ],
        ),
      ),
    );

    final uploadResult = await ref
        .read(usersProvider.notifier)
        .uploadCsv(file.bytes!, file.name);

    if (!mounted) return;
    Navigator.of(context).pop(); // Close loading dialog

    if (uploadResult != null) {
      _showUploadResultDialog(uploadResult);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'CSV upload failed. Please check the file format and try again.',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.red.shade700,
        ),
      );
    }
  }

  void _showUploadResultDialog(BulkUploadResult result) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
        title: Row(
          children: [
            Icon(Icons.check_circle, color: Colors.green.shade600, size: 24),
            const SizedBox(width: 8),
            Text(
              'Upload Complete',
              style: GoogleFonts.hankenGrotesk(
                fontWeight: FontWeight.bold,
                color: _navy,
              ),
            ),
          ],
        ),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              '${result.successCount} user(s) added successfully.',
              style: GoogleFonts.hankenGrotesk(fontSize: 14),
            ),
            if (result.skippedRows.isNotEmpty) ...[
              const SizedBox(height: 12),
              Text(
                '${result.skippedRows.length} row(s) skipped:',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 13,
                  fontWeight: FontWeight.w600,
                  color: Colors.orange.shade700,
                ),
              ),
              const SizedBox(height: 8),
              Container(
                constraints: const BoxConstraints(maxHeight: 150),
                child: SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: result.skippedRows
                        .map((row) => Padding(
                              padding: const EdgeInsets.only(bottom: 4),
                              child: Text(
                                '• $row',
                                style: GoogleFonts.hankenGrotesk(
                                  fontSize: 12,
                                  color: Colors.grey.shade700,
                                ),
                              ),
                            ))
                        .toList(),
                  ),
                ),
              ),
            ],
          ],
        ),
        actions: [
          FilledButton(
            onPressed: () => Navigator.of(ctx).pop(),
            style: FilledButton.styleFrom(
              backgroundColor: _navy,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
            child: Text(
              'Done',
              style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
            ),
          ),
        ],
      ),
    );
  }
}

// ─── User Form Dialog ────────────────────────────────────────────────────────

class _UserFormDialog extends StatefulWidget {
  const _UserFormDialog({
    required this.title,
    required this.onSubmit,
    this.initialRole,
    this.initialIdentifier,
    this.initialFirstName,
    this.initialLastName,
    this.initialEmail,
  });

  final String title;
  final Future<bool> Function(
    String role,
    String identifier,
    String firstName,
    String lastName,
    String email,
  ) onSubmit;
  final String? initialRole;
  final String? initialIdentifier;
  final String? initialFirstName;
  final String? initialLastName;
  final String? initialEmail;

  @override
  State<_UserFormDialog> createState() => _UserFormDialogState();
}

class _UserFormDialogState extends State<_UserFormDialog> {
  final _formKey = GlobalKey<FormState>();
  late String _role;
  late final TextEditingController _identifierCtrl;
  late final TextEditingController _firstNameCtrl;
  late final TextEditingController _lastNameCtrl;
  late final TextEditingController _emailCtrl;
  bool _submitting = false;

  @override
  void initState() {
    super.initState();
    const validRoles = ['Student', 'Marshal', 'Admin'];
    final initial = widget.initialRole ?? 'Student';
    _role = validRoles.contains(initial) ? initial : 'Student';
    _identifierCtrl =
        TextEditingController(text: widget.initialIdentifier ?? '');
    _firstNameCtrl =
        TextEditingController(text: widget.initialFirstName ?? '');
    _lastNameCtrl = TextEditingController(text: widget.initialLastName ?? '');
    _emailCtrl = TextEditingController(text: widget.initialEmail ?? '');
  }

  @override
  void dispose() {
    _identifierCtrl.dispose();
    _firstNameCtrl.dispose();
    _lastNameCtrl.dispose();
    _emailCtrl.dispose();
    super.dispose();
  }

  Future<void> _handleSubmit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _submitting = true);

    final success = await widget.onSubmit(
      _role,
      _identifierCtrl.text.trim(),
      _firstNameCtrl.text.trim(),
      _lastNameCtrl.text.trim(),
      _emailCtrl.text.trim(),
    );

    if (!mounted) return;
    setState(() => _submitting = false);

    if (success) {
      Navigator.of(context).pop();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            '${widget.title == 'Add User' ? 'User added' : 'User updated'} successfully.',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.green.shade700,
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Operation failed. Please try again.',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.red.shade700,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      title: Text(
        widget.title,
        style: GoogleFonts.hankenGrotesk(
          fontWeight: FontWeight.bold,
          color: _navy,
        ),
      ),
      content: SizedBox(
        width: 420,
        child: Form(
          key: _formKey,
          child: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                // Role dropdown
                DropdownButtonFormField<String>(
                  value: _role,
                  decoration: _inputDecoration('Role'),
                  style: GoogleFonts.hankenGrotesk(
                    color: Colors.black87,
                    fontSize: 14,
                  ),
                  items: ['Student', 'Marshal', 'Admin']
                      .map((r) => DropdownMenuItem(
                            value: r,
                            child: Text(r),
                          ))
                      .toList(),
                  onChanged: (value) {
                    if (value != null) {
                      setState(() => _role = value);
                      _formKey.currentState?.validate();
                    }
                  },
                ),
                const SizedBox(height: 16),

                // Identifier
                TextFormField(
                  controller: _identifierCtrl,
                  style: GoogleFonts.hankenGrotesk(fontSize: 14),
                  decoration: _inputDecoration(
                    'Identifier',
                    hint: _role == 'Student'
                        ? 'e.g. s312345678'
                        : 'e.g. 1234567',
                  ),
                  validator: (v) => _validateIdentifier(v, _role),
                ),
                const SizedBox(height: 16),

                // First Name
                TextFormField(
                  controller: _firstNameCtrl,
                  style: GoogleFonts.hankenGrotesk(fontSize: 14),
                  decoration: _inputDecoration('First Name'),
                  validator: (v) => (v == null || v.trim().isEmpty)
                      ? 'First name is required'
                      : null,
                ),
                const SizedBox(height: 16),

                // Last Name
                TextFormField(
                  controller: _lastNameCtrl,
                  style: GoogleFonts.hankenGrotesk(fontSize: 14),
                  decoration: _inputDecoration('Last Name'),
                  validator: (v) => (v == null || v.trim().isEmpty)
                      ? 'Last name is required'
                      : null,
                ),
                const SizedBox(height: 16),

                // Email
                TextFormField(
                  controller: _emailCtrl,
                  style: GoogleFonts.hankenGrotesk(fontSize: 14),
                  decoration: _inputDecoration('Email'),
                  keyboardType: TextInputType.emailAddress,
                  validator: (v) {
                    if (v == null || v.trim().isEmpty) {
                      return 'Email is required';
                    }
                    if (!v.contains('@') || !v.contains('.')) {
                      return 'Enter a valid email address';
                    }
                    return null;
                  },
                ),
              ],
            ),
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: _submitting ? null : () => Navigator.of(context).pop(),
          child: Text(
            'Cancel',
            style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade700),
          ),
        ),
        FilledButton(
          onPressed: _submitting ? null : _handleSubmit,
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
          child: _submitting
              ? const SizedBox(
                  width: 18,
                  height: 18,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: Colors.white,
                  ),
                )
              : Text(
                  widget.title == 'Add User' ? 'Add' : 'Save',
                  style:
                      GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
                ),
        ),
      ],
    );
  }

  InputDecoration _inputDecoration(String label, {String? hint}) {
    return InputDecoration(
      labelText: label,
      hintText: hint,
      labelStyle: GoogleFonts.hankenGrotesk(color: _navy),
      hintStyle: GoogleFonts.hankenGrotesk(color: Colors.grey.shade400),
      filled: true,
      fillColor: Colors.grey.shade50,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: Colors.grey.shade300),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: Colors.grey.shade300),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: const BorderSide(color: _navy, width: 1.5),
      ),
      errorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: Colors.red.shade400),
      ),
      focusedErrorBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(8),
        borderSide: BorderSide(color: Colors.red.shade400, width: 1.5),
      ),
      contentPadding: const EdgeInsets.symmetric(
        horizontal: 16,
        vertical: 14,
      ),
    );
  }
}
