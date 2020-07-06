using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Models
{
    public class ApiUserModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BusinessName { get; set; }
        public string Role { get; set; }
        public string Website { get; set; }
        public string StateId { get; set; }
        public string State { get; set; }
        public string SubStateId { get; set; }
        public string SubState { get; set; }
        public string Password { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
