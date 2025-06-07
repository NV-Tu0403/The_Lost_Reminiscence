using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Loc_Backend.Localization.Scripts
{
    public class LanguageSelector : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        [SerializeField] private List<Sprite> flagIcons;

        private void Start()
        {
            PopulateDropdown();
            dropdown.onValueChanged.AddListener(ChangeLanguage);
            SetDropdownToCurrentLocale();
        }

        void PopulateDropdown()
        {
            dropdown.ClearOptions();

            var options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                var locale = LocalizationSettings.AvailableLocales.Locales[i];
                var option = new TMP_Dropdown.OptionData(locale.Identifier.CultureInfo.NativeName);

                // Gắn sprite cờ 
                if (i < flagIcons.Count)
                    option.image = flagIcons[i];

                options.Add(option);
            }

            dropdown.options = options;
            dropdown.RefreshShownValue();
        }

        void SetDropdownToCurrentLocale()
        {
            var currentLocale = LocalizationSettings.SelectedLocale;
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                if (LocalizationSettings.AvailableLocales.Locales[i].Identifier == currentLocale.Identifier)
                {
                    dropdown.value = i;
                    break;
                }
            }
        }

        void ChangeLanguage(int index)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        }
    }
}