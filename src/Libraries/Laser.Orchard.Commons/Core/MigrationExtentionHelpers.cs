using System.Globalization;
using System.Text;
using Laser.Orchard.Commons.Enums;
using Orchard.ContentManagement.MetaData.Builders;


namespace Laser.Orchard.Commons.Core {


  public static class MigrationExtentionHelpers {


    # region ** Estensioni per Content Parts **

    /// <summary>
    /// OBSOLETO.
    /// </summary>
    public static ContentPartDefinitionBuilder MediaPickerField(
      this ContentPartDefinitionBuilder builder,
      string name, 
      bool required = false,
      string displayName = null, 
      string hint = null) {

      if (displayName == null)
        displayName = SplitCamel(name);

      return builder
        .WithField(name, 
          fieldBuilder => fieldBuilder
            .OfType("MediaPickerField")
            .WithDisplayName(displayName)
            .WithSetting("MediaPickerFieldSettings.Required", required.ToString(CultureInfo.InvariantCulture))
            .WithSetting("MediaPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
            .WithSetting("MediaPickerFieldSettings.Hint", hint));
    }

		   
		public static ContentPartDefinitionBuilder MediaLibraryPickerField(
			this ContentPartDefinitionBuilder builder,
      string name, 
      bool multiple = false,
			bool required = false,
      string displayedContentTypes = null,
      string displayName = null, 
			string hint = null) {

      if (displayName == null)
        displayName = SplitCamel(name);

      return builder
        .WithField(name, 
          fieldBuilder => fieldBuilder
            .OfType("MediaLibraryPickerField")
            .WithDisplayName(displayName)
            .WithSetting("MediaLibraryPickerFieldSettings.Required", required.ToString(CultureInfo.InvariantCulture))
            .WithSetting("MediaLibraryPickerFieldSettings.Multiple", multiple.ToString(CultureInfo.InvariantCulture))
            .WithSetting("MediaLibraryPickerFieldSettings.DisplayedContentTypes", displayedContentTypes)
						.WithSetting("MediaLibraryPickerFieldSettings.AllowedExtensions", "jpg jpeg png gif")
            .WithSetting("MediaLibraryPickerFieldSettings.Hint", hint));
    }


    public static ContentPartDefinitionBuilder TextField(
      this ContentPartDefinitionBuilder builder,
      string name, 
      TextFieldFlavor flavor, 
      bool required = false, 
      string displayName = null, 
      string hint = null) {

      if (displayName == null)
        displayName = SplitCamel(name);

      return builder
        .WithField(name, 
          fieldBuilder => fieldBuilder
            .OfType("TextField")
            .WithDisplayName(displayName)
            .WithSetting("TextFieldSettings.Required", required.ToString(CultureInfo.InvariantCulture))
            .WithSetting("TextFieldSettings.Flavor", SplitCamel(flavor.ToString()))
            .WithSetting("TextFieldSettings.Hint", hint));
    }


    public static ContentPartDefinitionBuilder BooleanField(
      this ContentPartDefinitionBuilder builder,
      string name, 
      bool defaultValue,
      string displayName = null, 
      string hint = null) {

      if (displayName == null)
        displayName = SplitCamel(name);

      return builder
        .WithField(name, 
          fieldBuilder => fieldBuilder
            .OfType("BooleanField")
            .WithDisplayName(displayName)
            .WithSetting("BooleanFieldSettings.Hint", hint)
            .WithSetting("BooleanFieldSettings.DefaultValue", defaultValue.ToString(CultureInfo.InvariantCulture)));
    }

    # endregion

    
    # region ** Estensioni per Content Types **

    public static ContentTypeDefinitionBuilder AutoroutePart(
      this ContentTypeDefinitionBuilder builder, 
      string pathPrefix = null) {

      // DEFAULT: "[{Name:'Title', Pattern: '{Content.Slug}', Description: 'my-page'}]"
      var pattern = 
        string.Format("[{{Name:'{0}/Title', Pattern: '{0}/{{Content.Slug}}', Description: 'my-page'}}]", pathPrefix);

      return builder
        .WithPart("AutoroutePart", 
          partBuilder => partBuilder
            .WithSetting("AutorouteSettings.PatternDefinitions", pattern)
            //.WithSetting("AutorouteSettings.AllowCustomPattern", "true")
            //.WithSetting("AutorouteSettings.AutomaticAdjustmentOnEdit", "false")
            //.WithSetting("AutorouteSettings.DefaultPatternIndex", "0")
            );
    }


    public static ContentTypeDefinitionBuilder BodyPart(
      this ContentTypeDefinitionBuilder builder, 
      BodyPartFlavor defaultFlavor = BodyPartFlavor.Html) {

      // TinyMce
      return builder
        .WithPart("BodyPart", 
          partBuilder => partBuilder
            .WithSetting("BodyTypePartSettings.Flavor", defaultFlavor.ToString()));
    }



		/// <summary>
		/// Restituisce una PART con relativi settings
		/// </summary>
		/// <param name="builder">Builder</param>
		/// <param name="showOwnerEditor">Visualizza le informazioni sul proprietario</param>
		/// <returns>ContentTypeDefinitionBuilder</returns>
	  public static ContentTypeDefinitionBuilder CommomPart(
		  this ContentTypeDefinitionBuilder builder,
		  bool showOwnerEditor = false) {
		  return(CommomPart(builder, showOwnerEditor, false));
	  }

		/// <summary>
		/// Restituisce una PART con relativi settings
		/// </summary>
		/// <param name="builder">Builder</param>
		/// <param name="showOwnerEditor">Visualizza le informazioni sul proprietario</param>
		/// <param name="showDateEditor">Visualizza le informazioni sulla data di creazione</param>
		/// <returns>ContentTypeDefinitionBuilder</returns>
	  public static ContentTypeDefinitionBuilder CommomPart(
      this ContentTypeDefinitionBuilder builder, 
      bool showOwnerEditor = false,
			bool showDateEditor = false) {

      return builder
        .WithPart("CommonPart",
					partBuilder => partBuilder
						.WithSetting("OwnerEditorSettings.ShowOwnerEditor", showOwnerEditor.ToString(CultureInfo.InvariantCulture).ToLower())
    				.WithSetting("DateEditorSettings.ShowDateEditor", showDateEditor.ToString(CultureInfo.InvariantCulture).ToLower()));
}

    # endregion
    

    private static string SplitCamel(string enumString) {

      var sb = new StringBuilder();
      var last = char.MinValue;

      foreach (var c in enumString) {
        if (char.IsLower(last) && char.IsUpper(c)) {
          sb.Append(' ');
          sb.Append(c.ToString(CultureInfo.InvariantCulture).ToLower());
        } else {
          sb.Append(c);
        }
        last = c;
      }

      return sb.ToString();
    }


  }
}