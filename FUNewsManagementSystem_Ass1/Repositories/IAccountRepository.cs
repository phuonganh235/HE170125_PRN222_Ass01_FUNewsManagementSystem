// IAccountRepository.cs
using System.Collections.Generic;
using BusinessObjects;

namespace Repositories
{
    public interface IAccountRepository
    {
        IEnumerable<SystemAccount> GetAll();        // Lấy tất cả tài khoản
        SystemAccount GetById(int accountId);       // Tìm tài khoản theo ID
        SystemAccount GetByEmail(string email);     // Tìm tài khoản theo email
        SystemAccount Login(string email, string password); // Kiểm tra đăng nhập
        void Add(SystemAccount account);
        void Update(SystemAccount account);
        void Delete(int accountId);
    }
}
