# Unit 5: Use Case Modelling

## Unit Learning Objectives
Use case diagrams provide a straightforward method to document and visualize the use cases that have been identified using one of the techniques as discussed in Unit 4.

The unit learning outcomes are to:

Describe the notation and purpose for the use case diagram

Apply the user goal technique to identify use cases

Draw a use case diagram

## Use Case Diagram notation
The use case diagram is a UML model used to graphically show the use cases and their relationship to users. Recall that UML is the Unified Modeling Language, the standard for diagrams and terminology for developing information systems in an object-oriented environment. The objects that go onto a use case diagram are as follows:

## Actor
In UML, a person that uses the system is called an actor. An actor is always outside (external) to the automation boundary of the system but may be part of the manual portion of the system. Temporal events can also become actors and are named TIME on the use case diagram.

### Guidelines for Actors:
Shown as a simple stick figure on the use case diagram

Actors model roles (not positions or individuals)

In a hotel both the front office executive and shift manager can make reservations. So something like "Reservation Agent" should be used for the actor name to highlight the role

Give meaningful business relevant names for actors

For example, if your use case interacts with an outside organization it is much better to name it with the function rather than the organization name. (E.g.: Airline Company is better than SAA)

Primary actors should be to the left side of the diagram

This enables you to quickly highlight the important roles in the system

External systems are actors

If your use case is send-email and if it interacts with the email management software (e.g. Outlook) then the software is an actor to that particular use case

Actors do not interact with other actors

Place inheriting actors below the parent actor

This is to make it more readable and to quickly highlight the use cases specific for that actor

Actors must be associated with at least one use case

Actors can be associated with multiple use cases

Multiple actors can be associated with a single use case

### Actor Representation
graph TB
    subgraph "Actors"
        A1((Customer))
        A2((Reservation Agent))
        A3((Manager))
    end
    
    subgraph "System Boundary"
        UC1[Make Reservation]
        UC2[Cancel Reservation]
        UC3[View Reports]
    end
    
    A1 --> UC1
    A1 --> UC2
    A2 --> UC1
    A2 --> UC2
    A3 --> UC3

## Use Case
Represents a process / tasks in the system. The use case itself is represented by an oval with the name of the use case inside.

### Guidelines for Use Cases:
Names begin with a verb

A use case models an action so the name should begin with a verb.

Make the name descriptive

This is to give more information for others who are looking at the diagram. For example, "Print Patient History" is better than "Print"

Place included use cases to the right of the invoking use case

This is done to improve readability and add clarity

Place inheriting use case below parent use case

Again, this is done to improve the readability of the diagram

## Relationship/Association
### Association between Actor and Use Case
This one is straightforward and present in every use case diagram. It is the connecting line between the actor and the use case that indicates that the actor is involved with that use case.

Important Notes:

An actor must be associated with at least one use case

An actor can be associated with multiple use cases

Multiple actors can be associated with a single use case

There are no arrows drawn in on the line

### Association Examples
graph LR
    subgraph "Association Types"
        A1((Customer)) --- UC1[Make Reservation]
        A2((Customer)) --- UC2[Cancel Reservation]
        A3((Customer)) --- UC3[View Booking]
        A4((Customer)) --- UC4[Update Profile]
    end

## Generalization of an Actor
Generalization of an actor means that one actor can inherit the role of the other actor. The child inherits all the use cases of the parent. The child has one or more use cases that are specific to that role.

### Example: Resident Management System
graph TB
    subgraph "Actors"
        Resident((Resident))
        Tenant((Tenant))
        Landlord((Landlord))
    end
    
    subgraph "System Boundary"
        UC1[Update Personal Details]
        UC2[Pay Rent]
        UC3[Send Out Newsletter]
        UC4[Report Maintenance]
    end
    
    Resident --> UC1
    Tenant --> UC4
    Tenant --> UC2
    Landlord --> UC3
    Landlord --> UC1
    
    Resident -.-> Tenant
    Resident -.-> Landlord
In the example above:

The Tenant and Landlord inherit the Update Personal Details use case from the Resident

The Landlord is the only actor that may access the Send out Newsletter use case

The Tenant has specific use cases like Pay Rent and Report Maintenance

## Extend between Two Use Cases
The extend relationship models optional behaviour or behaviour that only runs under certain conditions. As the name implies it extends the base use case and adds more functionality to the system.

Key Points about <<extend>> Relationship:
The extending use case is dependent on the extended (base) use case

In the diagram below, the "Assign Seat" use case doesn't make much sense without the "Check in Passenger" use case

