using JulyArch;
using OffTrail.Crafting;
using OffTrail.Inventory;
using OffTrail.Knowledge;
using TMPro;
using UnityEngine;

namespace OffTrail
{
    public class CraftingWindow : GameUIView
    {
        [SerializeField] private Transform _recipeContainer;
        [SerializeField] private TextMeshProUGUI _titleText;

        protected override void OnBeforeOpen()
        {
            this.Subscribe<InventoryChanged>(_ => RefreshRecipes());
            this.Subscribe<RecipeUnlocked>(_ => RefreshRecipes());

            if (_titleText) _titleText.text = "制作";
            RefreshRecipes();
            base.OnBeforeOpen();
        }

        private void RefreshRecipes()
        {
            // Placeholder: will list available recipes with craft buttons
        }
    }
}
