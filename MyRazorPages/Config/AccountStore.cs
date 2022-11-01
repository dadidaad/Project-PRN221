using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MyRazorPages.Models
{
    public class AccountStore : IUserPasswordStore<Account>, IUserEmailStore<Account> 
    {
        private readonly PRN221DBContext db;

        public AccountStore(PRN221DBContext db)
        {
            this.db = db;
        }

        public Task<IdentityResult> CreateAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task<Account> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await db.Accounts.FirstOrDefaultAsync(a => a.Email.Equals(normalizedEmail.ToLower()));
        }

        public async Task<Account> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return (await db.Accounts.FirstOrDefaultAsync(a => a.AccountId == Int32.Parse(userId)));
        }

        public async Task<Account> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return (await db.Accounts.FirstOrDefaultAsync(a => a.Email.Equals(normalizedUserName.ToLower())));
        }

        public async Task<string> GetEmailAsync(Account user, CancellationToken cancellationToken)
        {
            return (await db.Accounts.FirstOrDefaultAsync(a => a.Email.Equals(user.Email))).Email;
        }

        public Task<bool> GetEmailConfirmedAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedEmailAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(Account user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPasswordHashAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Password);
        }

        public async Task<string> GetUserIdAsync(Account user, CancellationToken cancellationToken)
        {
            return (await db.Accounts.FirstOrDefaultAsync(a => a.AccountId == user.AccountId)).AccountId.ToString();
        }

        public async Task<string> GetUserNameAsync(Account user, CancellationToken cancellationToken)
        {
            return (await db.Accounts.FirstOrDefaultAsync(a => a.Email.Equals(user.Email))).Email; 
        }

        public Task<bool> HasPasswordAsync(Account user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrWhiteSpace(user.Password));
        }

        public Task SetEmailAsync(Account user, string email, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetEmailConfirmedAsync(Account user, bool confirmed, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(Account user, string normalizedEmail, CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null);
        }

        public Task SetNormalizedUserNameAsync(Account user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.FromResult((object)null); 
        }

        public  Task SetPasswordHashAsync(Account user, string passwordHash, CancellationToken cancellationToken)
        {
            user.Password = passwordHash;
            return Task.FromResult((object)null);
        }

        public Task SetUserNameAsync(Account user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityResult> UpdateAsync(Account user, CancellationToken cancellationToken)
        {
            db.Accounts.Update(user);
            await db.SaveChangesAsync();
            return await Task.FromResult(IdentityResult.Success);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                db?.Dispose();
            }
        }


    }
}
