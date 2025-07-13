using ProConnect.Core.Entities;

namespace ProConnect.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<string> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(string id);
        Task<bool> EmailExistsAsync(string email);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetByUserTypeAsync(UserType userType);
        Task<bool> UpdateProfileFieldsAsync(string userId, string firstName, string lastName, string phone, string bio);
    }
}
