using Microsoft.EntityFrameworkCore.Metadata;

public class ExamClass
{
    public required int ExamID{get;set;} // PPK , FK
    public required int ClassID{get;set;}// PPK FK

// relation exam class has 1 class and class has 0 or  many exam classes 
public required Class Class {get;set;}

// relation Exam Class has 1 Exam and exam has 1 or many exam classes
public required Exam Exam   {get;set;}


}

