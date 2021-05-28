using Terraria.ModLoader;

namespace TinyPlayerHItbox
{
    public class TinyPlayer : ModPlayer
    {
        public override void Initialize()
        {
            // player.width = 5;
            // player.height = 5;
        }

        public override void PreUpdate()
        {
            // player.Hitbox = new Microsoft.Xna.Framework.Rectangle(player.Hitbox.X, player.Hitbox.Y + (player.Hitbox.Height / 2 + 10), 10, 10);
            // player.width = 5;
            // player.height = 5;
        }
    }
}