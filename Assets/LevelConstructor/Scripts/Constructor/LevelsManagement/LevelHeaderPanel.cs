using Drawmasters.LevelsRepository;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class LevelHeaderPanel : MonoBehaviour
    {
        [SerializeField] Image back = default;
        [SerializeField] Button button = default;
        [SerializeField] TextMeshProUGUI indexLabel = default;
        [SerializeField] TextMeshProUGUI titleLabel = default;
        [SerializeField] TextMeshProUGUI enemiesCountLabel = default;
        [SerializeField] TextMeshProUGUI weaponTypeLabel = default;


        public LevelHeader Header { get; private set; }


        public void Set(int index, LevelHeader value, Action<LevelHeaderPanel> onClick)
        {
            indexLabel.text = (index + 1).ToString();
            Header = value;
            button.onClick.AddListener(() => onClick(this));
            Refresh();
            gameObject.SetActive(true);
        }


        public void Select(bool value) => back.color = value ? Color.gray : Color.white;

        public void Clear()
        {
            Header = null;
            gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
            Select(false);
        }

        
        void Refresh()
        {
            if (Header == null)
            {
                return;
            }

            indexLabel.color = Header.isDisabled ? Color.gray : Color.black;
            titleLabel.color = Header.isDisabled ? Color.gray : Color.black;
            enemiesCountLabel.color = Header.isDisabled ? Color.gray : Color.black;

            titleLabel.text = Header.title;
            enemiesCountLabel.text = Header.projectilesCount.ToString();

            weaponTypeLabel.text = Header.weaponType.ToString();
        }
    }
}
