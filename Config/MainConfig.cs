using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SoundOverlap.Config;

public class MainConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[DefaultValue(true)]
	public bool Enabled;
}
