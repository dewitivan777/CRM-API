using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiScope
    {
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> SubClaims { get; set; } = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public List<string> Paths { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        public bool AllowCreateAccount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool AllowRepeat { get; set; }
    }
}
