using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.DTOs;

namespace TaskApi.Application.Common.Interfaces
{
    public interface ITokenValidatorService
    {
        Task<TokenValidationResult> ValidateAsync(TokenValidationContext context);
    }

    public class TokenValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }

        public static TokenValidationResult Success() => new() { IsValid = true };
        public static TokenValidationResult Fail(string error) => new() { IsValid = false, ErrorMessage = error };
    }
}
