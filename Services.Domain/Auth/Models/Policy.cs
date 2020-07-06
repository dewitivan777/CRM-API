namespace Services.Domain.Auth.Models
{
    public class Policy
    {
        /// <summary>
        /// <para>Indicates that the entity may only be actioned by the owner (typically specified by userId) or a moderator</para>
        /// <see cref="EntityOwnerOrRoleHandler"/>
        /// </summary>
        public const string EntityOwnerOrModerator = "EntityOwnerOrModerator";
        public const string Moderator = "Moderator";
        public const string Root = "Root";
        public const string EntityOwner = "EntityOwner";
    }
}
