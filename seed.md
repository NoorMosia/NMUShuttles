# Exhaustive Seed Data — NMU Shuttle Management Network

This file contains comprehensive historical seed data for development, testing, and dashboard demonstration.
All data is fictitious. The dataset covers a 2-week operational window (July 28 – August 12, 2026) with
every possible entity state represented.

**Timeline anchor:** "Today" for this seed = August 13, 2026. All trip data below is in the past.

**Operational days (13 total):**
- Weekdays (11): Jul 28(Tue), Jul 29(Wed), Jul 30(Thu), Jul 31(Fri), Aug 3(Mon), Aug 4(Tue), Aug 5(Wed), Aug 6(Thu), Aug 7(Fri), Aug 11(Tue), Aug 12(Wed)
- Saturdays (2): Aug 1(Sat), Aug 8(Sat)
- Skipped: Aug 2(Sun), Aug 9(Sun), Aug 10(Mon — National Women's Day observed)

**All timestamps are UTC. SAST = UTC+2.**

---

## 1. Admins

| AdminId | Email | StaffNumber | Password (plain, dev only) | FirstName | LastName | IsActive | CreatedAt |
|---|---|---|---|---|---|---|---|
| 1 | admin@mandela.ac.za | 7001001 | Admin@2025! | Thabo | Mokoena | TRUE | 2025-01-15T00:00:00Z |
| 2 | transport@mandela.ac.za | 7001002 | Transport@2025! | Nomsa | Dlamini | TRUE | 2025-01-15T00:00:00Z |

---

## 2. Campuses

| CampusId | CampusName | GpsCoordinates |
|---|---|---|
| 1 | South Campus | -33.9580,25.5920 |
| 2 | North Campus | -33.9185,25.5870 |
| 3 | 2nd Avenue Campus | -33.9610,25.6100 |

---

## 3. Stops

| StopId | StopName | Street | Suburb | City | PostalCode | GpsCoordinates | IsActive |
|---|---|---|---|---|---|---|---|
| 1 | Summerstrand Village | 12 Marine Drive | Summerstrand | Gqeberha | 6001 | -33.9680,25.6450 | TRUE |
| 2 | Humewood | 45 Beach Road | Humewood | Gqeberha | 6001 | -33.9630,25.6280 | TRUE |
| 3 | Newton Park | 78 Cape Road | Newton Park | Gqeberha | 6045 | -33.9480,25.5750 | TRUE |
| 4 | Walmer Heights | 23 Heugh Road | Walmer Heights | Gqeberha | 6070 | -33.9850,25.6100 | TRUE |
| 5 | Lorraine | 91 Circular Drive | Lorraine | Gqeberha | 6070 | -33.9320,25.5620 | TRUE |
| 6 | Bluewater Bay | 5 Seaview Crescent | Bluewater Bay | Gqeberha | 6210 | -33.8710,25.5590 | TRUE |
| 7 | Framesby | 14 Langenhoven Drive | Framesby | Gqeberha | 6045 | -33.9260,25.5810 | TRUE |
| 8 | Greenacres | 30 Ring Road | Greenacres | Gqeberha | 6045 | -33.9410,25.5870 | TRUE |

---

## 4. Vendors

| VendorId | CompanyName | ContactEmail | ContactPhone | CreatedAt |
|---|---|---|---|---|
| 1 | Bay Transit Corp | info@baytransit.co.za | 041-555-0101 | 2025-01-15T00:00:00Z |
| 2 | Coastal Movers | info@coastalmovers.co.za | 041-555-0202 | 2025-01-15T00:00:00Z |
| 3 | Metro Link SA | info@metrolinksa.co.za | 041-555-0303 | 2025-01-15T00:00:00Z |

---

## 5. Shuttles

19 shuttles total. Accessible vehicles are marked. These form the convoy fleet.

| ShuttleId | LicensePlate | Capacity | VendorId | VendorCompanyName | IsAccessibleVehicle | IsActive | CreatedAt |
|---|---|---|---|---|---|---|---|
| 1 | EC 142-837 | 22 | 1 | Bay Transit Corp | TRUE | TRUE | 2025-01-15T00:00:00Z |
| 2 | EC 256-491 | 35 | 1 | Bay Transit Corp | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 3 | EC 389-720 | 22 | 1 | Bay Transit Corp | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 4 | EC 501-163 | 28 | 2 | Coastal Movers | TRUE | TRUE | 2025-01-15T00:00:00Z |
| 5 | EC 628-945 | 35 | 2 | Coastal Movers | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 6 | EC 714-382 | 22 | 2 | Coastal Movers | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 7 | EC 837-256 | 28 | 3 | Metro Link SA | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 8 | EC 948-617 | 35 | 3 | Metro Link SA | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 9 | EC 103-475 | 22 | 3 | Metro Link SA | TRUE | TRUE | 2025-01-15T00:00:00Z |
| 10 | EC 217-864 | 28 | 1 | Bay Transit Corp | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 11 | EC 331-058 | 35 | 1 | Bay Transit Corp | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 12 | EC 445-912 | 22 | 2 | Coastal Movers | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 13 | EC 552-376 | 28 | 2 | Coastal Movers | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 14 | EC 668-741 | 35 | 2 | Coastal Movers | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 15 | EC 774-285 | 22 | 3 | Metro Link SA | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 16 | EC 889-639 | 28 | 3 | Metro Link SA | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 17 | EC 993-104 | 35 | 3 | Metro Link SA | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 18 | EC 107-528 | 22 | 1 | Bay Transit Corp | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 19 | EC 214-963 | 28 | 1 | Bay Transit Corp | TRUE | TRUE | 2025-01-15T00:00:00Z |

**Fleet summary:**
- Bay Transit Corp (VendorId=1): Shuttles 1,2,3,10,11,18,19 — 7 vehicles (capacity: 22+35+22+28+35+22+28 = 192)
- Coastal Movers (VendorId=2): Shuttles 4,5,6,12,13,14 — 6 vehicles (capacity: 28+35+22+22+28+35 = 170)
- Metro Link SA (VendorId=3): Shuttles 7,8,9,15,16,17 — 6 vehicles (capacity: 28+35+22+22+28+35 = 170)

**Accessible shuttles:** 1 (Bay Transit), 4 (Coastal), 9 (Metro Link), 19 (Bay Transit)

---

## 6. StopShuttleDefaults (Standing Convoy Rosters)

Accessible shuttles always in position 1 (where available). Busier stops get 3-shuttle convoys.

| StopShuttleDefaultId | StopId | ShuttleId | OrderPosition | CreatedAt |
|---|---|---|---|---|
| 1 | 1 | 1 | 1 | 2025-01-15T00:00:00Z |
| 2 | 1 | 2 | 2 | 2025-01-15T00:00:00Z |
| 3 | 1 | 3 | 3 | 2025-01-15T00:00:00Z |
| 4 | 2 | 4 | 1 | 2025-01-15T00:00:00Z |
| 5 | 2 | 5 | 2 | 2025-01-15T00:00:00Z |
| 6 | 2 | 6 | 3 | 2025-01-15T00:00:00Z |
| 7 | 3 | 9 | 1 | 2025-01-15T00:00:00Z |
| 8 | 3 | 7 | 2 | 2025-01-15T00:00:00Z |
| 9 | 3 | 8 | 3 | 2025-01-15T00:00:00Z |
| 10 | 4 | 19 | 1 | 2025-01-15T00:00:00Z |
| 11 | 4 | 10 | 2 | 2025-01-15T00:00:00Z |
| 12 | 5 | 11 | 1 | 2025-01-15T00:00:00Z |
| 13 | 5 | 12 | 2 | 2025-01-15T00:00:00Z |
| 14 | 6 | 13 | 1 | 2025-01-15T00:00:00Z |
| 15 | 6 | 14 | 2 | 2025-01-15T00:00:00Z |
| 16 | 7 | 15 | 1 | 2025-01-15T00:00:00Z |
| 17 | 7 | 16 | 2 | 2025-01-15T00:00:00Z |
| 18 | 8 | 17 | 1 | 2025-01-15T00:00:00Z |
| 19 | 8 | 18 | 2 | 2025-01-15T00:00:00Z |

**Convoy capacity by stop:**
- Stop 1 (Summerstrand): S1(22) + S2(35) + S3(22) = **79 seats**
- Stop 2 (Humewood): S4(28) + S5(35) + S6(22) = **85 seats**
- Stop 3 (Newton Park): S9(22) + S7(28) + S8(35) = **85 seats**
- Stop 4 (Walmer Heights): S19(28) + S10(28) = **56 seats**
- Stop 5 (Lorraine): S11(35) + S12(22) = **57 seats**
- Stop 6 (Bluewater Bay): S13(28) + S14(35) = **63 seats**
- Stop 7 (Framesby): S15(22) + S16(28) = **50 seats**
- Stop 8 (Greenacres): S17(35) + S18(22) = **57 seats**

---

## 7. Marshals

| MarshalId | StaffNumber | FirstName | LastName | UniversityEmail | AssignedCampusId | AssignedStopId | IsActive | CreatedAt |
|---|---|---|---|---|---|---|---|---|
| 1 | 4827391 | Bongani | Cele | b.cele@mandela.ac.za | 1 | NULL | TRUE | 2025-01-15T00:00:00Z |
| 2 | 5914826 | Karen | Swart | k.swart@mandela.ac.za | 2 | NULL | TRUE | 2025-01-15T00:00:00Z |
| 3 | 3208574 | Vuyo | Mabena | v.mabena@mandela.ac.za | 3 | NULL | TRUE | 2025-01-15T00:00:00Z |
| 4 | 6153042 | Thulani | Ngcobo | t.ngcobo@mandela.ac.za | NULL | 1 | TRUE | 2025-01-15T00:00:00Z |
| 5 | 7294185 | Lindiwe | Shabalala | l.shabalala@mandela.ac.za | NULL | 2 | TRUE | 2025-01-15T00:00:00Z |
| 6 | 8401267 | Pieter | Venter | p.venter@mandela.ac.za | NULL | 3 | TRUE | 2025-01-15T00:00:00Z |
| 7 | 9537148 | Nombuso | Mkhize | n.mkhize@mandela.ac.za | NULL | 4 | TRUE | 2025-01-15T00:00:00Z |
| 8 | 2648309 | Craig | Daniels | c.daniels@mandela.ac.za | NULL | 5 | TRUE | 2025-01-15T00:00:00Z |
| 9 | 1785423 | Noluthando | Mthethwa | n.mthethwa@mandela.ac.za | NULL | 6 | TRUE | 2025-01-15T00:00:00Z |
| 10 | 4062891 | Willem | Pretorius | w.pretorius@mandela.ac.za | NULL | 7 | TRUE | 2025-01-15T00:00:00Z |
| 11 | 5379216 | Zandile | Mdluli | z.mdluli@mandela.ac.za | NULL | 8 | TRUE | 2025-01-15T00:00:00Z |

---

## 8. Students (60 total)

- StudentId 1–15: Original roster (from firstSeed.md)
- StudentId 16–60: New students
- Priority users: StudentId 3, 6, 13, 20, 35
- Inactive (struck out): StudentId 47, 48, 49, 50

| StudentId | StudentNumber | FirstName | LastName | UniversityEmail | IsPriorityUser | IsActive | CreatedAt |
|---|---|---|---|---|---|---|---|
| 1 | s312847563 | Sarah | Nkosi | s312847563@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 2 | s319284731 | Liam | van der Merwe | s319284731@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 3 | s321056839 | Ayanda | Mthembu | s321056839@mandela.ac.za | TRUE | TRUE | 2025-01-15T00:00:00Z |
| 4 | s317493208 | Chloe | Botha | s317493208@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 5 | s329187465 | Sipho | Zulu | s329187465@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 6 | s318274056 | Jessica | Adams | s318274056@mandela.ac.za | TRUE | TRUE | 2025-01-15T00:00:00Z |
| 7 | s322941587 | Mandla | Ndlovu | s322941587@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 8 | s316785234 | Regan | Pieterse | s316785234@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 9 | s325098471 | Zanele | Khumalo | s325098471@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 10 | s311653928 | Devon | Jacobs | s311653928@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 11 | s328417509 | Thandi | Sithole | s328417509@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 12 | s314928673 | Jason | Williams | s314928673@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 13 | s327651084 | Nokwanda | Dube | s327651084@mandela.ac.za | TRUE | TRUE | 2025-01-15T00:00:00Z |
| 14 | s319073845 | Michael | van Niekerk | s319073845@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 15 | s323846190 | Lerato | Molefe | s323846190@mandela.ac.za | FALSE | TRUE | 2025-01-15T00:00:00Z |
| 16 | s314527896 | Thabo | Maseko | s314527896@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 17 | s328193045 | Amahle | Nxumalo | s328193045@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 18 | s311847290 | Ethan | du Plessis | s311847290@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 19 | s325604183 | Naledi | Tshabalala | s325604183@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 20 | s319438267 | Kwame | Osei | s319438267@mandela.ac.za | TRUE | TRUE | 2025-02-01T00:00:00Z |
| 21 | s322785410 | Bianca | Ferreira | s322785410@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 22 | s316094358 | Siyabonga | Mhlongo | s316094358@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 23 | s329471023 | Megan | Swanepoel | s329471023@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 24 | s313268947 | Thandolwethu | Gumede | s313268947@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 25 | s327815604 | Ryan | Cloete | s327815604@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 26 | s311542789 | Nompumelelo | Zwane | s311542789@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 27 | s324896031 | Dylan | Viljoen | s324896031@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 28 | s318623475 | Palesa | Mokoena | s318623475@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 29 | s326057184 | Luca | Naude | s326057184@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 30 | s312384956 | Zinhle | Buthelezi | s312384956@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 31 | s329710438 | Joshua | Erasmus | s329710438@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 32 | s315847291 | Busisiwe | Mabaso | s315847291@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 33 | s323196804 | Luke | Barnard | s323196804@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 34 | s317429058 | Nonhlanhla | Nkabinde | s317429058@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 35 | s321863742 | Thato | Letsoko | s321863742@mandela.ac.za | TRUE | TRUE | 2025-02-01T00:00:00Z |
| 36 | s314085267 | Hannah | Muller | s314085267@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 37 | s328574190 | Lungelo | Shabangu | s328574190@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 38 | s316238904 | Amber | Joubert | s316238904@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 39 | s322491637 | Sbusiso | Radebe | s322491637@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 40 | s319764082 | Tamsin | Olivier | s319764082@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 41 | s325138476 | Buhle | Mahlangu | s325138476@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 42 | s313692815 | Connor | le Roux | s313692815@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 43 | s327045963 | Sinethemba | Majola | s327045963@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 44 | s311928347 | Kayla | Steyn | s311928347@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 45 | s324581076 | Andile | Dladla | s324581076@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 46 | s318305492 | Emma | van Wyk | s318305492@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 47 | s326742815 | Themba | Vilakazi | s326742815@mandela.ac.za | FALSE | FALSE | 2025-02-01T00:00:00Z |
| 48 | s312059638 | Danielle | Potgieter | s312059638@mandela.ac.za | FALSE | FALSE | 2025-02-01T00:00:00Z |
| 49 | s329384071 | Mthunzi | Cele | s329384071@mandela.ac.za | FALSE | FALSE | 2025-02-01T00:00:00Z |
| 50 | s315670284 | Robyn | Prinsloo | s315670284@mandela.ac.za | FALSE | FALSE | 2025-02-01T00:00:00Z |
| 51 | s323947158 | Anathi | Magwaza | s323947158@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 52 | s317283046 | Jaden | Booysen | s317283046@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 53 | s325816490 | Lindiwe | Ngubane | s325816490@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 54 | s311475823 | Aiden | Marais | s311475823@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 55 | s328640175 | Nandi | Mthimkhulu | s328640175@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 56 | s314903268 | Caleb | Bezuidenhout | s314903268@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 57 | s322167549 | Yonela | Sifuba | s322167549@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 58 | s316538291 | Mia | Hendriks | s316538291@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 59 | s329081467 | Khaya | Mkhwanazi | s329081467@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |
| 60 | s313724850 | Jade | van Rensburg | s313724850@mandela.ac.za | FALSE | TRUE | 2025-02-01T00:00:00Z |

**Summary:** 60 students — 5 priority, 4 inactive (struck out), 51 active regular.

---
 
## 9. Schedule Times

19 entries per stop × 8 stops = **152 total ScheduleTime records**.

Pattern per stop (IDs are sequential, 19 per stop):
- Positions 1–4: Weekday ToCampus (06:45, 07:45, 08:45, 09:45)
- Positions 5–11: Weekday FromCampus (12:30, 14:30, 16:00, 17:30, 18:30, 20:30, 22:00)
- Positions 12–15: Saturday ToCampus (06:45, 07:45, 08:45, 09:45)
- Positions 16–19: Saturday FromCampus (12:30, 14:30, 16:00, 17:30)

| StopId | ScheduleTimeId Range | Example: ID for 06:45 Weekday ToCampus |
|---|---|---|
| 1 | 1–19 | 1 |
| 2 | 20–38 | 20 |
| 3 | 39–57 | 39 |
| 4 | 58–76 | 58 |
| 5 | 77–95 | 77 |
| 6 | 96–114 | 96 |
| 7 | 115–133 | 115 |
| 8 | 134–152 | 134 |

All schedule times have `RequiresScanning = TRUE`.

**Full reference (Stop 1 shown, others follow same pattern with offset):**

| ScheduleTimeId | DepartureTime | Direction | ScheduleType | StopId |
|---|---|---|---|---|
| 1 | 06:45 | ToCampus | Weekday | 1 |
| 2 | 07:45 | ToCampus | Weekday | 1 |
| 3 | 08:45 | ToCampus | Weekday | 1 |
| 4 | 09:45 | ToCampus | Weekday | 1 |
| 5 | 12:30 | FromCampus | Weekday | 1 |
| 6 | 14:30 | FromCampus | Weekday | 1 |
| 7 | 16:00 | FromCampus | Weekday | 1 |
| 8 | 17:30 | FromCampus | Weekday | 1 |
| 9 | 18:30 | FromCampus | Weekday | 1 |
| 10 | 20:30 | FromCampus | Weekday | 1 |
| 11 | 22:00 | FromCampus | Weekday | 1 |
| 12 | 06:45 | ToCampus | Saturday | 1 |
| 13 | 07:45 | ToCampus | Saturday | 1 |
| 14 | 08:45 | ToCampus | Saturday | 1 |
| 15 | 09:45 | ToCampus | Saturday | 1 |
| 16 | 12:30 | FromCampus | Saturday | 1 |
| 17 | 14:30 | FromCampus | Saturday | 1 |
| 18 | 16:00 | FromCampus | Saturday | 1 |
| 19 | 17:30 | FromCampus | Saturday | 1 |

---

## 10. ScheduleTimeCampus (Junction)

All schedule times route to South Campus only (CampusId=1), StopOrder=1.

| ScheduleTimeCampusId | ScheduleTimeId | CampusId | StopOrder |
|---|---|---|---|
| 1–152 | 1–152 | 1 | 1 |

(One row per ScheduleTime. ScheduleTimeCampusId = ScheduleTimeId for simplicity.)

---

## 11. Calendar Exceptions

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
| 11 | 2026-03-21 | PublicHoliday | Human Rights Day |
| 12 | 2026-04-03 | PublicHoliday | Good Friday |
| 13 | 2026-04-06 | PublicHoliday | Family Day |
| 14 | 2026-04-27 | PublicHoliday | Freedom Day |
| 15 | 2026-05-01 | PublicHoliday | Workers Day |
| 16 | 2026-06-16 | PublicHoliday | Youth Day |
| 17 | 2026-08-10 | PublicHoliday | National Women's Day (observed, Aug 9 is Sunday) |
| 18 | 2026-09-24 | PublicHoliday | Heritage Day |

---

## 12. Priority Schedules

5 priority students with recurring travel needs.

| PriorityScheduleId | StudentId | StopId | ScheduleTimeId | DayOfWeek | IsActive | CreatedAt |
|---|---|---|---|---|---|---|
| 1 | 3 | 1 | 1 | Monday | TRUE | 2025-01-15T00:00:00Z |
| 2 | 3 | 1 | 1 | Wednesday | TRUE | 2025-01-15T00:00:00Z |
| 3 | 3 | 1 | 1 | Friday | TRUE | 2025-01-15T00:00:00Z |
| 4 | 3 | 1 | 7 | Monday | TRUE | 2025-01-15T00:00:00Z |
| 5 | 3 | 1 | 7 | Wednesday | TRUE | 2025-01-15T00:00:00Z |
| 6 | 3 | 1 | 7 | Friday | TRUE | 2025-01-15T00:00:00Z |
| 7 | 6 | 2 | 20 | Tuesday | TRUE | 2025-01-15T00:00:00Z |
| 8 | 6 | 2 | 20 | Thursday | TRUE | 2025-01-15T00:00:00Z |
| 9 | 6 | 2 | 26 | Tuesday | TRUE | 2025-01-15T00:00:00Z |
| 10 | 6 | 2 | 26 | Thursday | TRUE | 2025-01-15T00:00:00Z |
| 11 | 13 | 4 | 58 | Monday | TRUE | 2025-01-15T00:00:00Z |
| 12 | 13 | 4 | 58 | Wednesday | TRUE | 2025-01-15T00:00:00Z |
| 13 | 13 | 4 | 63 | Monday | TRUE | 2025-01-15T00:00:00Z |
| 14 | 13 | 4 | 63 | Wednesday | TRUE | 2025-01-15T00:00:00Z |
| 15 | 20 | 3 | 39 | Monday | TRUE | 2025-02-01T00:00:00Z |
| 16 | 20 | 3 | 39 | Tuesday | TRUE | 2025-02-01T00:00:00Z |
| 17 | 20 | 3 | 39 | Wednesday | TRUE | 2025-02-01T00:00:00Z |
| 18 | 20 | 3 | 39 | Thursday | TRUE | 2025-02-01T00:00:00Z |
| 19 | 20 | 3 | 45 | Monday | TRUE | 2025-02-01T00:00:00Z |
| 20 | 20 | 3 | 45 | Tuesday | TRUE | 2025-02-01T00:00:00Z |
| 21 | 20 | 3 | 45 | Wednesday | TRUE | 2025-02-01T00:00:00Z |
| 22 | 20 | 3 | 45 | Thursday | TRUE | 2025-02-01T00:00:00Z |
| 23 | 35 | 5 | 77 | Monday | TRUE | 2025-02-01T00:00:00Z |
| 24 | 35 | 5 | 77 | Wednesday | TRUE | 2025-02-01T00:00:00Z |
| 25 | 35 | 5 | 77 | Friday | TRUE | 2025-02-01T00:00:00Z |
| 26 | 35 | 5 | 83 | Monday | TRUE | 2025-02-01T00:00:00Z |
| 27 | 35 | 5 | 83 | Wednesday | TRUE | 2025-02-01T00:00:00Z |
| 28 | 35 | 5 | 83 | Friday | TRUE | 2025-02-01T00:00:00Z |

**Priority student summary:**
- Student 3 (Ayanda): Stop 1, ToCampus 06:45 + FromCampus 16:00, Mon/Wed/Fri
- Student 6 (Jessica): Stop 2, ToCampus 06:45 + FromCampus 16:00, Tue/Thu
- Student 13 (Nokwanda): Stop 4, ToCampus 06:45 + FromCampus 14:30, Mon/Wed
- Student 20 (Kwame): Stop 3, ToCampus 06:45 + FromCampus 16:00, Mon/Tue/Wed/Thu
- Student 35 (Thato): Stop 5, ToCampus 06:45 + FromCampus 16:00, Mon/Wed/Fri

---

## 13. Device Registrations

Seeded for a subset of active students and all marshals.

| DeviceRegistrationId | OwnerId | OwnerType | FcmToken | Platform | UpdatedAt |
|---|---|---|---|---|---|
| 1 | 1 | Student | fcm_token_student_1_a1b2c3d4e5 | android | 2026-07-20T08:00:00Z |
| 2 | 2 | Student | fcm_token_student_2_f6g7h8i9j0 | android | 2026-07-20T08:00:00Z |
| 3 | 3 | Student | fcm_token_student_3_k1l2m3n4o5 | android | 2026-07-20T08:00:00Z |
| 4 | 4 | Student | fcm_token_student_4_p6q7r8s9t0 | android | 2026-07-20T08:00:00Z |
| 5 | 5 | Student | fcm_token_student_5_u1v2w3x4y5 | android | 2026-07-20T08:00:00Z |
| 6 | 6 | Student | fcm_token_student_6_z6a7b8c9d0 | android | 2026-07-20T08:00:00Z |
| 7 | 7 | Student | fcm_token_student_7_e1f2g3h4i5 | android | 2026-07-20T08:00:00Z |
| 8 | 8 | Student | fcm_token_student_8_j6k7l8m9n0 | android | 2026-07-20T08:00:00Z |
| 9 | 9 | Student | fcm_token_student_9_o1p2q3r4s5 | android | 2026-07-20T08:00:00Z |
| 10 | 10 | Student | fcm_token_student_10_t6u7v8w9x0 | android | 2026-07-20T08:00:00Z |
| 11 | 13 | Student | fcm_token_student_13_y1z2a3b4c5 | android | 2026-07-20T08:00:00Z |
| 12 | 16 | Student | fcm_token_student_16_d6e7f8g9h0 | android | 2026-07-20T08:00:00Z |
| 13 | 20 | Student | fcm_token_student_20_i1j2k3l4m5 | android | 2026-07-20T08:00:00Z |
| 14 | 25 | Student | fcm_token_student_25_n6o7p8q9r0 | android | 2026-07-20T08:00:00Z |
| 15 | 30 | Student | fcm_token_student_30_s1t2u3v4w5 | android | 2026-07-20T08:00:00Z |
| 16 | 35 | Student | fcm_token_student_35_x6y7z8a9b0 | android | 2026-07-20T08:00:00Z |
| 17 | 40 | Student | fcm_token_student_40_c1d2e3f4g5 | android | 2026-07-20T08:00:00Z |
| 18 | 45 | Student | fcm_token_student_45_h6i7j8k9l0 | android | 2026-07-20T08:00:00Z |
| 19 | 50 | Student | fcm_token_student_50_m1n2o3p4q5 | android | 2026-07-20T08:00:00Z |
| 20 | 55 | Student | fcm_token_student_55_r6s7t8u9v0 | android | 2026-07-20T08:00:00Z |
| 21 | 1 | Marshal | fcm_token_marshal_1_w1x2y3z4a5 | android | 2026-07-20T08:00:00Z |
| 22 | 2 | Marshal | fcm_token_marshal_2_b6c7d8e9f0 | android | 2026-07-20T08:00:00Z |
| 23 | 3 | Marshal | fcm_token_marshal_3_g1h2i3j4k5 | android | 2026-07-20T08:00:00Z |
| 24 | 4 | Marshal | fcm_token_marshal_4_l6m7n8o9p0 | android | 2026-07-20T08:00:00Z |
| 25 | 5 | Marshal | fcm_token_marshal_5_q1r2s3t4u5 | android | 2026-07-20T08:00:00Z |
| 26 | 6 | Marshal | fcm_token_marshal_6_v6w7x8y9z0 | android | 2026-07-20T08:00:00Z |
| 27 | 7 | Marshal | fcm_token_marshal_7_a1b2c3d4e5 | android | 2026-07-20T08:00:00Z |
| 28 | 8 | Marshal | fcm_token_marshal_8_f6g7h8i9j0 | android | 2026-07-20T08:00:00Z |
| 29 | 9 | Marshal | fcm_token_marshal_9_k1l2m3n4o5 | android | 2026-07-20T08:00:00Z |
| 30 | 10 | Marshal | fcm_token_marshal_10_p6q7r8s9t0 | android | 2026-07-20T08:00:00Z |
| 31 | 11 | Marshal | fcm_token_marshal_11_u1v2w3x4y5 | android | 2026-07-20T08:00:00Z |

---

## 14. Trip Instances

### ID Scheme

Trips are generated sequentially by date, then by stop, then by schedule time slot.

**Per day:**
- Weekday: 11 timeslots × 8 stops = 88 trips
- Saturday: 8 timeslots × 8 stops = 64 trips

**Day order and base IDs:**

| Day# | Date | DayOfWeek | Type | Trips | TripId Range |
|---|---|---|---|---|---|
| 1 | 2026-07-28 | Tuesday | Weekday | 88 | 1–88 |
| 2 | 2026-07-29 | Wednesday | Weekday | 88 | 89–176 |
| 3 | 2026-07-30 | Thursday | Weekday | 88 | 177–264 |
| 4 | 2026-07-31 | Friday | Weekday | 88 | 265–352 |
| 5 | 2026-08-01 | Saturday | Saturday | 64 | 353–416 |
| 6 | 2026-08-03 | Monday | Weekday | 88 | 417–504 |
| 7 | 2026-08-04 | Tuesday | Weekday | 88 | 505–592 |
| 8 | 2026-08-05 | Wednesday | Weekday | 88 | 593–680 |
| 9 | 2026-08-06 | Thursday | Weekday | 88 | 681–768 |
| 10 | 2026-08-07 | Friday | Weekday | 88 | 769–856 |
| 11 | 2026-08-08 | Saturday | Saturday | 64 | 857–920 |
| 12 | 2026-08-11 | Tuesday | Weekday | 88 | 921–1008 |
| 13 | 2026-08-12 | Wednesday | Weekday | 88 | 1009–1096 |

**Total: 1,096 TripInstances** (note: Aug 10 is skipped due to CalendarException)

### Within each weekday (88 trips), ordering is:

Stops 1–8, within each stop the 11 weekday timeslots in order (matching ScheduleTimeId offsets 1–11 within the stop's range).

| Offset within day | StopId | ScheduleTimeId (relative to stop) | DepartureTime |
|---|---|---|---|
| 1 | 1 | +0 (06:45 ToCampus) | 06:45 |
| 2 | 1 | +1 (07:45 ToCampus) | 07:45 |
| 3 | 1 | +2 (08:45 ToCampus) | 08:45 |
| 4 | 1 | +3 (09:45 ToCampus) | 09:45 |
| 5 | 1 | +4 (12:30 FromCampus) | 12:30 |
| 6 | 1 | +5 (14:30 FromCampus) | 14:30 |
| 7 | 1 | +6 (16:00 FromCampus) | 16:00 |
| 8 | 1 | +7 (17:30 FromCampus) | 17:30 |
| 9 | 1 | +8 (18:30 FromCampus) | 18:30 |
| 10 | 1 | +9 (20:30 FromCampus) | 20:30 |
| 11 | 1 | +10 (22:00 FromCampus) | 22:00 |
| 12 | 2 | +0 (06:45 ToCampus) | 06:45 |
| ... | ... | ... | ... |
| 88 | 8 | +10 (22:00 FromCampus) | 22:00 |

Within each Saturday (64 trips), ordering is:

Stops 1–8, within each stop the 8 Saturday timeslots (matching ScheduleTimeId offsets 12–19 within the stop's range, i.e., positions 12–15 ToCampus + 16–19 FromCampus).

### Formula to compute any TripInstanceId:

```
For weekday (Day# d where d is 1-based index in the day list above, excluding Saturdays):
  base = sum of all trips in prior days
  offset = (StopId - 1) * 11 + timeslotIndex (1-based within the 11 weekday slots)
  TripInstanceId = base + offset

For Saturday:
  base = sum of all trips in prior days
  offset = (StopId - 1) * 8 + timeslotIndex (1-based within the 8 Saturday slots)
  TripInstanceId = base + offset
```

### Trip Status Distribution

The vast majority of trips are **Completed** (normal operations). Special statuses are applied to specific scenario trips:

| TripInstanceId | Date | Stop | Time | Status | Scenario |
|---|---|---|---|---|---|
| 1–504 | Jul 28–Aug 3 | All | All | Completed | Normal completed operations (first 6 days) |
| 505 | 2026-08-04 | 1 | 06:45 | Completed | **Breakdown + Replacement** — Shuttle 2 broke down, replaced by shuttle 11 |
| 506–592 | 2026-08-04 | All | Various | Completed | Normal (rest of Aug 4) |
| 593 | 2026-08-05 | 1 | 06:45 | Completed | **Waitlist overflow** — more bookings than capacity, waitlist promotions occurred |
| 594–680 | 2026-08-05 | All | Various | Completed | Normal (rest of Aug 5) |
| 769 | 2026-08-07 | 1 | 06:45 | Completed | **Breakdown + Cancelled** — Shuttle 3 broke down, no replacement, trip partially served |
| 770–856 | 2026-08-07 | All | Various | Completed | Normal (rest of Aug 7) |
| 681–768 | 2026-08-06 | All | All | Completed | Normal operations |
| 857–920 | 2026-08-08 | All | All | Completed | Normal Saturday operations |
| 921–1096 | Aug 11–12 | All | All | Completed | Normal operations |

**All trips in this seed are Completed** (they're all in the past). The special scenarios show different *journeys* a trip took to reach Completed status (breakdown events, waitlist promotions, etc.).

### Timestamp fields for Completed trips:

For a trip with DepartureTime `T` on date `D`:
- `LockedAt` = `D` at `T - 60min` UTC (SAST departure - 1hr - 2hr UTC offset = T - 3hrs UTC... wait)

**Timezone clarification:** DepartureTime is in SAST. UTC = SAST - 2hrs.
- SAST 06:45 = UTC 04:45
- `LockedAt` = TripDate at (DepartureTime_SAST - 60min) converted to UTC = TripDate at (DepartureTime - 01:00 - 02:00) UTC
  - e.g., 06:45 SAST departure → locked at 05:45 SAST → `LockedAt` = 03:45 UTC on TripDate
- `InTransitAt` = DepartureTime in UTC = DepartureTime_SAST - 2hrs
  - e.g., 06:45 SAST → `InTransitAt` = 04:45 UTC
- `CompletedAt` = DepartureTime + ~45min (estimated transit duration) converted to UTC
  - e.g., 06:45 SAST → completed ~07:30 SAST → `CompletedAt` = 05:30 UTC

### Sample explicit rows (key scenario trips):

| TripInstanceId | ScheduleTimeId | StopId | TripDate | Status | LockedAt | InTransitAt | CompletedAt |
|---|---|---|---|---|---|---|---|
| 1 | 1 | 1 | 2026-07-28 | Completed | 2026-07-28T03:45:00Z | 2026-07-28T04:45:00Z | 2026-07-28T05:30:00Z |
| 2 | 2 | 1 | 2026-07-28 | Completed | 2026-07-28T04:45:00Z | 2026-07-28T05:45:00Z | 2026-07-28T06:30:00Z |
| 12 | 20 | 2 | 2026-07-28 | Completed | 2026-07-28T03:45:00Z | 2026-07-28T04:45:00Z | 2026-07-28T05:30:00Z |
| 505 | 1 | 1 | 2026-08-04 | Completed | 2026-08-04T03:45:00Z | 2026-08-04T04:45:00Z | 2026-08-04T05:30:00Z |
| 593 | 1 | 1 | 2026-08-05 | Completed | 2026-08-05T03:45:00Z | 2026-08-05T04:45:00Z | 2026-08-05T05:30:00Z |
| 769 | 1 | 1 | 2026-08-07 | Completed | 2026-08-07T03:45:00Z | 2026-08-07T04:45:00Z | 2026-08-07T05:30:00Z |

### Generation rule for all other trips:

All 1,096 trips follow the pattern above. Each trip's fields are deterministic from its position:
- `ScheduleTimeId` = derived from stop and timeslot position
- `StopId` = derived from position within day
- `TripDate` = the day's date
- `Status` = "Completed"
- `LockedAt` = TripDate + (DepartureTime_SAST - 01:00:00) - 02:00:00 (UTC)
- `InTransitAt` = TripDate + DepartureTime_SAST - 02:00:00 (UTC)
- `CompletedAt` = TripDate + DepartureTime_SAST + 00:45:00 - 02:00:00 (UTC)

---

## 15. Trip Shuttle Allocations

Every trip gets the default convoy roster from StopShuttleDefaults (Section 6). Same shuttles, same order positions.

**Total allocations:** Each trip gets 2 or 3 shuttles depending on stop.
- Stops 1,2,3: 3 allocations per trip
- Stops 4,5,6,7,8: 2 allocations per trip
- Per weekday: (3 stops × 11 timeslots × 3 shuttles) + (5 stops × 11 timeslots × 2 shuttles) = 99 + 110 = 209
- Per Saturday: (3 stops × 8 timeslots × 3 shuttles) + (5 stops × 8 timeslots × 2 shuttles) = 72 + 80 = 152
- Total: (11 weekdays × 209) + (2 Saturdays × 152) = 2,299 + 304 = **2,603 allocation records**

### Allocation rule:

For TripInstanceId `T` at StopId `S`:
- Look up StopShuttleDefaults for StopId = S
- Create one TripShuttleAllocation per default, preserving OrderPosition and ShuttleId
- `IsOperational` = TRUE (except for breakdown scenarios)
- `AssignedAt` = trip's LockedAt timestamp (shuttles confirmed at lock time)

### Exception allocations (breakdown scenarios):

**Trip 505 (Aug 4, Stop 1, 06:45) — Breakdown + Replacement:**

| AllocationId | TripInstanceId | ShuttleId | OrderPosition | IsOperational | AssignedAt |
|---|---|---|---|---|---|
| (computed) | 505 | 1 | 1 | TRUE | 2026-08-04T03:45:00Z |
| (computed) | 505 | 2 | 2 | FALSE | 2026-08-04T03:45:00Z |
| (computed) | 505 | 3 | 3 | TRUE | 2026-08-04T03:45:00Z |
| (computed) | 505 | 11 | 4 | TRUE | 2026-08-04T04:50:00Z |

Shuttle 2 was flagged as broken down (IsOperational=FALSE). Shuttle 11 was assigned as replacement (position 4, operational). Admin resolved by adding replacement.

**Trip 769 (Aug 7, Stop 1, 06:45) — Breakdown + Cancelled:**

| AllocationId | TripInstanceId | ShuttleId | OrderPosition | IsOperational | AssignedAt |
|---|---|---|---|---|---|
| (computed) | 769 | 1 | 1 | TRUE | 2026-08-07T03:45:00Z |
| (computed) | 769 | 2 | 2 | TRUE | 2026-08-07T03:45:00Z |
| (computed) | 769 | 3 | 3 | FALSE | 2026-08-07T03:45:00Z |

Shuttle 3 flagged as broken. No replacement assigned. Trip still completed (other shuttles served remaining passengers).

### AllocationId assignment:

AllocationIds are assigned sequentially across all trips in TripInstanceId order. For the implementation, use:
- Allocations for Trip 1 start at AllocationId 1
- Each trip adds 2 or 3 allocations
- Trip 505 adds 4 allocations (3 original + 1 replacement)

Approximate AllocationId for scenario trips:
- Trip 505 allocations: ~1156–1159 (but exact IDs depend on sequential assignment)
- Trip 769 allocations: ~1768–1770

For seed implementation, sequential IDs are computed programmatically.

---

## 16. Bookings

### Booking volume design:

To make dashboards representative, each trip gets a realistic number of bookings based on time of day:

| DepartureTime | Direction | Typical load (% of capacity) | Description |
|---|---|---|---|
| 06:45 | ToCampus | 85–95% | Morning rush, highest demand |
| 07:45 | ToCampus | 70–80% | Second wave |
| 08:45 | ToCampus | 50–60% | Late arrivals |
| 09:45 | ToCampus | 30–40% | Light |
| 12:30 | FromCampus | 40–50% | Lunch break departures |
| 14:30 | FromCampus | 55–65% | Afternoon wave |
| 16:00 | FromCampus | 80–90% | Peak departure |
| 17:30 | FromCampus | 70–80% | Evening rush |
| 18:30 | FromCampus | 45–55% | Late evening |
| 20:30 | FromCampus | 25–35% | Night |
| 22:00 | FromCampus | 15–20% | Last shuttle |

Saturday loads are ~60% of equivalent weekday loads.

### Student-to-Stop affinity:

Students primarily use the stop closest to their residence. Distribution across 8 stops:

| StopId | Primary Students (by StudentId) | Count |
|---|---|---|
| 1 (Summerstrand) | 1, 2, 3, 16, 17, 18, 51, 52 | 8 |
| 2 (Humewood) | 4, 5, 6, 19, 20, 21, 53, 54 | 8 |
| 3 (Newton Park) | 7, 8, 9, 22, 23, 24, 55, 56 | 8 |
| 4 (Walmer Heights) | 10, 11, 13, 25, 26, 27, 57 | 7 |
| 5 (Lorraine) | 12, 14, 35, 28, 29, 30, 58 | 7 |
| 6 (Bluewater Bay) | 15, 31, 32, 33, 59 | 5 |
| 7 (Framesby) | 34, 36, 37, 38, 60 | 5 |
| 8 (Greenacres) | 39, 40, 41, 42, 43, 44, 45, 46 | 8 |

Inactive students (47–50) have NO bookings in the seed window (they were struck out before Jul 28).

### Booking states represented:

| BookingStatus | BoardedStatus | BookingType | ShuttleId | Scenario |
|---|---|---|---|---|
| Confirmed | Boarded | Standard | Set | Normal successful trip |
| Confirmed | Boarded | Priority | Set | Priority auto-booking, checked in |
| Confirmed | NoShow | Standard | NULL | Student didn't show up |
| Confirmed | NoShow | Priority | NULL | Priority student didn't show |
| Confirmed | Pending | Standard | NULL | Exists on completed trips with mid-boarding breakdown (capacity dropped, student couldn't board) |
| WaitingList | Pending | Standard | NULL | Student never got promoted (trip was full) |
| Cancelled | Pending | Standard | NULL | Student cancelled before lock |

### Detailed booking scenarios:

#### Scenario A: Normal completed trip (majority of trips)
Most bookings are `Confirmed / Boarded / Standard` with ShuttleId assigned.
~5% are `Confirmed / NoShow` (student didn't show).

#### Scenario B: Trip 593 (Aug 5, Stop 1, 06:45) — Waitlist overflow
- Convoy capacity: 79 seats (S1=22, S2=35, S3=22)
- 79 Confirmed bookings (filled to capacity) — includes students from neighboring stops who also use Summerstrand Village
- 4 additional bookings initially created as `WaitingList`
- 2 cancellations occurred before lock → 2 waitlist students promoted to `Confirmed` (Students 45, 46 — FIFO)
- Final state: 79 Confirmed (77 Boarded + 2 NoShow), 2 WaitingList/Pending (Students 42, 43 — never promoted), 2 Cancelled (Students 39, 40)

#### Scenario C: Trip 505 (Aug 4, Stop 1, 06:45) — Breakdown with replacement
- S2 (35 seats) broke down → capacity dropped to 44 (S1=22 + S3=22)
- S11 (35 seats) assigned as replacement → capacity restored to 79
- All boarded students have ShuttleId set to S1, S3, or S11 (not S2)
- 70 Confirmed/Boarded, 3 NoShow

#### Scenario D: Trip 769 (Aug 7, Stop 1, 06:45) — Breakdown, no replacement
- S3 (22 seats) broke down → effective capacity 57 (S1=22 + S2=35)
- 57 Confirmed/Boarded, 5 NoShow, students who were assigned to S3 in queue were reassigned to S1/S2 (since assignment happens at scan time and S3 wasn't available)
- Trip still completed with reduced capacity

### Explicit booking records for scenario trips:

#### Trip 593 — Waitlist overflow scenario (83 total booking records)

**Breakdown: 79 Confirmed + 2 WaitingList + 2 Cancelled = 83 total.** The 2 promoted students (45, 46) are counted within the 79 Confirmed. The 2 cancelled students (39, 40) freed their slots which the promoted students filled.

| BookingId | StudentId | TripInstanceId | BookingStatus | BookingType | BoardedStatus | QrCodeToken | ShuttleId | BoardedAt | CreatedAt |
|---|---|---|---|---|---|---|---|---|---|
| (base+1) | 3 | 593 | Confirmed | Priority | Boarded | qr_593_s3_pri | 1 | 2026-08-05T04:35:00Z | 2026-08-01T22:00:00Z |
| (base+2) | 1 | 593 | Confirmed | Standard | Boarded | qr_593_s1 | 1 | 2026-08-05T04:37:00Z | 2026-08-03T06:10:00Z |
| (base+3) | 2 | 593 | Confirmed | Standard | Boarded | qr_593_s2 | 1 | 2026-08-05T04:38:00Z | 2026-08-03T06:15:00Z |
| (base+4) | 16 | 593 | Confirmed | Standard | Boarded | qr_593_s16 | 2 | 2026-08-05T04:36:00Z | 2026-08-03T06:20:00Z |
| (base+5) | 17 | 593 | Confirmed | Standard | Boarded | qr_593_s17 | 2 | 2026-08-05T04:39:00Z | 2026-08-03T06:25:00Z |
| (base+6) | 18 | 593 | Confirmed | Standard | NoShow | qr_593_s18 | NULL | NULL | 2026-08-03T06:30:00Z |
| (base+7) | 51 | 593 | Confirmed | Standard | NoShow | qr_593_s51 | NULL | NULL | 2026-08-03T07:00:00Z |
| (base+8) | 52 | 593 | Confirmed | Standard | Boarded | qr_593_s52 | 3 | 2026-08-05T04:40:00Z | 2026-08-03T07:05:00Z |
| ... | (remaining confirmed students fill shuttle positions sequentially) | ... | ... | ... | ... | ... | ... | ... | ... |
| (base+77) | 44 | 593 | Confirmed | Standard | Boarded | qr_593_s44 | 3 | 2026-08-05T04:42:00Z | 2026-08-03T14:00:00Z |
| (base+78) | 45 | 593 | Confirmed | Standard | Boarded | qr_593_s45_promoted | 1 | 2026-08-05T04:44:00Z | 2026-08-03T14:30:00Z |
| (base+79) | 46 | 593 | Confirmed | Standard | Boarded | qr_593_s46_promoted | 2 | 2026-08-05T04:43:00Z | 2026-08-03T14:45:00Z |
| (base+80) | 43 | 593 | WaitingList | Standard | Pending | qr_593_s43_wl | NULL | NULL | 2026-08-03T15:00:00Z |
| (base+81) | 42 | 593 | WaitingList | Standard | Pending | qr_593_s42_wl | NULL | NULL | 2026-08-03T15:15:00Z |
| (base+82) | 39 | 593 | Cancelled | Standard | Pending | qr_593_s39_cx | NULL | NULL | 2026-08-03T08:00:00Z |
| (base+83) | 40 | 593 | Cancelled | Standard | Pending | qr_593_s40_cx | NULL | NULL | 2026-08-03T09:00:00Z |

**Notes on Trip 593:**
- Student 3 (priority) was auto-booked by rolling injection job (CreatedAt = midnight SAST 3 days before trip = Aug 2 00:00 SAST = Aug 1 22:00 UTC)
- Students 39 and 40 cancelled before lock time
- Students 45 and 46 were promoted from WaitingList → Confirmed at lock time (FIFO by CreatedAt)
- Students 42 and 43 remained on waitlist (booked after 45 and 46, so lower priority in FIFO)
- Boarded students get ShuttleId assigned at scan time — front shuttle (S1, position 1) fills first, then S2, then S3.
- BoardedAt times are ~35min to ~5min before departure (scanning window opens 35min before)

### Booking generation pattern for normal trips:

For a normal weekday 06:45 trip at Stop 1 (capacity 79):
- ~67 bookings (85% load)
- 1 Priority booking (Student 3 on Mon/Wed/Fri)
- ~63 Confirmed/Boarded
- ~3 Confirmed/NoShow
- 0 WaitingList (not at capacity)
- 0–1 Cancelled

**QrCodeToken format:** `qr_{tripId}_s{studentId}_{suffix}`
- suffix: empty for normal, `pri` for priority, `wl` for waitlist, `cx` for cancelled

**CreatedAt for bookings:**
- Priority bookings: midnight SAST on (TripDate - 3 days). In UTC = (TripDate - 4 days) at 22:00:00 UTC. Example: Trip on Aug 5 → CreatedAt = Aug 1 at 22:00 UTC.
- Standard bookings: random times 1–3 days before trip, during waking hours (06:00–22:00 SAST)
- Cancelled bookings: created normally, then cancelled before LockedAt

**BoardedAt for boarded passengers:**
- Scanning window: 35min before departure
- BoardedAt values spread across [DepartureTime - 35min, DepartureTime - 2min] (SAST, stored as UTC)
- For 06:45 SAST departure: BoardedAt range = 06:10–06:43 SAST = 04:10–04:43 UTC

**ShuttleId assignment for boarded passengers:**
- Fill front shuttle first (OrderPosition 1), then next, etc.
- For Stop 1: first 22 boarded → ShuttleId 1, next 35 → ShuttleId 2, next 22 → ShuttleId 3

### Total estimated bookings:

Given the load percentages and 1,096 trips:
- Average ~35 bookings per trip (weighted by time-of-day load and stop capacity)
- Estimated total: **~38,000 booking records**

For the seed implementation, bookings are generated programmatically using the rules above. The document defines the rules; the C# SeedData class implements the generation.

---

## 17. Breakdown Logs

| BreakdownLogId | TripInstanceId | ShuttleId | VendorCompanyName | FlaggedAt | FlaggedByMarshalId | ResolutionType | ReplacementShuttleId | ResolvedAt | ResolvedByAdminId |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 505 | 2 | Bay Transit Corp | 2026-08-04T04:30:00Z | 4 | Replaced | 11 | 2026-08-04T04:50:00Z | 1 |
| 2 | 769 | 3 | Bay Transit Corp | 2026-08-07T04:28:00Z | 4 | Cancelled | NULL | 2026-08-07T04:55:00Z | 2 |

**Narrative:**
- **Breakdown 1 (Trip 505, Aug 4):** Marshal Thulani Ngcobo (MarshalId=4, assigned to Stop 1) flagged Shuttle 2 (EC 256-491, Bay Transit Corp) at 06:30 SAST. Admin Thabo resolved 20 minutes later by assigning Shuttle 11 (EC 331-058, Bay Transit Corp) as replacement.
- **Breakdown 2 (Trip 769, Aug 7):** Same marshal flagged Shuttle 3 (EC 389-720, Bay Transit Corp) at 06:28 SAST. Admin Nomsa resolved as Cancelled (no replacement available). Trip completed with reduced convoy.

**Analytics impact:** Bay Transit Corp has 2 breakdown incidents in this period. Other vendors have 0. This shows in admin analytics dashboard.

---

## 18. Trip Notes

| TripNoteId | TripInstanceId | NoteType | Body | CreatedByAdminId | CreatedAt | IsResolutionAction |
|---|---|---|---|---|---|---|
| 1 | 505 | Incident | Marshal reported Shuttle EC 256-491 failed to arrive at Summerstrand Village stop. Driver reported engine failure en route. | 1 | 2026-08-04T04:35:00Z | FALSE |
| 2 | 505 | Replacement | Replacement shuttle EC 331-058 dispatched from depot. ETA 15 minutes. Vendor confirmed availability. | 1 | 2026-08-04T04:50:00Z | TRUE |
| 3 | 769 | Incident | Marshal reported Shuttle EC 389-720 mechanical failure at stop. Tyre blowout. | 2 | 2026-08-07T04:30:00Z | FALSE |
| 4 | 769 | General | No replacement available from Bay Transit Corp. Remaining shuttles (S1, S2) absorbed passengers. Capacity reduced but manageable. | 2 | 2026-08-07T04:55:00Z | TRUE |
| 5 | 100 | General | High passenger volume noted. All shuttles at near capacity. Consider adding fourth shuttle for this route on Wednesdays. | 1 | 2026-07-29T05:30:00Z | FALSE |
| 6 | 417 | NoShow | Unusually high no-show rate (8 students). May correlate with reported campus event schedule change. | 2 | 2026-08-03T05:45:00Z | FALSE |

---

## 19. Notifications

Notifications are sent to students for various system events. All notifications below reference completed past trips.

### Notification types represented:

1. **Trip Locked** — Sent at T-60min to all confirmed passengers
2. **Waitlist Promoted** — Sent when a waitlisted student gets promoted to Confirmed
3. **Breakdown Resolved (Replacement)** — Sent after admin resolves with replacement shuttle
4. **Breakdown Resolved (Cancelled)** — Sent when trip has no replacement shuttle
5. **Booking Confirmed** — Sent when standard booking is confirmed
6. **Priority Booking Created** — Sent when rolling injection creates priority booking

| NotificationId | StudentId | Title | Message | IsRead | RelatedTripInstanceId | CreatedAt |
|---|---|---|---|---|---|---|
| 1 | 6 | Priority Booking Confirmed | Your priority seat for Humewood → South Campus on 28 Jul (06:45) has been automatically reserved. | TRUE | 12 | 2026-07-24T22:00:00Z |
| 2 | 20 | Priority Booking Confirmed | Your priority seat for Newton Park → South Campus on 28 Jul (06:45) has been automatically reserved. | TRUE | 23 | 2026-07-24T22:00:00Z |
| 3 | 3 | Priority Booking Confirmed | Your priority seat for Summerstrand Village → South Campus on 29 Jul (06:45) has been automatically reserved. | TRUE | 89 | 2026-07-25T22:00:00Z |
| 4 | 1 | Booking Confirmed | Your seat for Summerstrand Village → South Campus on 28 Jul (06:45) is confirmed. Show your QR code to the marshal when boarding. | TRUE | 1 | 2026-07-26T08:15:00Z |
| 5 | 2 | Booking Confirmed | Your seat for Summerstrand Village → South Campus on 28 Jul (06:45) is confirmed. Show your QR code to the marshal when boarding. | TRUE | 1 | 2026-07-26T09:30:00Z |
| 6 | 1 | Trip Locked | Your trip from Summerstrand Village (06:45, 28 Jul) is now locked. No changes can be made. Please be at the stop 10 minutes early. | TRUE | 1 | 2026-07-28T03:45:00Z |
| 7 | 2 | Trip Locked | Your trip from Summerstrand Village (06:45, 28 Jul) is now locked. No changes can be made. Please be at the stop 10 minutes early. | TRUE | 1 | 2026-07-28T03:45:00Z |
| 8 | 16 | Trip Locked | Your trip from Summerstrand Village (06:45, 28 Jul) is now locked. No changes can be made. Please be at the stop 10 minutes early. | TRUE | 1 | 2026-07-28T03:45:00Z |
| 9 | 1 | Shuttle Update | Shuttle EC 256-491 on your trip (Summerstrand Village, 06:45, 04 Aug) experienced a breakdown. A replacement shuttle EC 331-058 has been dispatched. Your trip will proceed. | TRUE | 505 | 2026-08-04T04:50:00Z |
| 10 | 2 | Shuttle Update | Shuttle EC 256-491 on your trip (Summerstrand Village, 06:45, 04 Aug) experienced a breakdown. A replacement shuttle EC 331-058 has been dispatched. Your trip will proceed. | TRUE | 505 | 2026-08-04T04:50:00Z |
| 11 | 3 | Shuttle Update | Shuttle EC 256-491 on your trip (Summerstrand Village, 06:45, 04 Aug) experienced a breakdown. A replacement shuttle EC 331-058 has been dispatched. Your trip will proceed. | TRUE | 505 | 2026-08-04T04:50:00Z |
| 12 | 16 | Shuttle Update | Shuttle EC 256-491 on your trip (Summerstrand Village, 06:45, 04 Aug) experienced a breakdown. A replacement shuttle EC 331-058 has been dispatched. Your trip will proceed. | TRUE | 505 | 2026-08-04T04:50:00Z |
| 13 | 1 | Shuttle Cancelled | Shuttle EC 389-720 on your trip (Summerstrand Village, 06:45, 07 Aug) broke down and no replacement is available. Remaining shuttles will serve all passengers. Expect possible delays. | TRUE | 769 | 2026-08-07T04:55:00Z |
| 14 | 2 | Shuttle Cancelled | Shuttle EC 389-720 on your trip (Summerstrand Village, 06:45, 07 Aug) broke down and no replacement is available. Remaining shuttles will serve all passengers. Expect possible delays. | TRUE | 769 | 2026-08-07T04:55:00Z |
| 15 | 3 | Shuttle Cancelled | Shuttle EC 389-720 on your trip (Summerstrand Village, 06:45, 07 Aug) broke down and no replacement is available. Remaining shuttles will serve all passengers. Expect possible delays. | TRUE | 769 | 2026-08-07T04:55:00Z |
| 16 | 45 | Waitlist Promoted | Great news! A spot opened up for your trip from Summerstrand Village (06:45, 05 Aug). Your booking is now confirmed. | TRUE | 593 | 2026-08-05T03:45:00Z |
| 17 | 46 | Waitlist Promoted | Great news! A spot opened up for your trip from Summerstrand Village (06:45, 05 Aug). Your booking is now confirmed. | FALSE | 593 | 2026-08-05T03:45:00Z |
| 18 | 6 | Priority Booking Confirmed | Your priority seat for Humewood → South Campus on 04 Aug (06:45) has been automatically reserved. | TRUE | 516 | 2026-07-31T22:00:00Z |
| 19 | 13 | Priority Booking Confirmed | Your priority seat for Walmer Heights → South Campus on 03 Aug (06:45) has been automatically reserved. | TRUE | 450 | 2026-07-30T22:00:00Z |
| 20 | 35 | Priority Booking Confirmed | Your priority seat for Lorraine → South Campus on 03 Aug (06:45) has been automatically reserved. | TRUE | 461 | 2026-07-30T22:00:00Z |

**Note:** In full implementation, Trip Locked notifications would be sent to ALL confirmed passengers on each trip at lock time. The above shows representative samples. The seed implementation should generate these programmatically for all bookings.

### Notification generation rules for full seed:

1. **Priority Booking Created**: One per priority booking, CreatedAt = booking CreatedAt (rolling injection time)
2. **Booking Confirmed**: One per standard booking when created, CreatedAt = booking CreatedAt
3. **Trip Locked**: One per confirmed passenger at trip lock time, CreatedAt = trip LockedAt
4. **Waitlist Promoted**: One per promoted booking, CreatedAt = trip LockedAt (promotion runs at lock time)
5. **Breakdown Resolved**: One per confirmed passenger on affected trip, CreatedAt = BreakdownLog.ResolvedAt
6. **IsRead**: All notifications older than 3 days = TRUE, recent ones = mix of TRUE/FALSE

---

## 20. Reports

| ReportId | StudentId | Category | Description | Status | TripInstanceId | BookingId | AdminNotes | CreatedAt | UpdatedAt |
|---|---|---|---|---|---|---|---|---|---|
| 1 | 5 | DriverBehavior | The driver of shuttle EC 628-945 was driving recklessly on Marine Drive, exceeding speed limit and overtaking dangerously. Several passengers felt unsafe. | Resolved | 12 | NULL | Incident reported to Coastal Movers management. Driver has been given a written warning. Vendor confirmed corrective action. | 2026-07-28T10:30:00Z | 2026-07-30T14:00:00Z |
| 2 | 21 | VehicleQuality | Shuttle EC 714-382 had broken air conditioning. The vehicle was extremely hot and uncomfortable during the afternoon trip. Windows were also difficult to open. | Resolved | 107 | NULL | Reported to Coastal Movers. Vehicle pulled from service for maintenance on 2026-08-01. Returned to service 2026-08-03 after AC repair confirmed. | 2026-07-30T12:45:00Z | 2026-08-01T09:00:00Z |
| 3 | 14 | TimetableIssue | The 14:30 shuttle from South Campus consistently arrives 10-15 minutes late. This has happened three days in a row. Students are missing connections. | UnderReview | 314 | NULL | Investigating with Bay Transit Corp dispatch. Requested GPS logs for the past week. | 2026-07-31T13:00:00Z | 2026-08-02T11:00:00Z |
| 4 | 22 | Other | The shelter at Newton Park stop has a broken bench. Students have to stand while waiting for the shuttle in the rain. | Submitted | NULL | NULL | NULL | 2026-08-03T07:30:00Z | NULL |
| 5 | 7 | DriverBehavior | Driver of shuttle EC 837-256 was on his phone while driving. Very dangerous and irresponsible. | UnderReview | 439 | NULL | Report forwarded to Metro Link SA on 2026-08-05. Awaiting their investigation response. | 2026-08-04T09:15:00Z | 2026-08-05T10:00:00Z |
| 6 | 16 | VehicleQuality | Shuttle EC 256-491 has a strong diesel smell inside. Multiple passengers complained of nausea during the morning trip. | Resolved | 593 | NULL | Bay Transit Corp inspected the vehicle. Exhaust seal replaced. Vehicle cleared for service after inspection on 2026-08-08. | 2026-08-05T06:00:00Z | 2026-08-08T15:30:00Z |
| 7 | 41 | TimetableIssue | Saturday 08:45 shuttle from Greenacres never arrived. Waited 30 minutes then had to arrange alternative transport. | Submitted | 915 | NULL | NULL | 2026-08-08T08:00:00Z | NULL |
| 8 | 55 | Other | Marshal at Newton Park was rude to a student who arrived 2 minutes after departure time. The student was told to "find your own way" in front of other passengers. | UnderReview | 703 | NULL | HR department notified. Reviewing CCTV footage from stop. Marshal will be counselled. | 2026-08-06T07:00:00Z | 2026-08-09T10:00:00Z |

**Report summary:**
- DriverBehavior: 2 (1 Resolved, 1 UnderReview)
- VehicleQuality: 2 (2 Resolved)
- TimetableIssue: 2 (1 UnderReview, 1 Submitted)
- Other: 2 (1 Submitted, 1 UnderReview)
- Statuses covered: Submitted (2), UnderReview (3), Resolved (3)

---

## 21. Scenario Narratives (for reference)

### Scenario 1: Normal day (Jul 28, first operational day)
- Rolling injection job created all trips 3 days earlier (Jul 25)
- Priority bookings auto-created for the target date's day-of-week (Tuesday): Student 6 (Tue/Thu at Stop 2), Student 20 (Mon-Thu at Stop 3)
- Students booked their seats over the following days
- At T-60, trips locked, waitlist promotion ran (no waitlisted students on this day)
- Marshals scanned QR codes during the 35-min boarding window
- All trips completed normally

### Scenario 2: Breakdown with replacement (Aug 4, Trip 505)
- 06:30 SAST: Marshal Thulani notices Shuttle 2 hasn't arrived at Summerstrand stop
- 06:30 SAST: Marshal flags the shuttle via app
- System: TripShuttleAllocation for S2 → IsOperational = FALSE, TripInstance stays as-is (scanning already open)
- 06:35 SAST: Admin Thabo sees the incident, contacts Bay Transit Corp
- 06:50 SAST: Admin resolves — assigns Shuttle 11 as replacement, creates new allocation
- System: TripShuttleAllocation for S11 created (IsOperational=TRUE), BreakdownLog.ResolutionType = 'Replaced'
- System: Push notification sent to all confirmed passengers on Trip 505
- Boarding continues with S1, S3, and S11

### Scenario 3: Waitlist overflow with promotion (Aug 5, Trip 593)
- Trip created Aug 2 via rolling injection (3 days ahead)
- Priority booking for Student 3 auto-created
- Over next days, 83 students attempt to book (79 Confirmed, 4 WaitingList because capacity = 79)
- Students 39, 40 cancel their bookings before lock
- At T-60 (05:45 SAST = 03:45 UTC): Trip locks
- Waitlist promotion runs: Students 45, 46 promoted from WaitingList → Confirmed (FIFO by CreatedAt — they booked before students 42, 43)
- Notifications sent to promoted students
- Final: 79 Confirmed, 2 still on WaitingList (Students 42, 43), 2 Cancelled (Students 39, 40)
- All 79 confirmed students either boarded or no-showed

### Scenario 4: Breakdown with cancellation (Aug 7, Trip 769)
- 06:28 SAST: Marshal flags Shuttle 3 — tyre blowout at stop
- System: Allocation for S3 → IsOperational = FALSE
- Effective capacity drops from 79 to 57 (S1=22, S2=35)
- 06:55 SAST: Admin Nomsa resolves as Cancelled (no replacement available)
- System: BreakdownLog.ResolutionType = 'Cancelled', notification sent to passengers
- Trip had ~67 confirmed bookings. With only 57 seats now available:
  - 55 students boarded successfully into S1 and S2 (Confirmed/Boarded)
  - 7 students marked as NoShow (mix of genuine no-shows + some who arrived after capacity was reached)
  - 5 students remain as Confirmed/Pending (arrived at the stop but couldn't board due to reduced capacity; received the breakdown notification)
- This edge case demonstrates that BoardedStatus = 'Pending' CAN exist on completed trips when capacity drops mid-boarding

---

## 22. Implementation Notes

### For the C# seed implementation:

1. **Do NOT enumerate all 38,000+ bookings in this document.** Use programmatic generation based on the rules in Section 16.
2. **Trip generation** follows the deterministic formula in Section 14.
3. **Allocation generation** follows StopShuttleDefaults, with exceptions for breakdown trips.
4. **Booking generation** uses:
   - Student-to-stop affinity table
   - Load percentage by time slot
   - Random but deterministic distribution (use a seeded Random for reproducibility)
   - Priority bookings injected first
5. **Notification generation** follows the rules in Section 19.
6. **All explicit scenario data** (breakdowns, reports, scenario bookings) should be hardcoded.
7. **QrCodeToken** generation: Use deterministic GUIDs or formatted strings for reproducibility.

### Key referential integrity constraints:

- Every Booking.TripInstanceId must reference a valid TripInstance
- Every Booking.ShuttleId (when set) must reference a shuttle allocated to that trip
- Every BreakdownLog.ShuttleId must be allocated to the referenced trip
- Priority bookings only exist for days matching the PrioritySchedule.DayOfWeek
- No bookings exist for Aug 10 (CalendarException — no trips generated)
- Inactive students (47–50) have no bookings in the Jul 28–Aug 12 window
- BoardedAt is only set when BoardedStatus = 'Boarded'
- ShuttleId on booking is only set when BoardedStatus = 'Boarded'

---

## 23. Summary Statistics

| Entity | Count |
|---|---|
| Admins | 2 |
| Campuses | 3 |
| Stops | 8 |
| Vendors | 3 |
| Shuttles | 19 |
| StopShuttleDefaults | 19 |
| Marshals | 11 |
| Students | 60 (56 active, 4 inactive) |
| ScheduleTimes | 152 |
| ScheduleTimeCampuses | 152 |
| CalendarExceptions | 18 |
| PrioritySchedules | 28 |
| DeviceRegistrations | 31 |
| TripInstances | 1,096 |
| TripShuttleAllocations | ~2,605 |
| Bookings | ~38,000 (generated programmatically) |
| BreakdownLogs | 2 |
| TripNotes | 6 |
| Notifications | ~120,000+ (generated programmatically — trip locked notifications alone = ~38,000) |
| Reports | 8 |

---

## 24. Implementation Recommendations

### Notification volume management

The full seed generates ~120,000+ notification records (38,000 Trip Locked + 38,000 Booking Confirmed + priority/breakdown/promotion notifications). This is realistic but heavy for development environments.

**Recommended approach:**
- **Full notifications** only for the 3 scenario trips (505, 593, 769) plus a sample of ~50 normal trips spread across different days and stops
- **Skip Trip Locked and Booking Confirmed notifications** for the remaining ~1,040 trips
- This brings the notification count down to ~2,000–3,000 records — enough for dashboard pagination, filtering, and read/unread state testing without bloating the DB
- If full-volume testing is needed later (e.g., performance testing), add a flag like `--full-notifications` to the seed runner

### Booking generation seeded randomness

Use `new Random(42)` (or any fixed seed) for all random decisions in booking generation:
- Which students book which trips
- CreatedAt time offsets
- BoardedAt time distribution within the scanning window
- Which students are NoShow (~5% rate)

This ensures the seed is **reproducible** — running it twice produces the same database state. Critical for test assertions and for syncing between developers.

### Seed execution strategy

Given the volume (~38,000 bookings + ~2,600 allocations + 1,096 trips), do NOT use EF Core `HasData()` for the runtime entities. Instead:

1. **Static seed via `HasData()`**: Admins, Campuses, Stops, Vendors, Shuttles, StopShuttleDefaults, Marshals, Students, ScheduleTimes, ScheduleTimeCampuses, CalendarExceptions, PrioritySchedules, DeviceRegistrations (small, config-level data)
2. **Programmatic seed via `IHostedService` or a dedicated seed command**: TripInstances, TripShuttleAllocations, Bookings, Notifications, BreakdownLogs, TripNotes, Reports (large, runtime data generated by algorithm)

Reason: EF Core migrations with 38,000+ `HasData()` rows generate enormous migration files and are extremely slow to apply. A programmatic seeder using `context.BulkInsert()` or batched `SaveChanges()` is far more practical.

**Suggested pattern:**
```csharp
// In Program.cs or a dedicated SeedCommand
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<HistoricalDataSeeder>();
    await seeder.SeedAsync(); // Only runs if TripInstances table is empty
}
```

### Dashboard analytics coverage

The seed data produces the following analytics scenarios for the admin portal:

| Metric | Expected values |
|---|---|
| Total trips (period) | 1,096 |
| Average load factor | ~55% (weighted across all timeslots) |
| Peak load factor (06:45 ToCampus) | ~90% |
| Breakdown incidents | 2 (both Bay Transit Corp) |
| Vendor reliability ranking | Coastal Movers & Metro Link SA: 100%, Bay Transit Corp: 99.8% (2 incidents / ~770 allocations) |
| No-show rate | ~5% overall |
| Busiest stop | Stop 1 (Summerstrand) — 3-shuttle convoy, highest load |
| Quietest stop | Stop 7 (Framesby) — 2-shuttle convoy, 50 seat capacity |
| Reports by vendor | Coastal Movers: 2, Metro Link SA: 2, Bay Transit Corp: 2, Non-vendor: 2 |
| Student activity | 56 active bookers, 4 struck out |
| Priority students served | 5 (with recurring auto-bookings) |
| Waitlist events | 1 trip with overflow (Trip 593) |

### Deprecation of firstSeed.md

This document (`seed.md`) fully replaces `MySpecGuide/firstSeed.md`. The old seed covered only configuration entities (Admins, Campuses, Stops, Students, Marshals, Shuttles, ScheduleTimes, CalendarExceptions, PrioritySchedules) with no runtime data.

**What this seed adds over the old one:**
- Vendors entity (extracted from Shuttle.VendorCompanyName)
- StopShuttleDefaults (convoy assignments)
- ScheduleTimeCampus (route junction)
- DeviceRegistrations
- 45 additional students (60 total vs 15)
- 9 additional shuttles (19 total vs 10)
- 4 inactive/struck-out students
- 2 additional priority students
- Full 2-week historical trip data (1,096 trips)
- Complete booking lifecycle coverage (all status combinations)
- Breakdown incidents with resolution paths
- Trip notes (admin operational documentation)
- Notifications (all system event types)
- Reports (all categories and statuses)
- Programmatic generation rules for high-volume data
- Scenario narratives documenting the "story" behind edge-case data

Once the new seed implementation is complete, `MySpecGuide/firstSeed.md` can be deleted.
