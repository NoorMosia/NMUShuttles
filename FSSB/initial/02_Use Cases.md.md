# 2. Functional Requirements & Use Case Mapping

## 2.1 Team Responsibilities Matrix

| Module Range | Lead Developer | Assigned Use Cases | Key Requirements & Lecturer Annotations |
| :--- | :--- | :--- | :--- |
| `A100 - A500` | Lusanda Ngubeni | View Notification (A100), View Booking (A200), Report Issue (A300), Cancel Booking (A400), Check Waiting List (A500) | Handle mark-as-read state in DB; allow report category selection (General, Marshal, Driver) |
| `B100 - B400` | Letlhogonolo Setati | Load Student Data (B100), Manage Routes (B200), View Demand Overview (B300), Manage Shuttle (B400) | Provide filtering/search options for long shuttle lists; add report criteria/export options |
| `C100 - C600` | Anele Sombo | Add Shuttle (C100), Update Shuttle (C200), Send Notifications (C300), Re-Allocate Shuttle (C400), Scan QR Code (C500), Verify Booking (C600) | Auto-generate Shuttle IDs; implement re-scan loop ($Alt: Red\ X \rightarrow Re\text{-}Scan$) on scan failure |
| `D100 - D600` | Tshepo Noor Mosia | Generate Confirmation (D100), Login (D200), Change Password (D300), Book Shuttle (D400), Check Capacity (D500), Place On Waiting List (D600) | Omit authentication from UCD under Perfect Technology Assumption; route full bookings to waitlist logic |

---

## 2.2 System Use Case Architecture

```mermaid
graph TD
    subgraph Student Actions
        A[Student] --> A200[View Booking]
        A --> A300[Report Issue]
        A --> A400[Cancel Booking]
        A --> D400[Book Shuttle]
    end

    subgraph Admin Actions
        B[Admin] --> B100[Load Student Data]
        B --> B200[Manage Routes]
        B --> B300[View Demand Overview]
        B --> B400[Manage Shuttle]
        B400 --> C100[Add Shuttle]
        B400 --> C200[Update Shuttle]
    end

    subgraph Marshal Actions
        C[Marshal] --> C500[Scan QR Code]
        C500 --> C600[Verify Booking]
    end

    subgraph System & Time Triggers
        D[Time Trigger] --> C300[Send Notifications]
        D --> C400[Re-Allocate Shuttle]
        D --> A500[Check Waiting List]
        
        D400 --> D500[Check Capacity]
        D500 -- Full --> D600[Place On Waiting List]
        D500 -- Available --> D100[Generate Confirmation]
    end