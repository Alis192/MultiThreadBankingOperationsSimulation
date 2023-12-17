# MTBankingOperationsSimulation

## Overview
MTBankingOperationsSimulation is a C# console application designed to simulate a multi-threaded banking environment. It demonstrates concurrent operations like deposits, withdrawals, and transfers across multiple bank accounts, ensuring thread safety and proper synchronization.

![image](https://github.com/Alis192/MultiThreadBankingOperationsSimulation/assets/67966115/dbc67478-abb2-4c22-a654-d58da8f95f8e)


## Features
- **Multi-threaded Operations**: Simulates banking transactions (deposits, withdrawals, and transfers) in a multi-threaded setup.
- **Thread Safety**: Implements locking mechanisms to prevent race conditions and ensure data integrity.
- **Exception Handling**: Robust handling of scenarios like insufficient funds during transactions.
- **Concurrent Collections**: Utilizes `ConcurrentDictionary` for thread-safe operations on shared data.

## Getting Started
### Prerequisites
- .NET SDK
- A C# compatible IDE (e.g., Visual Studio, VSCode with C# extension)

### Installation
1. Clone the repository: git clone https://github.com/Alis192/MultiThreadBankingOperationsSimulation.git
2. Open the solution file (`MTBankingOperationsSimulation.sln`) in your IDE.
3. Build and run the application.

## Usage
The application starts by initializing a bank with a set of accounts, each with an initial balance of $1000. It then spawns multiple threads simulating various banking operations:
- **Deposits Thread**: Randomly deposits money into different accounts.
- **Withdrawals Thread**: Randomly withdraws money, ensuring the account balance doesn't go negative.
- **Transfers Thread**: Randomly transfers money between accounts.
- **Monitoring Thread**: Continuously monitors and displays the total balance in the bank.

Each operation's outcome, along with any exceptions like `InsufficientBalanceException`, is logged to the console.

## Technical Details
### Thread Safety
The application uses `lock` statements to manage access to shared resources, ensuring that only one thread modifies the state of an account at any given time.

### Concurrent Collections
`ConcurrentDictionary` is used for storing and managing bank accounts, providing thread-safe access and updates.

### Exception Handling
Custom exceptions like `InsufficientBalanceException` are used to handle specific error scenarios gracefully.

