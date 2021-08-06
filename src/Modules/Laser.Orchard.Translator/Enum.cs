public enum ElementToTranslate
{
    Module, // -> TranslationRecord.ContainerType = "M"
    Theme, // -> TranslationRecord.ContainerType = "T"
    Tenant, // -> TranslationRecord.ContainerType = "A"
    Undefined // -> TranslationRecord.ContainerType = "U"
}