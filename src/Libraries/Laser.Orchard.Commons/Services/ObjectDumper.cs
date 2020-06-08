using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Shapes;
using System.Globalization;
using System.Web.Helpers;
using Orchard.Core.Common.Fields;
using Markdown.Services;

namespace Laser.Orchard.Commons.Services {

    public class ContentFlags {
        public string Name { get; set; }
        public List<string> Ids { get; set; }
    }
    public class VirtualPart {
        public string Name { get { return "ContentPart"; } }
        public string ContentObject { get { return "ContentPart"; } }

    }
    public class ObjectDumper {
        private const int MaxStringLength = 60;

        private readonly Stack<object> _parents = new Stack<object>();
        private readonly Stack<XElement> _currents = new Stack<XElement>();

        private readonly int _levels;
        private readonly List<ContentFlags> _contentRendered;
        private readonly XDocument _xdoc;


        private XElement _current;


        private string[] _skipParts;
        private string[] _skipMembers;
        private string[] _keepOnlyTheeseMembers;
        private string[] _filterContentFieldsParts;
        private bool _omitContentItem;
        private string[] _complexBehaviour;
        private string[] _includeMembers; // members ending with "Record" or "Definition" that have to be always included 
                                          // object/key/dump
        private readonly MarkdownFilter _markdownFilter;

        public ObjectDumper(int levels, string[] filterContentFieldsParts = null, bool omitContentItem = false, bool tinyResponse = true, string[] complexBehaviour = null) {
            _levels = levels;
            _xdoc = new XDocument();
            _xdoc.Add(_current = new XElement("ul"));
            _complexBehaviour = complexBehaviour;
            _markdownFilter = new MarkdownFilter();

            _contentRendered = new List<ContentFlags>();
            if (tinyResponse) {
                _skipParts = new string[]{"CommonPart", "Version","LocalizationPart","Storage", "Taxonomy",
                             "Ids","LayerPart","CommentsPart","QrCodePart","MobilePushPart","InfosetPart","FieldIndexPart","IdentityPart","UserPart",
                             "UserRolesPart", "AdminMenuPart", "MenuPart", "TermsPart","FieldStorageEventStorage","ButtonToWorkflowsPart" };
            } else {
                _skipParts = new string[]{
                "InfosetPart","FieldIndexPart","IdentityPart","UserPart","UserRolesPart", "AdminMenuPart", "MenuPart"};
            }
            _skipMembers = new string[]{
               "*/Storage", "*/Settings","*/Zones","LocalizationPart/CultureField","LocalizationPart/MasterContentItemField","DisplayName","TableData"/*,"LocalizationPart/MasterContentItem"*/
            };
            _keepOnlyTheeseMembers = new string[]{
                "MasterContentItem/Id"
            };
            _filterContentFieldsParts = filterContentFieldsParts;
            _omitContentItem = omitContentItem;
            // members ending with "Record" or "Definition" that have to be always included 
            _includeMembers = new string[] { "PolicyTextInfoPartRecord" };
        }
        public List<ContentFlags> RenderedContentList { get { return _contentRendered; } }

        private dynamic cleanobj(dynamic objec) {
            if (((dynamic)objec).ToRemove != null) {
                return cleanobj(((dynamic)objec).ToRemove);
            } else
                return objec;
        }


        public XElement Dump(object o, string name, string nameDynamicJsonArray = "") {
            if (_parents.Count >= _levels) {
                return _current;
            }
            if (o == null && !_complexBehaviour.Select(s => s.ToLowerInvariant()).Contains("returnnulls")) { // Se non devo
                return null;
            }
            if (o != null){
                if(string.Equals(name, "JsonDataTablePart", StringComparison.InvariantCulture)) {
                    name = string.Format("{0}{1}", (o as ContentPart).ContentItem.ContentType, name);
                    nameDynamicJsonArray = name;
                }
                if (FormatType(o) == "FieldExternal") {
                    // testo il caso in cui non ho un contentobject ma un content url
                    if (((dynamic)o).ContentObject != null) {
                        //     return _current;
                        if (!(((dynamic)o).Setting.GenerateL)) {

                            o = ((dynamic)o).ContentObject;
                            o = cleanobj(o);
                            name = name + "_exp";
                            nameDynamicJsonArray = "List<generic>";
                        } else {
                            return _current;
                        }
                    }
                }
            } 
            //_parents.Push(o);
            _parents.Push("a");
            // starts a new container
            EnterNode("li");

            try {
                if (o == null) {
                    DumpValue(null, name);
                } else if (o.GetType().IsValueType || o is string) {
                    DumpValue(o, name);
                } else {
                    if (_parents.Count >= _levels) {
                        return _current;
                    } else if (o.ToString().EndsWith(".ContentItemVersionRecord") || o.ToString().EndsWith(".ContentItemRecord")) {
                        return _current;
                    }

                    DumpObject(o, name, nameDynamicJsonArray);
                }
            } finally {
                _parents.Pop();
                RestoreCurrentNode();
            }

            return _current;
        }

