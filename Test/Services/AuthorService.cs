using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;

namespace Test.Services
{
    internal class AuthorService
    {
        private readonly DBContext _context;

        public AuthorService(DBContext context)
        {
            _context = context;
        }

        public void AddAuthor(string name, DateTime? birthDate = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Ім'я автора обов'язкове.");

            var author = new Author();
            // Використовуємо метод моделі для встановлення даних
            author.UpdateProfile(name, birthDate);

            _context.Authors.Add(author);
            _context.SaveChanges();
        }

        public List<Author> GetAllAuthors()
        {
            return _context.Authors
                .OrderBy(a => a.FullName) // Використовуємо FullName
                .ToList();
        }

        // Допоміжний метод: отримати автора за Id
        public Author? GetById(int authorId)
        {
            if (_context.GetType().GetProperty("Authors") == null)
                return null;

            var authorsList = (IEnumerable<Author>)_context.GetType().GetProperty("Authors")!.GetValue(_context)!;
            return authorsList.FirstOrDefault(a => a.Id == authorId);
        }
    }
}