using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {

    public class Library {
        public int Id { get; set; }
        public String? Address { get; set; }
        public String? Name { get; set; }


        public List<Book> Books { get; set; } = new();
        public List<Reader> Readers { get; set; } = new();
        public List<Loan> Loans { get; set; } = new();

        // Історія 1: Читач - Шукати книги за назвою
        public List<Book> FindByTitle(string title) =>
            Books.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)).ToList();

        // Історія 2: Читач - Шукати книги за автором
        public List<Book> FindByAuthor(string authorName) =>
            Books.Where(b => b.BookAuthors.Any(ba => ba.Author.FullName.Contains(authorName))).ToList();

        // Історія 7: Бібліотекар - Видаляти книги
        public void RemoveBook(string isbn) {
            var book = Books.FirstOrDefault(b => b.ISBN == isbn);
            if (book != null) Books.Remove(book);
        }

        // Історія 10: Бібліотекар - Видавати книги
        public void IssueBooks(Reader reader, List<Book> books) {
            var loan = new Loan {
                ReaderId = reader.Id,
                Reader = reader,
                PlannedReturnDate = DateTime.Now.AddDays(14)
            };

            Loans.Add(loan);
        }

        // Історія 12: Бібліотекар - Переглядати список виданих книг
        public List<Loan> GetActiveLoans() =>
            Loans.Where(l => l.LoanStatus == LoanStatus.Active).ToList();
    }
}
