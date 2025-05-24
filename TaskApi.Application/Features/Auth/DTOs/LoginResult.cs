using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.DTOs;

namespace TaskApi.Application.Features.Auth.DTOs
{
    public class LoginResult : AuthenticationResult
    {
        public Guid UserId { get; set; } = default!;
    }
}
