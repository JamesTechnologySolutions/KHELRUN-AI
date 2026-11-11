using System.Windows;
using System.Windows.Media;

namespace KhelrunAI.Services;

/// <summary>Swaps the color brushes in Application.Resources. All UI uses DynamicResource, so it updates live.</summary>
public static class ThemeService
{
    public static void Apply(string theme)
    {
        bool light = string.Equals(theme, "Light", StringComparison.OrdinalIgnoreCase);
        var r = Application.Current.Resources;

        void Set(string key, string hex) =>
            r[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));

        if (light)
        {
            Set("Bg",            "#F4F5F9");
            Set("Panel",         "#FFFFFF");
            Set("PanelElevated", "#F0F2F7");
            Set("Accent",        "#6B4EFF");
            Set("AccentHover",   "#856CFF");
            Set("AccentDim",     "#C4B8FF");
            Set("UserBubble",    "#E8E2FF");
            Set("BotBubble",     "#EEF0F6");
            Set("Fg",            "#151822");
            Set("Muted",         "#6B7280");
            Set("Border",        "#D8DCE8");
            Set("InputBg",       "#FFFFFF");
            Set("Success",       "#10B981");
            Set("Danger",        "#EF4444");
            Set("SidebarBg",     "#ECEEF5");
        }
        else
        {
            // Premium deep dark with violet accent
            Set("Bg",            "#0B0D12");
            Set("Panel",         "#13161F");
            Set("PanelElevated", "#1A1E2A");
            Set("Accent",        "#7C5CFF");
            Set("AccentHover",   "#9B82FF");
            Set("AccentDim",     "#4A3A99");
            Set("UserBubble",    "#2A2450");
            Set("BotBubble",     "#1A1E2A");
            Set("Fg",            "#F0F2F8");
            Set("Muted",         "#8B93A7");
            Set("Border",        "#2A3040");
            Set("InputBg",       "#0F1219");
            Set("Success",       "#3DDB8A");
            Set("Danger",        "#FF5C6C");
            Set("SidebarBg",     "#0E1017");
        }
    }
}
