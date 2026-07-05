# Seed Data — NMU Shuttle Management Network

This file contains the initial seed data for development and testing. All values are fictitious.

---

## Admins

| AdminId | Email | Password (plain, for dev only) | FirstName | LastName |
|---|---|---|---|---|
| 1 | admin@mandela.ac.za | Admin@2025! | Thabo | Mokoena |
| 2 | transport@mandela.ac.za | Transport@2025! | Nomsa | Dlamini |

---

## Campuses

| CampusId | CampusName | GpsCoordinates |
|---|---|---|
| 1 | South Campus | -33.9580,25.5920 |
| 2 | North Campus | -33.9185,25.5870 |
| 3 | 2nd Avenue Campus | -33.9610,25.6100 |

---

## Stops (Off-Campus Residential)

| StopId | StopName | FullAddress | GpsCoordinates | IsActive |
|---|---|---|---|---|
| 1 | Summerstrand Village | 12 Marine Drive, Summerstrand, Gqeberha, 6001 | -33.9680,25.6450 | TRUE |
| 2 | Humewood | 45 Beach Road, Humewood, Gqeberha, 6001 | -33.9630,25.6280 | TRUE |
| 3 | Newton Park | 78 Cape Road, Newton Park, Gqeberha, 6045 | -33.9480,25.5750 | TRUE |
| 4 | Walmer Heights | 23 Heugh Road, Walmer Heights, Gqeberha, 6070 | -33.9850,25.6100 | TRUE |
| 5 | Lorraine | 91 Circular Drive, Lorraine, Gqeberha, 6070 | -33.9320,25.5620 | TRUE |
| 6 | Bluewater Bay | 5 Seaview Crescent, Bluewater Bay, Gqeberha, 6210 | -33.8710,25.5590 | TRUE |
| 7 | Framesby | 14 Langenhoven Drive, Framesby, Gqeberha, 6045 | -33.9260,25.5810 | TRUE |
| 8 | Greenacres | 30 Ring Road, Greenacres, Gqeberha, 6045 | -33.9410,25.5870 | TRUE |

---

## Students

| StudentId | StudentNumber | FirstName | LastName | UniversityEmail | IsPriorityUser |
|---|---|---|---|---|---|
| 1 | s312847563 | Sarah | Nkosi | s312847563@mandela.ac.za | FALSE |
| 2 | s319284731 | Liam | van der Merwe | s319284731@mandela.ac.za | FALSE |
| 3 | s321056839 | Ayanda | Mthembu | s321056839@mandela.ac.za | TRUE |
| 4 | s317493208 | Chloe | Botha | s317493208@mandela.ac.za | FALSE |
| 5 | s329187465 | Sipho | Zulu | s329187465@mandela.ac.za | FALSE |
| 6 | s318274056 | Jessica | Adams | s318274056@mandela.ac.za | TRUE |
| 7 | s322941587 | Mandla | Ndlovu | s322941587@mandela.ac.za | FALSE |
| 8 | s316785234 | Regan | Pieterse | s316785234@mandela.ac.za | FALSE |
| 9 | s325098471 | Zanele | Khumalo | s325098471@mandela.ac.za | FALSE |
| 10 | s311653928 | Devon | Jacobs | s311653928@mandela.ac.za | FALSE |
| 11 | s328417509 | Thandi | Sithole | s328417509@mandela.ac.za | FALSE |
| 12 | s314928673 | Jason | Williams | s314928673@mandela.ac.za | FALSE |
| 13 | s327651084 | Nokwanda | Dube | s327651084@mandela.ac.za | TRUE |
| 14 | s319073845 | Michael | van Niekerk | s319073845@mandela.ac.za | FALSE |
| 15 | s323846190 | Lerato | Molefe | s323846190@mandela.ac.za | FALSE |

---

## Marshals

| MarshalId | StaffNumber | FirstName | LastName | UniversityEmail | AssignedCampusId | AssignedStopId |
|---|---|---|---|---|---|---|
| 1 | 4827391 | Bongani | Cele | b.cele@mandela.ac.za | 1 | NULL |
| 2 | 5914826 | Karen | Swart | k.swart@mandela.ac.za | 2 | NULL |
| 3 | 3208574 | Vuyo | Mabena | v.mabena@mandela.ac.za | 3 | NULL |
| 4 | 6153042 | Thulani | Ngcobo | t.ngcobo@mandela.ac.za | NULL | 1 |
| 5 | 7294185 | Lindiwe | Shabalala | l.shabalala@mandela.ac.za | NULL | 2 |
| 6 | 8401267 | Pieter | Venter | p.venter@mandela.ac.za | NULL | 3 |
| 7 | 9537148 | Nombuso | Mkhize | n.mkhize@mandela.ac.za | NULL | 4 |
| 8 | 2648309 | Craig | Daniels | c.daniels@mandela.ac.za | NULL | 5 |
| 9 | 1785423 | Noluthando | Mthethwa | n.mthethwa@mandela.ac.za | NULL | 6 |
| 10 | 4062891 | Willem | Pretorius | w.pretorius@mandela.ac.za | NULL | 7 |
| 11 | 5379216 | Zandile | Mdluli | z.mdluli@mandela.ac.za | NULL | 8 |

