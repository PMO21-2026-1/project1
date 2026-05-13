using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;
using Microsoft.EntityFrameworkCore;

namespace Test.Services
{
    internal class ReaderService
    {
        private readonly DBContext _context;

        public ReaderService(DBContext context)
        {
            _context = context;
        }

        // Реєстрація читача (можна перенести з LibraryService)
        public void RegisterReader(Reader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (_context.Readers.Any(r => r.CardNumber == reader.CardNumber))
                throw new InvalidOperationException("Читач з таким номером картки вже існує.");

            _context.Readers.Add(reader);
            _context.SaveChanges();
        }

        // Пошук читача за ID
        public Reader GetReaderById(int id)
        {
            return _context.Readers.Find(id);
        }

        // Отримати список усіх зареєстрованих читачів
        public List<Reader> GetAllReaders()
        {
            return _context.Readers.ToList();
        }
    }
}