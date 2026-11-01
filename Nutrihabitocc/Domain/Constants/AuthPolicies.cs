namespace Nutrihabitocc.Domain.Constants;

public static class AuthPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string AdminWebOnly = "AdminWebOnly";
    public const string StaffOnly = "StaffOnly";
    public const string WebStaffOnly = "WebStaffOnly";
    public const string PatientOnly = "PatientOnly";
    public const string MobilePatientOnly = "MobilePatientOnly";
    public const string Authenticated = "Authenticated";
}
