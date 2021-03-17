using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Drawmasters.LevelConstructor
{
    public class InfoPopup : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI titleLabel = default;
        [SerializeField] TextMeshProUGUI mainLabel = default;
        [SerializeField] Button button = default;


        void Awake() => button.onClick.AddListener(Hide);


        public void Show(string title, string mainText)
        {
            gameObject.SetActive(true);
            titleLabel.text = title;
            mainLabel.text = mainText;
        }


        public void Hide() => gameObject.SetActive(false);
    }
}
