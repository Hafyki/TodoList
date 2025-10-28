sap.ui.define(["sap/ui/core/UIComponent"],
	function (UIComponent) {
	"use strict";

	return UIComponent.extend("com.todolist.Component", {
		metadata: {
			manifest: "json"
		},
		init: function(){
			UIComponent.prototype.init.apply(this,arguments);

			this.getRouter().initialize();
		},

		 createContent: function () {
      		return sap.ui.view({
				id: "app",
				viewName: "com.todolist.view.App",
				type: "XML"
			});
		}
	});
});