The extending use case is usually optional and can be triggered conditionally

The extending use case is triggered only when a passenger wishes to be assigned a seat or if they have baggage to check in

The extend (base) use case must be meaningful on its own

This means it should be independent and must not rely on the behaviour of the extending use case

Note that the arrow points to the use case that the additional actions are being added on to

### Extend Relationship Example
graph LR
    subgraph "System Boundary"
        UC1[Check in Passenger]
        UC2[Assign Seat]
        UC3[Check Baggage]
    end
    
    Actor((Passenger))
    
    Actor --> UC1
    UC1 -.->|<<extend>>| UC2
    UC1 -.->|<<extend>>| UC3
In this example:

Assign Seat and Check Baggage are optional extensions of Check in Passenger

They only occur under certain conditions (passenger requests seat assignment or has baggage)

Check in Passenger is complete and meaningful on its own

## Include between Two Use Cases
This relationship shows the mandatory inclusion of another use case - like a called subroutine. The include relationship shows that the behaviour of the included use case is part of the including (base) use case.

### Key Points about <<include>> Relationship:
The base use case is incomplete without the included use case

The included use case is mandatory and not optional

The main reason for this is to reuse common actions across multiple use cases

In some situations, this is done to simplify complex behaviours

Take note of how the relationship line is drawn - it is a dotted-line with an arrow that points to the common use case

### Include Relationship Example
graph LR
    subgraph "System Boundary"
        UC1[Register for Course]
        UC2[Book Accommodation]
        UC3[Pay Fees]
    end
    
    Attendee((Course Attendee))
    
    Attendee --> UC1
    Attendee --> UC2
    UC1 -.->|<<include>>| UC3
    UC2 -.->|<<include>>| UC3

In this example:

When a Course Attendee registers for a course, they HAVE to pay the fees

The same applies when the Course Attendee books accommodation, the fees MUST be paid

The Pay Fees use case is shared between the other two

The use of the words HAVE and MUST indicates that the Pay Fees use case is mandatory

## Automation Boundary
The automation boundary defines the border between the computerized portion of the application and the people operating the application. It is shown as a rectangle containing the use cases.

### Complete Use Case Diagram Example
graph TB
    subgraph "System Boundary - Medical Practice Management System"
        UC1[Register Patient]
        UC2[Schedule Appointment]
        UC3[Update Medical Record]
        UC4[Generate Invoice]
        UC5[Process Payment]
        UC6[Print Patient History]
        UC7[Send Reminder]
    end
    
    subgraph "Actors"
        Patient((Patient))
        Receptionist((Receptionist))
        Doctor((Doctor))
        Accountant((Accountant))
        Time((TIME))
    end
    
    Patient --> UC1
    Patient --> UC2
    Receptionist --> UC1
    Receptionist --> UC2
    Doctor --> UC3
    Doctor --> UC6
    Accountant --> UC4
    Accountant --> UC5
    Time --> UC7
    UC4 -.->|<<include>>| UC5
    UC2 -.->|<<extend>>| UC7

## Creating a Use Case Diagram
In this module, it is recommended that you use the User Goal Technique to identify possible users and use cases from a given scenario.

### Steps to Create a Use Case Diagram:
flowchart TD
    A[Identify all actors/users] --> B[Identify use cases for each actor]
    B --> C[Identify relationships between use cases]
    C --> D[Apply <<include>> for mandatory shared behaviour]
    C --> E[Apply <<extend>> for optional behaviour]
    D --> F[Apply actor generalization where appropriate]
    E --> F
    F --> G[Draw the complete diagram with system boundary]

## Summary of Relationships
Relationship Type	Symbol	Direction	Mandatory/Optional	Description
Association	Solid line	No arrow	N/A	Links actor to use case
Generalization	Solid line with hollow triangle	Points to parent	N/A	Child inherits from parent
<<include>>	Dotted line with arrow	Points to included use case	Mandatory	Base use case requires included use case
<<extend>>	Dotted line with arrow	Points to base use case	Optional	Extending use case adds optional behaviour

## Quick Reference Guide
Actor Checklist:
□ Represented as stick figure
□ Models a role, not a specific person
□ Has meaningful business name
□ Primary actors on left side
□ Associated with at least one use case
□ External systems included where needed
Use Case Checklist:
□ Name begins with a verb
□ Name is descriptive
□ Represents one function/event
□ Unique within the system
Relationship Checklist:
□ Associations connect actors to use cases
□ <<include>> used for mandatory, shared functionality
□ <<extend>> used for optional, conditional behaviour
□ Generalization used for actor inheritance