using System;
using Test.Models;
using Test.Services;

namespace Test.UI {
    internal class LibraryCli {
        private readonly DBContext       _db;
        private readonly LibraryService  _libraryService;
        private readonly BookService     _bookService;
        private readonly ReaderService   _readerService;
        private readonly AuthorService   _authorService;

        public LibraryCli(DBContext db) {
            _db             = db;
            _libraryService = new LibraryService(db);
            _bookService    = new BookService(db);
            _readerService  = new ReaderService(db);
            _authorService  = new AuthorService(db);
        }

        // ────────────────────────────────────────────────────────────
        public void Run() {
            PrintBanner();

            while (true) {
                PrintMainMenu();
                var choice = Prompt("Ваш вибір");

                try {
                    switch (choice) {
                        case "1": BookMenu();   break;
                        case "2": ReaderMenu(); break;
                        case "3": AuthorMenu(); break;
                        case "4": LoanMenu();   break;
                        case "0": Goodbye(); return;
                        default:  Warn("Невірна команда. Спробуйте ще раз."); break;
                    }
                } catch (Exception ex) {
                    Error($"Помилка: {ex.Message}");
                }
            }
        }

        // ── МЕНЮ КНИГ ────────────────────────────────────────────────
        private void BookMenu() {
            while (true) {
                Header("КНИГИ");
                Console.WriteLine("  1  Показати всі книги");
                Console.WriteLine("  2  Пошук за назвою");
                Console.WriteLine("  3  Пошук за автором");
                Console.WriteLine("  4  Пошук за жанром");
                Console.WriteLine("  5  Показати доступні");
                Console.WriteLine("  6  Додати книгу");
                Console.WriteLine("  7  Редагувати книгу");
                Console.WriteLine("  8  Видалити книгу");
                Console.WriteLine("  9  Додати автора до книги");
                Console.WriteLine(" 10  Додати жанр до книги");
                Console.WriteLine("  0  Назад");

                switch (Prompt("Вибір")) {
                    case "1": ListAllBooks();        break;
                    case "2": SearchBookByTitle();   break;
                    case "3": SearchBookByAuthor();  break;
                    case "4": SearchBookByGenre();   break;
                    case "5": ListAvailableBooks();  break;
                    case "6": AddBook();             break;
                    case "7": EditBook();            break;
                    case "8": DeleteBook();          break;
                    case "9": AddAuthorToBook();     break;
                    case "10": AddGenreToBook();     break;
                    case "0": return;
                    default:  Warn("Невірна команда."); break;
                }
            }
        }

        // ── МЕНЮ ЧИТАЧІВ ─────────────────────────────────────────────
        private void ReaderMenu() {
            while (true) {
                Header("ЧИТАЧІ");
                Console.WriteLine("  1  Показати всіх читачів");
                Console.WriteLine("  2  Знайти читача за ID");
                Console.WriteLine("  3  Зареєструвати читача");
                Console.WriteLine("  0  Назад");

                switch (Prompt("Вибір")) {
                    case "1": ListAllReaders();    break;
                    case "2": FindReaderById();    break;
                    case "3": RegisterReader();    break;
                    case "0": return;
                    default:  Warn("Невірна команда."); break;
                }
            }
        }

        // ── МЕНЮ АВТОРІВ ─────────────────────────────────────────────
        private void AuthorMenu() {
            while (true) {
                Header("АВТОРИ");
                Console.WriteLine("  1  Показати всіх авторів");
                Console.WriteLine("  2  Додати автора");
                Console.WriteLine("  0  Назад");

                switch (Prompt("Вибір")) {
                    case "1": ListAllAuthors(); break;
                    case "2": AddAuthor();      break;
                    case "0": return;
                    default:  Warn("Невірна команда."); break;
                }
            }
        }

        // ── МЕНЮ ПОЗИК ───────────────────────────────────────────────
        private void LoanMenu() {
            while (true) {
                Header("ПОЗИКИ");
                Console.WriteLine("  1  Видати книгу читачу");
                Console.WriteLine("  2  Зареєструвати повернення");
                Console.WriteLine("  3  Активні позики");
                Console.WriteLine("  4  Прострочені позики");
                Console.WriteLine("  0  Назад");

                switch (Prompt("Вибір")) {
                    case "1": IssueBook();       break;
                    case "2": ReturnBook();      break;
                    case "3": ActiveLoans();     break;
                    case "4": OverdueLoans();    break;
                    case "0": return;
                    default:  Warn("Невірна команда."); break;
                }
            }
        }

        // ── РЕАЛІЗАЦІЯ ДІЙ — КНИГИ ───────────────────────────────────
        private void ListAllBooks() {
            var books = _bookService.GetAllBooks().ToList();
            if (!books.Any()) { Info("Книг немає."); return; }
            Header($"Книги ({books.Count})");
            foreach (var b in books) PrintBook(b);
        }

        private void SearchBookByTitle() {
            var q = Prompt("Назва (або частина)");
            var books = _bookService.SearchByTitle(q).ToList();
            Info($"Знайдено: {books.Count}");
            foreach (var b in books) PrintBook(b);
        }

