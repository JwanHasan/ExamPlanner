namespace DatabaseConnection.dto;

public class ClassDto
{
public required int CourseId{get;set;} //FK
public required string ClassCode{get;set;} //CK

public required string NickName{get;set;}
public required string Prefix {get;set;}
public required int Semester {get;set;}
public DateTime StartDate{get;set;}
public DateTime EndDate{get;set;}
public required string CourseOffering {get;set;}
}

