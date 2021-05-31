using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Iis.Services.Contracts.Interfaces
{
    public interface IMatrixService
    {
        bool AutoCreateUsers { get; }
        Task<string> CheckMatrixAvailable();
        Task<bool> UserExistsAsync(string userName);
        Task<string> CreateUserAsync(string userName, string password);
    }
}
