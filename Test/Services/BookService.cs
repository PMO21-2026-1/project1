using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Test.Services {
    internal class BookService {

        private readonly DBContext _context;

        public BookService(DBContext context) {
            _context = context;
        }

        // Історія 1-2: Пошук книг
        public IEnumerable<Book> GetAllBooks() {
            return _context.Books
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .Include(b => b.Library)
                .ToList();
        }

        public Book? GetBookByIsbn(string isbn) {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN не може бути порожнім.", nameof(isbn));

            return _context.Books
                .AsSplitQuery()
            .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
            .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
            .FirstOrDefault(b => b.ISBN == isbn);
        }

        public IEnumerable<Book> SearchByTitle(string title) {
            if (string.IsNullOrWhiteSpace(title))
                return Enumerable.Empty<Book>();

            return _context.Books
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.Title.Contains(title))
                .ToList();
        }

        public IEnumerable<Book> SearchByAuthor(string authorName) {
            if (string.IsNullOrWhiteSpace(authorName))
                return Enumerable.Empty<Book>();

            return _context.Books
                .AsNoTracking()
                .AsSplitQuery()
                .Include(b => b.BookAuthors)
                    .ThenInclude(ba => ba.Author)
                .Where(b => b.BookAuthors.Any(ba => ba.Author != null &&
                    ba.Author.FullName.Contains(authorName, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public IEnumerable<Book> SearchByGenre(string genreName) {
            if (string.IsNullOrWhiteSpace(genreName)) return Enumerable.Empty<Book>();

            return _context.Books
                .AsNoTracking() 
                .AsSplitQuery()
                .Include(b => b.BookGenres).ThenInclude(bg => bg.Genre)
                .Where(b => b.BookGenres.Any(bg => bg.Genre != null &&
                     bg.Genre.GenreName.Contains(genreName)))
                .ToList();
        }

        public IEnumerable<Book> GetAvailableBooks() {
            return _context.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Where(b => b.BookStatus == BookStatus.Available && b.BooksCount > 0)
                .ToList();
        }

        public IEnumerable<Book> GetBooksByLibrary(int libraryId) {
            return _context.Books
                .AsNoTracking()
                .Include(b => b.BookAuthors).ThenInclude(ba => ba.Author)
                .Where(b => b.LibraryId == libraryId)
                .ToList();
        }

        // --- МЕТОДИ МОДИФІКАЦІЇ (З трекінгом та SaveChanges) ---

        public Book AddBook(string title, string isbn, int libraryId, int count = 1, int? year = null) {
            // Швидка перевірка на дублікат
            if (_context.Books.Any(b => b.ISBN == isbn))
                throw new InvalidOperationException($"Книга з ISBN '{isbn}' вже існує.");

            // Перевірка існування бібліотеки
            if (!_context.Libraries.Any(l => l.Id == libraryId))
                throw new InvalidOperationException("Бібліотеку не знайдено.");

            var book = new Book {
                Title = title.Trim(),
                ISBN = isbn.Trim(),
                LibraryId = libraryId,
                BooksCount = count,
                YearOfPublish = year,
                BookStatus = count > 0 ? BookStatus.Available : BookStatus.Borrowed
            };

            _context.Books.Add(book);
            _context.SaveChanges();
            return book;
        }

        public void UpdateBook(string isbn, string title, int? year, int count) {
            // Тут AsNoTracking НЕ МОЖНА, бо ми міняємо дані
            var book = _context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if (book == null) throw new KeyNotFoundException("Книгу не знайдено.");

            book.UpdateDetails(title, isbn, year, count);
            _context.SaveChanges();
        }

        public void DeleteBook(string isbn) {
            // Завантажуємо книгу разом із активними позиками для перевірки
            var book = _context.Books
                .Include(b => b.Loans.Where(l => l.LoanStatus == LoanStatus.Active))
                .FirstOrDefault(b => b.ISBN == isbn);

            if (book == null) throw new KeyNotFoundException("Книгу не знайдено.");

            if (book.Loans.Any())
                throw new InvalidOperationException("Неможливо видалити книгу з активними позиками.");

            _context.Books.Remove(book);
            _context.SaveChanges();
        }

        public void AddAuthorToBook(string isbn, int authorId) {
            var book = _context.Books
                .Include(b => b.BookAuthors)
                .FirstOrDefault(b => b.ISBN == isbn);

            var author = _context.Authors.Find(authorId);

            if (book != null && author != null) {
                book.AddAuthor(author); // Викликаємо логіку з моделі
                _context.SaveChanges();
            }
        }

        public void AddGenreToBook(string isbn, int genreId) {
            var book = _context.Books
                .Include(b => b.BookGenres)
                .FirstOrDefault(b => b.ISBN == isbn);
            var genre = _context.Genres.Find(genreId);
            if (book != null && genre != null) {
                book.AddGenre(genre); // Викликаємо логіку з моделі
                _context.SaveChanges();
            }
        }

        // --- МАЛЕНЬКІ БІЗНЕС-МЕТОДИ ---

        public void BorrowBook(string isbn) {
            var book = _context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if (book != null) {
                book.MarkAsBorrowed();
                _context.SaveChanges();
            }
        }

        public void ReturnBook(string isbn) {
            var book = _context.Books.FirstOrDefault(b => b.ISBN == isbn);
            if (book != null) {
                book.MarkAsReturned();
                _context.SaveChanges();
            }
        }
    }
}