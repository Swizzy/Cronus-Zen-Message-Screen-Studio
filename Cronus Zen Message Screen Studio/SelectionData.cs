namespace CronusZenMessageScreenStudio
{
    public class SelectionData<TValue>
    {
        public SelectionData(string name, TValue value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }
        public TValue Value { get; set; }
    }
}