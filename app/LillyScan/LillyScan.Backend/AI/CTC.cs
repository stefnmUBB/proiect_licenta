namespace LillyScan.Backend.AI
{
    public static class CTC
    {
        public static int GreedyDecode(int[] input, int[] outputBuffer, int labelsCount)
        {
            int len = 0;
            int c = -1;

            for(int i=0, l=input.Length;i<l;i++)
            {
                if (input[i] == labelsCount - 1)
                {
                    if (c >= 0) outputBuffer[len++] = c;
                    c = -1;
                    continue;
                }
                if (input[i]!=c)
                {
                    if (c >= 0) outputBuffer[len++] = c;
                    c = input[i];
                    continue;
                }
            }

            return len;
        }
    }
}
