namespace Shared.SecurityContext;

public interface ISecurityContextAccessor
{
    string? UserId { get; }
    string? Username { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    string? IpAddressClient { get; }
}