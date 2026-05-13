using Microsoft.EntityFrameworkCore.Internal;
using System.Text;
using Test.UI;


namespace Test
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var app = new LibraryCli(new Services.DBContext());
            app.Run();
        }
    }
}