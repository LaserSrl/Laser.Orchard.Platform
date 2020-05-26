using Newtonsoft.Json;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Projections.Descriptors.SortCriterion;
using Orchard.Projections.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Utility.Extensions;
using Orchard.ContentManagement.MetaData;
using Orchard.Projections.FieldTypeEditors;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;

namespace Laser.Orchard.StartupConfig.Projections {
    public class ConfigurableSortCriterionProvider : ISortCriterionProvider {
        private readonly IEnumerable<IMemberBindingProvider> _bindingProviders;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IEnumerable<IFieldTypeEditor> _fieldTypeEditors;

        public ConfigurableSortCriterionProvider(
            IEnumerable<IMemberBindingProvider> bindingProviders,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IEnumerable<IFieldTypeEditor> fieldTypeEditors) {

            _bindingProviders = bindingProviders;
            _contentDefinitionManager = contentDefinitionManager;
            _contentFieldDrivers = contentFieldDrivers;
            _fieldTypeEditors = fieldTypeEditors;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public void Describe(DescribeSortCriterionContext describe) {
            describe.For("General", T("General"), T("General sort criteria"))
                .Element("ConfigurableSorting",
                    T("Configurable Sort Criteria"),
                    T("Sorts the results by choosing the criterion from a list"),
                    context => ApplyFilter(context),
                    context => T("Order based off a variable configuration"),
                    ConfigurableSortCriterionProviderForm.FormName);
        }

        public void ApplyFilter(SortCriterionContext context) {
            var json = (string)context.State.CriteriaArray;
            var indexStr = (string)context.State.CriterionIndex;
            int criterionIndex = 0;
            int.TryParse(indexStr, out criterionIndex);
            try {
                var criteriaArray = JsonConvert
                    .DeserializeObject<SortCriterionConfiguration[]>(json);
                if (criteriaArray != null && criteriaArray.Any()) {
                    if (criterionIndex >= criteriaArray.Count()) {
                        // given index is out of range
                        criterionIndex = 0;
                    }
                    // get the ith criterion
                    var criterionToUse = criteriaArray[criterionIndex];
                    if (criterionToUse.IsForField()) {
                        // order based on a field
                        ApplyCriterionForField(context, criterionToUse);
                    } else if (criterionToUse.IsForPart()) {
                        // order based on a part's property
                        ApplyCriterionForPart(context, criterionToUse);
                    }
                }
            } catch (Exception ex) {
                // impossible to parse the array
                var eMsg = T("There was an error while attempting to process the Sorting configuration.").Text
                    + Environment.NewLine
                    + T(" Array: {0}", json).Text
                    + Environment.NewLine
                    + T("Index: {0}", indexStr).Text
                    + Environment.NewLine
                    + T("Error: {0}", ex.Message).Text;
                Logger.Error(eMsg);
            }
        }

        #region Sorting on a ContentField or one of its properties
        private Dictionary<string, IEnumerable<IContentFieldDriver>> _groupedDrivers;
        private IEnumerable<IContentFieldDriver> DriversForField(string definitionName) {
            if (_groupedDrivers == null) {
                _groupedDrivers = new Dictionary<string, IEnumerable<IContentFieldDriver>>();
            }
            if(!_groupedDrivers.ContainsKey(definitionName)) {
                _groupedDrivers.Add(definitionName, 
                    _contentFieldDrivers
                        .Where(x => x.GetFieldInfo()
                            .Any(fi => fi.FieldTypeName == definitionName)));
            }
            return _groupedDrivers[definitionName];
        }
        private List<IFieldTypeEditor> GetFieldEditors(string definitionName, string propertyName) {
            var fieldTypeEditors = new List<IFieldTypeEditor>();
            // drivers for the ContentField
            var drivers = DriversForField(definitionName);
            // delegate that will help us figure out the tables
            var membersContext = new DescribeMembersContext(
                (storageName, storageType, displayName, description) => {
                    // get the correct field type editor
                    if ((storageName == null && propertyName == null)
                        || (storageName ?? string.Empty).Equals(propertyName ?? string.Empty)) {
                        IFieldTypeEditor fieldTypeEditor = _fieldTypeEditors
                            .FirstOrDefault(x => x.CanHandle(storageType));
                        if (fieldTypeEditor != null) {
                            fieldTypeEditors.Add(fieldTypeEditor);
                        }
                    }
                });
            foreach (var driver in drivers) {
                driver.Describe(membersContext);
            }
            return fieldTypeEditors;
        }
        private void ApplyCriterionForField(
            SortCriterionContext context, SortCriterionConfiguration criterion) {
            // This uses the logic from ContentFieldsSortCriterion
            var partDefinition = _contentDefinitionManager.GetPartDefinition(criterion.PartName);
            if (partDefinition == null) {
                Logger.Error(T("Impossible to find a part definition with name {0}.", 
                    criterion.PartName).Text);
            } else {
                var fieldDefinition = partDefinition.Fields
                    .FirstOrDefault(fd => fd.Name.Equals(criterion.FieldName));
                if (fieldDefinition == null) {
                    Logger.Error(T("Impossible to find a field definition with name {0} within the part {1}.",
                        criterion.FieldName, criterion.PartName).Text);
                } else {

                    var propertyName = string.Join(".",
                        // part
                        criterion.PartName,
                        // field
                        criterion.FieldName,
                        // field's property (e.g. LinkField.Text)
                        criterion.PropertyName ?? "");
                    
                    var fieldTypeEditors = GetFieldEditors(fieldDefinition.FieldDefinition.Name, criterion.PropertyName);
                    if (fieldTypeEditors.Any()) {
                        // I think there should be only one
                        foreach (var fieldTypeEditor in fieldTypeEditors) {
                            // use an alias with the join so that two filters on the same Field Type wont collide
                            var relationship = fieldTypeEditor.GetFilterRelationship(propertyName.ToSafeName());
                            // generate the predicate based on the editor which has been used
                            Action<IHqlExpressionFactory> predicate = y => y.Eq("PropertyName", propertyName);
                            // apply a filter for the specific property
                            context.Query = context.Query.Where(relationship, predicate);
                            // apply sort
                            context.Query = criterion.Ascending
                                ? context.Query.OrderBy(relationship, x => x.Asc(context.GetSortColumnName()))
                                : context.Query.OrderBy(relationship, x => x.Desc(context.GetSortColumnName()));
                        }
                    } else {
                        Logger.Error(T("Impossible to identify the IFieldTypeEditor to sort by {0}.", propertyName).Text);
                    }
                }
            }
        }
        #endregion

        #region Sorting on ContentPartRecord.Property based on MemberBinding
        private Dictionary<Type, IGrouping<Type, BindingItem>> _groupsDictionary;
        private Dictionary<Type, IGrouping<Type, BindingItem>> GetMemberGroups() {
            if (_groupsDictionary == null) {
                // use MemberBinding to get the Record
                var builder = new BindingBuilder();
                foreach (var bindingProvider in _bindingProviders) {
                    bindingProvider.GetMemberBindings(builder);
                }
                _groupsDictionary = builder.Build()
                    .GroupBy(b => b.Property.DeclaringType)
                    .ToDictionary(b => b.Key, b => b);
            }
            return _groupsDictionary;
        }
        private void ApplyCriterionForPart(
            SortCriterionContext context, SortCriterionConfiguration criterion) {
            var groupedMembers = GetMemberGroups();

            var typeKey = groupedMembers.Keys
                // the type for the ContentPartRecord we asked
                .FirstOrDefault(k => k.FullName
                    .Equals(criterion.PartRecordTypeName, StringComparison.OrdinalIgnoreCase));
            if (typeKey == null) {
                // quality of life: allow to input only the class name rather than
                // the full name. e.g. TitlePartRecord rather than 
                // Orchard.Core.Title.Models.TitlePartRecord
                var types = groupedMembers.Keys
                    .Where(k => k.Name
                        .Equals(criterion.PartRecordTypeName, StringComparison.OrdinalIgnoreCase));
                if (types.Count() == 1) {
                    typeKey = types.First();
                } else if (types.Count() > 1) {
                    var eMsg = T("The type asked for in the sort criterion does not have a unique name. Use its FullName.").Text
                        + Environment.NewLine
                        + T("The configuration is {0}.", criterion.PartRecordTypeName).Text
                        + Environment.NewLine
                        + T("Possible types found are: {0}", string.Join(Environment.NewLine, types.Select(t => t.FullName))).Text;
                    Logger.Error(eMsg);
                }
            }
            if (typeKey != null) {
                var membersGroup = groupedMembers[typeKey];
                var member = membersGroup
                    // the property we asked
                    .FirstOrDefault(m => m.Property.Name
                        .Equals(criterion.PropertyName, StringComparison.OrdinalIgnoreCase));
                if (member != null) {
                    context.Query = criterion.Ascending
                        ? context.Query
                            .OrderBy(alias => 
                            alias.ContentPartRecord(typeKey),
                                x => x.Asc(criterion.PropertyName))
                        : context.Query
                            .OrderBy(alias => 
                            alias.ContentPartRecord(typeKey),
                                x => x.Desc(criterion.PropertyName));
                } else {
                    Logger.Error(
                        T("It was impossible to uniquely identify a property named {0} for type {1}. Perhaps it lacks a MemberBinding configuration?",
                        criterion.PropertyName, criterion.PartRecordTypeName).Text);
                }
            } else {
                Logger.Error(
                    T("It was impossible to uniquely identify a type for {0}. Perhaps it lacks a MemberBinding configuration?", 
                    criterion.PartRecordTypeName).Text);
            }
        }
        #endregion
    }
}