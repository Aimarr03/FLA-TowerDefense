using UnityEngine;

public class FibonacciSequence
{
    public static int GetValue(int number)
    {
        if (number <= 0)
        {
            return 0;
        }
        else if (number == 1)
        {
            return 1;
        }
        else
        {
            return GetValue(number - 1) + GetValue(number - 2);
        }
    }
}
