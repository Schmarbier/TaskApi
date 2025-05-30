﻿using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace TaskApi.Application.DTOs
{
    public class TokenValidationContext
    {
        public ClaimsPrincipal? Principal { get; set; }
        public SecurityToken? SecurityToken { get; set; }
    }
}
