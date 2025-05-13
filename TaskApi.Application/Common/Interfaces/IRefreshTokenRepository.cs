using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Domain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TaskApi.Application.Common.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken);
        Task SaveAsync(CancellationToken cancellationToken);
    }
}
