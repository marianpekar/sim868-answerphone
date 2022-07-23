namespace Answerphone
{
    public struct Ring<T>
    {
        public T[] Items { get; private set; }
        public int Length { get => Items.Length; }
        public int Head { get; private set; } = 0;
        public T AtHead { get => Items[Head]; }

        public Ring(T[] items)
        {
            Items = items;
        }

        public void PushHead()
        {
            Head++;
            if (Head >= Length)
                Head = 0;
        }

        public void PushHead(int offset)
        {
            Head = offset % Length;
        }
    }
}
