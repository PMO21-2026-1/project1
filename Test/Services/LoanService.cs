using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Test.Models;

namespace Test.Services
{
    internal class LoanService
    {
        private readonly DBContext _context;

        public LoanService(DBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Видача однієї книги
        public Loan IssueBook(int readerId, string isbn)
        {
            return IssueLoan(readerId, new[] { isbn });
        }

        // Видача кількох книг
        public Loan IssueLoan(int readerId, IEnumerable<string> isbns)
        {
            if (isbns == null)
                throw new ArgumentNullException(nameof(isbns));

            var isbnList = isbns
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!isbnList.Any())
                throw new ArgumentException("Потрібно передати хоча б один ISBN.");

            var reader = _context.Readers.Find(readerId);

            if (reader == null)
                throw new InvalidOperationException("Читача не знайдено.");

            // Один SQL-запит замість багатьох
            var books = _context.Books
                .Where(b => isbnList.Contains(b.ISBN))
                .ToList();

            if (books.Count != isbnList.Count)
                throw new InvalidOperationException("Деякі книги не знайдено.");

            foreach (var book in books)
            {
                if (!book.IsAvailableForLoan())
                    throw new InvalidOperationException(
                        $"Книга '{book.Title}' недоступна для видачі.");
            }

            var now = DateTime.Now;

            var loan = new Loan
            {
                ReaderId = reader.Id,
                Reader = reader,
                IssueDate = now,
                PlannedReturnDate = now.AddDays(14),
                ExpirationDate = now.AddDays(14),
                LoanStatus = LoanStatus.Active
            };

            foreach (var book in books)
            {
                book.MarkAsBorrowed();

                loan.BookLoans.Add(new BookLoan
                {
                    Book = book,
                    Loan = loan
                });
            }

            _context.Loans.Add(loan);
            _context.SaveChanges();

            return loan;
        }

        // Повернення видачі
        public void ReturnLoan(int loanId)
        {
            var loan = _context.Loans
                .Include(l => l.BookLoans)
                .ThenInclude(bl => bl.Book)
                .FirstOrDefault(l => l.Id == loanId);

            if (loan == null)
                throw new InvalidOperationException("Видачу не знайдено.");

            if (loan.LoanStatus == LoanStatus.Returned)
                throw new InvalidOperationException("Видачу вже повернено.");

            foreach (var bookLoan in loan.BookLoans)
            {
                if (bookLoan.Book != null)
                    bookLoan.Book.MarkAsReturned();
            }

            loan.MarkAsReturned();

            _context.SaveChanges();
        }

        // Активні видачі
        public List<Loan> GetActiveLoans()
        {
            return _context.Loans
                .Include(l => l.Reader)
                .Include(l => l.BookLoans)
                .ThenInclude(bl => bl.Book)
                .Where(l => l.LoanStatus == LoanStatus.Active)
                .ToList();
        }

        // Прострочені видачі
        public List<Loan> GetOverdueLoans()
        {
            var loans = _context.Loans
                .Include(l => l.Reader)
                .Include(l => l.BookLoans)
                .ThenInclude(bl => bl.Book)
                .ToList();

            foreach (var loan in loans)
            {
                loan.CheckIfOverdue();
            }

            _context.SaveChanges();

            return loans
                .Where(l => l.LoanStatus == LoanStatus.Overdue)
                .ToList();
        }

        // Пошук по Id
        public Loan? GetById(int loanId)
        {
            return _context.Loans
                .Include(l => l.Reader)
                .Include(l => l.BookLoans)
                .ThenInclude(bl => bl.Book)
                .FirstOrDefault(l => l.Id == loanId);
        }

        // Видалення видачі
        public void RemoveLoan(int loanId)
        {
            var loan = _context.Loans
                .Include(l => l.BookLoans)
                .ThenInclude(bl => bl.Book)
                .FirstOrDefault(l => l.Id == loanId);

            if (loan == null)
                throw new InvalidOperationException("Видачу не знайдено.");

            // Повертаємо книги якщо видача ще не повернена
            if (loan.LoanStatus != LoanStatus.Returned)
            {
                foreach (var bookLoan in loan.BookLoans)
                {
                    if (bookLoan.Book != null)
                        bookLoan.Book.MarkAsReturned();
                }
            }

            _context.Loans.Remove(loan);

            _context.SaveChanges();
        }
    }
}