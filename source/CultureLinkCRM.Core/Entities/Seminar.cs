namespace CultureLinkCRM.Core.Entities;

public class Seminar : AuditableEntity
{
    public string City { get; set; } = string.Empty;
    public int Year { get; set; }

    public int? ParentSeminarId { get; set; }
    public Seminar? ParentSeminar { get; set; }
    public ICollection<Seminar> ChildSeminars { get; set; } = [];

    public ICollection<SeminarAttendance> Attendances { get; set; } = [];
}

public class SeminarAttendance
{
    public int Id { get; set; }
    public int SeminarId { get; set; }
    public Seminar? Seminar { get; set; }
    public int PersonId { get; set; }
    public Person? Person { get; set; }
}
