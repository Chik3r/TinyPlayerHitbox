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
		// private static MethodInfo _hitboxGetMethodCache = null;
		//
		// private static MethodInfo HitboxGetInfo
		// {
		// 	get
		// 	{
		// 		if (_hitboxGetMethodCache == null)
		// 			_hitboxGetMethodCache = typeof(Entity).GetProperty(nameof(Entity.Hitbox)).GetGetMethod();
		//
		// 		return _hitboxGetMethodCache;
		// 	}
		// }
		//
		// private delegate Rectangle OrigHitboxGetter(Entity self);
		//
		// private delegate Rectangle HookHitboxGetter(OrigHitboxGetter orig, Entity self);
		
		// private static event HookHitboxGetter OnHitboxGetter
		// {
		// 	add => HookEndpointManager.Add(HitboxGetInfo, value);
		// 	remove => HookEndpointManager.Remove(HitboxGetInfo, value);
		// }

		public override void Load()
		{
			// IL.Terraria.Player.getRect += PlayerOngetRect;
			IL.Terraria.Player.Update_NPCCollision += PlayerOnUpdate_NPCCollision;
			On.Terraria.Player.getRect += PlayerOngetRect;
			// OnHitboxGetter += CustomHitboxGetter;
		}

		public override void Unload()
		{
			// OnHitboxGetter -= CustomHitboxGetter;
			// _hitboxGetMethodCache = null;
		}
		
		// private Rectangle CustomHitboxGetter(OrigHitboxGetter orig, Entity p)
		// {
		// 	if (!(p is Player player))
		// 		return orig(p);
		// 	
		// 	return CalculateNewRect(player);
		// }
		
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
		
		// private void PlayerOngetRect(ILContext il)
		// {
		// 	ILCursor c = new ILCursor(il);
		// 	c.Emit(OpCodes.Ldarg_0);
		// 	c.EmitDelegate<Func<Player, Rectangle>>(p =>
		// 	{
		// 		return CalculateNewRect(p);
		// 		return new Rectangle((int) p.position.X, (int) p.position.Y, p.width, p.height);
		// 	});
		// 	c.Emit(OpCodes.Ret);
		// }

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