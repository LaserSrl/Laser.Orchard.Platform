using Contrib.Profile.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentTypes.Extensions;
using Orchard.ContentTypes.Settings;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Configuration;
using Orchard.Logging;
using Orchard.UI.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Contrib.Profile.Settings {
    public class ProfileFrontEndEditorEvents : ContentDefinitionEditorEventsBase {
        // The settings are attached to all parts and fields
        // They are attached only if there is a ProfilePart
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly Lazy<IEnumerable<IShellSettingsManagerEventHandler>> _settingsManagerEventHandlers;
        private readonly ShellSettings _shellSettings;
        private readonly IContentManager _contentManager;
        private readonly IShapeFactory _shapeFactory;
        private readonly IEnumerable<IContentPartDriver> _contentPartDrivers;
        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IFrontEndProfileService _frontEndProfileService;

        // This event handler is instanced once per transaction, so I can use a bool to save checks
        private bool _typeHasProfilePart { get; set; }

        public ProfileFrontEndEditorEvents(
            IContentDefinitionManager contentDefinitionManager,
            Lazy<IEnumerable<IShellSettingsManagerEventHandler>> settingsManagerEventHandlers,
            ShellSettings shellSettings,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IEnumerable<IContentPartDriver> contentPartDrivers,
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IFrontEndProfileService frontEndProfileService) {

            _contentDefinitionManager = contentDefinitionManager;
            _settingsManagerEventHandlers = settingsManagerEventHandlers;
            _contentManager = contentManager;
            _shapeFactory = shapeFactory;
            _contentPartDrivers = contentPartDrivers;
            _contentFieldDrivers = contentFieldDrivers;
            _frontEndProfileService = frontEndProfileService;

            _shellSettings = shellSettings;
        }

        public ILogger Logger { get; set; }

        // We will store the ContentTypeDefinitionBuilder at the first opportunity, so that we can 
        // safely use it to affect the type's settings.
        private ContentTypeDefinitionBuilder _typeBuilder;

        #region ProfileFrontEndSettings for Fields
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition) {
            if (_typeHasProfilePart) {
                var settings = definition.Settings.GetModel<ProfileFrontEndSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel) {

            if (_typeHasProfilePart) {
                var settings = new ProfileFrontEndSettings();
                if (updateModel.TryUpdateModel(settings, "ProfileFrontEndSettings", null, null)) {
                    ProfileFrontEndSettings.SetValues(builder, settings.AllowFrontEndDisplay, settings.AllowFrontEndEdit);

                    // Update the type settings
                    var partDefinition = _contentDefinitionManager.GetPartDefinition(builder.PartName);
                    var typeDefinition = _typeBuilder.Current;

                    // At this stage, the settings in the FieldDefinition are not updated, so the IFrontEndProfileService 
                    // has no way of knowing whether the value of settings.AllowFrontEndEdit has changed. We need to pass 
                    // it along to be used.
                    var fieldPlacements = GetEditorPlacement(typeDefinition, partDefinition, builder.Current, settings.AllowFrontEndEdit);
                    if (fieldPlacements.Any()) {
                        UpdateFrontEndPlacements(typeDefinition, fieldPlacements);
                        //// schedules a re-evaluation of the shell
                        //_settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_shellSettings), Logger);
                    }
                }
                yield return DefinitionTemplate(settings);
            }
        }
        #endregion

        #region ProfileFrontEndSettings for Parts
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (_typeHasProfilePart ||
                definition.ContentTypeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                var settings = definition.Settings.GetModel<ProfileFrontEndSettings>();
                yield return DefinitionTemplate(settings);
            }
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(builder.TypeName);
            if (_typeHasProfilePart ||
                typeDefinition.Parts.Any(ctpd => ctpd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                var settings = new ProfileFrontEndSettings();
                if (updateModel.TryUpdateModel(settings, "ProfileFrontEndSettings", null, null)) {
                    ProfileFrontEndSettings.SetValues(builder, settings.AllowFrontEndDisplay, settings.AllowFrontEndEdit);

                    // Update the type settings
                    var partDefinition = typeDefinition.Parts
                        .FirstOrDefault(ctpd => ctpd.PartDefinition.Name == builder.Name)?.PartDefinition;
                    if (partDefinition != null) { // sanity check
                        // At this stage, the settings in the PartDefinition are not updated, so the IFrontEndProfileService 
                        // has no way of knowing whether the value of settings.AllowFrontEndEdit has changed. We need to 
                        // pass it along to be used.
                        var partPlacements = GetEditorPlacement(typeDefinition, partDefinition, settings.AllowFrontEndEdit);
                        if (partPlacements.Any()) {
                            UpdateFrontEndPlacements(typeDefinition, partPlacements);
                            //// schedules a re-evaluation of the shell
                            //_settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_shellSettings), Logger);
                        }
                    }
                }

                yield return DefinitionTemplate(settings);
            }
        }
        #endregion


        public override void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) {
            // This is called after everything else has been done for a type.
            // However, the updates for parts and fields are processed afterwards, meaning that we need a
            // further step there to make sure the type settings are up to date.
            // At this stage, check whether this is a type with a ProfilePart
            var typeDefinition = builder.Current;
            if (typeDefinition.Parts.Any(cptd => cptd.PartDefinition.Name == "ProfilePart")) {
                _typeHasProfilePart = true;
                // In this case we want to save in a setting for the type all the configuration regarding
                // front-end edit for the different fields and parts. This is similar to what
                // is done in Orchard.ContentTypes for the "Edit Placement" functionality
                var placements = GetEditorPlacement(typeDefinition).ToList();

                // We need to hold the builder here so that we can save settings without them being overwritten
                // after the event handling here completes
                _typeBuilder = builder;

                UpdateFrontEndPlacements(typeDefinition, placements);
            }
            base.TypeEditorUpdated(builder);
        }

        /// <summary>
        /// We build a dummy content item in order to execute the drivers for all ContentParts and ContentFields.
        /// This way we can process the resulting shapes one by one and set the placement to "-" (don't place) for
        /// those we don't want to show on front end editors. We need this to prevent their UpdateEditor methods to
        /// be executed.
        /// </summary>
        /// <param name="definition">The definition of the ContentType we are working on.</param>
        /// <returns>The PlacementSetting objects for all ContentParts and ContentFields in the type.</returns>
        private IEnumerable<PlacementSettings> GetEditorPlacement(
            ContentTypeDefinition definition) {
            var contentType = definition.Name;

            var context = BuildContext(definition);

            var placementSettings = new List<PlacementSettings>();

            foreach (var result in GetPartDriversResults(context).Values) {
                if (result != null) {
                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType));
                }
            }

            foreach (var results in GetFieldDriversResults(context).Values) {
                if (results != null) {
                    foreach (var result in results) {
                        placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType));
                    }
                }
            }

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;
            }
        }

        /// <summary>
        /// We build a dummy content item in order to execute the drivers for a specific ContentPart and its ContentFields.
        /// This way we can process the resulting shapes one by one and set the placement to "-" (don't place) for
        /// those we don't want to show on front end editors. We need this to prevent their UpdateEditor methods to
        /// be executed.
        /// </summary>
        /// <param name="definition">The definition of the ContentType we are working on.</param>
        /// <param name="partDefinition">The definition for the part we are processing.</param>
        /// <param name="showEditor">The new value for the flag telling wheter the editor should be shown on frontend.</param>
        /// <returns>The PlacementSettings objects for the ContentPart and its ContentFields.</returns>
        private IEnumerable<PlacementSettings> GetEditorPlacement(
            ContentTypeDefinition definition, ContentPartDefinition partDefinition, bool showEditor) {
            var contentType = definition.Name;

            var context = BuildContext(definition);

            var placementSettings = new List<PlacementSettings>();

            var partDrivers = _contentPartDrivers;
            var fieldDrivers = _contentFieldDrivers;
            if (partDefinition != null) { // Only drivers for the part
                partDrivers = partDrivers
                    .Where(cpd => cpd.GetPartInfo()
                        .Any(pi => pi.PartName == partDefinition.Name ||
                            pi.PartName == "ContentPart")); // this handles metadata parts that do not have their own driver
            }

            var resultsDictionary = GetPartDriversResults(context);
            foreach (var driver in partDrivers) {
                DriverResult result = null;
                if (resultsDictionary.TryGetValue(driver, out result) && result != null) {
                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType, showEditor));
                }
            }

            foreach (var results in GetFieldDriversResults(context).Values
                .Where(dr => dr != null && dr.Any())) {
                foreach (var result in results.Where(dr => dr != null &&
                    dr.ContentPart != null &&
                    dr.ContentPart.PartDefinition.Name == partDefinition.Name)) {

                    placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType, showEditor));
                }
            }

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;
            }
        }

        /// <summary>
        /// We build a dummy content item in order to execute the drivers for a specific ContentField.
        /// This way we can process the resulting shapes one by one and set the placement to "-" (don't place) for
        /// those we don't want to show on front end editors. We need this to prevent their UpdateEditor methods to
        /// be executed.
        /// </summary>
        /// <param name="definition">The definition of the ContentType we are working on.</param>
        /// <param name="partDefinition">The definition for the part we are processing.</param>
        /// <param name="fieldDefinition">The definition for the field we are processing.</param>
        /// <param name="showEditor">The new value for the flag telling wheter the editor should be shown on frontend.</param>
        /// <returns>The PlacementSettings objects for the ContentField.</returns>
        private IEnumerable<PlacementSettings> GetEditorPlacement(
            ContentTypeDefinition definition, ContentPartDefinition partDefinition, ContentPartFieldDefinition fieldDefinition, bool showEditor) {
            var contentType = definition.Name;

            var context = BuildContext(definition);

            var placementSettings = new List<PlacementSettings>();

            var fieldDrivers = _contentFieldDrivers;
            if (fieldDefinition != null) { // only drivers for the field we are processing
                fieldDrivers = fieldDrivers
                    .Where(cfd => cfd.GetFieldInfo().Any(fi => fi.FieldTypeName == fieldDefinition.FieldDefinition.Name));
            }

            var resultsDictionary = GetFieldDriversResults(context);
            foreach (var driver in fieldDrivers) {
                IEnumerable<DriverResult> results;
                if (resultsDictionary.TryGetValue(driver, out results) && results != null && results.Any()) {
                    foreach (var result in results.Where(dr => dr != null &&
                        dr.ContentPart != null &&
                        dr.ContentPart.PartDefinition.Name == partDefinition.Name &&
                        dr.ContentField != null &&
                        dr.ContentField.Name == fieldDefinition.Name)) {
                        placementSettings.AddRange(ProcessEditDriverResult(result, context, contentType, showEditor));
                    }
                }
            }

            foreach (var placementSetting in placementSettings) {
                yield return placementSetting;
            }
        }

        private BuildEditorContext _defaultContext;

        /// <summary>
        /// Create a context for the execution of the drivers we use to compute the shapes we will have to place
        /// on the frontend editor views. The context is computed only once per request.
        /// </summary>
        /// <param name="definition">The definition of the ContentType we are working on.</param>
        /// <returns>The context for the drivers.</returns>
        private BuildEditorContext BuildContext(ContentTypeDefinition definition) {
            if (_defaultContext == null) {
                var content = _contentManager.New(definition.Name); //our dummy content

                dynamic itemShape = CreateItemShape("Content_Edit");
                itemShape.ContentItem = content;

                _defaultContext = new BuildEditorContext(itemShape, content, string.Empty, _shapeFactory);
                //get the default placements: if we don't provide these ourselves, placement for shapes will default
                //to null, preventing them to be displayed at all times.
                var defaultPlacements = definition.GetPlacement(PlacementType.Editor);
                BindPlacement(_defaultContext, defaultPlacements);
            }
            return _defaultContext;
        }

        private Dictionary<IContentPartDriver, DriverResult> _partDriverResults;

        /// <summary>
        /// Computes the Dictionary of the results for the drivers for the parts. This lets us avoid running the
        /// methods from the drivers several times.
        /// </summary>
        /// <param name="context">The context for the drivers.</param>
        /// <returns></returns>
        private Dictionary<IContentPartDriver, DriverResult> GetPartDriversResults(BuildEditorContext context) {
            if (_partDriverResults == null) {
                _partDriverResults = new Dictionary<IContentPartDriver, DriverResult>(_contentPartDrivers.Count());
                _contentPartDrivers.Invoke(driver => {
                    var result = driver.BuildEditor(context);
                    if (result != null) {
                        _partDriverResults.Add(driver, result);
                    }
                }, Logger);
            }
            return _partDriverResults;
        }

        private Dictionary<IContentFieldDriver, IEnumerable<DriverResult>> _fieldDriverResults;

        /// <summary>
        /// Computes the Dictionary of the results for the drivers for the fields. This lets us avoid running the
        /// methods from the drivers several times.
        /// </summary>
        /// <param name="context">The context for the drivers.</param>
        /// <returns></returns>
        private Dictionary<IContentFieldDriver, IEnumerable<DriverResult>> GetFieldDriversResults(BuildEditorContext context) {
            if (_fieldDriverResults == null) {
                _fieldDriverResults = new Dictionary<IContentFieldDriver, IEnumerable<DriverResult>>(_contentFieldDrivers.Count());
                _contentFieldDrivers.Invoke(driver => {
                    var result = driver.BuildEditorShape(context);
                    if (result != null) {
                        // We split CombinedResult. The reason is that fields of the same type are "received" in a same result.
                        // By splitting them, it's easier to process things later.
                        if (result is CombinedResult) {
                            _fieldDriverResults.Add(driver, ((CombinedResult)result).GetResults());
                        }
                        else {
                            _fieldDriverResults.Add(driver, new List<DriverResult>(1) { result });
                        }
                    }
                }, Logger);
            }
            return _fieldDriverResults;
        }

        /// <summary>
        /// Extract the PlacementSetting objects for the shapes returned by a driver.
        /// </summary>
        /// <param name="result">The result of executing a Driver.BuildEditor method.</param>
        /// <param name="context">The execution context for the driver.</param>
        /// <param name="typeName">The name of the ContentType we are processing.</param>
        /// <returns>The PlacementSetting objects for the results of the driver.</returns>
        private IEnumerable<PlacementSettings> ProcessEditDriverResult(
            DriverResult result, BuildShapeContext context, string typeName) {

            if (result is CombinedResult) {
                foreach (var subResult in ((CombinedResult)result).GetResults()) {
                    foreach (var placement in ProcessEditDriverResult(subResult, context, typeName)) {
                        yield return placement;
                    }
                }
            }
            else if (result is ContentShapeResult) {
                var part = result.ContentPart;
                if (part != null) { // sanity check: should always be true
                    var typePartDefinition = part.TypePartDefinition;
                    bool hidePlacement = false;
                    if (_frontEndProfileService.MayAllowPartEdit(typePartDefinition, typeName)) {
                        var field = result.ContentField;
                        if (field != null) { // we ran a driver for a ContentField rather than a ContentPart
                            hidePlacement = !(_frontEndProfileService.MayAllowFieldEdit(field.PartFieldDefinition));
                        }
                    }
                    else {
                        // don't show anything of this part
                        hidePlacement = true;
                    }
                    if (hidePlacement) { // Override placement only if the part/field have to be hidden
                        yield return GetPlacement((ContentShapeResult)result, context, typeName, hidePlacement);
                    }
                }
            }
        }

        /// <summary>
        /// Extract the PlacementSetting objects for the shapes returned by a driver.
        /// </summary>
        /// <param name="result">The result of executing a Driver.BuildEditor method.</param>
        /// <param name="context">The execution context for the driver.</param>
        /// <param name="typeName">The name of the ContentType we are processing.</param>
        /// <param name="showEditor">A boolean telling whether the results being processed should appear on the frontend.</param>
        /// <returns>The PlacementSetting objects for the results of the driver.</returns>
        private IEnumerable<PlacementSettings> ProcessEditDriverResult(
            DriverResult result, BuildShapeContext context, string typeName, bool showEditor = true) {

            if (result is CombinedResult) {
                foreach (var subResult in ((CombinedResult)result).GetResults()) {
                    foreach (var placement in ProcessEditDriverResult(subResult, context, typeName, showEditor)) {
                        yield return placement;
                    }
                }
            }
            else if (result is ContentShapeResult) {
                var part = result.ContentPart;
                if (part != null && !showEditor) { // sanity check: part should always be true; override placement only if the part/field have to be hidden
                    yield return GetPlacement((ContentShapeResult)result, context, typeName, !showEditor);
                }
            }
        }

        /// <summary>
        /// Compute the PlacementSetting object for a specific driver result.
        /// </summary>
        /// <param name="result">The driver result.</param>
        /// <param name="context">The execution context for the driver.</param>
        /// <param name="typeName">The name of the ContentType we are processing.</param>
        /// <param name="hidden">A boolean telling whether the results should be hidden on the frontend.</param>
        /// <returns>Th PlacementSetting object for the results being processed.</returns>
        private PlacementSettings GetPlacement(
            ContentShapeResult result, BuildShapeContext context, string typeName, bool hidden = false) {
            if (!hidden) return null; // override placement only if the part/field have to be hidden

            var placement = context.FindPlacement(
                result.GetShapeType(),
                result.GetDifferentiator(),
                result.GetLocation()
                );

            string zone = hidden ? "-" : placement.Location;
            string position = string.Empty;

            return new PlacementSettings {
                ShapeType = result.GetShapeType(),
                Zone = zone,
                Position = position,
                Differentiator = result.GetDifferentiator() ?? string.Empty
            };
        }

        /// <summary>
        /// Creates a default shape to hold the ones that are used to compute placement information.
        /// </summary>
        /// <param name="actualShapeType">The type for the shape.</param>
        /// <returns>The default shape we will use to hold the one sin the remainder of the processing.</returns>
        private dynamic CreateItemShape(string actualShapeType) {
            var zoneHolding = new ZoneHolding(() => _shapeFactory.Create("ContentZone", Arguments.Empty()));
            zoneHolding.Metadata.Type = actualShapeType;
            return zoneHolding;
        }

        /// <summary>
        /// Binds a default placement to the context for a part's driver execution.
        /// </summary>
        /// <param name="context">The execution context.</param>
        /// <param name="defaultSettings">PlacementSetting objects to fall back on.</param>
        private void BindPlacement(
            BuildShapeContext context, IEnumerable<PlacementSettings> defaultSettings) {

            context.FindPlacement = (partShapeType, differentiator, defaultLocation) => {
                var mockSetting = new PlacementSettings {
                    ShapeType = partShapeType,
                    Differentiator = differentiator
                };
                var defaultSetting = defaultSettings.FirstOrDefault(ps => ps.IsSameAs(mockSetting));
                defaultLocation = defaultSetting == null ? defaultLocation : //may still end up with a null defaultLocation
                    defaultSetting.Zone + (string.IsNullOrEmpty(defaultSetting.Position) ? "" : ":" + defaultSetting.Position);
                defaultLocation = string.IsNullOrWhiteSpace(defaultLocation) ? "Content:1" : defaultLocation; //avoid null fallbacks
                return new PlacementInfo {
                    Location = defaultLocation,
                    Source = string.Empty
                };
            };
        }

        /// <summary>
        /// Update the settings for the placement in the TypeDefinition.
        /// </summary>
        /// <param name="contentTypeDefinition">The definition of the ContentType that we will update.</param>
        /// <param name="newPlacements">The new placement settings, that will either replace their old versions,
        /// or be added anew to the settings.</param>
        private void UpdateFrontEndPlacements(
            ContentTypeDefinition contentTypeDefinition, IEnumerable<PlacementSettings> newPlacements) {
            SetCurrentPlacement(contentTypeDefinition, newPlacements);
        }

        // We use this dictionary to store, for each ContentType, all the settings.
        // this dictionary, when a type is being updated, will itself be updated
        // several times, as each part and field may update its own information.
        private Dictionary<string, // ContentType Name
            Dictionary<string, // ShapeType
                Dictionary<string, // Differentiator (important for ContentFields)
                    PlacementSettings>>> _currentPlacement;
        private PlacementSettings[] SetCurrentPlacement(
            ContentTypeDefinition contentTypeDefinition, IEnumerable<PlacementSettings> newPlacements) {
            // create the dictioanry if it does not exist (i.e. the first time this method
            // is ever called in a request)
            if (_currentPlacement == null) {
                _currentPlacement = new Dictionary<string, Dictionary<string, Dictionary<string, PlacementSettings>>>();
            }
            // add a Dictionary for the ContentType we are processing
            if (!_currentPlacement.ContainsKey(contentTypeDefinition.Name)) {
                _currentPlacement.Add(contentTypeDefinition.Name,
                    new Dictionary<string, Dictionary<string, PlacementSettings>>());
            }
            // dictionary of placements for this type
            var placementsForType = _currentPlacement[contentTypeDefinition.Name];
            // update placements for this type
            foreach (var placement in newPlacements) {
                // If we already had some information for this ShapeType
                if (placementsForType.ContainsKey(placement.ShapeType)) {
                    // update setting: we need to further drill things down on the differentiation
                    // this will generally not matter for parts, but it is required to correctly
                    // manage ContentFields, that generally share a single ShapeType.
                    var differentPlacements = placementsForType[placement.ShapeType];
                    if (differentPlacements.ContainsKey(placement.Differentiator)) {
                        // update
                        differentPlacements[placement.Differentiator] = placement;
                    } else {
                        // add
                        differentPlacements.Add(placement.Differentiator, placement);
                    }
                } else {
                    // add settings for this ShapeType
                    placementsForType.Add(placement.ShapeType,
                        new Dictionary<string, PlacementSettings>());
                    placementsForType[placement.ShapeType].Add(placement.Differentiator, placement);
                }
            }

            // pull the settings from the dictionary: for each shapeType we have a dictionary
            // of settings
            var placementsArray = placementsForType // Dictionary<string, Dictionary<string, PlacementSettings>>
                .SelectMany(kvp => kvp.Value.Values)
                .ToArray();

            // write the placement settings as a setting for the type, by serializing them all
            var serializer = new JavaScriptSerializer();
            if (_typeBuilder.Current.Name == contentTypeDefinition.Name) {
                _typeBuilder.WithSetting("ContentTypeSettings.Placement.ProfileFrontEndEditor",
                    serializer.Serialize(placementsArray));
            }
            else {
                contentTypeDefinition.Settings["ContentTypeSettings.Placement.ProfileFrontEndEditor"] =
                    serializer.Serialize(placementsArray);

                // persist changes: The type definition is persisted already after these events are processed
                _contentDefinitionManager.StoreTypeDefinition(contentTypeDefinition);
                // schedules a re-evaluation of the shell
                _settingsManagerEventHandlers.Value.Invoke(x => x.Saved(_shellSettings), Logger);
            }

            return placementsArray;
        }
    }
}