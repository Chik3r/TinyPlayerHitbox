using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using Terraria;
using Terraria.ModLoader;

namespace TinyPlayerHItbox
{
	public class TinyPlayerHItbox : Mod
	{
		public override void Load()
		{
			IL.Terraria.Player.Update_NPCCollision += PlayerOnUpdate_NPCCollision;
			On.Terraria.Player.getRect += PlayerOngetRect;
		}
		
		private Rectangle PlayerOngetRect(On.Terraria.Player.orig_getRect orig, Player self)
		{
			return CalculateNewRect(self);
		}
		
		private void PlayerOnUpdate_NPCCollision(ILContext il)
		{
			ILCursor c = new ILCursor(il);
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdfld(typeof(Entity).GetField("width")),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Entity).GetField("height")),
				i => i.MatchCall(out _)))
				goto Error;

			// c.Emit(OpCodes.Ldloca, 0);
			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, Rectangle>>(player => CalculateNewRect(player));
			c.Emit(OpCodes.Stloc, 0);

			return;
			Error:
				Logger.Error("Failed to IL edit Update_NPCCollision");
		}

		private Rectangle CalculateNewRect(Player p)
		{
			int centerX = (int) (p.position.X + p.width / 2f);
			int centerY = (int) (p.position.Y + p.height / 2f);
				
			int newWidth = 0;
			int newHeight = 0;

			int startX = centerX - newWidth / 2;
			int startY = centerY - newHeight / 2;

			Rectangle newRect = new Rectangle(startX, startY, newWidth, newHeight);
			return newRect;
		}
	}
}