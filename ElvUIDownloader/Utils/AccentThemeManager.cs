using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace ElvUIDownloader.Utils;

public enum EApplicationTheme
{
    [Description("Светлая")]
    Light,

    [Description("Темная")]
    Dark,

    [Description("Системная")]
    System
}

/// <summary>
/// Управление темами приложения
/// </summary>
public class AccentThemeManager
{
    /// <summary>
    /// Установить тему и акцентный цвет
    /// </summary>
    /// <param name="mode"></param>
    /// <param name="customAccentColor"></param>
    public static void SetTheme(EApplicationTheme mode, Color? customAccentColor = null)
    {
        string[] fluentKeys =
        [
            "SystemAccentColor", "SystemAccentColorLight1", "SystemAccentColorLight2", "SystemAccentColorDark1",
            "AccentButtonBackground", "AccentButtonBackgroundPointerOver", "AccentButtonBackgroundPressed",
            "TextControlBorderBrushFocused", "TextControlBorderBrushFocusedPointerOver",
            "CheckBoxBackgroundSelected", "CheckBoxBackgroundSelectedPointerOver", "CheckBoxBackgroundSelectedPressed",
            "RadioButtonBackgroundSelected", "RadioButtonBackgroundSelectedPointerOver", "RadioButtonBackgroundSelectedPressed",
            "ListViewItemBackgroundSelected", "ListViewItemBackgroundSelectedPointerOver", "ListViewItemBackgroundSelectedPressed",
            "ProgressBarBackground", "SliderThumbBackground", "SliderTrackFill"
        ];

        switch (mode)
        {
            case EApplicationTheme.Dark:
                Application.Current.ThemeMode = ThemeMode.Dark;
                break;
            case EApplicationTheme.Light:
                Application.Current.ThemeMode = ThemeMode.Light;
                break;
            case EApplicationTheme.System:
            default:
                Application.Current.ThemeMode = ThemeMode.System;
                break;
        }

        if (customAccentColor == null)
        {
            ResetToSystem();
            
            foreach (var key in fluentKeys)
            {
                if (Application.Current.Resources.Contains(key))
                    Application.Current.Resources.Remove(key);
            }
            return; // Важно выйти из метода, чтобы не затереть сброс кастомными расчетами ниже!
        }
         
        var baseColor = customAccentColor.Value;
        Color light1, light2, light3;
        Color dark1, dark2, dark3;
        
        // В зависимости от выбранной темы (или системной на данный момент) подбираем яркость
        // Проверяем ActualThemeMode для точного определения текущего состояния (особенно для System)
        var isDark = Application.Current.ThemeMode == ThemeMode.Dark;

        if (Application.Current.ThemeMode == ThemeMode.Dark)
        {
            // В ТЕМНОЙ ТЕМЕ: при наведении/нажатии акцент должен становиться СВЕТЛЕЕ
            light1 = ChangeBrightness(baseColor, 1.15f); // Hover
            light2 = ChangeBrightness(baseColor, 1.30f); // Pressed
            light3 = ChangeBrightness(baseColor, 1.45f);
            
            dark1 = ChangeBrightness(baseColor, 0.85f);
            dark2 = ChangeBrightness(baseColor, 0.70f);
            dark3 = ChangeBrightness(baseColor, 0.55f);
        }
        else
        {
            // В СВЕТЛОЙ ТЕМЕ: при наведении/нажатии акцент должен становиться ТЕМНЕЕ
            dark1 = ChangeBrightness(baseColor, 0.85f);  // Hover
            dark2 = ChangeBrightness(baseColor, 0.70f);  // Pressed
            dark3 = ChangeBrightness(baseColor, 0.55f);
            
            light1 = ChangeBrightness(baseColor, 1.15f);
            light2 = ChangeBrightness(baseColor, 1.30f);
            light3 = ChangeBrightness(baseColor, 1.45f);
        }

        //return;
        var baseBrush = new SolidColorBrush(baseColor);
        var light1Brush = new SolidColorBrush(light1);
        var light2Brush = new SolidColorBrush(light2);
        var dark1Brush = new SolidColorBrush(dark1);
        var dark2Brush = new SolidColorBrush(dark2);

        // 5. ЗАПИСЫВАЕМ СИСТЕМНЫЕ КЛЮЧИ SYSTEMCOLORS
        UpdateResource(SystemColors.AccentColorKey, baseColor);
        UpdateResource(SystemColors.AccentColorLight1Key, light1);
        UpdateResource(SystemColors.AccentColorLight2Key, light2);
        UpdateResource(SystemColors.AccentColorLight3Key, light3);
        UpdateResource(SystemColors.AccentColorDark1Key, dark1);
        UpdateResource(SystemColors.AccentColorDark2Key, dark2);
        UpdateResource(SystemColors.AccentColorDark3Key, dark3);

        UpdateResource(SystemColors.AccentColorBrushKey, baseBrush);
        UpdateResource(SystemColors.AccentColorLight1BrushKey, light1Brush);
        UpdateResource(SystemColors.AccentColorLight2BrushKey, light2Brush);
        UpdateResource(SystemColors.AccentColorLight3BrushKey, new SolidColorBrush(light3));
        UpdateResource(SystemColors.AccentColorDark1BrushKey, dark1Brush);
        UpdateResource(SystemColors.AccentColorDark2BrushKey, new SolidColorBrush(dark2));
        UpdateResource(SystemColors.AccentColorDark3BrushKey, new SolidColorBrush(dark3));

        // 6. ЗАПИСЫВАЕМ СТРОКОВЫЕ КЛЮЧИ FLUENT-ТЕМЫ (Для мгновенной работы без лагов)
        UpdateResource("SystemAccentColor", baseColor);
        UpdateResource("SystemAccentColorLight1", light1);
        UpdateResource("SystemAccentColorLight2", light2);
        UpdateResource("SystemAccentColorDark1", dark1);

        UpdateResource("AccentButtonBackground", baseBrush);
        UpdateResource("AccentButtonBackgroundPointerOver", isDark ? light1Brush : dark1Brush);
        UpdateResource("AccentButtonBackgroundPressed", isDark ? light2Brush : dark2Brush);

        UpdateResource("TextControlBorderBrushFocused", baseBrush);
        UpdateResource("TextControlBorderBrushFocusedPointerOver", isDark ? light1Brush : dark1Brush);

        UpdateResource("CheckBoxBackgroundSelected", baseBrush);
        UpdateResource("CheckBoxBackgroundSelectedPointerOver", isDark ? light1Brush : dark1Brush);
        UpdateResource("CheckBoxBackgroundSelectedPressed", isDark ? light2Brush : dark2Brush);

        UpdateResource("RadioButtonBackgroundSelected", baseBrush);
        UpdateResource("RadioButtonBackgroundSelectedPointerOver", isDark ? light1Brush : dark1Brush);
        UpdateResource("RadioButtonBackgroundSelectedPressed", isDark ? light2Brush : dark2Brush);

        UpdateResource("ListViewItemBackgroundSelected", new SolidColorBrush(Color.FromArgb(50, baseColor.R, baseColor.G, baseColor.B)));
        UpdateResource("ListViewItemBackgroundSelectedPointerOver", new SolidColorBrush(Color.FromArgb(80, baseColor.R, baseColor.G, baseColor.B)));
        UpdateResource("ListViewItemBackgroundSelectedPressed", isDark ? light1Brush : dark1Brush);

        UpdateResource("ProgressBarBackground", baseBrush);
        UpdateResource("SliderThumbBackground", baseBrush);
        UpdateResource("SliderTrackFill", baseBrush);
    }