---

## Shuttles

| ShuttleId | LicensePlate | Capacity | VendorCompanyName | IsAccessibleVehicle |
|---|---|---|---|---|
| 1 | EC 142-837 | 22 | Bay Transit Corp | TRUE |
| 2 | EC 256-491 | 35 | Bay Transit Corp | FALSE |
| 3 | EC 389-720 | 22 | Bay Transit Corp | FALSE |
| 4 | EC 501-163 | 28 | Coastal Movers | TRUE |
| 5 | EC 628-945 | 35 | Coastal Movers | FALSE |
| 6 | EC 714-382 | 22 | Coastal Movers | FALSE |
| 7 | EC 837-256 | 28 | Metro Link SA | FALSE |
| 8 | EC 948-617 | 35 | Metro Link SA | FALSE |

---

## Schedule Times

| ScheduleTimeId | DepartureTime | Direction | ScheduleType | CampusId | StopId |
|---|---|---|---|---|---|
| 1 | 06:30 | ToCampus | Weekday | 1 | 1 |
| 2 | 06:30 | ToCampus | Weekday | 1 | 2 |
| 3 | 07:15 | ToCampus | Weekday | 1 | 3 |
| 4 | 07:15 | ToCampus | Weekday | 2 | 4 |
| 5 | 07:45 | ToCampus | Weekday | 2 | 5 |
| 6 | 08:30 | ToCampus | Weekday | 3 | 6 |
| 7 | 12:30 | FromCampus | Weekday | 1 | 1 |
| 8 | 12:30 | FromCampus | Weekday | 1 | 2 |
| 9 | 14:00 | FromCampus | Weekday | 1 | 3 |
| 10 | 14:00 | FromCampus | Weekday | 2 | 4 |
| 11 | 16:30 | FromCampus | Weekday | 1 | 1 |
| 12 | 16:30 | FromCampus | Weekday | 1 | 2 |
| 13 | 16:30 | FromCampus | Weekday | 2 | 5 |
| 14 | 17:15 | FromCampus | Weekday | 3 | 6 |
| 15 | 08:00 | ToCampus | Saturday | 1 | 1 |
| 16 | 08:00 | ToCampus | Saturday | 1 | 2 |
| 17 | 13:00 | FromCampus | Saturday | 1 | 1 |
| 18 | 13:00 | FromCampus | Saturday | 1 | 2 |

---

## Calendar Exceptions (2025)

| CalendarExceptionId | TargetDate | ExceptionType | Description |
|---|---|---|---|
| 1 | 2025-03-21 | PublicHoliday | Human Rights Day |
| 2 | 2025-04-18 | PublicHoliday | Good Friday |
| 3 | 2025-04-21 | PublicHoliday | Family Day |
| 4 | 2025-04-28 | UniversityRecess | Mid-year recess start |
| 5 | 2025-04-29 | UniversityRecess | Mid-year recess |
| 6 | 2025-04-30 | UniversityRecess | Mid-year recess |
| 7 | 2025-05-01 | PublicHoliday | Workers Day |
| 8 | 2025-06-16 | PublicHoliday | Youth Day |
| 9 | 2025-08-09 | PublicHoliday | National Women's Day |
| 10 | 2025-09-24 | PublicHoliday | Heritage Day |

---

## Priority Schedules (Disability-Cleared Students)

| PriorityScheduleId | StudentId | StopId | ScheduleTimeId | DayOfWeek | IsActive |
|---|---|---|---|---|---|
| 1 | 3 | 1 | 1 | Monday | TRUE |
| 2 | 3 | 1 | 1 | Wednesday | TRUE |
| 3 | 3 | 1 | 1 | Friday | TRUE |
| 4 | 3 | 1 | 11 | Monday | TRUE |
| 5 | 3 | 1 | 11 | Wednesday | TRUE |
| 6 | 3 | 1 | 11 | Friday | TRUE |
| 7 | 6 | 2 | 2 | Tuesday | TRUE |
| 8 | 6 | 2 | 2 | Thursday | TRUE |
| 9 | 6 | 2 | 12 | Tuesday | TRUE |
| 10 | 6 | 2 | 12 | Thursday | TRUE |
| 11 | 13 | 4 | 4 | Monday | TRUE |
| 12 | 13 | 4 | 4 | Wednesday | TRUE |
| 13 | 13 | 4 | 10 | Monday | TRUE |
| 14 | 13 | 4 | 10 | Wednesday | TRUE |

---

## Notes

- Admin passwords above are in plain text for development seeding only. In production, they will be stored as BCrypt hashes (cost factor 12).
- Student OTP fields (`OtpHash`, `OtpExpiry`) and Marshal OTP fields are initially NULL — OTPs are generated on-demand at login time.
- `DeviceRegistration` records are not seeded — they are created dynamically when a user authenticates from a device.
- `TripInstance`, `TripShuttleAllocation`, `Booking`, `Notification`, and `Report` records are not seeded — they are generated by the system at runtime (nightly injection job, student bookings, marshal check-ins, etc.).
