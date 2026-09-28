namespace DatabaseConnection.dto;

public class EnrollmentDto
{
    public required int StudentId {get;set;} // PPK, FK
    public required int ClassId {get;set;} //PPK,FK
}
