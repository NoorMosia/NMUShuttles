# 4. Interface Guidelines & Design System

## 4.1 Design System Constants
* **Primary Branding:** Nelson Mandela University (NMU) Theme[cite: 1]
* **Color Palette:**
  * Primary Accent Gold: `#F2A900`[cite: 1]
  * Deep Navy Blue: `#001F40`[cite: 1]
  * Background Pure White: `#FFFFFF`[cite: 1]
* **Typography:** `Poppins Regular`[cite: 1]
* **Layout Principles:** Clean, modern, high contrast for outdoor visibility[cite: 1].

---

## 4.2 Key Flow Requirements

### Marshal Verification Flow (`C500` / `C600`)
```mermaid
stateDiagram-v2
    [*] --> Scanning
    Scanning --> ValidBooking: QR Valid & Slot Matches
    Scanning --> InvalidBooking: QR Expired / Wrong Route

    state ValidBooking {
        [*] --> DisplayGreenTick
        DisplayGreenTick --> MarkAsBoarded
    }

    state InvalidBooking {
        [*] --> DisplayRedX
        DisplayRedX --> PromptRescan
        PromptRescan --> Scanning: Click 'RE-SCAN'
    }


    sequenceDiagram
    autonumber
    actor Student
    participant System
    participant DB

    Student->>System: Request Booking (D400)
    System->>DB: Check Capacity (D500)
    alt Capacity Available
        DB-->>System: Seats > 0
        System->>DB: Save Confirmed Booking
        System-->>Student: Return QR Code Ticket (D100)
    else Capacity Full
        DB-->>System: Seats == 0
        System->>DB: Add Student to WaitingList (D600)
        System-->>Student: Notify "Added to Waitlist"
    end
---

