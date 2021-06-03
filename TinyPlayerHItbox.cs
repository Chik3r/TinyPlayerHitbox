using System;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ModLoader;

namespace TinyPlayerHItbox
{
	public class TinyPlayerHItbox : Mod
	{
		// Set the value of the new hitbox
		private static readonly int NewWidth = 2;
		private static readonly int NewHeight = 2;
		
		public override void Load()
		{
			IL.Terraria.Player.Update_NPCCollision += PlayerOnUpdate_NPCCollision;
			IL.Terraria.Player.Update += PlayerOnUpdate;
			On.Terraria.Player.getRect += PlayerOngetRect;
		}

		private void PlayerOnUpdate(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			// IL_9276: sub
			// IL_9277: stloc.s 4
			//
			// // bool flag25 = Collision.LavaCollision(position, width, num137);
			// IL_9279: ldarg.0
			// IL_927a: ldfld valuetype [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Vector2 Terraria.Entity::position
			// IL_927f: ldarg.0
			// IL_9280: ldfld int32 Terraria.Entity::width
			// IL_9285: ldloc.s 4
			// <--- Insert here --->
			// IL_9287: call bool Terraria.Collision::LavaCollision(valuetype [Microsoft.Xna.Framework]Microsoft.Xna.Framework.Vector2, int32, int32)
			// IL_928c: stloc.s 5
			
			if (!c.TryGotoNext(MoveType.After,
				// i => i.MatchLdloc(4),
				// i => i.MatchLdcI4(6),
				i => i.MatchSub(),
				i => i.MatchStloc(4),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Entity).GetField("position")),
				i => i.MatchLdarg(0),
				i => i.MatchLdfld(typeof(Entity).GetField("width")),
				i => i.MatchLdloc(4)))
				goto Error;

			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Pop);
			c.Emit(OpCodes.Ldc_I4, NewWidth);
			c.Emit(OpCodes.Ldc_I4, NewHeight);
				
			return;
			Error:
				Logger.Error("Failed to IL edit Update");
		}

		// Make the player have a custom rect
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

			c.Emit(OpCodes.Ldarg_0);
			c.EmitDelegate<Func<Player, Rectangle>>(player => CalculateNewRect(player));
			c.Emit(OpCodes.Stloc, 0);

			return;
			Error:
				Logger.Error("Failed to IL edit Update_NPCCollision");
		}

		private Rectangle CalculateNewRect(Player p)
		{
			// Calculate the center of the player, then get the new start of the rectangle
			int centerX = (int) (p.position.X + p.width / 2f);
			int centerY = (int) (p.position.Y + p.height / 2f);

			int startX = centerX - NewWidth / 2;
			int startY = centerY - NewHeight / 2;

			Rectangle newRect = new Rectangle(startX, startY, NewWidth, NewHeight);
			return newRect;
		}
	}
}