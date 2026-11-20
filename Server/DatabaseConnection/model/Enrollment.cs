using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DatabaseConnection.model;

public class Enrollment
{
    public required int StudentId {get;set;} // PPK, FK
   public required int PlanningElementId{get;set;}
   public required string Hold{get;set;}
   public int SourceImportId{get;set;}

    // relation Enrollment must have 1 Class and Class can have 0 or many Enrollment

    public PlanningEducationElement PlanningEducationElement {get;set;}=null!;
    // Enrollment has 1 student and student have many enrollment 
    public Student Student {get;set;}=null!;
}