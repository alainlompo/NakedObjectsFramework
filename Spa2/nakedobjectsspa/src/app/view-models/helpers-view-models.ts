﻿import { FieldViewModel } from './field-view-model';
import { MenuItemViewModel } from './menu-item-view-model';
import { ActionViewModel } from './action-view-model';
import { ContextService } from '../context.service';
import { ErrorService } from '../error.service';
import { IDraggableViewModel } from './idraggable-view-model';
import { ChoiceViewModel } from './choice-view-model';
import { IMessageViewModel } from './imessage-view-model';
import * as Models from '../models';
import * as Msg from '../user-messages';
import * as _ from 'lodash';
import { FormBuilder, FormGroup, FormControl, AbstractControl } from '@angular/forms';
import { DialogViewModel } from './dialog-view-model';
import { ParameterViewModel} from './parameter-view-model';

export function createForm(dialog: DialogViewModel, formBuilder: FormBuilder) {
    const pps = dialog.parameters;
    const parms = _.zipObject(_.map(pps, p => p.id), _.map(pps, p => p)) as _.Dictionary<ParameterViewModel>;
    const controls = _.mapValues(parms, p => [p.getValueForControl(), (a: AbstractControl) => p.validator(a)]);
    const form = formBuilder.group(controls);

    form.valueChanges.subscribe((data: any) => {
        // cache parm values
        _.forEach(data, (v, k) => parms[k!].setValueFromControl(v));
        dialog.setParms();
    });

    return { form: form, dialog: dialog, parms: parms };
}



export function copy(event: KeyboardEvent, item: IDraggableViewModel, context: ContextService) {
    const cKeyCode = 67;
    if (event && (event.keyCode === cKeyCode && event.ctrlKey)) {
        context.setCopyViewModel(item);
        event.preventDefault();
    }
}

export function tooltip(onWhat: { clientValid: () => boolean }, fields: FieldViewModel[]): string {
    if (onWhat.clientValid()) {
        return "";
    }

    const missingMandatoryFields = _.filter(fields, p => !p.clientValid && !p.getMessage());

    if (missingMandatoryFields.length > 0) {
        return _.reduce(missingMandatoryFields, (s, t) => s + t.title + "; ", Msg.mandatoryFieldsPrefix);
    }

    const invalidFields = _.filter(fields, p => !p.clientValid);

    if (invalidFields.length > 0) {
        return _.reduce(invalidFields, (s, t) => s + t.title + "; ", Msg.invalidFieldsPrefix);
    }

    return "";
}

function getMenuForLevel(menupath: string, level: number) {
    let menu = "";

    if (menupath && menupath.length > 0) {
        const menus = menupath.split("_");

        if (menus.length > level) {
            menu = menus[level];
        }
    }

    return menu || "";
}

function removeDuplicateMenus(menus: MenuItemViewModel[]) {
    return _.uniqWith(menus,
        (m1: MenuItemViewModel, m2: MenuItemViewModel) => {
            if (m1.name && m2.name) {
                return m1.name === m2.name;
            }
            return false;
        });
}

export function createSubmenuItems(avms: ActionViewModel[], menu: MenuItemViewModel, level: number) {
    // if not root menu aggregate all actions with same name
    if (menu.name) {
        const actions = _.filter(avms, a => getMenuForLevel(a.menuPath, level) === menu.name && !getMenuForLevel(a.menuPath, level + 1));
        menu.actions = actions;

        //then collate submenus 

        const submenuActions = _.filter(avms, (a: ActionViewModel) => getMenuForLevel(a.menuPath, level) === menu.name && getMenuForLevel(a.menuPath, level + 1));
        let menus = _
            .chain(submenuActions)
            .map(a => new MenuItemViewModel(getMenuForLevel(a.menuPath, level + 1), null, null))
            .value();

        menus = removeDuplicateMenus(menus);

        menu.menuItems = _.map(menus, m => createSubmenuItems(submenuActions, m, level + 1));
    }
    return menu;
}

export function createMenuItems(avms: ActionViewModel[]) {

    // first create a top level menu for each action 
    // note at top level we leave 'un-menued' actions
    let menus = _
        .chain(avms)
        .map(a => new MenuItemViewModel(getMenuForLevel(a.menuPath, 0), [a], null))
        .value();

    // remove non unique submenus 
    menus = removeDuplicateMenus(menus);

    // update submenus with all actions under same submenu
    return _.map(menus, m => createSubmenuItems(avms, m, 0));
}

