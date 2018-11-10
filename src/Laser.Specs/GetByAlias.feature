Feature: GetByAlias
	In order to receive content
	As a mobile user
	I want to view the content I ask for

@mytag
Scenario: GetByAlias a page
	# enable stuff
	Given I have installed Orchard
		And I have installed "Laser.Orchard.StartupConfig"
		And I have installed "Laser.Orchard.WebServices"
	# create the page
	When I go to "Admin/Contents/Create/Page"
		And I fill in
			| name        | value   |
			| Title.Title | MyTitle |
		And I hit "Publish Now"
		# get the page from our webservice
		And I go to "Laser.Orchard.WebServices/json/getbyalias?displayalias=MyTitle"
	Then the status should be 200 "OK"
		And response starts with "{"n":"Model","v":"ContentItem""
		And response contains "{"n":"Title","v":"\"MyTitle\""}"

	#I can also use the different route
	# page has been created already
	# get the page from our webservice
	When I go to "WebServices/Alias?displayalias=MyTitle"
	Then the status should be 200 "OK"
		And response starts with "{"n":"Model","v":"ContentItem""
		And response contains "{"n":"Title","v":"\"MyTitle\""}"	


