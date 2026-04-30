using System.Text;
using Test.Models;
using Test.Services;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

Library library = new Library
{
    Id = 1,
    Name = "Центральна бібліотека",
    Address = "Львів"
};

LibraryService libraryService = new LibraryService(library);

Console.WriteLine(" ПЕРЕВІРКА БІЗНЕС-ЛОГІКИ БІБЛІОТЕКИ ");
Console.WriteLine($"Бібліотека: {library.Name}");
Console.WriteLine($"Адреса: {library.Address}");
Console.WriteLine();


Author author = new Author
{
    Id = 1,
    FullName = "Тарас Шевченко",
    BirthDate = new DateTime(1814, 3, 9)
};

Genre genre = new Genre
{
    Id = 1,
    GenreName = "Поезія"
};

Book book = new Book
{
    Title = "Кобзар",
    ISBN = "978-966-03-0000-1",
    YearOfPublish = 1840,
    BooksCount = 2,
    LibraryId = library.Id,
    Library = library
};

book.AddAuthor(author);
book.AddGenre(genre);

Reader reader = new Reader
{
    Id = 1,
    FullName = "Саша",
    CardNumber = "R001",
    Address = "Львів",
    PhoneNumber = "+380683085280"
};


Console.WriteLine("1. ДОДАВАННЯ КНИГИ");

libraryService.AddBook(book);

Console.WriteLine("Книгу успішно додано.");
Console.WriteLine($"Назва: {book.Title}");
Console.WriteLine($"ISBN: {book.ISBN}");
Console.WriteLine($"Автор: {author.FullName}");
Console.WriteLine($"Жанр: {genre.GenreName}");
Console.WriteLine($"Кількість примірників: {book.BooksCount}");
Console.WriteLine($"Статус книги: {book.BookStatus}");
Console.WriteLine();


Console.WriteLine("2. РЕЄСТРАЦІЯ ЧИТАЧА");

libraryService.RegisterReader(reader);

Console.WriteLine("Читача успішно зареєстровано.");
Console.WriteLine($"ПІБ: {reader.FullName}");
Console.WriteLine($"Номер читацького квитка: {reader.CardNumber}");
Console.WriteLine($"Телефон: {reader.PhoneNumber}");
Console.WriteLine();


Console.WriteLine("3. ПОШУК КНИГИ ЗА НАЗВОЮ");

var booksByTitle = libraryService.SearchByTitle("Кобзар");

if (booksByTitle.Count == 0)
{
    Console.WriteLine("Книг за такою назвою не знайдено.");
}
else
{
    foreach (var foundBook in booksByTitle)
    {
        Console.WriteLine($"Знайдено книгу: {foundBook.Title}, ISBN: {foundBook.ISBN}");
    }
}

Console.WriteLine();


Console.WriteLine("4. ПОШУК КНИГИ ЗА АВТОРОМ");

var booksByAuthor = libraryService.SearchByAuthor("Тарас");

if (booksByAuthor.Count == 0)
{
    Console.WriteLine("Книг цього автора не знайдено.");
}
else
{
    foreach (var foundBook in booksByAuthor)
    {
        Console.WriteLine($"Знайдено книгу автора: {foundBook.Title}, ISBN: {foundBook.ISBN}");
    }
}

Console.WriteLine();


Console.WriteLine("5. ВИДАЧА КНИГИ ЧИТАЧУ");

Console.WriteLine($"Кількість примірників ДО видачі: {book.BooksCount}");

Loan loan = libraryService.IssueBook(reader.Id, book.ISBN);

Console.WriteLine("Книгу успішно видано читачу.");
Console.WriteLine($"ID видачі: {loan.Id}");
Console.WriteLine($"Читач: {loan.Reader?.FullName}");
Console.WriteLine($"Дата видачі: {loan.IssueDate:d}");
Console.WriteLine($"Планова дата повернення: {loan.PlannedReturnDate:d}");
Console.WriteLine($"Кількість примірників ПІСЛЯ видачі: {book.BooksCount}");
Console.WriteLine($"Статус книги: {book.BookStatus}");
Console.WriteLine($"Статус видачі: {loan.LoanStatus}");
Console.WriteLine();


