# Unit 7: UML Class Diagram Specialized Associations Explained with Examples

## Unit Learning Objectives
Classes are interrelated to each other in specific ways. In particular, associations (relationships) in class diagrams include different types of logical connections. In Unit 6, the examples shown were specifically for binary association between two classes. In this Unit, I will introduce you to two specialized associations that can be modelled on a Domain Class Diagram.

## Types of Associations (Relationships)
### Binary Association
Associations between exactly two different classes. The binary association shown in the example below can also be called a one-to-many association.

classDiagram
    Team "1" --> "11..40" Player : has
    Player "11..40" --> "1" Team : belongs to
Reading the association:

1 Team can have a minimum of 11 players to a maximum of 40 players

1 Player can be part of only 1 Team

### Unary Association (Recursive)
Associations between two instances of the same class.

classDiagram
    Person "0..1" --> "0..1" Person : is married to
classDiagram
    Employee "1" --> "0..*" Employee : manages
Reading the associations:

A Person is married to a person

An Employee manages no or many employees

### Ternary Association
Between 3 entities, with a 4th entity being the association.

classDiagram
    Employee "1" --> "0..*" Assignment : has
    Project "1" --> "0..*" Assignment : includes
    Department "1" --> "0..*" Assignment : assigns
Note: The Unary and Ternary associations will be discussed in a lot more detail when you get to your 3rd year Database module.

## Many-to-Many Associations
Many-to-many associations allow data objects from one class to be connected to many objects of the associated class, and this association occurs in both directions. Sometimes the association itself will need to have attributes associated with it.

### Example: Orders and Products
classDiagram
    Order "1" --> "*" Product : contains
    Product "*" --> "1" Order : belongs to
Scenario Description:
An order can include many products, though a product will only be in an order once. Each time an order is placed for a product, we need to know:

How many units of that product are being ordered

The price we are actually selling the product for (the sale price might vary from the list price by customer discount, special sale, price changes, etc.)

Constraints:

Each Order must contain one and at most many Products (it doesn't make sense to have an order with no products)

Each Product may appear in many Orders (a product can exist in the database and not have been ordered by anyone)

### The Problem
There are significant differences in this modeling problem compared to what you have seen in Unit 6:

The maximum multiplicity in each direction is "many" — this is called a many-to-many relationship between Orders and Products

There are two attributes (sale price and quantity) that cannot possibly be attributes of either the Product or Order classes

These attributes are a result of the association between the Order and the Product

They describe each individual instance of the association

### Solution: Association Class
UML gives us a way to model relationship attributes by showing them in an association class that is connected to the association by a dotted line.

classDiagram
    class Order {
        -orderID: int
        -orderDate: Date
        -customerID: int
    }
    
    class Product {
        -productID: int
        -productName: String
        -listPrice: double
    }
    
    class OrderLine {
        -quantity: int
        -unitSalePrice: double
    }
    
    Order "1" --> "*" OrderLine : includes
    Product "1" --> "*" OrderLine : appears in
    OrderLine "*" --> "1" Order : belongs to
    OrderLine "*" --> "1" Product : refers to
In the figure above, the OrderLine association class contains the two attributes:

quantity

unitSalePrice

### Association Class Process Flow
flowchart TD
    A[Identify Many-to-Many Association] --> B[Are there attributes that belong to the association?]
    B -->|Yes| C[Create Association Class]
    B -->|No| D[Model as standard many-to-many]
    C --> E[Connect with dotted line to association]
    E --> F[Add attributes to association class]

### Guidelines for Association Classes
Question	Answer
What name must you give to the association class?	Something that describes the relationship (i.e. GamePlayed) or concatenate the names of the 2 classes (i.e. PlayerGame)
Does the Association Line look different?	Yes, this must be a dotted line
Do you place multiplicities on the line?	No - this will be resolved when implementing within a physical database structure

## Example 1: Football Team Scenario
### Scenario Description
A football team has at least 11 players and may have up to 40 players. A Player can only belong to 1 Team. Each player may or may not play one or more games. A minimum of 11 players and a maximum of 14 players may participate in one game. A player may or may not score one or more goals. A goal is scored by only one player. Each game may have zero or more goals. A goal is scored in one specific game.

### Domain Class Diagram
classDiagram
    class Team {
        -teamID: int
        -teamName: String
        -coachName: String
    }
    
    class Player {
        -playerID: int
        -firstName: String
        -lastName: String
        -dateOfBirth: Date
        -position: String
    }
    
    class Game {
        -gameID: int
        -gameDate: Date
        -opponent: String
        -venue: String
    }
    
    class Goal {
        -goalID: int
        -timeScored: Time
        -goalType: String
    }
    
    class PlayerGame {
        -positionPlayed: String
        -comments: String
    }
    
    Team "1" --> "11..40" Player : has
    Player "11..40" --> "1" Team : belongs to
    Player "1" --> "*" PlayerGame : participates in
    Game "1" --> "11..14" PlayerGame : includes
    PlayerGame "*" --> "1" Player : belongs to
    PlayerGame "*" --> "1" Game : is played in
    Player "1" --> "*" Goal : scores
    Goal "*" --> "1" Player : is scored by
    Game "1" --> "*" Goal : has
    Goal "*" --> "1" Game : occurs in

### Extracted Sentences (Colour Coded)
Association	Sentences
Team ↔ Player	A football team has at least 11 players and may have up to 40 players. A Player can only belong to 1 Team.
Player ↔ Game	Each player may or may not play one or more games. A minimum of 11 players and a maximum of 14 players may participate in one game.
Player ↔ Goal	A player may or may not score one or more goals. A goal is scored by only one player.
Game ↔ Goal	Each game may have zero or more goals. A goal is scored in one specific game.


### The Many-to-Many Association
You will notice that the association between Player and Game is a Many-to-Many association.

Assume that a Player can play various positions, i.e. a Netball player can be a Goal Shooter or a Goal Attack. If you would like to know which position a specific Player played in a specific Game and maybe make some comments on their game play, where would you store this information?

Solution: Create an association class between Player and Game which will then contain the attributes (positionPlayed, comments) that relates to the specific relationship.

classDiagram
    class Player {
        -playerID: int
        -firstName: String
        -lastName: String
    }
    
    class Game {
        -gameID: int
        -gameDate: Date
        -opponent: String
    }
    
    class PlayerGame {
        -positionPlayed: String
        -comments: String
    }
    
    Player "1" --> "*" PlayerGame : participates in
    Game "1" --> "*" PlayerGame : includes
Important: When you have a Many-to-Many association, you will need to create an Association Class. The association class then captures the information related to that relationship.

## Example 2: Company Employees and Projects
### Scenario Description
A company has a number of employees, identified by Employee Numbers. The company wants to keep track of the employees' names, addresses and ages. The company also has several projects. Projects are assigned a unique project identifier, and also have the attributes project name and start date.

Each employee may be assigned to one or more projects, or may not be assigned to any projects. A project must have at least one employee assigned to it, and may have any number of employees assigned.

The company wishes to keep track of the employees' billing rates on every project, and when the employees started to work on their projects.

### Domain Class Diagram
classDiagram
    class Employee {
        -employeeNumber: int
        -name: String
        -address: String
        -age: int
    }
    
    class Project {
        -projectIdentifier: int
        -projectName: String
        -startDate: Date
    }
    
    class ProjectAssignment {
        -billingRate: double
        -startedWorkOn: Date
    }
    
    Employee "1" --> "*" ProjectAssignment : has
    Project "1" --> "*" ProjectAssignment : includes
    ProjectAssignment "*" --> "1" Employee : assigned to
    ProjectAssignment "*" --> "1" Project : belongs to

### Why is the Company not represented as a class?
In this case there will only be one instance of the Company, so there is no need to create a class (but it will be OK if you did).

### Association Class Details
The attributes billingRate and startedWorkOn are only applicable to the relationship between Employee and Project, so they belong in the ProjectAssignment association class.

### Generalization/Inheritance Association
Generalization/specialization relationships are based on the idea that people classify things in terms of similarities and differences. A generalization/specialization relationship is used to structure these things from the more general to the more special.

### Key Concepts
Term	Description
Superclass	The more general class above in the hierarchy
Subclass	The more specialized class below in the hierarchy
Inheritance	The subclass inherits all attributes of the superclass

## UML Notation
UML class diagram notation uses a triangle that points to the superclass to show a generalization/specialization hierarchy.

classDiagram
    Superclass <|-- Subclass1
    Superclass <|-- Subclass2
    class Superclass {
        -attribute1: type
        -attribute2: type
    }
    class Subclass1 {
        -specificAttribute1: type
    }
    class Subclass2 {
        -specificAttribute2: type
    }

### Example: Bank Account System
Scenario Description:
A bank keeps the following details about its Customers: name, address and phone number. A customer can have many accounts. The system must keep the account number, the date opened by the customer and the balance.

An account is either a cheque account or a savings account. The following is stored for each type of account:

Cheque accounts: cheque book style and the minimum balance

Savings accounts: the interest rate must be kept

### Domain Class Diagram
classDiagram
    class Customer {
        -name: String
        -address: String
        -phoneNumber: String
    }
    
    class Account {
        -accountNumber: int
        -dateOpened: Date
        -balance: double
    }
    
    class ChequeAccount {
        -chequeBookStyle: String
        -minimumBalance: double
    }
    
    class SavingsAccount {
        -interestRate: double
    }
    
    Customer "1" --> "*" Account : has
    Account <|-- ChequeAccount
    Account <|-- SavingsAccount

## Inheritance Reading
Customer has a one-to-many relationship with Account

Account is the superclass (general class)

ChequeAccount and SavingsAccount are subclasses (specialized classes)

ChequeAccount and SavingsAccount inherit all attributes from Account

## Guidelines for Generalization
All attributes in the superclass are inherited by subclasses

Subclasses can have additional attributes specific to them

The triangle points to the superclass (upward)

No multiplicity is shown on the generalization association

## Summary of Specialized Associations
### Comparison Table
Association Type	Number of Classes	Example	Key Feature
Binary	2	Team ↔ Player	Standard one-to-many or many-to-many
Unary (Recursive)	1 (same class)	Employee manages Employee	Same class connected to itself
Ternary	3	Employee ↔ Project ↔ Department	Three classes involved
Many-to-Many with Association Class	2 + association class	Order ↔ Product with OrderLine	Association has its own attributes
Generalization/Inheritance	Superclass + Subclass(s)	Account ↔ ChequeAccount	Inheritance of attributes


## Quick Reference
flowchart TD
    subgraph "Types of Associations"
        BA[Binary Association<br>Between 2 different classes]
        UA[Unary Association<br>Same class]
        TA[Ternary Association<br>Between 3 classes]
        MM[Many-to-Many<br>Requires Association Class]
        GI[Generalization/Inheritance<br>Superclass → Subclass]
    end
Key Points to Remember
Association Class: Used when a many-to-many association has its own attributes

Dotted Line: Connects the association class to the association

Generalization: Triangle points to the superclass

Inheritance: Subclasses inherit all attributes from the superclass

No Multiplicity: On generalization association lines