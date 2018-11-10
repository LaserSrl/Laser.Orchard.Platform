Feature: GDPR
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

@mytag
Scenario: The GDPR Module should be there with all its features
	Given I have installed Orchard
		# Installing Orchard does not automatically install all our modules, so install the GDPR module
		And I have installed "Laser.Orchard.GDPR"
	# I should see the features
	When I go to "Admin/Modules/Features"
	Then I should see "Laser.Orchard.GDPR"
		And I should see "Laser.Orchard.GDPR.Workflows"
		And I should see "Laser.Orchard.GDPR.ContentPickerFieldExtension"
		And I should see "Laser.Orchard.GDPR.MediaExtension"
		And I should see "Laser.Orchard.GDPR.Scheduling"
	# some features should all be enabled already, because of the GDPRFeaturesEventHandler. However, those 
	# handlers are not called in this kind of test. This means I should do UnitTests.
		#And I should see "<li class="feature enabled[^"]*" id="laser-orchard-g-d-p-r-workflows-feature"[^>]*>"
		#And I should see "<li class="feature enabled[^"]*" id="laser-orchard-g-d-p-r-content-picker-field-extension-feature"[^>]*>"
		#And I should see "<li class="feature enabled[^"]*" id="laser-orchard-g-d-p-r-media-extension-feature"[^>]*>"

# I split the other features in different scenarios because they require a bunch
# of our own modules
Scenario: The extension feature for contacts is there
	Given I have installed Orchard
        # That feature requires a bunch of our other modules
        # CommunicationGateway:
		And I have installed and not enabled "Laser.Orchard.Queries"
		And I have installed and not enabled "Laser.Orchard.ShortLinks"
		And I have installed and not enabled "Laser.Orchard.StartupConfig"
		And I have installed and not enabled "Laser.Orchard.ZoneAlternates"
		And I have installed and not enabled "Laser.Orchard.jQueryPlugins"
		And I have installed "Laser.Orchard.CommunicationGateway"
        # Finally:
		And I have installed "Laser.Orchard.GDPR"
	When I try to go to "Admin/Modules/Features"
	Then I should see "Laser.Orchard.GDPR.ContactExtension"

Scenario: The extension features for mobile are there
	Given I have installed Orchard
		# Mobile:
		And I have installed and not enabled "Laser.Orchard.StartupConfig"
		And I have installed and not enabled "Laser.Orchard.jQueryPlugins"
		And I have installed and not enabled "Laser.Orchard.Queries"
		And I have installed "Laser.Orchard.Mobile"
		# Finally:
		And I have installed "Laser.Orchard.GDPR"
	When I try to go to "Admin/Modules/Features"
	Then I should see "Laser.Orchard.GDPR.MobileExtension"
		And I should see "Laser.Orchard.GDPR.PushGatewayExtension"
		And I should see "Laser.Orchard.GDPR.SmsGatewayExtension"
		And I should see "Laser.Orchard.GDPR.SmsExtension"

Scenario: The extension feature for OpenAuth is there
	Given I have installed Orchard
		# OpenAuthentication:
		And I have installed and not enabled "Laser.Orchard.StartupConfig"
		And I have installed "Laser.Orchard.OpenAuthentication"
		# Finally:
		And I have installed "Laser.Orchard.GDPR"
	When I try to go to "Admin/Modules/Features"
	Then I should see "Laser.Orchard.GDPR.OpenAuthExtension"


Scenario: GDPRPart can be attached to a content type
	Given I have installed Orchard
		# Installing a module like this also enables it
		And I have installed "Laser.Orchard.GDPR"
	# check that the GDPPart exists among the other parts
	When I go to "Admin/ContentTypes/ListParts"
	Then the status should be 200 "OK"
		# Orchard parses part names in its own special way, by removing part, and splitting on capital letters
		And I should see "G D P R"
	# Create a new type and see that I can attach the GDPRPart
	When I go to "Admin/ContentTypes/Create"
        And I fill in
            | name | value |
            | DisplayName | MyGDPRItem |
            | Name | MyGDPRItem |
        And I hit "Create"
		# here we should be redirected to the form to add parts
		And I go to "Admin/ContentTypes/AddPartsTo/MyGDPRItem"
	Then I should see "G D P R"
		And I should see "value="GDPRPart""

