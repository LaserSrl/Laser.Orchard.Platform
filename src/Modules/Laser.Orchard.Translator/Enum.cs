public enum ElementToTranslate
{
    Module, // -> TranslationRecord.ContainerType = "M"
    Theme, // -> TranslationRecord.ContainerType = "T"
    Tenant, // -> TranslationRecord.ContainerType = "A"
    Undefined, // -> TranslationRecord.ContainerType = "U"
    OrchardModule, // -> TranslationRecord.ContainerType = "W"
    OrchardTheme, // -> TranslationRecord.ContainerType = "X"
    OrchardCore, // -> TranslationRecord.ContainerType = "Y"
    OrchardFramework // -> TranslationRecord.ContainerType = "Z"
}