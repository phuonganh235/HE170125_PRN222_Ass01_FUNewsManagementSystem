// IAccountService.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Services
{
    public interface IAccountService
    {
        SystemAccount Login(string email, string password);
        IEnumerable<SystemAccount> GetAccounts();
        SystemAccount GetAccount(int id);
        bool CreateAccount(SystemAccount account, out string error);
        bool UpdateAccount(SystemAccount account, out string error);
        bool DeleteAccount(int accountId, out string error);
    }
}
