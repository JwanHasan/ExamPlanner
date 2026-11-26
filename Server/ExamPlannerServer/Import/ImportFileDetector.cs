using ClosedXML.Excel;

namespace ExamPlannerServer.Import;

// Tells which kind of file was uploaded, so the right importer gets it.
public static class ImportFileDetector
{
    // The Opslagsark is recognised by its student column. Every other file goes to the planning-element
    // importer, so that a planning-element export with a column missing is still told exactly which column
    // is missing (and a file that is not Excel at all gets that importer's "cannot be read" message).
    // The stream is rewound afterwards, so the importer can read it from the start.
    public static bool IsStudentRegistrations(Stream stream)
    {
        try
        {
            using var workbook = new XLWorkbook(stream);
            return ExcelTable.HasColumn(workbook, StudentRegistrationParser.ColStudent);
        }
        catch (Exception)
        {
            return false; // not a readable workbook: the importer will say so
        }
        finally
        {
            if (stream.CanSeek)
                stream.Position = 0;
        }
    }
}
