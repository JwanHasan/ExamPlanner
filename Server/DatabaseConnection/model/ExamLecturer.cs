public class ExamLecturer
{
    public required int ExamId{get;set;}//PPK, FK
    public required int LecturerId{get;set;}//PPK,FK
    public required string Role{get;set;}
    // Exam lecture has 1 lecturer and lecture can be 0 or many exam lecturer
    public required Lecturer Lecturer{get;set;}
}