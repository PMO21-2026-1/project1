using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;

namespace Test.Services
{
    public class AuthorService
    {
        private readonly Library _library;

        public AuthorService(Library library)
        {
            _library = library ?? throw new ArgumentNullException(nameof(library));
        }

        // Додає нового автора після базової валідації; перевіряє наявність дубліката за FullName
        public void AddAuthor(Author author)
        {
            if (author == null)
                throw new ArgumentNullException(nameof(author));

            if (string.IsNullOrWhiteSpace(author.FullName))
                throw new ArgumentException("Ім'я автора не може бути порожнім.", nameof(author.FullName));

            var name = author.FullName.Trim();

            // Перевіряємо на дублікати в централізованій колекції авторів (якщо вона є)
            if (_library.GetType().GetProperty("Authors") != null)
            {
                // передбачаємо, що Library має List<Author> Authors
                var authorsProp = (IEnumerable<Author>)_library.GetType().GetProperty("Authors")!.GetValue(_library)!;
                if (authorsProp.Any(a => string.Equals(a.FullName?.Trim(), name, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Автор з таким іменем вже існує.");

                // додаємо автора
                ((List<Author>)_library.GetType().GetProperty("Authors")!.GetValue(_library)!).Add(author);
                return;
            }

            // Якщо в цій версії Library немає Authors — намагаємося не створювати помилку,
            // але логічно зберегти автора через зв'язки з книгами або кинути помилку.
            throw new InvalidOperationException("Library не підтримує пряме збереження Authors в поточній версії моделі.");
        }

        // Видаляє автора за Id (видаляє також зв'язки BookAuthor у книгах)
        public void RemoveAuthor(int authorId)
        {
            if (_library.GetType().GetProperty("Authors") == null)
                throw new InvalidOperationException("Library не підтримує Authors в поточній версії моделі.");

            var authorsList = (List<Author>)_library.GetType().GetProperty("Authors")!.GetValue(_library)!;
            var author = authorsList.FirstOrDefault(a => a.Id == authorId);
            if (author == null)
                throw new InvalidOperationException("Автора не знайдено.");

            // Видаляємо зв'язки author -> book (BookAuthor) у всіх книгах
            foreach (var book in _library.Books)
            {
                var links = book.BookAuthors.Where(ba => ba.Author != null && ba.Author.Id == author.Id).ToList();
                foreach (var l in links)
                {
                    book.BookAuthors.Remove(l);
                }
            }

            authorsList.Remove(author);
        }

        // Пошук авторів за повним ім'ям (підрядок, регістронезалежно)
        public List<Author> SearchByFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new List<Author>();

            if (_library.GetType().GetProperty("Authors") == null)
                return new List<Author>();

            var authorsList = (IEnumerable<Author>)_library.GetType().GetProperty("Authors")!.GetValue(_library)!;

            return authorsList
                .Where(a => !string.IsNullOrEmpty(a.FullName) &&
                            a.FullName.Contains(fullName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Допоміжний метод: отримати автора за Id
        public Author? GetById(int authorId)
        {
            if (_library.GetType().GetProperty("Authors") == null)
                return null;

            var authorsList = (IEnumerable<Author>)_library.GetType().GetProperty("Authors")!.GetValue(_library)!;
            return authorsList.FirstOrDefault(a => a.Id == authorId);
        }
    }
}