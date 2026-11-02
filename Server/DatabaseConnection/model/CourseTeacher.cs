using System;

namespace DatabaseConnection.model;

public class CourseTeacher
{
    public int CourseId{get;set;}
    public int TeacherId{get;set;}

    public Course Course{get;set;}=null!;
    public Teacher Teacher{get;set;}=null!;
}
