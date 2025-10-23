using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using DataAccessObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects
{
    public static class AccountDAO
    {
        // Lấy danh sách tất cả tài khoản
        public static List<SystemAccount> GetAccounts()
        {
            using (var context = new FUNewsContext())
            {
                return context.SystemAccounts.ToList();
            }
        }

        // Tìm tài khoản theo ID
        public static SystemAccount FindById(int accountId)
        {
            using (var context = new FUNewsContext())
            {
                return context.SystemAccounts.Find(accountId);
            }
        }

        // Tìm tài khoản theo email (dùng cho đăng nhập hoặc kiểm tra trùng email)
        public static SystemAccount FindByEmail(string email)
        {
            using (var context = new FUNewsContext())
            {
                return context.SystemAccounts
                              .FirstOrDefault(acc => acc.AccountEmail.ToLower() == email.ToLower());
            }
        }

        // Thêm tài khoản mới vào DB
        public static void Add(SystemAccount account)
        {
            using (var context = new FUNewsContext())
            {
                context.SystemAccounts.Add(account);
                context.SaveChanges();
            }
        }

        // Cập nhật thông tin tài khoản
        public static void Update(SystemAccount account)
        {
            using (var context = new FUNewsContext())
            {
                context.SystemAccounts.Update(account);
                context.SaveChanges();
            }
        }

        // Xóa tài khoản theo ID
        public static void Delete(int accountId)
        {
            using (var context = new FUNewsContext())
            {
                var acc = context.SystemAccounts.Find(accountId);
                if (acc != null)
                {
                    context.SystemAccounts.Remove(acc);
                    context.SaveChanges();
                }
            }
        }

        // Kiểm tra đăng nhập: trả về tài khoản nếu email/mật khẩu khớp, ngược lại null
        public static SystemAccount CheckLogin(string email, string password)
        {
            using (var context = new FUNewsContext())
            {
                return context.SystemAccounts
                              .FirstOrDefault(acc => acc.AccountEmail == email
                                                   && acc.AccountPassword == password);
            }
        }
    }
}
