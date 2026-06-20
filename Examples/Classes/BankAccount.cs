namespace Examples.Classes;

class BankAccount
{
    public static int TotalAccounts { get; private set; }

    public string Owner { get; }
    public decimal Balance { get; private set; }

    public BankAccount(string owner, decimal balance = 0)
    {
        Owner = owner;
        Balance = balance;

        TotalAccounts++;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            return; // raise some error here
        }

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount > Balance || amount <= 0)
        {
            return; // raise some error here
        }

        Balance -= amount;
    }

    public override string ToString()
        => $"{Owner}: {Balance:C}";

    public static void Run()
    {
        var bankAccount = new BankAccount(owner: "Bruce Wayne");

        bankAccount.Deposit(1_000_000_000);

        bankAccount.Withdraw(1_000_000);

        WriteLine(bankAccount);

        WriteLine(BankAccount.TotalAccounts);
    }
}
