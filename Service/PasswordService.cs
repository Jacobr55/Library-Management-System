using Microsoft.AspNetCore.Identity;

namespace LibraryManagementSystem.Services
{
    public class PasswordService
    {

        private readonly PasswordHasher<string> _hasher = new();

        public string Hash(string password)
        {
            return _hasher.HashPassword("", password);
        }

        public bool Verify(string hash, string password)
        {
            var result = _hasher.VerifyHashedPassword("", hash, password);
            return result != PasswordVerificationResult.Failed;
        }
    }
}