        private void SearchBookByAuthor() {
            var q = Prompt("Ім'я автора (або частина)");
            var books = _bookService.SearchByAuthor(q).ToList();
            Info($"Знайдено: {books.Count}");
            foreach (var b in books) PrintBook(b);
        }

        private void SearchBookByGenre() {
            var q = Prompt("Жанр (або частина)");
            var books = _bookService.SearchByGenre(q).ToList();
            Info($"Знайдено: {books.Count}");
            foreach (var b in books) PrintBook(b);
        }

        private void ListAvailableBooks() {
            var books = _bookService.GetAvailableBooks().ToList();
            Info($"Доступних книг: {books.Count}");
            foreach (var b in books) PrintBook(b);
        }

        private void AddBook() {
            Header("ДОДАТИ КНИГУ");
            var title = Prompt("Назва");
            var isbn  = Prompt("ISBN");
            var year  = PromptIntOptional("Рік видання (Enter — пропустити)");
            var count = PromptInt("Кількість примірників");

            // Бібліотека за замовчуванням — перша, або створити нову
            var libraryId = EnsureDefaultLibrary();

            var book = _bookService.AddBook(title, isbn, libraryId, count, year);
            Ok($"Книгу '{book.Title}' додано (ID ISBN: {book.ISBN}).");
        }

        private void EditBook() {
            var isbn  = Prompt("ISBN книги для редагування");
            var title = Prompt("Нова назва");
            var year  = PromptIntOptional("Новий рік (Enter — без змін)");
            var count = PromptInt("Нова кількість примірників");
            _bookService.UpdateBook(isbn, title, year, count);
            Ok("Книгу оновлено.");
        }

        private void DeleteBook() {
            var isbn = Prompt("ISBN книги для видалення");
            Warn($"Видалити книгу з ISBN '{isbn}'? (y/n)");
            if (Console.ReadLine()?.Trim().ToLower() != "y") { Info("Скасовано."); return; }
            _bookService.DeleteBook(isbn);
            Ok("Книгу видалено.");
        }

        private void AddAuthorToBook() {
            var isbn     = Prompt("ISBN книги");
            ListAllAuthors(silent: true);
            var authorId = PromptInt("ID автора");
            _bookService.AddAuthorToBook(isbn, authorId);
            Ok("Автора прив'язано до книги.");
        }

        private void AddGenreToBook() {
            var isbn    = Prompt("ISBN книги");
            ListAllGenres();
            var genreId = PromptInt("ID жанру");
            _bookService.AddGenreToBook(isbn, genreId);
            Ok("Жанр прив'язано до книги.");
        }

        // ── РЕАЛІЗАЦІЯ ДІЙ — ЧИТАЧІ ──────────────────────────────────
        private void ListAllReaders() {
            var readers = _readerService.GetAllReaders();
            if (!readers.Any()) { Info("Читачів немає."); return; }
            Header($"Читачі ({readers.Count})");
            foreach (var r in readers)
                Console.WriteLine($"  [{r.Id}] {r.FullName,-25} Картка: {r.CardNumber,-12} Тел: {r.PhoneNumber}");
        }

        private void FindReaderById() {
            var id     = PromptInt("ID читача");
            var reader = _readerService.GetReaderById(id);
            if (reader == null) { Warn("Читача не знайдено."); return; }
            Console.WriteLine($"  [{reader.Id}] {reader.FullName}");
            Console.WriteLine($"  Картка: {reader.CardNumber}  Тел: {reader.PhoneNumber}");
            Console.WriteLine($"  Адреса: {reader.Address}");
        }

        private void RegisterReader() {
            Header("РЕЄСТРАЦІЯ ЧИТАЧА");
            var fullName = Prompt("ПІБ");
            var card     = Prompt("Номер картки");
            var phone    = Prompt("Телефон");
            var address  = Prompt("Адреса");

            var reader = new Reader {
                FullName    = fullName,
                CardNumber  = card,
                PhoneNumber = phone,
                Address     = address
            };

            _readerService.RegisterReader(reader);
            Ok($"Читача '{fullName}' зареєстровано (ID: {reader.Id}).");
        }

        // ── РЕАЛІЗАЦІЯ ДІЙ — АВТОРИ ──────────────────────────────────
        private void ListAllAuthors(bool silent = false) {
            var authors = _authorService.GetAllAuthors();
            if (!silent) Header($"Автори ({authors.Count})");
            if (!authors.Any()) { Info("Авторів немає."); return; }
            foreach (var a in authors)
                Console.WriteLine($"  [{a.Id}] {a.FullName,-30} {(a.BirthDate.HasValue ? a.BirthDate.Value.ToString("dd.MM.yyyy") : "")}");
        }

        private void AddAuthor() {
            Header("ДОДАТИ АВТОРА");
            var name  = Prompt("ПІБ автора");
            var birth = PromptDateOptional("Дата народження (дд.мм.рррр, Enter — пропустити)");
            _authorService.AddAuthor(name, birth);
            Ok("Автора додано.");
        }

