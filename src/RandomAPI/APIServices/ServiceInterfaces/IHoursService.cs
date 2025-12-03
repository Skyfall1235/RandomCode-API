using RandomAPI.DTOs;

public interface IHoursService
{
    HoursResponseDto Calculate(HoursRequestDto request);
}
