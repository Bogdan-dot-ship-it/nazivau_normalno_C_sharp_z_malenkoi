using Core;
using DataAccess;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;
using System.Text;

namespace BusinessLogic
{
    public class UserService
    {
        private readonly UserRepository _userRepository = new UserRepository();

        private const int Pbkdf2Iterations = 100_000;
        private const int Pbkdf2SaltSizeBytes = 16;
        private const int Pbkdf2KeySizeBytes = 32;

        public void RegisterUser(string username, string password, string firstName, string lastName, string role = "USER")
        {
            if (_userRepository.GetUserByUsername(username) != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            string normalizedRole = (role ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(normalizedRole))
                normalizedRole = "USER";

            if (normalizedRole != "USER" && normalizedRole != "MASTER" && normalizedRole != "ADMIN")
                throw new InvalidOperationException("Invalid role.");

            if (normalizedRole == "USER" && _userRepository.GetUsersCount() == 0)
                normalizedRole = "ADMIN";

            string passwordHash = HashPassword(password);
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                Role = new UserRole { Code = normalizedRole },
                FirstName = firstName,
                LastName = lastName
            };
            _userRepository.AddUser(user);
        }

        public void EnsureDefaultAdminUser()
        {
            if (_userRepository.GetUserByUsername("admin") != null)
                return;

            RegisterUser("admin", "admin", "Admin", "Admin", "ADMIN");
        }

        public User? Authenticate(string username, string password)
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user != null && VerifyPassword(password, user.PasswordHash))
            {
                return user;
            }
            return null;
        }

        public List<User> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }

        public List<UserRole> GetAllRoles()
        {
            return _userRepository.GetAllRoles();
        }

        public bool UpdateUserRole(int userId, string roleCode)
        {
            return _userRepository.UpdateUserRole(userId, roleCode);
        }

        public List<User> GetUsersByRole(string role)
        {
            return _userRepository.GetUsersByRole(role);
        }

        private static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(Pbkdf2SaltSizeBytes);
            byte[] key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, Pbkdf2KeySizeBytes);
            return $"PBKDF2${Pbkdf2Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            if (storedHash.StartsWith("PBKDF2$", StringComparison.OrdinalIgnoreCase))
            {
                string[] parts = storedHash.Split('$');
                if (parts.Length != 4)
                {
                    return false;
                }

                if (!int.TryParse(parts[1], out int iterations))
                {
                    return false;
                }

                byte[] salt;
                byte[] expectedKey;
                try
                {
                    salt = Convert.FromBase64String(parts[2]);
                    expectedKey = Convert.FromBase64String(parts[3]);
                }
                catch
                {
                    return false;
                }

                byte[] actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);
                return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
            }

            string legacy = HashPasswordLegacySha256(password);
            return string.Equals(legacy, storedHash, StringComparison.OrdinalIgnoreCase);
        }

        private static string HashPasswordLegacySha256(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
