using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using Seion.Iof.Reflection;
using static Seion.Iof.Features.Locker;

namespace Seion.Iof.Patches
{
    // Static SortClass.Sort() checks if Item.CurrentLocation is null
    // so preventing sort locked items from being removed
    // makes the sort method ignore them.
    internal class PreGridClassRemoveAll : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // Find the Grid class (Per STP-AKI 3.5.5 it's a GClass2166)

            var gridClassMethods = new string[] { "FindItem", "GetItemsInRect", "FindLocationForItem", "Add", "AddItemWithoutRestrictions", "Remove", "RemoveAll", "CanReplace" };
            return AccessTools.Method(ReflectionHelper.FindClassTypeByMethodNames(gridClassMethods), "RemoveAll");

            //return typeof(Class1966).GetMethod(nameof(Class1966.RemoveAll));
        }

        // Conditional reimplementation of original method, but with tweaks to ignore "Sort/Move Locked" items
        // Since the Sort method has a check for "item.CurrentAddress == null", it simply won't touch these items if they weren't removed
        [PatchPrefix]
        private static bool PatchPrefix(ref object __instance)
        {
            // Dynamically find static SortClass
            var sortClassMethods = new string[] { "Sort", "ApplyItemToRevolverDrum", "ApplySingleItemToAddress", "Fold", "CanRecode", "CanFold" };
            var sortClassType = ReflectionHelper.FindClassTypeByMethodNames(sortClassMethods);

            var callerClassType = new StackTrace().GetFrame(2).GetMethod().ReflectedType;
            // If method is being called from the static SortClass - run patched code, if not - run default code.

            if (callerClassType != sortClassType) return true;

            var itemCollection = __instance.GetPropertyValue<IEnumerable<KeyValuePair<Item, LocationInGrid>>>("ItemCollection");
            //if (!__instance.ItemCollection.Any())
            if (!itemCollection.Any())
            {
                return false;
            }

            var itemCollectionRemove = itemCollection.GetMethod("Remove");
            var gridSetLayout = __instance.GetMethod("SetLayout");

            var itemcollection = itemCollection.Where(pair => !IsSortLocked(pair.Key)).ToList();

            foreach (var kvp in itemcollection)
            {

                //kvp.Deconstruct(out Item item, out LocationInGrid locationInGrid); - uses a GClass781 extension
                var item = kvp.Key;
                var locationInGrid = kvp.Value;
                //__instance.ItemCollection.Remove(item, __instance);
                itemCollectionRemove.Invoke(itemCollection, new object[] { item, __instance });
                //__instance.SetLayout(item, locationInGrid, false);
                gridSetLayout.Invoke(__instance, new object[] { item, locationInGrid, false });
            }

            var lastLineMethod = __instance // look for method with generic name, called on the last line of RemoveAll()
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method =>
                {
                    return method.ReturnType == typeof(void)
                    && method.GetMethodBody().LocalVariables.Count == 6
                    && method.GetMethodBody().LocalVariables.All(variable => variable.LocalType == typeof(int));
                })
                .First(); // let it throw exception if somehow the method wasn't found.

            lastLineMethod.Invoke(__instance, null);
            //NotificationManagerClass.DisplayMessageNotification("Ran the RemoveAll patch");
            return false;
        }
    }
}