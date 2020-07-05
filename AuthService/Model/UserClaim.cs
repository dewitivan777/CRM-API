namespace AuthService.Model
{
    public class UserClaim
    {
        public UserClaim()
        {
        }

        public UserClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }
        public string Value { get; set; }
    }
}
