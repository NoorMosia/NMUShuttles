# Unit 6: Introduction to Class Diagrams

## Unit Learning Objectives
Another very important activity during systems analysis is to identify and define the data that need to be captured by the system. In the previous units, we focused on understanding the functionality of the system and we modelled the functionality by means of use cases and created a use case diagram to document them.

This unit will introduce you to the technique which you can use to identify and model the data elements of a system.

The unit learning outcomes are to:

Explain how the concept of "things" in the problem domain also defines requirements

Identify and analyze data entities and domain classes needed in a system

Read, interpret, and create a domain model class diagram

## Introduction
Use Case models are referred to as Functional Models as they represent and model the system behavior as a result of a request by a user. For example, in a Student Registration system, a function would be to Register for a module.

A Structural Model on the other hand represents the system objects and their relationships. These objects can be people, places or "things" about which information need to be captured. In the example of a Student Registration system, information regarding the students, their registration and the modules need to be captured. System analysts use structural modelling to model and document this information.

The focus of this unit is to identify and define the entities or problem domain classes through data modelling. Traditional system development methodologies called these "things" data entities and used Entity-Relationship diagrams to model the data structures. Object-oriented techniques call these "things" problem domain classes, or just classes for short. A UML class diagram is used to model the data structure. Either type of model can be used and results in equivalent solutions.

## "Things" in the Problem Domain
The term "problem domain" is an interesting word choice. Often when we think of a problem, we think that something is broken. However, in this context, the word problem simply means a need that requires a business solution. In other words, system developers are developing a solution for a business need or business problem. Hence the term problem domain, is the context and arena in which the business problem exists.

A basic problem with data modeling is what to use for the generic term of items in the real world that a system analyst is trying to discover. We have chosen to simply call them "things." Although "things" is a very ambiguous term that can be used to identify almost any real-world item or abstract idea, that information is in fact what data modeling is trying to capture.

## How are "things" discovered in the Problem Domain?
Systems analysts can use a combination of various techniques to help them identify the "things" within the problem domain.

### The Brainstorming Technique
This technique is a joint effort between the analyst and the users.

Types of "Things" to Consider:

Category	Examples
Tangible things	Products, equipment, vehicles, buildings
Roles	Customer, employee, manager, student
Locations	Office, warehouse, store, campus
Organizations	Department, company, division, team
Events/Transactions	Sale, registration, order, payment
Documents	Invoice, receipt, report, application
Processes	Registration, application, approval

### Brainstorming Technique Steps:
Identify a user and a set of use cases.

Brainstorm with the user to identify things involved when carrying out the use case—that is, things about which information should be captured by the system.

Use the types of things (categories) to systematically ask questions about potential things, such as the following:

Are there any tangible things you store information about?

Are there any locations involved?

Are there roles played by people that you need to remember?

Continue to work with all types of users and stakeholders to expand the brainstorming list.

Merge the results, eliminate any duplicates, and compile an initial list.

### Brainstorming Process Flow
flowchart TD
    A[Identify user and use cases] --> B[Brainstorm things involved in use cases]
    B --> C[Use categories to ask systematic questions]
    C --> D[Expand list with all users and stakeholders]
    D --> E[Merge results and eliminate duplicates]
    E --> F[Compile initial list of classes]

## The Noun Technique
The noun technique is a more mechanical approach to identifying classes, but it is also an effective technique. The basic idea is to identify all the nouns within the problem domain, which are always some type of "thing," and build the list of all these nouns. Then refine the list to remove duplicates and identify which are classes and which are attributes of classes.

### Noun Technique Steps:
Using the use cases, actors, and other information about the system—including inputs and outputs—identify all nouns.

Using other information from existing systems, current procedures, and current reports or forms, add items or categories of information needed.

As this list of nouns builds, you will need to refine it. Ask these questions about each noun to help you decide whether you should include it:

Ask questions, such as is it important and inside the scope, to see if it should be included.

Ask questions to see if it really should be excluded, such as is it only a report or an input or is it an attribute.

Create a master list of all nouns identified and then note whether each one should be included, excluded, or researched further.

Review the list with users, stakeholders, and team members and then refine the list of things in the problem domain.

