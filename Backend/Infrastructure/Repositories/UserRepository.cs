using Finanzas.Application.Interfaces;
using Finanzas.Domain.Entities;
using Finanzas.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Finanzas.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext db) : IUserRepository
    {
        private readonly AppDbContext _db = db;

        public async Task<User?> GetByEmailAsync(string email) =>
            await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

        public async Task<User?> GetByIdAsync(Guid id) =>
            await _db.Users.FindAsync(id);

        public async Task<User?> GetByWhatsAppIdAsync(string whatsAppId) =>
            await _db.Users.FirstOrDefaultAsync(u => u.WhatsAppId == whatsAppId);

        public async Task<User?> GetByWhatsAppCodigoAsync(string codigo) =>
            await _db.Users.FirstOrDefaultAsync(u => u.WhatsAppCodigo == codigo && u.WhatsAppCodigoExpira > DateTime.UtcNow);

        public async Task<bool> ExistsAsync(string email) =>
            await _db.Users.AnyAsync(u => u.Email == email.ToLower());

        public async Task<User> CreateAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetAllAsync() =>
            await _db.Users.ToListAsync();
    }
}
