1.1 Problem Description
The current university shuttle system has significant challenges that impact student safety, efficiency
and overall satisfaction.
Students living off-campus are forced to queue at shuttle stops at dangerous hours of the morning.
Despite these long waits, many students are unable to board due to limited shuttle capacity.
The inefficiency of the system leads to resources being allocated poorly. High demand areas
experience overcrowding and shuttle shortages, while less populated areas may have unutilized
shuttles with empty seats. This is an imbalance that result in a waste of valuable transport resources.
Additionally, students who are unfamiliar with the area struggle to locate shuttle stops, causing
confusion and missed trips.
Our team is planning to solve these issues by –
- Eliminating the need for students to queue in unsafe conditions
- Ensuring a seat is reserved before arriving at stop.
- Optimizing shuttle allocation based on demand.
- Providing students with clear information on schedules and stop locations.
1.2 Business Rules / Assumptions
Authentication:
1. Only registered students with a valid student number may login and book for a shuttle.
2. Shuttle drivers and marshals are registered directly by the system administrator.
Bookings:
1. A student may only have one active booking per shuttle trip time slot.
2. Bookings open 24 hours in advanced of the trip.
3. Bookings close either when a shuttle reaches full capacity or 1 hour before departure,
whichever comes first.
4. When a shuttle is full, subsequent students are placed on a waiting list. If a cancellation
occurs, the first student placed on the waiting list is automatically moved to confirmed
booking, and a push notification is sent to the student. This process happens until 1 hour
before departure.
5. Students may cancel booking up to one hour before departure.
QR code tickets
1. Each successful booking generates a unique QR code that serves as a student’s ticket.
2. The QR code is valid only for the specific shuttle route, date, and departure time it was
issued for. This will be scanned and confirmed by the driver or marshal.
3. After a successful scan during boarding, the QR code us marked as used and cannot be
re-used.
Validation on check-in:
1. The assigned driver or marshal will scan the QR code
2. A student will only be accepted for boarding if the QR code is valid and matches the
assigned route.
Notifications:
Page 4 of 34
Students receive push notifications for:
1. Confirmed booking
2. Booking cancellation
3. Being moved from waiting list to confirmed spot.
4. Reminder of booking 1 hour before the trip.
Assumptions:
1. The university will provide the shuttle schedule, routes and stop information.
2. The university will also provide a list of all students who are illegible to use the shuttle
service.
3. Students have phones that have a smartphone. The screen of the smartphone must be
able to display QR codes.
Note: screenshots of QR code are acceptable for offline use.
4. The service does not required payments through app.
Any monetary transaction will happen between university and student directly.
1.3 System Constraints
1. Platform: The application will only be designed for android devices
2. Connectivity: A stable internet connection is required to use the app.
3. Location services: “find my stop” is a location-based feature that will require GPS/location
services to be enabled on a student’s phone.
4. QR code quality: QR codes must be decodable even when displayed on low-quality screens.