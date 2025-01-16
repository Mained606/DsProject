using UnityEngine;

public class ShopManager : BaseManager<ShopManager>
{
    #region Variables
    #endregion

    
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

public enum StoreType
{
    Equipment,
    General,
    Specialty
}
