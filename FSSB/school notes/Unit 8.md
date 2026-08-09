# Unit 8: Creating Use Case Narratives
## Unit Learning Objectives
The main objective of defining requirements in system development is understanding users' needs, how the business processes are carried out, and how the system will be used to support those business processes. Use case diagrams, as discussed in Unit 5, provide a straightforward method to document and visualize the use cases that have been identified using one of the techniques as discussed in Unit 4. For the systems analyst to gain a deeper and more detailed understanding of each use case, he/she also need to document descriptions or narratives for the use cases. These use case narratives are used to document the context, purpose, description, conditions, and workflow of each use case.

The unit learning outcome is to:

Be able to write a use case narrative for a use case

## Writing a Use Case Narrative
A use case narrative / description lists and describes the processing details for a use case. A use case description provides the documentation for the processing steps that are internal to a use case. Depending on an analyst's needs, use case descriptions tend to be written at two separate levels of detail:

## Brief Description

Fully Developed Description

Brief Use Case Descriptions
A brief description can be used for very simple use cases, especially when the system to be developed is a small, well-understood application. A simple use case would normally have a single scenario and very few—if any—exception conditions.

### Example Brief Use Case Descriptions
Use Case	Brief Use Case Description
Create customer account	User/actor enters new customer account data, and the system assigns account number, creates a customer record, and creates an account record.
Look up customer	User/actor enters customer account number, and the system retrieves and displays customer and account data.
Process account adjustment	User/actor enters order number, and the system retrieves customer and order data; actor enters adjustment amount, and the system creates a transaction record for the adjustment.

### When to Use Brief Descriptions
Very simple use cases

Small, well-understood applications

Single scenario use cases

Very few exception conditions

### Fully Developed Use Case Descriptions
The fully developed description (narrative) is the most formal method for documenting a use case. The narrative describes a complete interaction between the software and users, it describes how the system will act in response to an action of the user. The narrative basically documents the "conversation" between the user and the system.

### Use Case Narrative Template
Section	Description
Use Case ID	A unique ID allocated to the use case
Use Case Name	A unique name for this use case
Business Actors	All actors who use this use case
Brief Description	A one or two sentence description of the purpose of the use case
Pre-Conditions	The state of the system before the use case begins, especially any required conditions of the database
Triggers	The event that initiates or triggers this use case
Post-Conditions	The state of the system at the successful completion of the use case, especially any updates to the database
Basic Flow of Events	A step-by-step sequence of the actions by the actor and the system internal to the use case. Usually the flow of activities is the most difficult part to develop, but also assists the analyst and the user to understand the requirements better. The idea is simply that the user does something, the system responds. It goes back and forth.
Alternate Flow of Events	Any exception conditions that cause the system not to follow the expected flow of activities or that cause the use case to terminate abnormally

### Template Structure in UML
flowchart TD
    subgraph "Use Case Narrative Template"
        ID[Use Case ID] --> Name[Use Case Name]
        Name --> Actors[Business Actors]
        Actors --> Desc[Brief Description]
        Desc --> Pre[Pre-Conditions]
        Pre --> Trig[Triggers]
        Trig --> Post[Post-Conditions]
        Post --> Basic[Basic Flow of Events]
        Basic --> Alt[Alternate Flow of Events]
    end

### Basic Flow of Events Format
flowchart LR
    subgraph "Basic Flow Pattern"
        Actor[User/Actor<br>does something] --> System[System<br>responds]
        System --> Actor2[User/Actor<br>does something else]
        Actor2 --> System2[System<br>responds again]
    end

## Example: Login Use Case Narrative
### Scenario
A student wanting to login to one of the computers in the lab in the Computing Sciences department.

### Use Case Narrative
Section	Details
Use Case ID	A100
Use Case Name	Login
Business Actors	Student
Brief Description	This use case describes the process of a student logging in to a PC in the CS lab
Pre-Conditions	- Student must be registered for an NMU Computer Science module
- Student must not be logged on already
Triggers	Ctrl-Alt-Del keys are pressed
Post-Conditions	Student is logged onto a computer in the Computer Science labs


### Basic Flow of Events
Step	Action Description
1	System displays login screen.
2	User enters username and password.
3	System checks user credentials.
4	System displays Terms and Conditions.
5	User accepts Terms and Conditions by clicking OK.
6	System displays welcome screen.


