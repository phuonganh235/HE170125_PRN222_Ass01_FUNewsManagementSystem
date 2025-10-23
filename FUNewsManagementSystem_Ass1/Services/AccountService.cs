// AccountService.cs
using System.Collections.Generic;
using BusinessObjects;
using Repositories;

namespace Services
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepo;
        private readonly INewsRepository newsRepo;

        public AccountService(IAccountRepository _accountRepo, INewsRepository _newsRepo)
        {
            accountRepo = _accountRepo;
            newsRepo = _newsRepo;
        }

        // Xử lý đăng nhập: trả về tài khoản nếu thành công, ngược lại null
        public SystemAccount Login(string email, string password)
        {
            return accountRepo.Login(email, password);
        }

        public IEnumerable<SystemAccount> GetAccounts()
        {
            return accountRepo.GetAll();
        }

        public SystemAccount GetAccount(int id)
        {
            return accountRepo.GetById(id);
        }

        // Tạo tài khoản mới (Admin tạo)
        public bool CreateAccount(SystemAccount account, out string error)
        {
            error = string.Empty;
            // Kiểm tra trùng email
            var existing = accountRepo.GetByEmail(account.AccountEmail);
            if (existing != null)
            {
                error = "Email đã được sử dụng bởi tài khoản khác!";
                return false;
            }
            // Có thể kiểm tra thêm các ràng buộc khác (ví dụ: độ mạnh mật khẩu, định dạng, v.v.)
            accountRepo.Add(account);
            return true;
        }

        // Cập nhật thông tin tài khoản
        public bool UpdateAccount(SystemAccount account, out string error)
        {
            error = string.Empty;
            // Kiểm tra email có trùng với tài khoản khác không (nếu email thay đổi)
            var existing = accountRepo.GetByEmail(account.AccountEmail);
            if (existing != null && existing.AccountId != account.AccountId)
            {
                error = "Email đã thuộc về một tài khoản khác!";
                return false;
            }
            accountRepo.Update(account);
            return true;
        }

        // Xóa tài khoản
        public bool DeleteAccount(int accountId, out string error)
        {
            error = string.Empty;
            // Không cho xóa nếu tài khoản đã tạo bài viết nào (đảm bảo tính toàn vẹn)
            var relatedNews = newsRepo.GetByAuthor(accountId);
            if (relatedNews != null && ((IList<NewsArticle>)relatedNews).Count > 0)
            {
                error = "Không thể xóa tài khoản vì đã có bài viết do tài khoản này tạo!";
                return false;
            }
            // (Ngoài ra có thể kiểm tra nếu tài khoản đang được dùng làm UpdatedBy nhưng đơn giản có thể bỏ qua 
            // hoặc coi cập nhật cũng là tác giả liên quan)
            accountRepo.Delete(accountId);
            return true;
        }
    }
}
