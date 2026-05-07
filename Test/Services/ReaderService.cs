using System;
using System.Collections.Generic;
using System.Linq;
using Test.Models;

namespace Test.Services
{
    public class ReaderService
    {
        private readonly Library _context;

        public ReaderService(Library context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Додає нового читача після валідації; перевіряє унікальність CardNumber
        public void AddReader(Reader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            if (string.IsNullOrWhiteSpace(reader.FullName))
                throw new ArgumentException("Ім'я читача не може бути порожнім.", nameof(reader.FullName));

            if (string.IsNullOrWhiteSpace(reader.CardNumber))
                throw new ArgumentException("Номер читацького квитка не може бути порожнім.", nameof(reader.CardNumber));

            bool cardExists = _context.Readers.Any(r => r.CardNumber == reader.CardNumber);
            if (cardExists)
                throw new InvalidOperationException("Читач з таким номером картки вже існує.");

            _context.Readers.Add(reader);
        }

        // Видаляє читача за його Id
        public void RemoveReader(int readerId)
        {
            var reader = _context.Readers.FirstOrDefault(r => r.Id == readerId);
            if (reader == null)
                throw new InvalidOperationException("Читача не знайдено.");

            _context.Readers.Remove(reader);
        }

        // Альтернативний метод видалення за номером картки
        public void RemoveReaderByCardNumber(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                throw new ArgumentException("Номер картки не може бути порожнім.", nameof(cardNumber));

            var reader = _context.Readers.FirstOrDefault(r => r.CardNumber == cardNumber);
            if (reader == null)
                throw new InvalidOperationException("Читача з таким номером картки не знайдено.");

            _context.Readers.Remove(reader);
        }

        // Пошук читачів за повним ім'ям (підрядок, регістронезалежно)
        public List<Reader> SearchByFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return new List<Reader>();

            return _context.Readers
                .Where(r => r.FullName != null &&
                            r.FullName.Contains(fullName, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Повернути читача за Id (допоміжний метод)
        public Reader? GetById(int readerId)
        {
            return _context.Readers.FirstOrDefault(r => r.Id == readerId);
        }
    }
}