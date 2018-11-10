function removeNestedForm(element, container, deleteElement) {
	$container = $(element).parents(container);
	$container.find(deleteElement).val('True');
	$container.hide();
}

function addNestedForm(container, counter, ticks, content) {
	var nextIndex = $(container + " " + counter).length;
	//var nextIndex = $(counter).length;
	var pattern = new RegExp(ticks, "gi");
	content = content.replace(pattern, nextIndex);
	$(container).append(content);
}