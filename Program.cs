using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicalBookstore
{
    // Enum for book categories
    public enum Category { SpellTome, EnchantedScroll, MagicalNovel }

    // ------------- CUSTOMER CLASS -------------
    // Represents a customer with rental history, coupons, & loyalty details
    public class Customer
    {
        public string Name { get; set; }
        public int RegistrationYear { get; set; }
        public List<Book> RentalHistory { get; set; } = new List<Book>();

        public double TotalSpent { get; set; } = 0;
        public int CouponsEarned { get; set; } = 0;
        public int CouponsUsed { get; set; } = 0;
        public List<string> RoomOfRequirementRewards { get; set; } = new List<string>();

        public int TotalBooksRented => RentalHistory.Count;
        public int AvailableCoupons => CouponsEarned - CouponsUsed;
    }

    // ------------- BOOK CLASS -------------
    // Represents a book with category and price
    public class Book
    {
        public string Title { get; set; }
        public Category Category { get; set; } // Use the existing Category enum
        public double Price { get; set; }
    }

    // ------------- CUSTOMER MANAGER -------------
    // Handles adding, finding, and dispalying customers
    public static class CustomerManager
    {
        public static List<Customer> Customers = new List<Customer>();

        // Adds a new customer with validation
        public static void AddCustomer()
        {
            // Ask for customer name and validate input (cannot be empty)
            Console.Write("Enter customer name: ");
            string name = Console.ReadLine().Trim();
            while (string.IsNullOrWhiteSpace(name))
            {
                Console.Write("Name cannot be empty. Try again: ");
                name = Console.ReadLine().Trim();
            }
            // Ask for registration year and ensure it’s a valid number and realistic year
            Console.Write("Enter registration year (e.g. 2023): ");
            string input = Console.ReadLine();
            int year;
            while (!int.TryParse(input, out year) || year < 1900 || year > DateTime.Now.Year)
            {
                Console.Write("Invalid year. Try again: ");
                input = Console.ReadLine();
            }
            // Save customer to the list with a formatted name (e.g. capitalize first letters)
            Customers.Add(new Customer { Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower()), RegistrationYear = year });
            // Confirmation message
            Console.WriteLine("Customer added successfully.");
        }

        // Shows all registered customers with key details
        public static void ShowCustomers()
        {
            Console.WriteLine("\n Registered Customers:");
            if (Customers.Count == 0)
            {
                Console.WriteLine("No customers found.");
                return;
            }
            foreach (var cust in Customers)
                Console.WriteLine($"- {cust.Name} (Registered: {cust.RegistrationYear})");
        }

        // Finds a customer by name (case-insensitive)
        public static Customer FindCustomer(string name)
        {
            return Customers.FirstOrDefault(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    // ------------- BOOK MANAGER -------------
    // Handles adding, finding, and displaying books
    public static class BookManager
    {
        public static List<Book> Books = new List<Book>();

        // Adds a new book with category-based price
        public static void AddBook()
        {
            Console.Write("Enter book title: ");
            string title = Console.ReadLine().Trim();
            while (string.IsNullOrWhiteSpace(title))
            {
                Console.Write("Title cannot be empty. Try again: ");
                title = Console.ReadLine().Trim();
            }

            Console.WriteLine("Available Categories:");
            foreach (var cat in Enum.GetValues(typeof(Category)))
                Console.WriteLine($"- {cat}");

            Console.Write("Enter category: ");
            Category category;
            while (!Enum.TryParse(Console.ReadLine(), true, out category))
            {
                Console.Write("Invalid category. Try again: ");
            }

            // Replace the switch expression with a traditional switch statement
            // Price assignment based on category
            double price;
            switch (category)
            {
                case Category.SpellTome:
                    price = 25;
                    break;
                case Category.EnchantedScroll:
                    price = 30;
                    break;
                case Category.MagicalNovel:
                    price = 40;
                    break;
                default:
                    price = 25;
                    break;
            }

            Books.Add(new Book { Title = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(title.ToLower()), Category = category, Price = price });
            Console.WriteLine($" Book added: {title} - { category} - R{price}");
        }

        // Shows all available books
        public static void ShowBooks()
        {
            Console.WriteLine("\n Available Books:");
            if (Books.Count == 0)
            {
                Console.WriteLine("No books found.");
                return;
            }

            foreach (var book in Books)
                Console.WriteLine($"- {book.Title} [{book.Category}] - R{book.Price}");
        }

        // Finds a book by title
        public static Book FindBook(string title)
        {
            return Books.FirstOrDefault(b => b.Title.Equals(title.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }

    // ------------- RENTAL PROCESSOR -------------
    // Manages book rentals and applies logic for discount and rewards
    public static class RentalProcessor
    {
        public static void StartRental()
        {
            Console.Write("Enter customer name: ");
            string customerName = Console.ReadLine().Trim();
            var customer = CustomerManager.FindCustomer(customerName);
            if (customer == null)
            {
                Console.WriteLine("Customer not found.");
                return;
            }

            // Cart holds books before checkout
            List<Book> cart = new List<Book>(); // Declare and initialize 'cart'
            Console.WriteLine("Enter book titles to rent (type 'done' to finish):");
            while (true)
            {
                Console.Write("Book Title: ");
                string title = Console.ReadLine().Trim();
                // Exit loop when user is done
                if (title.ToLower() == "done") break;
                // Search for the book in the system
                Book book = BookManager.FindBook(title);
                if (book != null)
                    cart.Add(book);
                else
                    Console.WriteLine($"Book '{title}' not found.");
            }
            // Check that at least one valid book was selected
            if (cart.Count == 0)
            {
                Console.WriteLine("No valid books selected.");
                return;
            }

            // Show cart items
            Console.WriteLine("\nBooks in Cart:");
            foreach (var book in cart)
                Console.WriteLine($"- {book.Title} [{book.Category}] - R{book.Price}");

            // Coupon usage logic
            bool usedCoupon = false;
            double couponValue = 0;
            if (customer.AvailableCoupons > 0)
            {
                Console.Write($"You have {customer.AvailableCoupons} coupon(s). Use one? (yes/no): ");
                if (Console.ReadLine().Trim().ToLower().StartsWith("y"))
                {
                    Book freeBook = cart.OrderBy(b => b.Price).First();
                    couponValue = freeBook.Price;
                    cart.Remove(freeBook);
                    customer.CouponsUsed++;
                    Console.WriteLine($"Coupon applied! '{freeBook.Title}' is free.");
                    usedCoupon = true;
                }
            }

            // Calculate totals and apply discount
            double totalBeforeDiscount = cart.Sum(b => b.Price) + couponValue;
            double discount = CalculateDiscount(customer, cart.Sum(b => b.Price));
            double finalCost = cart.Sum(b => b.Price) - discount;

            // Update customer data
            customer.TotalSpent += finalCost;
            customer.RentalHistory.AddRange(cart);

            // Apply rewards and coupons
            CheckCoupons(customer);
            AwardBonus(customer);

            // Print receipt
            Console.WriteLine("\n--- Rental Receipt ---");
            foreach (var book in cart)
                Console.WriteLine($"- {book.Title} ({book.Category}) - R{book.Price}");

            Console.WriteLine($"\nSubtotal: R{totalBeforeDiscount:F2}");
            if (usedCoupon) Console.WriteLine($"Coupon discount: -R{couponValue:F2}");
            Console.WriteLine($"Loyalty Discount: -R{discount:F2}");
            Console.WriteLine($"Total Payable: R{finalCost:F2}");
        }

        // Applies discounts based on time and number of books rented
        public static double CalculateDiscount(Customer customer, double totalCost)
        {
            int years = DateTime.Now.Year - customer.RegistrationYear;
            double rate;

            if (years >= 15)
                rate = 0.35;
            else if (years >= 10)
                rate = 0.20;
            else if (years >= 5)
                rate = 0.10;
            else
                rate = 0.05;

            return totalCost * rate;
        }

        //  Checks if the customer earns new coupons based on rentals
        public static void CheckCoupons(Customer customer)
        {
            int rentals = customer.TotalBooksRented;
            int prev = customer.CouponsEarned;

            if (rentals >= 75) customer.CouponsEarned = 8;
            else if (rentals >= 50) customer.CouponsEarned = 4;
            else if (rentals >= 25) customer.CouponsEarned = 2;
            else if (rentals >= 10) customer.CouponsEarned = 1;

            if (customer.CouponsEarned > prev)
                Console.WriteLine($"You earned {customer.CouponsEarned - prev} new coupon(s)!");
        }

        // Awards magical bonus if 3 or more magical books are rented
        public static void AwardBonus(Customer customer)
        {
            int years = DateTime.Now.Year - customer.RegistrationYear;
            int rentals = customer.TotalBooksRented;

            if (years >= 15 && rentals >= 75)
                customer.RoomOfRequirementRewards.AddRange(new[] { "5 Bronze", "2 Silver", "1 Gold" });
            else if (years >= 10 && rentals >= 50)
                customer.RoomOfRequirementRewards.AddRange(new[] { "3 Bronze", "1 Silver" });
            else if (years >= 5 && rentals >= 25)
                customer.RoomOfRequirementRewards.Add("1 Bronze");

            if (customer.RoomOfRequirementRewards.Count > 0)
            {
                Console.WriteLine("\nMagical Bonus Books from Room of Requirement:");
                foreach (var reward in customer.RoomOfRequirementRewards)
                    Console.WriteLine($"- {reward}");
            }
        }
    }

    // ------------- MENU ENUM -------------
    // Used for main menu options
    public enum MenuOptions
    {
        AddCustomer = 1,
        AddBook,
        ShowCustomers,
        ShowBooks,
        RentBooks,
        Exit
    }

    // ------------- PROGRAM ENTRY POINT -------------
    // It has main menu loop
    public class Program
    {
        public static void Main()
        {
            Console.Title = "Flourish & Blotts Loyalty System";

            while (true)
            {
                ShowMenu();
                MenuOptions choice = GetUserChoice();

                switch (choice)
                {
                    case MenuOptions.AddCustomer:
                        CustomerManager.AddCustomer();
                        break;
                    case MenuOptions.AddBook:
                        BookManager.AddBook();
                        break;
                    case MenuOptions.ShowCustomers:
                        CustomerManager.ShowCustomers();
                        break;
                    case MenuOptions.ShowBooks:
                        BookManager.ShowBooks();
                        break;
                    case MenuOptions.RentBooks:
                        RentalProcessor.StartRental();
                        break;
                    case MenuOptions.Exit:
                        Console.WriteLine("Thank you! Goodbye.");
                        return;
                }

                Console.WriteLine("\nPress ENTER to return to the main menu...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        //Shows the main menu (enum-based)
        public static void ShowMenu()
        {
            Console.WriteLine("=== Welcome to Flourish & Blotts Loyalty System ===");
            foreach (var opt in Enum.GetValues(typeof(MenuOptions)))
                Console.WriteLine($"{(int)opt}. {opt}");
        }

        // Gets a validated menu option from the user
        public static MenuOptions GetUserChoice()
        {
            Console.Write("\nSelect an option: ");
            MenuOptions choice; // Declare the variable 'choice' here
            while (!Enum.TryParse(Console.ReadLine(), true, out choice) || !Enum.IsDefined(typeof(MenuOptions), choice))
            {
                Console.WriteLine("Invalid choice. Try again:");
            }
            return choice; // Use the declared variable 'choice'
        }
    }
}

