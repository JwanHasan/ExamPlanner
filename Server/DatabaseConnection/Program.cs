// See https://aka.ms/new-console-template for more information

using DatabaseConnection.model;
Console.WriteLine("this is database");
AppDbContext context = new AppDbContext();


Student student = new Student
{
    Name= "John"
};
 
 
 await  context.Student.AddAsync(student);
 await context.SaveChangesAsync();
