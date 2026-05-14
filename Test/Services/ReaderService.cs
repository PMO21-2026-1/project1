using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;

namespace Test.Services
{
    internal class ReaderService
    {
        private readonly DBContext _context;

        public ReaderService(DBContext context)
        {
            _context = context;
        }

        public void RegisterReader(Reader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            if (_context.Readers.Any(r => r.CardNumber == reader.CardNumber))
                throw new InvalidOperationException("Читач з таким номером картки вже існує.");

            _context.Readers.Add(reader);
            _context.SaveChanges();
        }

        // Додано '?', щоб дозволити повернення null
        public Reader? GetReaderById(int id)
        {
            return _context.Readers.Find(id);
        }

        public List<Reader> GetAllReaders()
        {
            return _context.Readers.ToList();
        }
    }
}