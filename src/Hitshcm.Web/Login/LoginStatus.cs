namespace Hitshcm.Web.Login;

public enum LoginStatus
{
    Succeeded,
    InvalidCredentials,
    LockedOut,
    PolicyBlocked,
    MustChangePassword,
    NeedsBusinessGroup,
    NeedsFirstLogonAck,
    FailedClosed
}
