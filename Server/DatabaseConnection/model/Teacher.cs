namespace DatabaseConnection.model;
public class Teacher
{
    public int Id{get;set;}
    public int UserId{get;set;}

    public User User {get;set;} = null!;
    public ICollection<TeacherConstraint>? TeacherConstraints{get;set;}
    public ICollection<TeacherAssignment>? TeacherAssignments{get;set;}
    public ICollection<ScheduleReview>? ScheduleReviewss{get;set;}
    public ICollection<CourseTeacher>? CourseTeachers{get;set;}

   
}