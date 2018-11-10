namespace Laser.Orchard.Reporting.Models {
    public class ColorsSettings {
        public static string[] ChartColors = new[] { "#3C8DBC", "#00C0EF", "#00A65A", "#F39C12", "#F56954", "#D2D6DE", "#001F3F", "#39CCCC", "#605CA8", "#FF851B", "#D81B60", "#111111" };
        public static string[] ChartColorsLight = new[] { "#81B5D4", "#5AD6F5", "#5AC594", "#F7BF66", "#E98B7F", "#E2E4EA", "#5A6E83", "#7FDEDE", "#9896C7", "#FFB06B", "#E66B98", "#656565" };
        public static string[] ChartColorsDark = new[] { "#357CA5", "#00A7D0", "#008D4C", "#DB8B0B", "#D33724", "#B5BBC8", "#001A35", "#30BBBB", "#555299", "#FF7701", "#CA195A", "#000000" };
    }
    public enum ColorStyleValues {
        Normal = 0,
        Light = 1,
        Dark = 2
    }
    public enum ChartColorNames {
        Primary = 0,
        Info = 1,
        Success = 2,
        Warning = 3,
        Danger = 4,
        Gray = 5,
        Navy = 6,
        Teal = 7,
        Purple = 8,
        Orange = 9,
        Maroon = 10,
        Black = 11
    }
}