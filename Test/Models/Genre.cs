using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {
    public class Genre {
        public int Id { get; set; }
        public String GenreName { get; set; } = String.Empty;

        // Історія 6: Бібліотекар - Редагування інформації про жанр
        public void Rename(string newName) {
            GenreName = newName;
        }

        public List<BookGenre> Books { get; set; } = new List<BookGenre>();
    }
}
