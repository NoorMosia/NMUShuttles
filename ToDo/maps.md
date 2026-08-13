# Maps Integration — Admin Stop Lat/Long Picker

## Backend (minimal changes)

- Add `Latitude` (double, nullable) and `Longitude` (double, nullable) columns to the `Stop` entity if not already present.
- Create an EF Core migration for the new columns.
- Update the Admin Stop DTOs (create/update request + response) to include `Latitude` and `Longitude`.
- Update the Admin Stops controller to accept and return the new fields.

## Flutter Web Portal (main effort)

### 1. Map widget on Add/Edit Stop form

- Add `flutter_map` and `latlong2` packages to `WebPortalUI/pubspec.yaml`.
- Embed a `FlutterMap` widget in the stop creation/edit form.
- Center the map on Gqeberha by default (~-33.96, 25.60).

### 2. Interaction model

- Listen to `onTap` on the map to capture a `LatLng` from the admin's click.
- Place a draggable marker at the tapped position.
- Optionally: add a geocoding search box (Nominatim or Google Geocoding API) to let the admin search by address and auto-center the map.

### 3. Display selected coordinates

- Show the selected lat/long in read-only text fields below the map so the admin can confirm before submitting.
- Submit lat/long as part of the existing stop create/update form payload.

## Dependencies

| Package | Purpose |
|---------|---------|
| `flutter_map` | Map widget for Flutter Web |
| `latlong2` | LatLng model (required by flutter_map) |
| Optional: `http` or geocoding package | Address search / geocoding |

No new backend NuGet packages needed.

## Key Decision

**`flutter_map` + OpenStreetMap** vs **Google Maps**:

- `flutter_map` + OSM: Free, no API key, no billing. Good enough for pin-drop location picking.
- Google Maps (`google_maps_flutter_web`): Needed if you want satellite imagery, Google Places autocomplete, or Street View. Requires a Google Maps API key + billing account.

Recommendation: Start with `flutter_map` + OSM. Upgrade to Google Maps later only if richer features are needed.

## Effort Estimate

| Layer | Estimated Time |
|-------|----------------|
| Backend (columns, migration, DTOs) | ~30 minutes |
| Web Portal UI (map widget, picker, form wiring) | ~2–4 hours |
| Optional geocoding search | +1–2 hours |
