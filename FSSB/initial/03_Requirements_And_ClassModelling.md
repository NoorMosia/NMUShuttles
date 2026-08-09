# 3. Data Requirements & Class Modeling

## 3.1 Domain Entity Specification

* **User:** `firstName`, `lastName`, `password`, `userType`[cite: 1]
* **Student (extends User):** `studentNo`, `flagCount`[cite: 1]
* **Admin (extends User):** `adminID`[cite: 1]
* **Marshal (extends User):** `marshalID`, `studentNo`, `stopID`[cite: 1]
* **Booking:** `bookingID`, `studentNo`, `shuttleID`, `departureTime`, `status`, `createdTime`, `listID`[cite: 1]
* **Notification:** `notificationID`, `studentNo`, `bookingID`, `description`, `read`[cite: 1]
* **WaitingList:** `listID`, `stopID`[cite: 1]
* **Stop:** `stopID`, `stopName`, `location`, `area`[cite: 1]
* **Route:** `routeID`, `departure`, `destination`[cite: 1]
* **RouteStop:** `routeID`, `stopID`[cite: 1]
* **ShuttleSchedule:** `scheduleID`, `adminID`, `driverID`, `routeID`[cite: 1]
* **Shuttle:** `shuttleID`, `capacity`, `routeID`, `driverName`, `status`[cite: 1]
* **Trip:** `tripID`, `scheduleID`, `shuttleID`[cite: 1]
* **IssueReport:** `reportID`, `studentNo`, `reportType`, `shuttleID`, `createdTime`, `status`, `description`[cite: 1]

---

## 3.2 Domain Class Diagram

```mermaid
classDiagram
    class User {
        +String firstName
        +String lastName
        +String password
        +String userType
    }

    class Student {
        +String studentNo
        +int flagCount
    }

    class Admin {
        +String adminID
    }

    class Marshal {
        +String marshalID
        +String studentNo
        +String stopID
    }

    class Booking {
        +String bookingID
        +String studentNo
        +String shuttleID
        +DateTime departureTime
        +String status
        +DateTime createdTime
        +String listID
    }

    class Shuttle {
        +String shuttleID
        +int capacity
        +String routeID
        +String driverName
        +String status
    }

    class Route {
        +String routeID
        +String departure
        +String destination
    }

    class Stop {
        +String stopID
        +String stopName
        +String location
        +String area
    }

    class IssueReport {
        +String reportID
        +String studentNo
        +String reportType
        +String shuttleID
        +DateTime createdTime
        +String status
        +String description
    }

    class Notification {
        +String notificationID
        +String studentNo
        +String bookingID
        +String description
        +boolean read
    }

    User <|-- Student
    User <|-- Admin
    User <|-- Marshal

    Student "1" -- "*" Booking : places
    Student "1" -- "*" IssueReport : files
    Student "1" -- "*" Notification : receives
    Booking "*" -- "1" Shuttle : assigns
    Route "1" -- "*" Shuttle : operates on
    Route "1" -- "*" Stop : contains

---