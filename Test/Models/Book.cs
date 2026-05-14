using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Test.Models
{
    public class Book
    {
        [Key]
        public String ISBN { get; set; } = String.Empty;

        public String Title { get; set; } = String.Empty;
        public BookStatus BookStatus { get; set; } = BookStatus.Available;
        public int BooksCount { get; set; } = 0;
        public int? YearOfPublish { get; set; } = null;

        public int LibraryId { get; set; }
        public Library? Library { get; set; }

        public List<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
        public List<Loan> Loans { get; set; } = new List<Loan>();
        public List<BookGenre> BookGenres { get; set; } = new List<BookGenre>();

        // Історія 6: Бібліотекар - Бібліотекар - Редагування інформації про книгу
        public void UpdateDetails(string title, string isbn, int? year, int count)
        {
            if (count < 0) throw new ArgumentException("Кількість книг не може бути від'ємною.");

            Title = title;
            ISBN = isbn;
            YearOfPublish = year;
            BooksCount = count;

            UpdateStatus(); // Перевіряємо статус після зміни кількості
        }

        public bool IsAvailableForLoan()
        {
            // Книга доступна, якщо статус "Available" і кількість примірників > 0
            return BookStatus == BookStatus.Available && BooksCount > 0;
        }

        // Історія 10: Бібліотекар - Видача книги (зменшення кількості)
        public void MarkAsBorrowed()
        {
            if (!IsAvailableForLoan())
                throw new InvalidOperationException($"Книга '{Title}' наразі недоступна для видачі.");

            BooksCount--;
            UpdateStatus();
        }

        // Історія 11: Бібліотекар - Повернення книги (збільшення кількості)
        public void MarkAsReturned()
        {
            BooksCount++;
            UpdateStatus();
        }

        // Приватний допоміжний метод для автоматичного оновлення статусу
        private void UpdateStatus()
        {
            if (BooksCount == 0)
                BookStatus = BookStatus.Borrowed;
            else if (BookStatus == BookStatus.Borrowed && BooksCount > 0)
                BookStatus = BookStatus.Available;
        }

        // Історії 14-15: Бібліотекар -  Зв'язок з авторами та жанрами (N..M)
        public void AddAuthor(Author author)
        {
            // Перевіряємо, чи вже не додано такого автора (щоб уникнути дублів у БД)
            if (!BookAuthors.Any(ba => ba.AuthorId == author.Id))
            {
                BookAuthors.Add(new BookAuthor { Book = this, Author = author });
            }
        }
        // Додавання жанру до книги (запобігання дублікатів)
        public void AddGenre(Genre genre)
        {
            if (!BookGenres.Any(bg => bg.GenreId == genre.Id))
            {
                BookGenres.Add(new BookGenre { Book = this, Genre = genre });
            }
        }
    }
}
