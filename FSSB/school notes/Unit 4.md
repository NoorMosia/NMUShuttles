# Unit 4: Identifying Use Cases

## Unit Learning Objectives
One of the first activities an analyst must do is to determine the functional requirements of the proposed system. This is the single most critical step of the entire SDLC. Its purpose is to convert high level business requirements (from the system request) into detailed requirements that can be used as inputs for creating functional models. This unit focuses on identifying and modelling the key aspect of functional requirements - use cases.

The unit learning outcomes are to:

Understand what a use case is
Describe the two techniques for identifying use cases

# What is a Use Case?
A use case is an activity that the system performs, usually in response to a request by a user. Use Cases describe the basic functions of the system:

What the user can do and how the system responds.

Each Use Case describes 1 and only 1 function / event in the system.

Use cases are labelled in a specific way using a Verb-Noun phrase, i.e. Print Patient History

Use Cases are documented using a Use Case Diagram. The Use Case Diagram provides a visual overview of the requirements.

## Example of a partial Use Case diagram
graph TB
    subgraph "System Boundary"
        UC1[Add Customer]
        UC2[Update Order]
        UC3[Print Report]
    end
    
    Actor1((Sales Clerk))
    Actor2((Manager))
    
    Actor1 --> UC1
    Actor1 --> UC2
    Actor2 --> UC2
    Actor2 --> UC3

## Techniques used for identifying use cases
### User Goal Technique
In this technique a systems analyst identifies the users of the system, as a role or type of user, and then identifies each goal or "actions to perform." These goals then are used to define use cases. A user goal can be thought of as a piece of work, or a task, that the user must complete.

The user goal technique for identifying use cases includes these steps:

Identify all the potential users for the new system.

Classify the potential users in terms of their functional role (e.g., shipping, marketing, sales).

Further classify potential users by organizational level (e.g., operational, management, executive).

For each type of user, interview them to find a list of specific goals they will have when using the new system. Start with goals they currently have and then get them to imagine innovative functions they think would add value. Encourage them to state each goal in the imperative verb-noun form, such as Add customer; Update order and Produce month end report.

Create a list of preliminary use cases and organize it by type of user.

Look for duplicates with similar use case names and resolve inconsistencies.

Identify where different types of users need the same use cases.

Review the completed list with each type of user and then with interested stakeholders.

This technique is the most common one used in industry as it is quite simple and effective.

## User Goal Technique Process Flow
flowchart TD
    A[Identify all potential users] --> B[Classify users by functional role]
    B --> C[Classify users by organizational level]
    C --> D[Interview users to identify goals]
    D --> E[Create preliminary use case list organized by user type]
    E --> F[Look for duplicates and resolve inconsistencies]
    F --> G[Identify shared use cases across user types]
    G --> H[Review list with users and stakeholders]

## User Goal Example
graph LR
    subgraph "User Types"
        Sales[Sales Staff]
        Manager[Manager]
        Shipping[Shipping Staff]
    end
    
    subgraph "User Goals"
        G1[Add Customer]
        G2[Update Order]
        G3[Print Report]
        G4[Process Return]
        G5[Generate Invoice]
    end
    
    Sales --> G1
    Sales --> G2
    Sales --> G4
    Manager --> G2
    Manager --> G3
    Manager --> G5
    Shipping --> G4