Console.WriteLine("6. ПЕРЕГЛЯД АКТИВНИХ ВИДАЧ");

var activeLoansBeforeReturn = libraryService.GetActiveLoans();

if (activeLoansBeforeReturn.Count == 0)
{
    Console.WriteLine("Активних видач немає.");
}
else
{
    foreach (var activeLoan in activeLoansBeforeReturn)
    {
        Console.WriteLine($"Активна видача: ID {activeLoan.Id}, читач: {activeLoan.Reader?.FullName}, статус: {activeLoan.LoanStatus}");
    }
}

Console.WriteLine();


Console.WriteLine("7. ПОВЕРНЕННЯ КНИГИ");

Console.WriteLine($"Кількість примірників ДО повернення: {book.BooksCount}");

libraryService.ReturnBook(loan.Id);

Console.WriteLine("Книгу успішно повернено.");
Console.WriteLine($"Дата повернення: {loan.ReturnDate:d}");
Console.WriteLine($"Кількість примірників ПІСЛЯ повернення: {book.BooksCount}");
Console.WriteLine($"Статус книги: {book.BookStatus}");
Console.WriteLine($"Статус видачі: {loan.LoanStatus}");
Console.WriteLine();


Console.WriteLine("8. АКТИВНІ ВИДАЧІ ПІСЛЯ ПОВЕРНЕННЯ");

var activeLoansAfterReturn = libraryService.GetActiveLoans();

if (activeLoansAfterReturn.Count == 0)
{
    Console.WriteLine("Активних видач немає, бо книгу вже повернули.");
}
else
{
    foreach (var activeLoan in activeLoansAfterReturn)
    {
        Console.WriteLine($"Активна видача: ID {activeLoan.Id}, читач: {activeLoan.Reader?.FullName}");
    }
}

Console.WriteLine();


Console.WriteLine("9. ПЕРЕВІРКА ПРОСТРОЧЕНИХ ВИДАЧ");

Book overdueBook = new Book
{
    Title = "Лісова пісня",
    ISBN = "978-966-03-0000-2",
    YearOfPublish = 1911,
    BooksCount = 1,
    LibraryId = library.Id,
    Library = library
};

Author secondAuthor = new Author
{
    Id = 2,
    FullName = "Леся Українка",
    BirthDate = new DateTime(1871, 2, 25)
};

overdueBook.AddAuthor(secondAuthor);
libraryService.AddBook(overdueBook);

Loan overdueLoan = libraryService.IssueBook(reader.Id, overdueBook.ISBN);

overdueLoan.PlannedReturnDate = DateTime.Now.AddDays(-3);

var overdueLoans = libraryService.GetOverdueLoans();

if (overdueLoans.Count == 0)
{
    Console.WriteLine("Прострочених видач немає.");
}
else
{
    foreach (var item in overdueLoans)
    {
        Console.WriteLine($"Прострочена видача: ID {item.Id}");
        Console.WriteLine($"Читач: {item.Reader?.FullName}");
        Console.WriteLine($"Планова дата повернення: {item.PlannedReturnDate:d}");
        Console.WriteLine($"Статус видачі: {item.LoanStatus}");
    }
}

Console.WriteLine();


Console.WriteLine("10. ВИДАЛЕННЯ КНИГИ");

Console.WriteLine($"Кількість книг у бібліотеці ДО видалення: {library.Books.Count}");

libraryService.RemoveBook("978-966-03-0000-1");

Console.WriteLine("Книгу з ISBN 978-966-03-0000-1 успішно видалено.");
Console.WriteLine($"Кількість книг у бібліотеці ПІСЛЯ видалення: {library.Books.Count}");

Console.WriteLine();


Console.WriteLine(" ПЕРЕВІРКУ ЗАВЕРШЕНО ");
Console.WriteLine("Усі основні функції бізнес-логіки були протестовані.");
Console.ReadLine();