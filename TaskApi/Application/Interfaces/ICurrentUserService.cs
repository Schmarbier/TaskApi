﻿namespace TaskApi.Application.Interfaces
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        bool IsAuthenticated { get; }
    }
}
