namespace ExamPlannerServer.Import;

// One row of the Opslagsark ("Data fra esas"): one student registered on one planning element.
public class ParsedStudentRegistrationRow
{
    public int SourceRowNumber { get; set; }                     // row in the Excel sheet
    public Guid RegistrationId { get; set; }                     // Gennemførelsesuddannelseselement (unique per row)
    public required string StudentCode { get; set; }             // Anonymt Student-ID, e.g. "STU-0001"
    public int StudentNumber { get; set; }                       // the number in it, e.g. 1
    public required string PlanningElementNickname { get; set; } // Kaldenavn (Planlægningsuddannelseselement): how the row finds its element
    public required string PlanningElementName { get; set; }     // Planlægningsuddannelseselement (the name)
    public required string Hold { get; set; }                    // the student's group on it, e.g. "KA61-BDL-F26"
    public int AttemptNo { get; set; }                           // Antal forsøg
}
