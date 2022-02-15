namespace Pandorai.Conditions
{
    public abstract class Condition
    {
        public abstract bool Check();
        public abstract void SetAttribute(string name, string value);
    }
}