export function actionsTooltip(onWhat: { disableActions: () => boolean }, actionsOpen: boolean) {
    if (actionsOpen) {
        return Msg.closeActions;
    }
    return onWhat.disableActions() ? Msg.noActions : Msg.openActions;
}

export function getCollectionDetails(count: number | null) {
    if (count == null) {
        return Msg.unknownCollectionSize;
    }

    if (count === 0) {
        return Msg.emptyCollectionSize;
    }

    const postfix = count === 1 ? "Item" : "Items";

    return `${count} ${postfix}`;
}

export function drop(context: ContextService, error: ErrorService, vm: FieldViewModel, newValue: IDraggableViewModel) {
    return context.isSubTypeOf(newValue.draggableType, vm.returnType).
        then((canDrop: boolean) => {
            if (canDrop) {
                vm.setNewValue(newValue);
                return true;
            }
            return false;
        }).
        catch((reject: Models.ErrorWrapper) => error.handleError(reject));
};

export function validate(rep: Models.IHasExtensions, vm: FieldViewModel, modelValue: any, viewValue: string, mandatoryOnly: boolean) {
    const message = mandatoryOnly ? Models.validateMandatory(rep, viewValue) : Models.validate(rep, modelValue, viewValue, vm.localFilter);

    if (message !== Msg.mandatory) {
        vm.setMessage(message);
    } else {
        vm.resetMessage();
    }

    vm.clientValid = !message;
    return vm.clientValid;
};

export function setScalarValueInView(vm: { value: string | number | boolean | Date | null }, propertyRep: Models.PropertyMember, value: Models.Value) {
    if (Models.isDateOrDateTime(propertyRep)) {
        //vm.value = Models.toUtcDate(value);
        const date = Models.toUtcDate(value);
        vm.value = date ? Models.toDateString(date) : "";
    } else if (Models.isTime(propertyRep)) {
        vm.value = Models.toTime(value);
    } else {
        vm.value = value.scalar();
    }
}

export function createChoiceViewModels(id: string, searchTerm: string, choices: _.Dictionary<Models.Value>) {
    return Promise.resolve(_.map(choices, (v, k) => new ChoiceViewModel(v, id, k, searchTerm)));
}

export function handleErrorResponse(err: Models.ErrorMap, messageViewModel: IMessageViewModel, valueViewModels: FieldViewModel[]) {

    let requiredFieldsMissing = false; // only show warning message if we have nothing else 
    let fieldValidationErrors = false;
    let contributedParameterErrorMsg = "";

    _.each(err.valuesMap(), (errorValue, k) => {

        const valueViewModel = _.find(valueViewModels, vvm => vvm.id === k);

        if (valueViewModel) {
            const reason = errorValue.invalidReason;
            if (reason) {
                if (reason === "Mandatory") {
                    const r = "REQUIRED";
                    requiredFieldsMissing = true;
                    valueViewModel.description = valueViewModel.description.indexOf(r) === 0 ? valueViewModel.description : `${r} ${valueViewModel.description}`;
                } else {
                    valueViewModel.setMessage(reason);
                    fieldValidationErrors = true;
                }
            }
        } else {
            // no matching parm for message - this can happen in contributed actions 
            // make the message a dialog level warning.                               
            contributedParameterErrorMsg = errorValue.invalidReason || "";
        }
    });

    let msg = contributedParameterErrorMsg || err.invalidReason() || "";
    if (requiredFieldsMissing) msg = `${msg} Please complete REQUIRED fields. `;
    if (fieldValidationErrors) msg = `${msg} See field validation message(s). `;

    if (!msg) msg = err.warningMessage;
    messageViewModel.setMessage(msg);
}


export function incrementPendingPotentAction(context: ContextService, invokableaction: Models.IInvokableAction, paneId: number) {
    if (invokableaction.isPotent()) {
        context.incPendingPotentActionOrReload(paneId);
    }
}

export function decrementPendingPotentAction(context: ContextService, invokableaction: Models.IInvokableAction, paneId: number) {
    if (invokableaction.isPotent()) {
        context.decPendingPotentActionOrReload(paneId);
    }
}