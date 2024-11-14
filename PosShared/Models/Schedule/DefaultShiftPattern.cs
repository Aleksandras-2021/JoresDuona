namespace PosShared.Models;


public class DefaultShiftPattern
{
    public int Id { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ICollection<User> Users { get; set; }
}
