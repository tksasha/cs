class BubleSort
{
    public static void Run()
    {
        int[] digits = new int[10];

        Random random = new();

        for (int idx = 0; idx < digits.Length; idx++)
        {
            digits[idx] = random.Next(100);
        }

        for (int i = 0; i < digits.Length - 1; i++)
        {
            for (int j = 0; j < digits.Length - i - 1; j++)
            {
                if (digits[j] > digits[j + 1])
                {
                    (digits[j], digits[j + 1]) = (digits[j + 1], digits[j]);
                }
            }
        }

        WriteLine($"digits = {string.Join(",", digits)}");
    }
}