        private void DumpValue(object o, string name) {
            string formatted = FormatValue(o);
            _current.Add(
                new XElement("h1", new XText(name)),
                new XElement("span", formatted)
            );
        }

        private void DumpObject(object o, string name, string nameDynamicJsonArray = "") {
            string elementType;// = nameDynamicJsonArray == "" ? (FormatType(o) == "DynamicJsonObject" ? name + "[]" : FormatType(o) == "DynamicJsonArray" ? name : FormatType(o)) : nameDynamicJsonArray;
            if (nameDynamicJsonArray == "") {
                string tmp_FormatType = FormatType(o);
                switch (tmp_FormatType) {
                    case "DynamicJsonObject":
                        nameDynamicJsonArray = name;
                        var dynJsonObject = (DynamicJsonObject)o;
                        dynamic dynObject = (dynamic)dynJsonObject;
                        var members = dynJsonObject.GetDynamicMemberNames();
                        string member = members.First();
                        if ((((dynamic)dynObject)[member]).GetType().Equals(typeof(DynamicJsonArray)))
                            elementType = name + "[]";//"List<generic>";
                        else
                            elementType = name;
                        break;
                    case "DynamicJsonArray":
                        elementType = name;
                        break;
                    default:
                        elementType = tmp_FormatType;
                        break;
                }
            } else
                elementType = nameDynamicJsonArray;
            _current.Add(
                new XElement("h1", new XText(name)),
                // new XElement("span", elementType)
                 new XElement("span", elementType, new XAttribute("type", "string"))

            );


            EnterNode("ul");

            try {
                if (o is IDictionary) {
                    DumpDictionary((IDictionary)o);
                } else if (o is IShape) {
                    DumpShape((IShape)o);

                    // a shape can also be IEnumerable
                    if (o is Shape) {
                        var items = o.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "Items");

                        var classes = o.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "Classes");

                        var attributes = o.GetType()
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                            .FirstOrDefault(m => m.Name == "Attributes");

                        if (classes != null) {
                            DumpMember(o, classes);
                        }

                        if (attributes != null) {
                            DumpMember(o, attributes);
                        }

                        if (items != null) {
                            DumpMember(o, items);
                        }


                        // DumpEnumerable((IEnumerable) o);
                    }

                } else if (o is IEnumerable) {
                    DumpEnumerable((IEnumerable)o);
                } else if (o is ContentItem && name.StartsWith("[")) { // Array di Contents
                    DumpMembers(o, _filterContentFieldsParts, name);
                } else if (o.GetType().Equals(typeof(DynamicJsonObject))) {
                    DumpDynamicMembers(o);
                } else {
                    //???
                    DumpMembers(o, _filterContentFieldsParts, name);
                }
            } finally {
                RestoreCurrentNode();
            }
        }

        private void DumpDynamicMembers(object o) {
            var dynJsonObject = (DynamicJsonObject)o;
            dynamic dynObject = (dynamic)dynJsonObject;
            var members = dynJsonObject.GetDynamicMemberNames();
            string padrearray = "";
            foreach (var member in members) {
                if ((((dynamic)dynObject)[member]) == null) {
                    continue;
                }
                if (member != "ToRemove") {
                    if ((((dynamic)dynObject)[member]).GetType().Equals(typeof(DynamicJsonArray))) {
                        padrearray = member;
                        dynamic ArrayLoop = (((dynamic)dynObject)[member]);
                        if (ArrayLoop[0].GetType().Equals(typeof(string)))
                            SafeCall(() => Dump(dynObject[member], member, "string[]"));//, "List<"+member+">"));
                        else
                            if (ArrayLoop[0].GetType().Equals(typeof(int)))
                                SafeCall(() => Dump(dynObject[member], member, "Int32[]"));
                            else
                                if (ArrayLoop[0].GetType().Equals(typeof(DateTime)))
                                    SafeCall(() => Dump(dynObject[member], member, "DateTime[]"));
                                else {
                                    int posizion = 0;
                                    //    SafeCall(() => Dump(new VirtualPart(), '[' + posizion.ToString() + ']'));

                                    foreach (var item in ArrayLoop) {
                                        if (item != null) {
                                            SafeCall(() => Dump(item, '[' + posizion.ToString() + ']', padrearray));
                                            posizion++;
                                        }
                                    }
                                }

                    } else {
                        SafeCall(() => Dump(dynObject[member], member, padrearray));
                    }
                }
            }

        }
        private void DumpMembers(object o, string[] filterFields = null, string objectName = "") {
            if (o.GetType().Name.ToLower().Contains("contentitem")) {
                var contentId = ((ContentItem)o).Id;
                var contentType = ((ContentItem)o).ContentType;
                if (!_omitContentItem) {
                    var currentContent = _contentRendered.SingleOrDefault(w => w.Name == contentType);
                    if (currentContent == null) {
                        _contentRendered.Add(new ContentFlags { Name = contentType, Ids = new List<string> { contentId.ToString() } });
                    } else if (!currentContent.Ids.Contains(contentId.ToString())) {
                        currentContent.Ids.Add(contentId.ToString());
                    } else {
                        //this contentItem has been rendered yet
                        // But I need to render Type and Id
                        var tinyObject = new {
                            Id = contentId,
                            ContentType = contentType
                        };
                        DumpMembers(tinyObject);
                        return;
                    }
                } else {
                    //this contentItem has been rendered yet
                    // But I need to render Type and Id
                    var tinyObject = new {
                        Id = contentId,
                        ContentType = contentType
                    };
                    DumpMembers(tinyObject);
                    return;
                }
            }
            var members = o.GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public).Cast<MemberInfo>()
                .Union(o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                .Where(m => !m.Name.StartsWith("_")) // remove members with a name starting with '_' (usually proxied objects)
                .Where(m => (!m.Name.EndsWith("Definition") && (!m.Name.EndsWith("Record")) || _includeMembers.Contains(m.Name)));
            if (_keepOnlyTheeseMembers.Select(s => s.Split('/')[0]).Contains(objectName)) { // se ho impostato Membri specifici allora recupero solo quelli
                members = members
                    .Where(m => _keepOnlyTheeseMembers.Contains(objectName + "/" + m.Name));
            } else { // altrimenti escludo quelli dannosi o ridondanti o inutili
                members = members
                    .Where(m => !_skipMembers.Contains(objectName + "/" + m.Name) && !_skipMembers.Contains("*/" + m.Name));
            }
            members = members.ToList();


            if (!members.Any()) {
                return;
            }

            foreach (var member in members) {
                if (o is ContentItem && member.Name == "ContentManager"
                    || member.Name == "Parts"
                    || member.Name == "Fields"
                    || o is Delegate
                    || o is Type
                    || _skipMembers.Contains(member.Name)) {
                    continue;
                }
                if (member is PropertyInfo) {
                    var prop = (PropertyInfo)member;
                    if (prop.GetIndexParameters().Length == 0 && prop.CanRead && _skipMembers.Contains(prop.GetValue(o, null))) {
                        continue;
                    }

                }
                SafeCall(() => DumpMember(o, member));
            }

            // process ContentItem.Parts specifically
            foreach (var member in members) {
                if (o is ContentItem && member.Name == "Parts") {
                    foreach (var part in ((ContentItem)o).Parts) {
                        // ignore contentparts like ContentPart<ContentItemVersionRecord>
                        if (part.GetType().IsGenericType) {
                            continue;
                        }
                        if (!_skipParts.Contains(part.PartDefinition.Name)) {
                            if (filterFields == null || filterFields.Length == 0 || filterFields.Contains(part.PartDefinition.Name.ToLower()) || !part.PartDefinition.Name.EndsWith("Part"))
                                SafeCall(() => Dump(part, part.PartDefinition.Name));
                        }
                        else if (part.PartDefinition.Name == "UserPart") {
                            SafeCall(() => Dump(new UserViewModel(((dynamic)part).Email), "UserPart"));
                        }
                    }
                }
            }
            foreach (var member in members) {
                // process ContentPart.Fields specifically
                if (o is ContentPart && member.Name == "Fields") {
                    foreach (var field in ((ContentPart)o).Fields) {
                        if (!_skipParts.Contains(field.FieldDefinition.Name)) {

                            if (filterFields == null || filterFields.Length == 0 || filterFields.Contains(field.Name.ToLower())) {
                                SafeCall(() => Dump(field, field.Name));
                            }
                        }
                    }
                }
            }
        }

        private void DumpEnumerable(IEnumerable enumerable) {
            if (!enumerable.GetEnumerator().MoveNext()) {
                return;
            }

            int i = 0;
            foreach (var child in enumerable) {
                Dump(child, string.Format("[{0}]", i++));
            }
        }

        private void DumpDictionary(IDictionary dictionary) {
            if (dictionary.Keys.Count == 0) {
                return;
            }

            foreach (var key in dictionary.Keys) {
                Dump(dictionary[key], string.Format("[\"{0}\"]", key));
            }
        }

        private void DumpShape(IShape shape) {
            var value = shape as Shape;

            if (value == null) {
                return;
            }

            foreach (DictionaryEntry entry in value.Properties) {
                // ignore private members (added dynamically by the shape wrapper)
                if (entry.Key.ToString().StartsWith("_")) {
                    continue;
                }

                Dump(entry.Value, entry.Key.ToString());
            }
        }

        private void DumpMember(object o, MemberInfo member) {
            if (member is MethodBase || member is EventInfo)
                return;
            //if (_filterSubItemsParts != null && _filterSubItemsParts.Length != 0 && !_filterSubItemsParts.Contains(member.Name)) {
            //    return;
            //}

            if (member is FieldInfo) {
                var field = (FieldInfo)member;
                Dump(field.GetValue(o), member.Name);
            } else if (member is PropertyInfo) {
                var prop = (PropertyInfo)member;

                if (prop.GetIndexParameters().Length == 0 && prop.CanRead) {
                    if (!_skipParts.Contains(o.GetType().Name)) {
                        var val = prop.GetValue(o, null);
                        if(o.GetType().Name == "TextField") {
                            var tf = o as TextField;
                            if (tf.PartFieldDefinition.Settings.ContainsKey("TextFieldSettings.Flavor")) {
                                var flavor = tf.PartFieldDefinition.Settings["TextFieldSettings.Flavor"];
                                // markdownFilter acts only if flavor is "markdown"
                                val = _markdownFilter.ProcessContent(val?.ToString(), flavor);
                            }
                        }
                        Dump(val, member.Name);
                    }
                }
            }
        }

        private static string FormatValue(object o) {
            if (o == null)
                return "null";

            string formatted;

            if (o is string || o is Enum) {
                // remove central part if tool long
                //if (formatted.Length > MaxStringLength) {
                //    formatted = formatted.Substring(0, MaxStringLength / 2) + "..." + formatted.Substring(formatted.Length - MaxStringLength / 2);
                //}
                formatted = "\"" + o.ToString() + "\"";
            } else if (o is DateTime) {
                formatted = "#" + ((DateTime)o).ToString(CultureInfo.InvariantCulture) + "#";
            } else if (o is Single || o is float || o is double) {
                try {
                    formatted = ((float)o).ToString(CultureInfo.InvariantCulture);
                } catch {
                    NumberFormatInfo nfi = new NumberFormatInfo();
                    nfi.NumberGroupSeparator = "";
                    nfi.NumberDecimalSeparator = ",";
                    formatted = ((double)o).ToString("N0", nfi);
                }
            } else if (o is decimal) {
                formatted = ((decimal)o).ToString(CultureInfo.InvariantCulture);
            } else {
                formatted = o.ToString();
            }

            return formatted;
        }

        private static string FormatType(object item) {
            var shape = item as IShape;
            if (shape != null) {
                return shape.Metadata.Type + " Shape";
            }
            return FormatType(item.GetType());
        }

        private static string FormatType(Type type) {
            if (type.IsGenericType) {
                var genericArguments = String.Join(", ", type.GetGenericArguments().Select(FormatType).ToArray());
                if (genericArguments.EndsWith("Record") && type.Name.Substring(0, type.Name.IndexOf('`')) == "PersistentGenericBag") {
                    return String.Format("{0}<{1}>", "List", genericArguments);
                } else {
                    return String.Format("{0}<{1}>", type.Name.Substring(0, type.Name.IndexOf('`')), genericArguments);
                }
            }
            if (type.Name.EndsWith("Proxy")) {
                return type.BaseType.Name;
            }
            return type.Name;
        }

        private static void SafeCall(Action action) {
            try {
                action();
            } catch {
                // ignore exceptions is safe call
            }
        }

        private void SaveCurrentNode() {
            _currents.Push(_current);
        }

        private void RestoreCurrentNode() {
            if (!_current.DescendantNodes().Any()) {
                _current.Remove();
            }

            _current = _currents.Pop();
        }

        private void EnterNode(string tag) {
            SaveCurrentNode();
            _current.Add(_current = new XElement(tag));
        }

        //Fake UserPart in order to expose some properties of the User
        private class UserViewModel {
            public UserViewModel(string email) {
                Email = email ?? "";
            }

            public string Email { get; private set; }
        }
    }

   
}