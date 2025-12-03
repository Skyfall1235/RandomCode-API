namespace RandomAPI.DTOs
{
    public class HoursRequestDto
    {
        public List<double> Hours { get; set; } = new();
        public string TimeIn { get; set; } = "";
    }
}
