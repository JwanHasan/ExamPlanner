// See https://aka.ms/new-console-template for more information

using DatabaseConnection.model;
Console.WriteLine("this is database");

AppDbContext context = new AppDbContext();

await context.Class.AddRangeAsync(new Class
{
    CourseId = 1,
    ClassCode = "X1",
    NickName = "the naughty",
    Prefix = "PRO1X",
    Semester= 1,
    StartDate = new DateTime(DateTime.Today.Year, 9,1),
    EndDate = new DateTime(DateTime.Today.Year, 12,24),
    CourseOffering =" this course allows student to understand the basic of coding"
    

},
new Class
{
    CourseId = 1,
    ClassCode = "Y1",
    NickName = "the naughtier",
    Prefix = "PRO1Y",
    Semester= 1,
    StartDate = new DateTime(DateTime.Today.Year, 9,1),
    EndDate = new DateTime(DateTime.Today.Year, 12,24),
    CourseOffering =" this course allows student to understand the basic of coding"
    

},
new Class
{
    CourseId = 2,
    ClassCode = "X1",
    NickName = "the smart people",
    Prefix = "PRO2X",
    Semester= 2,
    StartDate = new DateTime(2027, 2,1),
    EndDate = new DateTime(2027, 08,24),
    CourseOffering =" this course allows student to understand the basic of coding"
    

}
);
await context.SaveChangesAsync();



