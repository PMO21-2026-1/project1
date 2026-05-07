using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;

namespace Test.Services
{
    public class LoanService
    {
        private readonly DBC _library;

        public LoanService(Library library)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
        }

        // Видача однієї книги
        public Loan IssueBook(int readerId, string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN не може бути порожнім.", nameof(isbn));

            var reader = _library.Readers.FirstOrDefault(r => r.Id == readerId)
                         ?? throw new InvalidOperationException("Читача не знайдено.");

            var book = _library.Books.FirstOrDefault(b => b.ISBN == isbn)
                       ?? throw new InvalidOperationException("Книгу не знайдено.");

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

        // Видача кількох книг в одному Loan
        public Loan IssueLoan(int readerId, IEnumerable<string> isbns)
        {
            if (isbns == null) throw new ArgumentNullException(nameof(isbns));
            var distinct = isbns.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!distinct.Any()) throw new ArgumentException("Потрібен хоча б один дійсний ISBN.", nameof(isbns));

            var reader = _library.Readers.FirstOrDefault(r => r.Id == readerId)
                         ?? throw new InvalidOperationException("Читача не знайдено.");

            var books = new List<Book>();
            foreach (var isbn in distinct)
            {
                var book = _library.Books.FirstOrDefault(b => b.ISBN == isbn)
                           ?? throw new InvalidOperationException($"Книгу з ISBN '{isbn}' не знайдено.");
                if (!book.IsAvailableForLoan())
                    throw new InvalidOperationException($"Книга '{book.Title}' недоступна для видачі.");
                books.Add(book);
            }

            foreach (var b in books) b.MarkAsBorrowed();

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

            foreach (var b in books)
            {
                loan.BookLoans.Add(new BookLoan
                {
                    Book = b,
                    Loan = loan,
                    LoanId = loan.Id
                });
            }

            _library.Loans.Add(loan);
            return loan;
        }

        // Повернення по Id видачі
        public void ReturnLoan(int loanId)
        {
            var loan = _library.Loans.FirstOrDefault(l => l.Id == loanId)
                       ?? throw new InvalidOperationException("Запис видачі не знайдено.");

            if (loan.LoanStatus == LoanStatus.Returned)
                throw new InvalidOperationException("Цю видачу вже повернули.");

            foreach (var bookLoan in loan.BookLoans)
            {
                if (bookLoan.Book != null)
                    bookLoan.Book.MarkAsReturned();
            }

            loan.MarkAsReturned();
        }

        public List<Loan> GetActiveLoans()
        {
            return _library.Loans.Where(l => l.LoanStatus == LoanStatus.Active).ToList();
        }

        public List<Loan> GetOverdueLoans()
        {
            foreach (var loan in _library.Loans)
                loan.CheckIfOverdue();

            return _library.Loans.Where(l => l.LoanStatus == LoanStatus.Overdue).ToList();
        }

        public Loan? GetById(int loanId) => _library.Loans.FirstOrDefault(l => l.Id == loanId);
    }
}