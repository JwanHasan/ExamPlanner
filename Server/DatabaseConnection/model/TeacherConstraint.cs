namespace DatabaseConnection.model;
public enum ConstraintType
{
    MustToHave,NiceToHave
}
public class TeacherConstraint
{
    public int Id{get;set;}
    public int TeacherId{get;set;}
    public DateOnly ConstraintDate{get;set;}
    public ConstraintType ConstraintType {get;set;}
    public string Note{get;set;} = "";
}