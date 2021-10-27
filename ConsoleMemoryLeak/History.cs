namespace ConsoleMemoryLeak
{
    public class History
    {
        public int[] values;
        public long lastRAM;

        public bool isNotificationSent;

        private int index;

        public History(int size)
        {
            values = new int[size];
        }

        public void Append(int value)
        {
            if (index < values.Length)
            {
                values[index] = value;
                index++;
            }
            else
            {
                for (int i = 1; i < values.Length; i++)
                {
                    values[i - 1] = values[i];
                }
                values[^1] = value;
            }
        }
    }
}
