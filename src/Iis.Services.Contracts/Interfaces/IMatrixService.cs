using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IMatrixService
    {
        bool AutoCreateUsers { get; }
        Task<string> CheckMatrixAvailableAsync();
        Task<bool> UserExistsAsync(string userName, string password);
        Task<string> CreateUserAsync(string userName, string password);
    }
}
