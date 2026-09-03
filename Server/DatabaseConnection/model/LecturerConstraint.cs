public class LecturerConstraint
{
    public required int ConstraintId{get;set;} //PK
    public required int LecturerId{get;set;} //FK
    public DateTime ConstraintDate{get;set;}
    public required string ConstraintType{get;set;}
    public string Note {get;set;} =""; 

    // Lecturer constraint has  1 lecturer and lecture can be 0 or many constraint
    public required Lecturer Lecturer{get;set;}
}