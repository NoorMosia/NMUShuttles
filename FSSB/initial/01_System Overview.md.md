# 1. System Overview - LATT Mobile

## 1.1 Problem Description
The current university shuttle system presents challenges impacting student safety, efficiency, and overall satisfaction[cite: 1]:
* **Safety Concerns:** Off-campus students queue at shuttle stops at dangerous hours in the morning[cite: 1].
* **Capacity Bottlenecks:** Despite long wait times, many students miss shuttles due to unmanaged capacity[cite: 1].
* **Resource Imbalance:** High-demand stops experience severe overcrowding, while low-demand routes run underutilized shuttles with empty seats[cite: 1].
* **Navigation Issues:** Students unfamiliar with area routes struggle to locate pickup points, resulting in missed trips[cite: 1].

### Core Solutions
1. Reserve seats prior to arriving at the stop[cite: 1].
2. Eliminate early-morning physical queueing in unsafe conditions[cite: 1].
3. Dynamic shuttle allocation based on real-time route demand[cite: 1].
4. Clear schedule, route, and pickup point visibility[cite: 1].

---

## 1.2 Business Rules & System Assumptions

### Authentication & Access
1. Only registered students with valid student numbers may log in and book shuttle seats[cite: 1].
2. Drivers and Marshals are provisioned directly by the System Administrator[cite: 1].

### Bookings & Waitlists
1. **Active Booking Limit:** A student can hold only **one active booking** per shuttle trip time slot[cite: 1].
2. **Booking Window:** Bookings open 24 hours prior to trip departure[cite: 1].
3. **Cut-off Window:** Bookings close when full capacity is reached OR 1 hour before departure (whichever occurs first)[cite: 1].
4. **Waitlist Automation:**
   * When a shuttle reaches full capacity, subsequent requests join an ordered waitlist[cite: 1].
   * On cancellation, the top waitlisted student is automatically promoted to a confirmed booking[cite: 1].
   * A push notification is issued upon promotion[cite: 1].
   * Automated promotions run until 1 hour prior to departure[cite: 1].
5. **Cancellations:** Students can cancel bookings up to 1 hour before departure[cite: 1].

### Tickets & Boarding Validation
1. A unique QR code ticket is generated upon booking confirmation[cite: 1].
2. QR codes are valid exclusively for the assigned route, date, and time slot[cite: 1].
3. Screenshots of QR codes are accepted for offline validation[cite: 1].
4. Scanning invalidates the ticket single-use token upon boarding[cite: 1].
5. Marshals/Drivers scan QR codes at boarding; validation must match route and schedule[cite: 1].

### Payments & Infrastructure Assumptions
1. No in-app monetary transactions; funding/eligibility is handled direct by the university[cite: 1].
2. University provides master schedules, route definitions, stop locations, and eligible student lists[cite: 1].

---

## 1.3 System Constraints
1. **Target Platform:** Android mobile devices only[cite: 1].
2. **Connectivity:** Active internet connection required for live sync[cite: 1].
3. **Location:** GPS access required for the "Find My Stop" feature[cite: 1].
4. **Scanner Reliability:** QR codes must render clearly for low-spec screens and varied scanner lighting[cite: 1].