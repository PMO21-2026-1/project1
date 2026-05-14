using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test.Models
{
    public class Reader
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = String.Empty;
        public string FullName { get; set; } = String.Empty;
        public string Address { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;

        // Історія 9: Бібліотекар - Оновлювати інформацію про читача
        public void UpdateInfo(string name, string addr, string phone)
        {
            if (!string.IsNullOrWhiteSpace(name)) FullName = name;
            Address = addr;
            PhoneNumber = phone;
        }
    }
}
