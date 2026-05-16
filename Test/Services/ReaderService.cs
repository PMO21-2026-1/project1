using System.Text.RegularExpressions;
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
            ValidateReaderData(reader.FullName, reader.Address, reader.PhoneNumber);

            // Валідація унікального стандарту карти (строго 13 цифр)
            if (string.IsNullOrWhiteSpace(reader.CardNumber) || !Regex.IsMatch(reader.CardNumber.Trim(), @"^\d{13}$"))
                throw new InvalidOperationException("Помилка: Номер картки повинен складатися строго з 13 цифр (стандарт EAN-13).");

            _context.Readers.Add(reader);
            _context.SaveChanges();
        }

        public void UpdateReader(int id, string newName, string newAddr, string newPhone)
        {
            var reader = _context.Readers.Find(id);
            if (reader == null)
                throw new InvalidOperationException("Помилка: Читача з таким ID не знайдено.");

            // Перед оновленням проганяємо через ті самі суворі правила
            ValidateReaderData(newName, newAddr, newPhone);

            // Якщо все добре — оновлюємо поля
            reader.FullName = newName.Trim();
            reader.Address = newAddr.Trim();
            reader.PhoneNumber = newPhone.Trim();

            _context.SaveChanges();
        }

        // 3. ДОПОМІЖНИЙ МЕТОД ВАЛІДАЦІЇ (Щоб не дублювати код)
        private void ValidateReaderData(string name, string addr, string phone)
        {
            // Перевірка ПІБ повністю
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Помилка: ПІБ є обов'язковим для заповнення.");

            var nameParts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 3)
                throw new InvalidOperationException("Помилка: ПІБ має містити Прізвище, Ім'я та По батькові повністю.");

            // Перевірка телефону за міжнародним стандартом України
            if (string.IsNullOrWhiteSpace(phone))
                throw new InvalidOperationException("Помилка: Номер телефону є обов'язковим.");

            string cleanPhone = phone.Replace(" ", "").Replace("-", "");
            if (!Regex.IsMatch(cleanPhone, @"^(\+380|0)\d{9}$"))
                throw new InvalidOperationException("Помилка: Некоректний формат телефону. Використовуйте стандарт: +380XXXXXXXXX або 0XXXXXXXXX.");

            // Перевірка адреси повністю (мінімум 10 символів)
            if (string.IsNullOrWhiteSpace(addr) || addr.Trim().Length < 10)
                throw new InvalidOperationException("Помилка: Вкажіть повну адресу (наприклад: м. Львів, вул. Наукова, буд. 5, кв. 12).");
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