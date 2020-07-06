using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway
{
    /// <summary>
    /// 
    /// </summary>
    public class ApiScope
    {

        /// <summary>
        /// 
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<string> SubClaims { get; set; } = new List<string>();

    }
}