### Noun Technique Process Flow
flowchart TD
    A[Identify all nouns from use cases, actors, and system info] --> B[Add nouns from existing systems and reports]
    B --> C[Refine the list - ask inclusion/exclusion questions]
    C --> D[Create master list with status: Include/Exclude/Research]
    D --> E[Review with users and stakeholders]
    E --> F[Refine final list of classes]

### Attributes of Things
The noun technique involves listing all the nouns that come up in discussions or documents about the requirements. Many of these nouns are actually attributes. During the refinement of the nouns on the list of nouns, an evaluation of each noun will determine whether the noun is an independent item, e.g. a class, or whether it describes or characterizes another noun and hence is an attribute.

### Identifiers/Keys
One attribute may be used to identify a specific thing, such as an Employee Number for an employee or an Order Number for a purchase. The attribute that uniquely identifies the thing is called an identifier or key.

### Properties of Attributes
Attributes are the properties of a class

When identifying attributes of a class, only the relevant attributes within the given problem domain that relates to a class should be listed

In a class, each attribute will have a specific value for each occurrence of that class

### Example: Customer Class with Attributes
classDiagram
    class Customer {
        -customerID: int
        -firstName: String
        -lastName: String
        -homePhone: String
        -workPhone: String
    }

### Sample Data for Customer Class
Customer ID	First Name	Last Name	Home Phone	Work Phone
101	John	Smith	555-9182	555-3425
102	Mary	Jones	423-1298	423-3419
103	Bill	Casper	874-1297	874-8546
Each customer has specific values for each attribute.

## Associations among Things
An association is a naturally occurring relationship between specific things, such as:

An order is placed by a customer

An employee works in a department

"Is placed by" and "works in" are two associations that naturally occur between specific things. In the example used above, the information system needs to store information about employees and about departments, but equally important is storing information about their specific associations.

In database management, the term relationship is often used in place of association, which is the term used when modeling in UML. We will use association in this module because we emphasize UML diagrams and terminology.

## Reading Associations
Associations apply in two directions and must be read separately in each way. For example:

A customer places an order

An order is placed by a customer

graph LR
    Customer((Customer)) -->|places| Order((Order))
    Order -->|is placed by| Customer

### Association Example in UML
classDiagram
    Customer "0..*" --> "1" Order : places
    Order "1" --> "0..*" Customer : is placed by

##  Multiplicity
Multiplicity refers to the number of links in UML and should be used when discussing UML models. Multiplicity is established for each direction of the association. It is important to describe not just the multiplicity but also the range of possible values of the multiplicity (the minimum and maximum multiplicity). These terms are referred to as multiplicity constraints.

### Multiplicity Notation
Multiplicity is denoted by numbers placed next to the class at either end of an association line.

Notation	Meaning
0..1	Zero or one
1	Exactly one
0..* or *	Zero to many
1..*	One or many
2..4	Two to four (specific range)
0..2	Zero to two

### Multiplicity Examples
classDiagram
    Customer "1" --> "0..*" Order : places
    Order "1" --> "1" Customer : is placed by
    Order "1" --> "1..*" OrderItem : consists of
    OrderItem "1" --> "1" Order : belongs to

### Reading Multiplicities
A customer places zero to many Orders

An order is placed by exactly one customer

An order consists of one or more Order Items

An Order Item is part of exactly one Order

## Types of Associations
### Binary Associations
Associations between two classes are called binary associations. These are the most common type and are the focus of this unit.

classDiagram
    Employee "1" --> "0..*" Department : works in
    Department "1" --> "1..*" Employee : employs

### Unary Associations
Sometimes an association will be between different elements in the same class or set. These are called unary associations.

classDiagram
    Employee "0..*" --> "0..1" Employee : manages

### Ternary Associations
Associations that exist between three classes.

classDiagram
    Employee "1" --> "0..*" Assignment : has
    Project "1" --> "0..*" Assignment : includes
    Department "1" --> "0..*" Assignment : assigns
Note: For this unit we will focus on the binary association. The others will be discussed in Unit 7.

## Domain Class Diagram Notation
Current approaches to system development, primarily object-oriented approaches, use the term class rather than data entity and use concepts and notations based on UML to model the things in the problem domain.

### What is a Class?
A class is a category or classification used to describe a set of objects. So, a class is a category, but it is also a set of objects. Classes that describe things in the problem domain are called domain classes. Domain classes have attributes and associations. Multiplicity applies among classes.

