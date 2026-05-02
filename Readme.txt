CIS-560 Library Management System — Submission README

This submission contains a web-based Library Management System built with ASP.NET Core MVC and SQL Server. The system supports two main user types:

Library members — register, log in, browse the catalog, check out books, view their checked-out books, and return them.
Librarians — manage the database tables directly (add/edit Authors, Books, Book Copies, Members, and Checkouts) through a dedicated Librarian Access section



Tables
Location: Tables.sql
Contains all CREATE TABLE statements for the database. The schema includes:

Genre — lookup table for book genres
Condition — lookup table for book conditions (Good, Fair, New, Poor)
Author — author records with optional bio
BookTitle — distinct book titles, with foreign keys to Author and Genre
BookCopy — physical copies of book titles, with purchase and retirement dates
Members — library members (with a nullable PasswordHash column to support both registered and librarian-added members)
Checkout — checkout records linking a BookCopy, Member, and Condition



Data
Location: Data.sql
A complete seed script that wipes existing data (in foreign-key dependency order) and populates the database with realistic test data:

69 authors (classic and contemporary fiction, history, non-fiction, self-help)
140 book titles spanning all 12 genres
~241 book copies (some retired)
60 members (no PasswordHash set — representing pre-existing members. New members register through the application to get a password hash)
145 checkouts



Procedures / SQL Operations
Location: Procedures.sql
Contains every SQL statement invoked by the application, plus the four aggregating queries from the report's Aggregating Queries section.
This project does not use stored procedures. All SQL is executed as parameterized inline queries from C# using Microsoft.Data.SqlClient. 
To make these queries reviewable as standalone SQL, they have been extracted into Procedures.sql with a comment block above each one explaining:

The query's purpose
Which controller and action invokes it
The parameters it expects

The file starts with an Aggregating Queries section based on our Queries from our report. After that, the queries are organized by controller. 



Other Objects
This project does not use any SQL views, functions, or triggers.
(It does have a Views folder of course, for the Razor html views.)


Application Code
Location: LibraryManagementSystem/

The complete ASP.NET Core MVC project