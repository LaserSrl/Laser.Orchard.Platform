using Orchard;
using System;

namespace Laser.Orchard.StartupConfig.Localization
{
    public interface IDateLocalization : IDependency
    {
        DateTime? ReadDateLocalized(DateTime? thedate, bool withTimezone = false, string cultureName = "");
        string WriteDateLocalized(DateTime? thedate, bool withtime = false, string cultureName = "");
        string WriteTimeLocalized(DateTime? thedate, string cultureName = "");
        DateTime? StringToDatetime(string date, string time, bool withTimezone = false, string cultureName = "");

    }
}