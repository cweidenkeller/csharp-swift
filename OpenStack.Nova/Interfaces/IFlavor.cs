namespace OpenStack.Nova
{
	using System;

	public interface IFlavor
	{
		FlavorResponse GetAvailableFlavors();
		FlavorResponse GetFlavorInfo();
		FlavorResponse GetFlavorSetInfo();
	}
}

