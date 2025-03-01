using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VasilVasilev.UI
{
    public class DataItemView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _textItemNumber;
        [SerializeField] private TextMeshProUGUI _textItemDescription;
        [SerializeField] private Image _imageBadge;
        [SerializeField] private GameObject _gameObjectGlow;

        [Header("Badge Sprites")] 
        [SerializeField] private Sprite _spriteGreenBadge;
        [SerializeField] private Sprite _spriteRedBadge;
        [SerializeField] private Sprite _spriteBlueBadge;
        
        public void Setup(DataItem item, int itemNumber)
        {
            _textItemNumber.text = itemNumber.ToString();
            _textItemDescription.text = item.Description;
        
            switch (item.Category)
            {
                case DataItem.CategoryType.RED:
                    _imageBadge.sprite = _spriteRedBadge;
                    break;
                case DataItem.CategoryType.GREEN:
                    _imageBadge.sprite = _spriteGreenBadge;
                    break;
                case DataItem.CategoryType.BLUE:
                    _imageBadge.sprite = _spriteBlueBadge;
                    break;
            }

            _gameObjectGlow.SetActive(item.Special);
        }
    }
}