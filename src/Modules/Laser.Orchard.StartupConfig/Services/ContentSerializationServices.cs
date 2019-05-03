using Markdown.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Fields;
using Orchard.Fields.Fields;
using Orchard.Localization.Models;
using Orchard.Localization.Services;
using Orchard.Projections.Services;
using Orchard.Taxonomies.Fields;
using Orchard.Taxonomies.Models;
using Orchard.Taxonomies.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Laser.Orchard.StartupConfig.Services {
    public interface IContentSerializationServices : IDependency {
        JProperty SerializeContentItem(ContentItem item, int actualLevel);
        JObject GetJson(IContent content, int page = 1, int pageSize = 10);
        JObject Terms(IContent content, int maxLevel = 10);
        void NormalizeSingleProperty(JObject json);
    }

    public class ContentSerializationServices : IContentSerializationServices {
        private readonly IOrchardServices _orchardServices;
        private readonly IProjectionManager _projectionManager;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ILocalizationService _localizationService;
        private readonly MarkdownFilter _markdownFilter;

        private readonly string[] _skipAlwaysProperties;
        private readonly string _skipAlwaysPropertiesEndWith;
        private readonly string[] _skipFieldProperties;
        private readonly string[] _skipFieldTypes;
        private readonly string[] _skipPartNames;
        private readonly string[] _skipPartProperties;
        private readonly string[] _skipPartTypes;

        private readonly Type[] _basicTypes;

        private int _maxLevel = 10;

        private List<string> processedItems;

        public ContentSerializationServices(IOrchardServices orchardServices,
            IProjectionManager projectionManager, ITaxonomyService taxonomyService,
            ILocalizationService localizationService) {
            _orchardServices = orchardServices;
            _projectionManager = projectionManager;
            _taxonomyService = taxonomyService;
            _localizationService = localizationService;
            _markdownFilter = new MarkdownFilter();

            _skipAlwaysProperties = new string[] { "ContentItemRecord", "ContentItemVersionRecord" };
            _skipAlwaysPropertiesEndWith =  "Proxy" ;
            _skipFieldProperties = new string[] { "Storage", "Name", "DisplayName", "Setting" };
            _skipFieldTypes = new string[] { "FieldDefinition", "PartFieldDefinition" };
            _skipPartNames = new string[] { "InfosetPart", "FieldIndexPart", "IdentityPart", "UserPart", "UserRolesPart", "AdminMenuPart", "MenuPart" };
            _skipPartProperties = new string[] { };
            _skipPartTypes = new string[] { "ContentItem", "Zones", "TypeDefinition", "TypePartDefinition", "PartDefinition", "Settings", "Fields", "Record" };

            _basicTypes = new Type[] {
                typeof(string),
                typeof(decimal),
                typeof(float),
                typeof(int),
                typeof(bool),
                typeof(DateTime),
                typeof(Enum)
            };

            processedItems = new List<string>();
        }

        public JObject Terms(IContent content, int maxLevel = 10) {
            JObject json;
            _maxLevel = maxLevel;
            dynamic contentToSerialize = null, termPart = null;
            try {
                if (content.ContentItem.ContentType.EndsWith("Taxonomy")) {
                    contentToSerialize = content;
                    json = new JObject(SerializeObject(content, 0));
                    //NormalizeSingleProperty(json);
                    return json;
                } else if (content.ContentItem.ContentType.EndsWith("Term") || !String.IsNullOrWhiteSpace(content.ContentItem.TypeDefinition.Settings["Taxonomy"])) {
                    termPart = ((dynamic)content.ContentItem).TermPart;
                    if (termPart != null) {
                        json = new JObject(SerializeObject(content, 0));
                        contentToSerialize = _taxonomyService.GetChildren(termPart, false);
                        var resultArray = new JArray();
                        foreach (var resulted in contentToSerialize) {
                            resultArray.Add(new JObject(SerializeObject(resulted, 0)));
                        }
                        json.Add("SubTerms", resultArray);
                        //NormalizeSingleProperty(json);
                        return json;
                    }
                }
            } catch {
            }
            return null;
        }

        public JObject GetJson(IContent content, int page = 1, int pageSize = 10) {
            JObject json;
            dynamic shape = _orchardServices.ContentManager.BuildDisplay(content); // Forse non serve nemmeno
            var filteredContent = shape.ContentItem;

            json = new JObject(SerializeObject(filteredContent, 0));
            dynamic part;

            #region [Projections]
            // Projection
            try {
                part = ((dynamic)filteredContent).ProjectionPart;
            } catch {
                part = null;
            }
            if (part != null) {
                var queryId = part.Record.QueryPartRecord.Id;
                var queryItems = _projectionManager.GetContentItems(queryId, (page - 1) * pageSize, pageSize);
                var resultArray = new JArray();
                foreach (var resulted in queryItems) {
                    resultArray.Add(new JObject(SerializeContentItem((ContentItem)resulted, 0)));
                }
                if ((json.Root.HasValues) && (json.Root.First.HasValues) && (json.Root.First.First.HasValues)) {
                    JProperty array = new JProperty("ProjectionPageResults", resultArray);
                    json.Root.First.First.Last.AddAfterSelf(array);
                } else {
                    json.Add("ProjectionPageResults", resultArray);
                }
            }
            #endregion

            //NormalizeSingleProperty(json);

            return json;
        }

        /// <summary>
        /// Accorpa gli oggetti che hanno una sola proprietà con la proprietà padre.
        /// Es. Creator: { Value: 2 } diventa Creator: 2.
        /// </summary>
        /// <param name="json"></param>
        public void NormalizeSingleProperty(JObject json) {
            List<JToken> nodeList = new List<JToken>();

            // scandisce tutto l'albero dei nodi e salva i nodi potenzialmente "interessanti" in una lista
            nodeList.Add(json.Root);
            for (int i = 0; i < nodeList.Count; i++) {
                foreach (var tempNode in nodeList[i].Children()) {
                    if (tempNode.HasValues) {
                        nodeList.Add(tempNode);
                    }
                }
            }

            // scorre tutti i nodi per cercare quelli da accorpare
            foreach (var tempNode in nodeList) {
                if (tempNode.Count() == 1) {
                    if (tempNode.First.HasValues == false) {
                        if ((tempNode.Parent != null) && (tempNode.Parent.Count == 1)) {
                            if ((tempNode.Parent.Parent != null)
                                && (tempNode.Parent.Parent.Count == 1)
                                && (tempNode.Parent.Parent.Type == JTokenType.Property)) {
                                (tempNode.Parent.Parent as JProperty).Value = tempNode.First;
                            }
                        }
                    }
                }
            }
        }

        public JProperty SerializeContentItem(ContentItem item, int actualLevel) {
            if ((actualLevel + 1) > _maxLevel) {
                return new JProperty("ContentItem", null);
            }
            JProperty jsonItem;
            var jsonProps = new JObject(
                new JProperty("Id", item.Id),
                new JProperty("Version", item.Version));

            var partsObject = new JObject();
            var parts = item.Parts
                .Where(cp => !cp.PartDefinition.Name.Contains("`") && !_skipPartNames.Contains(cp.PartDefinition.Name)
                );
            foreach (var part in parts) {
                jsonProps.Add(SerializePart(part, actualLevel + 1, item));
            }

            jsonItem = new JProperty(item.ContentType,
                jsonProps
                );

            return jsonItem;
        }

        private JProperty SerializePart(ContentPart part, int actualLevel, ContentItem item = null) {
            // ciclo sulle properties delle parti
            var properties = part.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                !_skipPartTypes.Contains(prop.Name) //skip 
                );
            var partObject = new JObject();
            foreach (var property in properties) {
                // skippa la property "Id" se ha lo stesso valore del content item che contiene la part
                if ((property.Name == "Id") && (part.Id == part.ContentItem.Id)) {
                    continue;
                }
                try {
                    if (!_skipPartProperties.Contains(property.Name)) {
                        object val = property.GetValue(part, BindingFlags.GetProperty, null, null, null);
                        if (val != null) {
                            PopulateJObject(ref partObject, property, val, _skipPartProperties, actualLevel);
                        }
                    }
                } catch { }
            }

            //// now add the fields to the json object....
            foreach (var contentField in part.Fields) {
                var fieldObject = SerializeField(contentField, actualLevel, item);
                partObject.Add(fieldObject);
            }

            try {
                if (part.GetType() == typeof(ContentPart) && !part.PartDefinition.Name.EndsWith("Part")) {
                    return new JProperty(part.PartDefinition.Name + "DPart", partObject);
                } else {
                    return new JProperty(part.PartDefinition.Name, partObject);
                }
            } catch {
                return new JProperty(Guid.NewGuid().ToString(), partObject);
            }
        }

        private void SerializeTaxonomyField(TaxonomyField taxoField, int actualLevel, ref JObject fieldObject, ContentItem item) {
            var localizationPart = item?.As<LocalizationPart>();
            fieldObject.Add("Terms", JToken.FromObject(taxoField.Terms.Select(x => x.Id).ToList()));
            var taxo = taxoField.PartFieldDefinition.Settings["TaxonomyFieldSettings.Taxonomy"];
            var taxoPart = _taxonomyService.GetTaxonomyByName(taxo);
            if (localizationPart != null && localizationPart.Culture != null) {
                var taxoLocalization = taxoPart.ContentItem?.As<LocalizationPart>();
                if (taxoLocalization != null && taxoLocalization.Culture != null) {
                    if (localizationPart.Culture.Culture != taxoLocalization.Culture.Culture) {
                        //try to find the correctly localized taxonomy
                        taxoPart = _localizationService
                          .GetLocalizedContentItem(taxoPart.ContentItem, localizationPart.Culture.Culture)
                          ?.ContentItem?.As<TaxonomyPart>() ?? taxoPart;
                    }
                }
            }
            JArray arr = new JArray();
            fieldObject.Add("Taxonomy", arr);
            foreach (var term in taxoPart.Terms) {
                CustomTermPart customTermPart = new CustomTermPart {
                    Id = term.Id,
                    Name = term.Name,
                    Path = term.Path,
                    Selectable = term.Selectable,
                    Slug = term.Slug
                };
                JToken jObj = JToken.FromObject(customTermPart);
                arr.Add(jObj);
                var contentPartList = term.ContentItem.Parts.Where(x => (x.GetType().Name == "ContentPart") && (x.PartDefinition.Name == term.ContentItem.TypeDefinition.Name)); // part aggiunta da Orchard per contenere i fields diretti
                foreach (var contentPart in contentPartList) {
                    foreach (var innerField in contentPart.Fields) {
                        jObj.Last.AddAfterSelf(SerializeField(innerField, actualLevel));
                    }
                }
            }
        }

        private JProperty SerializeField(ContentField field, int actualLevel, ContentItem item = null) {
            var fieldObject = new JObject();
            if (field.FieldDefinition.Name == "EnumerationField") {
                var enumField = (EnumerationField)field;
                string[] selected = enumField.SelectedValues;
                string[] options = enumField.PartFieldDefinition.Settings["EnumerationFieldSettings.Options"].Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                fieldObject.Add("Options", JToken.FromObject(options));
                fieldObject.Add("SelectedValues", JToken.FromObject(selected));
            } else if (field.FieldDefinition.Name == "TaxonomyField") {
                SerializeTaxonomyField((TaxonomyField)field, actualLevel, ref fieldObject, item);
            }
            else if (field.FieldDefinition.Name == "NumericField") {
                var numericField = field as NumericField;
                object val = 0;
                if (numericField.Value.HasValue) {
                    val = numericField.Value.Value;
                }
                FormatValue(ref val);
                return new JProperty(field.Name + field.FieldDefinition.Name, val);
            }
            else if (field.FieldDefinition.Name == "TextField") {
                var textField = field as TextField;
                object val = textField.Value;
                if(val != null) {
                    if (textField.PartFieldDefinition.Settings.ContainsKey("TextFieldSettings.Flavor")) {
                        var flavor = textField.PartFieldDefinition.Settings["TextFieldSettings.Flavor"];
                        // markdownFilter acts only if flavor is "markdown"
                        val = _markdownFilter.ProcessContent(val.ToString(), flavor);
                    }
                    FormatValue(ref val);
                }
                return new JProperty(field.Name + field.FieldDefinition.Name, val);
            }
            else if (field.FieldDefinition.Name == "InputField") {
                var inputField = field as InputField;
                object val = inputField.Value;
                FormatValue(ref val);
                return new JProperty(field.Name + field.FieldDefinition.Name, val);
            }
            else if (field.FieldDefinition.Name == "BooleanField") {
                var booleanField = field as BooleanField;
                object val = false;
                if (booleanField.Value.HasValue) {
                    val = booleanField.Value.Value;
                }
                FormatValue(ref val);
                return new JProperty(field.Name + field.FieldDefinition.Name, val);
            }
            else if (field.FieldDefinition.Name == "DateTimeField") {
                var dateTimeField = field as DateTimeField;
                object val = dateTimeField.DateTime;
                FormatValue(ref val);
                return new JProperty(field.Name + field.FieldDefinition.Name, val);
            }
            else {
                var properties = field.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(prop =>
                    !_skipFieldTypes.Contains(prop.Name) //skip 
                    );

                foreach (var property in properties) {
                    try {
                        if (!_skipFieldProperties.Contains(property.Name)) {
                            object val = property.GetValue(field, BindingFlags.GetProperty, null, null, null);
                            if (val != null) {
                                PopulateJObject(ref fieldObject, property, val, _skipFieldProperties, actualLevel);
                            }
                        }
                    } catch {

                    }
                }
            }

            return new JProperty(field.Name + field.FieldDefinition.Name, fieldObject);
        }

        private JProperty SerializeObject(object item, int actualLevel, string[] skipProperties = null) {
            JProperty aux = null;
            if ((actualLevel + 1) > _maxLevel) {
                return new JProperty(item.GetType().Name, null);
            }
            try {
                if (item.GetType().Name.EndsWith(_skipAlwaysPropertiesEndWith))
                    return new JProperty(item.GetType().Name, null);
                if (((dynamic)item).Id != null) {
                    if (processedItems.Contains(String.Format("{0}({1})", item.GetType().Name, ((dynamic)item).Id)))
                        return new JProperty(item.GetType().Name, null);
                }
            } catch {
            }
            skipProperties = skipProperties ?? new string[0];
            try {
                if (item is ContentPart) {
                    return SerializePart((ContentPart)item, actualLevel);
                } else if (item is ContentField) {
                    return SerializeField((ContentField)item, actualLevel);
                } else if (item is ContentItem) {
                    return SerializeContentItem((ContentItem)item, actualLevel + 1);
                } else if (typeof(IEnumerable).IsInstanceOfType(item)) { // Lista o array
                    JArray array = new JArray();
                    foreach (var itemArray in (item as IEnumerable)) {
                        if (IsBasicType(itemArray.GetType())) {
                            var valItem = itemArray;
                            FormatValue(ref valItem);
                            array.Add(valItem);
                        } else {
                            aux = SerializeObject(itemArray, actualLevel + 1, skipProperties);
                            array.Add(new JObject(aux));
                        }
                    }
                    if (item.GetType().GetProperties().Count(x => x.Name == "Id") > 0) {
                        PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    }
                    return new JProperty(item.GetType().Name, array);
                } else if (item.GetType().IsClass) {
                    var members = item.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                    .Union(item.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    .Where(m => !skipProperties.Contains(m.Name) && !_skipAlwaysProperties.Contains(m.Name) && !m.Name.EndsWith(_skipAlwaysPropertiesEndWith))
                    ;
                    List<JProperty> properties = new List<JProperty>();
                    foreach (var member in members) {
                        var propertyInfo = item.GetType().GetProperty(member.Name);
                        object val = item.GetType().GetProperty(member.Name).GetValue(item);
                        if (IsBasicType(propertyInfo.PropertyType)) {
                            var memberVal = val;
                            FormatValue(ref memberVal);
                            properties.Add(new JProperty(member.Name, memberVal));
                        } else if (typeof(IEnumerable).IsInstanceOfType(val)) {
                            JArray arr = new JArray();
                            properties.Add(new JProperty(member.Name, arr));
                            foreach (var element in (val as IEnumerable)) {
                                if (IsBasicType(element.GetType())) {
                                    var valItem = element;
                                    FormatValue(ref valItem);
                                    arr.Add(valItem);
                                } else {
                                    aux = SerializeObject(element, actualLevel + 1, skipProperties);
                                    arr.Add(new JObject(aux));
                                }
                            }
                        } else {
                                                        
                            aux = SerializeObject(propertyInfo.GetValue(item), actualLevel + 1, skipProperties);
                            properties.Add(aux);
                        }
                    }
                    if (item.GetType().GetProperties().Count(x => x.Name == "Id") > 0) {
                        PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    }
                    return new JProperty(item.GetType().Name, new JObject(properties));

                    //JObject propertiesObject;
                    //var serializer = JsonSerializerInstance();
                    //propertiesObject = JObject.FromObject(item, serializer);
                    //foreach (var skip in skipProperties) {
                    //    propertiesObject.Remove(skip);
                    //}
                    //PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    //return new JProperty(item.GetType().Name, propertiesObject);
                } else {
                    if (item.GetType().GetProperties().Count(x => x.Name == "Id") > 0) {
                        PopulateProcessedItems(item.GetType().Name, ((dynamic)item).Id);
                    }
                    return new JProperty(item.GetType().Name, item);
                }
            } catch (Exception ex) {
                return new JProperty(item.GetType().Name, ex.Message);
            }
        }

        private void PopulateJObject(ref JObject jObject, PropertyInfo property, object val, string[] skipProperties, int actualLevel) {

            JObject propertiesObject;
            var serializer = JsonSerializerInstance();
            if (val is Array || val.GetType().IsGenericType) {
                JArray array = new JArray();
                foreach (var itemArray in (IEnumerable)val) {

                    if (!IsBasicType(itemArray.GetType())) {
                        var aux = SerializeObject(itemArray, actualLevel, skipProperties);
                        array.Add(new JObject(aux));
                    } else {
                        var valItem = itemArray;
                        FormatValue(ref valItem);
                        array.Add(valItem);
                    }
                }
                jObject.Add(new JProperty(property.Name, array));

            } else {
                // jObject.Add(SerializeObject(val, skipProperties));
            }
            if (!IsBasicType(val.GetType())) {
                try {
                    propertiesObject = JObject.FromObject(val, serializer);
                    foreach (var skip in skipProperties) {
                        propertiesObject.Remove(skip);
                    }
                    jObject.Add(property.Name, propertiesObject);
                } catch {
                    jObject.Add(new JProperty(property.Name, val.GetType().FullName));
                }
            } else {
                FormatValue(ref val);
                jObject.Add(new JProperty(property.Name, val));
            }
        }

        private bool IsBasicType(Type type) {
            return _basicTypes.Contains(type) || type.IsEnum;
        }

        private void FormatValue(ref object val) {
            if (val != null && val.GetType().IsEnum) {
                val = val.ToString();
            }
        }

        private void PopulateProcessedItems(string key, dynamic id) {
            if (id != null)
                processedItems.Add(String.Format("{0}({1})", key, id.ToString()));
        }

        private JsonSerializer JsonSerializerInstance() {
            return new JsonSerializer {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                DateFormatString = "#MM-dd-yyyy hh.mm.ss#",
            };
        }
    }

    class CustomTermPart {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Slug { get; set; }
        public bool Selectable { get; set; }
    }
}