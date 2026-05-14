using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Author
    {
        public int Id { get; set; }
        public String FullName { get; private set; } = String.Empty;
        public DateTime? BirthDate { get; private set; }

        // Зв'язок N..M з книгами
        public List<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();

        // Оновлення профілю з валідацією (no-op якщо без змін)
        public void UpdateProfile(string name, DateTime? birth)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ім'я автора не може бути порожнім.", nameof(name));

            var trimmed = name.Trim();
            const int MaxLength = 200;
            if (trimmed.Length > MaxLength)
                throw new ArgumentException($"Ім'я автора не може бути довшим за {MaxLength} символів.", nameof(name));

            if (trimmed == FullName && Nullable.Equals(birth, BirthDate))
                return;

            FullName = trimmed;
            BirthDate = birth;
        }

        // Додає книгу до автора, синхронізує дві сторони зв'язку та запобігає дублікатам
        public void AddBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            // Якщо на боці книги вже є зв'язок до цього автора — повторно використовуємо існуючий об'єкт зв'язку
            var existingOnBook = book.BookAuthors.FirstOrDefault(ba =>
                ReferenceEquals(ba.Author, this) ||
                (ba.Author != null && this.Id != 0 && ba.Author.Id == this.Id)
            );

            if (existingOnBook != null)
            {
                if (!BookAuthors.Any(ba => ReferenceEquals(ba, existingOnBook)))
                    BookAuthors.Add(existingOnBook);
                return;
            }

            // Якщо автор вже має зв'язок до цієї книги — нічого не робимо
            if (BookAuthors.Any(ba => ReferenceEquals(ba.Book, book)))
                return;

            var link = new BookAuthor { Book = book, Author = this };
            BookAuthors.Add(link);
            book.BookAuthors.Add(link);
        }

        // Видаляє зв'язок книги з автором, синхронізує дві сторони
        public void RemoveBook(Book book)
        {
            if (book == null) throw new ArgumentNullException(nameof(book));

            var existingOnBook = book.BookAuthors.FirstOrDefault(ba =>
                ReferenceEquals(ba.Author, this) ||
                (ba.Author != null && this.Id != 0 && ba.Author.Id == this.Id)
            );

            if (existingOnBook != null)
            {
                if (BookAuthors.Contains(existingOnBook))
                    BookAuthors.Remove(existingOnBook);
                book.BookAuthors.Remove(existingOnBook);
                return;
            }

            var links = BookAuthors.Where(ba => ReferenceEquals(ba.Book, book)).ToList();
            foreach (var l in links)
            {
                BookAuthors.Remove(l);
                if (book.BookAuthors.Contains(l))
                    book.BookAuthors.Remove(l);
                else
                {
                    var other = book.BookAuthors.FirstOrDefault(ba =>
                        ReferenceEquals(ba.Author, this) ||
                        (ba.Author != null && this.Id != 0 && ba.Author.Id == this.Id)
                    );
                    if (other != null) book.BookAuthors.Remove(other);
                }
            }
        }

        // Допоміжні запити
        public IEnumerable<Book> GetBooks()
        {
            return BookAuthors.Select(ba => ba.Book).Where(b => b != null).Select(b => b!);
        }

        public IEnumerable<Book> GetAvailableBooks()
        {
            return GetBooks().Where(b => b.IsAvailableForLoan());
        }

        public bool HasBook(Book book)
        {
            if (book == null) return false;
            return BookAuthors.Any(ba => ReferenceEquals(ba.Book, book)) ||
                   book.BookAuthors.Any(ba => ReferenceEquals(ba.Author, this));
        }

        // Вік автора (null якщо невідомо)
        public int? Age
        {
            get
            {
                if (!BirthDate.HasValue) return null;
                var today = DateTime.UtcNow.Date;
                var age = today.Year - BirthDate.Value.Year;
                if (BirthDate.Value.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