Scenario: The GDPR permissions are there and can be configured
	Given I have installed Orchard
		# Installing a module like this also enables it
		And I have installed "Laser.Orchard.GDPR"
	When I go to "Admin/Roles"
	Then I should see "Administrator"
	# see that the admin role has the permissions
	When I go to "Admin/Roles/Edit/1"
	# the feature is there
	Then I should see "Laser.Orchard.GDPR"
		# the permissions are all there
		And I should see "Effective.ManageAnonymization"
		And I should see "Effective.ManageErasure"
		And I should see "Effective.ManageItemProtection"

Scenario: Admin can see and edit type settings for GDPRPart
	Given I have installed Orchard
	# Installing a module like this also enables it
		And I have installed "Laser.Orchard.GDPR"
	# create the type with the GDPRPart
		And I have a type "MyGDPRItem" with part "GDPRPart"
		And I have a type "MyGDPRItem" with part "TitlePart"
		And I have a type "MyGDPRItem" with a "TextField" field called "MyTextField"
	# go to the type editor view
	When I go to "Admin/ContentTypes/Edit/MyGDPRItem"
	# see that the UI for the settings is there
	Then response contains "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemType"
		And response contains "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.DeleteItemsAfterErasure"
	# check TitlePart settings too
		And response contains "Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldAnonymize"
		And response contains "Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldErase"
	# check field settings too
		And response contains "Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldAnonymize"
		And response contains "Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldErase"
	# go again to the type editor view
	When I go to "Admin/ContentTypes/Edit/MyGDPRItem"
	# and try to change the settings for type, part and field:
	# initially those values are at false
		And I fill in
            | name | value |
            | Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemType | True |
            | Parts[GDPRPart].GDPRPartTypeSettingsViewModel.DeleteItemsAfterErasure | True |
            | Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldAnonymize | True |
            | Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldErase | True |
		And I hit "Save"
	# Make sure we are on the right page
		And I go to "Admin/ContentTypes/Edit/MyGDPRItem"
	# check that those settings have changed and the others have not
	Then response has an element called "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemType" with attribute "checked" with value "checked"
		And response has an element called "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.DeleteItemsAfterErasure" with attribute "checked" with value "checked"
		And response has an element called "Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldAnonymize" with attribute "checked" with value "checked"
		And response has an element called "Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldErase" with attribute "checked" with value "checked"
	# negatives
		And response has an element called "Parts[TitlePart].GDPRPartPartSettingsViewModel.ShouldErase" without attribute "checked"
		And response has an element called "Fields[MyTextField].GDPRPartFieldSettingsViewModel.ShouldAnonymize" without attribute "checked"

Scenario: Admin can see the settings for ContentPickerFields
	Given I have installed Orchard
	# Installing a module like this also enables it
		And I have installed "Laser.Orchard.GDPR"
		And I have enabled "Laser.Orchard.GDPR.ContentPickerFieldExtension"
	# create the type with the GDPRPart
		And I have a type "MyGDPRItem" with part "GDPRPart"
		And I have a type "MyGDPRItem" with part "TitlePart"
		And I have a type "MyGDPRItem" with a "ContentPickerField" field called "MyCPF"
	# go to the type editor view
	When I go to "Admin/ContentTypes/Edit/MyGDPRItem"
	# see that the UI for the settings is there
	Then response contains "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemType"
	# check field settings too
		And response contains "Fields[MyCPF].ContentPickerFieldGDPRPartFieldSettingsViewModel.AttemptToAnonymizeItems"

Scenario: Admin can see the settings for MediaLibraryPickerFields
	Given I have installed Orchard
	# Installing a module like this also enables it
		And I have installed "Laser.Orchard.GDPR"
		And I have enabled "Laser.Orchard.GDPR.MediaExtension"
	# create the type with the GDPRPart
		And I have a type "MyGDPRItem" with part "GDPRPart"
		And I have a type "MyGDPRItem" with part "TitlePart"
		And I have a type "MyGDPRItem" with a "MediaLibraryPickerField" field called "MyMLPF"
	# go to the type editor view
	When I go to "Admin/ContentTypes/Edit/MyGDPRItem"
	# see that the UI for the settings is there
	Then response contains "Parts[GDPRPart].GDPRPartTypeSettingsViewModel.IsProfileItemType"
	# check field settings too
		And response contains "Fields[MyMLPF].MediaLibraryPickerFieldGDPRPartFieldSettingsViewModel.AttemptToAnonymizeItems"