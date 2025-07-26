namespace Powers
{
    public abstract class Power
    {
        public abstract void Activate(Player player);
        public abstract void Deactivate(Player player);

        public static Power GetPower(PowerTypes type)
        {
            return type switch
            {
                PowerTypes.Dash => new Dash(),
                PowerTypes.Shoot => new Shoot(),
                PowerTypes.DoubleJump => new DoubleJump(),
                _ => null
            };
        }
    }

    public enum PowerTypes : byte
    {
        Dash,
        Shoot,
        DoubleJump
    }
}