## Event-decomposition Technique
The event decomposition technique focuses on identifying the events to which a system must respond and then determining how a system must respond (i.e., the system's use cases). When defining the requirements for a system, it is useful to begin by asking, "What business events occur that will require the system to respond?"

## Types of Events
An Event is something that occurs at a specific time and place, can be described, and should be remembered by the system.

### External Events
An external event is an event that occurs outside the system—usually initiated by an external agent or actor. An external agent (or actor) is a person or organizational unit that supplies or receives data from the system. To identify the key external events, the analyst first tries to identify all the external agents that might want something from the system.

A classic example of an external agent is a customer. When describing external events, it is important to name the event so the external agent is clearly defined. The description should also include the action that the external agent wants to pursue.

### Temporal Events
A temporal event is an event that occurs as a result of reaching a point in time. For example, on month-end certain automatic processing may need to be initiated. The analyst begins identifying temporal events by asking about the specific deadlines that the system must accommodate. What outputs are produced at that deadline? What other processing might be required at that deadline?

### State Events
A state event is an event that occurs when something happens inside the system that triggers the need for processing. For example, a state event can occur when an inventory level reaches a pre-order point.

### Types of Events Summary
graph TD
    Event[Event] --> External[External Event]
    Event --> Temporal[Temporal Event]
    Event --> State[State Event]
    
    External --> ExtDesc[Initiated by external actor<br>Example: Customer places order]
    Temporal --> TempDesc[Occurs at a point in time<br>Example: Month-end reporting]
    State --> StateDesc[Internal system state change<br>Example: Inventory reorder point reached]

## Identifying Events

### Events versus Prior Conditions and Responses
It is sometimes difficult to distinguish between an event and part of a sequence of prior conditions that leads up to the event. The way to determine whether an occurrence is an event or part of the interaction following the event is by asking whether any long pauses or intervals occur (i.e., can the system transaction be completed without interruption?). Or is the system at rest again, waiting for the next transaction?

Example - Customer buying a shirt at a clothing shop:
The only event that affects the system is when the customer actually buys the shirt and the transaction needs to be captured by the system. After the transaction is complete, the system is at rest, waiting for the next transaction to begin. Any manual activities are not relevant input to the system.

flowchart LR
    subgraph "System at Rest"
        Wait[Waiting for<br>next transaction]
    end
    
    Event[Customer<br>purchases shirt] --> System[System processes<br>transaction]
    System --> Wait


### The Sequence of Events: Tracing a Transaction's Life Cycle
It is often useful in identifying events to trace the sequence of events that might occur for a specific external agent or actor. This is similar to the user goal technique, in that it focuses on a single actor. But it focuses on a narrower set of tasks.

Example - Customer purchasing a shirt:
Things a customer might want to do when buying a shirt: buy the shirt, return the shirt, exchange the shirt, and so forth.

flowchart LR
    Customer[Customer] --> Buy[Buy Shirt]
    Customer --> Return[Return Shirt]
    Customer --> Exchange[Exchange Shirt]
    Customer --> Track[Track Order]
    Customer --> Review[Review Product]


### Technology-Dependent Events and System Controls
Sometimes, the analyst needs to be aware of events that are important to the system but do not directly concern users or transactions. Such events typically involve design choices or system controls. These types of events are not part of the problem domain, i.e. not part of the users' requirements. Typically, those events are not addressed during analysis but are addressed during the design activities.

The Perfect Technology Assumption
One technique used to help decide which events apply to controls is to assume that technology is perfect. The perfect technology assumption states that events should be included during analysis only if the system would be required to respond under perfect conditions.

Examples of events excluded under the perfect technology assumption:

Event Type	Example
Login	User authentication
Logout	Session termination
Password management	Change password, reset password
System maintenance	Backup database
System maintenance	Restore database
Error handling	Recovery from system failures


## Using the Event Decomposition Technique
To summarize, the event decomposition technique for identifying use cases includes these steps:

Consider the external events in the system environment that require a response from the system

For each external event, identify and name the use case that the system requires.

Consider the temporal events that require a response from the system

For each temporal event, identify and name the use case that the system requires and then establish the point of time that will trigger the use case.

Consider the state events that the system might respond to, particularly if it is a real-time system in which devices or internal state changes trigger use cases.

For each state event, identify and name the use case that the system requires and then define the state change.

When events and use cases are defined, check to see if they are required by using the perfect technology assumption. Do not include events that involve such system controls such as login, logout, change password, and backup or restore the database, as these are put in as system controls.

### Event Decomposition Process Flow
flowchart TD
    A[Identify external events requiring system response] --> B[Name use case for each external event]
    B --> C[Identify temporal events]
    C --> D[Name use case and establish trigger time for each temporal event]
    D --> E[Identify state events]
    E --> F[Name use case and define state change for each state event]
    F --> G[Apply Perfect Technology Assumption]
    G --> H[Remove system control events<br>login, logout, backup, etc.]
    H --> I[Final use case list]

## Comparison of Techniques
graph TD
    subgraph "User Goal Technique"
        U1[Start with users]
        U2[Identify goals/tasks]
        U3[Create use cases from goals]
        U4[Common in industry]
    end
    
    subgraph "Event Decomposition Technique"
        E1[Start with events]
        E2[Identify external/temporal/state events]
        E3[Create use cases from events]
        E4[More comprehensive]
    end

## Summary
Aspect	User Goal Technique	Event Decomposition Technique
Starting Point	Users and their goals	Events that trigger system response
Focus	What users want to accomplish	What the system must respond to
Steps	Identify users → Identify goals → Create use cases	Identify events → Create use cases
Complexity	Simpler, more straightforward	More comprehensive, covers temporal and state events
Common Use	Most common in industry	Provides broader coverage
Advantage	Easy to implement, user-centric	Captures all types of system responses
Disadvantage	May miss system-initiated responses	More complex to implement