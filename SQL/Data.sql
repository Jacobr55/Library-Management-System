USE LibrarySystemDb;
GO


-- Genres

INSERT INTO Genre (GenreName) VALUES
('Fiction'),
('Non-Fiction'),
('Science Fiction'),
('Fantasy'),
('Mystery'),
('Thriller'),
('Romance'),
('Horror'),
('Biography'),
('History'),
('Self-Help'),
('Children''s');
GO


-- Conditions

INSERT INTO Condition (ConditionName) VALUES
('New'),
('Good'),
('Fair'),
('Poor');
GO


-- Authors

INSERT INTO Author (FirstName, LastName, Bio) VALUES
('George',       'Orwell',      'English novelist best known for 1984 and Animal Farm.'),
('J.K.',         'Rowling',     'British author of the Harry Potter fantasy series.'),
('Stephen',      'King',        'American author known for his horror and suspense novels.'),
('Agatha',       'Christie',    'English writer known for her detective novels.'),
('J.R.R.',       'Tolkien',     'English author of The Lord of the Rings.'),
('Jane',         'Austen',      'English novelist known for Pride and Prejudice.'),
('Mark',         'Twain',       'American author of The Adventures of Tom Sawyer.'),
('Ernest',       'Hemingway',   'American novelist and Nobel Prize winner.'),
('F. Scott',     'Fitzgerald',  'American novelist known for The Great Gatsby.'),
('Harper',       'Lee',         'American novelist known for To Kill a Mockingbird.');
GO


-- BookTitles

INSERT INTO BookTitle (ISBN, AuthorID, GenreID, BookTitleName) VALUES
('978-0451524935', 1,  1,  '1984'),
('978-0743273565', 9,  1,  'The Great Gatsby'),
('978-0061096525', 10, 1,  'To Kill a Mockingbird'),
('978-0547928227', 5,  4,  'The Fellowship of the Ring'),
('978-0439708180', 2,  4,  'Harry Potter and the Sorcerer''s Stone'),
('978-0307743657', 3,  8,  'The Shining'),
('978-0062316097', 3,  8,  'It'),
('978-0062073488', 4,  5,  'Murder on the Orient Express'),
('978-0451526342', 6,  7,  'Pride and Prejudice'),
('978-0486280615', 7,  1,  'The Adventures of Tom Sawyer'),
('978-0684801469', 8,  1,  'The Old Man and the Sea'),
('978-0547928210', 5,  4,  'The Two Towers');
GO


-- BookCopies

INSERT INTO BookCopy (BookTitleID, PurchasedDate, RetiredDate) VALUES
(1,  '2020-01-15', NULL),
(1,  '2020-01-15', NULL),
(1,  '2021-06-10', '2024-03-01'),  -- retired copy
(2,  '2019-03-22', NULL),
(2,  '2022-08-05', NULL),
(3,  '2018-11-30', NULL),
(3,  '2020-05-14', NULL),
(4,  '2021-02-28', NULL),
(5,  '2019-07-19', NULL),
(5,  '2020-12-01', NULL),
(5,  '2023-01-10', NULL),
(6,  '2020-09-15', NULL),
(7,  '2021-04-22', NULL),
(8,  '2019-01-08', NULL),
(8,  '2022-03-17', NULL),
(9,  '2018-06-25', NULL),
(10, '2020-10-11', NULL),
(11, '2021-07-30', NULL),
(12, '2022-11-05', NULL);
GO


-- Members

INSERT INTO Members (Email, FirstName, LastName) VALUES
('alice.johnson@email.com',   'Alice',   'Johnson'),
('bob.smith@email.com',       'Bob',     'Smith'),
('carol.white@email.com',     'Carol',   'White'),
('david.brown@email.com',     'David',   'Brown'),
('emma.davis@email.com',      'Emma',    'Davis'),
('frank.miller@email.com',    'Frank',   'Miller'),
('grace.wilson@email.com',    'Grace',   'Wilson'),
('henry.moore@email.com',     'Henry',   'Moore'),
('isabella.taylor@email.com', 'Isabella','Taylor'),
('james.anderson@email.com',  'James',   'Anderson');
GO


-- Checkouts

INSERT INTO Checkout (BookCopyID, MemberID, ConditionID, CheckoutDate, DueDate, ReturnDate) VALUES
-- Returned checkouts
(1,  1, 1, '2025-01-05', '2025-01-19', '2025-01-17'),
(4,  2, 2, '2025-01-10', '2025-01-24', '2025-01-22'),
(6,  3, 1, '2025-02-01', '2025-02-15', '2025-02-14'),
(9,  4, 2, '2025-02-10', '2025-02-24', '2025-02-20'),
(14, 5, 1, '2025-03-01', '2025-03-15', '2025-03-10'),
(16, 6, 2, '2025-03-05', '2025-03-19', '2025-03-18'),
(2,  7, 1, '2025-03-15', '2025-03-29', '2025-03-28'),
(5,  1, 2, '2025-04-01', '2025-04-15', '2025-04-12'),
(8,  2, 1, '2025-04-05', '2025-04-19', '2025-04-18'),
(12, 3, 2, '2025-04-10', '2025-04-24', '2025-04-22'),

-- Currently checked out (no ReturnDate)
(1,  5, 2, '2026-04-01', '2026-04-15', NULL),
(4,  6, 1, '2026-04-03', '2026-04-17', NULL),
(7,  7, 2, '2026-04-05', '2026-04-19', NULL),
(10, 8, 1, '2026-04-10', '2026-04-24', NULL),
(13, 9, 2, '2026-04-12', '2026-04-26', NULL),
(15, 10,1, '2026-04-15', '2026-04-29', NULL),

-- Older checkouts for inactive member testing
(17, 8, 2, '2024-06-01', '2024-06-15', '2024-06-14'),
(18, 9, 1, '2024-03-10', '2024-03-24', '2024-03-20'),
(19, 10,2, '2023-11-05', '2023-11-19', '2023-11-15');
GO