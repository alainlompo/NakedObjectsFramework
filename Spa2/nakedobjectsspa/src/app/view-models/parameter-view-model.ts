﻿import { FieldViewModel } from './field-view-model';
import { ColorService } from '../color.service';
import { ErrorService } from '../error.service';
import { ChoiceViewModel } from './choice-view-model';
import { ContextService } from '../context.service';
import { ViewModelFactoryService } from '../view-model-factory.service';
import { MaskService } from '../mask.service';
import * as Helpers from './helpers-view-models';
import * as Models from '../models';
import * as Msg from '../user-messages';
import * as _ from 'lodash';
import { ConfigService } from '../config.service';

export class ParameterViewModel extends FieldViewModel {

    constructor(
        public readonly parameterRep: Models.Parameter,
        onPaneId: number,
        color: ColorService,
        error: ErrorService,
        private readonly maskService: MaskService,
        private readonly previousValue: Models.Value,
        private readonly viewModelFactory: ViewModelFactoryService,
        context: ContextService,
        configService: ConfigService
    ) {

        super(parameterRep,
            color,
            error,
            context,
            configService,
            onPaneId,
            parameterRep.isScalar(),
            parameterRep.id(),
            parameterRep.isCollectionContributed(),
            parameterRep.entryType());

        this.dflt = parameterRep.default().toString();

        const fieldEntryType = this.entryType;

        if (fieldEntryType === Models.EntryType.Choices || fieldEntryType === Models.EntryType.MultipleChoices) {
            this.setupParameterChoices();
        }

        if (fieldEntryType === Models.EntryType.AutoComplete) {
            this.setupParameterAutocomplete();
        }

        if (fieldEntryType === Models.EntryType.FreeForm && this.type === "ref") {
            this.setupParameterFreeformReference();
        }

        if (fieldEntryType === Models.EntryType.ConditionalChoices || fieldEntryType === Models.EntryType.MultipleConditionalChoices) {
            this.setupParameterConditionalChoices();
        }

        if (fieldEntryType !== Models.EntryType.FreeForm || this.isCollectionContributed) {
            this.setupParameterSelectedChoices();
        } else {
            this.setupParameterSelectedValue();
        }

        const remoteMask = parameterRep.extensions().mask();

        if (remoteMask && parameterRep.isScalar()) {
            const localFilter = this.maskService.toLocalFilter(remoteMask, parameterRep.extensions().format() !);
            this.localFilter = localFilter;
            // formatting also happens in in directive - at least for dates - value is now date in that case
            this.formattedValue = localFilter.filter(this.value!.toString());
        }

        this.description = this.getRequiredIndicator() + this.description;
    }

    private readonly dflt: string;

    private setupParameterChoices() {
        this.setupChoices(this.parameterRep.choices() !);
    }

    private setupParameterAutocomplete() {
        const parmRep = this.parameterRep;
        this.setupAutocomplete(parmRep, () => <_.Dictionary<Models.Value>>{});
    }

    private setupParameterFreeformReference() {
        const parmRep = this.parameterRep;
        this.description = this.description || Msg.dropPrompt;

        const val = this.previousValue && !this.previousValue.isNull() ? this.previousValue : parmRep.default();

        if (!val.isNull() && val.isReference()) {
            const link = val.link() !;
            this.reference = link.href();
            this.selectedChoice = new ChoiceViewModel(val, this.id, link.title());
        }
    }

    private setupParameterConditionalChoices() {
        const parmRep = this.parameterRep;
        this.setupConditionalChoices(parmRep);
    }

    private setupParameterSelectedChoices() {
        const parmRep = this.parameterRep;
        const fieldEntryType = this.entryType;
        const parmViewModel = this;
        function setCurrentChoices(vals: Models.Value) {
            const list = vals.list() !;
            const choicesToSet = _.map(list, val => new ChoiceViewModel(val, parmViewModel.id, val.link() ? val.link() !.title() : undefined));

            if (fieldEntryType === Models.EntryType.MultipleChoices) {
                parmViewModel.selectedMultiChoices = _.filter(parmViewModel.choices, c => _.some(choicesToSet, choiceToSet => c.valuesEqual(choiceToSet)));
            } else {
                parmViewModel.selectedMultiChoices = choicesToSet;
            }
        }

        function setCurrentChoice(val: Models.Value) {
            const choiceToSet = new ChoiceViewModel(val, parmViewModel.id, val.link() ? val.link() !.title() : undefined);

            if (fieldEntryType === Models.EntryType.Choices) {
                const choices = parmViewModel.choices!;
                parmViewModel.selectedChoice = _.find(choices, c => c.valuesEqual(choiceToSet)) || null;
            } else {
                if (!parmViewModel.selectedChoice || parmViewModel.selectedChoice.getValue().toValueString() !== choiceToSet.getValue().toValueString()) {
                    parmViewModel.selectedChoice = choiceToSet;
                }
            }
        }

        parmViewModel.refresh = (newValue: Models.Value) => {

            if (newValue || parmViewModel.dflt) {
                const toSet = newValue || parmRep.default();
                if (fieldEntryType === Models.EntryType.MultipleChoices || fieldEntryType === Models.EntryType.MultipleConditionalChoices ||
                    parmViewModel.isCollectionContributed) {
                    setCurrentChoices(toSet);
                } else {
                    setCurrentChoice(toSet);
                }
            }
        }

        parmViewModel.refresh(this.previousValue);

    }

    private toTriStateBoolean(valueToSet: string | boolean | number | null): boolean | null {

        // looks stupid but note type checking
        if (valueToSet === true || valueToSet === "true") {
            return true;
        }
        if (valueToSet === false || valueToSet === "false") {
            return false;
        }
        return null;
    }

    private setupParameterSelectedValue() {
        const parmRep = this.parameterRep;
        const returnType = parmRep.extensions().returnType();

        this.refresh = (newValue: Models.Value) => {

            if (returnType === "boolean") {
                const valueToSet = (newValue ? newValue.toValueString() : null) || parmRep.default().scalar();
                const bValueToSet = this.toTriStateBoolean(valueToSet);

                this.value = bValueToSet;
            } else if (Models.isDateOrDateTime(parmRep)) {
                const date = Models.toUtcDate(newValue || new Models.Value(this.dflt));
                this.value = date ? Models.toDateString(date) : "";
            } else if (Models.isTime(parmRep)) {
                this.value = Models.toTime(newValue || new Models.Value(this.dflt));
            } else {
                this.value = (newValue ? newValue.toString() : null) || this.dflt || "";
            }
        }

        this.refresh(this.previousValue);
    }

    readonly setAsRow = (i: number) => this.paneArgId = `${this.argId}${i}`;

    protected update() {
        super.update();

        switch (this.entryType) {
            case (Models.EntryType.FreeForm):
                if (this.type === "scalar") {
                    if (this.localFilter) {
                        this.formattedValue = this.value ? this.localFilter.filter(this.value) : "";
                    } else {
                        this.formattedValue = this.value ? this.value.toString() : "";
                    }
                    break;
                }
            // fall through 
            case (Models.EntryType.AutoComplete):
            case (Models.EntryType.Choices):
            case (Models.EntryType.ConditionalChoices):
                this.formattedValue = this.selectedChoice ? this.selectedChoice.toString() : "";
                break;
            case (Models.EntryType.MultipleChoices):
            case (Models.EntryType.MultipleConditionalChoices):
                const count = !this.selectedMultiChoices ? 0 : this.selectedMultiChoices.length;
                this.formattedValue = `${count} selected`;
                break;
            default:
                this.formattedValue = this.value ? this.value.toString() : "";
        }
    }
}