# Work Done — 5 August 2026

## UI/UX Redesign (Admin Portal)

- Discussed 3 UI options (A: refined current, B: two-column + top bar, C: navy sidebar)
- Created 3 HTML mockups for visual comparison
- Chose Option B and created a refined version with sticky/collapsible sidebar toggle
- Applied Nielsen's usability heuristics and Gestalt principles from UIUX study materials
- Implemented Option B in Flutter web portal:
  - Rewrote `main.dart` theme (navy primary, gold secondary, consistent radii, DataTable/Dialog/Input themes)
  - Rewrote `admin_scaffold.dart` — top bar (56px) + animated collapsible sidebar (68px ↔ 240px)
  - Sidebar: tooltips when collapsed, gold accent on active item, logout pinned at bottom
  - Top bar: toggle button, brand dot, breadcrumb, user avatar pill
- Fixed sidebar overflow error during animation (used animation-value-based rendering threshold + `Clip.hardEdge` on containers + `mainAxisSize.min` with `Flexible` text)

## Schedule/Calendar/Priority Split

- Separated the tabbed `ScheduleScreen` into 3 standalone screens:
  - `schedule_screen.dart` — Schedule Times only
  - `calendar_exceptions_screen.dart` — Calendar Exceptions
  - `priority_schedules_screen.dart` — Priority Schedules
- Each has its own sidebar nav item (no more redundant top tab navigation)
- Updated `admin_router.dart` with separate routes

## Shuttles Screen Fixes

- Removed DataTable row hover color (globally and locally)
- Replaced `SwitchListTile` with plain `Row` + `Switch` for accessible vehicle toggle (no hover)
- Changed capacity field to slider + editable number input

## Vendors Entity (Full Stack)

- Created `Vendor` model (VendorId, CompanyName, ContactEmail, ContactPhone, CreatedAt)
- Added `VendorId` FK to `Shuttle` model with navigation property
- Created EF Core migration (`AddVendorEntity`)
- Seeded 3 vendors: Bay Transit Corp, Coastal Movers, Metro Link SA
- Created `VendorsAdminController` with full CRUD (GET, POST, PATCH, DELETE with FK guard)
- Updated `ShuttlesAdminController` to accept/return VendorId, validate vendor exists
- Created Flutter `vendors_providers.dart` (model + state notifier)
- Created Flutter `vendors_screen.dart` (full CRUD admin screen)
- Replaced vendor free-text field in shuttle dialog with a dropdown from vendors API
- Added "Vendors" nav item to sidebar
- Fixed vendor API paths (removed double `/api/v1` prefix)

## Stop Address Split (Atomicity)

- Split `FullAddress` into 4 atomic fields: Street, Suburb, City, PostalCode
- Updated Stop model, DbContext config, seed data (all 8 stops)
- Created EF migration (`SplitStopAddress`)
- Updated `StopsAdminController` and DTOs to use the 4 fields
- Updated Flutter stops provider model and CRUD methods
- Rewrote stops dialog: each field on its own row, postal code is numeric-only with `FilteringTextInputFormatter.digitsOnly` and helper text
- Split GPS coordinates into separate Latitude/Longitude fields with range validation

## Admin Staff Number

- Added `StaffNumber` field to Admin model
- Updated DbContext, seed data (2 admins seeded with staff numbers)
- Updated `UsersAdminController` to return/accept admin staff number
- Created EF migration (`AddAdminStaffNumber`)
- Updated Flutter label to show "Staff Number" for Marshal/Admin roles

## User Active/Inactive Status

- Added `IsActive` field to Student and Admin models (default true)
- Updated DbContext configurations for both
- Updated `UserResponseDto` and `UpdateUserDto` with IsActive
- Updated controller: list/get/update all include IsActive for students and admins
- Created EF migration (`AddIsActiveToStudentAndAdmin`)
- Added Status column to users table (green Active / red Inactive badge)
- Added activate/deactivate toggle button (students and marshals only, not admins)
- Toggle shows confirmation dialog before applying

## Marshal Assignment Constraint Fix

- Relaxed `CK_Marshals_AssignedXOR` check constraint to allow both-null (unassigned marshals)
- Created EF migration (`RelaxMarshalAssignmentConstraint`)
- Fixes CSV import of marshals without pre-assigned campus/stop

## CSV Upload Improvements

- Added preview/confirmation screen before upload (parses CSV locally)
  - Shows file name, user count summary (students/marshals)
  - Scrollable table with Role, Identifier, Name, Email
  - Parse error warnings displayed before upload
  - "Confirm & Upload" or "Cancel" actions
- Fixed `BulkUploadResult.fromJson` to match backend response format (`created`/`skipped`)
- Fixed Navigator.pop targeting (uses `rootNavigator: true` to avoid popping the route)
- Created sample CSV file (`MySpecGuide/sample_users_upload.csv`)

## User Create/Update API Field Fixes

- Fixed add user: send `type` instead of `role`, `studentNumber`/`staffNumber` instead of `identifier`
- Fixed update user: send `studentNumber`/`staffNumber` based on role, removed generic `identifier`
- User role dropdown disabled when editing (not changeable)

## Users Table Fix

- Merged split-header/body DataTable into single DataTable (fixes column alignment)
- Removed LayoutBuilder wrapper

## Vendor Phone Number Formatter

- Auto-formats phone input in 3-3-4 pattern (e.g. 041-555-0101)
- Digits only, capped at 10 characters

## Miscellaneous

- Added favicon (SVG with navy background + gold person silhouette)
- Created `trackRecord/` folder for session documentation

## Credits Used

- ~244 
