using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {
    public class Genre {
        public int Id { get; set; }
        public String GenreName { get; private set; } = String.Empty;

        // Історія 6: Бібліотекар - Редагування інформації про жанр
        public void Rename(string newName) {
            if (string.IsNullOrWhiteSpace(newName))
                throw new ArgumentException("Назва жанру не може бути порожньою.", nameof(newName));

            var trimmed = newName.Trim();
            const int MaxLength = 200;
            if (trimmed.Length > MaxLength)
                throw new ArgumentException($"Назва жанру не може бути довшою за {MaxLength} символів.", nameof(newName));

            if (trimmed == GenreName) return; // no-op якщо без змін

            GenreName = trimmed;
        }

        public List<BookGenre> Books { get; set; } = new List<BookGenre>();

        // Додати книгу до жанру (підтримка двостороннього зв'язку, уникнення дублікатів)
        public void AddBook(Book book) {
            if (book == null) throw new ArgumentNullException(nameof(book));

            // Якщо на боці книги вже є зв'язок з цим жанром — повторно використовуємо той самий об'єкт зв'язку
            var existingOnBook = book.BookGenres.FirstOrDefault(bg => ReferenceEquals(bg.Genre, this) || bg.Genre == this);
            if (existingOnBook != null) {
                if (!Books.Any(bg => ReferenceEquals(bg, existingOnBook)))
                    Books.Add(existingOnBook);
                return;
            }

            // Якщо на боці жанру вже є зв'язок з цією книгою — нічого не робимо
            if (Books.Any(bg => ReferenceEquals(bg.Book, book) || bg.Book == book))
                return;

            // Створюємо новий зв'язок і додаємо його обом сторонам
            var link = new BookGenre { Book = book, Genre = this };
            Books.Add(link);
            book.BookGenres.Add(link);
        }

        // Видалити книгу з жанру (підтримка двостороннього зв'язку)
        public void RemoveBook(Book book) {
            if (book == null) throw new ArgumentNullException(nameof(book));

            // Спробуємо знайти такий самий об'єкт зв'язку у списку книги — у ідеалі саме його слід видалити з обох сторін
            var existingOnBook = book.BookGenres.FirstOrDefault(bg => ReferenceEquals(bg.Genre, this) || bg.Genre == this);
            if (existingOnBook != null) {
                if (Books.Contains(existingOnBook))
                    Books.Remove(existingOnBook);
                book.BookGenres.Remove(existingOnBook);
                return;
            }

            // Інакше видаляємо всі зв'язки у жанру, що посилаються на задану книгу,
            // та намагаємось видалити відповідні зв'язки з книги (за посиланням або за властивостями)
            var links = Books.Where(bg => ReferenceEquals(bg.Book, book) || bg.Book == book).ToList();
            foreach (var l in links) {
                Books.Remove(l);
                // Видаляємо ту саму інстанцію з боку книги, якщо присутня
                if (book.BookGenres.Contains(l))
                    book.BookGenres.Remove(l);
                else {
                    // Інакше шукаємо будь-який інший зв'язок книги до цього жанру і видаляємо
                    var other = book.BookGenres.FirstOrDefault(bg => ReferenceEquals(bg.Genre, this) || bg.Genre == this);
                    if (other != null) book.BookGenres.Remove(other);
                }
            }
        }

        // Допоміжні запити
        public IEnumerable<Book> GetBooks() {
            return Books.Select(bg => bg.Book).Where(b => b != null).Select(b => b!);
        }

        public IEnumerable<Book> GetAvailableBooks() {
            return GetBooks().Where(b => b.IsAvailableForLoan());
        }

        public bool HasBook(Book book) {
            if (book == null) return false;
            return Books.Any(bg => ReferenceEquals(bg.Book, book) || bg.Book == book) ||
                   book.BookGenres.Any(bg => ReferenceEquals(bg.Genre, this) || bg.Genre == this);
        }
    }
}
