using UnityEngine;
using TanksNetworkPlayer = Tanks.Networking.NetworkPlayer;

namespace Tanks.UI
{
	public interface IColorProvider
	{
		Color ServerGetColor(TanksNetworkPlayer player);
		void Reset();
	}
}