namespace IctBaden.Framework.Arithmetic;

public class Hysteresis
{
    private readonly double _lower;
    private readonly double _upper;

    private bool _output;

    private Hysteresis(double lower, double upper)
    {
        _lower = lower;
        _upper = upper;
    }

    public static Hysteresis FromRange(double lower, double upper) => 
        new(lower, upper);

    public static Hysteresis FromValueAndRange(double value, double range) => 
        new(value - range / 2, value + range / 2);

    public bool Get(double value)
    {
        _output = (value >= _upper || _output) && value > _lower;
        return _output;
    }
}