using System.Linq;
using System.Text.RegularExpressions;
using EFT.InventoryLogic;
using HarmonyLib;
using Seion.Iof.Reflection;
using Seion.Iof.Reflection.Extensions;
using static Seion.Iof.Reflection.Extensions.LocaleHelper;
using static Seion.Iof.Features.OrganizedContainer;

namespace Seion.Iof.Features
{
    internal class Organizer
    {
        public const string OrganizeTag = "@o";
        public const char OrganizeTagSeparator = '|';
        public const char OrganizeTagEnd = ';';
        public static Regex OrganizeRegex = new(OrganizeTag + " (.*?)" + OrganizeTagEnd);

        public static Handbook Handbook { get; set; } = null;

        public static void Organize(CompoundItem topLevelItem, InventoryController controller)
        {
            OrganizeRecursive(topLevelItem, controller);
        }

        private static void OrganizeRecursive(CompoundItem currentItem, InventoryController controller)
        {
            foreach (var grid in currentItem.Grids)
            {
                var organizedContainers = grid.Items.Where(IsOrganized).Select(item => new OrganizedContainer((CompoundItem)item, currentItem, controller)).ToList();
                organizedContainers.Sort();
                foreach (var container in organizedContainers)
                {
                    LogNotif($"Organized Container: {container.TargetItem.RLocalizedName()}");
                    container.Organize();

                    // Recursively organize containers inside this container
                    OrganizeRecursive((CompoundItem)container.TargetItem, controller);
                }
            }
        }

        private static void LogNotif(string message)
        {
            if (Plugin.EnableLogs) Plugin.GlobalLogger.LogMessage(message);
        }

        public static bool IsOrganized(Item item)
        {
            if (!item.TryGetItemComponent(out TagComponent tagComponent)) return false;
            if (!item.IsContainer) return false;
            return IsOrganized(tagComponent.Name);
        }
        
        public static bool IsOrganized(string tagName)
        {
            return ParseOrganizeParams(tagName).Length > 0;
        }

        private static bool ContainsSeparate(string tagName, string findTag)
        {
            if (tagName.Contains(findTag))
            {
                int beforeTagIdx = tagName.IndexOf(findTag) - 1;
                if (beforeTagIdx >= 0)
                {
                    if (tagName[beforeTagIdx] != ' ') return false;
                }
                int afterTagIdx = tagName.IndexOf(findTag) + findTag.Length;
                if (afterTagIdx <= tagName.Length - 1)
                {
                    if (tagName[afterTagIdx] != ' ') return false;
                }
                return true;
            }
            return false;
        }

        public static string[] ParseOrganizeParams(Item item)
        {
            if (!IsOrganized(item)) return new string[0];
            if (!item.TryGetItemComponent(out TagComponent tagComponent)) return new string[0];
            return ParseOrganizeParams(tagComponent.Name);
        }

        public static string[] ParseOrganizeParams(string tagName)
        {
            string organizeStr = OrganizeRegex.Match(tagName).Value;
            if (string.IsNullOrEmpty(organizeStr))
            {
                if (ContainsSeparate(tagName, OrganizeTag))
                {
                    return new string[] { ParamDefault };
                }
                return new string[0];
            }

            var result = organizeStr
                .Substring(OrganizeTag.Length + 1)
                .TrimEnd(OrganizeTagEnd)
                .Trim()
                .Split(OrganizeTagSeparator)
                .Select(param => param.Trim())
                .Where(param => !string.IsNullOrEmpty(param))
                .ToArray();

            if (result.Length > 0 && GetCategoryParams(result).AddRangeToArray(GetNameParams(result)).All(IsNegatedParam))
            {
                result = result.Prepend(ParamDefault).ToArray();
            }
            if (result.Length == 1 && (result.Contains(ParamFoundInRaid) || result.Contains(ParamNotFoundInRaid)))
            {
                result = result.Prepend(ParamDefault).ToArray();
            }
            if (result.Length < 1) result = result.Prepend(ParamDefault).ToArray();
            return result;
        }
    }
}
