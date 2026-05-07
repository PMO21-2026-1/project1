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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Додавання читача
        public void AddReader(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (string.IsNullOrWhiteSpace(reader.FullName))
                throw new ArgumentException(
                    "Ім'я читача не може бути порожнім.",
                    nameof(reader.FullName));

            if (string.IsNullOrWhiteSpace(reader.CardNumber))
                throw new ArgumentException(
                    "Номер читацького квитка не може бути порожнім.",
                    nameof(reader.CardNumber));

            var cardNumber = reader.CardNumber.Trim();
            var fullName = reader.FullName.Trim();

            // Перевірка унікальності номера картки
            var exists = _context.Readers
                .Any(r => r.CardNumber == cardNumber);

            if (exists)
                throw new InvalidOperationException(
                    "Читач з таким номером картки вже існує.");

            reader.CardNumber = cardNumber;
            reader.FullName = fullName;

            _context.Readers.Add(reader);

            _context.SaveChanges();
        }

        // Видалення читача
        public void RemoveReader(int readerId)
        {
            var reader = _context.Readers.Find(readerId);

            if (reader == null)
                throw new InvalidOperationException(
                    "Читача не знайдено.");

            // Перевірка активних або прострочених видач
            var hasLoans = _context.Loans.Any(l =>
                l.ReaderId == readerId &&
                (l.LoanStatus == LoanStatus.Active ||
                 l.LoanStatus == LoanStatus.Overdue));

            if (hasLoans)
                throw new InvalidOperationException(
                    "Неможливо видалити читача: є активні або прострочені видачі.");

            _context.Readers.Remove(reader);

            _context.SaveChanges();
        }

        // Пошук за ім'ям
        public List<Reader> SearchByFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new List<Reader>();

            var query = fullName.Trim().ToLower();

            return _context.Readers
                .Where(r =>
                    !string.IsNullOrWhiteSpace(r.FullName) &&
                    r.FullName.ToLower().Contains(query))
                .ToList();
        }

        // Отримання читача по Id
        public Reader? GetById(int readerId)
        {
            return _context.Readers.Find(readerId);
        }
    }
}