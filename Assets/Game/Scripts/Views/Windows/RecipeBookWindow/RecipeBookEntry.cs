using TMPro;
using UnityEngine;

namespace CozyYard
{
    public class RecipeBookEntry : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;

        public void Setup(string name)
        {
            if (_nameText) _nameText.text = name;
        }
    }
}
