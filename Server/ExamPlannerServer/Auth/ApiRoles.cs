using DatabaseConnection.model;

namespace ExamPlannerServer.Auth;

// The access tiers, as they appear in the "role" claim of the login token.
//   Admin   - everything: read, edit and delete
//   Teacher - read everything, and edit or delete teacher notes (the note on an exam)
//   Student - only their own information
public static class ApiRoles
{
    public const string Admin = AdminSettings.AdminRole;
    public const string Teacher = nameof(Role.Teacher);
    public const string Student = nameof(Role.Student);

    // For [Authorize(Roles = ...)]: any one of these.
    public const string Staff = Admin + "," + Teacher;
}
