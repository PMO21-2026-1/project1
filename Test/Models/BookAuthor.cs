using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {
    public class BookAuthor {
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public BookAuthor(int bookId, int authorId)
        {
            BookId = bookId;
            AuthorId = authorId;
        }

    }

    
}
