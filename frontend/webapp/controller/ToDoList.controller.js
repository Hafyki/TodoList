sap.ui.define([
    "com/todolist/controller/BaseController",
], function (Controller) {
    "use strict";

    return Controller.extend("com.todolist.controller.ToDoList", {
        onInit: function () {
            var oModel = new sap.ui.model.json.JSONModel([]);
            this.getView().setModel(oModel, "todoModel");

            this._currentPage = 1;
            this._pageSize = 10;
            this._sortBy = "Id";
            this._isDescending = false;
            this._searchQuery = "";
            this._fetchFilteredData();
        },
        //GET
        _fetchFilteredData: function (sQuery) {
            var oModel = this.getView().getModel("todoModel");
            //Parametros de paginação
            var params = new URLSearchParams({
                pageNumber: this._currentPage,
                pageSize: this._pageSize,
                sortBy: this._sortBy,
                isDescending: this._isDescending
            });

            //Filtro da Barra de pesquisa
            if (this._searchQuery && this._searchQuery.trim() !== "") {
                params.append("title", this._searchQuery.trim());
            }
            var sBaseUrl = "http://localhost:5128/api/todos";
            
            var sUrl = `${sBaseUrl}?${params.toString()}`;
            
            sap.ui.core.BusyIndicator.show(0);

            fetch(sUrl)
                .then(function (response) {
                    if (!response.ok) {
                        throw new Error("Erro ao buscar tarefas");
                    }
                    return response.json();
                })
                .then(data => {
                    oModel.setData(data || []);

                    this._totalPages = data.totalPages;
                    this._totalItems = data.totalItems;
                    this._pageSize = data.pageSize;
                    this._currentPage = data.pageNumber;

                    this._updateTotalTasks();
                    this._updatePageIndicator();
                })
                .catch(function (error) {
                    console.error("Erro na busca:", error);
                    sap.m.MessageToast.show("Erro ao carregar tarefas filtradas");
                }).finally(() => {
                    sap.ui.core.BusyIndicator.hide();
                });
        },

        _updatePageIndicator:function () {
            var oText = this.byId("pageIndicator");
            oText.setText("Página " + this._currentPage + " de " + (this._totalPages || 1));
        },
        
        _updateTotalTasks:function () {
            var oText = this.byId("totalTasks");
            oText.setText("Tarefas (" + this._totalItems + ")");
        },

        //--- Busca ---
        onSearch: function (oEvent) {
            //Debounce
            if (this._searchTimer) {
                clearTimeout(this._searchTimer);
            }

            var sQuery = oEvent.getParameter("newValue");
            this._searchQuery = sQuery;
            this._searchTimer = setTimeout(() => {
                this._fetchFilteredData();
            }, 250);
        },
        // --- Tipo de Ordenação ---
        onSortFieldChange: function (oEvent) {
            this._sortBy = oEvent.getParameter("selectedItem").getKey();
            sap.m.MessageToast.show("Ordenar por: " + this._sortBy);
            this._currentPage = 1;
            this._fetchFilteredData();
        },

        // --- Direção de Ordenação ---
        onToggleSortDirection: function () {
            this._isDescending = !this._isDescending;

            var oButton = this.byId("sortDirectionButton");
            oButton.setIcon(
                this._isDescending ? "sap-icon://sort-descending" : "sap-icon://sort-ascending"
            );

            sap.m.MessageToast.show(
                "Ordenação: " + (this._isDescending ? "Descendente" : "Crescente")
            );

            this._fetchFilteredData();
        },

        // --- Tamanho da página ---
        onPageSizeChange: function (oEvent) {
            this._pageSize = parseInt(oEvent.getParameter("selectedItem").getKey(), 10);
            this._currentPage = 1;
            this._fetchFilteredData();
        },

        // --- Página anterior ---
        onPrevPage: function () {
            if (this._currentPage > 1) {
                this._currentPage--;
                this._fetchFilteredData();
            }
        },

        // --- Próxima página ---
        onNextPage: function () {
            this._currentPage++;
            this._fetchFilteredData();
        },
        
        //--- Alternar estado da tarefa ---
        onStatusChange: function (oEvent) {
            var bSelected = oEvent.getParameter("selected");
            var oSource = oEvent.getSource();
            var oContext = oSource.getBindingContext("todoModel");
            var oData = oContext.getObject();

            

            sap.ui.core.BusyIndicator.show(0);

            fetch(`http://localhost:5128/api/todos/${oData.id}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" }
            }).then(async response => {
                    // Captura o corpo da resposta APENAS UMA VEZ
                    const responseText = await response.text();

                    if (!response.ok) {
                        let errorMessage = "Erro ao atualizar status.";

                        if (responseText) errorMessage = responseText;

                        throw new Error(errorMessage);
                    }

                })
                .then(() => {
                    oData.completed = bSelected;
                    oContext.getModel().refresh(true);
                    sap.m.MessageToast.show("Status atualizado com sucesso!");
                })
                .catch(error => {
                    console.error("Erro ao atualizar status:", error);

                    oSource.setSelected(!bSelected);
                    sap.m.MessageToast.show(error.message || "Falha ao atualizar status.");
                })
                .finally(() => {
                    sap.ui.core.BusyIndicator.hide();
                });
        },

        //--- Sincronizar Banco de Dados ---
        onSyncData: function() {
            const sUrl = "http://localhost:5128/api/sync";

            sap.ui.core.BusyIndicator.show(0); 

            fetch(sUrl, {
                method: "POST",          
                headers: {
                    "Content-Type": "application/json"
                }
            })
            .then(response => {
                if (!response.ok) throw new Error("Falha na sincronização");
                return response.text(); 
            })
            .then(data => {
                sap.m.MessageToast.show("Sincronização concluída com sucesso!");
                this._fetchFilteredData();
            })
            .catch(error => {
                console.error("Erro ao sincronizar:", error);
                sap.m.MessageToast.show("Erro ao sincronizar dados");
            })
            .finally(() => {
                sap.ui.core.BusyIndicator.hide();
            });
        },
        onNavigateToDetails: function(oEvent){
            var oButton = oEvent.getSource();
            var oContext = oButton.getBindingContext("todoModel");
            var oData = oContext.getObject();

            this.getOwnerComponent().getRouter().navTo("details",{id : oData.id});
        }
        
    });
});