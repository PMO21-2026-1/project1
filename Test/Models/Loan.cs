using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {
    public class Loan {
        public int Id { get; set; }
        public DateTime IssueDate { get; set; } = DateTime.Now; // коли видали
        public DateTime ExpirationDate { get; set; } // Гранична дата за договором
        public DateTime PlannedReturnDate { get; set; } // Планова дата (на яку домовилися саме зараз)
        public DateTime? ReturnDate { get; set; } // файктична дата повернення (може бути null, якщо книга ще не повернута)
        public LoanStatus LoanStatus { get; set; } = LoanStatus.Active;

        public int ReaderId { get; set; }
        public Reader? Reader { get; set; }

        public List<BookLoan> BookLoans { get; set; } = new List<BookLoan>();

        // Історія 11: Бібліотекар - Реєструвати повернення
        public void MarkAsReturned() {
            ReturnDate = DateTime.Now;
            LoanStatus = LoanStatus.Returned;
            // У майбутньому тут можна додати логіку звільнення кожної книги з BookLoans
        }

        // Історія 13: Бібліотекар - Бачити прострочені книги
        public bool CheckIfOverdue() {
            if (ReturnDate == null && DateTime.Now > PlannedReturnDate) {
                LoanStatus = LoanStatus.Overdue;
                return true;
            }
            return false;
        }
    }
}