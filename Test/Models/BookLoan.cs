using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {
    public class BookLoan {
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public int LoanId { get; set; }
        public Loan? Loan { get; set; }

        public BookLoan() { }

        public BookLoan(int bookId, int loanId)
        {
            BookId = bookId;
            LoanId = loanId;
        }
    }
}