    /// <summary>
    /// Обновить ресурсы
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private static void UpdateResource(object key, object value)
    {
        // Перезаписываем глобальный словарь ресурсов приложения
        Application.Current.Resources[key] = value;
    }

    // Метод красивого изменения яркости (фактор > 1 делает светлее, < 1 темнее)
    private static Color ChangeBrightness(Color color, float correctionFactor)
    {
        float red = color.R;
        float green = color.G;
        float blue = color.B;

        if (correctionFactor < 1)
        {
            // Делаем темнее
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            // Делаем светлее
            red = (255 - red) * (correctionFactor - 1) + red;
            green = (255 - green) * (correctionFactor - 1) + green;
            blue = (255 - blue) * (correctionFactor - 1) + blue;
        }

        return Color.FromRgb(
            (byte)Math.Clamp(red, 0, 255), 
            (byte)Math.Clamp(green, 0, 255), 
            (byte)Math.Clamp(blue, 0, 255));
    }
    
    /// <summary>
    /// Вернуть все настройки по умолчанию
    /// </summary>
    private static void ResetToSystem()
    {
        object[] keys =
        [
            SystemColors.AccentColorKey, SystemColors.AccentColorLight1Key, SystemColors.AccentColorLight2Key, SystemColors.AccentColorLight3Key,
            SystemColors.AccentColorDark1Key, SystemColors.AccentColorDark2Key, SystemColors.AccentColorDark3Key,
            SystemColors.AccentColorBrushKey, SystemColors.AccentColorLight1BrushKey, SystemColors.AccentColorLight2BrushKey, SystemColors.AccentColorLight3BrushKey,
            SystemColors.AccentColorDark1BrushKey, SystemColors.AccentColorDark2BrushKey, SystemColors.AccentColorDark3BrushKey
        ];

        foreach (var key in keys)
        {
            if (Application.Current.Resources.Contains(key))
            {
                Application.Current.Resources.Remove(key);
            }
        }
    }
}