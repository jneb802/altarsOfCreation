using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;

namespace Altars_of_Creation;

public class WarpTextManager
{
    public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

    public static void AddlocalizationsEnglish()
    {
        Localization = LocalizationManager.Instance.GetLocalization();
        Localization.AddTranslation("English", new Dictionary<string, string>
        {
            { "$warp_churchgate", "Lower Crypt Gate" },
            { "$warp_churchkey", "Lower Crypt Key" },
            { "$warp_churchkey_description", "Use to enter the Lower Crypt beneath Kristnir's Cathedral" },
            { "$warp_lowercrypt", "Lower Crypt" },
        });
    }
}