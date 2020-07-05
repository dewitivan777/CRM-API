using System;
using Microsoft.AspNetCore.Authentication;

namespace ApiGateway.Auth
{
    /// <summary>
    /// 
    /// </summary>
    public class GatewaySystemClock : ISystemClock
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Func<DateTime> UtcNowFunc = () => DateTime.UtcNow;

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset UtcNow => new DateTimeOffset(UtcNowFunc());
    }
}
