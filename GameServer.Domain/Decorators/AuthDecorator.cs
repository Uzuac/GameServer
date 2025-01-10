using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Domain.Decorators
{
    using GameServer.Domain.Commands;
    using GameServer.Domain.Interfaces;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class AuthDecorator<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IRequestHandler<TRequest, TResponse> _inner;
        private readonly IAppDbContext _dbContext;

        public AuthDecorator(IRequestHandler<TRequest, TResponse> inner, IAppDbContext dbContext)
        {
            _inner = inner;
            _dbContext = dbContext;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
        {
            if (typeof(TRequest).BaseType == typeof(AuthRequest))
            {
                var authRequest = request as AuthRequest;
                var device = _dbContext.Devices.AsNoTracking()
                    .Include(d => d.PlayerAuth)
                    .Where(d => d.Id == authRequest.DeviceId)
                    .FirstOrDefault();

                if (device == null || !device.IsAuthenticated())
                {
                   return (TResponse)(object)"Device is not authenticated. Login first.";
                }
            }

            var response = await _inner.Handle(request, cancellationToken);

            return response;
        }
    }

}
