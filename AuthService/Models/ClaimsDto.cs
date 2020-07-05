using System.Collections.Generic;

namespace AuthService.Models
{
    public class ClaimsDto
    {
        public string UserId { get; set; }
        public List<string> Claims { get; set; }
    }
}
