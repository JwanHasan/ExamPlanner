namespace ExamPlannerServer.Import;

public class ParsedPlanningEducationElementRow
{
    public int SourceRowNumber { get; set; }          // row in the Excel sheet
    public Guid PlanningElementId { get; set; }       // A: Planlægningsuddannelseselement
    public required string SourceChecksum  { get; set; }       // B: Kontrolsum for række
    public DateTime SourceModifiedAt { get; set; }    // C: Ændret
    public required string Name { get; set; }                  // Navn
    public required string Nickname  { get; set; }             // Kaldenavn
    public  decimal Ects { get; set; }
    public int ExecutionElementCount  { get; set; }   // Antal gennemførelsesuddannelseselementer
    public DateTime StartDate { get; set; }           //Startdato
    public DateTime EndDate { get; set; }             //Slutdato
    public required string ActivityOffering { get; set; }      // Aktivitetsudbud
    public required string AssessmentType  { get; set; }       // Bedømmelsestype (Intern/Ekstern)
    public bool CompletionAssessment {get; set;}      // Gennemførelse
    public required string GradingScale {get; set;}            // Karakterskala
    //the five Ja/Nej columns for exam type
    public bool CombinedWrittenOral {get; set;}       // Kombineret skriftlig og mundtlig
    public bool Oral {get; set;}                      // Mundtlig
    public bool PracticalExam {get; set;}             // Praktisk prøve
    public bool Project {get; set;}                   // Projekt
    public bool Written {get; set;}                   // Skriftlig
}