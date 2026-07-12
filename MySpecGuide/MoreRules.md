1.
I don't want the marshal to only flag a shuttle that didn't arrive or broken down. I don't want marshals to have too much work.

Admin can later fill in details of the replacement for the shuttle that was flagged by the marshal, if there is a replacement. otherwise mark it as a cancelled trip.

Shuttle breakdowns and cancelations will be tallied to the shuttle and the vendor who owns the vehicle. This info will be used for analytics in the admin portal.

2.
scanning should only be available 35 minutes before departure time.

3. 
Shuttles should be arranged in some sort of order
the front shuttle gets populated first.

THUS, students are assigned to a specific shuttle when they are scanned. If this shuttle has been flagged by marshal for not having arrived or having broken down, the scanned students will be registered on the system as having arrived. and the admin can later on add details of the shuttle that came to pick them up. The push notification associated with the vehicle breakdown will arrive after the admin has filled in the detailed of the replacement if there was one. 

Accessible shuttles are placed in front. And the priority students will be placed in the first shuttle during the automated process that creates a booking for them.

4. 
Populate the seedData.
students numbers should start with s31 or s32 then 7 more random numbers. meaning, letter s plus 9 numbers.
staff numbers are just 7 random numbers.
everything else thats not given, populate it based on whats required in our db

5. 
for a stop, add the fullAddress attribute, to be used later in the app UI, to search. thus the search will be by name and by full address

6. 
For UI, I don't want a top bar, 
I only want a hamburger icon with white background square background, this square background should a some small border radius. 
The logout button will be in the sidebar, pinned to the bottom.

7. 
On the qr code, the center image will be the logo.svg file in the assets folder.
the main logo for the website, is 1.png file in the logos folder, this is the picture that will also be used on the login pages.