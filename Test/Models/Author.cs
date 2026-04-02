using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models {
    public class Author {
        public int Id { get; set; }
        public String FullName { get; set; } = String.Empty;
        public DateTime? BirthDate { get; set; }

        // 
        public void UpdateProfile(string name, DateTime? birth) {
            FullName = name;
            BirthDate = birth;
        }
    }
}
