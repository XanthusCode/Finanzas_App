using Finanzas.Domain.Entities;

namespace Finanzas.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByWhatsAppIdAsync(string whatsAppId);
        Task<User?> GetByWhatsAppCodigoAsync(string codigo);
        Task<bool> ExistsAsync(string email);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(User user);
        Task<IEnumerable<User>> GetAllAsync();
    }
}
