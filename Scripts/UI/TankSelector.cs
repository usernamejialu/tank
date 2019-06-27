using UnityEngine;
using UnityEngine.UI;
using Tanks.Data;

namespace Tanks.UI
{
	//Base class for Tank selection
	public class TankSelector : MonoBehaviour
	{
		[SerializeField]
		protected bool m_FilterLockedItems = false;

		[SerializeField]
		protected DiscretePointSlider m_SpeedSlider;

		[SerializeField]
		protected DiscretePointSlider m_RefireRateSlider;

		[SerializeField]
		protected DiscretePointSlider m_ArmorSlider;

		[SerializeField]
		protected Text m_TankName;


		[SerializeField]
		protected GameObject m_NextButton, m_PreviousButton;

		[SerializeField]
		protected bool m_Capitalize = false;

		protected int m_CurrentIndex = 0;

		protected virtual void OnEnable()
		{
			ResetSelections();

			UpdateTankStats(m_CurrentIndex);

			if (m_FilterLockedItems)
			{
				int length = TankLibrary.s_Instance.GetNumberOfUnlockedTanks();
				bool isActive = length > 1;
				SetActivationOfButton(m_NextButton, isActive);
				SetActivationOfButton(m_PreviousButton, isActive);
			}
			else
			{
				SetActivationOfButton(m_NextButton, true);
				SetActivationOfButton(m_PreviousButton, true);
			}
		}
		
		//Handles tank selection
		protected virtual void ResetSelections()
		{
			PlayerDataManager dataManager = PlayerDataManager.s_Instance;
			if (dataManager != null)
			{
				m_CurrentIndex = dataManager.selectedTank;
			}
		}
		
		//Called by next/previous tank button
		public void ChangeTankButton(int direction)
		{
			int next = Wrap(m_CurrentIndex + direction, TankLibrary.s_Instance.GetNumberOfDefinitions());

			if (m_FilterLockedItems)
			{
				while (!PlayerDataManager.s_Instance.IsTankUnlocked(next) || DailyUnlockManager.s_Instance.IsItemTempUnlocked(TankLibrary.s_Instance.GetTankDataForIndex(next).id))
				{
					next = Wrap(next + direction, TankLibrary.s_Instance.GetNumberOfDefinitions());
				}
			}

			m_CurrentIndex = next;

			UpdateTankStats(next);

			TankRotator.s_Instance.LoadModelForTankIndex(m_CurrentIndex);
		}

		//Updates the UI
		protected virtual void UpdateTankStats(int index)
		{
			TankTypeDefinition tankData = TankLibrary.s_Instance.GetTankDataForIndex(index);

			if (m_Capitalize)
			{
				m_TankName.text = tankData.name.ToUpperInvariant();
			}
			else
			{
				m_TankName.text = tankData.name;
			}

			if (m_SpeedSlider != null)
			{
				m_SpeedSlider.UpdateValue(tankData.speedRating);
			}

			if (m_RefireRateSlider != null)
			{
				m_RefireRateSlider.UpdateValue(tankData.refireRating);
			}
			if (m_ArmorSlider != null)
			{
				m_ArmorSlider.UpdateValue(tankData.armourRating);
			}
		}

		//Convenience helper for enabling previous/next buttons
		protected void SetActivationOfButton(GameObject button, bool isActive)
		{
			if (button == null)
			{
				return;
			}

			button.SetActive(isActive);
		}

		//Allows the list to wrap
		private int Wrap(int indexToWrap, int arraySize)
		{
			if (indexToWrap < 0)
			{
				indexToWrap = arraySize - 1;
			}
			else if (indexToWrap >= arraySize)
			{
				indexToWrap = 0;
			}

			return indexToWrap;
		}
	}
}
