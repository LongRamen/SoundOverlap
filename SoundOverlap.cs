using MonoMod.Cil;
using SoundOverlap.Config;
using System;
using Terraria.Audio;
using Terraria.ModLoader;

namespace SoundOverlap;

public class SoundOverlap : Mod
{
	public override void Load()
	{
		IL_SoundPlayer.Play_Inner += Hook_Play_Inner;
	}

	private static void Hook_Play_Inner(ILContext il)
	{
		try
		{
			ILCursor c = new(il);

			#region Previous version (without config option)
			//c.GotoNext(MoveType.Before, i => i.MatchLdloc1() && i.Next.MatchLdcI4(0) && i.Next.Next.MatchBle(out _)); // Cursor is before i
			///* Before:
			//          i: ldloc.1     // Load local variable 1 onto evaluation stack
			//     i.Next: ldc.i4.0    // Load value 0 onto evaluation stack
			//i.Next.Next: ble IL_0108 // Branch if variable1 <= 0
			//*/
			//c.Remove();     // Delete i
			//c.EmitLdcI4(0); // Replace with ldc.i4.0
			///* After:
			//          i: ldc.i4.0    // Load value 0 onto evaluation stack
			//     i.Next: ldc.i4.0    // Load value 0 onto evaluation stack
			//i.Next.Next: ble IL_0108 // Branch if 0 <= 0
			//*/
			#endregion

			c.GotoNext(MoveType.After, i => (i.Previous?.Previous?.MatchLdloc1() ?? false) && (i.Previous?.MatchLdcI4(0) ?? false) && i.MatchBle(out _)); // Cursor is after i
			c.Prev.MatchBle(out ILLabel label);
			/* Before:
			   ldloc.1     // Load local variable 1 onto evaluation stack
			   ldc.i4.0    // Load value 0 onto evaluation stack
			i: ble IL_0108 // Branch if variable1 <= 0
			*/
			c.EmitDelegate(() => ModContent.GetInstance<MainConfig>().Enabled);
			c.EmitBrtrue(label);
			/* After:
			   ldloc.1          // Load local variable 1 onto evaluation stack
			   ldc.i4.0         // Load value 0 onto evaluation stack
			i: ble IL_0108      // Branch if variable1 <= 0
			   call             // Call delegate (push Enabled onto stack)
			   brtrue.s IL_0108 // Branch if true
			*/
		}
		catch (Exception e)
		{
			throw new ILPatchFailureException(ModContent.GetInstance<SoundOverlap>(), il, e);
		}
	}
}