        // ── РЕАЛІЗАЦІЯ ДІЙ — ПОЗИКИ ──────────────────────────────────
        private void IssueBook() {
            Header("ВИДАТИ КНИГУ");
            ListAllReaders();
            var readerId = PromptInt("ID читача");
            var isbn     = Prompt("ISBN книги");
            var loan     = _libraryService.IssueBook(readerId, isbn);
            Ok($"Книгу видано. Позика №{loan.Id}. Планове повернення: {loan.PlannedReturnDate:dd.MM.yyyy}.");
        }

        private void ReturnBook() {
            Header("ПОВЕРНЕННЯ КНИГИ");
            ActiveLoans();
            var loanId = PromptInt("Номер позики (ID)");
            _libraryService.ReturnBook(loanId);
            Ok("Книгу повернуто.");
        }

        private void ActiveLoans() {
            var loans = _libraryService.GetActiveLoans();
            if (!loans.Any()) { Info("Активних позик немає."); return; }
            Header($"Активні позики ({loans.Count})");
            foreach (var l in loans) PrintLoan(l);
        }

        private void OverdueLoans() {
            var loans = _libraryService.GetOverdueLoans();
            if (!loans.Any()) { Info("Прострочених позик немає."); return; }
            Header($"Прострочені позики ({loans.Count})");
            foreach (var l in loans) PrintLoan(l);
        }

        // ── ДОПОМІЖНІ — ВИВІД ────────────────────────────────────────
        private static void PrintBook(Models.Book b) {
            var authors = b.BookAuthors.Count > 0
                ? string.Join(", ", b.BookAuthors.Select(ba => ba.Author?.FullName ?? "—"))
                : "—";
            var genres = b.BookGenres.Count > 0
                ? string.Join(", ", b.BookGenres.Select(bg => bg.Genre?.GenreName ?? "—"))
                : "—";
            Console.WriteLine($"  [{b.ISBN}] {b.Title,-30} | {authors,-20} | {b.BookStatus,-10} | прим.: {b.BooksCount} | {genres}");
        }

        private static void PrintLoan(Models.Loan l) {
            var books = l.BookLoans.Count > 0
                ? string.Join(", ", l.BookLoans.Select(bl => bl.Book?.Title ?? "—"))
                : "—";
            Console.WriteLine($"  #{l.Id:D4} | {l.Reader?.FullName ?? "—",-25} | {books,-30} | до {l.PlannedReturnDate:dd.MM.yyyy} | {l.LoanStatus}");
        }

        private void ListAllGenres() {
            var genres = _db.Genres.ToList();
            if (!genres.Any()) { Info("Жанрів немає."); return; }
            foreach (var g in genres)
                Console.WriteLine($"  [{g.Id}] {g.GenreName}");
        }

        private int EnsureDefaultLibrary() {
            var lib = _db.Libraries.FirstOrDefault();
            if (lib != null) return lib.Id;

            var newLib = new Models.Library { Name = "Центральна бібліотека", Address = "Головна вул., 1" };
            _db.Libraries.Add(newLib);
            _db.SaveChanges();
            return newLib.Id;
        }

        // ── ДОПОМІЖНІ — ВВЕДЕННЯ ─────────────────────────────────────
        private static string Prompt(string label) {
            Console.Write($"  {label}: ");
            return Console.ReadLine()?.Trim() ?? string.Empty;
        }

        private static int PromptInt(string label) {
            while (true) {
                var raw = Prompt(label);
                if (int.TryParse(raw, out var value)) return value;
                Warn("Очікується ціле число. Спробуйте ще раз.");
            }
        }

        private static int? PromptIntOptional(string label) {
            var raw = Prompt(label);
            return int.TryParse(raw, out var v) ? v : null;
        }

        private static DateTime? PromptDateOptional(string label) {
            var raw = Prompt(label);
            return DateTime.TryParseExact(raw, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var d) ? d : null;
        }

        // ── ДОПОМІЖНІ — СТИЛІ ────────────────────────────────────────
        private static void PrintBanner() {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════╗");
            Console.WriteLine("║        БІБЛІОТЕЧНА СИСТЕМА           ║");
            Console.WriteLine("╚══════════════════════════════════════╝");
            Console.ResetColor();
        }

        private static void PrintMainMenu() {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  ══ ГОЛОВНЕ МЕНЮ ══");
            Console.ResetColor();
            Console.WriteLine("  1  📚 Книги");
            Console.WriteLine("  2  👤 Читачі");
            Console.WriteLine("  3  ✍️  Автори");
            Console.WriteLine("  4  📋 Позики");
            Console.WriteLine("  0  🚪 Вихід");
        }

        private static void Header(string title) {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"  ── {title} ──");
            Console.ResetColor();
        }

        private static void Ok(string msg) {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  ✓ {msg}");
            Console.ResetColor();
        }

        private static void Info(string msg) {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"  {msg}");
            Console.ResetColor();
        }

        private static void Warn(string msg) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  ⚠ {msg}");
            Console.ResetColor();
        }

        private static void Error(string msg) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  ✗ {msg}");
            Console.ResetColor();
        }

        private static void Goodbye() {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  До побачення!");
            Console.ResetColor();
        }
    }
}