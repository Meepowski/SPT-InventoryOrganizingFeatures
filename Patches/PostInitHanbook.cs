using System;
using System.Reflection;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using Seion.Iof.Reflection;
using Seion.Iof.Features;

namespace Seion.Iof.Patches
{
    internal class PostInitHanbook : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MenuTaskBar), "InitHandbook");
        }

        [PatchPostfix]
        private static void PatchPostfix(ref object handbook)
        {
            try
            {
                Organizer.Handbook ??= new Handbook(handbook);
                //Logger.LogMessage($"Elements: {Organizer.Handbook.NodesTree.Count}");
                //var search = Organizer.Handbook.FindNode("5751496424597720a27126da");
                //if (search != null)
                //{
                //    Logger.LogMessage($"Found: {search.Data.Name.Localized()}");
                //    Logger.LogMessage($"Categories: {string.Join(" > ", search.Category.Select(cat => cat.Localized()))}");
                //}
            }
            catch (Exception ex)
            {
                throw Plugin.ShowErrorNotif(ex);
            }
        }
    }
}