### Alternate Flow of Events
Step	Action Description
3.1	User credential check fails, system calls A110
3.2	Returns to step 1 in basic flow
5.1	User declines to accept Terms and Conditions
5.2	System locks the screen

### Login Narrative Visualization
flowchart TD
    subgraph "Basic Flow"
        S1[Step 1: System displays login screen]
        S2[Step 2: User enters username and password]
        S3[Step 3: System checks user credentials]
        S4[Step 4: System displays Terms and Conditions]
        S5[Step 5: User accepts Terms and Conditions]
        S6[Step 6: System displays welcome screen]
        
        S1 --> S2 --> S3 --> S4 --> S5 --> S6
    end
    
    subgraph "Alternate Flow"
        A1[3.1: User credential check fails]
        A2[3.2: System calls A110]
        A3[5.1: User declines Terms and Conditions]
        A4[5.2: System locks the screen]
    end
    
    S3 -.->|If credentials invalid| A1
    A1 --> A2 --> S1
    S5 -.->|If declined| A3
    A3 --> A4

## Step-by-Step Guide to Writing Use Case Narratives
### Step 1: Identify the Use Case
flowchart LR
    A[Identify use case from diagram] --> B[Assign unique Use Case ID]
    B --> C[Give meaningful Use Case Name]
    C --> D[List all Business Actors]
Step 2: Write Brief Description
Write one or two sentences explaining the purpose of the use case

Focus on what the use case accomplishes

Keep it concise and clear

### Step 3: Define Conditions
Condition Type	What to Include
Pre-Conditions	- State of the system before the use case begins
- Required conditions of the database
- What must be true for the use case to start
Post-Conditions	- State of the system upon successful completion
- Database updates that occur
- What is true after the use case ends

### Step 4: Identify Triggers
The event that initiates the use case

Could be a user action, time-based event, or system event

### Step 5: Document Basic Flow of Events
Start with the actor's action

Follow with the system's response

Continue alternating between actor and system

Number each step sequentially

Keep steps at a consistent level of detail

### Step 6: Document Alternate Flow of Events
Identify exception conditions

Use numbering that references the basic flow step (e.g., 3.1, 5.1)

Describe what happens when the exception occurs

Show where the flow returns to in the basic flow

## Common Mistakes to Avoid
Mistake	Solution
Too much detail	Keep steps at a consistent level of detail - not too high level, not too low level
Skipping actor actions	Remember to include both actor actions AND system responses
Missing alternate flows	Think about what could go wrong or what might happen differently
Vague descriptions	Be specific about what the user does and how the system responds
Forgetting conditions	Always include pre-conditions and post-conditions
Inconsistent numbering	Use clear numbering for alternate flows (e.g., 3.1, 3.2)

### Good vs Bad Step Writing
Bad Example	Good Example
"User logs in"	"User enters username and password"
"System processes"	"System checks user credentials against database"
"System displays"	"System displays Terms and Conditions screen"
"User accepts"	"User accepts Terms and Conditions by clicking OK"

### Complete Use Case Narrative Checklist
Element	Check
Use Case ID assigned	☐
Use Case Name begins with verb	☐
All Business Actors listed	☐
Brief Description (1-2 sentences)	☐
Pre-Conditions defined	☐
Triggers identified	☐
Post-Conditions defined	☐
Basic Flow of Events complete with step numbers	☐
Alternate Flow of Events documented	☐
Alternate flows reference basic flow steps	☐
Narrative reviewed with stakeholders	☐


## Summary
### Two Levels of Use Case Descriptions
Level	Description	When to Use
Brief Description	One or two sentences per use case	Simple use cases, small applications
Fully Developed Narrative	Complete template with all sections	Complex use cases, detailed documentation needed

### Key Components of a Use Case Narrative
Use Case ID - Unique identifier

Use Case Name - Verb-noun phrase

Business Actors - All users of the use case

Brief Description - Purpose of the use case

Pre-Conditions - What must be true before start

Triggers - What initiates the use case

Post-Conditions - What is true after completion

Basic Flow of Events - Normal sequence of steps

Alternate Flow of Events - Exception conditions