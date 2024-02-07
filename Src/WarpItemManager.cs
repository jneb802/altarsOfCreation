using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Extensions;
using Jotunn.Managers;

namespace Altars_of_Creation;

public class WarpItemManager
{
    public static void CreateChurchKey()
    {
        // Create and add a custom item
        ItemConfig keyConfig = new ItemConfig();
        keyConfig.Amount = 1;

        // Prefab did not use mocked refs so no need to fix them
        var churchKeyItem = new CustomItem(WarpAssetManager.mwl_tyggrason_bundle, "ChurchKey", fixReference: false, keyConfig);
        ItemManager.Instance.AddItem(churchKeyItem);
        
        Altars_of_CreationPlugin.Altars_of_CreationLogger.LogDebug("ChurchKey has been added");
        
        WarpLockManager.AddChurchKeytoChild(WarpAssetManager.MWL_RuinsCathedral1_GameObject.gameObject.transform.FindDeepChild("Blueprint").gameObject, "churchgate");
    }
}