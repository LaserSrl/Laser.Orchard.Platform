## Special Syntax of ColumnAliases

ColumnAliases support some specific metalanguage functions to parse special values for the columns.

Where these have parameters, they are separated by a semicolon ';'.

### AHREF(ColumnName; Text)

Results for this column are formatted as anchor tags. The value extracted from the report is the href attribute of the tag. The Text parameter is used as link text:

`<a href="value">Text</a>`

### IMGSRC(ColumnName; Title)

Results for this column are formatted as image tags. The value extracted from the report is the src attribute of the tag. The Title parameter is used as titlle for the image:

`<img src="value" title="Title" />`