namespace DatabaseConnection.model;

public class Lecturer
{
 public int LecturerId {get;set;} //PK
 public required string Initials {get;set;} //CK

     // Exam lecture assign 1 lecturer and lecture can be 0 or many exam lecturer


    public ICollection<ExamLecturer> ExamLecturers{get;set;}= new List<ExamLecturer>();

     // Lecturer constraint has  1 lecturer and lecture can be 0 or many constraint

    public ICollection<LecturerConstraint> LecturerConstraints{get;set;}= new List<LecturerConstraint>();


}