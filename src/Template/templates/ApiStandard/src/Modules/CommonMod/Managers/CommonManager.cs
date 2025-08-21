namespace CommonMod.Managers;

public class CommonManager(ILogger<CommonManager> logger, Localizer localizer) : ManagerBase(logger)
{
    public Dictionary<string, List<EnumDictionary>> GetEnumDictionary()
    {
        var enums = EnumHelper.GetAllEnumInfo();

        enums
            .Values.ToList()
            .ForEach(v =>
            {
                v.ForEach(e =>
                {
                    e.Description = localizer?.Get(e.Description) ?? e.Description;
                });
            });

        return enums;
    }
}
