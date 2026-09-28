namespace DatabaseConnection.model;

public class Lecturer
{
 public int Id {get;set;} //PK
 public int UserAccountId{get;set;}
 public required string Initials {get;set;} //CK

     // Exam lecture assign 1 lecturer and lecture can be 0 or many exam lecturer


    public ICollection<ExamLecturer>? ExamLecturers{get;set;}

     // Lecturer constraint has  1 lecturer and lecture can be 0 or many constraint

    public ICollection<LecturerConstraint>? LecturerConstraints{get;set;}
    public ICollection<ScheduleReview>? ScheduleReviews{get;set;}
    public UserAccount? UserAccount{get;set;}


}