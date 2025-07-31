using ProConnect.Core.Entities;

namespace ProConnect.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByDocumentIdAsync(string documentId);
        Task<string> CreateAsync(User user);
        Task<bool> UpdateAsync(User user);
        Task<bool> DeleteAsync(string id);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> DocumentIdExistsAsync(string documentId);
        Task<List<User>> GetAllAsync();
        Task<List<User>> GetByUserTypeAsync(UserType userType);
        Task<bool> UpdateProfileFieldsAsync(string userId, string firstName, string lastName, string phone, string bio, string documentId, DocumentType documentType);
    }
}
