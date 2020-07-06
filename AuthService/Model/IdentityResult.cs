using System.Collections.Generic;

namespace AuthService.Model
{
    public class IdentityResult
    {
        public bool Succeeded { get; set; }
        public List<string> Errors { get; set; }
    }
}
