namespace RandomAPI.DTOs
{
    public class HoursResponseDto
    {
        public double HoursWorkedActual { get; set; }
        public double HoursWorkedRounded { get; set; }
        public double RemainingHours { get; set; }
        public string MinTimeOut { get; set; } = "";
        public string MaxTimeOut { get; set; } = "";
    }
}
