using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace MTBankingOperationsSimulation
{

    class InsufficientBalanceException : InvalidOperationException
    {
        public InsufficientBalanceException() { }

        public InsufficientBalanceException(string message) : base(message) { }

        public InsufficientBalanceException(string message, Exception innerException) : base(message, innerException) { }
    }


    public class BankAccount
    {
        //Defining fields
        private readonly int _accountNumber;
        private double _balance;
        private static readonly object _accountLock = new object();

        //Properties
        public int AccountNumber { get => _accountNumber; init => _accountNumber = value; }


        public double Balance
        {
            get => _balance;
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException($"Balance can't be negative. You have supplied {value}", "Balance");
                }
                _balance = value;
            }
        }



        //Defining a constructor
        public BankAccount(double balance)
        {
            //Assigning Account number to default number
            AccountNumber = Settings.BaseAccountNo;


            Balance = balance;


            //Increasing default Account number for next Accounts
            Settings.BaseAccountNo++;
        }

        //Methods

        /// <summary>
        /// Top Ups the Balance
        /// </summary>
        /// <param name="amount"></param>
        public void Deposit(double amount, bool isTransfer = false)
        {
            lock (_accountLock)
            {
                if (amount < 0 || amount > 100000)
                {
                    throw new ArgumentOutOfRangeException("amount", "For funds transfer, the value of 'amount' should be between 1 to 10000");
                }
                Balance += amount;

                //If method is not called from Transfer method then default message will be displayed
                if (!isTransfer)
                {
                    Console.WriteLine($"Deposit amount ${amount} successfully transferred into account No {AccountNumber}. Current balance is ${Balance}");
                }
            }
        }

        /// <summary>
        /// Withdraw amount from balance
        /// </summary>
        /// <param name="amount"></param>
        public void WithDraw(double amount, bool isTransfer = false)
        {
            lock (_accountLock)
            {
                if (amount > Balance)
                    throw new InsufficientBalanceException($"The balance is insufficient. The current balance is {Balance}. But the amount to transfer is {amount}");

                Balance -= amount;

                //If method is not called from Transfer method then default message will be displayed
                if (!isTransfer)
                {
                    Console.WriteLine($"${amount} is withdrawed from Account No {AccountNumber}. Current balance is ${Balance}");
                }
            }
        }

        /// <summary>
        /// Transfer amount from current Bank Account to destinated Bank Account
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="amount"></param>
        /// <exception cref="InsufficientBalanceException"></exception>
        public void TransferTo(BankAccount destination, double amount)
        {
            lock(_accountLock)
            {
                if (Balance < amount)
                {
                    throw new InsufficientBalanceException($"The balance is insufficient. The current balance is {Balance}. But the amount to transfer is {amount}");
                }
                else
                {
                    Balance -= amount;
                    destination.Balance += amount;
                }
            }
        }
    }


    class Bank
    {
        //Defining fields
        private ConcurrentDictionary<int, BankAccount> _bankAccounts;
        private static readonly object _bankLock = new object();

        
        public ConcurrentDictionary<int, BankAccount> BankAccounts { get => _bankAccounts; set => _bankAccounts = value; }



        //Defining Constructor
        public Bank()
        {
            BankAccounts = new ConcurrentDictionary<int, BankAccount>();
        }

        

        /// <summary>
        /// Opens account with the given balance
        /// </summary>
        /// <param name="initialBalance"></param>
        /// <returns>BankAccount object</returns>
        public void OpenAccount(double initialBalance)
        {

            BankAccount account = new BankAccount(initialBalance);
            BankAccounts.TryAdd(account.AccountNumber, account);
            Console.WriteLine($"The bank account with Account No: {account.AccountNumber} is created.");

        }


        /// <summary>
        /// Terminates an account with a given account number
        /// </summary>
        /// <param name="accountNumber"></param>
        public void CloseAccount(int accountNumber)
        {

            //Checks if a Bank Account exists with the gives Account Number
            if (BankAccounts.TryGetValue(accountNumber, out BankAccount? bankAccount))
            {
                //If an account found with a given number, it is deleted from List(DB)
                BankAccounts.TryRemove(accountNumber, out _);
            }
            else
            {
                Console.WriteLine($"No account was found with a given Account number: {accountNumber}");
            }
        }

        /// <summary>
        /// Calculates total balance of all bank customers
        /// </summary>
        /// <returns>Total amount</returns>
        public void GetTotalBalance()
        {
            double sum = 0;

            foreach (var account in BankAccounts)
            {
                sum += account.Value.Balance;
            }
            Console.WriteLine($"Total ${sum} amount exists in the bank"); 
        }

        /// <summary>
        /// Transfers amount from source account to destination account
        /// </summary>
        /// <param name="sourceAccountNumber"></param>
        /// <param name="destinationAccountNumber"></param>
        /// <param name="amount"></param>
        public void Transfer(int sourceAccountNumber, int destinationAccountNumber, double amount)
        {
            if (!_bankAccounts.TryGetValue(sourceAccountNumber, out BankAccount? sourceAccount) || !_bankAccounts.TryGetValue(destinationAccountNumber, out BankAccount? destinationAccount))
            {
                throw new ArgumentOutOfRangeException("One or both account numbers are not found");
            }

            if (amount > _bankAccounts[sourceAccountNumber].Balance)
            {
                throw new InsufficientBalanceException($"The balance is insufficient. The current balance is {_bankAccounts[sourceAccountNumber].Balance}. But the amount to transfer is {amount}");
            }
            _bankAccounts[sourceAccountNumber].WithDraw(amount, true);
            _bankAccounts[destinationAccountNumber].Deposit(amount, true);
            Console.WriteLine($"${amount} is transferred from Account No: {sourceAccountNumber} to {destinationAccountNumber}.");                
        }


        /// <summary>
        /// Retrieves current balance of an account
        /// </summary>
        /// <param name="accountNumber"></param>
        public void GetAccountBalance(int accountNumber)
        {
            //Checks if a Bank Account exists with the gives Account Number
            if (_bankAccounts.TryGetValue(accountNumber, out BankAccount? bankAccount))
            {
                Console.WriteLine($"The current balance of Account: {accountNumber} is {_bankAccounts[accountNumber].Balance}");
            }
            else
            {
                Console.WriteLine("No account was found with the given ");
            }
        }

    }


    public class Program
    {
        static DateTime startTime = DateTime.Now;
        static readonly TimeSpan executionTime = TimeSpan.FromSeconds(60);


        static void Main(string[] args)
        {
            try
            { 
                // Initializing Bank class
                Bank bank = new Bank();
                    

                // Creating 5 account with the balance $1000 in each
                for (int i = 0; i < 5; i++)
                {
                    bank.OpenAccount(1000);
                }

                // Creating main thread object
                Thread mainThread = Thread.CurrentThread;
                mainThread.Name = "Main Thread";

                // Creating sub threads
                Thread depositThread = new Thread(() => { Deposits(bank);});
                Thread withdrawalThread = new Thread(() => { Withdrawal(bank); });
                Thread monitoringThread = new Thread(() => { Monitoring(bank); });
                Thread transferThread = new Thread(() => { Transfers(bank); });

                
                // Starting the threads
                depositThread.Start();
                withdrawalThread.Start();
                monitoringThread.Start();
                transferThread.Start();

                // Joining threads into Main thread. So that Main thread waits for them to finish
                depositThread.Join();
                withdrawalThread.Join();
                monitoringThread.Join();
                transferThread.Join();

                Console.WriteLine("End if the main thread");



            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }



        /// <summary>
        /// Simulates deposits over random bank accounts for a given time
        /// </summary>
        /// <param name="bank"></param>
        static void Deposits(Bank bank)
        {
            Random rn = new Random();

            // Executing the process for 60 secs
            while (DateTime.Now - startTime <= executionTime )
            {
                Console.ForegroundColor = ConsoleColor.Green;
                //Generates random Account number between 100 and 105 (105 is not included)
                int accountNumber = rn.Next(100, 105);

                // Minimum 100, maximum 500
                double amount = Math.Round(rn.NextDouble() * (500 - 100) + 100, 0);


                // Returns a bank account with specified Account number
                bank.BankAccounts.TryGetValue(accountNumber, out BankAccount? account);
                
                if (account != null)
                    account.Deposit(amount);
                Thread.Sleep(3000);

            }
        }
            

        /// <summary>
        /// Simulates withdrawals over random bank accounts for a given time
        /// </summary>
        static void Withdrawal(Bank bank)
        {
            try
            {
                Random rn = new Random();

                // Executing the process for 60 secs
                while (DateTime.Now - startTime <= executionTime)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    //Generates random Account number between 100 and 105 (105 is not included)
                    int accountNumber = rn.Next(100, 105);

                    // Minimum 100, maximum 500
                    double amount = Math.Round(rn.NextDouble() * (500 - 100) + 100, 0);


                    // Returns a bank account with specified Account number
                    bank.BankAccounts.TryGetValue(accountNumber, out BankAccount? account);

                    if (account != null)
                        account.WithDraw(amount);
                    Thread.Sleep(3000);

                }
            }
            catch(InsufficientBalanceException ex)
            {
                Console.WriteLine(ex.Message);  
            }
        }


        /// <summary>
        /// Simulates Transfers over random bank accounts for a given time
        /// </summary>
        static void Transfers(Bank bank)
        {
            try
            {
                Random rn = new Random();

                // Executing the process for 60 secs
                while (DateTime.Now - startTime <= executionTime)
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    //Generates random Account number between 100 and 105 (105 is not included)
                    int sourceAccountNumber = rn.Next(100, 105);
                    int destinationAccountNumber;
                    
                    //Making sure Source and Destination accounts have no same Account Number
                    do
                    {
                        destinationAccountNumber = rn.Next(100, 105);
                    } while (sourceAccountNumber == destinationAccountNumber);

                    // Minimum 100, maximum 500
                    double amount = Math.Round(rn.NextDouble() * (500 - 100) + 100, 0);


                    // Returns a bank account with specified Account number
                    bool sourceExists = bank.BankAccounts.TryGetValue(sourceAccountNumber, out BankAccount? sourceAccount);
                    bool destinationExists = bank.BankAccounts.TryGetValue(destinationAccountNumber, out BankAccount? destinationAccount);


                    // If both Soruce and Destination Accounts exist then we proceed the Transfer operation
                    if (sourceExists && destinationExists)
                        bank.Transfer(sourceAccountNumber, destinationAccountNumber, amount);
                    Thread.Sleep(3000);

                }
            }
            catch (InsufficientBalanceException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// Continuously monitor the total balance of all accounts and display it every 5 seconds
        /// </summary>
        static void Monitoring(Bank bank)
        {
            while (DateTime.Now - startTime <= executionTime)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                bank.GetTotalBalance();
                Thread.Sleep(5000);
            }

        }

    }
}

