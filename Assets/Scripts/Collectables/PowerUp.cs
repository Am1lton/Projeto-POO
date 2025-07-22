using Classes;
using Powers;

namespace Collectables
{
    public class PowerUp : Collectable<PowerTypes, Player>
    {
        protected override void Collect(Player player)
        {
            player.AddPower(Power.GetPower(content));
        }
    }
}