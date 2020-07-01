﻿import { VirtualComponent } from "../../components/virtualComponent";
import { Modal } from "../../components/modal";
import { FlexPanel } from "../../components/flexPanel";
import { Button } from "../../components/button";
import { DeleteModPackViewModel } from "./DeleteModPackViewModel";
import { Label } from "../../components/label";

export class DeleteModPackView extends VirtualComponent {
    constructor(deleteModPackViewModel: DeleteModPackViewModel) {
        super();

        let title = document.createElement('h4');
        title.textContent = 'Confirm Delete Mod Pack';

        let mainPanel = new FlexPanel(FlexPanel.direction.column);

        let label = new Label()
        label.textContent = `Delete Mod Pack ${deleteModPackViewModel.name}?`;
        label.style.fontWeight = 'bold';
        label.style.marginBottom = '1em';

        let buttonsPanel = new FlexPanel(FlexPanel.direction.row);
        buttonsPanel.classList.add('no-spacing');
        let createButton = new Button('Delete', Button.classes.danger).setCommand(deleteModPackViewModel.deleteCommand);
        let cancelButton = new Button('Cancel', Button.classes.primary).setCommand(deleteModPackViewModel.cancelCommand);
        buttonsPanel.append(createButton, cancelButton);

        mainPanel.append(label, buttonsPanel);

        let modal = new Modal(mainPanel)
            .setHeader(title);
        modal.style.minWidth = '600px';

        this._root = modal;
    }
}