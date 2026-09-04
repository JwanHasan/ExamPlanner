namespace DatabaseConnection.model;

public class ExamHandIn
{
    public required int HandInId {get;set;}//PK
    public required int ExamId   {get;set;}//FK
    public required DateTime HandInDate{get;set;}
    public required DateTime HandInTime{get;set;}
    public int Part{get;set;}
    public string Note{get;set;}="";

    // exam hand in has only 1 exam and exam can have 0 or many hand in

    public required Exam Exam {get;set;}

}