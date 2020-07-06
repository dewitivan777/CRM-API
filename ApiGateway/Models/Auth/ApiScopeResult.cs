namespace ApiGateway.Models.Auth
{
    public class ApiScopeResult
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsInScope { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsModerator { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool ScopeToUser { get; set; }
    }
}
