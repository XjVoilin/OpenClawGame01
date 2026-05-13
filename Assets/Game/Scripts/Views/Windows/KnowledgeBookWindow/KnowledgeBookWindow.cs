using JulyArch;
using OffTrail.Knowledge;
using TMPro;
using UnityEngine;

namespace OffTrail
{
    public class KnowledgeBookWindow : GameUIView
    {
        [SerializeField] private Transform _entryContainer;
        [SerializeField] private TextMeshProUGUI _titleText;

        protected override void OnBeforeOpen()
        {
            this.Subscribe<KnowledgeDiscovered>(_ => RefreshEntries());

            if (_titleText) _titleText.text = "知识本";
            RefreshEntries();
            base.OnBeforeOpen();
        }

        private void RefreshEntries()
        {
            // Placeholder: will list discovered knowledge entries
        }
    }
}
