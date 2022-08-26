using HarmonyLib;
using PotionCraft.Npc;
using PotionCraft.SaveFileSystem;
using PotionCraft.SaveLoadSystem;
using System.Linq;
using System.Reflection;
using static PotionCraft.Utils.Dictionaries;

namespace PotionCraftFileFixer.Scripts
{

    [HarmonyPatch(typeof(File), "CreateNewFromState")]
    public class HidePotionStackVisualEffectsPatch
    {
        static bool Prefix(SavedState savedState)
        {
            return Ex.RunSafe(() =>
            {
                var progressState = savedState as ProgressState;
                if (progressState == null) return true;
                var queue = typeof(NpcVirtualQueue).GetField("queue", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(progressState.extraTradersVirtualQueue) as IntString;
                if (queue == null)
                {
                    Plugin.PluginLogger.LogError("ERROR: failed to get queue");
                    return true;
                }
                var currentDay = progressState.day;
                Plugin.PluginLogger.LogInfo("Merchant queue before fix:");
                queue.ToList().ForEach(v => Plugin.PluginLogger.LogInfo($"{v.Key} - {v.Value}"));

                AddMerchantToQueue("Dwarf", queue, currentDay);
                AddMerchantToQueue("WMerchant", queue, currentDay);

                Plugin.PluginLogger.LogInfo("Merchant queue after fix:");
                queue.ToList().ForEach(v => Plugin.PluginLogger.LogInfo($"{v.Key} - {v.Value}"));

                return true;
            });
        }

        private static void AddMerchantToQueue(string merchant, IntString queue, int currentDay)
        {
            var nextDay = GetNextAvailableDay(currentDay, queue);

            if (!queue.Any(v => v.Value.Equals(merchant)))
            {
                queue.Add(nextDay, merchant);
            }
        }

        private static int GetNextAvailableDay(int currentDay, IntString queue)
        {
            while(true)
            {
                currentDay++;
                if (queue.ContainsKey(currentDay)) continue;
                return currentDay;
            }
        }
    }

}
