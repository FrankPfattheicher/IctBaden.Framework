namespace IctBaden.Framework.Arithmetic
{
  public class AverageSumLong
  {
    private int averageCount;
    private long averageSum;

    public const int DefaultCount = 2;

    public AverageSumLong(int count = DefaultCount)
    {
      averageCount = count;
    }
    public AverageSumLong(int count, long startValue)
    {
      averageCount = count;
      averageSum = startValue * (count - 1);
    }


    public void Reset()
    {
      averageSum = 0;
    }
    public void SetAverageCount(int newCount)
    {
      averageCount = newCount;
      Reset();
    }
    public long GetAverageValue(long newValue)
    {
      averageSum += newValue;
      var retValue = averageSum / averageCount;
      averageSum -= retValue;
      return retValue;
    }

  }

  public class AverageSumDouble
  {
    private int averageCount;
    private double averageSum;

    public const int DefaultCount = 2;

    public AverageSumDouble(int count = DefaultCount)
    {
      averageCount = count;
    }
    public AverageSumDouble(int count, double startValue)
    {
      averageCount = count;
      averageSum = startValue * (count - 1);
    }

    public void Reset()
    {
      averageSum = 0;
    }
    public void SetAverageCount(int newCount)
    {
      averageCount = newCount;
      Reset();
    }
    public double GetAverageValue(double newValue)
    {
      averageSum += newValue;
      var retValue = averageSum / averageCount;
      averageSum -= retValue;
      return retValue;
    }

  }
}
