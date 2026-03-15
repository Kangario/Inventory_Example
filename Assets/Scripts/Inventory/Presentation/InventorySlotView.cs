using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestTaskInventory.Inventory.Presentation
{
    public sealed class InventorySlotView : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _countLabel;
        [SerializeField] private TMP_Text _slotIndexLabel;
        [SerializeField] private TMP_Text _unlockPriceLabel;
        [SerializeField] private GameObject _contentRoot;
        [SerializeField] private GameObject _emptyRoot;
        [SerializeField] private GameObject _lockedRoot;
        [SerializeField] private Button _unlockButton;

        private System.Action<int> _unlockRequested;
        private int _slotIndex;

        public void Bind(int slotIndex, System.Action<int> unlockRequested)
        {
            _slotIndex = slotIndex;
            _unlockRequested = unlockRequested;

            if (_slotIndexLabel != null)
            {
                _slotIndexLabel.text = (slotIndex + 1).ToString();
            }

            if (_unlockButton != null)
            {
                _unlockButton.onClick.RemoveListener(OnUnlockClicked);
                _unlockButton.onClick.AddListener(OnUnlockClicked);
            }
        }

        public void Render(Runtime.InventorySlotState state, int unlockPrice)
        {
            var isUnlocked = state != null && state.IsUnlocked;
            var hasItem = isUnlocked && state != null && !state.IsEmpty;

            if (_lockedRoot != null)
            {
                _lockedRoot.SetActive(!isUnlocked);
            }

            if (_contentRoot != null)
            {
                _contentRoot.SetActive(hasItem);
            }

            if (_emptyRoot != null)
            {
                _emptyRoot.SetActive(isUnlocked && !hasItem);
            }

            if (_unlockPriceLabel != null)
            {
                _unlockPriceLabel.text = unlockPrice.ToString();
            }

            if (!hasItem)
            {
                if (_iconImage != null)
                {
                    _iconImage.enabled = false;
                    _iconImage.sprite = null;
                }

                if (_countLabel != null)
                {
                    _countLabel.gameObject.SetActive(false);
                }

                return;
            }

            var entry = state.Entry;

            if (_iconImage != null)
            {
                _iconImage.enabled = true;
                _iconImage.sprite = entry.Definition.Icon;
            }

            if (_countLabel != null)
            {
                var shouldShowCount = entry.Quantity > 1;
                _countLabel.gameObject.SetActive(shouldShowCount);
                if (shouldShowCount)
                {
                    _countLabel.text = entry.Quantity.ToString();
                }
            }
        }

        private void OnUnlockClicked()
        {
            _unlockRequested?.Invoke(_slotIndex);
        }
    }
}
