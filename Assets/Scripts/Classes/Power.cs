namespace Powers
{
    public abstract class Power
    {
        public abstract void Activate(Player player);
        public abstract void Deactivate(Player player);

        public static Power GetPower(PowerTypes type)
        {
            switch (type)
            {
                case PowerTypes.Dash:
                    return new Dash();
                    break;
                default:
                    return null;
            }
        }
    }

    public enum PowerTypes : byte
    {
        Dash
    }
}