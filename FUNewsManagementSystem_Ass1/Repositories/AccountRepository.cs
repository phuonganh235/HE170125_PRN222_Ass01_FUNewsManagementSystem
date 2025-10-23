// AccountRepository.cs
using System.Collections.Generic;
using BusinessObjects;
using DataAccessObjects;

namespace Repositories
{
    public class AccountRepository : IAccountRepository
    {
        public IEnumerable<SystemAccount> GetAll()
        {
            return AccountDAO.GetAccounts();
        }

        public SystemAccount GetById(int accountId)
        {
            return AccountDAO.FindById(accountId);
        }

        public SystemAccount GetByEmail(string email)
        {
            return AccountDAO.FindByEmail(email);
        }

        public SystemAccount Login(string email, string password)
        {
            return AccountDAO.CheckLogin(email, password);
        }

        public void Add(SystemAccount account)
        {
            AccountDAO.Add(account);
        }

        public void Update(SystemAccount account)
        {
            AccountDAO.Update(account);
        }

        public void Delete(int accountId)
        {
            AccountDAO.Delete(accountId);
        }
    }
}
