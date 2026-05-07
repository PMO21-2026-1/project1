using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Models;

namespace Test.Services
{
    public class LibraryService
    {
        private readonly Library _library;

        public LibraryService(Library library)
        {
            _library = library;
        }

        public void AddBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (string.IsNullOrWhiteSpace(book.Title))
                throw new ArgumentException("Назва книги не може бути порожньою.");

            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("ISBN не може бути порожнім.");

            if (book.BooksCount < 0)
                throw new ArgumentException("Кількість книг не може бути від'ємною.");

            bool alreadyExists = _library.Books.Any(b => b.ISBN == book.ISBN);

            if (alreadyExists)
                throw new InvalidOperationException("Книга з таким ISBN вже існує.");

            _library.Books.Add(book);
        }

        public void RemoveBook(string isbn)
        {
            var book = _library.Books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null)
                throw new InvalidOperationException("Книгу з таким ISBN не знайдено.");

            _library.Books.Remove(book);
        }

        public List<Book> SearchByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return new List<Book>();

            return _library.Books
                .Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public List<Book> SearchByAuthor(string authorName)
        {
            if (string.IsNullOrWhiteSpace(authorName))
                return new List<Book>();

            return _library.Books
                .Where(b => b.BookAuthors.Any(ba =>
                    ba.Author != null &&
                    ba.Author.FullName.Contains(authorName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public void RegisterReader(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (string.IsNullOrWhiteSpace(reader.FullName))
                throw new ArgumentException("Ім'я читача не може бути порожнім.");

            if (string.IsNullOrWhiteSpace(reader.CardNumber))
                throw new ArgumentException("Номер читацького квитка не може бути порожнім.");

            bool cardExists = _library.Readers.Any(r => r.CardNumber == reader.CardNumber);

            if (cardExists)
                throw new InvalidOperationException("Читач з таким номером картки вже існує.");

            _library.Readers.Add(reader);
        }

        public Loan IssueBook(int readerId, string isbn)
        {
            var reader = _library.Readers.FirstOrDefault(r => r.Id == readerId);

            if (reader == null)
                throw new InvalidOperationException("Читача не знайдено.");

            var book = _library.Books.FirstOrDefault(b => b.ISBN == isbn);

            if (book == null)
                throw new InvalidOperationException("Книгу не знайдено.");

            if (!book.IsAvailableForLoan())
                throw new InvalidOperationException("Книга недоступна для видачі.");

            book.MarkAsBorrowed();

            var loan = new Loan
            {
                Id = _library.Loans.Count + 1,
                ReaderId = reader.Id,
                Reader = reader,
                IssueDate = DateTime.Now,
                PlannedReturnDate = DateTime.Now.AddDays(14),
                ExpirationDate = DateTime.Now.AddDays(14),
                LoanStatus = LoanStatus.Active
            };

            loan.BookLoans.Add(new BookLoan
            {
                Book = book,
                Loan = loan,
                LoanId = loan.Id
            });

            _library.Loans.Add(loan);

            return loan;
        }

        public void ReturnBook(int loanId)
        {
            var loan = _library.Loans.FirstOrDefault(l => l.Id == loanId);

            if (loan == null)
                throw new InvalidOperationException("Запис видачі не знайдено.");

            if (loan.LoanStatus == LoanStatus.Returned)
                throw new InvalidOperationException("Цю книгу вже повернули.");

            foreach (var bookLoan in loan.BookLoans)
            {
                if (bookLoan.Book != null)
                {
                    bookLoan.Book.MarkAsReturned();
                }
            }

            loan.MarkAsReturned();
        }

        public List<Loan> GetActiveLoans()
        {
            return _library.Loans
                .Where(l => l.LoanStatus == LoanStatus.Active)
                .ToList();
        }

        public List<Loan> GetOverdueLoans()
        {
            foreach (var loan in _library.Loans)
            {
                loan.CheckIfOverdue();
            }

            return _library.Loans
                .Where(l => l.LoanStatus == LoanStatus.Overdue)
                .ToList();
        }
    }
}
н