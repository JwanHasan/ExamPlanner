using ExamPlannerServer.Import;

namespace TesProject;

// The rules that turn the export's columns into course values. Just text in,
// text out. The examples are taken from the real export, including its awkward rows.
public class CourseDerivationTests
{
    [Theory]
    [InlineData("IT-ADS1-A25", "IT")]
    [InlineData("xa-PHY1FI-S26", "XA")]            // capitalised
    [InlineData("  BY-BET1-A25-Sommer ", "BY")]    // surrounding spaces
    [InlineData("IT-PRO2,1.0-S25", "IT")]          // awkward rows still have a prefix
    public void Prefix_IsTheFirstTwoLetters(string nickname, string expected) =>
        Assert.Equal(expected, CourseDerivation.Prefix(nickname));

    [Theory]
    [InlineData("")]
    [InlineData("I")]
    [InlineData("1-ADS1-A25")]
    [InlineData("-ADS1-A25")]
    [InlineData("I1-ADS1")]
    public void Prefix_IsNull_WhenTheNicknameDoesNotStartWithTwoLetters(string nickname) =>
        Assert.Null(CourseDerivation.Prefix(nickname));

    [Theory]
    [InlineData("ADS1 Algoritmer og datastrukturer", "ADS1")]
    [InlineData("BET1-Sommer - Grundlæggende materialelære og betonkonstruktioner", "BET1")]   // cut at the hyphen
    [InlineData("MATx Grundlæggende matematik", "MATx")]                                       // the row a letter-and-digit pattern misses
    [InlineData("  VP2 Valgfag", "VP2")]
    [InlineData("ALI1", "ALI1")]                                                               // only a code
    public void CourseCode_IsTheFirstWordUpToAHyphen(string name, string expected) =>
        Assert.Equal(expected, CourseDerivation.CourseCode(name));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-Sommer something")]
    public void CourseCode_IsNull_WhenThereIsNone(string name) =>
        Assert.Null(CourseDerivation.CourseCode(name));

    [Theory]
    [InlineData("ADS1 Algoritmer og datastrukturer", "Algoritmer og datastrukturer")]
    [InlineData("BET1-Sommer - Grundlæggende materialelære og betonkonstruktioner", "Grundlæggende materialelære og betonkonstruktioner")]
    [InlineData("BPR2 Bachelor Projekt", "Bachelor Projekt")]
    [InlineData("ADS1   Extra   spaces", "Extra   spaces")]       // spaces inside the name are kept as they are
    [InlineData("ALI1", "ALI1")]                                  // nothing left, so the full name stays
    public void CourseName_DropsTheLeadingCode(string name, string expected) =>
        Assert.Equal(expected, CourseDerivation.CourseName(name));

    [Theory]
    [InlineData("Software Technology Engineering  - VIA University College, Campus Horsens ", "Horsens")]
    [InlineData("Global Business og Teknologi 2024  - VIA University College, Campus Aarhus C ", "Aarhus C")]
    [InlineData("Maskiningeniør Trainee  - VIA University College, Campus Horsens (online) ", "Horsens")]   // "(online)" is not part of the campus
    [InlineData("Bygningsingeniør - VIA University College, Campus Horsens", "Horsens")]
    [InlineData("134 - 8761 - XA18 - Værkstedskursus - VIA University College, Campus Aarhus C", "Aarhus C")]
    public void Campus_IsTheTextAfterTheWordCampus(string activityOffering, string expected) =>
        Assert.Equal(expected, CourseDerivation.Campus(activityOffering));

    [Theory]
    [InlineData("")]
    [InlineData("Software Technology Engineering - Horsens")]
    [InlineData("A programme with no campus in its name")]
    [InlineData("Online campus course")]              // the word alone is not enough
    [InlineData("VIA University College, campus Horsens")]  // capital C is required
    public void Campus_IsEmpty_WhenTheTextHasNoCampus(string activityOffering) =>
        Assert.Equal("", CourseDerivation.Campus(activityOffering));

    [Fact]
    public void CourseKey_IgnoresCapitalisation_ButSeparatesProgrammes()
    {
        Assert.Equal(CourseDerivation.CourseKey("it", "ads1"), CourseDerivation.CourseKey("IT", "ADS1"));
        Assert.NotEqual(CourseDerivation.CourseKey("IT", "DBS1"), CourseDerivation.CourseKey("TS", "DBS1"));
    }
}
