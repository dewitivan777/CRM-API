using Services.Domain.Auth.Interface;

namespace Services.Domain.Auth.Models
{
   public abstract class BaseEntity : IEntity
    {
        public string Id { get; set; }
    }
}
