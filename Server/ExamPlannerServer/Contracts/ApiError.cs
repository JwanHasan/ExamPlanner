using System.Text.Json.Serialization;

namespace ExamPlannerServer.Contracts;

/// <summary>
/// The error body every endpoint returns (API_CONTRACT.md, section 4).
/// The frontend decides what to do from <see cref="Code"/>, never from the message text.
/// </summary>
public record ApiError(
    string Code,
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Details = null);

/// <summary>Machine-readable values for <see cref="ApiError.Code"/>, as named in API_CONTRACT.md.</summary>
public static class ErrorCodes
{
    // General (sections 3.8 and 5.2)
    public const string InvalidRequest = "INVALID_REQUEST";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InternalError = "INTERNAL_ERROR";

    // Login (section 5.1)
    public const string InvalidLoginRequest = "INVALID_LOGIN_REQUEST";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";

    // Import (section 6.1)
    public const string FileRequired = "FILE_REQUIRED";
    public const string UnsupportedFileType = "UNSUPPORTED_FILE_TYPE";
    public const string InvalidImportData = "INVALID_IMPORT_DATA";
    public const string ImportFailed = "IMPORT_FAILED";

    // Courses (not in the contract yet; named like CLASS_NOT_FOUND in section 7.2)
    public const string CourseNotFound = "COURSE_NOT_FOUND";
}
