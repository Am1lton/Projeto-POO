using Classes;

namespace Collectables
{
    public class Coin : Collectable<int, Player>
    {
        protected override void Collect(Player player)
        {
            Player.AddScore(content);
        }
    }
}