using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;
using System.Text;
using System.Threading.Tasks;

namespace Test.Services {
    internal class BookService {
        private readonly DBContext _context;

        public BookService(DBContext context) {
            _context = context;
        }

        public IEnumerable<Book> GetAllBooks() {
            return _context.Books.ToList();
        }
    }
}
