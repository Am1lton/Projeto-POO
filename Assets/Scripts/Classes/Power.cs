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
                PowerTypes.WallJump => new WallJump(),
                _ => null
            };
        }public static PowerTypes GetPowerType(Power type)
        {
            return type switch
            {
                Dash => PowerTypes.Dash,
                DoubleJump => PowerTypes.DoubleJump,
                WallJump => PowerTypes.WallJump,
                Shoot => PowerTypes.Shoot,
                _ => PowerTypes.None
            };
        }
    }

    public enum PowerTypes : byte
    {
        None = 0,
        Dash,
        Shoot,
        DoubleJump,
        WallJump
    }
}