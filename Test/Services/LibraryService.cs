using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Models;
using Microsoft.EntityFrameworkCore;

namespace Test.Services {
    internal class LibraryService {

        private readonly DBContext _context;

        public LibraryService(DBContext context)
        {
            _context = context;
        }

        public void AddBook(Book book) {
            if (book == null) throw new ArgumentNullException(nameof(book));

            // Перевірка бізнес-правил
            if (string.IsNullOrWhiteSpace(book.Title)) throw new ArgumentException("Назва не може бути порожньою.");
            if (string.IsNullOrWhiteSpace(book.ISBN)) throw new ArgumentException("ISBN не може бути порожнім.");

            // Перевірка на дублікат у базі
            if (_context.Books.Any(b => b.ISBN == book.ISBN))
                throw new InvalidOperationException("Книга з таким ISBN вже існує.");

            _context.Books.Add(book);
            _context.SaveChanges();
        }

        public List<Book> SearchByTitle(string title) {
            if (string.IsNullOrWhiteSpace(title)) return new List<Book>();

            return _context.Books
                .AsNoTracking() // Тільки для читання
                .AsSplitQuery()
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Where(b => b.Title.Contains(title)) // SQL сам ігнорує регістр (зазвичай)
                .ToList();
        }

        // --- РОБОТА З ЧИТАЧАМИ ---

        public void RegisterReader(Reader reader) {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (_context.Readers.Any(r => r.CardNumber == reader.CardNumber))
                throw new InvalidOperationException("Читач з таким номером картки вже існує.");

            _context.Readers.Add(reader);
            _context.SaveChanges();
        }

        // --- ВИДАЧА ТА ПОВЕРНЕННЯ (Найважливіша логіка) ---

        public Loan IssueBook(int readerId, string isbn) {
            // 1. Шукаємо читача
            var reader = _context.Readers.Find(readerId);
            if (reader == null) throw new InvalidOperationException("Читача не знайдено.");

            // 2. Шукаємо книгу (завантажуємо її, бо будемо міняти статус)
            var book = _context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if (book == null) throw new InvalidOperationException("Книгу не знайдено.");

            if (!book.IsAvailableForLoan())
                throw new InvalidOperationException("Книга недоступна для видачі.");

            // 3. Логіка моделі: зменшуємо кількість, міняємо статус
            book.MarkAsBorrowed();

            // 4. Створюємо запис видачі
            var loan = new Loan {
                ReaderId = reader.Id,
                IssueDate = DateTime.Now,
                PlannedReturnDate = DateTime.Now.AddDays(14),
                ExpirationDate = DateTime.Now.AddDays(14),
                LoanStatus = LoanStatus.Active
            };

            // Зв'язуємо книгу з видачею через проміжну таблицю
            loan.BookLoans.Add(new BookLoan {
                Book = book,
                Loan = loan
            });

            _context.Loans.Add(loan);
            _context.SaveChanges(); // Зберігаємо все одним махом (транзакція)

            return loan;
        }

        public void ReturnBook(int loanId) {
            // Завантажуємо видачу разом із книгами
            var loan = _context.Loans
                .Include(l => l.BookLoans)
                    .ThenInclude(bl => bl.Book)
                .FirstOrDefault(l => l.Id == loanId);

            if (loan == null) throw new InvalidOperationException("Запис видачі не знайдено.");
            if (loan.LoanStatus == LoanStatus.Returned) throw new InvalidOperationException("Вже повернуто.");

            // Повертаємо всі книги у цій видачі
            foreach (var bookLoan in loan.BookLoans) {
                bookLoan.Book?.MarkAsReturned();
            }

            loan.MarkAsReturned(); // Метод у моделі Loan
            _context.SaveChanges();
        }

        // --- ЗВІТНІСТЬ ---

        public List<Loan> GetActiveLoans() {
            return _context.Loans
                .AsNoTracking()
                .Include(l => l.Reader)
                .Include(l => l.BookLoans).ThenInclude(bl => bl.Book)
                .Where(l => l.LoanStatus == LoanStatus.Active)
                .ToList();
        }

        public List<Loan> GetOverdueLoans() {
            // Оновлюємо статуси прострочених в базі перед виводом
            var now = DateTime.Now;
            var loansToUpdate = _context.Loans
                .Where(l => l.LoanStatus == LoanStatus.Active && l.ExpirationDate < now)
                .ToList();

            foreach (var loan in loansToUpdate) {
                loan.CheckIfOverdue();
            }

            if (loansToUpdate.Any()) _context.SaveChanges();

            return _context.Loans
                .AsNoTracking()
                .Include(l => l.Reader)
                .Where(l => l.LoanStatus == LoanStatus.Overdue)
                .ToList();
        }
    }
}
