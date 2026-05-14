using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Services;
using Test.Models;
using Microsoft.EntityFrameworkCore;

namespace Test.Services
{
    public class DBContext : DbContext
    {
        public DbSet<Reader> Readers { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Library> Libraries { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<BookGenre> BookGenres { get; set; }
        public DbSet<BookAuthor> BookAuthors { get; set; }
        public DbSet<BookLoan> BookLoans { get; set; }

        public DBContext()
        {
            // Створюємо базу даних, якщо її ще немає
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // "Data Source" вказує ім'я файлу, який створиться в папці з проектом
            optionsBuilder.UseSqlite("Data Source=mydata.db");
        }


    }
}
