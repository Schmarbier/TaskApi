using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskApi.Application.Common.Interfaces;
using TaskApi.Domain.Entities;

namespace TaskApi.Application.Features.Auth.Queries.GetRefreshToken
{
    public record GetRefreshTokenQuery(string Token) : IRequest<RefreshToken>;
}
