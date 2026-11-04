namespace Accounts.Domain.Models.Accounts;

public class AccountModelConstants
{
    public class Account
    {
        public const int MaxNameLength = 100;
        public const int MaxEmailLength = 254;

        // ACC-F3 / ADR-001: 10 to 128 characters, no composition rules.
        public const int MinPasswordLength = 10;
        public const int MaxPasswordLength = 128;
    }
}
