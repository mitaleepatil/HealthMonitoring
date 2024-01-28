namespace HealthMonitoring.Models;

public class Range
{
    public Range(int lowRange, int highRange, int score)
    {
        LowRange = lowRange;
        HighRange = highRange;
        Score = score;
    }
    public int LowRange { get; }
    public int HighRange { get; }
    public int Score { get; }
}