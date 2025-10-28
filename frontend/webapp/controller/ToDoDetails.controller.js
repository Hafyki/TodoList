sap.ui.define([
    "com/todolist/controller/BaseController"
], function (Controller) {
    "use strict";

    return Controller.extend("com.todolist.controller.ToDoDetails", {
        onInit: function () {

            var oModel = new sap.ui.model.json.JSONModel();
            this.getView().setModel(oModel, "todoModel");
            var oRouter = this.getOwnerComponent().getRouter();
            oRouter.getRoute("details").attachPatternMatched(this._onObjectMatched, this);

        },

        _onObjectMatched: function (oEvent) {

            var sId = oEvent.getParameter("arguments").id;
            this._searchData(sId);
        },

        _searchData: function (sId) {
            var oModel = this.getView().getModel("todoModel");
            var sUrl = `http://localhost:5128/api/todos/${sId}`;

            fetch(sUrl)
                .then(async (response) => {
                    if (!response.ok) {
                        throw new Error(`Erro ao buscar detalhes da tarefa de id ${sId}`);
                    }
                    return await response.json();
                })
                .then((data) => {
                    oModel.setData(data || {});

                    this.getView().bindElement({
                        path: "todoModel>/", 
                        model: "todoModel"
                    });
                })
                .catch((error) => {
                    console.error("Erro na busca:", error);
                    sap.m.MessageToast.show("Erro ao carregar detalhes da tarefa");
                });
        }
    });
});
