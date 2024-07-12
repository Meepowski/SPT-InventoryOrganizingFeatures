using EFT.InventoryLogic;
using ContainerFilter = GClass2524;

namespace Seion.Iof
{
    internal static class Extensions
    {
        public static bool CanAccept(this StashGridClass grid, Item item)
        {
            // find the class using [CheckItemExcludedFilter, CheckItemFilter, CanAccept]
            return ContainerFilter.CanAccept(grid, item);
        }
    }
}
