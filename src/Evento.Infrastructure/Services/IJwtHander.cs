using System;
using Evento.Infrastructure.DTO;

namespace Evento.Infrastructure.Services
{
    public interface IJwtHander
    {
         JwtDto CreateToken(Guid userId, string role);
    }
}