A UML domain class diagram is used to model the data structure within the problem domain.

### Domain Class Symbol
On a class diagram, rectangles represent classes, and the lines connecting the rectangles show the associations among classes.

The domain class symbol is a rectangle with two sections:

Top section: Contains the name of the class (capitalized)

Bottom section: Lists the attributes of the class (begin with lower case)

classDiagram
    class Customer {
        -customerID: int
        -name: String
        -address: String
        -phone: String
    }
### Important Notes:
Class names are capitalized

Attributes begin with lower case

A domain class has NO methods (only attributes)

## Practical Examples
### Example 1: Employee Payroll Deductions
classDiagram
    Employee "1" --> "0..*" PayrollDeduction : has
    PayrollDeduction "0..*" --> "1" Employee : belongs to
Multiplicity: An employee can have zero payroll deductions or many deductions.

### Example 2: Employee Spouse
classDiagram
    Employee "1" --> "0..1" Spouse : has
    Spouse "0..1" --> "1" Employee : belongs to
Multiplicity: An employee can have no spouse or one spouse.

### Example 3: Office Manager and Sales Office
classDiagram
    OfficeManager "1" --> "1" SalesOffice : manages
    SalesOffice "1" --> "1" OfficeManager : is managed by
Multiplicity: An office manager manages one and only one office, and an office is managed by one and only one office manager.

### Example 4: Order and Order Items
classDiagram
    Order "1" --> "1..*" OrderItem : contains
    OrderItem "1..*" --> "1" Order : belongs to
Multiplicity: One order can include one or many items ordered. An item ordered can belong to only one order.

## Complete Scenario Example
### Scenario Description
A company has seven departments and each department belongs to one company. The company has a unique name and each department has a unique number and name. Each department employs one or more employees and an employee works for only one department. Each employee has an employee number, name, date of birth and age.

Each of the employees may or may not have one or more dependents, and each dependent belongs to only one employee. The dependent name, relationship to employee and 3 telephone numbers are kept.

Each employee may or may not have an employment history. Each employment history has the name of the company that the employee worked for and in what capacity, the start date and finish date for each employment.

### Domain Class Diagram for the Scenario
classDiagram
    class Company {
        -name: String
    }
    
    class Department {
        -deptNumber: int
        -name: String
    }
    
    class Employee {
        -employeeNumber: int
        -name: String
        -dateOfBirth: Date
        -age: int
    }
    
    class Dependent {
        -name: String
        -relationship: String
        -phone1: String
        -phone2: String
        -phone3: String
    }
    
    class EmploymentHistory {
        -companyName: String
        -capacity: String
        -startDate: Date
        -finishDate: Date
    }
    
    Company "1" --> "1..*" Department : has
    Department "1..*" --> "1" Company : belongs to
    Department "1" --> "1..*" Employee : employs
    Employee "1..*" --> "1" Department : works for
    Employee "1" --> "0..*" Dependent : has
    Dependent "0..*" --> "1" Employee : belongs to
    Employee "1" --> "0..*" EmploymentHistory : has
    EmploymentHistory "0..*" --> "1" Employee : belongs to

### Reading the Diagram:
A company has one or more departments

A department belongs to exactly one company

A department employs one or more employees

An employee works for exactly one department

An employee may have zero or more dependents

A dependent belongs to exactly one employee

An employee may have zero or more employment history records

An employment history record belongs to exactly one employee

## Summary of Key Concepts
### Class Diagram Components
Component	Description	Notation
Class	A category of objects	Rectangle with name and attributes
Attribute	Property of a class	Listed in lower section of class box
Identifier/Key	Uniquely identifies an instance	Underlined in some notations
Association	Relationship between classes	Line connecting classes
Multiplicity	Number of links between classes	Numbers at ends of association lines
Multiplicity Summary
Multiplicity	Meaning
0..1	Zero or one
1	Exactly one
* or 0..*	Zero to many
1..*	One to many
n (e.g., 2, 3, 4)	Exactly n
n..m (e.g., 2..4)	Between n and m


## Key Rules for Domain Class Diagrams
Class names are capitalized

Attributes begin with lower case

Domain classes have NO methods

Associations are read in both directions

Multiplicity is specified for both ends of an association

Each class should have an identifier/key